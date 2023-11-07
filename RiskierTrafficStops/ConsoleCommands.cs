using Rage.Attributes;
using RiskierTrafficStops.Systems;

namespace RiskierTrafficStops
{
    internal static class ConsoleCommands
    {
        [ConsoleCommand("Open the Riskier Traffic Stops configuration menu")]
        public static void RtsOpenConfigMenu()
        {
            ConfigMenu.MainMenu.Visible = true;
        }
    }
}