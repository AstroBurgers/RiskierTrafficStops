using System.Net;
using System.Reflection;
using System.Threading;
using Rage;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    
    /*
     * CREDIT TO SuperPyroManiac for the orignal code
     * Modifications made by myself
     * https://github.com/SuperPyroManiac/SuperPlugins/blob/master/SuperCallouts/SimpleFunctions/VersionChecker.cs
     */
    
    internal static class VersionChecker
    {
        private enum State
        {
            Failed,
            Update,
            Current
        }

        private static State _state = State.Current;
        private static string _receivedData = string.Empty;
        internal static Thread UpdateThread = new Thread(CheckVersion);

        internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        internal static string OnlineVersion;

        internal static void IsUpdateAvailable()
        {
            UpdateThread.Start();
            GameFiber.Sleep(20000);

            while (UpdateThread.IsAlive) GameFiber.Wait(1000);

            switch (_state)
            {
                case State.Failed:
                    Debug("Update check failed... please check your internet connection");
                    break;
                case State.Update:
                    Game.DisplayNotification("3dtextures",
                        "mpgroundlogo_cops",
                        "Riskier Traffic Stops",
                        "~b~By Astro",
                        "Plugin is ~r~out of to date~s~, Auto Logging ~r~disabled~s~, Please update ~r~ASAP~s~!");
                    Logger.Debug($"Online Version: {OnlineVersion} | Installed Version: {CurrentVersion}");
                    Logger.Debug("Plugin is outdated, please up date to the latest version as soon as possible");
                    break;
                case State.Current:
                    Logger.Debug($"Online Version: {OnlineVersion} | Installed Version: {CurrentVersion}");
                    Game.DisplayNotification("3dtextures",
                        "mpgroundlogo_cops",
                        "Riskier Traffic Stops",
                        "~b~By Astro",
                        "Plugin is ~g~up to date!");
                    break;
            }
        }

        private static void CheckVersion()
        {
            try
            {
                _receivedData = new WebClient()
                    .DownloadString(
                        "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44036&textOnly=1")
                    .Trim();
            }
            catch (WebException)
            {
                _state = State.Failed;
            }

            if (_receivedData == CurrentVersion) return;
            _state = State.Update;
        }
    }
}