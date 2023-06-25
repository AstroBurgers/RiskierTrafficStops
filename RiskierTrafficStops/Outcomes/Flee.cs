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
                Debug("Setting up Suspect");
                Suspect = Functions.GetPulloverSuspect(handle);
                if (Suspect.Exists() && Suspect.IsInAnyVehicle(false))
                {
                    Debug("Setting up Suspect vehicle");
                    suspectVehicle = Suspect.CurrentVehicle;
                    Suspect.BlockPermanentEvents = true;
                    Debug("Setting up Suspect as Persistent and Blocking perm events");
                    Suspect.IsPersistent = true;
                    Debug("Setting up Suspect vehicle as Persistent");
                    suspectVehicle.IsPersistent = true;
                }

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