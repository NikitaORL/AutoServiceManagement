
using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace AutoService.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("AutoServiceApi");

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/auth/login", model);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Login failed. Check your credentials.");
                    return View(model);
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResult>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    ModelState.AddModelError("", "Invalid server response.");
                    return View(model);
                }

                HttpContext.Session.SetString("Token", result.Token);
                HttpContext.Session.SetString("Role", result.Role);

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Cannot connect to the API.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    public class LoginResult
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
