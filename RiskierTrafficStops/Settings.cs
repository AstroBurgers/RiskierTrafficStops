using Rage;
using RiskierTrafficStops.Systems;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Main;

namespace RiskierTrafficStops
{
    internal class Settings
    {
        internal static int Chance = 15;
        internal static List<Scenarios> enabledScenarios = new List<Scenarios>();
        internal static Keys GetBackIn = Keys.Y;
        internal static InitializationFile inifile; // Defining a new INI File

        // Event Booleans
        internal static bool getOutAndShootEnabled = true;
        internal static bool ramEnabled = true;
        internal static bool fleeEnabled = true;
        internal static bool revEnabled = true;
        internal static bool yellEnabled = true;
        internal static bool yellInCarEnabled = true;
        internal static bool shootAndFleeEnabled = true;

        // Webhook boolean
        internal static bool autoLogEnabled = true;

        internal static void INIFileSetup()
        {
            inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
            inifile.Create();

            Chance = inifile.ReadInt32("Settings", "Chance", Chance);
            GetBackIn = inifile.ReadEnum("Settings", "Keybind", GetBackIn);

            // Reading event Booleans
            getOutAndShootEnabled = inifile.ReadBoolean("Settings", "Get Out And Shoot Outcome Enabled", getOutAndShootEnabled);
            ramEnabled = inifile.ReadBoolean("Settings", "Ramming Outcome Enabled", ramEnabled);
            fleeEnabled = inifile.ReadBoolean("Settings", "Flee Outcome Enabled", fleeEnabled);
            revEnabled = inifile.ReadBoolean("Settings", "Revving Outcome Enabled", revEnabled);
            yellEnabled = inifile.ReadBoolean("Settings", "Yelling Outcome Enabled", yellEnabled);
            yellInCarEnabled = inifile.ReadBoolean("Settings", "Yelling In Car Outcome Enabled", yellInCarEnabled);
            shootAndFleeEnabled = inifile.ReadBoolean("Settings", "Shoot And Flee Outcome Enabled", shootAndFleeEnabled);

            // Reading Auto Log Boolean
            autoLogEnabled = inifile.ReadBoolean("Settings", "Automatic Error Reporting Enabled", autoLogEnabled);

            FilterOutcomes();
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

            Logger.Debug("----Enabled Scenarios----");
            foreach (Scenarios i in enabledScenarios)
            {
                Logger.Debug(i.ToString());
            }
            Logger.Debug("----Enabled Scenarios----");
        }
    }
}
