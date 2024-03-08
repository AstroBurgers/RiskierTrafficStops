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
        if (Suspect.IsAvailable())
        {
            Suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
            GameFiber.Wait(3500);
            if (Suspect.IsAvailable()) {
                Suspect.Tasks.Clear();
            }
        }

        if (Functions.GetCurrentPullover() == null)
        {
            CleanupOutcome(false);
            return;
        }

        PursuitLHandle = SetupPursuitWithList(true, SuspectVehicle.Occupants);
    }
}