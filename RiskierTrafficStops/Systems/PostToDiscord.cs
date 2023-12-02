using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace RiskierTrafficStops.Systems
{
    internal static class PostToDiscord
    {
        // Send message
        private static readonly List<string> Blacklist = new();

        internal static void LogToDiscord(Exception ex, string location)
        {
            try
            {
                if (VersionChecker.CurrentVersion == VersionChecker.OnlineVersion)
                {
                    if (!Blacklist.Contains(ex.GetType().Name))
                    {
                        Post("https://discord.com/api/webhooks/1180306078068637696/AjfrUv1o0Au5vVw83GFm_7zocDWorntcakPAyYtLvzQFpaYD01BzgbFaBOani2yjBoNn" +
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

                        Blacklist.Add(ex.GetType().Name);
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
        private static byte[] Post(string uri, NameValueCollection pair)
        {
            using (var wc = new WebClient())
            {
                return wc.UploadValues(uri, pair);
            }
        }
    }
}