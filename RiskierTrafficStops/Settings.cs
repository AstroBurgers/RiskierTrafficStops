using Rage;
using RiskierTrafficStops.Systems;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RiskierTrafficStops
{
    internal class Settings
    {
        internal static int Chance = 15;
        internal static List<Scenarios> enabledScenarios = new List<Scenarios>();
        internal static Keys GetBackInKey = Keys.Y;
        internal static InitializationFile inifile; // Defining a new INI File

        // Event Booleans
        internal static bool getOutAndShootEnabled = true;
        internal static bool ramEnabled = true;
        internal static bool fleeEnabled = true;
        internal static bool revEnabled = true;
        internal static bool yellEnabled = true;
        internal static bool yellInCarEnabled = true;
        internal static bool shootAndFleeEnabled = true;
        internal static bool spittingEnabled = true;

        // Webhook boolean
        internal static bool autoLogEnabled = true;

        internal static void INIFileSetup()
        {
            inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
            inifile.Create();

            Chance = inifile.ReadInt32("General_Settings", "Chance", Chance);
            GetBackInKey = inifile.ReadEnum("General_Settings", "Keybind", GetBackInKey);

            // Reading event Booleans
            getOutAndShootEnabled = inifile.ReadBoolean("Outcome_Configuration", "Get Out And Shoot Outcome Enabled", getOutAndShootEnabled);
            ramEnabled = inifile.ReadBoolean("Outcome_Configuration", "Ramming Outcome Enabled", ramEnabled);
            fleeEnabled = inifile.ReadBoolean("Outcome_Configuration", "Flee Outcome Enabled", fleeEnabled);
            revEnabled = inifile.ReadBoolean("Outcome_Configuration", "Revving Outcome Enabled", revEnabled);
            yellEnabled = inifile.ReadBoolean("Outcome_Configuration", "Yelling Outcome Enabled", yellEnabled);
            yellInCarEnabled = inifile.ReadBoolean("Outcome_Configuration", "Yelling In Car Outcome Enabled", yellInCarEnabled);
            shootAndFleeEnabled = inifile.ReadBoolean("Outcome_Configuration", "Shoot And Flee Outcome Enabled", shootAndFleeEnabled);
            spittingEnabled = inifile.ReadBoolean("Outcome_Configuration", "Spitting Outcome Enabled", spittingEnabled);

            // Reading Auto Log Boolean
            autoLogEnabled = inifile.ReadBoolean("Auto_Logging", "Automatic Error Reporting Enabled", autoLogEnabled);
            ValidateINIValues();
            FilterOutcomes();
        }

        internal static void ValidateINIValues()
        {
            if (Chance > 100)
            {
                Logger.Debug("Chance value was greater than 100, setting value to 100...");
                Chance = 100;
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Riskier Traffic Stops", "~b~By Astro", "Chance value is ~r~over 100~w~!!");
                Logger.Debug("Chance value set to 100");
            }
        }

        internal static void FilterOutcomes()
        {
            Logger.Debug("Adding enabled scenarios to enabledScenarios");
            enabledScenarios.Clear();
            if (getOutAndShootEnabled) { enabledScenarios.Add(Scenarios.GetOutAndShoot); }
            if (ramEnabled) { enabledScenarios.Add(Scenarios.RamIntoPlayerVehicle); }
            if (fleeEnabled) { enabledScenarios.Add(Scenarios.FleeFromTrafficStop); }
            if (revEnabled) { enabledScenarios.Add(Scenarios.RevEngine); }
            if (yellEnabled) { enabledScenarios.Add(Scenarios.GetOutOfCarAndYell); }
            if (yellInCarEnabled) { enabledScenarios.Add(Scenarios.YellInCar); }
            if (shootAndFleeEnabled) { enabledScenarios.Add(Scenarios.ShootAndFlee); }
            if (spittingEnabled) { enabledScenarios.Add(Scenarios.Spit); }

            Logger.Debug("----Enabled Scenarios----");
            foreach (Scenarios i in enabledScenarios)
            {
                Logger.Debug(i.ToString());
            }
            Logger.Debug("----Enabled Scenarios----");
        }
    }
}