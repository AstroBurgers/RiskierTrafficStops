using System.Collections.Generic;
using System.Linq;
using Rage;

namespace RiskierTrafficStops.Engine.InternalSystems;

public class GameFiberHandling
{
    internal static List<GameFiber> OutcomeGameFibers = new();

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

                OutcomeGameFibers.Clear();
            }
        });
    }
}