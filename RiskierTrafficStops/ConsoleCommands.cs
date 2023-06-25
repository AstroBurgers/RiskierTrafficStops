using Rage.Attributes;
using RiskierTrafficStops.Systems;

namespace RiskierTrafficStops
{
    internal static class ConsoleCommands
    {
        [ConsoleCommand("Open the Riskier Traffic Stops configuration menu")]
        public static void RTSOpenConfigMenu()
        {
            ConfigMenu.MainMenu.Visible = true;
        }
    }
}
