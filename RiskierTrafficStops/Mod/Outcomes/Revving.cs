using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.Extensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class Revving : Outcome
{
    public Revving(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, nameof(StartOutcome));
            CleanupOutcome();
        }
    }

    internal override void StartOutcome()
    {
        APIs.InvokeEvent(RTSEventType.Start);

        RevEngine(Suspect, SuspectVehicle, new[] { 2, 4 }, new[] { 2, 4 }, 2);

        var chance = Rndm.Next(1, 101);
        switch (chance)
        {
            case <= 25:
                Normal("Suspect chose not to run after revving");
                break;
            default:
                if (Suspect.IsAvailable())
                {
                    if (Functions.GetCurrentPullover() == null)
                    {
                        CleanupEvent();
                        return;
                    }

                    PursuitLHandle = SetupPursuit(true, Suspect);
                }

                break;
        }

        GameFiberHandling.CleanupFibers();
        APIs.InvokeEvent(RTSEventType.End);
    }
}