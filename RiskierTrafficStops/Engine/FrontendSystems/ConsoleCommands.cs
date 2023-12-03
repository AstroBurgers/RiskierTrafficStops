using Rage.Attributes;

namespace RiskierTrafficStops.Engine.FrontendSystems
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