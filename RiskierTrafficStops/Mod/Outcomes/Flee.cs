namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class Flee : Outcome, IProccessing
{
    private enum FleeOutcomes
    {
        Flee = 0,
        BurnOut = 1,
        LeaveVehicle = 2,
    }

    private static readonly FleeOutcomes[] AllFleeOutcomes = (FleeOutcomes[])Enum.GetValues(typeof(FleeOutcomes));
    
    public Flee(LHandle handle) : base(handle)
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
        
        var chosenFleeOutcome = AllFleeOutcomes.PickRandom();

        switch (chosenFleeOutcome)
        {
            case FleeOutcomes.Flee:
                Normal("Starting pursuit");
                
                SetupPursuitWithList(true, pedsInVehicle);
                break;
            
            case FleeOutcomes.BurnOut:
                Normal("Making suspect do burnout");
                Suspect.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.BurnOut, 2000)
                    .WaitForCompletion(2000);
                Normal("Clearing suspect tasks");
                Suspect.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.GoForwardStraight, 750)
                    .WaitForCompletion(750);
                Normal("Starting pursuit");
                
                SetupPursuitWithList(true, pedsInVehicle);
                break;
            case FleeOutcomes.LeaveVehicle:
                foreach (var i in pedsInVehicle.Where(i => i.IsAvailable()))
                {
                    i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }

                SetupPursuitWithList(true, pedsInVehicle);
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