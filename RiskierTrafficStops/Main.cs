using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Systems;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops
{
    public class Main : Plugin
    {
        internal static bool OnDuty;

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            OnDuty = onDuty;
            if (onDuty)
            {
                // Setting up INI And checking for updates
                Settings.IniFileSetup();
                ConfigMenu.CreateMenu();
                Game.AddConsoleCommands();
                VersionChecker.CheckForUpdates();
                // Displaying startup Notification
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Watch you back out there officer!");
                Debug("Loaded successfully");
                switch (Settings.AutoLogEnabled)
                {
                    //Displaying Auto-log Notification
                    case true:
                        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops",
                            "~b~Auto Logging Status", "Auto Logging is ~g~Enabled");
                        break;
                    case false:
                        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops",
                            "~b~Auto Logging Status", "Auto Logging is ~r~Disabled");
                        break;
                }

                //Subscribes to events
                PulloverEventHandler.SubscribeToEvents();
            }
        }

        public override void Finally()
        {
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Did you crash or are you a dev?");
            //Unsubscribes from events
            PulloverEventHandler.UnsubscribeToEvents();
            Debug("Unloaded successfully");
        }
    }
}