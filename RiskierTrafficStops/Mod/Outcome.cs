namespace RiskierTrafficStops.Mod;

internal abstract class Outcome
{
    internal static Ped Suspect;
    internal static Vehicle SuspectVehicle;
    internal static RelationshipGroup SuspectRelateGroup;
    internal static LHandle PursuitLHandle;
    internal static LHandle TrafficStopLHandle;
    
    internal virtual void StartOutcome(){}

    internal static bool MeetsRequirements(LHandle handle)
    {
        if (!GetSuspectAndSuspectVehicle(handle, out Suspect, out SuspectVehicle) || Functions.GetCurrentPullover() == null)
        {
            Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
            CleanupEvent();
            return false;
        }

        return true;
    }
    
    internal static void CleanupOutcome()
    {
        Normal("Cleaning up RTS Outcome...");
        PulloverEventHandler.HasEventHappened = false;
        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }
    
    internal Outcome(LHandle handle)
    {
        TrafficStopLHandle = handle;
    }
}