using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Helper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Deployment.Internal;
using RiskierTrafficStops.Outcomes;
using static RiskierTrafficStops.Outcomes.Yell;
using static RiskierTrafficStops.Logger;

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

        internal static Random rndm = new Random();
        internal static bool HasEventHappend = false;
        internal static Scenarios ChosenEnum;
        internal static int Chance;
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
            Chance = rndm.Next(1, 101);
            Scenarios[] ScenarioList = (Scenarios[])Enum.GetValues(typeof(Scenarios));
            ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
            if (Chance < Settings.Chance)
            {
                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                switch (ChosenEnum)
                {
                    case Scenarios.Run:
                        Flee.FleeOutcome(handle);
                        break;
                }
            }
        }

        private static void Events_OnPulloverDriverStopped(LHandle handle)
        {
            GameFiber.StartNew(() => ChooseEvent(handle));
        }

        private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
        {
            HasEventHappend = false;
        }

        private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
        {
            GameFiber.StartNew(() => ChooseEvent(handle));
        }

        internal static void ChooseEvent(LHandle handle)
        {
            Chance = rndm.Next(1, 101);
            if (!HasEventHappend && !Functions.IsCalloutRunning())
            {
                ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                int TimesRan = 0;
                Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                if (Chance < Settings.Chance)
                {
                    while (!HasEventHappend)
                    {
                        TimesRan += 1;
                        if (TimesRan > 6)
                        {
                            switch (ChosenEnum)
                            {
                                case Scenarios.Yell:
                                    if (Settings.Yell)
                                    {
                                        HasEventHappend = true;
                                        Yell.YellOutcome(handle);
                                    }
                                    else
                                    {
                                        ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                                        Normal("Chosen event is disabled, choosing a new one...");
                                        break;
                                    }
                                    break;
                                case Scenarios.Shoot:
                                    if (Settings.GOAS)
                                    {
                                        HasEventHappend = true;
                                        GetOutAndShoot.GOASOutcome(handle);
                                    }
                                    else
                                    {
                                        ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                                        Normal("Chosen event is disabled, choosing a new one...");
                                        break;
                                    }
                                    break;
                                case Scenarios.Run:
                                    if (Settings.Flee)
                                    {
                                        HasEventHappend = true;
                                        Flee.FleeOutcome(handle);
                                    }
                                    else
                                    {
                                        ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                                        Normal("Chosen event is disabled, choosing a new one...");
                                        break;
                                    }
                                    break;
                                case Scenarios.YellInCar:
                                    if (Settings.YIC)
                                    {
                                        HasEventHappend = true;
                                        YellInCar.YICEventHandler(handle);
                                    }
                                    else
                                    {
                                        ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                                        Normal("Chosen event is disabled, choosing a new one...");
                                        break;
                                    }
                                    break;
                                case Scenarios.RevEngine:
                                    if (Settings.Rev)
                                    {
                                        HasEventHappend = true;
                                        Rev.ROutcome(handle);
                                    }
                                    else
                                    {
                                        ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                                        Normal("Chosen event is disabled, choosing a new one...");
                                        break;
                                    }
                                    break;
                                case Scenarios.RamIntoYou:
                                    if (Settings.Ram)
                                    {
                                        HasEventHappend = true;
                                        RamIntoYou.RIYOutcome(handle);
                                    }
                                    else
                                    {
                                        ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
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
        public override void Finally()
        {
            Game.DisplayNotification();
            Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
            Events.OnPulloverDriverStopped -= Events_OnPulloverDriverStopped;
            Events.OnPulloverStarted -= Events_OnPulloverStarted;
            Events.OnPulloverEnded -= Events_OnPulloverEnded;
        }
    }
}