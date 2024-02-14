using RiskierTrafficStops.API.ExternalAPIs;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class PulloverEventHandler
{
    internal static bool HasEventHappened;
    
    internal static List<Type> EnabledOutcomes = new();
    
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
            if (!IaeFunctions.IaeCompatibilityCheck(handle) || Functions.IsCalloutRunning() || DisableRTSForCurrentStop) return;

            GameFiber.WaitWhile(() => !MainPlayer.LastVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover());

            if (MainPlayer.LastVehicle.IsSirenOn && Functions.IsPlayerPerformingPullover())
            {
                HasEventHappened = true;
                ChooseOutcome(handle);
            }
        });
    }

    private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
    {
        HasEventHappened = false;
        DisableRTSForCurrentStop = false;
    }

    private static void Events_OnPulloverDriverStopped(LHandle handle)
    {
        if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !DisableRTSForCurrentStop) { GameFiber.StartNew(() => ChooseOutcome(handle)); }
    }

    private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
    {
        if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !DisableRTSForCurrentStop) { GameFiber.StartNew(() => ChooseOutcome(handle)); }
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
                Activator.CreateInstance(typeof(HostageTaking), args: handle);

                // Normal($"DisableRTSForCurrentStop: {DisableRTSForCurrentStop}");
                //
                // Normal("Choosing Outcome");
                // _chosenOutcome = EnabledOutcomes.Count <= 1 ? EnabledOutcomes[Rndm.Next(EnabledOutcomes.Count)] : EnabledOutcomes[Rndm.Next(EnabledOutcomes.Where(i => i != _lastOutcome).ToList().Count)];
                // Normal($"Chosen Outcome: {_chosenOutcome}");
                //
                // _lastOutcome = _chosenOutcome;
                //
                // Activator.CreateInstance(_chosenOutcome, args: handle);
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, "PulloverEvents.cs: ChooseEvent()");
            GameFiberHandling.CleanupFibers();
        }
    }
        
    /// <summary>
    /// Does a chance generation and check to determine if a outcome should happen
    /// </summary>
    /// <returns>True/False</returns>
    private static bool ShouldEventHappen()
    {
        var convertedChance = GenerateChance();
        Normal("Chance: " + convertedChance);
        
        return convertedChance < Chance;
    }
}