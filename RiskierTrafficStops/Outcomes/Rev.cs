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
                Debug("Setting up Suspect and Suspect Vehicle");
                Suspect = Functions.GetPulloverSuspect(handle);
                if (Suspect.Exists())
                {
                    suspectVehicle = Suspect.CurrentVehicle;
                    Suspect.BlockPermanentEvents = true;
                    suspectVehicle.IsPersistent = true;
                }


                RevEngine(Suspect, suspectVehicle, new int[] { 2, 6 }, new int[] { 2, 6 }, 3);

                int Chance = rndm.Next(1, 101);

                if (Chance >= 25)
                {
                    Functions.ForceEndCurrentPullover();
                    PursuitLHandle = SetupPursuit(true, Suspect);
                }
            }
            catch (Exception e)
            {
                Error(e, "Rev.cs");
            }
        }
    }
}