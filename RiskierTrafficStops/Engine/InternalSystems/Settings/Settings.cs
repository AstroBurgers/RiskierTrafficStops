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
    internal static int Chance = 5;
    private static readonly List<(bool enabled, Type outcome)> AllOutcomes = new();
    internal static Keys GetBackInKey = Keys.Y;
    internal static InitializationFile Inifile; // Defining a new INI File
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
        Inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
        Inifile.Create();

        Chance = Inifile.ReadInt32("General_Settings", "Chance", Chance);
        GetBackInKey = Inifile.ReadEnum("General_Settings", "Keybind", GetBackInKey);
        ChanceSetting = Inifile.ReadEnum("General_Settings", "Chance_Setting", ChanceSetting);

        // Reading event Booleans
        GetOutAndShootEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Get Out And Shoot Outcome Enabled",
            GetOutAndShootEnabled);
        RamEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Ramming Outcome Enabled", RamEnabled);
        FleeEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Flee Outcome Enabled", FleeEnabled);
        RevEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Revving Outcome Enabled", RevEnabled);
        YellEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Yelling Outcome Enabled", YellEnabled);
        YellInCarEnabled =
            Inifile.ReadBoolean("Outcome_Configuration", "Yelling In Car Outcome Enabled", YellInCarEnabled);
        ShootAndFleeEnabled =
            Inifile.ReadBoolean("Outcome_Configuration", "Shoot And Flee Outcome Enabled", ShootAndFleeEnabled);
        SpittingEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Spitting Outcome Enabled", SpittingEnabled);

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