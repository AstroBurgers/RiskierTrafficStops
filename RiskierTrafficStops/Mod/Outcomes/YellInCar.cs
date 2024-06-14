namespace RiskierTrafficStops.Mod.Outcomes;

internal class YellInCar : Outcome, IUpdateable
{
    public YellInCar(LHandle handle) : base(handle)
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
        
        Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
        GameFiber.WaitWhile(() => Suspect.IsAvailable() && Suspect.IsAnySpeechPlaying);
        
        if (Suspect.IsAvailable())
            Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);

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