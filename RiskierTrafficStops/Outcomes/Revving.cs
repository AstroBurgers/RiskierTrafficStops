using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Revving
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void RevvingOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                RevEngine(Suspect, suspectVehicle, new int[] { 2, 4}, new int[] { 2, 4 }, 2);

                int Chance = rndm.Next(1, 101);

                if (Chance >= 25)
                {
                    PursuitLHandle = SetupPursuit(true, Suspect);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "Rev.cs");
            }
        }
    }
}