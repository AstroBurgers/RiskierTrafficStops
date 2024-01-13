using static RiskierTrafficStops.Engine.Helpers.Extensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class Flee : Outcome
{
    private enum FleeOutcomes
    {
        Flee,
        BurnOut,
        LeaveVehicle,
    }

    private static FleeOutcomes[] _allFleeOutcomes = (FleeOutcomes[])Enum.GetValues(typeof(FleeOutcomes));
    
    public Flee(LHandle handle) : base(handle)
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
        InvokeEvent(RTSEventType.Start);
            
        Normal("Getting all vehicle occupants");
        var pedsInVehicle = SuspectVehicle.Occupants;

        FleeOutcomes chosenFleeOutcome = _allFleeOutcomes.PickRandom();

        switch (chosenFleeOutcome)
        {
            case FleeOutcomes.Flee:
                Normal("Starting pursuit");

                if (Functions.GetCurrentPullover() == null)
                {
                    CleanupEvent();
                    return;
                }

                PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                break;
            case FleeOutcomes.BurnOut:
                Normal("Making suspect do burnout");
                Suspect.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.BurnOut, 2000)
                    .WaitForCompletion(2000);
                Normal("Clearing suspect tasks");
                Suspect.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.GoForwardStraight, 750)
                    .WaitForCompletion(750);
                Normal("Starting pursuit");

                if (Functions.GetCurrentPullover() == null)
                {
                    CleanupEvent();
                    return;
                }

                PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                break;
            case FleeOutcomes.LeaveVehicle:
                foreach (var i in pedsInVehicle)
                {
                    if (i.IsAvailable())
                    {
                        i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    }
                }

                if (Functions.GetCurrentPullover() == null) return;

                PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                break;
        }
            
        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }
}