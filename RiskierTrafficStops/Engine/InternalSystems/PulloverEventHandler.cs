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
        private static int _chosenChance;
        private static Scenario _chosenOutcome;
        internal static bool HasEventHappened;
        private static Scenario? _lastOutcome;
        
        
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

                _chosenChance = Rndm.Next(1, 101);
                _chosenOutcome = ChooseOutcome();

                Action<LHandle> chosenOutcome = null;
                
                if (_chosenChance <= Settings.Chance || HasEventHappened) return;
                switch (_chosenOutcome)
                {
                    case Scenario.FleeFromTrafficStop:
                        Normal($"Chosen Scenario: {_chosenOutcome}");
                        GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                        chosenOutcome = Flee.FleeOutcome;
                        HasEventHappened = true;

                        _lastOutcome = Scenario.FleeFromTrafficStop;
                        break;
                    case Scenario.GetOutAndShoot:
                        Normal($"Chosen Scenario: {_chosenOutcome}");
                        GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                        chosenOutcome = GetOutAndShoot.GoasOutcome;
                        HasEventHappened = true;

                        _lastOutcome = Scenario.GetOutAndShoot;
                        break;
                    case Scenario.ShootAndFlee:
                        Normal($"Chosen Scenario: {_chosenOutcome}");
                        GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());

                        chosenOutcome = ShootAndFlee.SafOutcome;
                        HasEventHappened = true;
                        _lastOutcome = Scenario.ShootAndFlee;
                        break;
                }
                
                if (Functions.IsPlayerPerformingPullover()) {Normal("Player is no longer performing pullover, ending RTS events");
                    GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() =>
                    {
                        if (chosenOutcome != null) chosenOutcome(handle);
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
                _chosenChance = Rndm.Next(1, 101);
                Normal($"Chance: {_chosenChance}");
                Normal($"HasEventHappened: {HasEventHappened}");
                Normal($"DisableRTSForCurrentStop: {API.APIs.DisableRTSForCurrentStop}");
                
                if (HasEventHappened || !(_chosenChance <= Settings.Chance)) { return; }

                HasEventHappened = true;
                Normal("Choosing Scenario");

                _chosenOutcome = Settings.EnabledScenarios[Rndm.Next(Settings.EnabledScenarios.Count)];
                Normal($"Chosen Outcome: {_chosenOutcome}");

                switch (_chosenOutcome)
                {
                    case Scenario.GetOutOfCarAndYell:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Yelling.YellingOutcome(handle)));
                        _lastOutcome = Scenario.GetOutOfCarAndYell;
                        break;

                    case Scenario.GetOutAndShoot:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetOutAndShoot.GoasOutcome(handle)));
                        _lastOutcome = Scenario.GetOutAndShoot;
                        break;

                    case Scenario.FleeFromTrafficStop:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Flee.FleeOutcome(handle)));
                        _lastOutcome = Scenario.FleeFromTrafficStop;
                        break;

                    case Scenario.YellInCar:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => YellInCar.YicEventHandler(handle)));
                        _lastOutcome = Scenario.YellInCar;
                        break;

                    case Scenario.RevEngine:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Revving.RevvingOutcome(handle)));
                        _lastOutcome = Scenario.RevEngine;
                        break;

                    case Scenario.RamIntoPlayerVehicle:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Ramming.RammingOutcome(handle)));
                        _lastOutcome = Scenario.RamIntoPlayerVehicle;
                        break;

                    case Scenario.ShootAndFlee:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => ShootAndFlee.SafOutcome(handle)));
                        _lastOutcome = Scenario.ShootAndFlee;
                        break;

                    case Scenario.Spit:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Spitting.SpittingOutcome(handle)));
                        _lastOutcome = Scenario.Spit;
                        break;

                    default:
                        Normal("No outcomes Enabled (or some other shit)");
                        break;
                }
            }
            catch (System.Exception e)
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
    }
}