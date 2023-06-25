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
                Suspect = GetSuspectAndVehicle(handle).Item1;
                suspectVehicle = GetSuspectAndVehicle(handle).Item2;

                Debug("Getting all vehicle occupants");
                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);

                int Chance = rndm.Next(1, 101);
                if (Chance < 50)
                {
                    PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
                }

                else if (Chance > 50)
                {
                    foreach (Ped i in PedsInVehicle)
                    {
                        if (i.Exists())
                        {
                            i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                        }
                    }
                    if (Functions.IsPlayerPerformingPullover())
                    {
                        Functions.ForceEndCurrentPullover();
                    }
                    PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
                }
            }
            catch (Exception e)
            {
                Error(e, "Flee.cs");
            }
        }
    }
}