using RiskierTrafficStops.Engine.Data;
using RiskierTrafficStops.Engine.Helpers;
using MathHelper = Rage.MathHelper;

namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class HostageTaking : Outcome, IUpdateable
{
    private static Vector3 _playerLastPos = Vector3.Zero;
    private static List<Ped> _pedsInVehicle = new();
    private static Suspect _suspect = new(Suspect);
    private static Suspect _hostage;
    //private static SoundPlayer _soundPlayer = new(@"LSPDFR\RiskierTrafficStops\bomb_beep.wav");

    public HostageTaking(LHandle handle) : base(handle)
    {
        try
        {
            if (!MeetsRequirements(TrafficStopLHandle)) return;
            SuspectRelateGroup = new RelationshipGroup("RTSHostageTakingSuspects");
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
        }
        catch (Exception e) when (e is not ThreadAbortException)
        {
            Error(e);
            CleanupOutcome(true);
        }
    }

    internal void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));
        Normal("Adding all suspect in the vehicle to a list");

        if (SuspectVehicle.IsAvailable()) {
            _pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        Debug("RemoveIgnoredPedsAndBlockEvents");
        RemoveIgnoredPedsAndBlockEvents(ref _pedsInVehicle);
        
        Debug("Checking pedsinveh");
        Debug($"{_pedsInVehicle.Count}");
        if (_pedsInVehicle.Count <= 1)
        {
            CleanupOutcome(true);
            return;
        }

        Debug("Settings hostage");
        _hostage = new Suspect(_pedsInVehicle[1]);

        // Hostage stuff
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetOutAndSurrender(_hostage.Ped)));
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(HandleSuspect));

        Debug("MakePedLeaveVehicle");
        foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped && i != Suspect))
        {
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => MakePedLeaveVehicle(ped)));
        }

        Debug("Displaying subtitle");
        Game.DisplaySubtitle(HostageSituationText.PickRandom());
        Debug("Settings playLastPos");
        _playerLastPos = MainPlayer.Position;

        Debug("DebugSuspectProperties");
        DebugSuspectProperties();

        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
        {
            GameFiber.Wait(9000);

            Debug("Doing tree shit");
            // Less than 2 suspects
            var commitSuicide = new Bdt.Node(true, null, null, CommitSuicide);
            var shootOut = new Bdt.Node(false, null, null, ShootOut);
            var surrender = new Bdt.Node(true, null, null, Surrender);
            
            var wantsToSurvive = new Bdt.Node(_suspect.WantToSurvive, shootOut, surrender);
            var wantsToDieByCop = new Bdt.Node(_suspect.WantsToDieByCop, commitSuicide, shootOut);
            var isSuicidal = new Bdt.Node(_suspect.IsSuicidal, wantsToSurvive, wantsToDieByCop);
            
            // More than 2 suspects
            var shootItOut = new Bdt.Node(false, null, null, ShootOutAllSuspects);
            var allSurrender = new Bdt.Node(true, null, null, AllSuspectsSurrender);
            var shootAtEachOther = new Bdt.Node(true, null, null, ShootAtEachOther);
            var killHostageThenShootOut = new Bdt.Node(true, null, null, KillHostageThenShootOut);
            var detonateBomb = new Bdt.Node(true, null, null, DetonateBomb);
            
            var allWantToSurvive = new Bdt.Node(_suspect.WantToSurvive, shootItOut, allSurrender);
            var areAnySuicidal = new Bdt.Node(_suspect.IsSuicidal, allWantToSurvive, shootAtEachOther);
            var areTerrorists = new Bdt.Node(_suspect.IsTerrorist, killHostageThenShootOut, detonateBomb);
            var hateHostage = new Bdt.Node(_suspect.HatesHostage, areAnySuicidal, areTerrorists);
            
            // Root Node
            var moreThan2Suspects = new Bdt.Node(_pedsInVehicle.Count > 2, isSuicidal, hateHostage);
            
            // Tree
            var bdt = new Bdt(moreThan2Suspects);
            
            Debug("bdt.FollowTruePath();");
            bdt.FollowTruePath();
            Debug("InvokeEvent(RTSEventType.End);");
            InvokeEvent(RTSEventType.End);
            
            //DetonateBomb();
        }, "Riskier Traffic Stops BDT Fiber"));
    }

    private void DebugSuspectProperties()
    {
        Debug($"IsSuicidal: {_suspect.IsSuicidal}");
        Debug($"HatesHostage: {_suspect.HatesHostage}");
        Debug($"WantToSurvive: {_suspect.WantToSurvive}");
        Debug($"WantsToDieByCop: {_suspect.WantsToDieByCop}");
        Debug($"IsTerrorist: {_suspect.IsTerrorist}");
    }

    private static void MakePedLeaveVehicle(Ped ped)
    {
        ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
        ped.GiveWeapon();
        NativeFunction.Natives.x9B53BB6E8943AF53(ped, MainPlayer, -1, false); // TASK_AIM_GUN_AT_ENTITY
    }

    private static void HandleSuspect()
    {
        if (_suspect.IsAvailable())
        {
            var suspectPos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -0.5f, 0f));
            Suspect.Tasks
                .FollowNavigationMeshToPosition(suspectPos, MathHelper.RotateHeading(SuspectVehicle.Heading, 180), 2f)
                .WaitForCompletion();
            Suspect.GiveWeapon();
            NativeFunction.Natives.x9B53BB6E8943AF53(Suspect, _hostage.Ped, -1, false); // TASK_AIM_GUN_AT_ENTITY
        }
    }

    private static void GetOutAndSurrender(Ped ped)
    {
        if (ped.IsAvailable())
        {
            ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            var pos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -1.5f, 0f));
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

    private static void HandlePlayerMovement()
    {
        Normal("Started Player movement process");
        _playerLastPos = MainPlayer.Position;
        GameFiber.Wait(500);
        var movementVector = MainPlayer.Position - _playerLastPos;
        var distanceMovedSquared = Vector3.Dot(movementVector, movementVector);

        if (distanceMovedSquared > 2f)
        {
            foreach (var ped in _pedsInVehicle.Where(i => i.IsAvailable() && i != _hostage.Ped))
            {
                NativeFunction.Natives.x08DA95E8298AE772(ped, _hostage.Ped, -1,
                    Game.GetHashKey("FIRING_PATTERN_FULL_AUTO")); // TASK_SHOOT_AT_ENTITY
            }
        }
    }

    private static void DetonateBomb()
    {
        Debug("DetonateBomb");
        if (SuspectVehicle.IsAvailable())
        {
            //GameFiber.StartNew(_soundPlayer.PlaySound2D);
            //GameFiberHandling.OutcomeGameFibers.Add();
            Game.DisplayHelp("A bomb is going to detonate any second!");
            GameFiber.Wait(6639);
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

        for (var i = 0; i < _pedsInVehicle.Count(ped => ped.IsAvailable() && ped != _hostage.Ped); i++)
        {
            _pedsInVehicle[i].RelationshipGroup = i % 2 == 0 ? SuspectRelateGroup : tempGroup;
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
    
    // Processing methods
    public void Start()
    {
        Normal($"Started checks for {ActiveOutcome}");
        
        while (ActiveOutcome is not null)
        {
            GameFiber.Yield();
            if (Functions.GetCurrentPullover() is null || !MainPlayer.IsAvailable())
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