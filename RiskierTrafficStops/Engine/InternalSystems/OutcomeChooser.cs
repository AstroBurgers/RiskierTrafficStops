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
        if (ShouldEventHappen(handle))
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
    private static bool ShouldEventHappen(LHandle handle)
    {
        if (EnabledOutcomes.Count == 0) return false;

        var convertedChance = GenerateChance();

        switch (UserConfig.ChanceSetting)
        {
            case ChancesSettingEnum.EStaticChance:
                Normal("Chance: " + convertedChance);
                return convertedChance < UserConfig.Chance;

            case ChancesSettingEnum.ECompoundingChance:
                Normal("Chance: " + convertedChance);
                return convertedChance < _currentChance;

            case ChancesSettingEnum.ESuspectBased:
            {
                if (!Functions.IsPlayerPerformingPullover()) return false;
                var suspect = Functions.GetPulloverSuspect(handle);
                if (!suspect.Exists()) return false;

                var pedData = suspect.GetPedData();
                var vehicleData = suspect.LastVehicle.GetVehicleData();

                var profile = new SuspectRiskProfile();
                profile.Evaluate(pedData, vehicleData);

                var totalScore = profile.ViolentScore + profile.NeutralScore + profile.SafeScore;
                if (totalScore == 0)
                    return false; // No factors, no chance

                // Convert score to probability
                // Normalize to 0–100 range for fair comparison with GenerateChance()
                // Example: If totalScore is 100, full 100% chance
                long suspectChance = Math.Min(totalScore, 100);
                Normal($"Suspect-Based Total Chance: {totalScore}");
                Normal("Normal Chance: " + convertedChance);
                Normal($"Should Event Happen: {convertedChance < suspectChance}");
                
                return convertedChance < suspectChance;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}