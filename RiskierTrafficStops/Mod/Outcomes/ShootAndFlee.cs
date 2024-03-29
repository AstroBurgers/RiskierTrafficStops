﻿namespace RiskierTrafficStops.Mod.Outcomes;

internal class ShootAndFlee : Outcome
{
    public ShootAndFlee(LHandle handle) : base(handle)
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
            CleanupOutcome(true);
        }
    }

    internal override void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);

        Normal("Adding all suspect in the vehicle to a list");
        var _pedsInVehicle = new List<Ped>();
        if (SuspectVehicle.IsAvailable()) {
            _pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        if (_pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }
        
        RemoveIgnoredPedsAndBlockEvents(ref _pedsInVehicle);
        
        var chance = GenerateChance();
        switch (chance)
        {
            case <= 60:
                Normal("Starting all suspects outcome");
                AllSuspects(_pedsInVehicle);
                break;
            default:
                Normal("Starting driver only outcome");
                DriverOnly();
                break;
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }

    private static void AllSuspects(List<Ped> peds)
    {
        foreach (var i in peds.Where(i => i.IsAvailable()))
        {
            i.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search",
                8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
            GameFiber.Wait(Rndm.Next(1, 4) * 750);
            i.GivePistol();

            Normal($"Making Suspect #{i} shoot at Player");
            NativeFunction.Natives.x10AB107B887214D8(i, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
        }

        GameFiber.Wait(5000);

        if ((Functions.GetCurrentPullover() == null) || !MainPlayer.IsAvailable())
        {
            CleanupOutcome(false);
            return;
        }

        PursuitLHandle = SetupPursuitWithList(true, peds);
    }

    private static void DriverOnly()
    {
        if (!Suspect.IsAvailable()) return;
        Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search",
            8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
        GameFiber.Wait(Rndm.Next(1, 4) * 750);
        Normal("Setting up Suspect Weapon");
        Suspect.GivePistol();

        Normal("Giving Suspect Tasks");
        NativeFunction.Natives.x10AB107B887214D8(Suspect, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
        GameFiber.Wait(5000);

        if (Functions.GetCurrentPullover() == null || !MainPlayer.IsAvailable())
        {
            CleanupOutcome(false);
            return;
        }

        PursuitLHandle = SetupPursuit(true, Suspect);
    }
}