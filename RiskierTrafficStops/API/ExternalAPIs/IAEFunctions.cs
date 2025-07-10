using System.IO;

namespace RiskierTrafficStops.API.ExternalAPIs;

internal static class IaeFunctions
{
    private const string Logmsg = "Immersive Ambient Events cannot be found, user might not have it installed";

    /// <summary>
    /// Checks if an entity is being used by a normal IAE event
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    internal static bool IsPedUsedByAmbientEvent(Entity entity)
    {
        try
        {
            return ImmersiveAmbientEvents.API.EventAPI.IsEntityUsedByAnyEvent(entity);
        }
        catch (FileNotFoundException)
        {
            Normal(Logmsg);
            return false;
        }
        catch (Exception)
        {
            Normal(Logmsg);
            return false;
        }
    }

    /// <summary>
    /// Checks if an entity is being used by an IAE BOLO event
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    internal static bool IsPedUsedByBoloEvent(Entity entity)
    {
        try
        {
            return ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(entity);
        }
        catch (FileNotFoundException)
        {
            Normal(Logmsg);
            return false;
        }
        catch (Exception)
        {
            Normal(Logmsg);
            return false;
        }
    }

    internal static bool IaeEventCheck()
    {
        try
        {
            if (ImmersiveAmbientEvents.API.EventAPI.CheckIfEventIsRunning(ImmersiveAmbientEvents.API.EventAPI.Events.StreetRacing))
            {
                Normal("Pullover is a part of an IAE street racing event, aborting RTS events...");
                return false;
            }
            return true;
        }
        catch (FileNotFoundException)
        {
            Normal(Logmsg);
            return false;
        }
        catch (Exception)
        {
            Normal(Logmsg);
            return false;
        }
    }

    /// <summary>
    /// Handles all compatibility checks for IAE, returns true if it passes the checks
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    internal static bool IaeCompatibilityCheck(LHandle handle)
    {
        try
        {
            Normal("Performing IAE compatibility check...");
            var ped = Functions.GetPulloverSuspect(handle);
            if (IsPedUsedByAmbientEvent(ped))
            {
                return IaeEventCheck();
            }
            else if (IsPedUsedByBoloEvent(ped))
            {
                Normal("Pullover is a part of an IAE BOLO event, aborting RTS events...");
                return false;
            }
            return true;
        }
        catch (FileNotFoundException)
        {
            Normal(Logmsg);
            return true;
        }
        catch (Exception)
        {
            Normal(Logmsg);
            return true;
        }
    }
}