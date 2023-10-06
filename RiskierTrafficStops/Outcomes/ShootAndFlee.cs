using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class ShootAndFlee
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new("Suspect");
        internal static LHandle PursuitLHandle;
        internal static void SAFOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                Debug("Adding all suspect in the vehicle to a list");
                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
                Debug($"Peds In Vehicle: {PedsInVehicle.Count}");

                GameFiber.Wait(4500);

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
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "ShootAndFlee.cs");
            }
        }


        internal static void AllSuspects(List<Ped> Peds)
        {
            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            MainPlayer.RelationshipGroup.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
            RelationshipGroup.Cop.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate);

            string Weapon = pistolList[rndm.Next(pistolList.Length)];
            foreach (Ped i in Peds)
            {
                if (i.Exists())
                {
                    if (!i.Inventory.HasLoadedWeapon) { Debug($"Giving Suspect weapon: {Weapon}"); i.Inventory.GiveNewWeapon(Weapon, 100, true); }

                    Debug($"Setting Suspect relationship group");
                    i.RelationshipGroup = SuspectRelateGroup;
                    Debug($"Giving Suspect FightAgainstClosestHatedTarget Task");
                    NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(i, MainPlayer, 20.0f);
                }
            }

            Debug("Wating 3750ms");

            GameFiber.Wait(3750);
            if (MainPlayer.Exists() && MainPlayer.IsAlive)
            {
                PursuitLHandle = SetupPursuitWithList(true, Peds);
            }
        }

        internal static void DriverOnly(List<Ped> Peds)
        {
            if (!Suspect.Exists()) { CleanupEvent(Suspect, suspectVehicle); return; }

            Debug("Setting up SuspectRelateGroup");

            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            MainPlayer.RelationshipGroup.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
            RelationshipGroup.Cop.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate);

            Debug("Adding Suspect to SuspectRelateGroup");
            Suspect.RelationshipGroup = SuspectRelateGroup;

            string Weapon = pistolList[rndm.Next(pistolList.Length)];
            Debug("Setting up Suspect weapon/tasks");
            if (!Suspect.Inventory.HasLoadedWeapon) { Debug("Giving Suspect Weapon"); Suspect.Inventory.GiveNewWeapon(Weapon, 100, true); }
            Debug("Giving suspect tasks");
            NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(Suspect, MainPlayer, 20.0f);
            GameFiber.Wait(3750);
            if (MainPlayer.Exists() && MainPlayer.IsAlive)
            {
                PursuitLHandle = SetupPursuitWithList(true, Peds);
            }
        }
    }
}