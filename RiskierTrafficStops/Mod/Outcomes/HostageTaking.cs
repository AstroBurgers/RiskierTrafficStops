using RiskierTrafficStops.Engine.Helpers;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.BDT;
using static RiskierTrafficStops.Mod.Outcomes.HostageTaking;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class HostageTaking : Outcome
{
    public HostageTaking(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
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
        var pedsInVehicle = SuspectVehicle.Occupants.ToList();

        foreach (var ped in pedsInVehicle)
        {
            if (ped.IsAvailable() && PedsToIgnore.Contains(ped))
            {
                pedsInVehicle.Remove(ped);
            }
        }

        if (pedsInVehicle.Count <= 1) CleanupOutcome(true);

        Suspect suspect = new Suspect(Suspect);

        // Less than 2 suspects
        Node CommitSuicide = new Node(true, null, null, HostageTaking.CommitSuicide);
        Node ShootOut = new Node(false, null, null, HostageTaking.ShootOut);
        Node Surrender = new Node(true, null, null, HostageTaking.Surrender);

        Node WantsToSurvive = new Node(suspect.WantToSurvive, ShootOut, Surrender);
        Node ShouldCommit = new Node(true, ShootOut, CommitSuicide);
        Node IsSuicidal = new Node(suspect.IsSuicidal, WantsToSurvive, ShouldCommit);

        // More than 2 suspects
        Node ShootItOut = new Node(false, null, null, ShootOutAllSuspects);
        Node AllSurrender = new Node(true, null, null, AllSuspectsSurrender);
        Node ShootAtEachother = new Node(true, null, null, HostageTaking.ShootAtEachOther);
        Node KillHostageThenShootOut = new Node(true, null, null, HostageTaking.KillHostageThenShootOut);

        Node AllWantToSurvive = new Node(suspect.WantToSurvive, ShootItOut, AllSurrender);
        Node AreAnySuicidal = new Node(suspect.IsSuicidal, AllWantToSurvive, ShootAtEachother);
        Node HateHostage = new Node(suspect.HatesHostage, AreAnySuicidal, KillHostageThenShootOut);

        // Root Node
        Node MoreThan2Suspects = new Node(pedsInVehicle.Count > 2, IsSuicidal, HateHostage);

        // Tree
        BDT bdt = new BDT(MoreThan2Suspects);
        
        bdt.FollowTruePath();
    }

    private static void KillHostageThenShootOut()
    {
    }

    private static void ShootAtEachOther()
    {
    }

    private static void AllSuspectsSurrender()
    {
    }

    private static void Surrender()
    {
    }

    private static void CommitSuicide()
    {
    }

    private static void ShootOutAllSuspects()
    {
    }

    private static void ShootOut()
    {
    }
}

internal class Suspect : Ped
{
    internal Ped suspect;

    internal bool IsSuicidal = false;
    internal bool WantToSurvive = false;
    internal bool HatesHostage = false;


    internal Suspect(Ped ped)
    {
        suspect = ped;
        IsSuicidal = GenerateChance() < 40;
        WantToSurvive = GenerateChance() < 30;
        HatesHostage = GenerateChance() < 20;
    }
}