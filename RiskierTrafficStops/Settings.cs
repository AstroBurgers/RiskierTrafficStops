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

        internal static void INIFile()
        {
            inifile = new InitializationFile(@"Plugins/Lspdfr/RiskierTrafficStops.ini");
            inifile.Create();

            Chance = inifile.ReadInt32("Settings", "Chance", 15);
            GetBackIn = inifile.ReadEnum("Settings", "Keybind", GetBackIn);
        }
    }
}
