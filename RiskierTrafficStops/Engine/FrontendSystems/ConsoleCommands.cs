using Rage.Attributes;

namespace RiskierTrafficStops.Engine.FrontendSystems;

internal static class ConsoleCommands
{
    [ConsoleCommand("Open the Riskier Traffic Stops configuration menu")]
    internal static void RtsOpenConfigMenu()
    {
        if (ConfigMenu.MenuRequirements())
        {
            ConfigMenu.MainMenu.Visible = true;
        }
    }
}