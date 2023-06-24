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
using static RiskierTrafficStops.Outcomes.Yell;
using static RiskierTrafficStops.Systems.Logger;
using System.Dynamic;

namespace RiskierTrafficStops
{
    public class Main : Plugin
    {
        internal enum Scenarios
        {
            Yell,
            Shoot,
            Run,
            YellInCar,
            RevEngine,
            RamIntoYou,
        }

        internal static bool HasEventHappend = false;
        internal static Scenarios chosenOutcome;
        internal static int chosenChance;
        internal static Scenarios[] ScenarioList = (Scenarios[])Enum.GetValues(typeof(Scenarios));
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                Settings.INIFile();
                Systems.VersionChecker.CheckForUpdates();
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
                    case Scenarios.Run:
                        Normal($"Chosen Scenario: {chosenOutcome.ToString()}");
                        GameFiber.WaitUntil(() => MainPlayer.CurrentVehicle.IsSirenOn);
                        Flee.FleeOutcome(handle);
                        break;
                    default:
                        Normal("Event not enabled");
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
            //if (!HasEventHappend) { GameFiber.StartNew(() => ChooseEvent(handle)); }
        }

        private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
        {
            if (!HasEventHappend)
            {
                ShootAndFlee.SAFOutcome(handle);
                HasEventHappend = true;
            }
            //if (!HasEventHappend) { GameFiber.StartNew(() => ChooseEvent(handle)); }
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
            chosenChance = rndm.Next(1, 101);
            if ((chosenChance <= Settings.Chance) && !HasEventHappend && !Functions.IsCalloutRunning())
            {
                chosenOutcome = Settings.enabledScenarios[rndm.Next(Settings.enabledScenarios.Count)];
                switch (chosenOutcome)
                {
                    case Scenarios.Yell:
                        Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                        Yell.YellOutcome(handle);
                        HasEventHappend = true;
                        break;
                    case Scenarios.Shoot:
                        Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                        GetOutAndShoot.GOASOutcome(handle);
                        HasEventHappend = true;
                        break;
                    case Scenarios.Run:
                        Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                        Flee.FleeOutcome(handle);
                        HasEventHappend = true;
                        break;
                    case Scenarios.YellInCar:
                        Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                        YellInCar.YICEventHandler(handle);
                        HasEventHappend = true;
                        break;
                    case Scenarios.RevEngine:
                        Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                        Rev.ROutcome(handle);
                        HasEventHappend = true;
                        break;
                    case Scenarios.RamIntoYou:
                        Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                        RamIntoYou.RIYOutcome(handle);
                        HasEventHappend = true;
                        break;
                }
            }
        }
    }
}