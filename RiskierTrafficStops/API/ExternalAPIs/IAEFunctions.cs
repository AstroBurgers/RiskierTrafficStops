using System.IO;

namespace RiskierTrafficStops.API.ExternalAPIs;

internal static class IaeFunctions
{
    private const string LogMessage = "Immersive Ambient Events cannot be found, user might not have it installed";

    private static readonly bool IaeAvailable = File.Exists("Plugins/LSPDFR/ImmersiveAmbientEvents.dll");

    /// <summary>
    /// Checks if an entity is being used by a normal IAE event
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private static bool IsPedUsedByAmbientEvent(Entity entity)
    {
        if (IaeAvailable) return ImmersiveAmbientEvents.API.EventAPI.IsEntityUsedByAnyEvent(entity);
        Normal(LogMessage);
        return false;

    }

    /// <summary>
    /// Checks if an entity is being used by an IAE BOLO event
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private static bool IsPedUsedByBoloEvent(Entity entity)
    {
        if (IaeAvailable) return ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(entity);
        Normal(LogMessage);
        return false;
    }

    private static bool IaeEventCheck()
    {
        if (!IaeAvailable) return false;

        if (ImmersiveAmbientEvents.API.EventAPI.GetActiveEvent() is not ImmersiveAmbientEvents.API.EventAPI
                .ActiveEvent.StreetRacing) return true;

        Normal("Pullover is a part of an IAE street racing event, aborting RTS events...");
        return false;
    }


    /// <summary>
    /// Handles all compatibility checks for IAE, returns true if it passes the checks
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    internal static bool IaeCompatibilityCheck(LHandle handle)
    {
        if (!IaeAvailable) return true;

        Normal("Performing IAE compatibility check...");
        Ped ped = Functions.GetPulloverSuspect(handle);

        if (IsPedUsedByAmbientEvent(ped))
            return IaeEventCheck();

        if (!IsPedUsedByBoloEvent(ped)) return true;

        Normal("Pullover is a part of an IAE BOLO event, aborting RTS events...");
        return false;
    }
}