using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AutoService.MVC.Controllers
{
    public class RepairWorkController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public RepairWorkController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private bool IsMechanic()
        {
            return HttpContext.Session.GetString("Role") == "Mechanic"
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
        }

        private bool CanAccess()
        {
            return IsMechanic() || (
                HttpContext.Session.GetString("Role") == "Admin"
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Token")));
        }

        private HttpClient Api()
        {
            var client = _factory.CreateClient("AutoServiceApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer", HttpContext.Session.GetString("Token"));
            return client;
        }

        [HttpGet]
        public IActionResult Index(int id)
        {
            if (!CanAccess()) return RedirectToAction("Index", "Home");
            return View(new RepairWorkViewModel { RepairOrderId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHours(RepairWorkViewModel model)
        {
            if (!IsMechanic()) return RedirectToAction("Index", "Home");

            if (string.IsNullOrWhiteSpace(model.Description) || model.HoursWorked <= 0)
            {
                TempData["Error"] = "Enter a description and positive hours.";
                return RedirectToAction("Index", new { id = model.RepairOrderId });
            }

            string description = Uri.EscapeDataString(model.Description);
            string hours = model.HoursWorked.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string url = $"api/repair-orders/{model.RepairOrderId}/worklogs" +
                         $"?description={description}&hoursWorked={hours}";

            try
            {
                var response = await Api().PostAsync(url, null);
                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode ? "Work hours saved." : "Could not save work hours.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to API.";
            }

            return RedirectToAction("Index", new { id = model.RepairOrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(RepairWorkViewModel model)
        {
            if (!CanAccess()) return RedirectToAction("Index", "Home");

            if (model.PartId <= 0 || model.Quantity <= 0)
            {
                TempData["Error"] = "Enter a part ID and positive quantity.";
                return RedirectToAction("Index", new { id = model.RepairOrderId });
            }

            string url = $"api/repair-orders/{model.RepairOrderId}/parts" +
                         $"?partId={model.PartId}&quantity={model.Quantity}";

            try
            {
                var response = await Api().PostAsync(url, null);
                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode ? "Spare part saved." :
                    "Could not add spare part. Check stock and part ID.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to API.";
            }

            return RedirectToAction("Index", new { id = model.RepairOrderId });
        }
    }
}
