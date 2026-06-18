namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class Flee : Outcome, IProccessing
{
    private enum FleeOutcomes
    {
        Flee        = 0,
        BurnOut     = 1,
        LeaveVehicle = 2,
        Revving     = 3,
    }

    private static readonly FleeOutcomes[] AllFleeOutcomes = (FleeOutcomes[])Enum.GetValues(typeof(FleeOutcomes));

    public Flee(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));

        Normal("Adding all suspects in the vehicle to a list");
        List<Ped> pedsInVehicle = new List<Ped>();
        if (SuspectVehicle.IsAvailable())
            pedsInVehicle = SuspectVehicle.Occupants.ToList();

        if (pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }

        RemoveIgnoredPedsAndBlockEvents(ref pedsInVehicle);

        FleeOutcomes chosenFleeOutcome = AllFleeOutcomes.PickRandom();
        Normal($"Chosen flee sub-outcome: {chosenFleeOutcome}");

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
                foreach (Ped ped in pedsInVehicle.Where(p => p.IsAvailable()))
                    ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);

                SetupPursuitWithList(true, pedsInVehicle);
                break;

            case FleeOutcomes.Revving:
                HandleRevving(pedsInVehicle);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(chosenFleeOutcome), chosenFleeOutcome, "Unhandled flee outcome");
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }

    /// <summary>
    /// Suspect revs their engine, then either backs down or flees based on chance.
    /// 25% chance they lose their nerve and stay; 75% chance they flee.
    /// </summary>
    private static void HandleRevving(List<Ped> pedsInVehicle)
    {
        Normal("Suspect is revving engine");
        Suspect.RevEngine(SuspectVehicle, [2, 4], [2, 4], 2);

        long chance = GenerateChance();
        switch (chance)
        {
            case <= 25:
                Normal("Suspect lost their nerve after revving — not fleeing");
                break;

            default:
                Normal("Suspect is fleeing after revving");
                if (Suspect.IsAvailable())
                    SetupPursuitWithList(true, pedsInVehicle);
                break;
        }
    }

    // Processing methods
    public void Start()
    {
        Normal($"Started checks for {ActiveOutcome}");

        while (ActiveOutcome is not null)
        {
            GameFiber.Yield();
            if (Functions.GetCurrentPullover() is null || !MainPlayer.IsAvailable())
                Abort();
        }
    }

    public void Abort()
    {
        CleanupOutcome(false);
    }
}