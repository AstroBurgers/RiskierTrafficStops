using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Drawing;
using static RiskierTrafficStops.Settings;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Systems
{
    internal class ConfigMenu
    {

        internal static readonly UIMenuNumericScrollerItem<int> SetChance = new("Chance", "Chance that a Outcome happens", 0, 100, 1);
        internal static readonly UIMenuListScrollerItem<bool> GOASOutcomeEnabled = new("Get Out And Shoot", "Enable or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> YICOutcomeEnabled = new("Yell In Car", "Enable or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> YellOutcomeEnabled = new("Yell", "Enable or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> RIYOutcomeEnabled = new("Ram Into You", "Enable or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> FleeOutcomeEnabled = new("Flee", "Enable or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> RevOutcomeEnabled = new("Rev Engine", "Enable or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> SAFOutcomeEnabled = new("Shoot And Flee", "Enable Or Disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> AutoLoggingEnabled = new("Auto Logging", "Enable or Disable Auto Logging", new[] { true, false });
        internal static readonly UIMenuItem SaveToINI = new("Save To INI", "Saves the current values to the INI File and Reloads the INI");

        internal static UIMenu MainMenu = new("RTS Config", "Configure Riskier Traffic Stops");

        internal static MenuPool MainMenuPool = new();

        internal static void CreateMenu()
        {
            Logger.Debug("Creating Menu...");
            MainMenuPool.Add(MainMenu);
            MainMenu.MouseControlsEnabled = false;
            MainMenu.AllowCameraMovement = true;
            TextStyle style = new TextStyle(TextFont.ChaletComprimeCologne, TextStyle.Current.Color, 1f, TextJustification.Center);
            MainMenu.TitleStyle = style;

            Debug("Adding Items to Menu");

            MainMenu.AddItems(SetChance, AutoLoggingEnabled, SAFOutcomeEnabled, GOASOutcomeEnabled, YICOutcomeEnabled, RIYOutcomeEnabled, FleeOutcomeEnabled, RevOutcomeEnabled, SaveToINI);
            SaveToINI.BackColor = Color.Green;

            MainMenu.OnItemSelect += MainMenu_OnItemSelect;

            GameFiber.StartNew(MenuPoolProcess);
            SetupMenu();
        }

        private static void MainMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem.Equals(SaveToINI))
            {
                AppendToINI();
            }
        }

        internal static void SetupMenu()
        {
            Debug("Assinging Menu values to their respective INI Values...");
            SetChance.Value = Settings.Chance;
            GOASOutcomeEnabled.SelectedItem = Settings.getOutAndShootEnabled;
            YICOutcomeEnabled.SelectedItem = Settings.yellInCarEnabled;
            RIYOutcomeEnabled.SelectedItem = Settings.ramEnabled;
            FleeOutcomeEnabled.SelectedItem = Settings.fleeEnabled;
            RevOutcomeEnabled.SelectedItem = Settings.revEnabled;
            AutoLoggingEnabled.SelectedItem = Settings.autoLogEnabled;
            SAFOutcomeEnabled.SelectedItem = Settings.shootAndFleeEnabled;
            Debug("Assigned Values");
        }

        internal static void AppendToINI()
        {
            Debug("Appending to INI...");
            Settings.inifile.Write("Settings", "Chance", SetChance.Value);
            Settings.inifile.Write("Settings", "Get Out And Shoot Outcome enabled", GOASOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Ramming Oucome enabled", RIYOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Flee Outcome enabled", FleeOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Revving Outcome enabled", RevOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Yelling Outcome enabled", YellOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Yelling in Car Outcome enabled", YICOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Shoot And Flee Outcome enabled", SAFOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Automatic Error Reporting enabled", AutoLoggingEnabled.SelectedItem);
            Debug("Finished Appending to INI");

            Debug("Reading new Values...");
            Chance = inifile.ReadInt32("Settings", "Chance", 15);
            getOutAndShootEnabled = inifile.ReadBoolean("Settings", "Get Out And Shoot Outcome enabled", getOutAndShootEnabled);
            ramEnabled = inifile.ReadBoolean("Settings", "Ramming Oucome enabled", ramEnabled);
            fleeEnabled = inifile.ReadBoolean("Settings", "Flee Outcome enabled", fleeEnabled);
            revEnabled = inifile.ReadBoolean("Settings", "Revving Outcome enabled", revEnabled);
            yellEnabled = inifile.ReadBoolean("Settings", "Yelling Outcome enabled", yellEnabled);
            yellInCarEnabled = inifile.ReadBoolean("Settings", "Yelling Car in Outcome enabled", yellInCarEnabled);
            shootAndFleeEnabled = inifile.ReadBoolean("Settings", "Shoot And Flee Outcome enabled", yellInCarEnabled);
            autoLogEnabled = inifile.ReadBoolean("Settings", "Automatic Error Reporting enabled", autoLogEnabled);
            Debug("Finished reading new vaules");

            Debug("----INI Values---");
            Debug("");
            Debug($"Chance: {Settings.Chance}");
            Debug($"Get Out And Shoot Outcome enabled: {Settings.getOutAndShootEnabled}");
            Debug($"Ramming Outcome enabled: {Settings.ramEnabled}");
            Debug($"Flee Outcome enabled: {Settings.fleeEnabled}");
            Debug($"Revving Outcome enabled: {Settings.revEnabled}");
            Debug($"Yelling Outcome enabled: {Settings.yellEnabled}");
            Debug($"Yelling in Car Outcome enabled: {Settings.yellInCarEnabled}");
            Debug($"Shoot And Flee Outcome enabled: {Settings.shootAndFleeEnabled}");
            Debug($"Automatic Error Reporting enabled: {Settings.autoLogEnabled}");
            Debug($"");
            Debug("----INI Values---");

            Debug("Reloading Enabled events...");
            Settings.FilterOutcomes();
            Debug("Finished Reloading Enabled events");
            Game.DisplayNotification("commonmenu", "shop_tick_icon", "Riskier Traffic Stops", "~b~INI Saving", "Saved to INI ~g~Successfully~w~!");
        }

        internal static void MenuPoolProcess()
        {
            try
            {
                Debug("Initializing MenuPoolProcess");
                while (Main._onDuty)
                {
                    GameFiber.Yield();

                    MainMenuPool.ProcessMenus();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Logger.Error(e, "Menu.cs: MenuPoolProcess");
            }
        }


        internal static bool MenuRequirements() // The afformentioned menu requirements
        {
            return !UIMenu.IsAnyMenuVisible && !TabView.IsAnyPauseMenuVisible; // Makes sure that the player is not paused/in a compulite style menu. Checks if any other menus are open
        }
    }
}