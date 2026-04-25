using System.Reflection;
using CommonDataFramework.API;
using RiskierTrafficStops.Engine.FrontendSystems;
using static RiskierTrafficStops.Engine.Helpers.DependencyHelper;
using static RiskierTrafficStops.Engine.InternalSystems.Localization;

namespace RiskierTrafficStops;

public class Main : Plugin
{
    internal static bool OnDuty;

    private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

    public override void Initialize()
    {
        Normal("Plugin initialized, go on duty to fully load plugin.");
        Functions.OnOnDutyStateChanged += Functions_OnDutyStateChanged;
    }

    private static void Functions_OnDutyStateChanged(bool onDuty)
    {
        OnDuty = onDuty;

        if (!onDuty) return;

        if (!DoesJsonFileExist())
        {
            Normal("JSON config file not found — plugin will not load. Please reinstall.");
            ShowNotification("~r~JSON config file missing!~s~ Please reinstall the plugin.");
            return;
        }

        if (!VerifyDependencies())
        {
            Normal("Dependency verification failed — plugin will not load.");
            ShowNotification("~r~Missing dependencies!~s~ Check the LSPDFR console for details.");
            return;
        }

        GameFiber.StartNew(LoadPlugin);
    }

    private static void LoadPlugin()
    {
        try
        {
            // Wait for CDF to be ready, bail out if it times out
            Normal("Waiting for CDF to be ready...");
            GameFiber.WaitUntil(CDFFunctions.IsPluginReady, 30000);

            if (!CDFFunctions.IsPluginReady())
            {
                Normal("CDF did not become ready within 30 seconds, aborting load.");
                ShowNotification("~r~CommonDataFramework timed out.~s~ RTS will not load.");
                return;
            }

            // INI setup
            Normal("Setting up INI file...");
            TryExecute("INI file setup", IniFileSetup);

            // JSON deserialization
            Normal("Deserializing and reading JSON...");
            if (!TryExecute("JSON deserialization", ReadJson))
            {
                Normal("JSON deserialization failed. Plugin will not load. Check your JSON file for syntax errors.");
                ShowNotification("~r~JSON file is invalid!~s~ Check the LSPDFR console for details.");
                return;
            }

            // Menu and commands
            Normal("Creating config menu...");
            TryExecute("Config menu creation", ConfigMenu.CreateMenu);

            Normal("Adding console commands...");
            TryExecute("Console commands", Game.AddConsoleCommands);

            // Background processing
            Normal("Starting process to handle API lists...");
            GameFiber.StartNew(Processing.HandleIgnoredPedsList);

            // Update check. Non-critical, never block load
            Normal("Checking for updates...");
            TryExecute("Update checker", StartUpdateCheck);

            // Startup notification
            TryExecute("Startup notification", () =>
            {
                var loadText = PluginLoadText?.PickRandom() ?? "Loaded!";
                ShowNotification(loadText);
            });

            if (DebugMode)
            {
                ShowNotification("~y~Debug mode is enabled~s~, please let the developer know.");
            }

            // Subscribe to events only after everything else is set up
            PulloverEventHandler.SubscribeToEvents();

            AppDomain.CurrentDomain.DomainUnload += Cleanup;
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

            Normal("Loaded successfully.");
        }
        catch (Exception ex)
        {
            Error(new Exception("Critical failure during plugin load", ex));
            ShowNotification("~r~RTS failed to load!~s~ Check the LSPDFR console for details.");
        }
    }

    private static void StartUpdateCheck()
    {
        new PluginUpdateChecker(CurrentAssembly).OnCompleted += (_, e) =>
        {
            try
            {
                var installedVersion = CurrentAssembly.GetName().Version?.ToString(3) ?? "Unknown";
                string onlineVersion = e.LatestVersion.ToString() ?? "Unknown";

                Normal($"Online Version: {onlineVersion} | Installed Version: {installedVersion}");

                if (e.Failed)
                {
                    Normal("Update check failed — skipping update notification.");
                    return;
                }

                if (e.UpdateAvailable)
                {
                    Normal("Plugin is outdated, please update to the latest version as soon as possible.");
                    ShowNotification(
                        $"Plugin is ~r~out of date~s~!\n" +
                        $"Online Version: ~g~{onlineVersion}~s~\n" +
                        $"Installed Version: ~y~{installedVersion}~s~\n" +
                        "Please update ~r~ASAP~s~!");
                }
                else
                {
                    ShowNotification("Plugin is ~g~up to date!");
                }
            }
            catch (Exception ex)
            {
                Error(new Exception("Exception in update check callback", ex));
            }
        };
    }

    /// <summary>
    /// Wraps an action in a try/catch, logging any exception without crashing the caller.
    /// Returns true if the action succeeded, false if it threw.
    /// </summary>
    private static bool TryExecute(string stepName, Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            Error(new Exception($"Non-critical failure during '{stepName}'", ex));
            return false;
        }
    }

    /// <summary>
    /// Displays a notification using the standard RTS header.
    /// </summary>
    private static void ShowNotification(string message)
    {
        Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", message);
    }

    private static void Cleanup(object sender, EventArgs e)
    {
        try
        {
            PulloverEventHandler.UnsubscribeFromEvents();
            Normal("Unloaded successfully.");
        }
        catch (Exception ex)
        {
            Error(new Exception("Exception during cleanup", ex));
        }
    }

    private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception ex)
            {
                Error(new Exception($"Unhandled global exception (Terminating={e.IsTerminating})", ex));
            }
            else
            {
                Normal($"Global exception thrown but no Exception object was provided (Terminating={e.IsTerminating})");
            }
        }
        catch
        {
            // Never let the exception handler itself throw
        }
    }

    public override void Finally()
    {
        try
        {
            var unloadText = PluginUnloadText?.PickRandom() ?? "Unloaded.";
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~Unload Message",
                unloadText);
        }
        catch
        {
            // Finally() must never throw
        }
    }
}