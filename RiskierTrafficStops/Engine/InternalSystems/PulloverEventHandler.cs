using RiskierTrafficStops.API.ExternalAPIs;
using RiskierTrafficStops.Engine.InternalSystems.Settings;
using RiskierTrafficStops.Mod.Outcomes;
using static RiskierTrafficStops.Engine.Helpers.OutcomeChooser;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class PulloverEventHandler
{
    internal static void SubscribeToEvents()
    {
        //Subscribing to events
        Normal("Subscribing to: OnPulloverOfficerApproachDriver");
        Events.OnPulloverOfficerApproachDriver += Events_OnPulloverOfficerApproachDriver;

        Normal("Subscribing to: OnPulloverDriverStopped");
        Events.OnPulloverDriverStopped += Events_OnPulloverDriverStopped;

        Normal("Subscribing to: OnPulloverStarted");
        Events.OnPulloverStarted += Events_OnPulloverStarted;

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
            if (!IaeFunctions.IaeCompatibilityCheck(handle) || Functions.IsCalloutRunning() ||
                DisableRTSForCurrentStop) return;

            GameFiber.WaitWhile(() =>
                MainPlayer.IsAvailable() && MainPlayer.LastVehicle.IsAvailable() && !MainPlayer.LastVehicle.IsSirenOn &&
                Functions.IsPlayerPerformingPullover());

            if (!MainPlayer.IsAvailable() || !MainPlayer.LastVehicle.IsAvailable() ||
                !MainPlayer.LastVehicle.IsSirenOn ||
                !Functions.IsPlayerPerformingPullover()) return;
            HasEventHappened = true;
            ChooseOutcome(handle, true);
        });
    }

    private static void Events_OnPulloverEnded(LHandle pullover, bool normalEnding)
    {
        HasEventHappened = false;
        DisableRTSForCurrentStop = false;
    }

    private static void Events_OnPulloverDriverStopped(LHandle handle)
    {
        if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !DisableRTSForCurrentStop)
        {
            GameFiber.StartNew(() => ChooseOutcome(handle));
        }
    }

    private static void Events_OnPulloverOfficerApproachDriver(LHandle handle)
    {
        if (!HasEventHappened && IaeFunctions.IaeCompatibilityCheck(handle) && !DisableRTSForCurrentStop)
        {
            GameFiber.StartNew(() => ChooseOutcome(handle));
        }
    }
}