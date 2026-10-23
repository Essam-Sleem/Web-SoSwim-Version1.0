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
                // Fallback to primary demo
                codes.Add("12344321");
            }

            TempData["NotificationMessage"] = $"تمت إزالة السباح ({codeToRemove}) من قائمة التتبع الحالية.";
            return RedirectToAction(nameof(Index), new { code = codes.FirstOrDefault(), codes = string.Join(",", codes) });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerParentSos([FromBody] TriggerSosDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "بيانات الطلب غير صالحة." });
            }

            var result = await _trackingService.TriggerParentSosAsync(request.TrackingCode, request.Reason);
            if (!result.Success)
            {
                return Ok(new
                {
                    success = false,
                    message = result.Message,
                    swimmer = result.Swimmer,
                    canParentTriggerSos = result.Swimmer?.CanParentTriggerSos ?? false
                });
            }

            _logger.LogWarning("Parent triggered SOS for swimmer {Code} with reason {Reason}", request.TrackingCode, request.Reason);

            return Ok(new
            {
                success = true,
                message = result.Message,
                swimmer = result.Swimmer,
                canParentTriggerSos = true
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetVitals(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { success = false, message = "رمز التتبع مطلوب." });
            }

            var swimmer = await _trackingService.GetByCodeAsync(code);
            if (swimmer == null)
            {
                return NotFound(new { success = false, message = "جلسة السباح غير متوفرة." });
            }

            return Ok(new
            {
                success = true,
                swimmer,
                canParentTriggerSos = swimmer.CanParentTriggerSos
            });
        }

        [HttpPost]
        public async Task<IActionResult> SimulateScenario([FromBody] SimulateScenarioDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "بيانات المحاكاة غير صالحة." });
            }

            var updated = await _trackingService.ApplySimulationScenarioAsync(request.TrackingCode, request.Scenario);
            if (updated == null)
            {
                return NotFound(new { success = false, message = "لم يتم العثور على السباح المطلوب للمحاكاة." });
            }

            return Ok(new
            {
                success = true,
                message = $"تم ضبط سيناريو المحاكاة إلى '{request.Scenario}' بنجاح.",
                swimmer = updated,
                canParentTriggerSos = updated.CanParentTriggerSos
            });
        }
    }
}
