using Web_SoSwim_Version1._0.Models.Domain;

namespace Web_SoSwim_Version1._0.Services
{
    public interface ISwimmerTrackingService
    {
        Task<SwimmerSession?> GetByCodeAsync(string trackingCode);
        Task<List<SwimmerSession>> GetMultipleByCodesAsync(IEnumerable<string> trackingCodes);
        Task<bool> ExistsAsync(string trackingCode);
        Task<SwimmerSession> RegisterDemoSwimmerIfMissingAsync(string trackingCode);
        Task<(bool Success, string Message, SwimmerSession? Swimmer)> TriggerParentSosAsync(string trackingCode, string? reason);
        Task<SwimmerSession?> ApplySimulationScenarioAsync(string trackingCode, string scenario);
    }
}
