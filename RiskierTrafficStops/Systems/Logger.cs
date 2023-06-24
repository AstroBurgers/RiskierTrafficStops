using System;
using Rage;

// Rohit said he wanted credit. So credit to Rohit for the code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs

namespace RiskierTrafficStops.Systems
{
    internal static class Logger
    {
        internal static string defaultInfo = "[{0}] RiskierTrafficStops: {1}";

        internal static void Normal(string msg)
        {
            Game.LogTrivial(String.Format(defaultInfo, "NORMAL", msg));
        }

        internal static void Warning(string msg)
        {
            Game.LogTrivial(String.Format(defaultInfo, "WARNING", msg));
        }

        internal static void Error(Exception ex, string Location)
        {
            Game.LogTrivial(String.Format(defaultInfo, "ERROR", ex.ToString()));
            PostToDiscord.LogToDiscord(ex, Location);
        }

        internal static void Debug(string msg)
        {
            Game.LogTrivial(String.Format(defaultInfo, "DEBUG", msg));
        }
    }
}