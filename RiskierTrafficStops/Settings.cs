using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // Event Booleans are enabled
        internal static bool GOAS = true;
        internal static bool Ram = true;
        internal static bool Flee = true;
        internal static bool Rev = true;
        internal static bool Yell = true;
        internal static bool YIC = true;
        internal static bool SAF = true;

        internal static void INIFile()
        {
            inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
            inifile.Create();

            Chance = inifile.ReadInt32("Settings", "Chance", 15);
            GetBackIn = inifile.ReadEnum("Settings", "Keybind", GetBackIn);

            // Checking if an event is enabled
            GOAS = inifile.ReadBoolean("Settings", "Get Out And Shoot Outcome enabled", GOAS);
            Ram = inifile.ReadBoolean("Settings", "Ramming Oucome enabled", Ram);
            Flee = inifile.ReadBoolean("Settings", "Flee Outcome enabled", Flee);
            Rev = inifile.ReadBoolean("Settings", "Revving Outcome enabled", Rev);
            Yell = inifile.ReadBoolean("Settings", "Yelling Outcome enabled", Yell);
            YIC = inifile.ReadBoolean("Settings", "Yelling Car in Outcome enabled", YIC);
            SAF = inifile.ReadBoolean("Settings", "Shoot And Flee Outcome enabled", YIC);

            FilterOutcomes();
        }

        internal static void FilterOutcomes()
        {
            if (GOAS) { enabledScenarios.Add(Main.Scenarios.Shoot); }
            if (Ram) { enabledScenarios.Add(Main.Scenarios.RamIntoYou); }
            if (Flee) { enabledScenarios.Add(Main.Scenarios.Run); }
            if (Rev) { enabledScenarios.Add(Main.Scenarios.RevEngine); }
            if (Yell) { enabledScenarios.Add(Main.Scenarios.Yell); }
            if (YIC) { enabledScenarios.Add(Main.Scenarios.YellInCar); }
            if (SAF) { enabledScenarios.Add(Main.Scenarios.ShootAndFlee); }
        }
    }
}
