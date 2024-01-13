

// Rohit said he wanted credit. So credit to Rohit for the code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class Logger
{
    // Thanks Khori
    internal static void Error(Exception ex, string location) => Game.LogTrivial($"[ERROR] RiskierTrafficStops: {ex}");

    internal static void Debug(string msg) => Game.LogTrivial($"[DEBUG] RiskierTrafficStops: {msg}");

    internal static void Normal(string msg) => Game.LogTrivial($"[NORMAL] RiskierTrafficStops: {msg}");
}