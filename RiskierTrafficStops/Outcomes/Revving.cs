using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Threading;
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
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                RevEngine(_suspect, _suspectVehicle, new[] { 2, 4 }, new[] { 2, 4 }, 2);

                var chance = Rndm.Next(1, 101);
                switch (chance)
                {
                    case <= 25:
                        Debug("Suspect chose not to run after revving");
                        break;
                    default:
                        if (_suspect.IsAvailable())
                        {
                            PursuitLHandle = SetupPursuit(true, _suspect);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(RevvingOutcome));
            }
        }
    }
}