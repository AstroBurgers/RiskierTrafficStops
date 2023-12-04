using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes
{
    internal static class Ramming
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void RammingOutcome(LHandle handle)
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
                
                if (!_suspect.IsAvailable()) { CleanupEvent();
                    return;
                }
                _suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
                GameFiber.Wait(3500);
                
                if (!_suspect.IsAvailable()) { CleanupEvent();
                    return;
                }
                _suspect.Tasks.Clear();
                if (Functions.GetCurrentPullover() == null) { GameFiberHandling.CleanupFibers(); return; }
                PursuitLHandle = SetupPursuitWithList(true, _suspectVehicle.Occupants);
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(RammingOutcome));
                GameFiberHandling.CleanupFibers();
            }
            
            GameFiberHandling.CleanupFibers();
            APIs.InvokeEvent(RTSEventType.End);
        }
    }
}