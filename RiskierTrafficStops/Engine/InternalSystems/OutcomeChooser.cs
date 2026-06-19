using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using RiskierTrafficStops.Engine.Data;
using RiskierTrafficStops.Engine.InternalSystems.Settings;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class OutcomeChooser
{
    internal static bool HasEventHappened;
    internal static List<Type> EnabledOutcomes = [];

    private static Type _chosenOutcome;
    private static Type _lastOutcome;
    private static long _currentChance = UserConfig.Chance;

    /// <summary>
    /// Chooses an outcome from the enabled outcomes.
    /// </summary>
    /// <param name="handle">Handle of the current traffic stop</param>
    /// <param name="onPulloverStarted">Whether the method was triggered from the on pullover started event</param>
    internal static void ChooseOutcome(LHandle handle, bool onPulloverStarted = false)
    {
        if (!ShouldEventHappen(handle))
        {
            if (UserConfig.ChanceSetting == ChancesSetting.ECompoundingChance)
                _currentChance += UserConfig.Chance;

            return;
        }

        Normal($"DisableRTSForCurrentStop: {DisableRTSForCurrentStop}");
        Normal("Choosing Outcome");

        List<Type> filteredOutcomes = EnabledOutcomes
            .Where(o => !onPulloverStarted ||
                        o != typeof(GetOutAndShoot) && o != typeof(Yelling) && o != typeof(GetOutRo))
            .ToList();

        if (filteredOutcomes.Count == 0)
        {
            Normal("No valid outcomes available after filtering");
            HasEventHappened = false;
            return;
        }

        _chosenOutcome = UserConfig.ChanceSetting == ChancesSetting.ESuspectBased
            ? TryGetSuspectBasedOutcome(handle, filteredOutcomes)
            : PickOutcome(filteredOutcomes);

        // Null fallback — suspect-based selection may fail or return null
        if (_chosenOutcome is null)
        {
            Normal("Outcome selection returned null — falling back to random outcome");
            _chosenOutcome = PickOutcome(filteredOutcomes);
        }

        // Final safety check after fallback
        if (_chosenOutcome is null)
        {
            Normal("Fallback outcome selection also returned null — aborting");
            HasEventHappened = false;
            GameFiberHandling.CleanupFibers();
            return;
        }

        Normal($"Chosen Outcome: {_chosenOutcome}");
        _lastOutcome = _chosenOutcome;

        if (UserConfig.ChanceSetting == ChancesSetting.ECompoundingChance)
            _currentChance = UserConfig.Chance;

        Activator.CreateInstance(_chosenOutcome, args: handle);
    }

    /// <summary>
    /// Attempts to pick a weighted outcome based on the suspect's risk profile.
    /// Returns null if the suspect/vehicle data is unavailable or selection fails.
    /// </summary>
    private static Type TryGetSuspectBasedOutcome(LHandle handle, List<Type> filteredOutcomes)
    {
        try
        {
            Ped suspect = Functions.GetPulloverSuspect(handle);
            if (suspect is null || !suspect.Exists() || !suspect.LastVehicle.Exists())
            {
                Normal("Suspect-based selection: suspect or vehicle not available");
                return null;
            }

            PedData suspectData = suspect.GetPedData();
            VehicleData vehicleData = suspect.LastVehicle.GetVehicleData();

            if (suspectData is null || vehicleData is null)
            {
                Normal("Suspect-based selection: ped or vehicle data returned null");
                return null;
            }

            SuspectRiskProfile profile = new();
            profile.Evaluate(suspectData, vehicleData);

            ERiskClassification classification = profile.WeightedClassification(new Random(DateTime.Now.Millisecond));
            Type outcome = SuspectRiskProfile.PickWeightedOutcome(classification, Rndm);

            if (outcome is null)
            {
                Normal("Suspect-based selection: PickWeightedOutcome returned null");
                return null;
            }

            // Ensure the suspect-chosen outcome is actually in the filtered set
            if (!filteredOutcomes.Contains(outcome))
            {
                Normal($"Suspect-based outcome {outcome} not in filtered list — discarding");
                return null;
            }

            return outcome;
        }
        catch (Exception ex)
        {
            Error(new Exception("Suspect-based outcome selection failed (probably due to CDF)", ex));
            return null;
        }
    }

    /// <summary>
    /// Picks a random outcome from the filtered list, avoiding repeating the last outcome where possible.
    /// </summary>
    private static Type PickOutcome(List<Type> filteredOutcomes)
    {
        switch (filteredOutcomes.Count)
        {
            case 0:
                return null;
            case 1:
                return filteredOutcomes[0];
        }

        List<Type> availableOutcomes = filteredOutcomes
            .Where(o => o != _lastOutcome)
            .ToList();

        // If filtering out the last outcome leaves nothing (e.g. only one type existed), use full list
        List<Type> pool = availableOutcomes.Count > 0 ? availableOutcomes : filteredOutcomes;
        return pool[Rndm.Next(pool.Count)];
    }

    /// <summary>
    /// Does a chance generation and check to determine if an outcome should happen.
    /// </summary>
    private static bool ShouldEventHappen(LHandle handle)
    {
        if (EnabledOutcomes.Count == 0) return false;

        ChancesSetting chanceSetting = UserConfig.ChanceSetting;
        long convertedChance = GenerateChance();

        switch (chanceSetting)
        {
            case ChancesSetting.EStaticChance:
                Normal("Chance: " + convertedChance);
                return convertedChance < UserConfig.Chance;

            case ChancesSetting.ECompoundingChance:
                Normal("Chance: " + convertedChance);
                return convertedChance < _currentChance;

            case ChancesSetting.ESuspectBased:
            {
                if (!Functions.IsPlayerPerformingPullover()) return false;

                Ped suspect = Functions.GetPulloverSuspect(handle);
                if (suspect is null || !suspect.Exists() || !suspect.LastVehicle.Exists()) return false;

                PedData pedData = suspect.GetPedData();
                VehicleData vehicleData = suspect.LastVehicle.GetVehicleData();

                if (pedData is null || vehicleData is null) return false;

                SuspectRiskProfile profile = new();
                profile.Evaluate(pedData, vehicleData);

                int totalScore = profile.ViolentScore + profile.NeutralScore + profile.SafeScore;
                if (totalScore == 0) return false;

                long suspectChance = Math.Min(totalScore, 100);
                Normal($"Suspect-Based Total Chance: {totalScore}");
                Normal("Normal Chance: " + convertedChance);
                Normal($"Should Event Happen: {convertedChance < suspectChance}");

                return convertedChance < suspectChance;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(chanceSetting), chanceSetting, "Unhandled chance setting");
        }
    }
}