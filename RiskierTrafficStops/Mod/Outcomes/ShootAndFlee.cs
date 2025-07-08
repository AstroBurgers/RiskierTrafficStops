namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class ShootAndFlee : Outcome, IProccessing
{
    private static bool _shouldPedsWaitForPlayer;
    
    public ShootAndFlee(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));
        Normal("Adding all suspect in the vehicle to a list");
        var pedsInVehicle = new List<Ped>();
        if (SuspectVehicle.IsAvailable()) {
            pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        if (pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }
        
        RemoveIgnoredPedsAndBlockEvents(ref pedsInVehicle);
        
        var chance = GenerateChance();
        _shouldPedsWaitForPlayer = chance <= 65;
        switch (chance)
        {
            case <= 60:
                Normal("Starting all suspects outcome");
                AllSuspects(pedsInVehicle);
                break;
            default:
                Normal("Starting driver only outcome");
                DriverOnly();
                break;
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }

    private static void AllSuspects(List<Ped> peds)
    {
        foreach (var i in peds.Where(i => i.IsAvailable()))
        {
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
            {
                i.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search",
                    8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly).WaitForCompletion();

                if (_shouldPedsWaitForPlayer) {
                    GameFiber.WaitUntil(() => MainPlayer.DistanceTo2D(SuspectVehicle) < 5f);
                }
                else {
                    GameFiber.Wait(Rndm.Next(1, 4) * 750);
                }
                
                i.GivePistol();

                Normal($"Making Suspect #{i} shoot at Player");
                i.Tasks.FireWeaponAt(MainPlayer, Rndm.Next(1, 4) * 750, FiringPattern.FullAutomatic);
                //NativeFunction.Natives.x10AB107B887214D8(i, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
            }));
        }

        GameFiber.Wait(7500);

        SetupPursuitWithList(true, peds);
    }

    private static void DriverOnly()
    {
        if (!Suspect.IsAvailable()) return;
        Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search",
            8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly).WaitForCompletion();
        
        if (_shouldPedsWaitForPlayer) {
            GameFiber.WaitUntil(() => MainPlayer.DistanceTo2D(SuspectVehicle) < 5f);
        }
        else {
            GameFiber.Wait(Rndm.Next(1, 4) * 750);
        }
        
        Normal("Setting up Suspect Weapon");
        Suspect.GivePistol();

        Normal("Giving Suspect Tasks");
        Suspect.Tasks.FireWeaponAt(MainPlayer, Rndm.Next(1, 4) * 750, FiringPattern.FullAutomatic);
        //NativeFunction.Natives.x10AB107B887214D8(Suspect, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
        GameFiber.Wait(5000);

        SetupPursuit(true, Suspect);
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