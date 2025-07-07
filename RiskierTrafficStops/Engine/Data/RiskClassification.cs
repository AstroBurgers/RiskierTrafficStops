using CommonDataFramework.Modules;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using LSPD_First_Response.Engine.Scripting.Entities;
using RiskierTrafficStops.Engine.Helpers;
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
    internal int ViolentScore { get; set; }
    internal int NeutralScore { get; set; }
    internal int SafeScore { get; set; }

    public void Evaluate(PedData suspect, VehicleData vehicle)
    {
        switch (suspect.DriversLicenseState)
        {
            case ELicenseState.Expired or ELicenseState.Unlicensed:
                NeutralScore += 5;
                break;
            case ELicenseState.Suspended:
                ViolentScore += 5;
                break;
        }

        ViolentScore += suspect.TimesStopped;
        NeutralScore += suspect.TimesStopped;
        SafeScore += suspect.TimesStopped;

        if (suspect.Wanted)
        {
            ViolentScore += 10;
            NeutralScore += 5;
        }

        if (vehicle.HasAnyBOLOs)
        {
            var boloCount = vehicle.GetAllBOLOs().Length;
            ViolentScore += 5 * boloCount;
            NeutralScore += 5 * boloCount;
        }

        if (vehicle.IsStolen)
            ViolentScore += 15;

        if (vehicle.Insurance.Status != EDocumentStatus.Valid)
        {
            SafeScore += 5;
            NeutralScore += 5;
        }

        if (vehicle.Registration.Status != EDocumentStatus.Valid)
        {
            SafeScore += 5;
            NeutralScore += 5;
        }

        if (vehicle.Vin.Status == EVinStatus.Scratched)
            ViolentScore += 15;
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

        return filtered.Last().OutcomeType;
    }
}