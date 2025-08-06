// Rohit said he wanted credit. So credit to Rohit for the original code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs

using System.IO;
using System.Runtime.CompilerServices;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class Logger
{
    // Thanks Khori
    internal static void Error(Exception ex, [CallerFilePath] string p = "", [CallerMemberName] string m = "",
        [CallerLineNumber] int l = 0)
    {
        Game.LogTrivial($"[ERROR] RiskierTrafficStops: Exception at '{Path.GetFileName(p)}' -> {m} (line {l})");
        Game.LogTrivial($"[ERROR] Message: {ex.Message}");
        Game.LogTrivial($"[ERROR] Type: {ex.GetType().FullName}");
        Game.LogTrivial($"[ERROR] Stack Trace: {ex.StackTrace}");

        var inner = ex.InnerException;
        var depth = 1;
        while (inner != null)
        {
            Game.LogTrivial($"[ERROR] Inner Exception (Depth {depth}): {inner.GetType().FullName} - {inner.Message}");
            Game.LogTrivial($"[ERROR] Inner Stack Trace: {inner.StackTrace}");
            inner = inner.InnerException;
            depth++;
        }
    }


    internal static void Debug(string msg)
    {
        if (DebugMode)
        {
            Game.LogTrivial($"[DEBUG] RiskierTrafficStops: {msg}");
        }
    }

    internal static void Normal(string msg) => Game.LogTrivial($"[NORMAL] RiskierTrafficStops: {msg}");
}