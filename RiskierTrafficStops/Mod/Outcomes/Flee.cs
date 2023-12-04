using System;
using System.Collections.Generic;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.API.APIs;
using static RiskierTrafficStops.Engine.Helpers.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes
{
    internal static class Flee
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        internal static LHandle PursuitLHandle;

        private enum FleeOutcomes
        {
            Flee,
            BurnOut,
            LeaveVehicle,
        }
        
        internal static void FleeOutcome(LHandle handle)
        {
            try
            {
                InvokeEvent(RTSEventType.Start);
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }
                
                Debug("Getting all vehicle occupants");
                var pedsInVehicle = _suspectVehicle.Occupants;

                List<FleeOutcomes> allOutcomes = new()
                    { FleeOutcomes.Flee, FleeOutcomes.BurnOut, FleeOutcomes.LeaveVehicle };
        
                FleeOutcomes chosenFleeOutcome = allOutcomes[Rndm.Next(allOutcomes.Count)];
                
                switch (chosenFleeOutcome)
                {
                    case FleeOutcomes.Flee:
                        Debug("Starting pursuit");
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    case FleeOutcomes.BurnOut:
                        Debug("Making suspect do burnout");
                        _suspect.Tasks.PerformDrivingManeuver(_suspectVehicle, VehicleManeuver.BurnOut, 2000).WaitForCompletion(2000);
                        Debug("Clearing suspect tasks");
                        _suspect.Tasks.PerformDrivingManeuver(_suspectVehicle, VehicleManeuver.GoForwardStraight, 750).WaitForCompletion(750);
                        Debug("Starting pursuit");
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    case FleeOutcomes.LeaveVehicle:
                        foreach (var i in pedsInVehicle)
                        {
                            if (i.IsAvailable())
                            {
                                i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                            }
                        }
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(FleeOutcome));
                GameFiberHandling.CleanupFibers();
            }
            
            InvokeEvent(RTSEventType.End);
        }
    }
}