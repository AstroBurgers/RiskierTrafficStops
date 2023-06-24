using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Deployment.Internal;
using System.Runtime.Serialization;

namespace RiskierTrafficStops.Outcomes
{
    internal class YellInCar
    {

        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;

        internal static void YICEventHandler(LHandle handle)
        {
            Debug("Setting up Suspect and Suspect Vehicle");
            Suspect = Functions.GetPulloverSuspect(handle);
            suspectVehicle = Suspect.CurrentVehicle;
            Suspect.BlockPermanentEvents = true;
            suspectVehicle.IsPersistent = true;

            Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
            GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
            Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
            GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
        }
    }
}