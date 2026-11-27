
using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AutoService.MVC.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public CustomersController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin"
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
        }

        private HttpClient GetClient()
        {
            var client = _factory.CreateClient("AutoServiceApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    HttpContext.Session.GetString("Token"));

            return client;
        }

        // Customer list
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (page < 1)
                page = 1;

            var client = GetClient();

            string url = $"api/customers?page={page}&pageSize=20";

            if (!string.IsNullOrWhiteSpace(search))
                url += "&search=" + Uri.EscapeDataString(search);

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return StatusCode(502, "Could not load customers");

                var model = await response.Content
                    .ReadFromJsonAsync<CustomerPageViewModel>();

                model ??= new CustomerPageViewModel();
                model.Search = search ?? "";

                return View(model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "API is unavailable");
            }
        }

        // Add customer
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            return View("Form", new CustomerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Form", model);

            var client = GetClient();
            var response = await client.PostAsJsonAsync("api/customers", model);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Could not create customer.");
            return View("Form", model);
        }

        // Edit customer
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var client = GetClient();
            var response = await client.GetAsync($"api/customers/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var customer = await response.Content
                .ReadFromJsonAsync<CustomerViewModel>();

            if (customer == null)
                return NotFound();

            return View("Form", customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Form", model);

            var client = GetClient();
            var response = await client.PutAsJsonAsync(
                $"api/customers/{model.Id}", model);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Could not update customer.");
            return View("Form", model);
        }

        // Delete customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var client = GetClient();
            var response = await client.DeleteAsync($"api/customers/{id}");

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Cannot delete this customer. They may have registered cars.";

            return RedirectToAction("Index");
        }
    }
}
