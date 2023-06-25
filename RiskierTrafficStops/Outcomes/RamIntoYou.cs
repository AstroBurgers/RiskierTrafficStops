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
                Debug("Setting up Suspect and Suspect Vehicle");
                Suspect = Functions.GetPulloverSuspect(handle);
                suspectVehicle = Suspect.CurrentVehicle;
                Suspect.BlockPermanentEvents = true;
                Suspect.IsPersistent = true;
                suspectVehicle.IsPersistent = true;

                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);

                Suspect.Tasks.DriveToPosition(MainPlayer.LastVehicle.Position, 100f, VehicleDrivingFlags.Reverse, 0.1f);
                GameFiber.Wait(6500);
                Suspect.Tasks.Clear();
                PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
            }
            catch (Exception e)
            {
                Error(e, "RamIntoYou.cs");
            }
        }
    }
}