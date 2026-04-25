namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class ShootAndFlee : Outcome, IProccessing
{
    private bool _shouldPedsWaitForPlayer;

    public ShootAndFlee(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
    }

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));

        Normal("Adding all suspects in the vehicle to a list");
        var pedsInVehicle = new List<Ped>();
        if (SuspectVehicle.IsAvailable())
            pedsInVehicle = SuspectVehicle.Occupants?.ToList() ?? [];

        if (pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }

        RemoveIgnoredPedsAndBlockEvents(ref pedsInVehicle);

        if (pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }

        var chance = GenerateChance();
        _shouldPedsWaitForPlayer = chance <= 65;

        if (chance <= 60)
        {
            Normal("Starting all suspects outcome");
            AllSuspects(pedsInVehicle);
        }
        else
        {
            Normal("Starting driver only outcome");
            DriverOnly();
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }

    private void AllSuspects(List<Ped> peds)
    {
        foreach (var ped in peds.Where(p => p.IsAvailable()))
        {
            var capturedPed = ped;
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
            {
                if (!capturedPed.IsAvailable()) return;

                capturedPed.Tasks.PlayAnimation(
                        new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"),
                        "player_search", 8f,
                        AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly)
                    .WaitForCompletion(3000);

                if (!capturedPed.IsAvailable()) return;

                if (_shouldPedsWaitForPlayer)
                    GameFiber.WaitUntil(() =>
                        !MainPlayer.IsAvailable() || MainPlayer.DistanceTo2D(SuspectVehicle) < 5f);
                else
                    GameFiber.Wait(Rndm.Next(1, 4) * 750);

                if (!capturedPed.IsAvailable()) return;

                capturedPed.GivePistol();
                AssignShootTask(capturedPed);
            }));
        }

        GameFiber.Wait(7500);
        SetupPursuitWithList(true, peds);
    }

    private void DriverOnly()
    {
        if (!Suspect.IsAvailable()) return;

        Suspect.Tasks.PlayAnimation(
                new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"),
                "player_search", 8f,
                AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly)
            .WaitForCompletion(3000);

        if (!Suspect.IsAvailable()) return;

        if (_shouldPedsWaitForPlayer)
            GameFiber.WaitUntil(() => !MainPlayer.IsAvailable() || MainPlayer.DistanceTo2D(SuspectVehicle) < 5f);
        else
            GameFiber.Wait(Rndm.Next(1, 4) * 750);

        if (!Suspect.IsAvailable()) return;

        Suspect.GivePistol();
        AssignShootTask(Suspect);

        GameFiber.Wait(5000);

        if (Suspect.IsAvailable())
            SetupPursuit(true, Suspect);
    }

    /// <summary>
    /// Assigns the appropriate shoot task depending on whether the ped is in a vehicle or on foot.
    /// TASK_VEHICLE_SHOOT_AT_PED handles in-vehicle shooting; FightAgainst handles on-foot.
    /// </summary>
    private static void AssignShootTask(Ped ped)
    {
        if (!ped.IsAvailable()) return;

        if (ped.IsInAnyVehicle(false))
        {
            Normal($"Suspect is in vehicle — using TASK_VEHICLE_SHOOT_AT_PED");
            NativeFunction.Natives.x10AB107B887214D8(ped, MainPlayer, 20.0f);
        }
        else
        {
            Normal($"Suspect is on foot — using FightAgainst");
            ped.Tasks.FightAgainst(MainPlayer, -1);
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
                Abort();
        }
    }

    public void Abort()
    {
        CleanupOutcome(false);
    }
}