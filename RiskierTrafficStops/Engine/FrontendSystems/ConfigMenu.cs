using System.Drawing;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;

namespace RiskierTrafficStops.Engine.FrontendSystems;

internal static class ConfigMenu
{
    private static readonly UIMenuNumericScrollerItem<int> SetChance = new("Chance", "Chance that an event happens", 0, 100, 1);
    private static readonly UIMenuListScrollerItem<bool> GoasOutcomeEnabled = new("Get Out And Shoot", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> YicOutcomeEnabled = new("Yell In Car", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> YellOutcomeEnabled = new("Yell", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> RiyOutcomeEnabled = new("Ram Into You", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> FleeOutcomeEnabled = new("Flee", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> RevOutcomeEnabled = new("Rev Engine", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> SafOutcomeEnabled = new("Shoot And Flee", "Enable Or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> SpitEnabled = new("Spitting", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuListScrollerItem<bool> HostageTakingEnabled = new("Hostage Taking", "Enable or disable this outcome", new[] { true, false });
    private static readonly UIMenuItem SaveToIni = new("Save To INI", "Saves the current values to the INI file and reloads the INI");

    internal static readonly UIMenu MainMenu = new("RTS Config", "Configure Riskier Traffic Stops");

    private static readonly MenuPool MainMenuPool = new();

    internal static void CreateMenu()
    {
            Normal("Creating Menu...");
            MainMenuPool.Add(MainMenu);
            MainMenu.MouseControlsEnabled = false;
            MainMenu.AllowCameraMovement = true;
            TextStyle style = new(TextFont.ChaletComprimeCologne, TextStyle.Current.Color, 1f, TextJustification.Center);
            MainMenu.TitleStyle = style;

            Normal("Adding Items to Menu");

            MainMenu.AddItems(SetChance, SafOutcomeEnabled, GoasOutcomeEnabled, YicOutcomeEnabled, RiyOutcomeEnabled, FleeOutcomeEnabled, RevOutcomeEnabled, YellOutcomeEnabled, SpitEnabled, HostageTakingEnabled, SaveToIni);
            SaveToIni.BackColor = Color.Green;

            MainMenu.OnItemSelect += (_, selectedItem, _) => //Easier way to do simple things in RNUI that don't require a lot of code
            {
                if (selectedItem.Equals(SaveToIni))
                {
                    AppendToIni();
                }
            };

            GameFiber.StartNew(MenuPoolProcess);
            SetupMenu();
        }

    private static void SetupMenu()
    {
            Normal("Assigning Menu values to their respective INI Values...");
            SetChance.Value = Chance;
            YellOutcomeEnabled.SelectedItem = YellEnabled;
            GoasOutcomeEnabled.SelectedItem = GetOutAndShootEnabled;
            YicOutcomeEnabled.SelectedItem = YellInCarEnabled;
            RiyOutcomeEnabled.SelectedItem = RamEnabled;
            FleeOutcomeEnabled.SelectedItem = FleeEnabled;
            RevOutcomeEnabled.SelectedItem = RevEnabled;
            SafOutcomeEnabled.SelectedItem = ShootAndFleeEnabled;
            SpitEnabled.SelectedItem = SpittingEnabled;
            HostageTakingEnabled.SelectedItem = Settings.HostageTakingEnabled;
            Normal("Assigned Values");
        }

    private static void AppendToIni()
    {
            Normal("Appending to INI...");
            Inifile.Write("General_Settings", "Chance", SetChance.Value);
            Inifile.Write("Outcome_Configuration", "Get Out And Shoot Outcome Enabled", GoasOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Ramming Outcome Enabled", RiyOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Flee Outcome Enabled", FleeOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Revving Outcome Enabled", RevOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Yelling Outcome Enabled", YellOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Yelling In Car Outcome Enabled", YicOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Shoot And Flee Outcome Enabled", SafOutcomeEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Spitting Outcome Enabled", SpitEnabled.SelectedItem);
            Inifile.Write("Outcome_Configuration", "Hostage Taking Outcome Enabled", HostageTakingEnabled.SelectedItem);  
            Normal("Finished Appending to INI");

            Normal("Reading new Values...");
            Chance = Inifile.ReadInt32("General_Settings", "Chance", 15);
            GetOutAndShootEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Get Out And Shoot Outcome Enabled", GetOutAndShootEnabled);
            RamEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Ramming Outcome Enabled", RamEnabled);
            FleeEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Flee Outcome Enabled", FleeEnabled);
            RevEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Revving Outcome Enabled", RevEnabled);
            YellEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Yelling Outcome Enabled", YellEnabled);
            YellInCarEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Yelling In Car Outcome Enabled", YellInCarEnabled);
            ShootAndFleeEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Shoot And Flee Outcome Enabled", ShootAndFleeEnabled);
            SpittingEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Spitting Outcome Enabled", SpittingEnabled);
            Settings.HostageTakingEnabled = Inifile.ReadBoolean("Outcome_Configuration", "Hostage Taking Outcome Enabled", Settings.HostageTakingEnabled);  
            Normal("Finished reading new values");

            Normal("----INI Values---");
            Normal($"Chance: {Chance}");
            Normal($"Get Out And Shoot Outcome Enabled: {GetOutAndShootEnabled}");
            Normal($"Ramming Outcome Enabled: {RamEnabled}");
            Normal($"Flee Outcome Enabled: {FleeEnabled}");
            Normal($"Revving Outcome Enabled: {RevEnabled}");
            Normal($"Yelling Outcome Enabled: {YellEnabled}");
            Normal($"Yelling in Car Outcome Enabled: {YellInCarEnabled}");
            Normal($"Shoot And Flee Outcome Enabled: {ShootAndFleeEnabled}");
            Normal($"Spitting Outcome Enabled: {SpittingEnabled}");
            Normal($"Hostage Taking Enabled: {Settings.HostageTakingEnabled}");
            Normal("----INI Values---");

            Normal("Reloading Enabled events...");
            FilterOutcomes();
            Normal("Finished Reloading Enabled events");
            Game.DisplayNotification("commonmenu", "shop_tick_icon", "Riskier Traffic Stops", "~b~INI Saving", "Saved to INI ~g~Successfully~w~!");
        }

    private static void MenuPoolProcess()
    {
            try
            {
                Normal("Initializing MenuPoolProcess");
                while (Main.OnDuty)
                {
                    GameFiber.Yield();

                    MainMenuPool.ProcessMenus();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e);
            }
    }

    internal static bool MenuRequirements() // The aforementioned menu requirements
    {
        return !UIMenu.IsAnyMenuVisible && !TabView.IsAnyPauseMenuVisible; // Makes sure that the player is not paused/in a Compulite style menu. Checks if any other menus are open
    }
}