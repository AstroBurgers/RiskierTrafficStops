using RiskierTrafficStops.Engine.Helpers;
using static RiskierTrafficStops.Engine.Helpers.BDT;

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
        GetOutAndSurrender(hostage.suspect);
        
        if (pedsInVehicle.Count > 2)
        {
            foreach (var ped in pedsInVehicle.Where(i => i != hostage && i.IsAvailable() && i != Suspect))
            {
                if (!ped.IsAvailable())
                {
                    pedsInVehicle.Remove(ped);
                }

                ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
                ped.GiveWeapon();
                ped.Tasks.AimWeaponAt(MainPlayer, -1);
            }

            Suspect.Tasks.LeaveVehicle(Suspect.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            Vector3 suspectPos = hostage.suspect.GetOffsetPosition(new Vector3(0f, 1f, 0f));
            Suspect.Tasks.FollowNavigationMeshToPosition(suspectPos, Suspect.Heading - 180, 2f).WaitForCompletion();
            Suspect.GiveWeapon();
            Suspect.Tasks.AimWeaponAt(hostage, -1);
        }
        else if (pedsInVehicle.Count == 2)
        {
            Vector3 suspectPos = hostage.suspect.GetOffsetPosition(new Vector3(-1.5f, -1f, 0f));
            Suspect.Tasks.FollowNavigationMeshToPosition(suspectPos, Suspect.Heading - 180, 2f).WaitForCompletion();
            Suspect.GiveWeapon();
            Suspect.Tasks.AimWeaponAt(hostage, -1);
        }
        
        Game.DisplaySubtitle(hostageSituationText.PickRandom());
        PlayerLastPos = MainPlayer.Position;
        
        Debug($"IsSuicidal: {suspect.IsSuicidal}");
        Debug($"HatesHostage: {suspect.HatesHostage}");
        Debug($"WantToSurvive: {suspect.WantToSurvive}");
        Debug($"WantsToDieByCop: {suspect.WantsToDieByCop}");
        
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
    }

    private static void GetOutAndSurrender(Ped ped)
    {
        ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.None).WaitForCompletion();
        Vector3 pos = SuspectVehicle.GetOffsetPosition(new Vector3(-1.5f, -1.5f, 0f));
        ped.Tasks.FollowNavigationMeshToPosition(pos, SuspectVehicle.Heading - 180, 2f).WaitForCompletion();
        ped.Tasks.PlayAnimation(new AnimationDictionary("random@getawaydriver"), "idle_2_hands_up", 1f, AnimationFlags.StayInEndFrame).WaitForCompletion(2500);
        ped.Tasks.PlayAnimation(new AnimationDictionary("random@arrests@busted"), "idle_c", 1f, AnimationFlags.Loop);
    }
    
    private static void DetonateBomb()
    {
        Debug("DetonateBomb");
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
        HandlePlayerMovement();
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
        HandlePlayerMovement();
    }

    private static void ShootOutAllSuspects()
    {
        Debug("ShootOutAllSuspects");
        HandlePlayerMovement();
    }

    private static void ShootOut()
    {
        Debug("ShootOut");
        HandlePlayerMovement();
    }

    private static void HandlePlayerMovement()
    {
        if ((PlayerLastPos.X += 2f) < MainPlayer.Position.X || (PlayerLastPos.Y += 2f) < MainPlayer.Position.Y)
        {
            foreach (var ped in pedsInVehicle.Where(i => i.IsAvailable() && i != pedsInVehicle[0]))
            {
                ped.Tasks.FireWeaponAt(pedsInVehicle[0], 2500, FiringPattern.FullAutomatic);
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