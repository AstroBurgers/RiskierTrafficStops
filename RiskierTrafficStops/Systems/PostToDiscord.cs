using Rage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Systems
{
    internal class PostToDiscord
    {
        // Send message
        private static readonly List<string> blacklist = new List<string>();
        internal static void LogToDiscord(Exception ex, string location)
        {
            try
            {
                if (VersionChecker.CurrentVersion == VersionChecker.onlineVersion)
                {
                    if (!blacklist.Contains(ex.GetType().Name))
                    {
                        POST("https://discord.com/api/webhooks/1122374055132143669/IBHwHqQusZZb0x8Po4qtBbYMBwDGV4a3WDyUFcxkQt3OsgdLL3X2-nMPmIRYTHGS0VU0" +
                            "", new NameValueCollection()
                        {
                            {
                                "username",
                                $"RiskierTrafficStops v{VersionChecker.CurrentVersion}"
                            },
                            {
                                "content",
                                $"**Exception Type**```fix\n{ex.GetType()}```\n" +
                                $"**Stack Trace**```\n{ex}\n```\n" +
                                $"**Message**```\n{ex.Message}\n```\n" +
                                $"**Location**\n```prolog\n{location}```"
                            },
                        });

                        blacklist.Add(ex.GetType().Name);
                    }

                    Logger.Debug("Sent exception message to Discord webhook");
                }
            }
            catch (WebException webEx)
            {
                Logger.Error(webEx, "PostToDiscord.cs");
            }
            catch (Exception ex2)
            {
                Logger.Error(ex2, "PostToDiscord.cs");
            }
        }

        // Connect to discord
        public static byte[] POST(string uri, NameValueCollection pair)
        {
            using (WebClient wc = new WebClient())
            {
                return wc.UploadValues(uri, pair);
            }
        }
    }
}
