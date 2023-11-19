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
        private static RelationshipGroup _suspectRelateGroup = new("RTSShootAndFleeSuspects");
        internal static LHandle PursuitLHandle;

        internal static void SafOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                Debug("Adding all suspect in the vehicle to a list");
                var pedsInVehicle = GetAllVehicleOccupants(_suspectVehicle);

                GameFiber.Wait(4500);

                var outcome = Rndm.Next(1, 101);
                switch (outcome)
                {
                    case > 50:
                        Debug("Starting all suspects outcome");
                        GameFiber.StartNew(() => AllSuspects(pedsInVehicle));
                        break;
                    case <= 50:
                        Debug("Starting driver only outcome");
                        GameFiber.StartNew(() => DriverOnly(pedsInVehicle));
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, nameof(SafOutcome));
            }
        }

        private static void AllSuspects(List<Ped> peds)
        {
            SetRelationshipGroups(_suspectRelateGroup);
            
            for (var i = 0; i < peds.Count; i++)
            {
                if (peds[i].IsAvailable())
                {
                    if (!peds[i].Inventory.HasLoadedWeapon)
                    {
                        var weapon = PistolList[Rndm.Next(PistolList.Length)];
                        Debug($"Giving Suspect #{i} weapon: {weapon}");
                        peds[i].Inventory.GiveNewWeapon(weapon, 500, true);
                    }

                    Debug($"Making Suspect #{i} shoot at Player");
                    NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(peds[i], MainPlayer, 20.0f);
                }
            }

            Debug("Waiting 4500ms");
            GameFiber.Wait(4500);
            if (!MainPlayer.IsAvailable()) return;
            PursuitLHandle = SetupPursuitWithList(true, peds);
        }

        private static void DriverOnly(List<Ped> peds)
        {
            if (!_suspect.Exists()) { CleanupEvent(); return; }

            var weapon = PistolList[Rndm.Next(PistolList.Length)];
            Debug("Setting up Suspect Weapon");
            
            if (!_suspect.Inventory.HasLoadedWeapon) { Debug("Giving Suspect Weapon"); _suspect.Inventory.GiveNewWeapon(weapon, 100, true); }
            Debug("Giving Suspect Tasks");
            NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(_suspect, MainPlayer, 20.0f);
            GameFiber.Wait(4500);
            
            if (!MainPlayer.IsAvailable()) return;
            PursuitLHandle = SetupPursuitWithList(true, peds);
        }
    }
}