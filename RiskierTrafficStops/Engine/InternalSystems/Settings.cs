using System.Collections.Generic;
using System.Windows.Forms;
using Rage;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    internal static class Settings
    {
        internal static int Chance = 15;
        internal static readonly List<Scenario> EnabledScenarios = new();
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

        // Webhook boolean
        internal static bool AutoLogEnabled = true;

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

            // Reading Auto Log Boolean
            AutoLogEnabled = Inifile.ReadBoolean("Auto_Logging", "Automatic Error Reporting Enabled", AutoLogEnabled);
            ValidateIniValues();
            FilterOutcomes();
        }

        private static void ValidateIniValues()
        {
            if (Chance <= 100) return;
            Logger.Debug("Chance value was greater than 100, setting value to 100...");
            Chance = 100;
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Riskier Traffic Stops", "~b~By Astro", "Chance value is ~r~over 100~w~!!");
            Logger.Debug("Chance value set to 100");
        }

        internal static void FilterOutcomes()
        {
            Logger.Debug("Adding enabled scenarios to enabledScenarios");
            EnabledScenarios.Clear();
            if (GetOutAndShootEnabled) { EnabledScenarios.Add(Scenario.GetOutAndShoot); }
            if (RamEnabled) { EnabledScenarios.Add(Scenario.RamIntoPlayerVehicle); }
            if (FleeEnabled) { EnabledScenarios.Add(Scenario.FleeFromTrafficStop); }
            if (RevEnabled) { EnabledScenarios.Add(Scenario.RevEngine); }
            if (YellEnabled) { EnabledScenarios.Add(Scenario.GetOutOfCarAndYell); }
            if (YellInCarEnabled) { EnabledScenarios.Add(Scenario.YellInCar); }
            if (ShootAndFleeEnabled) { EnabledScenarios.Add(Scenario.ShootAndFlee); }
            if (SpittingEnabled) { EnabledScenarios.Add(Scenario.Spit); }

            Logger.Debug("----Enabled Scenarios----");
            foreach (var i in EnabledScenarios)
            {
                // ReSharper disable once HeapView.BoxingAllocation
                Logger.Debug(i.ToString());
            }
            Logger.Debug("----Enabled Scenarios----");
        }
    }
}