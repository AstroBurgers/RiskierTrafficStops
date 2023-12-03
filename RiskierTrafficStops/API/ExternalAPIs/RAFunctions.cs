/*using System;
using System.IO;
using RiskierTrafficStops.Engine.InternalSystems;
using LSPD_First_Response.Mod;
using LSPD_First_Response.Mod.API;

namespace RiskierTrafficStops.API.ExternalAPIs;

internal static class RAFunctions
{
    internal static bool RaCompatibilityCheck(LHandle handle)
    {
        try
        {
            Logger.Debug("Performing RA compatibility check...");
            var ped = Functions.GetPulloverSuspect(handle);
            if (RansomAmbience.API.Functions.IsEventRunning())
            {
                return false;
            }
            return true;
        }
        catch (FileNotFoundException)
        {
            Logger.Debug("Ransom Ambience cannot be found, user might not have it installed");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Debug("Ransom Ambience cannot be found, user might not have it installed");
            return true;
        }
    }
}*/