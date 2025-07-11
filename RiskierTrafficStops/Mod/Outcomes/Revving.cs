﻿namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class Revving : Outcome, IProccessing
{
    public Revving(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));
        Normal("Adding all suspect in the vehicle to a list");
        var pedsInVehicle = new List<Ped>();
        if (SuspectVehicle.IsAvailable()) {
            pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        if (pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }
        
        RemoveIgnoredPedsAndBlockEvents(ref pedsInVehicle);
        
        Suspect.RevEngine(SuspectVehicle, [2,4], [2,4], 2);

        var chance = GenerateChance();
        switch (chance)
        {
            case <= 25:
                Normal("Suspect chose not to run after revving");
                break;
            
            default:
                if (Suspect.IsAvailable())
                {
                    SetupPursuit(true, Suspect);
                }
                break;
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }
    
    // Processing methods
    public void Start()
    {
        Normal($"Started checks for {ActiveOutcome}");
        
        while (ActiveOutcome is not null)
        {
            GameFiber.Yield();
            if (Functions.GetCurrentPullover() is null || !MainPlayer.IsAvailable())
            {
                Abort();
            }
        }
    }

    public void Abort()
    {
        CleanupOutcome(false);
    }
}