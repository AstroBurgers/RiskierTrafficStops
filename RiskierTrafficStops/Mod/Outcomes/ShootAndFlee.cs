namespace RiskierTrafficStops.Mod.Outcomes;

internal class ShootAndFlee : Outcome, IUpdateable
{
    public ShootAndFlee(LHandle handle) : base(handle)
    {
        try
        {
            if (!MeetsRequirements(TrafficStopLHandle)) return;
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e);
            CleanupOutcome(true);
        }
    }

    internal virtual void StartOutcome()
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
            i.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search",
                8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
            GameFiber.Wait(Rndm.Next(1, 4) * 750);
            i.GivePistol();

            Normal($"Making Suspect #{i} shoot at Player");
            //i.Tasks.FireWeaponAt(MainPlayer, Rndm.Next(1, 4) * 750, FiringPattern.FullAutomatic);
            NativeFunction.Natives.x10AB107B887214D8(i, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
        }

        GameFiber.Wait(5000);

        SetupPursuitWithList(true, peds);
    }

    private static void DriverOnly()
    {
        if (!Suspect.IsAvailable()) return;
        Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search",
            8f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
        GameFiber.Wait(Rndm.Next(1, 4) * 750);
        Normal("Setting up Suspect Weapon");
        Suspect.GivePistol();

        Normal("Giving Suspect Tasks");
        //Suspect.Tasks.FireWeaponAt(MainPlayer, Rndm.Next(1, 4) * 750, FiringPattern.FullAutomatic);
        NativeFunction.Natives.x10AB107B887214D8(Suspect, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
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