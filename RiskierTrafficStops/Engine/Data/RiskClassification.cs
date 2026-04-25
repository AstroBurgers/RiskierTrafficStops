using CommonDataFramework.Modules;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using LSPD_First_Response.Engine.Scripting.Entities;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.Data;

internal enum ERiskClassification
{
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
        // make sure neither object is null
        if (suspect is null || vehicle is null)
        {
            ViolentScore = 0;
            NeutralScore = 0;
            SafeScore = 0;
            return;
        }
        
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
        if (total <= 0)
            throw new InvalidOperationException("Total weight must be greater than 0.");

        var roll = rng.Next(0, total);

        return roll < ViolentScore ? ERiskClassification.Violent : ERiskClassification.Neutral;
    }

    private static readonly Dictionary<ERiskClassification, List<(Type OutcomeType, int Weight)>> OutcomeWeights = new()
    {
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
            Normal($"No enabled outcomes for classification {classification}, returning null");
            return null;
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
        Normal($"Filtered outcomes for classification {classification} was empty, returning null");
        return null;
    }
}