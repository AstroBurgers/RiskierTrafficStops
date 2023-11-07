using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal static class ShootAndFlee
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        private static RelationshipGroup _suspectRelateGroup = new("Suspect");
        internal static LHandle PursuitLHandle;

        internal static void SafOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    CleanupEvent(_suspect, _suspectVehicle);
                    return;
                }

                Debug("Adding all suspect in the vehicle to a list");
                var pedsInVehicle = GetAllVehicleOccupants(_suspectVehicle);
                Debug($"Peds In Vehicle: {pedsInVehicle.Count}");

                GameFiber.Wait(4500);

                var outcome = Rndm.Next(1, 101);
                switch (outcome)
                {
                    case >= 50:
                        GameFiber.StartNew(() => AllSuspects(pedsInVehicle));
                        break;
                    case <= 50:
                        GameFiber.StartNew(() => DriverOnly(pedsInVehicle));
                        break;
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

        private static void AllSuspects(List<Ped> peds)
        {
            _suspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            _suspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            MainPlayer.RelationshipGroup.SetRelationshipWith(_suspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
            RelationshipGroup.Cop.SetRelationshipWith(_suspectRelateGroup, Relationship.Hate);

            var weapon = PistolList[Rndm.Next(PistolList.Length)];
            for (var i = 0; i < peds.Count; i++)
            {
                if (!peds[i].Exists()) { CleanupEvent(peds[i]); continue; }
                if (!peds[i].Inventory.HasLoadedWeapon)
                {
                    Debug($"Giving Suspect #{i} weapon: {weapon}");
                    peds[i].Inventory.GiveNewWeapon(weapon, 500, true);
                }

                Debug($"Making Suspect #{i} shoot at Player");
                NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(peds[i], MainPlayer, 20.0f);
            }

            Debug("Waiting 4500ms");
            GameFiber.Wait(4500);
            if (MainPlayer.Exists() && MainPlayer.IsAlive)
            {
                PursuitLHandle = SetupPursuitWithList(true, peds);
            }
        }

        private static void DriverOnly(List<Ped> peds)
        {
            if (!_suspect.Exists()) { CleanupEvent(_suspect, _suspectVehicle); return; }

            var weapon = PistolList[Rndm.Next(PistolList.Length)];
            Debug("Setting up Suspect Weapon");
            if (!_suspect.Inventory.HasLoadedWeapon) { Debug("Giving Suspect Weapon"); _suspect.Inventory.GiveNewWeapon(weapon, 100, true); }
            Debug("Giving Suspect Tasks");
            NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(_suspect, MainPlayer, 20.0f);
            GameFiber.Wait(4500);
            if (MainPlayer.Exists() && MainPlayer.IsAlive)
            {
                PursuitLHandle = SetupPursuitWithList(true, peds);
            }
        }
    }
}