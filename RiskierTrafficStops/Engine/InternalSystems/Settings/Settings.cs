using System.Windows.Forms;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.InternalSystems.Settings;

internal enum ChancesSettingEnum
{
    EStaticChance,
    ERandomChance,
    ECompoundingChance
}

internal static class Settings
{
    internal static readonly Config UserConfig = new();
    internal static IniReflector<Config> IniReflector = new ("plugins/lspdfr/RiskierTrarfficStops.ini");
    
    internal static int Chance = 5;
    private static readonly List<(bool enabled, Type outcome)> AllOutcomes = new();
    internal static Keys GetBackInKey = Keys.Y;
    internal static ChancesSettingEnum ChanceSetting = ChancesSettingEnum.EStaticChance;

    // Event Booleans
    internal static bool GetOutAndShootEnabled = true;
    internal static bool RamEnabled = true;
    internal static bool FleeEnabled = true;
    internal static bool RevEnabled = true;
    internal static bool YellEnabled = true;
    internal static bool YellInCarEnabled = true;
    internal static bool ShootAndFleeEnabled = true;
    internal static bool SpittingEnabled = true;

    internal static void IniFileSetup()
    {
        IniReflector.Read(UserConfig, true);

        ValidateIniValues();
        FilterOutcomes();
    }

    private static void ValidateIniValues()
    {
        if (Chance <= 100) return;
        Normal("Chance value was greater than 100, setting value to 100...");
        Chance = 100;
        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Riskier Traffic Stops", "~b~By Astro",
            "Chance value is ~r~over 100~w~!!");
        Normal("Chance value set to 100");
    }

    internal static void FilterOutcomes()
    {
        Normal("Adding enabled outcomes to enabledOutcomes");
        AllOutcomes.Clear();

        AllOutcomes.Add((GetOutAndShootEnabled, typeof(GetOutAndShoot)));
        AllOutcomes.Add((RamEnabled, typeof(Ramming)));
        AllOutcomes.Add((FleeEnabled, typeof(Flee)));
        AllOutcomes.Add((RevEnabled, typeof(Revving)));
        AllOutcomes.Add((YellEnabled, typeof(Yelling)));
        AllOutcomes.Add((YellInCarEnabled, typeof(YellInCar)));
        AllOutcomes.Add((ShootAndFleeEnabled, typeof(ShootAndFlee)));
        AllOutcomes.Add((SpittingEnabled, typeof(Spitting)));

        PulloverEventHandler.EnabledOutcomes = AllOutcomes.Where(i => i.enabled).Select(i => i.outcome).ToList();

        Normal("----Enabled Outcomes----");
        PulloverEventHandler.EnabledOutcomes.ForEach(i => Normal(
            i.ToString()
                .Substring(
                    i.ToString()
                        .LastIndexOf('.') + 1
                )));
        Normal("----Enabled Outcomes----");
    }
}

[IniReflectorSection("General_Settings")]
[IniReflectorSection("Outcome_Configuration")]
internal class Config
{
    [IniReflectorValue(defaultValue: 5, description: "Chance value for any outcome to happen", name: "Chance")]
    public static int Chance;
    [IniReflectorValue(defaultValue: Keys.Y, description: "Used for certain outcomes, button to make suspect re-enter vehicle (where applicable)")]
    public static Keys GetBackInKey;
    [IniReflectorValue(defaultValue: ChancesSettingEnum.EStaticChance, description: "What chance setting to use", name: "Chance_Setting")]
    public static ChancesSettingEnum ChanceSetting;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true, description:"Whether or not an outcome can happen")]
    public static bool GetOutAndShootEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool RamEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool FleeEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool RevEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool YellEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool YellInCarEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool ShootAndFleeEnabled;
    
    [IniReflectorValue(sectionName: "Outcome_Configuration", defaultValue: true)]
    public static bool SpittingEnabled;
    
}