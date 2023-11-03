using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Ramming
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void RammingOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);

                Suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
                GameFiber.Wait(3500);
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