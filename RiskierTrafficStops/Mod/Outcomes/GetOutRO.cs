using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;
using Localization = RiskierTrafficStops.Engine.InternalSystems.Localization;

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
        Flee,
    }

    private static readonly GoRoOutcomes[] AllGoRoOutcomes =
        (GoRoOutcomes[])Enum.GetValues(typeof(GoRoOutcomes));

    private static readonly GunOutcomes[] AllGunOutcomes =
        (GunOutcomes[])Enum.GetValues(typeof(GunOutcomes));

    // Instance field
    private List<Ped> _pedsInVehicle = [];

    public GetOutRo(LHandle handle) : base(handle)
    {
        SuspectRelateGroup = new RelationshipGroup("RTSGoRoSuspects");
        TryStartOutcomeFiber(StartOutcome);
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));

        Normal("Adding all suspects in the vehicle to a list");

        if (!SuspectVehicle.IsAvailable())
        {
            Normal("Suspect vehicle is not available — aborting.");
            CleanupOutcome(true);
            return;
        }

        _pedsInVehicle = SuspectVehicle.Occupants?.ToList() ?? [];

        if (_pedsInVehicle.Count < 1)
        {
            Normal("No peds found in suspect vehicle — aborting.");
            CleanupOutcome(true);
            return;
        }

        RemoveIgnoredPedsAndBlockEvents(ref _pedsInVehicle);

        if (_pedsInVehicle.Count < 1)
        {
            Normal("No valid peds remain after filtering — aborting.");
            CleanupOutcome(true);
            return;
        }

        SetRelationshipGroups(SuspectRelateGroup);

        // Get driver out and facing player before branching
        var driver = _pedsInVehicle[0];
        if (!driver.IsAvailable())
        {
            Normal("Driver is not available — aborting.");
            CleanupOutcome(true);
            return;
        }

        if (driver.IsInAnyVehicle(false))
        {
            driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
        }

        if (!driver.IsAvailable())
        {
            Normal("Driver became unavailable after leaving vehicle — aborting.");
            CleanupOutcome(true);
            return;
        }

        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(driver, MainPlayer, 750);

        var outcome = AllGoRoOutcomes.PickRandom();
        Normal($"Chosen GoRo sub-outcome: {outcome}");

        switch (outcome)
        {
            case GoRoOutcomes.Recording:
                RecordingOutcome(_pedsInVehicle);
                break;
            case GoRoOutcomes.Knife:
                KnifeOutcome(_pedsInVehicle);
                break;
            case GoRoOutcomes.Gun:
                GunOutcome(_pedsInVehicle);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled GoRo outcome");
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }

    private void RecordingOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting recording outcome for the driver only.");

        var driver = pedsInVehicle.FirstOrDefault();
        if (driver is null || !driver.IsAvailable())
        {
            Normal("Driver is not available — aborting recording outcome.");
            return;
        }

        driver.PlayAmbientSpeech(VoiceLines.PickRandom());
        NativeFunction.Natives.x142A02425FF02BD9(driver, "world_human_mobile_film_shocking", 0, true);
        GameFiber.Wait(1500);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(KeyPressed));
    }

    private void KnifeOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting knife outcome");

        var driver = pedsInVehicle.FirstOrDefault(p => p.IsAvailable());
        if (driver is null)
        {
            Normal("No available driver for knife outcome — aborting.");
            return;
        }

        var chance = GenerateChance();
        if (chance <= 50)
        {
            Normal("Knife outcome: driver only");
            GetPedOutWithKnife(driver);

            foreach (var ped in pedsInVehicle.Where(p => p != driver && p.IsAvailable()))
            {
                // Passengers flee from the player, not from themselves
                ped.Tasks.ReactAndFlee(MainPlayer);
            }
        }
        else
        {
            Normal("Knife outcome: all peds");
            foreach (var ped in pedsInVehicle.Where(p => p.IsAvailable()))
            {
                GetPedOutWithKnife(ped);
            }
        }
    }

    private void GetPedOutWithKnife(Ped ped)
    {
        if (!ped.IsAvailable())
        {
            Normal("Ped is not available for knife exit.");
            return;
        }

        Normal("Making ped exit vehicle with knife");
        ped.RelationshipGroup = SuspectRelateGroup;
        ped.BlockPermanentEvents = true;
        ped.IsPersistent = true;

        if (ped.IsInAnyVehicle(false))
        {
            ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
        }

        if (!ped.IsAvailable())
        {
            Normal("Ped became unavailable after leaving vehicle.");
            return;
        }

        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, MainPlayer, 750);

        ped.Tasks.PlayAnimation(new AnimationDictionary("reaction@intimidation@1h"), "intro", 5f,
            AnimationFlags.None);
        GameFiber.Wait(2175);

        if (!ped.IsAvailable()) return;

        ped.Inventory.GiveNewWeapon(MeleeWeapons.PickRandom(), -1, true);
        ped.Tasks.FightAgainst(MainPlayer, -1);
    }

    private void GunOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting gun outcome");

        var driver = pedsInVehicle.FirstOrDefault(p => p.IsAvailable());
        if (driver is null)
        {
            Normal("No available driver for gun outcome — aborting.");
            return;
        }

        var gunOutcome = AllGunOutcomes.PickRandom();
        Normal($"Chosen gun sub-outcome: {gunOutcome}");

        var chance = GenerateChance();
        if (chance <= 50)
        {
            Normal("Gun outcome: driver only");
            GetPedOutWithGun(driver, gunOutcome);

            foreach (var ped in pedsInVehicle.Where(p => p != driver && p.IsAvailable()))
            {
                if (ped.IsInAnyVehicle(false))
                {
                    ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                }

                if (ped.IsAvailable())
                {
                    ped.Tasks.ReactAndFlee(MainPlayer);
                }
            }
        }
        else
        {
            Normal("Gun outcome: all peds");
            foreach (var ped in pedsInVehicle.Where(p => p.IsAvailable()))
            {
                GetPedOutWithGun(ped, gunOutcome);
            }
        }
    }

    private void GetPedOutWithGun(Ped ped, GunOutcomes gunOutcome)
    {
        if (!ped.IsAvailable())
        {
            Normal("Ped is not available for gun exit.");
            return;
        }

        Normal("Making ped exit vehicle with a gun");
        ped.RelationshipGroup = SuspectRelateGroup;
        ped.BlockPermanentEvents = true;
        ped.IsPersistent = true;

        if (ped.IsInAnyVehicle(false))
        {
            ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
        }

        if (!ped.IsAvailable())
        {
            Normal("Ped became unavailable after leaving vehicle.");
            return;
        }

        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, MainPlayer, 750);

        ped.Tasks.PlayAnimation(new AnimationDictionary("reaction@intimidation@1h"), "intro", 5f,
            AnimationFlags.None);
        GameFiber.Wait(2175);

        if (!ped.IsAvailable()) return;

        ped.GivePistol();

        switch (gunOutcome)
        {
            case GunOutcomes.ShootAtPlayer:
                ped.Tasks.FightAgainst(MainPlayer);
                break;
            case GunOutcomes.ShootAtOthers:
                ped.Tasks.FightAgainstClosestHatedTarget(60f, -1);
                break;
            case GunOutcomes.Flee:
                ped.Tasks.Flee(ped, 100f, 10000).WaitForCompletion(10002);
                if (ped.IsAvailable())
                    ped.Tasks.FightAgainstClosestHatedTarget(100f, -1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gunOutcome), gunOutcome, "Unhandled gun outcome");
        }
    }

    private void KeyPressed()
    {
        Game.DisplayHelp(
            $"~BLIP_INFO_ICON~ Press ~{UserConfig.GetBackInKey.GetInstructionalId()}~ {Localization.YellingNotiText}",
            10000);

        while (SuspectVehicle.IsAvailable() && Suspect.IsAvailable() && !Suspect.IsInAnyVehicle(false))
        {
            GameFiber.Yield();

            if (!Game.IsKeyDown(UserConfig.GetBackInKey)) continue;

            Suspect.Tasks.Clear();

            if (SuspectVehicle.IsAvailable())
                Suspect.Tasks.EnterVehicle(SuspectVehicle, -1).WaitForCompletion();

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
            if (!MainPlayer.IsAvailable())
                Abort();
        }
    }

    public void Abort()
    {
        CleanupOutcome(false);
    }
}