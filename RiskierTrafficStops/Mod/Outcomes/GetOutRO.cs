using RiskierTrafficStops.Engine.Data;
using RiskierTrafficStops.Engine.Helpers;
using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class GetOutRo : Outcome, IProccessing
{
    private enum GoRoOutcomes
    {
        Recording,
        Knife,
        Gun,
    }
    
    private enum GunOutcomes
    {
        ShootAtPlayer,
        ShootAtOthers,
        Flee
    }
    
    private static GoRoOutcomes _chosenOutcome;

    private static GoRoOutcomes[] _allGoRoOutcomes =
        (GoRoOutcomes[])Enum.GetValues(typeof(GoRoOutcomes));

    private static List<Ped> _pedsInVehicle;
    
    public GetOutRo(LHandle handle) : base(handle)
    {
        try
        {
            if (!MeetsRequirements(TrafficStopLHandle)) return;
            SuspectRelateGroup = new RelationshipGroup("RTSGoRoSuspects");
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e);
            CleanupOutcome(true);
        }
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));
        Normal("Adding all suspect in the vehicle to a list");

        if (SuspectVehicle.IsAvailable()) {
            _pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        if (_pedsInVehicle.Count < 1) {
            CleanupOutcome(true);
            return;
        }
        
        RemoveIgnoredPedsAndBlockEvents(ref _pedsInVehicle);
        
        SetRelationshipGroups(SuspectRelateGroup);

        //
        
        var outcome = _allGoRoOutcomes.PickRandom();

        switch (outcome)
        {
            case GoRoOutcomes.Recording:
                // Only driver
                RecordingOutcome(_pedsInVehicle);
                break;
            case GoRoOutcomes.Knife:
                KnifeOutcome(_pedsInVehicle);
                break;
            case GoRoOutcomes.Gun:
                GunOutcome(_pedsInVehicle);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        InvokeEvent(RTSEventType.End);
    }
    
    private static void RecordingOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting recording outcome for the driver only.");
        
        var driver = pedsInVehicle.FirstOrDefault();
        if (!driver.IsAvailable() || driver is null) {
            Normal("Driver wasn't drivering in this world... aborting recording outcome.");
            return;
        }

        // Start recording
        driver.PlayAmbientSpeech(VoiceLines.PickRandom());
        // Play animation and wait for completion
    }

    private static void KnifeOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting knife outcome");
        
        void GetPedOutOfVehicle(Ped ped)
        {
            if (!ped.IsAvailable())
            {
                Normal("Ped is not available.");
                return;
            }
            
            Normal($"Making ped exit vehicle with knife");
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;
            if (ped.IsInAnyVehicle(false))
            {
                ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            }
            ped.Face(MainPlayer);
            ped.Inventory.GiveNewWeapon(MeleeWeapons.PickRandom(), -1, true);
            ped.Tasks.FightAgainstClosestHatedTarget(40f, -1);
        }
        
        var chance = GenerateChance();
        if (chance <= 50)
        {
            // Only driver
            var driver = pedsInVehicle[0];
            GetPedOutOfVehicle(driver);

            foreach (var ped in pedsInVehicle.Where(p => p != driver && p.IsAvailable()))
            {
                ped.Tasks.ReactAndFlee(ped);
            }
        }else
        {
            // All peds in vehicle
            foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable()))
            {
                GetPedOutOfVehicle(ped);
            }
        }
    }
    
    private static void GunOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting Gun outcome");

        var chance = GenerateChance();
        var outcomeList = (GunOutcomes[])Enum.GetValues(typeof(GunOutcomes));
        var outcome = outcomeList.PickRandom();
        if (chance <= 50)
        {
            // Only driver
            var driver = pedsInVehicle[0];
            GetPedOutOfVehicle(driver, outcome);

            foreach (var ped in pedsInVehicle.Where(p => p != driver && p.IsAvailable()))
            {
                ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                ped.Tasks.ReactAndFlee(ped);
            }
        }else
        {
            // All peds in vehicle
            foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable()))
            {
                GetPedOutOfVehicle(ped, outcome);
            }
        }

        return;

        void GetPedOutOfVehicle(Ped ped, GunOutcomes outcome)
        {
            if (!ped.IsAvailable())
            {
                Normal("Ped is not available.");
                return;
            }
            
            Normal($"Making ped exit vehicle with a gun");
            
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;
            if (ped.IsInAnyVehicle(false))
            {
                ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            }
            ped.Face(MainPlayer);
            ped.GivePistol();
            
            switch (outcome)
            {
                case GunOutcomes.ShootAtPlayer:
                    ped.Tasks.FightAgainst(MainPlayer);
                    break;
                case GunOutcomes.ShootAtOthers:
                    ped.Tasks.FightAgainstClosestHatedTarget(60f, -1);
                    break;
                case GunOutcomes.Flee:
                    ped.Tasks.Flee(ped, 100f, 10000).WaitForCompletion();
                    ped.Tasks.FightAgainstClosestHatedTarget(100f, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }
        }
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