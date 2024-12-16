namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class Spitting : Outcome, IProccessing
{
    public Spitting(LHandle handle) : base(handle)
    {
        try
        {
            if (!MeetsRequirements(TrafficStopLHandle)) return;
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e);
            CleanupOutcome(true);
        }
    }

    internal void StartOutcome()
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
        
        GameFiber.WaitWhile(
            () => Suspect.IsAvailable() && MainPlayer.DistanceTo(Suspect) >= 3f && Suspect.IsInAnyVehicle(true),
            120000);
        if (Functions.IsPlayerPerformingPullover() && Suspect.IsAvailable() &&
            MainPlayer.DistanceTo(Suspect) <= 2.5f && Suspect.IsInAnyVehicle(true))
        {
            Game.DisplaySubtitle(SpittingText[Rndm.Next(SpittingText.Length)], 6000);
            Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
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
