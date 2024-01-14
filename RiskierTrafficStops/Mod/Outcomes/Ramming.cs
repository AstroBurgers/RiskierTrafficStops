using RiskierTrafficStops.Engine.Helpers.Extensions;
using static RiskierTrafficStops.Engine.Helpers.PursuitHelper;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class Ramming : Outcome
{
    public Ramming(LHandle handle) : base(handle)
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

        if (Suspect.IsAvailable())
        {
            Suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
            GameFiber.Wait(3500);
            Suspect.Tasks.Clear();
        }

        if (Functions.GetCurrentPullover() == null)
        {
            CleanupOutcome(false);
            return;
        }

        PursuitLHandle = SetupPursuitWithList(true, SuspectVehicle.Occupants);
    }
}