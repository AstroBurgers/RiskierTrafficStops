using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Windows.Forms;
using static RiskierTrafficStops.Systems.Helper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using RiskierTrafficStops.Outcomes;
using static RiskierTrafficStops.Systems.Logger;
using System.Dynamic;
using RiskierTrafficStops.Systems;

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
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }
        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                Settings.INIFileSetup();
                VersionChecker.CheckForUpdates();
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Watch you back out there officer!");
                Normal("Loaded succesfully");
                Events.OnPulloverOfficerApproachDriver += Events_OnPulloverOfficerApproachDriver;
                Events.OnPulloverDriverStopped += Events_OnPulloverDriverStopped;
                Events.OnPulloverStarted += Events_OnPulloverStarted;
                Events.OnPulloverEnded += Events_OnPulloverEnded;
            }
        }

        private static void Events_OnPulloverStarted(LHandle handle)
        {
            /*chosenChance = rndm.Next(1, 101);
            chosenOutcome = Settings.enabledScenarios[rndm.Next(Settings.enabledScenarios.Count)];
            if (chosenChance < Settings.Chance)
            {
                switch (chosenOutcome)
                {
                    case Scenarios.FleeFromTrafficStop:
                        Normal($"Chosen Scenario: {chosenOutcome.ToString()}");
                        GameFiber.WaitUntil(() => MainPlayer.CurrentVehicle.IsSirenOn);
                        Flee.FleeOutcome(handle);
                        HasEventHappend = true;
                        break;
                }
            }*/
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
            Normal("Unloaded succesfully");
            Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
            Events.OnPulloverDriverStopped -= Events_OnPulloverDriverStopped;
            Events.OnPulloverStarted -= Events_OnPulloverStarted;
            Events.OnPulloverEnded -= Events_OnPulloverEnded;
        }

        internal static void ChooseEvent(LHandle handle)
        {
            Debug($"HasEventHappend: {HasEventHappend}");
            if (HasEventHappend) { return; }
            HasEventHappend = true;
            Debug("Choosing Scenario");
            chosenChance = rndm.Next(1, 101);
            Debug($"Chance: {chosenChance}");
            if ((chosenChance <= Settings.Chance))
            {
                chosenOutcome = Settings.enabledScenarios[rndm.Next(Settings.enabledScenarios.Count)];
                Debug($"Chosen Outcome: {chosenOutcome.ToString()}");
                switch (chosenOutcome)
                {
                    case Scenarios.GetOutOfCarAndYell:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        Yell.YellOutcome(handle);
                        break;
                    case Scenarios.GetOutAndShoot:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        GetOutAndShoot.GOASOutcome(handle);
                        break;
                    case Scenarios.FleeFromTrafficStop:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        Flee.FleeOutcome(handle);
                        break;
                    case Scenarios.YellInCar:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        YellInCar.YICEventHandler(handle);
                        break;
                    case Scenarios.RevEngine:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        Rev.ROutcome(handle);
                        break;
                    case Scenarios.RamIntoPlayerVehicle:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        RamIntoYou.RIYOutcome(handle);
                        
                        break;
                    case Scenarios.ShootAndFlee:
                        Normal($"Chosen Scenario: {Yell.chosenOutcome.ToString()}");
                        ShootAndFlee.SAFOutcome(handle);
                        
                        break;
                    default:
                        Debug("No outcomes Enabled (or some other shit)");
                        break;
                }
            }
        }
    }
}