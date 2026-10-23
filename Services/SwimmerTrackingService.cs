using System.Collections.Concurrent;
using Web_SoSwim_Version1._0.Models.Domain;

namespace Web_SoSwim_Version1._0.Services
{
    public class SwimmerTrackingService : ISwimmerTrackingService
    {
        private static readonly ConcurrentDictionary<string, SwimmerSession> _sessions = new(StringComparer.OrdinalIgnoreCase);

        static SwimmerTrackingService()
        {
            SeedInitialSessions();
        }

        public Task<SwimmerSession?> GetByCodeAsync(string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
            {
                return Task.FromResult<SwimmerSession?>(null);
            }

            var cleanCode = trackingCode.Trim();
            _sessions.TryGetValue(cleanCode, out var session);
            return Task.FromResult(session);
        }

        public Task<List<SwimmerSession>> GetMultipleByCodesAsync(IEnumerable<string> trackingCodes)
        {
            var results = new List<SwimmerSession>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var code in trackingCodes)
            {
                if (string.IsNullOrWhiteSpace(code)) continue;
                var clean = code.Trim();
                if (seen.Add(clean) && _sessions.TryGetValue(clean, out var session))
                {
                    results.Add(session);
                }
            }

            return Task.FromResult(results);
        }

        public Task<bool> ExistsAsync(string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode)) return Task.FromResult(false);
            return Task.FromResult(_sessions.ContainsKey(trackingCode.Trim()));
        }

        public Task<SwimmerSession> RegisterDemoSwimmerIfMissingAsync(string trackingCode)
        {
            var cleanCode = trackingCode.Trim();
            var session = _sessions.GetOrAdd(cleanCode, code => new SwimmerSession
            {
                TrackingCode = code,
                FullName = $"سباح تجريبي ({code})",
                Age = 13,
                Gender = "ذكر",
                MedicalNotes = "فحص الشاطئ سليم، بدون عوارض",
                SwimmingLevel = "متوسط",
                BeachLocation = "شاطئ النخيل - القطاع الأوسط",
                LifeguardName = "كابتن أحمد سامي (برج 2)",
                SessionStartedAtUtc = DateTime.UtcNow.AddMinutes(-20),
                Status = SwimmerStatus.Stable,
                Vitals = new SwimmerVitals
                {
                    HeartRateBpm = 82,
                    InstabilityRiskPercentage = 15,
                    MovementPattern = "منتظم وثابت",
                    WaterTemperatureCelsius = 25.0,
                    DepthMeters = 1.1,
                    DistanceFromShoreMeters = 22.0,
                    BatteryPercentage = 95,
                    LastSignalTimeUtc = DateTime.UtcNow
                }
            });

            return Task.FromResult(session);
        }

        public Task<(bool Success, string Message, SwimmerSession? Swimmer)> TriggerParentSosAsync(string trackingCode, string? reason)
        {
            if (string.IsNullOrWhiteSpace(trackingCode) || !_sessions.TryGetValue(trackingCode.Trim(), out var swimmer))
            {
                return Task.FromResult<(bool, string, SwimmerSession?)>((false, "جلسة السباح غير موجودة أو انتهت صلاحيتها.", null));
            }

            // قاعدة العمل الأساسية: لا يمكن لولي الأمر إرسال استغاثة إلا إذا كانت نسبة عدم الاستقرار 50% أو أكثر
            if (swimmer.Vitals.InstabilityRiskPercentage < 50)
            {
                return Task.FromResult<(bool, string, SwimmerSession?)>((
                    false,
                    "لا يمكن إرسال نداء استغاثة الآن: المؤشرات الحيوية وحركة السباح مستقرة تماماً وأقل من 50% نسبة خطورة. النظام يمنع البلاغات غير المبررة لتفادي إرباك طواقم الإنقاذ.",
                    swimmer
                ));
            }

            // تفعيل الإنذار
            swimmer.IsParentAlertActive = true;
            swimmer.ParentAlertTriggeredAtUtc = DateTime.UtcNow;
            swimmer.ParentAlertReason = string.IsNullOrWhiteSpace(reason) ? "نداء استغاثة مؤكد من ولي الأمر عبر المنصة" : reason;
            swimmer.Status = SwimmerStatus.InDanger;

            return Task.FromResult<(bool, string, SwimmerSession?)>((
                true,
                "تم إرسال إشارة الاستغاثة العاجلة فوراً إلى برج الإنقاذ والدورية البحرية القريبة. فرق الإنقاذ في طريقها لموقع السباح الآن.",
                swimmer
            ));
        }

        public Task<SwimmerSession?> ApplySimulationScenarioAsync(string trackingCode, string scenario)
        {
            if (string.IsNullOrWhiteSpace(trackingCode) || !_sessions.TryGetValue(trackingCode.Trim(), out var swimmer))
            {
                return Task.FromResult<SwimmerSession?>(null);
            }

            var now = DateTime.UtcNow;
            switch (scenario.ToLowerInvariant())
            {
                case "stable":
                    swimmer.Status = SwimmerStatus.Stable;
                    swimmer.IsParentAlertActive = false;
                    swimmer.Vitals.HeartRateBpm = 76;
                    swimmer.Vitals.InstabilityRiskPercentage = 18;
                    swimmer.Vitals.MovementPattern = "طبيعي ومستقر";
                    swimmer.Vitals.DistanceFromShoreMeters = 24.0;
                    swimmer.Vitals.DepthMeters = 1.1;
                    swimmer.Vitals.IsEmergencyButtonPressed = false;
                    swimmer.Vitals.LastSignalTimeUtc = now;
                    break;

                case "unstable":
                case "warning":
                case "unstable_warning":
                    swimmer.Status = SwimmerStatus.Unstable;
                    swimmer.IsParentAlertActive = false;
                    swimmer.Vitals.HeartRateBpm = 138;
                    // تم رفع نسبة الخطورة لأعلى من 50% (مثلاً 68%) لتفعيل زر النجدة للأهل للاختبار
                    swimmer.Vitals.InstabilityRiskPercentage = 68;
                    swimmer.Vitals.MovementPattern = "حركات متخبطة وغير منتظمة";
                    swimmer.Vitals.DistanceFromShoreMeters = 58.0;
                    swimmer.Vitals.DepthMeters = 2.1;
                    swimmer.Vitals.IsEmergencyButtonPressed = false;
                    swimmer.Vitals.LastSignalTimeUtc = now;
                    break;

                case "danger":
                case "danger_sos":
                    swimmer.Status = SwimmerStatus.InDanger;
                    swimmer.IsParentAlertActive = true;
                    swimmer.Vitals.HeartRateBpm = 168;
                    swimmer.Vitals.InstabilityRiskPercentage = 94;
                    swimmer.Vitals.MovementPattern = "توقف مفاجئ عن الحركة واضطراب شديد";
                    swimmer.Vitals.DistanceFromShoreMeters = 85.0;
                    swimmer.Vitals.DepthMeters = 2.8;
                    swimmer.Vitals.IsEmergencyButtonPressed = true;
                    swimmer.Vitals.LastSignalTimeUtc = now;
                    break;
            }

            return Task.FromResult<SwimmerSession?>(swimmer);
        }

        private static void SeedInitialSessions()
        {
            // 1. يوسف - حالة مستقرة (الزر مقفل لأنه أقل من 50%)
            _sessions["12344321"] = new SwimmerSession
            {
                TrackingCode = "12344321",
                FullName = "يوسف محمود سالم",
                Age = 12,
                Gender = "ذكر",
                MedicalNotes = "لا توجد أمراض سابقة، مستوى لياقة جيد",
                SwimmingLevel = "مبتدئ خاضع للإشراف",
                BeachLocation = "شاطئ المنتزه - المنطقة (ب)",
                LifeguardName = "كابتن سيف الدين الشريف (برج 1)",
                SessionStartedAtUtc = DateTime.UtcNow.AddMinutes(-40),
                Status = SwimmerStatus.Stable,
                Vitals = new SwimmerVitals
                {
                    HeartRateBpm = 78,
                    InstabilityRiskPercentage = 16,
                    MovementPattern = "سباحة منتظمة معتدلة",
                    WaterTemperatureCelsius = 24.8,
                    DepthMeters = 1.2,
                    DistanceFromShoreMeters = 26.5,
                    BatteryPercentage = 91,
                    LastSignalTimeUtc = DateTime.UtcNow
                }
            };

            // 2. مريم - حالة شبه مستقرة / خطر محتمل 64% (الزر مفعل للأهل)
            _sessions["87654321"] = new SwimmerSession
            {
                TrackingCode = "87654321",
                FullName = "مريم إبراهيم حسن",
                Age = 9,
                Gender = "أنثى",
                MedicalNotes = "حساسية ربو خفيفة",
                SwimmingLevel = "مبتدئة",
                BeachLocation = "شاطئ المنتزه - المنطقة (أ)",
                LifeguardName = "كابتن يحيى علام (برج 4)",
                SessionStartedAtUtc = DateTime.UtcNow.AddMinutes(-25),
                Status = SwimmerStatus.Unstable,
                Vitals = new SwimmerVitals
                {
                    HeartRateBpm = 136,
                    InstabilityRiskPercentage = 64, // يتجاوز ال 50%
                    MovementPattern = "تخبط متكرر وتسارع في التجديف بالذراعين",
                    WaterTemperatureCelsius = 24.2,
                    DepthMeters = 1.8,
                    DistanceFromShoreMeters = 54.0,
                    BatteryPercentage = 84,
                    LastSignalTimeUtc = DateTime.UtcNow
                }
            };

            // 3. كريم - حالة خطر شديد / استغاثة نشطة 95%
            _sessions["11223344"] = new SwimmerSession
            {
                TrackingCode = "11223344",
                FullName = "كريم خالد العمري",
                Age = 15,
                Gender = "ذكر",
                MedicalNotes = "سليم",
                SwimmingLevel = "متوسط",
                BeachLocation = "شاطئ المنتزه - القطاع العميق",
                LifeguardName = "كابتن زياد المصري (برج الإنقاذ السريع)",
                SessionStartedAtUtc = DateTime.UtcNow.AddMinutes(-55),
                Status = SwimmerStatus.InDanger,
                IsParentAlertActive = false,
                Vitals = new SwimmerVitals
                {
                    HeartRateBpm = 172,
                    InstabilityRiskPercentage = 95,
                    MovementPattern = "انغمار تحت سطح الماء وحركات غير طبيعية",
                    WaterTemperatureCelsius = 23.5,
                    DepthMeters = 3.2,
                    DistanceFromShoreMeters = 92.0,
                    BatteryPercentage = 76,
                    IsEmergencyButtonPressed = true,
                    LastSignalTimeUtc = DateTime.UtcNow
                }
            };
        }
    }
}
