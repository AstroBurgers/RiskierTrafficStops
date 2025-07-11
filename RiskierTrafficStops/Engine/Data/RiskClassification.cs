﻿using CommonDataFramework.Modules;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using LSPD_First_Response.Engine.Scripting.Entities;
using RiskierTrafficStops.Engine.InternalSystems.Settings;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.Data;

internal enum ERiskClassification
{
    Safe,
    Neutral,
    Violent
}

internal class SuspectRiskProfile
{
    public int ViolentScore { get; private set; }
    public int NeutralScore { get; private set; }
    public int SafeScore { get; private set; }

    internal void Evaluate(PedData suspect, VehicleData vehicle)
    {
        var config = UserConfig;

        switch (suspect.DriversLicenseState)
        {
            case ELicenseState.Expired or ELicenseState.Unlicensed:
                NeutralScore += config.LicenseExpiredOrUnlicensedWeight;
                break;
            case ELicenseState.Suspended:
                ViolentScore += config.LicenseSuspendedWeight;
                break;
        }

        ViolentScore += config.TimesStoppedWeight * suspect.TimesStopped;
        NeutralScore += config.TimesStoppedWeight * suspect.TimesStopped;
        SafeScore += config.TimesStoppedWeight * suspect.TimesStopped;

        if (suspect.Wanted)
        {
            ViolentScore += config.WantedViolentWeight;
            NeutralScore += config.WantedNeutralWeight;
        }

        if (vehicle.HasAnyBOLOs)
        {
            var boloCount = vehicle.GetAllBOLOs().Length;
            ViolentScore += config.BoloWeightPerCount * boloCount;
            NeutralScore += config.BoloWeightPerCount * boloCount;
        }

        if (vehicle.IsStolen)
            ViolentScore += config.VehicleStolenWeight;

        if (vehicle.Insurance.Status != EDocumentStatus.Valid)
        {
            SafeScore += config.InvalidInsuranceWeight;
            NeutralScore += config.InvalidInsuranceWeight;
        }

        if (vehicle.Registration.Status != EDocumentStatus.Valid)
        {
            SafeScore += config.InvalidRegistrationSafeWeight;
            NeutralScore += config.InvalidRegistrationNeutralWeight;
            ViolentScore += config.InvalidRegistrationViolentWeight;
        }

        if (vehicle.Vin.Status == EVinStatus.Scratched)
            ViolentScore += config.VinScratchedWeight;
    }

    internal ERiskClassification WeightedClassification(Random rng)
    {
        var total = ViolentScore + NeutralScore + SafeScore;
        var roll = rng.Next(0, total);

        if (roll < ViolentScore)
            return ERiskClassification.Violent;
        return roll < ViolentScore + NeutralScore ? ERiskClassification.Neutral : ERiskClassification.Safe;
    }

    private static readonly Dictionary<ERiskClassification, List<(Type OutcomeType, int Weight)>> OutcomeWeights = new()
    {
        [ERiskClassification.Safe] =
        [
            (typeof(YellInCar), 70),
            (typeof(Spitting), 30)
        ],
        [ERiskClassification.Neutral] =
        [
            (typeof(GetOutRo), 50),
            (typeof(Yelling), 30),
            (typeof(Revving), 20)
        ],
        [ERiskClassification.Violent] =
        [
            (typeof(Flee), 35),
            (typeof(Ramming), 25),
            (typeof(GetOutAndShoot), 20),
            (typeof(ShootAndFlee), 20)
        ]
    };

    internal static Type PickWeightedOutcome(ERiskClassification classification, Random rng)
    {
        var pool = OutcomeWeights[classification];

        var filtered = pool
            .Where(entry => OutcomeChooser.EnabledOutcomes.Contains(entry.OutcomeType))
            .ToList();

        if (filtered.Count == 0)
        {
            Normal($"No enabled outcomes for classification {classification}, using fallback");
            return typeof(YellInCar); // or a safer neutral fallback
        }

        var totalWeight = filtered.Sum(x => x.Weight);
        var roll = rng.Next(0, totalWeight);

        var cumulative = 0;
        foreach (var (outcomeType, weight) in filtered)
        {
            cumulative += weight;
            if (roll < cumulative)
                return outcomeType;
        }

        if (filtered.Count != 0) return filtered[filtered.Count - 1].OutcomeType;
        Normal($"Filtered outcomes for classification {classification} was empty, using fallback");
        return typeof(YellInCar);
    }
}