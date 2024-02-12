using System.Reflection;
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
                new UpdateChecker(44036, Assembly.GetExecutingAssembly()).OnCompleted += (s, e) =>
                {
                    bool UpdateAvailable = e.UpdateAvailable;
                    var UpdateVersion = e.LatestVersion;

                    if (UpdateAvailable)
                    {
                        Game.DisplayNotification("3dtextures",
                            "mpgroundlogo_cops",
                            "Riskier Traffic Stops",
                            "~b~By Astro",
                            $"Plugin is ~r~out of to date~s~!\n" +
                            $"Online Version: ~g~{UpdateVersion}~s~\n" +
                            $"Installed version: ~y~{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}~s~\n" +
                            $"Please update ~r~ASAP~s~!");
                        Normal($"Online Version: {UpdateVersion} | Installed Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
                        Normal("Plugin is outdated, please up date to the latest version as soon as possible");
                    }
                    else if (!e.Failed)
                    {
                        Normal($"Online Version: {UpdateVersion} | Installed Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
                        Game.DisplayNotification("3dtextures",
                            "mpgroundlogo_cops",
                            "Riskier Traffic Stops",
                            "~b~By Astro",
                            "Plugin is ~g~up to date!");
                    }
                };

                // Displaying startup Notification
                Game.DisplayNotification("3dtextures",
                    "mpgroundlogo_cops",
                    "Riskier Traffic Stops",
                    "~b~By Astro",
                    $"{PluginLoadText.PickRandom()}");

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
            Game.DisplayNotification("3dtextures",
                "mpgroundlogo_cops",
                "Riskier Traffic Stops",
                "~b~By Astro",
                $"{PluginUnloadText.PickRandom()}");
            //Unsubscribes from events
            PulloverEventHandler.UnsubscribeFromEvents();

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