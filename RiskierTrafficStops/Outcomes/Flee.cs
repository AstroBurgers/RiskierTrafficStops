using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Flee
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void FleeOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                Debug("Getting all vehicle occupants");
                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);

                int Chance = rndm.Next(1, 101);
                if (Chance <= 50)
                {
                    Debug("Making suspect do burnout");
                    Suspect.Tasks.PerformDrivingManeuver(suspectVehicle, VehicleManeuver.BurnOut, 2000).WaitForCompletion(2000);
                    Debug("Clearing suspect tasks");
                    Suspect.Tasks.PerformDrivingManeuver(suspectVehicle, VehicleManeuver.GoForwardStraight, 750).WaitForCompletion(750);
                    Debug("Starting pursuit");
                    PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
                }

                else if (Chance >= 50)
                {
                    for (int i = 0; i < PedsInVehicle.Count; i++)
                    {
                        if (!PedsInVehicle[i].Exists()) { CleanupEvent(PedsInVehicle[i]); continue; }
                        PedsInVehicle[i].Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    }
                    PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "Flee.cs");
            }
        }
    }
}