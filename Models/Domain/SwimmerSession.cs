namespace Web_SoSwim_Version1._0.Models.Domain
{
    public class SwimmerSession
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = "ذكر";
        public string MedicalNotes { get; set; } = "لا توجد أمراض مزمنة";
        public string SwimmingLevel { get; set; } = "مبتدئ";
        public string LifeguardName { get; set; } = "كابتن زياد المصري (برج 3)";
        public string BeachLocation { get; set; } = "شاطئ النخيل - القطاع الشرقي";
        public DateTime SessionStartedAtUtc { get; set; } = DateTime.UtcNow.AddMinutes(-35);
        public SwimmerStatus Status { get; set; } = SwimmerStatus.Stable;
        public SwimmerVitals Vitals { get; set; } = new SwimmerVitals();
        public bool IsParentAlertActive { get; set; } = false;
        public DateTime? ParentAlertTriggeredAtUtc { get; set; }
        public string? ParentAlertReason { get; set; }

        public bool CanParentTriggerSos => Vitals.InstabilityRiskPercentage >= 50;
    }
}
