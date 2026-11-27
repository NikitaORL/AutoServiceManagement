
using Microsoft.AspNetCore.Mvc;

namespace AutoService.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private IActionResult OpenDashboard(string role)
        {
            var token = HttpContext.Session.GetString("Token");
            var userRole = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (userRole != role)
                return RedirectToAction("Index", "Home");

            ViewBag.Role = role;
            return View("Index");
        }

        public IActionResult Admin()
        {
            return OpenDashboard("Admin");
        }

        public IActionResult Mechanic()
        {
            return OpenDashboard("Mechanic");
        }

        public IActionResult Customer()
        {
            return OpenDashboard("Customer");
        }
    }
}
