using System;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class ShootAndFlee
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");
        internal static LHandle PursuitLHandle;
        internal static void SAFOutcome(LHandle handle)
        {
            Debug("Setting up Suspect");

            Suspect = Functions.GetPulloverSuspect(handle);
            Debug("Setting up suspectVehicle");
            suspectVehicle = Suspect.CurrentVehicle;
            Suspect.BlockPermanentEvents = true;
            Suspect.IsPersistent = true;
            suspectVehicle.IsPersistent = true;

            Debug("Adding all suspect in the vehicle to a list");

            List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
            Debug($"Peds In Vehicle: {PedsInVehicle.Count}");

            int outcome = rndm.Next(1, 101);
            if (outcome >= 50)
            {
                GameFiber.StartNew(() => AllSuspects(PedsInVehicle));
            }
            else if (outcome <= 50)
            {
                GameFiber.StartNew(() => DriverOnly(PedsInVehicle));
            }
        }


        internal static void AllSuspects(List<Ped> Peds)
        {
            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            string Weapon = pistolList[rndm.Next(pistolList.Length)];
            foreach (Ped i in Peds)
            {
                if (i.Exists())
                {
                    if (!i.Inventory.HasLoadedWeapon) { Debug($"Giving Suspect({i}) weapon: {Weapon}"); i.Inventory.GiveNewWeapon(Weapon, 100, true); }

                    Debug($"Setting Suspect({i}) relationship group");
                    i.RelationshipGroup = SuspectRelateGroup;
                    Debug($"Giving Suspect({i}) FightAgainstClosestHatedTarget Task");
                    i.Tasks.FightAgainstClosestHatedTarget(40f, 3750).WaitForCompletion(3750);
                }
            }

            Debug("Wating 3750ms");

            GameFiber.Wait(3750);

            PursuitLHandle = SetupPursuitWithList(true, Peds);
        }

        internal static void DriverOnly(List<Ped> Peds)
        {
            Debug("Setting up SuspectRelateGroup");
            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            Debug("Adding Suspect to SuspectRelateGroup");
            Suspect.RelationshipGroup = SuspectRelateGroup;
            string Weapon = pistolList[rndm.Next(pistolList.Length)];
            Debug("Setting up Suspect weapon/tasks");
            if (!Suspect.Inventory.HasLoadedWeapon) { Debug("Giving Suspect Weapon"); Suspect.Inventory.Weapons.Add(Weapon); }
            Debug("Giving suspect tasks");
            Suspect.Tasks.FightAgainstClosestHatedTarget(40f, 3750).WaitForCompletion(3750);
            PursuitLHandle = SetupPursuitWithList(true, Peds);
        }
    }
}