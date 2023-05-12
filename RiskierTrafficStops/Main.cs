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
        }

        internal static Random rndm = new Random();
        internal static bool HasEventHappend = false;
        internal static Scenarios ChosenEnum;
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
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
            if (!HasEventHappend && !Functions.IsCalloutRunning())
            {
                HasEventHappend = true;
                string Weapon = WeaponList[rndm.Next(WeaponList.Length)];
                Scenarios[] ScenarioList = (Scenarios[])Enum.GetValues(typeof(Scenarios));
                ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];
                int Chance = rndm.Next(1, 101);
                if (Chance < 10)
                {
                    Normal("Main.cs", $"Chosen Scenario: {ChosenEnum.ToString()}");
                    switch (ChosenEnum)
                    {                        
                        case Scenarios.Yell:
                            Yell.YellOutcome(handle);
                            break;
                        case Scenarios.Shoot:
                            GetOutAndShoot.GOASOutcome(handle, Weapon);
                            break;
                        case Scenarios.Run:
                            Flee.FleeOutcome(handle);
                            break;
                        case Scenarios.YellInCar:
                            YellInCar.YICEventHandler(handle);
                            break;
                        case Scenarios.RevEngine:

                            break;
                    }
                }
            }
        }

        public override void Finally()
        {
            Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
        }
    }
}