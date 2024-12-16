using RiskierTrafficStops.API.ExternalAPIs;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class PulloverEventHandler
{
    internal static bool HasEventHappened;

    internal static List<Type> EnabledOutcomes = new();

    private static Type _chosenOutcome;
    private static Type _lastOutcome;

    private static long _currentChance = Chance;

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
            ChooseOutcome(handle);
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

    /// <summary>
    /// Chooses an outcome from the enabled outcomes
    /// </summary>
    /// <param name="handle">Handle of the current traffic stop</param>
    private static void ChooseOutcome(LHandle handle)
    {
        if (ShouldEventHappen())
        {
            Normal($"DisableRTSForCurrentStop: {DisableRTSForCurrentStop}");

            Normal("Choosing Outcome");
            _chosenOutcome = EnabledOutcomes.Count <= 1
                ? EnabledOutcomes[Rndm.Next(EnabledOutcomes.Count)]
                : EnabledOutcomes[Rndm.Next(EnabledOutcomes.Where(i => i != _lastOutcome).ToList().Count)];
            Normal($"Chosen Outcome: {_chosenOutcome}");

            _lastOutcome = _chosenOutcome;

            if (ChanceSetting == ChancesSettingEnum.ECompoundingChance)
            {
                _currentChance = Chance;
            }

            Activator.CreateInstance(_chosenOutcome, args: handle);
        }
        else
        {
            if (ChanceSetting == ChancesSettingEnum.ECompoundingChance)
            {
                _currentChance += Chance;
            }
        }
    }

    /// <summary>
    /// Does a chance generation and check to determine if an outcome should happen
    /// </summary>
    /// <returns>True/False</returns>
    private static bool ShouldEventHappen()
    {
        if (EnabledOutcomes.Count == 0) return false;
        long convertedChance = 0;
        switch (ChanceSetting)
        {
            case ChancesSettingEnum.EStaticChance:
                convertedChance = GenerateChance();
                Normal("Chance: " + convertedChance);
                return convertedChance < Chance;

            case ChancesSettingEnum.ERandomChance:
                convertedChance = GenerateChance();
                Normal("Chance: " + convertedChance);
                return convertedChance < GenerateChance();

            case ChancesSettingEnum.ECompoundingChance:
                convertedChance = GenerateChance();
                Normal("Chance: " + convertedChance);
                return convertedChance < _currentChance;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}