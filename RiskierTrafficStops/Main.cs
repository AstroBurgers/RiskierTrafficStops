using System.Reflection;
using RiskierTrafficStops.Engine.FrontendSystems;
using Localization = RiskierTrafficStops.Engine.InternalSystems.Localization;

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
        if (onDuty && Localization.DoesJsonFileExist())
        {
            GameFiber.StartNew(() =>
            {
                // Setting up INI And checking for updates
                Normal("Setting up INI File...");
                IniFileSetup();
                Normal("Deserializing and reading Json...");
                Localization.ReadJson();
                Normal("Creating config menu menu...");
                ConfigMenu.CreateMenu();
                Normal("Adding console commands...");
                Game.AddConsoleCommands();
                Normal("Starting process to handle API lists...");
                GameFiber.StartNew(Processing.HandleIgnoredPedsList);
                Normal("Checking for updates...");
                new PluginUpdateChecker(Assembly.GetExecutingAssembly()).OnCompleted += (_, e) =>
                {
                    var updateAvailable = e.UpdateAvailable;
                    var updateVersion = e.LatestVersion;

                    if (updateAvailable)
                    {
                        Game.DisplayNotification("3dtextures",
                            "mpgroundlogo_cops",
                            "Riskier Traffic Stops",
                            "~b~By Astro",
                            $"Plugin is ~r~out of to date~s~!\n" +
                            $"Online Version: ~g~{updateVersion}~s~\n" +
                            $"Installed version: ~y~{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}~s~\n" +
                            $"Please update ~r~ASAP~s~!");
                        Normal(
                            $"Online Version: {updateVersion} | Installed Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
                        Normal("Plugin is outdated, please up date to the latest version as soon as possible");
                    }
                    else if (!e.Failed)
                    {
                        Normal(
                            $"Online Version: {updateVersion} | Installed Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
                        Game.DisplayNotification("3dtextures",
                            "mpgroundlogo_cops",
                            "Riskier Traffic Stops",
                            "~b~By Astro",
                            "Plugin is ~g~up to date!");
                    }
                };

                // Displaying startup Notifications
                Game.DisplayNotification("3dtextures",
                    "mpgroundlogo_cops",
                    "Riskier Traffic Stops",
                    "~b~By Astro",
                    $"{PluginLoadText.PickRandom()}");

                if (DebugMode)
                {
                    Game.DisplayNotification("3dtextures",
                        "mpgroundlogo_cops",
                        "Riskier Traffic Stops",
                        "~b~By Astro",
                        "Debug mode is enabled, please let the developer know.");
                }

                //Subscribes to events
                PulloverEventHandler.SubscribeToEvents();

                AppDomain.CurrentDomain.DomainUnload += Cleanup;
                AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
                Normal("Loaded successfully");
            });
        }
    }

    private static void Cleanup(object sender, EventArgs e)
    {
        try
        {
            //Unsubscribes from events
            PulloverEventHandler.UnsubscribeFromEvents();

            Normal("Unloaded successfully");
        }
        catch (Exception ex)
        {
            Error(ex);
        }
    }
    
    private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        // Credit to Khori for this
        if (e.ExceptionObject is Exception exception)
        {
            // Log or handle the exception here
            Normal("Global exception caught");
            Normal($"Terminating={e.IsTerminating}");
            Error(exception);
        }
        else
        {
            // It's not an Exception object; handle it accordingly
            Normal("Global exception thrown but no exception was provided");
        }
    }


    public override void Finally()
    {
        Game.DisplayNotification("3dtextures",
            "mpgroundlogo_cops",
            "Riskier Traffic Stops",
            "~b~Unload Message",
            $"{PluginUnloadText.PickRandom()}");
    }
}