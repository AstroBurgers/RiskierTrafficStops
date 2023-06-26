using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Rev
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static LHandle PursuitLHandle;

        internal static void ROutcome(LHandle handle)
        {
            try
            {
                Suspect = GetSuspectAndVehicle(handle).Item1;
                suspectVehicle = GetSuspectAndVehicle(handle).Item2;


                RevEngine(Suspect, suspectVehicle, new int[] { 2, 4}, new int[] { 2, 4 }, 2);

                int Chance = rndm.Next(1, 101);

                if (Chance >= 25)
                {
                    Functions.ForceEndCurrentPullover();
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