﻿using Debug = System.Diagnostics.Debug;

namespace RiskierTrafficStops.Mod;

internal abstract class Outcome
{
    internal static Ped Suspect;
    internal static Vehicle SuspectVehicle;
    internal static RelationshipGroup SuspectRelateGroup;
    private static LHandle _trafficStopLHandle;
    internal static Outcome ActiveOutcome;

    internal static readonly List<Ped> PedsToIgnore = [];

    internal static void RemoveIgnoredPedsAndBlockEvents(ref List<Ped> peds)
    {
        if (Suspect.IsAvailable() && PedsToIgnore.Contains(Suspect))
        {
            CleanupOutcome(true);
        }
        
        peds.RemoveAll(ped => ped.IsAvailable() && PedsToIgnore.Contains(ped));
        peds.ForEach(ped => ped.BlockPermanentEvents = true);
    }
    
    internal static bool MeetsRequirements(LHandle handle)
    {
        if (GetSuspectAndSuspectVehicle(handle, out Suspect, out SuspectVehicle) &&
            Functions.GetCurrentPullover() != null) return true;
        Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
        CleanupOutcome(false);
        return false;

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

        Debug.Assert(driver != null, nameof(driver) + " != null");
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
        OutcomeChooser.HasEventHappened = false;
        GameFiberHandling.CleanupFibers();
        if (throwEvent) InvokeEvent(RTSEventType.End);
    }

    protected static void TryStartOutcomeFiber(ThreadStart fiberStartMethod)
    {
        try
        {
            if (!MeetsRequirements(_trafficStopLHandle)) return;

            var fiber = GameFiber.StartNew(fiberStartMethod);
            GameFiberHandling.OutcomeGameFibers.Add(fiber);
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e);
            CleanupOutcome(true);
        }
    }

    
    internal Outcome(LHandle handle)
    {
        _trafficStopLHandle = handle;
        ActiveOutcome = this;
    }
}