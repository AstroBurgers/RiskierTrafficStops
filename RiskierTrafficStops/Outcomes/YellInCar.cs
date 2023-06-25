using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class YellInCar
    {

        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;

        internal static void YICEventHandler(LHandle handle)
        {
            try
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
            catch (Exception e)
            {
                Error(e, "YellinCar.cs");
            }
        }
    }
}