using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AutoService.MVC.Controllers
{
    public class RepairOrdersController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public RepairOrdersController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private string Role => HttpContext.Session.GetString("Role") ?? "";
        private bool CanView => !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"))
            && (Role == "Admin" || Role == "Mechanic");

        private HttpClient Api()
        {
            var client = _factory.CreateClient("AutoServiceApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
            return client;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            if (!CanView) return RedirectToAction("Index", "Home");
            if (page < 1) page = 1;

            string url = $"api/repair-orders?page={page}&pageSize=20";
            if (!string.IsNullOrWhiteSpace(search))
                url += "&search=" + Uri.EscapeDataString(search);

            try
            {
                var model = await Api().GetFromJsonAsync<RepairOrderPageViewModel>(url);
                model ??= new RepairOrderPageViewModel();
                model.Search = search ?? "";
                ViewBag.Role = Role;
                return View(model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot connect to the API.");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (Role != "Admin" || !CanView)
                return RedirectToAction("Index", "Home");

            return View("Form", new RepairOrderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairOrderViewModel model)
        {
            if (Role != "Admin" || !CanView)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Form", model);

            try
            {
                var response = await Api().PostAsJsonAsync("api/repair-orders",
                    new { model.CarId, model.Description });

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "Could not create order. Check the car ID.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Cannot connect to the API.");
            }

            return View("Form", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (Role != "Admin" || !CanView)
                return RedirectToAction("Index", "Home");

            try
            {
                var response = await Api().GetAsync($"api/repair-orders/{id}");
                if (!response.IsSuccessStatusCode) return NotFound();

                var model = await response.Content.ReadFromJsonAsync<RepairOrderViewModel>();
                return model == null ? NotFound() : View("Form", model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot connect to the API.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RepairOrderViewModel model)
        {
            if (Role != "Admin" || !CanView)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Form", model);

            try
            {
                var response = await Api().PutAsJsonAsync($"api/repair-orders/{model.Id}",
                    new { model.CarId, model.Description, model.Status });

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "Could not update repair order.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Cannot connect to the API.");
            }

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            if (!CanView)
                return RedirectToAction("Index", "Home");

            if (status != "Pending" && status != "InProgress" && status != "Completed")
                return BadRequest();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, $"api/repair-orders/{id}/status")
                {
                    Content = JsonContent.Create(new { Status = status })
                };
                var response = await Api().SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Could not change repair status.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to the API.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int mechanicUserId)
        {
            if (Role != "Admin" || !CanView)
                return RedirectToAction("Index", "Home");

            if (mechanicUserId <= 0) return BadRequest();

            try
            {
                var response = await Api().PutAsJsonAsync(
                    $"api/repair-orders/{id}/assign", new { MechanicUserId = mechanicUserId });
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Could not assign mechanic. Check the mechanic user ID.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to the API.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (Role != "Admin" || !CanView)
                return RedirectToAction("Index", "Home");

            try
            {
                var response = await Api().DeleteAsync($"api/repair-orders/{id}");
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Could not delete order. It may have parts or work logs.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to the API.";
            }

            return RedirectToAction("Index");
        }
    }
}
