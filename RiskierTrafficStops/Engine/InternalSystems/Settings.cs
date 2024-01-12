using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rage;
using RiskierTrafficStops.Mod.Outcomes;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class Settings
{
    internal static int Chance = 15;
    internal static List<(bool enabled, Type outcome)> AllOutcomes = new();
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
            
        ValidateIniValues();
        FilterOutcomes();
    }

    private static void ValidateIniValues()
    {
        if (Chance <= 100) return;
        Logger.Normal("Chance value was greater than 100, setting value to 100...");
        Chance = 100;
        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Riskier Traffic Stops", "~b~By Astro", "Chance value is ~r~over 100~w~!!");
        Logger.Normal("Chance value set to 100");
    }

    internal static void FilterOutcomes()
    {
        Logger.Normal("Adding enabled outcomes to enabledOutcomes");
        AllOutcomes.Clear();
        
        AllOutcomes.Add((GetOutAndShootEnabled, typeof(GetOutAndShoot)));
        AllOutcomes.Add((RamEnabled, typeof(Ramming)));
        AllOutcomes.Add((FleeEnabled, typeof(Flee)));
        AllOutcomes.Add((RevEnabled, typeof(Revving)));
        AllOutcomes.Add((YellEnabled, typeof(Yelling)));
        AllOutcomes.Add((YellInCarEnabled, typeof(YellInCar)));
        AllOutcomes.Add((ShootAndFleeEnabled, typeof(ShootAndFlee)));
        AllOutcomes.Add((SpittingEnabled, typeof(Spitting)));

        PulloverEventHandler.enabledOutcomes = AllOutcomes.Where(i => i.enabled).Select(i => i.outcome).ToList();
        
        Logger.Normal("----Enabled Outcomes----");
        PulloverEventHandler.enabledOutcomes.ForEach(i => Logger.Normal(i.ToString()));
        Logger.Normal("----Enabled Outcomes----");
    }
}