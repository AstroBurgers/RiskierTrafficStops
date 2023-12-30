using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.Extensions;

namespace RiskierTrafficStops.Mod.Outcomes
{
    internal static class YellInCar
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;

        internal static void YicEventHandler(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }
                APIs.InvokeEvent(RTSEventType.Start);
                
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
                CleanupEvent();
            }
            
            GameFiberHandling.CleanupFibers();
            APIs.InvokeEvent(RTSEventType.End);
        }
    }
}