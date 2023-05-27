using System.Net;
using System.Reflection;
using System.Windows.Forms;
using Rage;

namespace RiskierTrafficStops
{
    internal class VersionChecker
    {
        internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        internal static void CheckForUpdates()
        {
            var webClient = new WebClient();
            var pluginUpToDate = false;
            var webSuccess = false;

            try
            {
                var receivedVersion = webClient.DownloadString("https://www.lcpdfr.com/downloads/gta5mods/scripts/44036-riskier-traffic-stops/").Trim();
                Logger.Debug($"Recieved Version: {receivedVersion} | Local Version: {CurrentVersion}");
                pluginUpToDate = receivedVersion == CurrentVersion;
                webSuccess = true;
            }
            catch (WebException TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Logger.Error(ThrowHands);
                Logger.Error("Please check your internet connection");
                throw;
            }
            finally
            {

                if (!pluginUpToDate)
                {
                    Logger.Debug("Plugin is outdated, please up date to the latest version as soon as possible");
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Plugin is ~r~out of to date!");
                }
                if (pluginUpToDate)
                {
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Plugin is ~g~up to date!");
                }

            }
        }
    }
}