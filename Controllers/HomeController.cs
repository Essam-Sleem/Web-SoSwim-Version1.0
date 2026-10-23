using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web_SoSwim_Version1._0.Models;
using Web_SoSwim_Version1._0.Models.ViewModels;
using Web_SoSwim_Version1._0.Services;

namespace Web_SoSwim_Version1._0.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISwimmerTrackingService _trackingService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ISwimmerTrackingService trackingService, ILogger<HomeController> logger)
        {
            _trackingService = trackingService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickTrack(string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
            {
                ViewBag.ErrorMessage = "يرجى كتابة رمز التتبع المكون من أرقام والذي استلمته من المنقذ.";
                return View("Index");
            }

            var cleanCode = trackingCode.Trim();
            var exists = await _trackingService.ExistsAsync(cleanCode);
            if (!exists)
            {
                // Auto register so any entered code can be tested seamlessly
                await _trackingService.RegisterDemoSwimmerIfMissingAsync(cleanCode);
            }

            return RedirectToAction("Index", "Tracking", new { code = cleanCode });
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
