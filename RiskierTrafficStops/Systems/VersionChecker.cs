using Rage;
using System.Net;
using System.Reflection;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Systems
{
    internal class VersionChecker
    {
        internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        internal static string onlineVersion;
        internal static void CheckForUpdates()
        {
            var webClient = new WebClient();
            var pluginUpToDate = false;

            try
            {
                onlineVersion = webClient
     .DownloadString(
         "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44036&textOnly=1")
     .Trim();
                Logger.Debug($"Online Version: {onlineVersion} | Installed Version: {CurrentVersion}");
                pluginUpToDate = onlineVersion == CurrentVersion;
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
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Plugin is ~r~out of to date, Please update ASAP");
                }
                if (pluginUpToDate)
                {
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Plugin is ~g~up to date!");
                }

            }
        }
    }
}