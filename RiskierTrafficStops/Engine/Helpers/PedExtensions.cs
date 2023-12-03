using Rage;

namespace RiskierTrafficStops.Engine.Helpers;

internal static class PedExtensions
{
    /// <summary>
    /// Checks if a ped both exists and is alive
    /// </summary>
    /// <param name="ped"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Ped ped)
    {
        return ped.Exists() && ped.IsAlive;
    }
}