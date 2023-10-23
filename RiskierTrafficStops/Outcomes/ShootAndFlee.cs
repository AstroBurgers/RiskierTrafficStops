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
            for (int i = 0; i < Peds.Count; i++)
            {
                if (!Peds[i].Exists()) { CleanupEvent(Peds[i]); continue; }
                if (!Peds[i].Inventory.HasLoadedWeapon)
                {
                    Debug($"Giving Suspect #{i} weapon: {Weapon}");
                    Peds[i].Inventory.GiveNewWeapon(Weapon, 500, true);
                }

                Debug($"Making Suspect #{i} shoot at Player");
                NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(Peds[i], MainPlayer, 20.0f);
            }

            Debug("Wating 4500ms");
            GameFiber.Wait(4500);
            if (MainPlayer.Exists() && MainPlayer.IsAlive)
            {
                PursuitLHandle = SetupPursuitWithList(true, Peds);
            }
        }

        internal static void DriverOnly(List<Ped> Peds)
        {
            if (!Suspect.Exists()) { CleanupEvent(Suspect, suspectVehicle); return; }

            string Weapon = pistolList[rndm.Next(pistolList.Length)];
            Debug("Setting up Suspect Weapon");
            if (!Suspect.Inventory.HasLoadedWeapon) { Debug("Giving Suspect Weapon"); Suspect.Inventory.GiveNewWeapon(Weapon, 100, true); }
            Debug("Giving Suspect Tasks");
            NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(Suspect, MainPlayer, 20.0f);
            GameFiber.Wait(4500);
            if (MainPlayer.Exists() && MainPlayer.IsAlive)
            {
                PursuitLHandle = SetupPursuitWithList(true, Peds);
            }
        }
    }
}