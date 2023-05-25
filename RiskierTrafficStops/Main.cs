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
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                Settings.INIFile();
                Events.OnPulloverOfficerApproachDriver += Events_OnPulloverOfficerApproachDriver;
                Events.OnPulloverEnded += Events_OnPulloverEnded;
            }
        }

        private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
        {
            HasEventHappend = false;
        }

        private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
        {
            GameFiber.StartNew(delegate
            {
                ChooseEvent(handle);
            });
        }

        internal static void ChooseEvent(LHandle handle)
        {
            Chance = rndm.Next(1, 101);
            if (!HasEventHappend && !Functions.IsCalloutRunning())
            {
                HasEventHappend = true;
                Scenarios[] ScenarioList = (Scenarios[])Enum.GetValues(typeof(Scenarios));
                ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                if (Chance < Settings.Chance)
                {
                    Normal($"Chosen Scenario: {ChosenEnum.ToString()}");
                    switch (ChosenEnum)
                    {
                        case Scenarios.Yell:
                            Yell.YellOutcome(handle);
                            break;
                        case Scenarios.Shoot:
                            GetOutAndShoot.GOASOutcome(handle);
                            break;
                        case Scenarios.Run:
                            Flee.FleeOutcome(handle);
                            break;
                        case Scenarios.YellInCar:
                            YellInCar.YICEventHandler(handle);
                            break;
                        case Scenarios.RevEngine:
                            Rev.ROutcome(handle);
                            break;
                        case Scenarios.RamIntoYou:
                            RamIntoYou.RIYOutcome(handle);
                            break;
                    }
                }
            }
        }
        public override void Finally()
        {
            Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
            Events.OnPulloverEnded -= Events_OnPulloverEnded;
        }
    }
}