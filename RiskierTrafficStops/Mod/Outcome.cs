namespace RiskierTrafficStops.Mod;

internal abstract class Outcome
{
    internal static Ped Suspect;
    internal static Vehicle SuspectVehicle;
    internal static RelationshipGroup SuspectRelateGroup;
    internal static LHandle PursuitLHandle;
    internal static LHandle TrafficStopLHandle;

    internal static List<Ped> PedsToIgnore = new();
    
    internal virtual void StartOutcome(){}

    internal void RemoveIgnoredPedsAndBlockEvents(List<Ped> peds)
    {
        peds.RemoveAll(ped => ped.IsAvailable() && PedsToIgnore.Contains(ped));
        peds.ForEach(ped => ped.BlockPermanentEvents = true);
    }
    
    internal static bool MeetsRequirements(LHandle handle)
    {
        if (!GetSuspectAndSuspectVehicle(handle, out Suspect, out SuspectVehicle) || Functions.GetCurrentPullover() == null)
        {
            Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
            CleanupOutcome(false);
            return false;
        }

        return true;
    }
    
    
    /// <summary>
    /// Returns the Driver and its vehicle
    /// </summary>
    /// <returns>Ped, Vehicle</returns>
    private static bool GetSuspectAndSuspectVehicle(LHandle handle, out Ped suspect, out Vehicle suspectVehicle)
    {
        Ped driver = null;
        Vehicle driverVehicle = null;
        if (handle is not null && Functions.IsPlayerPerformingPullover() && Functions.GetPulloverSuspect(handle).IsAvailable())
        {
            Normal("Setting up Suspect");
            driver = Functions.GetPulloverSuspect(handle);
            driver.BlockPermanentEvents = true;
        }
        // ReSharper disable once PossibleNullReferenceException
        if (driver.IsAvailable() && driver.IsInAnyVehicle(false) && !driver.IsInAnyPoliceVehicle)
        {
            Normal("Setting up Suspect Vehicle");
            driverVehicle = driver.LastVehicle;
        }
            
        Normal("Returning Suspect & Suspect Vehicle");
        suspect = driver;
        suspectVehicle = driverVehicle;
        return suspect.IsAvailable() && suspectVehicle.IsAvailable();
    }
    
    internal static void CleanupOutcome(bool throwEvent)
    {
        Normal("Cleaning up RTS Outcome...");
        PulloverEventHandler.HasEventHappened = false;
        GameFiberHandling.CleanupFibers();
        if (throwEvent) InvokeEvent(RTSEventType.End);
    }
    
    internal Outcome(LHandle handle)
    {
        TrafficStopLHandle = handle;
    }
}