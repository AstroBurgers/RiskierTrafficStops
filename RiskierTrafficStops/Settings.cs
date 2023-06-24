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

        // Stage Booleans
        internal static bool onOfficerExitsVehicle = true;
        internal static bool onDriverStopped = true;
        internal static bool onPulloverStarted = true;

        // Webhook boolean
        internal static bool autoLogEnabled = true;

        internal static void INIFileSetup()
        {
            inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
            inifile.Create();

            Chance = inifile.ReadInt32("Settings", "Chance", 15);
            GetBackIn = inifile.ReadEnum("Settings", "Keybind", GetBackIn);

            // Reading event Booleans
            getOutAndShootEnabled = inifile.ReadBoolean("Settings", "Get Out And Shoot Outcome enabled", getOutAndShootEnabled);
            ramEnabled = inifile.ReadBoolean("Settings", "Ramming Oucome enabled", ramEnabled);
            fleeEnabled = inifile.ReadBoolean("Settings", "Flee Outcome enabled", fleeEnabled);
            revEnabled = inifile.ReadBoolean("Settings", "Revving Outcome enabled", revEnabled);
            yellEnabled = inifile.ReadBoolean("Settings", "Yelling Outcome enabled", yellEnabled);
            yellInCarEnabled = inifile.ReadBoolean("Settings", "Yelling Car in Outcome enabled", yellInCarEnabled);
            shootAndFleeEnabled = inifile.ReadBoolean("Settings", "Shoot And Flee Outcome enabled", yellInCarEnabled);
            
            // Reading Auto Log Boolean
            autoLogEnabled = inifile.ReadBoolean("Settings", "Automatic Error Reporting enabled", autoLogEnabled);

            // Reading Stage Booleans
            onDriverStopped = inifile.ReadBoolean("Settings", "On Driver Stopped", onDriverStopped);
            onOfficerExitsVehicle = inifile.ReadBoolean("Settings", "On Officer Exits Vehicle", onOfficerExitsVehicle);
            onPulloverStarted = inifile.ReadBoolean("Settings", "On Pullover Started", onPulloverStarted);

            FilterOutcomes();
        }

        internal static void FilterOutcomes()
        {
            Logger.Debug("Adding enabled Scenarios to enabledScenarios");
            if (getOutAndShootEnabled) { enabledScenarios.Add(Main.Scenarios.GetOutAndShoot); }
            if (ramEnabled) { enabledScenarios.Add(Main.Scenarios.RamIntoPlayerVehicle); }
            if (fleeEnabled) { enabledScenarios.Add(Main.Scenarios.FleeFromTrafficStop); }
            if (revEnabled) { enabledScenarios.Add(Main.Scenarios.RevEngine); }
            if (yellEnabled) { enabledScenarios.Add(Main.Scenarios.GetOutOfCarAndYell); }
            if (yellInCarEnabled) { enabledScenarios.Add(Main.Scenarios.YellInCar); }
            if (shootAndFleeEnabled) { enabledScenarios.Add(Main.Scenarios.ShootAndFlee); }
        }
    }
}
