using System.Windows.Forms;
using RiskierTrafficStops.Mod.Outcomes;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace RiskierTrafficStops.Engine.InternalSystems.Settings;

internal enum ChancesSetting
{
    EStaticChance,
    ESuspectBased,
    ECompoundingChance
}

internal static class Settings
{
    internal static readonly Config UserConfig = new();
    internal static IniReflector<Config> IniReflector = new ("plugins/LSPDFR/RiskierTrafficStops.ini");
    
    private static readonly List<(bool enabled, Type outcome)> AllOutcomes = [];
    
    internal static void IniFileSetup()
    {
        IniReflector.Read(UserConfig, true);

        ValidateIniValues();
        FilterOutcomes();
    }

    private static void ValidateIniValues()
    {
        if (UserConfig.Chance <= 100) return;
        Normal("Chance value was greater than 100, setting value to 100...");
        UserConfig.Chance = 100;
        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Riskier Traffic Stops", "~b~By Astro",
            "Chance value is ~r~over 100~w~!!");
        Normal("Chance value set to 100");
    }

    internal static void FilterOutcomes()
    {
        Normal("Adding enabled outcomes to enabledOutcomes");
        AllOutcomes.Clear();

        AllOutcomes.Add((UserConfig.GetOutAndShootEnabled, typeof(GetOutAndShoot)));
        AllOutcomes.Add((UserConfig.RamEnabled, typeof(Ramming)));
        AllOutcomes.Add((UserConfig.FleeEnabled, typeof(Flee)));
        AllOutcomes.Add((UserConfig.RevEnabled, typeof(Revving)));
        AllOutcomes.Add((UserConfig.YellEnabled, typeof(Yelling)));
        AllOutcomes.Add((UserConfig.YellInCarEnabled, typeof(YellInCar)));
        AllOutcomes.Add((UserConfig.ShootAndFleeEnabled, typeof(ShootAndFlee)));
        AllOutcomes.Add((UserConfig.SpittingEnabled, typeof(Spitting)));
        AllOutcomes.Add((UserConfig.GetOutROEnabled, typeof(GetOutRo)));
        
        OutcomeChooser.EnabledOutcomes = AllOutcomes.Where(i => i.enabled).Select(i => i.outcome).ToList();

        Normal("----Enabled Outcomes----");
        OutcomeChooser.EnabledOutcomes.ForEach(i => Normal(
            i.ToString()
                .Substring(
                    i.ToString()
                        .LastIndexOf('.') + 1
                )));
        Normal("----Enabled Outcomes----");
    }
}

internal class Config
{
    [IniReflectorValue(sectionName: "General_Settings", defaultValue: 15, name: "Chance")]
    public int Chance;
    [IniReflectorValue(sectionName: "General_Settings", defaultValue: Keys.Y)]
    public Keys GetBackInKey;
    [IniReflectorValue(sectionName: "General_Settings", defaultValue: ChancesSetting.EStaticChance, name: "Chance_Setting")]
    public ChancesSetting ChanceSetting;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool GetOutAndShootEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool RamEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool FleeEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool RevEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool YellEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool YellInCarEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool ShootAndFleeEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool SpittingEnabled;

    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public bool GetOutROEnabled;

    // Risk weightings for chancing system
    
    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 5, name: "LicenseExpiredOrUnlicensed")]
    public int LicenseExpiredOrUnlicensedWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 10, name: "LicenseSuspendedWeight")]
    public int LicenseSuspendedWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 1, name: "TimesStoppedWeight")]
    public int TimesStoppedWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 25, name: "WantedViolentWeight")]
    public int WantedViolentWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 10, name: "WantedNeutralWeight")]
    public int WantedNeutralWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 5, name: "BoloWeightPerCount")]
    public int BoloWeightPerCount;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 25, name: "VehicleStolenWeight")]
    public int VehicleStolenWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 5, name: "InvalidInsuranceWeight")]
    public int InvalidInsuranceWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 5, name: "InvalidRegistrationSafeWeight")]
    public int InvalidRegistrationSafeWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 5, name: "InvalidRegistrationNeutralWeight")]
    public int InvalidRegistrationNeutralWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 10, name: "InvalidRegistrationViolentWeight")]
    public int InvalidRegistrationViolentWeight;

    [IniReflectorValue(sectionName: "Chancing_Weights", defaultValue: 20, name: "VinScratchedWeight")]
    public int VinScratchedWeight;
}
