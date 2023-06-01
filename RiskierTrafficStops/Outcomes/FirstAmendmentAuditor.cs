using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Helper;
using static RiskierTrafficStops.Logger;
using System;

namespace RiskierTrafficStops.Outcomes
{
    internal class FirstAmendmentAuditor
    {
        internal static Ped Suspect;
        internal static LHandle PursuitLHandle;
        internal static Random rndm = new Random();

        internal static void FAAOutcome(LHandle handle)
        {
            Ped[] pedList = MainPlayer.GetNearbyPeds(16);
            foreach (Ped i in pedList)
            {
                if (i && !Functions.IsPedACop(i) && !i.IsInAnyVehicle(false) && !i.IsPlayer)
                {
                    Suspect = i;
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                }
            }
        }
    }
}
