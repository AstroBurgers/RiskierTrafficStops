using RiskierTrafficStops.API.ExternalAPIs;
using RiskierTrafficStops.Engine.InternalSystems.Settings;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class PulloverEventHandler
{
    internal static bool HasEventHappened;

    internal static List<Type> EnabledOutcomes = new();

    private static Type _chosenOutcome;
    private static Type _lastOutcome;

    private static long _currentChance = UserConfig.Chance;

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

    /// <summary>
    /// Chooses an outcome from the enabled outcomes
    /// </summary>
    /// <param name="handle">Handle of the current traffic stop</param>
    /// <param name="onPulloverStarted">Whether the method was triggered from the on pullover started event</param>
    private static void ChooseOutcome(LHandle handle, bool onPulloverStarted = false)
    {
        if (ShouldEventHappen())
        {
            Normal($"DisableRTSForCurrentStop: {DisableRTSForCurrentStop}");
            Normal("Choosing Outcome");

            var filteredOutcomes = EnabledOutcomes
                .Where(o => !onPulloverStarted ||
                            o != typeof(GetOutAndShoot) && o != typeof(Yelling) && o != typeof(GetOutRo) && o != typeof(Spitting))
                .ToList();

            switch (filteredOutcomes.Count)
            {
                // If there are no valid outcomes after filtering, return early.
                case 0:
                    Normal("No valid outcomes available");
                    HasEventHappened = false;
                    return;
                case <= 1:
                    _chosenOutcome = filteredOutcomes[Rndm.Next(filteredOutcomes.Count)];
                    break;
                default:
                {
                    var availableOutcomes = filteredOutcomes.Where(o => o != _lastOutcome).ToList();
                    _chosenOutcome = availableOutcomes[Rndm.Next(availableOutcomes.Count)];
                    break;
                }
            }

            Normal($"Chosen Outcome: {_chosenOutcome}");
            _lastOutcome = _chosenOutcome;

            if (UserConfig.ChanceSetting == ChancesSettingEnum.ECompoundingChance)
            {
                _currentChance = UserConfig.Chance;
            }
            
            Activator.CreateInstance(_chosenOutcome, args: handle);
        }
        else
        {
            if (UserConfig.ChanceSetting == ChancesSettingEnum.ECompoundingChance)
            {
                _currentChance += UserConfig.Chance;
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
        switch (UserConfig.ChanceSetting)
        {
            case ChancesSettingEnum.EStaticChance:
                convertedChance = GenerateChance();
                Normal("Chance: " + convertedChance);
                return convertedChance < UserConfig.Chance;

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