using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API.ExternalAPIs;
using RiskierTrafficStops.Mod.Outcomes;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using System.Security.Cryptography;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal enum Outcome //Enum is outside class so that it can be referenced anywhere without having to reference the class
{
    GetOutOfCarAndYell,
    GetOutAndShoot,
    FleeFromTrafficStop,
    YellInCar,
    RevEngine,
    RamIntoPlayerVehicle,
    ShootAndFlee,
    Spit,
}

internal static class PulloverEventHandler
{
    private static Type _chosenOutcome;
    internal static bool HasEventHappened;
    private static Type? _lastOutcome;
    private static RNGCryptoServiceProvider _outcomeRng = new RNGCryptoServiceProvider();
    
    internal static List<Type> enabledOutcomes = new ();
    
    internal static void SubscribeToEvents()
    {
        //Subscribing to events
        Normal("Subscribing to: OnPulloverOfficerApproachDriver");
        Events.OnPulloverOfficerApproachDriver += Events_OnPulloverOfficerApproachDriver;
        //\\
        Normal("Subscribing to: OnPulloverDriverStopped");
        Events.OnPulloverDriverStopped += Events_OnPulloverDriverStopped;
        //\\
        Normal("Subscribing to: OnPulloverStarted");
        Events.OnPulloverStarted += Events_OnPulloverStarted;
        //\\
        Normal("Subscribing to: OnPulloverEnded");
        Events.OnPulloverEnded += Events_OnPulloverEnded;
    }

    internal static void UnsubscribeFromEvents()
    {
        Normal("Unsubscribing from events...");
        Events.OnPulloverOfficerApproachDriver -= Events_OnPulloverOfficerApproachDriver;
        Events.OnPulloverDriverStopped -= Events_OnPulloverDriverStopped;
        Events.OnPulloverStarted -= Events_OnPulloverStarted;
        Events.OnPulloverEnded -= Events_OnPulloverEnded;
    }

    private static void Events_OnPulloverStarted(LHandle handle)
    {
        GameFiber.StartNew(() =>
        {
            if (!IaeFunctions.IaeCompatibilityCheck(handle) || Functions.IsCalloutRunning() || API.APIs.DisableRTSForCurrentStop) return;

            HasEventHappened = true;
            ChooseOutcome(handle);
        });
    }

    private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
    {
        HasEventHappened = false;
        API.APIs.DisableRTSForCurrentStop = false;
    }

    private static void Events_OnPulloverDriverStopped(LHandle handle)
    {
        if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !API.APIs.DisableRTSForCurrentStop) { GameFiber.StartNew(() => ChooseOutcome(handle)); }
    }

    private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
    {
        if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !API.APIs.DisableRTSForCurrentStop) { GameFiber.StartNew(() => ChooseOutcome(handle)); }
    }

    /// <summary>
    /// Chooses an outcome from the enabled outcomes
    /// </summary>
    /// <param name="handle">Handle of the current traffic stop</param>
    private static void ChooseOutcome(LHandle handle)
    {
        try
        {
            if (ShouldEventHappen())
            {
                Normal($"HasEventHappened: {HasEventHappened}");
                Normal($"DisableRTSForCurrentStop: {API.APIs.DisableRTSForCurrentStop}");

                if (HasEventHappened) return;
                
                Normal("Choosing Outcome");
                if (enabledOutcomes.Count <= 1)
                {
                    _chosenOutcome = enabledOutcomes[Rndm.Next(enabledOutcomes.Count)];
                }
                else
                {
                    _chosenOutcome = enabledOutcomes[Rndm.Next(enabledOutcomes.Where(i => i != _lastOutcome).ToList().Count)];
                }
                Normal($"Chosen Outcome: {_chosenOutcome}");
                
                _lastOutcome = _chosenOutcome;

                Activator.CreateInstance(_chosenOutcome, args: handle);
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, "PulloverEvents.cs: ChooseEvent()");
            GameFiberHandling.CleanupFibers();
        }
    }
        
    private static bool ShouldEventHappen()
    {
        byte[] randomBytes = new byte[8]; // Using 8 bytes for more randomization ig
        _outcomeRng.GetBytes(randomBytes);
 
        long randomNumber = BitConverter.ToInt64(randomBytes, 0) & 0x7FFFFFFF; // Convert to positive integer

        var convertedChance = randomNumber % 100;
        Normal("Chance: " + convertedChance);
            
        return convertedChance < Settings.Chance;
    }
}