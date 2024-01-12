using System.Collections.Generic;
using System.Linq;
using Rage;

namespace RiskierTrafficStops.Engine.InternalSystems;

public static class GameFiberHandling
{
    internal static readonly List<GameFiber> OutcomeGameFibers = new();

    internal static void CleanupFibers()
    {
        GameFiber.StartNew(() =>
        {
            Logger.Debug("Cleaning up running GameFibers...");
            foreach (var i in OutcomeGameFibers.ToList())
            {
                if (i.IsAlive)
                {
                    i.Abort();
                }

                if (OutcomeGameFibers != null) OutcomeGameFibers.Clear();
            }
        });
    }
}