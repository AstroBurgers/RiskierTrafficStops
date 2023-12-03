using System;
using System.IO;
using RiskierTrafficStops.Engine.InternalSystems;
using static RansomAmbience.API.Functions;
using LSPD_First_Response.Mod;
using LSPD_First_Response.Mod.API;

namespace RiskierTrafficStops.API.ExternalAPIs;

internal static class RAFunctions
{
    internal static bool RaEventCheck()
    {
        try
        {
            if (IsEventRunning())
            {
                Logger.Debug("Pullover is a part of an Ransome Ambience event, aborting RTS events...");
                return false;
            }
            return true;
        }
        catch (FileNotFoundException)
        {
            Logger.Debug("Ransom Ambience cannot be found, user might not have it installed");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Debug("Ransom Ambience cannot be found, user might not have it installed");
            return false;
        }
    }
    
    internal static bool RaCompatibilityCheck(LHandle handle)
    {
        try
        {
            Logger.Debug("Performing RA compatibility check...");
            var ped = Functions.GetPulloverSuspect(handle);
            if (RansomAmbience.API.Functions.IsEntityActivelyUsed(ped))
            {
                return RaEventCheck();
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
}