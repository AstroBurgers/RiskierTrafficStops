using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;
using System.Linq;
using System.Threading;
using RiskierTrafficStops.API;

namespace RiskierTrafficStops.Outcomes
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
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
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
                PursuitLHandle = SetupPursuitWithList(true, _suspectVehicle.Occupants);
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(RammingOutcome));
            }
            
            APIs.InvokeEvent(RTSEventType.End);
        }
    }
}