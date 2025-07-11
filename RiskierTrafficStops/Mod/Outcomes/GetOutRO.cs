﻿using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;
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
        Flee
    }
    
    private static GoRoOutcomes[] _allGoRoOutcomes =
        (GoRoOutcomes[])Enum.GetValues(typeof(GoRoOutcomes));

    private static List<Ped> _pedsInVehicle;
    
    public GetOutRo(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
        SuspectRelateGroup = new RelationshipGroup("RTSGoRoSuspects");
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

        var driver = _pedsInVehicle[0];
        if (driver.IsAvailable() && driver.IsInAnyVehicle(false))
        {
            driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
        }

        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(driver, MainPlayer, 750);
        
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
        NativeFunction.Natives.x142A02425FF02BD9(driver, "world_human_mobile_film_shocking", 0, true);
        GameFiber.Wait(1500);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(KeyPressed));
    }

    private static void KnifeOutcome(List<Ped> pedsInVehicle)
    {
        Normal("Starting knife outcome");

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

        return;

        void GetPedOutOfVehicle(Ped ped)
        {
            if (!ped.IsAvailable())
            {
                Normal("Ped is not available.");
                return;
            }
            
            Normal($"Making ped exit vehicle with knife");
            ped.RelationshipGroup = SuspectRelateGroup;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;
            if (ped.IsInAnyVehicle(false))
            {
                ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            }
            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, MainPlayer, 750);
            // fake draw
            ped.Tasks.PlayAnimation(new AnimationDictionary("reaction@intimidation@1h"), "intro", 5f,
                AnimationFlags.None);
            GameFiber.Wait(2175);
            // give weapon
            ped.Inventory.GiveNewWeapon(MeleeWeapons.PickRandom(), -1, true);
            
            ped.Tasks.FightAgainst(MainPlayer, -1);
        }
    }
    
    private static void KeyPressed()
    {
        Game.DisplayHelp(
            $"~BLIP_INFO_ICON~ Press ~{UserConfig.GetBackInKey.GetInstructionalId()}~ {Localization.YellingNotiText}",
            10000);
        while (SuspectVehicle.IsAvailable() && !Suspect.IsInAnyVehicle(false))
        {
            GameFiber.Yield();
            if (!Game.IsKeyDown(UserConfig.GetBackInKey)) continue;
            Suspect.Tasks.Clear();
            Suspect.Tasks.EnterVehicle(SuspectVehicle, -1).WaitForCompletion();
            break;
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

        void GetPedOutOfVehicle(Ped ped, GunOutcomes gunOutcomes)
        {
            if (!ped.IsAvailable())
            {
                Normal("Ped is not available.");
                return;
            }
            
            Normal($"Making ped exit vehicle with a gun");
            
            ped.RelationshipGroup = SuspectRelateGroup;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;
            if (ped.IsInAnyVehicle(false))
            {
                ped.Tasks.LeaveVehicle(SuspectVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            }
            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, MainPlayer, 750);
            // fake draw
            ped.Tasks.PlayAnimation(new AnimationDictionary("reaction@intimidation@1h"), "intro", 5f,
                AnimationFlags.None);
            GameFiber.Wait(2175);
            // give weapon
            ped.GivePistol();
            
            switch (gunOutcomes)
            {
                case GunOutcomes.ShootAtPlayer:
                    ped.Tasks.FightAgainst(MainPlayer);
                    break;
                case GunOutcomes.ShootAtOthers:
                    ped.Tasks.FightAgainstClosestHatedTarget(60f, -1);
                    break;
                case GunOutcomes.Flee:
                    ped.Tasks.Flee(ped, 100f, 10000).WaitForCompletion(10002);
                    ped.Tasks.FightAgainstClosestHatedTarget(100f, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gunOutcomes), gunOutcomes, null);
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