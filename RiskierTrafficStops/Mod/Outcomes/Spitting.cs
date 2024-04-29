namespace RiskierTrafficStops.Mod.Outcomes;

internal class Spitting : Outcome, IUpdateable
{
    public Spitting(LHandle handle) : base(handle)
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
        Start();
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
        
        GameFiber.WaitWhile(
            () => Suspect.IsAvailable() && MainPlayer.DistanceTo(Suspect) >= 3f && Suspect.IsInAnyVehicle(true),
            120000);
        if (Functions.IsPlayerPerformingPullover() && Suspect.IsAvailable() &&
            MainPlayer.DistanceTo(Suspect) <= 2.5f && Suspect.IsInAnyVehicle(true))
        {
            Game.DisplaySubtitle(SpittingText[Rndm.Next(SpittingText.Length+1)], 6000);
            Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length+1)]);
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
            if (Functions.GetCurrentCallout() is null || !MainPlayer.IsAvailable())
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