using System.Windows.Forms;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal enum ChancesSettingEnum
{
    EStaticChance,
    ERandomChance,
    ECompoundingChance
}

internal static class Settings
{
    internal static int Chance = 15;
    private static readonly List<(bool enabled, Type outcome)> AllOutcomes = new();
    internal static Keys GetBackInKey = Keys.Y;
    internal static InitializationFile Inifile; // Defining a new INI File

    // Event Booleans
    internal static bool GetOutAndShootEnabled = true;
    internal static bool RamEnabled = true;
    internal static bool FleeEnabled = true;
    internal static bool RevEnabled = true;
    internal static bool YellEnabled = true;
    internal static bool YellInCarEnabled = true;
    internal static bool ShootAndFleeEnabled = true;
    internal static bool SpittingEnabled = true;
    internal static bool HostageTakingEnabled = true;
    
    // doubles
    internal static double IsSuicidalChance = 40;
    internal static double WantsToSurviveChance = 30;
    internal static double WantsToDieBieCopChance = 25;
    internal static double HatesHostageChance = 20;
    internal static double IsTerroristChance = 10;
    
    internal static void IniFileSetup()
    {
        Inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
        Inifile.Create();

        Chance = Inifile.ReadInt32("General_Settings", "Chance", Chance);
        GetBackInKey = Inifile.ReadEnum("General_Settings", "Keybind", GetBackInKey);

        // Reading event Booleans
        GetOutAndShootEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Get Out And Shoot Outcome Enabled", GetOutAndShootEnabled);
        RamEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Ramming Outcome Enabled", RamEnabled);
        FleeEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Flee Outcome Enabled", FleeEnabled);
        RevEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Revving Outcome Enabled", RevEnabled);
        YellEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Yelling Outcome Enabled", YellEnabled);
        YellInCarEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Yelling In Car Outcome Enabled", YellInCarEnabled);
        ShootAndFleeEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Shoot And Flee Outcome Enabled", ShootAndFleeEnabled);
        SpittingEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Spitting Outcome Enabled", SpittingEnabled);
        HostageTakingEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Hostage Taking Outcome Enabled", HostageTakingEnabled);  
        
        // Reading Doubles [Hostage_Situation_Config]
        IsSuicidalChance = Inifile.ReadDouble("Hostage_Situation_Config", "IsSuicidal", IsSuicidalChance);
        WantsToSurviveChance = Inifile.ReadDouble("Hostage_Situation_Config", "WantsToSurvive", WantsToSurviveChance);
        WantsToDieBieCopChance = Inifile.ReadDouble("Hostage_Situation_Config", "WantsToDieByCop", WantsToDieBieCopChance);
        HatesHostageChance = Inifile.ReadDouble("Hostage_Situation_Config", "HatesHostage", HatesHostageChance);
        IsTerroristChance = Inifile.ReadDouble("Hostage_Situation_Config", "IsTerrorist", IsTerroristChance);
        
        ValidateIniValues();
        FilterOutcomes();
    }

    private static void ValidateIniValues()
    {
        if (Chance <= 100) return;
        Normal("Chance value was greater than 100, setting value to 100...");
        Chance = 100;
        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Riskier Traffic Stops", "~b~By Astro", "Chance value is ~r~over 100~w~!!");
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
        AllOutcomes.Add((HostageTakingEnabled, typeof(HostageTaking)));
        
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