using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TestMVCWithCookieIdentity.Models;

namespace TestMVCWithCookieIdentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "miniauth-admin")]
        public IActionResult Test()
        {
            return Content(nameof(Test));
        }
        [HttpGet]

        [HttpGet]
        [Authorize]
        public IActionResult Test2()
        {
            return Content(nameof(Test2));
        }


        [HttpGet]
        public IActionResult Test3()
        {
            return Content(nameof(Test3));
        }
        [Authorize(Roles = "denied")]
        public IActionResult Test4()
        {
            return Content(nameof(Test));
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
