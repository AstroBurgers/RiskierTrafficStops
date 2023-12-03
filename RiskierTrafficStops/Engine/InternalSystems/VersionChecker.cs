using System.Net;
using System.Reflection;
using Rage;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    internal static class VersionChecker
    {
        internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        internal static string OnlineVersion;

        internal static void CheckForUpdates()
        {
            var webClient = new WebClient();
            var pluginUpToDate = false;

            try
            {
                OnlineVersion = webClient
     .DownloadString(
         "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44036&textOnly=1")
     .Trim();
                Logger.Debug($"Online Version: {OnlineVersion} | Installed Version: {CurrentVersion}");
                pluginUpToDate = OnlineVersion == CurrentVersion;
            }
            catch (WebException e)
            {
                Error(e, "VersionChecker.cs");
                Debug("Please check your internet connection");
            }
            finally
            {
                if (!pluginUpToDate)
                {
                    Logger.Debug("Plugin is outdated, please up date to the latest version as soon as possible");
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Plugin is ~r~out of to date~s~, Auto Logging ~r~disabled~s~, Please update ~r~ASAP~s~!");
                }
                if (pluginUpToDate)
                {
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Plugin is ~g~up to date!");
                }
            }
        }
    }
}