﻿using RiskierTrafficStops.Engine.Helpers;
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

        if (pedsInVehicle.Count <= 1)
        {
            CleanupOutcome(true);
            return;
        }

        Suspect suspect = new Suspect(Suspect);
        List<Suspect> suspectsInVehicle = new();
        foreach (var ped in pedsInVehicle.Where(i => i != Suspect))
        {
            
        }
        
        
        Debug($"IsSuicidal: {suspect.IsSuicidal}");
        Debug($"HatesHostage: {suspect.HatesHostage}");
        Debug($"WantToSurvive: {suspect.WantToSurvive}");
        Debug($"WantsToDieByCop: {suspect.WantsToDieByCop}");
        
        // Less than 2 suspects
        Node commitSuicide = new Node(true, null, null, HostageTaking.CommitSuicide);
        Node shootOut = new Node(false, null, null, HostageTaking.ShootOut);
        Node surrender = new Node(true, null, null, HostageTaking.Surrender);

        Node wantsToSurvive = new Node(suspect.WantToSurvive, shootOut, surrender);
        Node wantsToDieByCop = new Node(suspect.WantsToDieByCop, commitSuicide, shootOut);
        Node isSuicidal = new Node(suspect.IsSuicidal, wantsToSurvive, wantsToDieByCop);

        // More than 2 suspects
        Node shootItOut = new Node(false, null, null, ShootOutAllSuspects);
        Node allSurrender = new Node(true, null, null, AllSuspectsSurrender);
        Node shootAtEachother = new Node(true, null, null, HostageTaking.ShootAtEachOther);
        Node killHostageThenShootOut = new Node(true, null, null, HostageTaking.KillHostageThenShootOut);

        Node allWantToSurvive = new Node(suspect.WantToSurvive, shootItOut, allSurrender);
        Node areAnySuicidal = new Node(suspect.IsSuicidal, allWantToSurvive, shootAtEachother);
        Node hateHostage = new Node(suspect.HatesHostage, areAnySuicidal, killHostageThenShootOut);

        // Root Node
        Node moreThan2Suspects = new Node(pedsInVehicle.Count > 2, isSuicidal, hateHostage);

        // Tree
        BDT bdt = new BDT(moreThan2Suspects);
        
        bdt.FollowTruePath();
    }

    private static void KillHostageThenShootOut()
    {
        // byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        // Convert.ToBase64String(textBytes);
        Debug("KillHostageThenShootOut");
    }

    private static void ShootAtEachOther()
    {
        Debug("ShootAtEachOther");
    }

    private static void AllSuspectsSurrender()
    {
        Debug("AllSuspectsSurrender");
    }

    private static void Surrender()
    {
        Debug("Surrender");
    }

    private static void CommitSuicide()
    {
        Debug("CommitSuicide");
    }

    private static void ShootOutAllSuspects()
    {
        Debug("ShootOutAllSuspects");
    }

    private static void ShootOut()
    {
        Debug("ShootOut");
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