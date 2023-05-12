using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Helper;
using static RiskierTrafficStops.Logger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Deployment.Internal;
using System.Runtime.Serialization;

namespace RiskierTrafficStops.Outcomes
{
    internal class Flee
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static LHandle PursuitLHandle;
        internal static Random rndm = new Random();

        internal static void FleeOutcome(LHandle handle)
        {
            Normal("Yell.cs", "Setting up Suspect and Suspect Vehicle");
            Suspect = Functions.GetPulloverSuspect(handle);
            suspectVehicle = Suspect.CurrentVehicle;
            Suspect.BlockPermanentEvents = true;
            suspectVehicle.IsPersistent = true;
            int Chance = rndm.Next(1, 101);
            if (Chance < 50)
            {
                SetupPursuit(true, Suspect);
            }

            else if (Chance > 50)
            {
                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                SetupPursuit(true, Suspect);
            }
        }
    }
}