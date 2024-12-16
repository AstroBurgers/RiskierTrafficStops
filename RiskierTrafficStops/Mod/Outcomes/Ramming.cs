namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class Ramming : Outcome, IUpdateable
{
    public Ramming(LHandle handle) : base(handle)
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
        if (Suspect.IsAvailable())
        {
            Suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
            GameFiber.Wait(3500);
            if (Suspect.IsAvailable()) {
                Suspect.Tasks.Clear();
            }
        }
        SetupPursuitWithList(true, SuspectVehicle.Occupants);
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