using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Outcomes;
using RiskierTrafficStops.Systems;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops
{
    public class Main : Plugin
    {
        internal enum Scenarios
        {
            GetOutOfCarAndYell,
            GetOutAndShoot,
            FleeFromTrafficStop,
            YellInCar,
            RevEngine,
            RamIntoPlayerVehicle,
            ShootAndFlee,
        }

        internal static bool HasEventHappend = false;
        internal static Scenarios chosenOutcome;
        internal static int chosenChance;
        internal static bool _onDuty;
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }
        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            _onDuty = onDuty;
            if (onDuty)
            {
                // Setting up INI And checking for updates
                Settings.INIFileSetup();
                ConfigMenu.CreateMenu();
                Game.AddConsoleCommands();
                VersionChecker.CheckForUpdates();
                // Displaying startup Notification
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Watch you back out there officer!");
                Debug("Loaded succesfully");
                //Displaying Autolog Notification
                if (Settings.autoLogEnabled) { Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~Auto Logging Status", "Auto Logging is ~g~Enabled"); }
                if (!Settings.autoLogEnabled) { Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~Auto Logging Status", "Auto Logging is ~r~Disabled"); }
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
        }

        private static void Events_OnPulloverStarted(LHandle handle)
        {
            chosenChance = rndm.Next(1, 101);
            chosenOutcome = Settings.enabledScenarios[rndm.Next(Settings.enabledScenarios.Count)];
            if (chosenChance < Settings.Chance)
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
            if (!HasEventHappend) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
        {
            if (!HasEventHappend) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        public override void Finally()
        {
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Hope you had a good patrol!");
            Debug("Unloaded succesfully");
            Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
            Events.OnPulloverDriverStopped -= Events_OnPulloverDriverStopped;
            Events.OnPulloverStarted -= Events_OnPulloverStarted;
            Events.OnPulloverEnded -= Events_OnPulloverEnded;
        }

        internal static void ChooseEvent(LHandle handle)
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
                    Yell.YellOutcome(handle);
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
                    Rev.ROutcome(handle);
                    break;
                case Scenarios.RamIntoPlayerVehicle:
                    RamIntoYou.RIYOutcome(handle);
                    break;
                case Scenarios.ShootAndFlee:
                    ShootAndFlee.SAFOutcome(handle);
                    break;
                default:
                    Debug("No outcomes Enabled (or some other shit)");
                    break;
            }
        }
    }
}