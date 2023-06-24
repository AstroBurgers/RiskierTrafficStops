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

        internal static Random rndm = new Random(DateTime.Now.Millisecond);
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
            chosenChance = rndm.Next(1, 101);
            Scenarios[] ScenarioList = (Scenarios[])Enum.GetValues(typeof(Scenarios));
            chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
            if (chosenChance < Settings.Chance)
            {
                switch (chosenOutcome)
                {
                    case Scenarios.Run:
                        if (Settings.Flee)
                        {
                            Normal($"Chosen Scenario: {chosenOutcome.ToString()}");
                            GameFiber.WaitUntil(() => MainPlayer.CurrentVehicle.IsSirenOn);
                            Flee.FleeOutcome(handle);
                        }
                        else
                        {
                            Normal("Chosen event is disabled");
                        }
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
                Scenarios[] ScenarioList = (Scenarios[])Enum.GetValues(typeof(Scenarios));
                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                int TimesRan = 0;
                while (TimesRan >= 6)
                {
                    GameFiber.Yield();
                    TimesRan += 1;
                    switch (chosenOutcome)
                    {
                        case Scenarios.Yell:
                            if (Settings.Yell)
                            {
                                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                                Yell.YellOutcome(handle);
                                HasEventHappend = true;
                            }
                            else
                            {
                                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                                Normal("Chosen event is disabled, choosing a new one...");
                                break;
                            }
                            break;
                        case Scenarios.Shoot:
                            if (Settings.GOAS)
                            {
                                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                                GetOutAndShoot.GOASOutcome(handle);
                                HasEventHappend = true;

                            }
                            else
                            {
                                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                                Normal("Chosen event is disabled, choosing a new one...");
                                break;
                            }
                            break;
                        case Scenarios.Run:
                            if (Settings.Flee)
                            {
                                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                                Flee.FleeOutcome(handle);
                                HasEventHappend = true;
                            }
                            else
                            {
                                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                                Normal("Chosen event is disabled, choosing a new one...");
                                break;
                            }
                            break;
                        case Scenarios.YellInCar:
                            if (Settings.YIC)
                            {
                                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                                YellInCar.YICEventHandler(handle);
                                HasEventHappend = true;
                            }
                            else
                            {
                                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                                Normal("Chosen event is disabled, choosing a new one...");
                                break;
                            }
                            break;
                        case Scenarios.RevEngine:
                            if (Settings.Rev)
                            {
                                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                                Rev.ROutcome(handle);
                                HasEventHappend = true;
                            }
                            else
                            {
                                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                                Normal("Chosen event is disabled, choosing a new one...");
                                break;
                            }
                            break;
                        case Scenarios.RamIntoYou:
                            if (Settings.Ram)
                            {
                                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                                RamIntoYou.RIYOutcome(handle);
                                HasEventHappend = true;
                            }
                            else
                            {
                                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                                Normal("Chosen event is disabled, choosing a new one...");
                                break;
                            }
                            break;
                    }
                }
            }
        }
    }
}