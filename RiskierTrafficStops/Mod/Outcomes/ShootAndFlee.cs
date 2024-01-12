using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.Extensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class ShootAndFlee : Outcome
{
    internal ShootAndFlee(LHandle handle) : base(handle)
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

        var outcome = Rndm.Next(1, 101);
        switch (outcome)
        {
            case > 50:
                Normal("Starting all suspects outcome");
                AllSuspects(SuspectVehicle.Occupants);
                break;
            case <= 50:
                Normal("Starting driver only outcome");
                DriverOnly();
                break;
        }

        GameFiberHandling.CleanupFibers();
        APIs.InvokeEvent(RTSEventType.End);
    }

    private static void AllSuspects(Ped[] peds)
    {
        foreach (var i in peds)
        {
            if (!i.IsAvailable()) continue;
            i.GivePistol();

            Normal($"Making Suspect #{i} shoot at Player");
            NativeFunction.Natives.x10AB107B887214D8(i, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
        }

        GameFiber.Wait(5000);

        if ((Functions.GetCurrentPullover() == null) || !MainPlayer.IsAvailable())
        {
            CleanupEvent();
            return;
        }

        PursuitLHandle = SetupPursuitWithList(true, peds);
    }

    private static void DriverOnly()
    {
        if (!Suspect.IsAvailable()) return;


        Normal("Setting up Suspect Weapon");
        Suspect.GivePistol();

        Normal("Giving Suspect Tasks");
        NativeFunction.Natives.x10AB107B887214D8(Suspect, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
        GameFiber.Wait(5000);

        if (Functions.GetCurrentPullover() == null || !MainPlayer.IsAvailable())
        {
            CleanupEvent();
            return;
        }

        PursuitLHandle = SetupPursuit(true, Suspect);
    }
}