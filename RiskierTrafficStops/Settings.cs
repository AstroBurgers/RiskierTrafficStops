using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RiskierTrafficStops
{
    internal class Settings
    {
        internal static int Chance = 15;
        internal static Keys GetBackIn = Keys.Y;
        internal static InitializationFile inifile; // Defining a new INI File

        // If events are enabled
        internal static bool GOAS = true;
        internal static bool Ram = true;
        internal static bool Flee = true;
        internal static bool Rev = true;
        internal static bool Yell = true;
        internal static bool YIC = true;

        internal static void INIFile()
        {
            inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
            inifile.Create();

            Chance = inifile.ReadInt32("Settings", "Chance", 15);
            GetBackIn = inifile.ReadEnum("Settings", "Keybind", GetBackIn);

            // Bools for events
            GOAS = inifile.ReadBoolean("Settings", "Get Out And Shoot Outcome enabled", GOAS);
            Ram = inifile.ReadBoolean("Settings", "Ramming Oucome enabled", Ram);
            Flee = inifile.ReadBoolean("Settings", "Flee Outcome enabled", Flee);
            Rev = inifile.ReadBoolean("Settings", "Revving Outcome enabled", Rev);
            Yell = inifile.ReadBoolean("Settings", "Yelling Outcome enabled", Yell);
            YIC = inifile.ReadBoolean("Settings", "Yelling Car in Outcome enabled", YIC);
        }
    }
}
