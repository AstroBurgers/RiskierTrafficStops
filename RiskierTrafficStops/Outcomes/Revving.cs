using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal static class Revving
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void RevvingOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    CleanupEvent(_suspect, _suspectVehicle);
                    return;
                }

                RevEngine(_suspect, _suspectVehicle, new[] { 2, 4 }, new[] { 2, 4 }, 2);

                var chance = Rndm.Next(1, 101);

                if (chance < 25) return;
                if (_suspect != null) PursuitLHandle = SetupPursuit(true, _suspect);
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, nameof(RevvingOutcome));
            }
        }
    }
}