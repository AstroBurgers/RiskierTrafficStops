using System;
using Rage;

// Rohit said he wanted credit. So credit to Rohit for the code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs

namespace RiskierTrafficStops
{
    internal static class Logger
    {
        internal static string defaultInfo = "RiskierTrafficStops[{0}][{1}]: {2}";

        internal static void Normal(string logLocation, string msg)
        {
            Game.LogTrivial(String.Format(defaultInfo, "NORMAL", logLocation, msg));
        }

        internal static void Warning(string logLocation, string msg)
        {
            Game.LogTrivial(String.Format(defaultInfo, "~y~WARNING~w~", logLocation, msg));
        }

        internal static void Error(string logLocation, string msg)
        {
            Game.LogTrivial(String.Format(defaultInfo, "~r~ERROR~w~", logLocation, msg));
        }
    }
}