using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RiskierTrafficStops.API;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes
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
                APIs.InvokeEvent(RTSEventType.Start);
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                GameFiber.Wait(4500);

                var outcome = Rndm.Next(1, 101);
                switch (outcome)
                {
                    case > 50:
                        Debug("Starting all suspects outcome");
                        GameFiber.StartNew(() => AllSuspects(_suspectVehicle.Occupants));
                        break;
                    case <= 50:
                        Debug("Starting driver only outcome");
                        GameFiber.StartNew(() => DriverOnly());
                        break;
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(SafOutcome));
            }
            APIs.InvokeEvent(RTSEventType.End);
        }

        private static void AllSuspects(Ped[] peds)
        {
            SetRelationshipGroups(_suspectRelateGroup);
            
            for (var i = 0; i < peds.Length; i++)
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

        private static void DriverOnly()
        {
            if (!_suspect.IsAvailable()) { CleanupEvent(); return; }

            var weapon = PistolList[Rndm.Next(PistolList.Length)];
            Debug("Setting up Suspect Weapon");
            
            if (!_suspect.Inventory.HasLoadedWeapon) { Debug("Giving Suspect Weapon"); _suspect.Inventory.GiveNewWeapon(weapon, 100, true); }
            Debug("Giving Suspect Tasks");
            NativeFunction.Natives.TASK_VEHICLE_SHOOT_AT_PED(_suspect, MainPlayer, 20.0f);
            GameFiber.Wait(5000);
            
            if (!MainPlayer.IsAvailable()) return;
            PursuitLHandle = SetupPursuit(true, _suspect);
        }
    }
}