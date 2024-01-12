using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API.ExternalAPIs;
using RiskierTrafficStops.Mod.Outcomes;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using System.Security.Cryptography;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    internal enum Scenario //Enum is outside class so that it can be referenced anywhere without having to reference the class
    {
        GetOutOfCarAndYell,
        GetOutAndShoot,
        FleeFromTrafficStop,
        YellInCar,
        RevEngine,
        RamIntoPlayerVehicle,
        ShootAndFlee,
        Spit,
    }

    internal static class PulloverEventHandler
    {
        private static Scenario _chosenOutcome;
        internal static bool HasEventHappened;
        private static Scenario? _lastOutcome;
        private static Action<LHandle> _chosenOutcomeAction;
        private static RNGCryptoServiceProvider _outcomeRng = new RNGCryptoServiceProvider();
        
        internal static void SubscribeToEvents()
        {
            //Subscribing to events
            Normal("Subscribing to: OnPulloverOfficerApproachDriver");
            Events.OnPulloverOfficerApproachDriver += Events_OnPulloverOfficerApproachDriver;
            //\\
            Normal("Subscribing to: OnPulloverDriverStopped");
            Events.OnPulloverDriverStopped += Events_OnPulloverDriverStopped;
            //\\
            Normal("Subscribing to: OnPulloverStarted");
            Events.OnPulloverStarted += Events_OnPulloverStarted;
            //\\
            Normal("Subscribing to: OnPulloverEnded");
            Events.OnPulloverEnded += Events_OnPulloverEnded;
        }

        internal static void UnsubscribeFromEvents()
        {
            Normal("Unsubscribing from events...");
            Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
            Events.OnPulloverDriverStopped -= Events_OnPulloverDriverStopped;
            Events.OnPulloverStarted -= Events_OnPulloverStarted;
            Events.OnPulloverEnded -= Events_OnPulloverEnded;
        }

        private static void Events_OnPulloverStarted(LHandle handle)
        {
            GameFiber.StartNew(() =>
            {
                if (!IaeFunctions.IaeCompatibilityCheck(handle) || Functions.IsCalloutRunning() || API.APIs.DisableRTSForCurrentStop) return;

                //_chosenChance = Rndm.Next(1, 101);
                _chosenOutcome = ChooseOutcome();
                Normal($"Chosen Scenario: {_chosenOutcome}");
                
                _chosenOutcomeAction = null;
                
                if (!ShouldEventHappen()) return;
                
                if (HasEventHappened) return;
                HasEventHappened = true;
                _lastOutcome = _chosenOutcome;
                switch (_chosenOutcome)
                {
                    case Scenario.FleeFromTrafficStop:
                        GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                        Flee fleeOutcome = new Flee(handle);
                        break;
                    
                    case Scenario.GetOutAndShoot:
                        GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                        GetOutAndShoot getOutAndShootOutcome = new GetOutAndShoot("RTSGetOutAndShootSuspects", handle);
                        break;
                    
                    case Scenario.ShootAndFlee:
                        GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                        _chosenOutcomeAction = ShootAndFlee.SafOutcome;
                        break;
                }
                
                if (Functions.IsPlayerPerformingPullover()) {Normal("Player is no longer performing pullover, ending RTS events");
                    GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                    {
                        if (_chosenOutcomeAction != null) _chosenOutcomeAction(handle);
                    }));
                }
            });
        }

        private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
        {
            HasEventHappened = false;
            API.APIs.DisableRTSForCurrentStop = false;
        }

        private static void Events_OnPulloverDriverStopped(LHandle handle)
        {
            if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !API.APIs.DisableRTSForCurrentStop) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
        {
            if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !API.APIs.DisableRTSForCurrentStop) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        //For all events after the vehicle has stopped
        private static void ChooseEvent(LHandle handle)
        {
            try
            {
                if (ShouldEventHappen())
                {
                    Normal($"HasEventHappened: {HasEventHappened}");
                    Normal($"DisableRTSForCurrentStop: {API.APIs.DisableRTSForCurrentStop}");

                    if (HasEventHappened)
                    {
                        return;
                    }
                    
                    Normal("Choosing Scenario");
                    _chosenOutcome = Settings.EnabledScenarios[Rndm.Next(Settings.EnabledScenarios.Count)];
                    _lastOutcome = _chosenOutcome;
                    Normal($"Chosen Outcome: {_chosenOutcome}");

                    switch (_chosenOutcome)
                    {
                        case Scenario.GetOutOfCarAndYell:
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                                Yelling.YellingOutcome(handle)));
                            break;

                        case Scenario.GetOutAndShoot:
                            GetOutAndShoot getOutAndShoot = new GetOutAndShoot("RTSGetOutAndShootSuspects", handle);
                            break;

                        case Scenario.FleeFromTrafficStop:
                            Flee fleeOutcome = new Flee(handle);
                            break;

                        case Scenario.YellInCar:
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                                YellInCar.YicEventHandler(handle)));
                            break;

                        case Scenario.RevEngine:
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                                Revving.RevvingOutcome(handle)));
                            break;

                        case Scenario.RamIntoPlayerVehicle:
                            Ramming rammingOutcome = new Ramming(handle);
                            break;

                        case Scenario.ShootAndFlee:
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                                ShootAndFlee.SafOutcome(handle)));
                            break;

                        case Scenario.Spit:
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                                Spitting.SpittingOutcome(handle)));
                            break;

                        default:
                            Normal("No outcomes Enabled (or some other shit)");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, "PulloverEvents.cs: ChooseEvent()");
                GameFiberHandling.CleanupFibers();
            }
        }

        private static Scenario ChooseOutcome()
        {
            if (_lastOutcome != null && Settings.EnabledScenarios.Count > 1)
            {
                List<Scenario> possibleOutcome = Settings.EnabledScenarios.Where(i => i != _lastOutcome).ToList();
                return possibleOutcome[Rndm.Next(possibleOutcome.Count)];
            }
            else
            {
                return Settings.EnabledScenarios[Rndm.Next(Settings.EnabledScenarios.Count)];
            }
        }
        
        private static bool ShouldEventHappen()
        {
            byte[] randomBytes = new byte[8]; // Using 8 bytes for more randomization ig
            _outcomeRng.GetBytes(randomBytes);
 
            long randomNumber = BitConverter.ToInt64(randomBytes, 0) & 0x7FFFFFFF; // Convert to positive integer

            var convertedChance = randomNumber % 100;
            Normal("Chance: " + convertedChance);
            
            return convertedChance < Settings.Chance;
        }
    }
}