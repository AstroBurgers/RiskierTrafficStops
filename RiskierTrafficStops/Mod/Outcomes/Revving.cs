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
            CleanupOutcome(true);
        }
    }

    internal override void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);

        Suspect.RevEngine(SuspectVehicle, new[] { 2, 4 }, new[] { 2, 4 }, 2);

        long chance = GenerateChance();
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
                        CleanupOutcome(false);
                        return;
                    }

                    PursuitLHandle = SetupPursuit(true, Suspect);
                }
                break;
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }
}