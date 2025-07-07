using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using RiskierTrafficStops.Engine.Data;
using RiskierTrafficStops.Engine.InternalSystems.Settings;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.Helpers;

internal static class OutcomeChooser
{
    internal static bool HasEventHappened;

    internal static List<Type> EnabledOutcomes = [];

    private static Type _chosenOutcome;
    private static Type _lastOutcome;

    private static long _currentChance = UserConfig.Chance;

    /// <summary>
    /// Chooses an outcome from the enabled outcomes
    /// </summary>
    /// <param name="handle">Handle of the current traffic stop</param>
    /// <param name="onPulloverStarted">Whether the method was triggered from the on pullover started event</param>
    internal static void ChooseOutcome(LHandle handle, bool onPulloverStarted = false)
    {
        if (ShouldEventHappen())
        {
            Normal($"DisableRTSForCurrentStop: {DisableRTSForCurrentStop}");
            Normal("Choosing Outcome");

            if (UserConfig.ChanceSetting == ChancesSettingEnum.ESuspectBased)
            {
                var suspectData = Functions.GetPulloverSuspect(handle).GetPedData();
                var vehicleData = Functions.GetPulloverSuspect(handle).LastVehicle.GetVehicleData();
                var profile = new SuspectRiskProfile();
                profile.Evaluate(suspectData, vehicleData);

                var classification = profile.WeightedClassification(new Random());
                _chosenOutcome =
                    SuspectRiskProfile.PickWeightedOutcome(classification, Rndm);

            }
            
            var filteredOutcomes = EnabledOutcomes
                .Where(o => !onPulloverStarted ||
                            o != typeof(GetOutAndShoot) && o != typeof(Yelling) && o != typeof(GetOutRo) &&
                            o != typeof(Spitting))
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

            case ChancesSettingEnum.ESuspectBased:
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