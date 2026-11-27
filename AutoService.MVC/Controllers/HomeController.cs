using AutoService.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AutoService.MVC.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("Token");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (role == "Admin")
                return RedirectToAction("Admin", "Dashboard");

            if (role == "Mechanic")
                return RedirectToAction("Mechanic", "Dashboard");

            if (role == "Customer")
                return RedirectToAction("Customer", "Dashboard");

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
