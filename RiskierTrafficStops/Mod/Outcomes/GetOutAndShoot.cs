using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class GetOutAndShoot : Outcome, IProccessing
{
    private static GetOutAndShootOutcomes[] _allGoasOutcomes =
        (GetOutAndShootOutcomes[])Enum.GetValues(typeof(GetOutAndShootOutcomes));

    private static List<Ped> _pedsInVehicle;

    // RTSGetOutAndShootSuspects
    public GetOutAndShoot(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
        SuspectRelateGroup = new RelationshipGroup("RTSGetOutAndShootSuspects");
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));
        Normal("Adding all suspect in the vehicle to a list");

        if (SuspectVehicle.IsAvailable()) {
            _pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        if (_pedsInVehicle.Count < 1)
        {
            Normal("No peds found in suspect vehicle — aborting.");
            CleanupOutcome(true);
            return;
        }

        if (!RemoveIgnoredPedsAndBlockEvents(ref _pedsInVehicle))
            return;
        
        SetRelationshipGroups(SuspectRelateGroup);

        foreach (Ped ped in _pedsInVehicle.Where(ped => ped.IsAvailable()))
        {
            ped.GiveWeapon();
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetPedOutOfVehicle(ped)));
        }

        GameFiber.Wait(7010);

        Normal("Choosing outcome from GetOutAndShootOutcomes");
        GetOutAndShootOutcomes chosenOutcome = _allGoasOutcomes.PickRandom();
        Normal($"Chosen Outcome: {chosenOutcome}");

        switch (chosenOutcome)
        {
            case GetOutAndShootOutcomes.Flee:
                SetupPursuitWithList(true, _pedsInVehicle);
                break;
            case GetOutAndShootOutcomes.KeepShooting:
                foreach (Ped i in _pedsInVehicle.Where(i => i.IsAvailable()))
                {
                    Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
                    i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        InvokeEvent(RTSEventType.End);
    }

    private static void GetPedOutOfVehicle(Ped ped)
    {
        if (!ped.IsAvailable()) return;

        ped.RelationshipGroup = SuspectRelateGroup;

        if (ped.IsInVehicle(ped.LastVehicle, false) && ped.LastVehicle.IsAvailable())
        {
            Normal("Making Suspect leave vehicle");
            ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
        }

        // Add small delay after getting out of vehicle to let ped fully stand up
        GameFiber.Sleep(100);

        if (!ped.IsAvailable()) return;

        Normal("Giving Suspect FightAgainstClosestHatedTarget Task");

        try
        {
            ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7001);
        }
        catch (Exception ex)
        {
            Error(new Exception("Failed to assign FightAgainstClosestHatedTarget task", ex));
        }
    }


    private enum GetOutAndShootOutcomes
    {
        Flee,
        KeepShooting
    }
    
    // Processing methods
    public void Start()
    {
        Normal($"Started checks for {ActiveOutcome}");
        
        while (ActiveOutcome is not null)
        {
            GameFiber.Yield();
            if (!MainPlayer.IsAvailable())
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