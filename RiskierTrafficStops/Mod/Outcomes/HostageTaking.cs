using RiskierTrafficStops.Engine.Helpers;
using static RiskierTrafficStops.Engine.Helpers.BDT;
using MathHelper = Rage.MathHelper;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class HostageTaking : Outcome
{
    internal static Vector3 PlayerLastPos = Vector3.Zero;
    internal static List<Ped> pedsInVehicle = new();
    
    public HostageTaking(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
                SuspectRelateGroup = new RelationshipGroup("RTSHostageTakingSuspects");
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

        Normal("Getting all vehicle occupants");
        pedsInVehicle = SuspectVehicle.Occupants.ToList();

        foreach (var ped in pedsInVehicle)
        {
            if (ped.IsAvailable() && PedsToIgnore.Contains(ped))
            {
                pedsInVehicle.Remove(ped);
            }
            ped.BlockPermanentEvents = true;
        }

        if (pedsInVehicle.Count <= 1)
        {
            CleanupOutcome(true);
            return;
        }
        
        Suspect suspect = new Suspect(Suspect);

        Suspect hostage = new Suspect(pedsInVehicle[1]);
        
        // Hostage stuff
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetOutAndSurrender(hostage.suspect)));
        
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
        {
            Vector3 suspectPos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -0.5f, 0f));
            Suspect.Tasks.FollowNavigationMeshToPosition(suspectPos, MathHelper.RotateHeading(SuspectVehicle.Heading, 180), 2f).WaitForCompletion();
            Suspect.GiveWeapon();
            NativeFunction.Natives.x9B53BB6E8943AF53(Suspect, hostage.suspect, -1, false); // TASK_AIM_GUN_AT_ENTITY
        }));
        
        if (pedsInVehicle.Count > 2)
        {
            foreach (var ped in pedsInVehicle.Where(i => i != hostage.suspect && i.IsAvailable() && i != Suspect))
            {
                if (!ped.IsAvailable())
                {
                    pedsInVehicle.Remove(ped);
                }

                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                {
                    ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                    ped.GiveWeapon();
                    NativeFunction.Natives.x9B53BB6E8943AF53(ped, MainPlayer, -1, false); // TASK_AIM_GUN_AT_ENTITY
                }));
            }
        }
        
        Game.DisplaySubtitle(hostageSituationText.PickRandom());
        PlayerLastPos = MainPlayer.Position;
        
        Debug($"IsSuicidal: {suspect.IsSuicidal}");
        Debug($"HatesHostage: {suspect.HatesHostage}");
        Debug($"WantToSurvive: {suspect.WantToSurvive}");
        Debug($"WantsToDieByCop: {suspect.WantsToDieByCop}");

        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
        {
            GameFiber.Wait(4500);
            
            // Less than 2 suspects
            Node commitSuicide = new Node(true, null, null, CommitSuicide);
            Node shootOut = new Node(false, null, null, ShootOut);
            Node surrender = new Node(true, null, null, Surrender);

            Node wantsToSurvive = new Node(suspect.WantToSurvive, shootOut, surrender);
            Node wantsToDieByCop = new Node(suspect.WantsToDieByCop, commitSuicide, shootOut);
            Node isSuicidal = new Node(suspect.IsSuicidal, wantsToSurvive, wantsToDieByCop);

            // More than 2 suspects
            Node shootItOut = new Node(false, null, null, ShootOutAllSuspects);
            Node allSurrender = new Node(true, null, null, AllSuspectsSurrender);
            Node shootAtEachother = new Node(true, null, null, ShootAtEachOther);
            Node killHostageThenShootOut = new Node(true, null, null, KillHostageThenShootOut);
            Node detonateBomb = new Node(true, null, null, DetonateBomb);
        
            Node allWantToSurvive = new Node(suspect.WantToSurvive, shootItOut, allSurrender);
            Node areAnySuicidal = new Node(suspect.IsSuicidal, allWantToSurvive, shootAtEachother);
            Node areTerrorists = new Node(suspect.IsTerrorist, killHostageThenShootOut, detonateBomb);
            Node hateHostage = new Node(suspect.HatesHostage, areAnySuicidal, areTerrorists);

            // Root Node
            Node moreThan2Suspects = new Node(pedsInVehicle.Count > 2, isSuicidal, hateHostage);

            // Tree
            BDT bdt = new BDT(moreThan2Suspects);
        
            bdt.FollowTruePath();
        }, "Riskier Traffic Stops BDT Fiber"));
    }

    private static void GetOutAndSurrender(Ped ped)
    {
        if (ped.IsAvailable())
        {
            ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            Vector3 pos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -1.5f, 0f));
            ped.Tasks.FollowNavigationMeshToPosition(pos, MathHelper.RotateHeading(SuspectVehicle.Heading, 180), 2f).WaitForCompletion();
            if (ped.IsAvailable())
            {
                ped.Tasks.PlayAnimation(new AnimationDictionary("random@getawaydriver"), "idle_2_hands_up", 1f, AnimationFlags.StayInEndFrame).WaitForCompletion(2500);
                ped.Tasks.PlayAnimation(new AnimationDictionary("random@arrests@busted"), "idle_c", 1f, AnimationFlags.Loop);
            }
        }
    }
    
    private static void DetonateBomb()
    {
        Debug("DetonateBomb");
        if (SuspectVehicle.IsAvailable())
        {
            NativeFunction.Natives.EXPLODE_VEHICLE(SuspectVehicle, true, false);
        }
    }
    
    private static void KillHostageThenShootOut()
    {
        // byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        // Convert.ToBase64String(textBytes);
        Debug("KillHostageThenShootOut");
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup, Relationship.Hate);
        foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]))
        {
            if (pedsInVehicle[1].IsAvailable())
            {
                NativeFunction.Natives.x08DA95E8298AE772(ped, pedsInVehicle[1], -1, Game.GetHashKey("FIRING_PATTERN_FULL_AUTO")); // TASK_SHOOT_AT_ENTITY
                GameFiber.WaitUntil(() => !pedsInVehicle[1].IsAlive);
            }

            ped.RelationshipGroup = SuspectRelateGroup;
            ped.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
    }

    private static void ShootAtEachOther()
    {
        Debug("ShootAtEachOther");
        HandlePlayerMovement();

        var tempGroup = new RelationshipGroup("RTSShootingAtEachOtherSuspects");
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, tempGroup, Relationship.Hate);

        if (Suspect.Exists()) Suspect.RelationshipGroup = tempGroup;

        for (int i = 0; i < pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]).Count(); i++)
        {
            if (i % 2 == 0)
            {
                pedsInVehicle[i].RelationshipGroup = SuspectRelateGroup;
            }
            else
            {
                pedsInVehicle[i].RelationshipGroup = tempGroup;
            }
        }
        
        foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]))
        {
            ped.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
        
        GameFiber.Wait(7500);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(tempGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(tempGroup, MainPlayer.RelationshipGroup, Relationship.Hate);
        foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]))
        {
            ped.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
    }

    private static void AllSuspectsSurrender()
    {
        Debug("AllSuspectsSurrender");
        foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]))
        {
            Game.LogTrivial("Making ped drop weapon...");
            NativeFunction.Natives.x6B7513D9966FBEC0(ped); // SET_PED_DROPS_WEAPON
            ped.Tasks.PutHandsUp(-1, MainPlayer);
        }
    }

    private static void Surrender()
    {
        Debug("Surrender");
        if (Suspect.IsAvailable())
        {
            Game.LogTrivial("Making ped drop weapon...");
            NativeFunction.Natives.x6B7513D9966FBEC0(Suspect); // SET_PED_DROPS_WEAPON
            Suspect.Tasks.PutHandsUp(-1, MainPlayer);
        }
    }

    private static void CommitSuicide()
    {
        Debug("CommitSuicide");
        HandlePlayerMovement();
        if (Suspect.IsAvailable())
        {
            Suspect.Inventory.GiveNewWeapon("WEAPON_UNARMED", -1, true);
            Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_suicide"), "pill", 5f, AnimationFlags.None);
            GameFiber.Wait(2500);
            Suspect.Kill();
        }
    }

    private static void ShootOutAllSuspects()
    {
        Debug("ShootOutAllSuspects");
        HandlePlayerMovement();
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup, Relationship.Hate);
        foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]))
        {
            ped.RelationshipGroup = SuspectRelateGroup;
            ped.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
    }

    private static void ShootOut()
    {
        Debug("ShootOut");
        HandlePlayerMovement();
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup, Relationship.Hate);

        if (Suspect.IsAvailable())
        {
            Suspect.RelationshipGroup = SuspectRelateGroup;
            Suspect.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
    }

    private static void HandlePlayerMovement()
    {
        Vector3 movementVector = MainPlayer.Position - PlayerLastPos;
        float distanceMovedSquared = Vector3.Dot(movementVector, movementVector);
        
        if (distanceMovedSquared > 2f)
        {
            foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[1]))
            {
                NativeFunction.Natives.x08DA95E8298AE772(ped, pedsInVehicle[1], -1, Game.GetHashKey("FIRING_PATTERN_FULL_AUTO")); // TASK_SHOOT_AT_ENTITY
            }
        }
    }
}

internal class Suspect : Ped
{
    internal Ped suspect;

    internal bool IsSuicidal { get; private set; }
    internal bool WantToSurvive { get; private set; }
    internal bool HatesHostage { get; private set; }
    internal bool WantsToDieByCop { get; private set; }
    internal bool IsTerrorist { get; private set; }
    
    internal Suspect(Ped ped)
    {
        suspect = ped;
        IsSuicidal = GenerateChance() < IsSuicidalChance;
        WantToSurvive = GenerateChance() < WantsToSurviveChance;
        WantsToDieByCop = GenerateChance() < WantsToDieBieCopChance;
        HatesHostage = GenerateChance() < HatesHostageChance;
        IsTerrorist = GenerateChance() < IsTerroristChance;
    }
}