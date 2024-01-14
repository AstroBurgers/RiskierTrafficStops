namespace RiskierTrafficStops.Engine.Helpers.Extensions;

public static class VehicleExtensions
{
    /// <summary>
    /// Checks if a ped both exists and is alive
    /// </summary>
    /// <param name="veh"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Vehicle veh) => veh.Exists() && veh.IsValid() && veh.Model.IsValid;
}