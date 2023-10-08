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
    internal class PulloverEventHandler
    {
        internal static int chosenChance;
        internal static Scenarios chosenOutcome;
        internal static bool HasEventHappend;
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
            if (!IAEFunctions.IAECompatibilityCheck(handle)) { return; };

            chosenChance = rndm.Next(1, 101);
            chosenOutcome = Settings.enabledScenarios[rndm.Next(Settings.enabledScenarios.Count)];
            if (chosenChance <= Settings.Chance)
            {
                switch (chosenOutcome)
                {
                    case Scenarios.FleeFromTrafficStop:
                        Debug($"Chosen Scenario: {chosenOutcome}");
                        GameFiber.WaitUntil(() => MainPlayer.CurrentVehicle.IsSirenOn);
                        Flee.FleeOutcome(handle);
                        HasEventHappend = true;
                        break;
                    case Scenarios.ShootAndFlee:
                        Debug($"Chosen Scenario: {chosenOutcome}");
                        GameFiber.WaitUntil(() => MainPlayer.CurrentVehicle.IsSirenOn);
                        ShootAndFlee.SAFOutcome(handle);
                        HasEventHappend = true;
                        break;
                }
            }
        }

        private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
        {
            HasEventHappend = false;
        }

        private static void Events_OnPulloverDriverStopped(LHandle handle)
        {
            if (!HasEventHappend && IAEFunctions.IAECompatibilityCheck(handle)) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
        {
            if (!HasEventHappend && IAEFunctions.IAECompatibilityCheck(handle)) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        //For all events after the vehicle has stopped
        internal static void ChooseEvent(LHandle handle)
        {
            try
            {
                chosenChance = rndm.Next(1, 101);
                Debug($"Chance: {chosenChance}");
                Debug($"HasEventHappend: {HasEventHappend}");

                if (HasEventHappend) { return; }
                if (!(chosenChance <= Settings.Chance)) { return; }

                HasEventHappend = true;
                Debug("Choosing Scenario");

                chosenOutcome = Settings.enabledScenarios[rndm.Next(Settings.enabledScenarios.Count)];
                Debug($"Chosen Outcome: {chosenOutcome}");

                switch (chosenOutcome)
                {
                    case Scenarios.GetOutOfCarAndYell:
                        Yelling.YellingOutcome(handle);
                        break;
                    case Scenarios.GetOutAndShoot:
                        GetOutAndShoot.GOASOutcome(handle);
                        break;
                    case Scenarios.FleeFromTrafficStop:
                        Flee.FleeOutcome(handle);
                        break;
                    case Scenarios.YellInCar:
                        YellInCar.YICEventHandler(handle);
                        break;
                    case Scenarios.RevEngine:
                        Revving.RevvingOutcome(handle);
                        break;
                    case Scenarios.RamIntoPlayerVehicle:
                        Ramming.RammingOutcome(handle);
                        break;
                    case Scenarios.ShootAndFlee:
                        ShootAndFlee.SAFOutcome(handle);
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
