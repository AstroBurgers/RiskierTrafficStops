using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes
{
    internal static class ShootAndFlee
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void SafOutcome(LHandle handle)
        {
            try
            {
                APIs.InvokeEvent(RTSEventType.Start);
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                GameFiber.Wait(4500);

                var outcome = Rndm.Next(1, 101);
                switch (outcome)
                {
                    case > 50:
                        Normal("Starting all suspects outcome");
                        AllSuspects(_suspectVehicle.Occupants);
                        break;
                    case <= 50:
                        Normal("Starting driver only outcome");
                        DriverOnly();
                        break;
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(SafOutcome));
                CleanupEvent();
            }
            
            GameFiberHandling.CleanupFibers();
            APIs.InvokeEvent(RTSEventType.End);
        }

        private static void AllSuspects(Ped[] peds)
        {
            foreach (var i in peds)
            {
                if (!i.IsAvailable()) continue;
                if (!i.Inventory.HasLoadedWeapon)
                {
                    var weapon = PistolList[Rndm.Next(PistolList.Length)];
                    Normal($"Giving Suspect #{i} weapon: {weapon}");
                    i.Inventory.GiveNewWeapon(weapon, 500, true);
                }

                Normal($"Making Suspect #{i} shoot at Player");
                NativeFunction.Natives.x10AB107B887214D8(i, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
            }

            if (!MainPlayer.IsAvailable()) return;
            if (Functions.GetCurrentPullover() == null) { CleanupEvent(); return; }
            PursuitLHandle = SetupPursuitWithList(true, peds);
        }

        private static void DriverOnly()
        {
            if (!_suspect.IsAvailable()) return;


            Normal("Setting up Suspect Weapon");
            if (!_suspect.Inventory.HasLoadedWeapon)
            {
                Normal("Giving Suspect Weapon");
                var weapon = PistolList[Rndm.Next(PistolList.Length)];
                _suspect.Inventory.GiveNewWeapon(weapon, 100, true);
            }
            
            Normal("Giving Suspect Tasks");
            NativeFunction.Natives.x10AB107B887214D8(_suspect, MainPlayer, 20.0f); // TASK_VEHICLE_SHOOT_AT_PED
            
            if (!MainPlayer.IsAvailable()) return;
            if (Functions.GetCurrentPullover() == null) { CleanupEvent(); return; }
            PursuitLHandle = SetupPursuit(true, _suspect);
        }
    }
}