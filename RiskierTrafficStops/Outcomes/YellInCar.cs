using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal static class YellInCar
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;

        internal static void YicEventHandler(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    CleanupEvent(_suspect, _suspectVehicle);
                    return;
                }

                _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                GameFiber.WaitWhile(() => _suspect.Exists() && _suspect.IsAnySpeechPlaying);
                if (_suspect.Exists())
                {
                    _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, "YellingCar.cs");
            }
        }
    }
}