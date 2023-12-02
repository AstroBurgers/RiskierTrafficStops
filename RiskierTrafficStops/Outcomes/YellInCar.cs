using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Threading;
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
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                GameFiber.WaitWhile(() => _suspect.IsAvailable() && _suspect.IsAnySpeechPlaying);
                if (_suspect.IsAvailable())
                {
                    _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(YicEventHandler));
            }
        }
    }
}