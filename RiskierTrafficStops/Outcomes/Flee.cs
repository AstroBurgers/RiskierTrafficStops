using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal static class Flee
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void FleeOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                Debug("Getting all vehicle occupants");
                var pedsInVehicle = _suspectVehicle.Occupants;

                var chance = Rndm.Next(1, 101);
                switch (chance)
                {
                    case <= 33:
                        Debug("Starting pursuit");
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    case <= 66:
                        Debug("Making suspect do burnout");
                        _suspect.Tasks.PerformDrivingManeuver(_suspectVehicle, VehicleManeuver.BurnOut, 2000).WaitForCompletion(2000);
                        Debug("Clearing suspect tasks");
                        _suspect.Tasks.PerformDrivingManeuver(_suspectVehicle, VehicleManeuver.GoForwardStraight, 750).WaitForCompletion(750);
                        Debug("Starting pursuit");
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    case <= 100:
                    {
                        for (var i = pedsInVehicle.Length - 1; i >= 0; i--)
                        {
                            if (pedsInVehicle[i].IsAvailable())
                            {
                                pedsInVehicle[i].Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                            }
                        }
                        PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, nameof(FleeOutcome));
            }
        }
    }
}