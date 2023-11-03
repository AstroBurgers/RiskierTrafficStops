using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Systems;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops
{
    public class Main : Plugin
    {
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
                //Subscribes to events
                PulloverEventHandler.SubscribeToEvents();
            }
        }

        public override void Finally()
        {
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Did you crash or are you a dev?");
            //Unsubscribes from events
            PulloverEventHandler.UnsubscribeToEvents();
            Debug("Unloaded succesfully");
        }
    }
}