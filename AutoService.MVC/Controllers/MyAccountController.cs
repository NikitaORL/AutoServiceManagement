using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AutoService.MVC.Controllers
{
    public class MyAccountController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public MyAccountController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private bool IsCustomer()
        {
            return HttpContext.Session.GetString("Role") == "Customer"
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
        }

        private HttpClient Api()
        {
            var client = _factory.CreateClient("AutoServiceApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
            return client;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsCustomer())
                return RedirectToAction("Index", "Home");

            var client = Api();

            try
            {
                var model = new MyAccountViewModel
                {
                    Cars = await client.GetFromJsonAsync<List<MyCarViewModel>>("api/me/cars") ?? new(),
                    Orders = await client.GetFromJsonAsync<List<MyOrderViewModel>>("api/me/repair-orders") ?? new(),
                    Notifications = await client.GetFromJsonAsync<List<MyNotificationViewModel>>("api/me/notifications") ?? new()
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot load your account. Check the API connection.");
            }
        }

        public async Task<IActionResult> Invoice(int id)
        {
            if (!IsCustomer())
                return RedirectToAction("Index", "Home");

            try
            {
                var response = await Api().GetAsync($"api/me/repair-orders/{id}/invoice");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                    return StatusCode(502, "Could not load invoice.");

                var invoice = await response.Content.ReadFromJsonAsync<MyInvoiceViewModel>();
                return invoice == null ? NotFound() : View(invoice);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Cannot connect to the API.");
            }
        }
    }
}
