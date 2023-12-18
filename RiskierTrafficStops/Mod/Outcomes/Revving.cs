using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes
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
                APIs.InvokeEvent(RTSEventType.Start);
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }
                
                RevEngine(_suspect, _suspectVehicle, new[] { 2, 4 }, new[] { 2, 4 }, 2);

                var chance = Rndm.Next(1, 101);
                switch (chance)
                {
                    case <= 25:
                        Normal("Suspect chose not to run after revving");
                        break;
                    default:
                        if (_suspect.IsAvailable())
                        {
                            if (Functions.GetCurrentPullover() == null) { CleanupEvent(); return; }
                            PursuitLHandle = SetupPursuit(true, _suspect);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(RevvingOutcome));
                CleanupEvent();
            }
            
            GameFiberHandling.CleanupFibers();
            APIs.InvokeEvent(RTSEventType.End);
        }
    }
}