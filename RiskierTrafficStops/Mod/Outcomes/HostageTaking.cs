using RiskierTrafficStops.Engine.Helpers;
using static RiskierTrafficStops.Engine.Helpers.Bdt;
using MathHelper = Rage.MathHelper;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class HostageTaking : Outcome
{
    private static Vector3 _playerLastPos = Vector3.Zero;
    private static List<Ped> _pedsInVehicle = new();
    private static Suspect _suspect = new(Suspect);
    private static Suspect _hostage;
    
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
        _pedsInVehicle = SuspectVehicle.Occupants.ToList();

        foreach (var ped in _pedsInVehicle)
        {
            if (ped.IsAvailable() && PedsToIgnore.Contains(ped))
            {
                _pedsInVehicle.Remove(ped);
            }

            ped.BlockPermanentEvents = true;
        }

        if (_pedsInVehicle.Count <= 1)
        {
            CleanupOutcome(true);
            return;
        }
        
        _hostage = new Suspect(_pedsInVehicle[1]);

        // Hostage stuff
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetOutAndSurrender(_hostage.Ped)));

        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
        {
            Vector3 suspectPos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -0.5f, 0f));
            Suspect.Tasks
                .FollowNavigationMeshToPosition(suspectPos, MathHelper.RotateHeading(SuspectVehicle.Heading, 180), 2f)
                .WaitForCompletion();
            Suspect.GiveWeapon();
            NativeFunction.Natives.x9B53BB6E8943AF53(Suspect, _hostage.Ped, -1, false); // TASK_AIM_GUN_AT_ENTITY
        }));

        if (_pedsInVehicle.Count > 2)
        {
            foreach (var ped in _pedsInVehicle.Where(i => i != _hostage.Ped && i.IsAvailable() && i != Suspect))
            {
                if (!ped.IsAvailable())
                {
                    _pedsInVehicle.Remove(ped);
                }

                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                {
                    ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                    ped.GiveWeapon();
                    NativeFunction.Natives.x9B53BB6E8943AF53(ped, MainPlayer, -1, false); // TASK_AIM_GUN_AT_ENTITY
                }));
            }
        }

        Game.DisplaySubtitle(HostageSituationText.PickRandom());
        _playerLastPos = MainPlayer.Position;

        Debug($"IsSuicidal: {_suspect.IsSuicidal}");
        Debug($"HatesHostage: {_suspect.HatesHostage}");
        Debug($"WantToSurvive: {_suspect.WantToSurvive}");
        Debug($"WantsToDieByCop: {_suspect.WantsToDieByCop}");

        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
        {
            GameFiber.Wait(4500);

            // Less than 2 suspects
            Node commitSuicide = new Node(true, null, null, CommitSuicide);
            Node shootOut = new Node(false, null, null, ShootOut);
            Node surrender = new Node(true, null, null, Surrender);

            Node wantsToSurvive = new Node(_suspect.WantToSurvive, shootOut, surrender);
            Node wantsToDieByCop = new Node(_suspect.WantsToDieByCop, commitSuicide, shootOut);
            Node isSuicidal = new Node(_suspect.IsSuicidal, wantsToSurvive, wantsToDieByCop);

            // More than 2 suspects
            Node shootItOut = new Node(false, null, null, ShootOutAllSuspects);
            Node allSurrender = new Node(true, null, null, AllSuspectsSurrender);
            Node shootAtEachother = new Node(true, null, null, ShootAtEachOther);
            Node killHostageThenShootOut = new Node(true, null, null, KillHostageThenShootOut);
            Node detonateBomb = new Node(true, null, null, DetonateBomb);

            Node allWantToSurvive = new Node(_suspect.WantToSurvive, shootItOut, allSurrender);
            Node areAnySuicidal = new Node(_suspect.IsSuicidal, allWantToSurvive, shootAtEachother);
            Node areTerrorists = new Node(_suspect.IsTerrorist, killHostageThenShootOut, detonateBomb);
            Node hateHostage = new Node(_suspect.HatesHostage, areAnySuicidal, areTerrorists);

            // Root Node
            Node moreThan2Suspects = new Node(_pedsInVehicle.Count > 2, isSuicidal, hateHostage);

            // Tree
            Bdt bdt = new Bdt(moreThan2Suspects);

            bdt.FollowTruePath();
        }, "Riskier Traffic Stops BDT Fiber"));
    }

    private static void GetOutAndSurrender(Ped ped)
    {
        if (ped.IsAvailable())
        {
            ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            Vector3 pos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -1.5f, 0f));
            ped.Tasks.FollowNavigationMeshToPosition(pos, MathHelper.RotateHeading(SuspectVehicle.Heading, 180), 2f)
                .WaitForCompletion();
            if (ped.IsAvailable())
            {
                ped.Tasks.PlayAnimation(new AnimationDictionary("random@getawaydriver"), "idle_2_hands_up", 1f,
                    AnimationFlags.StayInEndFrame).WaitForCompletion(2500);
                ped.Tasks.PlayAnimation(new AnimationDictionary("random@arrests@busted"), "idle_c", 1f,
                    AnimationFlags.Loop);
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
        Debug("KillHostageThenShootOut");
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup,
            Relationship.Hate);
        foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
        {
            if (_hostage.Ped.IsAvailable())
            {
                NativeFunction.Natives.x08DA95E8298AE772(ped, _hostage.Ped, -1,
                    Game.GetHashKey("FIRING_PATTERN_FULL_AUTO")); // TASK_SHOOT_AT_ENTITY
                GameFiber.WaitUntil(() => !_hostage.Ped.IsAlive);
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

        for (int i = 0; i < _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped).Count(); i++)
        {
            if (i % 2 == 0)
            {
                _pedsInVehicle[i].RelationshipGroup = SuspectRelateGroup;
            }
            else
            {
                _pedsInVehicle[i].RelationshipGroup = tempGroup;
            }
        }

        foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
        {
            ped.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }

        GameFiber.Wait(7500);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup,
            Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(tempGroup, RelationshipGroup.Cop, Relationship.Hate);
        Game.SetRelationshipBetweenRelationshipGroups(tempGroup, MainPlayer.RelationshipGroup, Relationship.Hate);
        foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
        {
            ped.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
    }

    private static void AllSuspectsSurrender()
    {
        Debug("AllSuspectsSurrender");
        foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
        {
            Game.LogTrivial("Making ped drop weapon...");
            ped.Surrender();
        }
    }

    private static void Surrender()
    {
        Debug("Surrender");
        Game.LogTrivial("Making ped drop weapon...");
        Suspect.Surrender();
    }

    private static void CommitSuicide()
    {
        Debug("CommitSuicide");
        HandlePlayerMovement();
        if (Suspect.IsAvailable())
        {
            NativeFunction.Natives.x6B7513D9966FBEC0(Suspect); // SET_PED_DROPS_WEAPON
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
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup,
            Relationship.Hate);
        foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
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
        Game.SetRelationshipBetweenRelationshipGroups(SuspectRelateGroup, MainPlayer.RelationshipGroup,
            Relationship.Hate);

        if (Suspect.IsAvailable())
        {
            Suspect.RelationshipGroup = SuspectRelateGroup;
            Suspect.Tasks.FightAgainstClosestHatedTarget(50f, -1);
        }
    }

    private static void HandlePlayerMovement()
    {
        Vector3 movementVector = MainPlayer.Position - _playerLastPos;
        float distanceMovedSquared = Vector3.Dot(movementVector, movementVector);

        if (distanceMovedSquared > 2f)
        {
            foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
            {
                NativeFunction.Natives.x08DA95E8298AE772(ped, _hostage.Ped, -1,
                    Game.GetHashKey("FIRING_PATTERN_FULL_AUTO")); // TASK_SHOOT_AT_ENTITY
            }
        }
    }
}