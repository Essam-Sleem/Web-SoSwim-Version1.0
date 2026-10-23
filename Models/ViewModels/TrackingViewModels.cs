using System.ComponentModel.DataAnnotations;
using Web_SoSwim_Version1._0.Models.Domain;

namespace Web_SoSwim_Version1._0.Models.ViewModels
{
    public class TrackingDashboardViewModel
    {
        public List<SwimmerSession> Swimmers { get; set; } = new();
        public string? ActiveTrackingCode { get; set; }
        public string? NotificationMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public SwimmerSession? ActiveSwimmer => 
            Swimmers.FirstOrDefault(s => s.TrackingCode.Equals(ActiveTrackingCode, StringComparison.OrdinalIgnoreCase)) 
            ?? Swimmers.FirstOrDefault();
    }

    public class AddSwimmerInputModel
    {
        [Required(ErrorMessage = "يرجى إدخال رمز التتبع الخاص بالسباح")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "رمز التتبع يجب أن يتكون من 4 إلى 20 خانة")]
        public string TrackingCode { get; set; } = string.Empty;

        public string? ExistingCodes { get; set; }
    }

    public class TriggerSosDto
    {
        [Required]
        public string TrackingCode { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }

    public class SimulateScenarioDto
    {
        [Required]
        public string TrackingCode { get; set; } = string.Empty;

        /// <summary>
        /// Scenarios: "stable", "unstable_warning" (instability 65%), "danger_sos"
        /// </summary>
        [Required]
        public string Scenario { get; set; } = "stable";
    }

    public class VitalsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SwimmerSession? Swimmer { get; set; }
        public bool CanParentTriggerSos { get; set; }
    }
}
