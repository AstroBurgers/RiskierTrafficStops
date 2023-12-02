using System.Security.Cryptography;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Outcomes;
using RiskierTrafficStops.Systems;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops
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
                            Flee.FleeOutcome(handle);
                            HasEventHappened = true;
                            break;
                        case Scenarios.GetOutAndShoot:
                            Debug($"Chosen Scenario: {_chosenOutcome}");
                            GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                            if (!Functions.IsPlayerPerformingPullover()) { Debug("Player is no longer performing pullover, ending RTS events"); break; };
                            GetOutAndShoot.GoasOutcome(handle);
                            HasEventHappened = true;
                            break;
                        case Scenarios.ShootAndFlee:
                            Debug($"Chosen Scenario: {_chosenOutcome}");
                            GameFiber.WaitWhile(() => !MainPlayer.CurrentVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());
                            if (!Functions.IsPlayerPerformingPullover()) { Debug("Player is no longer performing pullover, ending RTS events"); break; };
                            ShootAndFlee.SafOutcome(handle);
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
                        Yelling.YellingOutcome(handle);
                        break;

                    case Scenarios.GetOutAndShoot:
                        GetOutAndShoot.GoasOutcome(handle);
                        break;

                    case Scenarios.FleeFromTrafficStop:
                        Flee.FleeOutcome(handle);
                        break;

                    case Scenarios.YellInCar:
                        YellInCar.YicEventHandler(handle);
                        break;

                    case Scenarios.RevEngine:
                        Revving.RevvingOutcome(handle);
                        break;

                    case Scenarios.RamIntoPlayerVehicle:
                        Ramming.RammingOutcome(handle);
                        break;

                    case Scenarios.ShootAndFlee:
                        ShootAndFlee.SafOutcome(handle);
                        break;

                    case Scenarios.Spit:
                        Spitting.SpittingOutcome(handle);
                        break;

                    default:
                        Debug("No outcomes Enabled (or some other shit)");
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (System.Exception e)
            {
                Error(e, "PulloverEvents.cs: ChooseEvent()");
            }
        }
    }
}