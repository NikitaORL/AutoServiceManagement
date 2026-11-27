using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AutoService.MVC.Controllers
{
    public class CarsController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public CarsController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin"
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
        }

        private HttpClient Api()
        {
            var client = _factory.CreateClient("AutoServiceApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    HttpContext.Session.GetString("Token"));
            return client;
        }

        private async Task LoadCustomers()
        {
            var response = await Api().GetFromJsonAsync<CustomerPageViewModel>(
                "api/customers?pageSize=100");

            var customers = response?.Items ?? new List<CustomerViewModel>();
            ViewBag.Customers = new SelectList(customers, "Id", "Email");
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (page < 1) page = 1;

            var url = $"api/cars?page={page}&pageSize=20";

            if (!string.IsNullOrWhiteSpace(search))
                url += "&search=" + Uri.EscapeDataString(search);

            try
            {
                var model = await Api().GetFromJsonAsync<CarPageViewModel>(url);
                model ??= new CarPageViewModel();
                model.Search = search ?? "";
                return View(model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot connect to the API.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            try
            {
                await LoadCustomers();
                return View("Form", new CarViewModel { Year = DateTime.Now.Year });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot load customers.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await Api().PostAsJsonAsync("api/cars", model);
                    if (response.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    ModelState.AddModelError("", "Could not save the car. Check its owner and details.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError("", "Cannot connect to the API.");
                }
            }

            try { await LoadCustomers(); }
            catch (HttpRequestException) { ViewBag.Customers = new SelectList(new List<CustomerViewModel>()); }
            return View("Form", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            try
            {
                var response = await Api().GetAsync($"api/cars/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return NotFound();
                if (!response.IsSuccessStatusCode)
                    return StatusCode(502, "Could not load the car.");

                var model = await response.Content.ReadFromJsonAsync<CarViewModel>();
                if (model == null) return NotFound();

                await LoadCustomers();
                return View("Form", model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot connect to the API.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CarViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await Api().PutAsJsonAsync($"api/cars/{model.Id}", model);
                    if (response.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    ModelState.AddModelError("", "Could not update the car.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError("", "Cannot connect to the API.");
                }
            }

            try { await LoadCustomers(); }
            catch (HttpRequestException) { ViewBag.Customers = new SelectList(new List<CustomerViewModel>()); }
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            try
            {
                var response = await Api().DeleteAsync($"api/cars/{id}");
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Could not delete the car. It may have repair orders.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to the API.";
            }

            return RedirectToAction("Index");
        }
    }
}
