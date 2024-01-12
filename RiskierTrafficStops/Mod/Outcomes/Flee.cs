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
using static RiskierTrafficStops.Engine.Helpers.Extensions;

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
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    
                    return;
                }
                InvokeEvent(RTSEventType.Start);
                
                Normal("Getting all vehicle occupants");
                var pedsInVehicle = _suspectVehicle.Occupants;

                List<FleeOutcomes> allOutcomes = new()
                    { FleeOutcomes.Flee, FleeOutcomes.BurnOut, FleeOutcomes.LeaveVehicle };
        
                FleeOutcomes chosenFleeOutcome = allOutcomes[Rndm.Next(allOutcomes.Count)];
                
                switch (chosenFleeOutcome)
                {
                    case FleeOutcomes.Flee:
                        Normal("Starting pursuit");
                        
                        if (Functions.GetCurrentPullover() == null) { CleanupEvent(); return; }
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    case FleeOutcomes.BurnOut:
                        Normal("Making suspect do burnout");
                        _suspect.Tasks.PerformDrivingManeuver(_suspectVehicle, VehicleManeuver.BurnOut, 2000).WaitForCompletion(2000);
                        Normal("Clearing suspect tasks");
                        _suspect.Tasks.PerformDrivingManeuver(_suspectVehicle, VehicleManeuver.GoForwardStraight, 750).WaitForCompletion(750);
                        Normal("Starting pursuit");
                        
                        if (Functions.GetCurrentPullover() == null) { CleanupEvent(); return; }
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
                        if (Functions.GetCurrentPullover() == null) { return; }
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
            
            GameFiberHandling.CleanupFibers();
            InvokeEvent(RTSEventType.End);
        }
    }
}