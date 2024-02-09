using RiskierTrafficStops.Engine.FrontendSystems;
using static RiskierTrafficStops.Engine.Helpers.DependencyHelper;

namespace RiskierTrafficStops;

public class Main : Plugin
{
    internal static bool OnDuty;

    public override void Initialize()
    {
        Normal("Plugin initialized, go on duty to fully load plugin.");
        Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
    }

    private static void Functions_OnOnDutyStateChanged(bool onDuty)
    {
        OnDuty = onDuty;
        if (onDuty)
        {
            GameFiber.StartNew(() =>
            {
                if (!VerifyDependencies()) return;

                // Setting up INI And checking for updates
                Normal("Setting up INI File...");
                IniFileSetup();
                Normal("Creating config menu menu...");
                ConfigMenu.CreateMenu();
                Normal("Adding console commands...");
                Game.AddConsoleCommands();
                Normal("Checking for updates...");
                VersionChecker.IsUpdateAvailable();
                    
                // Displaying startup Notification
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro",
                    "Watch your back out there officer!");
                    
                //Subscribes to events
                PulloverEventHandler.SubscribeToEvents();

                AppDomain.CurrentDomain.DomainUnload += Cleanup;
                Normal("Loaded successfully");
            });
        }
    }

    private static void Cleanup(object sender, EventArgs e)
    {
        try
        {
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro",
                "Did you crash or are you a dev?");
            //Unsubscribes from events
            PulloverEventHandler.UnsubscribeFromEvents();
            if (VersionChecker.UpdateThread.IsAlive)
            {
                Normal("Update thread was still running, shutting down...");
                VersionChecker.UpdateThread.Abort();
            }

            Normal("Unloaded successfully");
        }
        catch (Exception ex)
        {
            Error(ex, nameof(Cleanup));
        }
    }


    public override void Finally()
    {
    }
}