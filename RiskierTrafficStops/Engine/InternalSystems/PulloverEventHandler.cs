using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API.ExternalAPIs;
using RiskierTrafficStops.Mod.Outcomes;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    internal enum Scenarios //Enum is outside class so that it can be referenced anywhere without having to reference the class
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
        private static Scenarios _chosenOutcome;
        internal static bool HasEventHappened;

        internal static void SubscribeToEvents()
        {
            //Subscribing to events
            Debug("Subscribing to: OnPulloverOfficerApproachDriver");
            Events.OnPulloverOfficerApproachDriver += Events_OnPulloverOfficerApproachDriver;
            //\\
            Debug("Subscribing to: OnPulloverDriverStopped");
            Events.OnPulloverDriverStopped += Events_OnPulloverDriverStopped;
            //\\
            Debug("Subscribing to: OnPulloverStarted");
            Events.OnPulloverStarted += Events_OnPulloverStarted;
            //\\
            Debug("Subscribing to: OnPulloverEnded");
            Events.OnPulloverEnded += Events_OnPulloverEnded;
        }

        internal static void UnsubscribeToEvents()
        {
            Debug("Unsubscribing from events...");
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
                _chosenOutcome = Settings.EnabledScenarios[Rndm.Next(Settings.EnabledScenarios.Count)];
                if (_chosenChance <= Settings.Chance)
                {
                    switch (_chosenOutcome)
                    {
                        case Scenarios.FleeFromTrafficStop:
                            Debug($"Chosen Scenario: {_chosenOutcome}");
                            GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                            if (!Functions.IsPlayerPerformingPullover()) { Debug("Player is no longer performing pullover, ending RTS events"); break; };
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Flee.FleeOutcome(handle)));
                            HasEventHappened = true;
                            break;
                        case Scenarios.GetOutAndShoot:
                            Debug($"Chosen Scenario: {_chosenOutcome}");
                            GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                            if (!Functions.IsPlayerPerformingPullover()) { Debug("Player is no longer performing pullover, ending RTS events"); break; };
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetOutAndShoot.GoasOutcome(handle)));
                            HasEventHappened = true;
                            break;
                        case Scenarios.ShootAndFlee:
                            Debug($"Chosen Scenario: {_chosenOutcome}");
                            GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                            if (!Functions.IsPlayerPerformingPullover()) { Debug("Player is no longer performing pullover, ending RTS events"); break; };
                            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => ShootAndFlee.SafOutcome(handle)));
                            HasEventHappened = true;
                            break;
                    }
                }
            });
        }

        private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
        {
            HasEventHappened = false;
            API.APIs.DisableRTSForCurrentStop = false;
            GameFiberHandling.CleanupFibers();
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
                Debug($"Chance: {_chosenChance}");
                Debug($"HasEventHappened: {HasEventHappened}");
                Debug($"DisableRTSForCurrentStop: {API.APIs.DisableRTSForCurrentStop}");
                
                if (HasEventHappened || !(_chosenChance <= Settings.Chance)) { return; }

                HasEventHappened = true;
                Debug("Choosing Scenario");

                _chosenOutcome = Settings.EnabledScenarios[Rndm.Next(Settings.EnabledScenarios.Count)];
                Debug($"Chosen Outcome: {_chosenOutcome}");

                switch (_chosenOutcome)
                {
                    case Scenarios.GetOutOfCarAndYell:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Yelling.YellingOutcome(handle)));
                        break;

                    case Scenarios.GetOutAndShoot:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetOutAndShoot.GoasOutcome(handle)));
                        break;

                    case Scenarios.FleeFromTrafficStop:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Flee.FleeOutcome(handle)));
                        break;

                    case Scenarios.YellInCar:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => YellInCar.YicEventHandler(handle)));
                        break;

                    case Scenarios.RevEngine:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Revving.RevvingOutcome(handle)));
                        break;

                    case Scenarios.RamIntoPlayerVehicle:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Ramming.RammingOutcome(handle)));
                        break;

                    case Scenarios.ShootAndFlee:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => ShootAndFlee.SafOutcome(handle)));
                        break;

                    case Scenarios.Spit:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => Spitting.SpittingOutcome(handle)));
                        break;

                    default:
                        Debug("No outcomes Enabled (or some other shit)");
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
    }
}