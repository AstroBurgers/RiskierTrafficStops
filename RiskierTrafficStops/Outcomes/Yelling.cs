using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal static class Yelling
    {
        private enum YellingScenarioOutcomes
        {
            GetBackInVehicle,
            ContinueYelling,
            PullOutKnife
        }

        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        private static RelationshipGroup _suspectRelationshipGroup = new("RTSYellingSuspects");
        private static YellingScenarioOutcomes _chosenOutcome;
        private static bool _isSuspectInVehicle;

        internal static void YellingOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                Debug("Making Suspect Leave Vehicle");
                _suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                Debug("Making Suspect Face Player");
                NativeFunction.Natives.x5AD23D40115353AC(_suspect, MainPlayer, -1);

                Debug("Making suspect Yell at Player");
                const int timesToSpeak = 2;

                for (var i = 0; i < timesToSpeak; i++)
                {
                    Debug($"Making Suspect Yell, time: {i}");
                    _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                    GameFiber.WaitWhile(() => _suspect.IsAvailable() && _suspect.IsAnySpeechPlaying);
                }

                Debug("Choosing outcome from possible Yelling outcomes");
                var scenarioList = (YellingScenarioOutcomes[])Enum.GetValues(typeof(YellingScenarioOutcomes));
                _chosenOutcome = scenarioList[Rndm.Next(scenarioList.Length)];
                Debug($"Chosen Outcome: {_chosenOutcome}");

                switch (_chosenOutcome)
                {
                    case YellingScenarioOutcomes.GetBackInVehicle:
                        if (_suspect.IsAvailable() && !Functions.IsPedArrested(_suspect)) //Double checking if suspect exists
                        {
                            _suspect.Tasks.EnterVehicle(_suspectVehicle, -1);
                        }
                        break;
                    case YellingScenarioOutcomes.PullOutKnife:
                        OutcomePullKnife();
                        break;
                    case YellingScenarioOutcomes.ContinueYelling:
                        GameFiber.StartNew(KeyPressed);
                        while (!_isSuspectInVehicle && _suspect.IsAvailable() && (!Functions.IsPedArrested(_suspect) || Functions.IsPedGettingArrested(_suspect)))
                        {
                            GameFiber.Yield();
                            _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                            GameFiber.WaitWhile(() => _suspect.IsAvailable() && _suspect.IsAnySpeechPlaying);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, nameof(YellingOutcome));
            }
        }

        private static void KeyPressed()
        {
            Game.DisplayHelp($"~BLIP_INFO_ICON~ Press {Settings.GetBackInKey.GetInstructionalId()} to have the suspect get back in their vehicle", 10000);
            while (_suspect.IsAvailable() && !_isSuspectInVehicle)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.GetBackInKey))
                {
                    _isSuspectInVehicle = true;
                    _suspect.Tasks.EnterVehicle(_suspectVehicle, -1);
                    break;
                }
            }
        }

        private static void OutcomePullKnife()
        {
            if (!_suspect.IsAvailable() || Functions.IsPedArrested(_suspect) ||
                Functions.IsPedGettingArrested(_suspect)) return;
            
            _suspect.Inventory.GiveNewWeapon(MeleeWeapons[Rndm.Next(MeleeWeapons.Length)], -1, true);

            SetRelationshipGroups(_suspectRelationshipGroup);

            Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
            _suspect.BlockPermanentEvents = true;
            _suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
        }
    }
}