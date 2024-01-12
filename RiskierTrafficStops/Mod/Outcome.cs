using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.Helpers;
using RiskierTrafficStops.Engine.InternalSystems;

namespace RiskierTrafficStops.Mod;

internal abstract class Outcome
{
    internal static Ped Suspect;
    internal static Vehicle SuspectVehicle;
    internal static RelationshipGroup SuspectRelateGroup;
    internal static LHandle PursuitLHandle;
    internal static LHandle TrafficStopLHandle;

    internal static bool IsOutcomeRunning = false;
    
    internal virtual void StartOutcome(){}

    internal virtual bool MeetsRequirements(LHandle handle)
    {
        if (!Helper.GetSuspectAndSuspectVehicle(handle, out Suspect, out SuspectVehicle) || Functions.GetCurrentPullover() == null)
        {
            Logger.Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
            Helper.CleanupEvent();
            return false;
        }

        return true;
    }
    
    internal virtual void CleanupOutcome()
    {
        Logger.Normal("Cleaning up RTS Outcome...");
        PulloverEventHandler.HasEventHappened = false;
        GameFiberHandling.CleanupFibers();
        APIs.InvokeEvent(RTSEventType.End);
    }
    
    internal Outcome(LHandle handle)
    {
        TrafficStopLHandle = handle;
    }
    
    internal Outcome(string RelationshipGroupName, LHandle handle)
    {
        TrafficStopLHandle = handle;
        SuspectRelateGroup = new RelationshipGroup(RelationshipGroupName);
    }
}