// Rohit said he wanted credit. So credit to Rohit for the original code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs

using System.Runtime.CompilerServices;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class Logger
{
    // Thanks Khori
    internal static void Error(Exception ex, [CallerFilePath] string p = "", [CallerMemberName] string m = "", [CallerLineNumber] int l = 0) => 
        Game.LogTrivial($"[ERROR] RiskierTrafficStops: An error occured at '{p} {m} line {l}' - {ex}");

    internal static void Debug(string msg)
    {
        if (DebugMode)
        {
            Game.LogTrivial($"[DEBUG] RiskierTrafficStops: {msg}");
        }
    }

    internal static void Normal(string msg) => Game.LogTrivial($"[NORMAL] RiskierTrafficStops: {msg}");
}