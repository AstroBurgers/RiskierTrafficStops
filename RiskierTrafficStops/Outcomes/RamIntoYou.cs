using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class RamIntoYou
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void RIYOutcome(LHandle handle)
        {
            try
            {
                Suspect = GetSuspectAndVehicle(handle).Item1;
                suspectVehicle = GetSuspectAndVehicle(handle).Item2;

                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);

                Suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
                GameFiber.Wait(6500);
                Suspect.Tasks.Clear();
                PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "RamIntoYou.cs");
            }
        }
    }
}