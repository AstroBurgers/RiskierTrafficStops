namespace RiskierTrafficStops.Engine.Helpers.Extensions;

internal static class VehicleExtensions
{
    /// <summary>
    /// Checks if the vehicle exists, is valid, and has a valid model.
    /// </summary>
    /// <param name="veh"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Vehicle veh) => veh.Exists() && veh.IsValid() && veh.Model.IsValid;
}