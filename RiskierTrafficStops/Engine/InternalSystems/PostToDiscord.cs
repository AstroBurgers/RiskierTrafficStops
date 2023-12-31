using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    internal static class PostToDiscord
    {
        // Send message
        private static readonly List<string> Blacklist = new();

        internal static void LogToDiscord(Exception ex, string location)
        {
            try
            {
                if (VersionChecker._state == VersionChecker.State.Current)
                {
                    if (!Blacklist.Contains(ex.GetType().Name))
                    {
                        Post("" +
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
                                $"**Location**\n```prolog\n{location}```" +
                                $"**Beta Version?**\n```Beta Version: false```"
                            },
                        });
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