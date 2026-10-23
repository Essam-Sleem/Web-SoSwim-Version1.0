using Microsoft.AspNetCore.Mvc;
using Web_SoSwim_Version1._0.Models.ViewModels;
using Web_SoSwim_Version1._0.Services;

namespace Web_SoSwim_Version1._0.Controllers
{
    public class TrackingController : Controller
    {
        private readonly ISwimmerTrackingService _trackingService;
        private readonly ILogger<TrackingController> _logger;

        public TrackingController(ISwimmerTrackingService trackingService, ILogger<TrackingController> logger)
        {
            _trackingService = trackingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? code, string? codes)
        {
            var codeList = new List<string>();

            if (!string.IsNullOrWhiteSpace(codes))
            {
                codeList.AddRange(codes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }

            if (!string.IsNullOrWhiteSpace(code) && !codeList.Contains(code.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                codeList.Insert(0, code.Trim());
            }

            // Default fallback demo code if none entered
            if (codeList.Count == 0)
            {
                codeList.Add("12344321");
            }

            var swimmers = await _trackingService.GetMultipleByCodesAsync(codeList);

            // If any requested code was missing, create a demo swimmer for it to provide a seamless testing experience
            foreach (var c in codeList)
            {
                if (!swimmers.Any(s => s.TrackingCode.Equals(c, StringComparison.OrdinalIgnoreCase)))
                {
                    var created = await _trackingService.RegisterDemoSwimmerIfMissingAsync(c);
                    swimmers.Add(created);
                }
            }

            var activeCode = !string.IsNullOrWhiteSpace(code) && swimmers.Any(s => s.TrackingCode.Equals(code, StringComparison.OrdinalIgnoreCase))
                ? code.Trim()
                : swimmers.FirstOrDefault()?.TrackingCode;

            var viewModel = new TrackingDashboardViewModel
            {
                Swimmers = swimmers,
                ActiveTrackingCode = activeCode,
                NotificationMessage = TempData["NotificationMessage"] as string,
                ErrorMessage = TempData["ErrorMessage"] as string
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSwimmer(AddSwimmerInputModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.TrackingCode))
            {
                TempData["ErrorMessage"] = "يرجى إدخال رمز تتبع صحيح بين 4 و 20 خانة.";
                return RedirectToAction(nameof(Index), new { codes = model.ExistingCodes });
            }

            var cleanCode = model.TrackingCode.Trim();
            var existing = (model.ExistingCodes ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (existing.Contains(cleanCode, StringComparer.OrdinalIgnoreCase))
            {
                TempData["NotificationMessage"] = $"السباح صاحب الرمز {cleanCode} موجود بالفعل في قائمة التتبع الحالية.";
                return RedirectToAction(nameof(Index), new { code = cleanCode, codes = string.Join(",", existing) });
            }

            // Register demo if missing
            await _trackingService.RegisterDemoSwimmerIfMissingAsync(cleanCode);
            existing.Add(cleanCode);

            TempData["NotificationMessage"] = $"تمت إضافة السباح بالرمز {cleanCode} بنجاح إلى شاشة المراقبة.";
            return RedirectToAction(nameof(Index), new { code = cleanCode, codes = string.Join(",", existing) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSwimmer(string codeToRemove, string? currentCodes)
        {
            var codes = (currentCodes ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(c => !c.Equals(codeToRemove?.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (codes.Count == 0)
            {
                codes.Add("12344321");
            }

            TempData["NotificationMessage"] = $"تمت إزالة السباح ({codeToRemove}) من قائمة التتبع الحالية.";
            return RedirectToAction(nameof(Index), new { code = codes.FirstOrDefault(), codes = string.Join(",", codes) });
        }

        /// <summary>
        /// Pure C# Server-Side SOS Trigger Form Action
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TriggerSos(string trackingCode, string? reason, string? currentCodes)
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
            {
                TempData["ErrorMessage"] = "رمز السباح غير محدد.";
                return RedirectToAction(nameof(Index), new { codes = currentCodes });
            }

            var result = await _trackingService.TriggerParentSosAsync(trackingCode, reason);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["NotificationMessage"] = result.Message;
                _logger.LogWarning("Parent triggered SOS in C# for swimmer {Code} with reason {Reason}", trackingCode, reason);
            }

            return RedirectToAction(nameof(Index), new { code = trackingCode, codes = currentCodes });
        }

        /// <summary>
        /// Pure C# Server-Side Scenario Switcher Form Action
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchScenario(string trackingCode, string scenario, string? currentCodes)
        {
            if (!string.IsNullOrWhiteSpace(trackingCode) && !string.IsNullOrWhiteSpace(scenario))
            {
                await _trackingService.ApplySimulationScenarioAsync(trackingCode, scenario);
            }

            return RedirectToAction(nameof(Index), new { code = trackingCode, codes = currentCodes });
        }
    }
}
