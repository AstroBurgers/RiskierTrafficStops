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

        internal static readonly UIMenuNumericScrollerItem<int> SetChance = new("Chance", "Chance that an event happens", 0, 100, 1);
        internal static readonly UIMenuListScrollerItem<bool> GOASOutcomeEnabled = new("Get Out And Shoot", "Enable or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> YICOutcomeEnabled = new("Yell In Car", "Enable or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> YellOutcomeEnabled = new("Yell", "Enable or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> RIYOutcomeEnabled = new("Ram Into You", "Enable or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> FleeOutcomeEnabled = new("Flee", "Enable or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> RevOutcomeEnabled = new("Rev Engine", "Enable or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> SAFOutcomeEnabled = new("Shoot And Flee", "Enable Or disable this outcome", new[] { true, false });
        internal static readonly UIMenuListScrollerItem<bool> AutoLoggingEnabled = new("Auto Logging", "Enable or disable auto logging", new[] { true, false });
        internal static readonly UIMenuItem SaveToINI = new("Save To INI", "Saves the current values to the INI file and reloads the INI");

        internal static UIMenu MainMenu = new("RTS Config", "Configure Riskier Traffic Stops");

        internal static MenuPool MainMenuPool = new();

        internal static void CreateMenu()
        {
            Logger.Debug("Creating Menu...");
            MainMenuPool.Add(MainMenu);
            MainMenu.MouseControlsEnabled = false;
            MainMenu.AllowCameraMovement = true;
            TextStyle style = new(TextFont.ChaletComprimeCologne, TextStyle.Current.Color, 1f, TextJustification.Center);
            MainMenu.TitleStyle = style;

            Debug("Adding Items to Menu");

            MainMenu.AddItems(SetChance, AutoLoggingEnabled, SAFOutcomeEnabled, GOASOutcomeEnabled, YICOutcomeEnabled, RIYOutcomeEnabled, FleeOutcomeEnabled, RevOutcomeEnabled, YellOutcomeEnabled, SaveToINI);
            SaveToINI.BackColor = Color.Green;

            MainMenu.OnItemSelect += (_, selectedItem, _) => //Easier way to do simple things in RNUI that dont require a lot of code
            {
                if (selectedItem.Equals(SaveToINI))
                {
                    AppendToINI();
                }
            };

            GameFiber.StartNew(MenuPoolProcess);
            SetupMenu();
        }

        internal static void SetupMenu()
        {
            Debug("Assinging Menu values to their respective INI Values...");
            SetChance.Value = Settings.Chance;
            YellOutcomeEnabled.SelectedItem = Settings.yellEnabled;
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
            Settings.inifile.Write("Settings", "Get Out And Shoot Outcome Enabled", GOASOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Ramming Outcome Enabled", RIYOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Flee Outcome Enabled", FleeOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Revving Outcome Enabled", RevOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Yelling Outcome Enabled", YellOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Yelling In Car Outcome Enabled", YICOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Shoot And Flee Outcome Enabled", SAFOutcomeEnabled.SelectedItem);
            Settings.inifile.Write("Settings", "Automatic Error Reporting Enabled", AutoLoggingEnabled.SelectedItem);
            Debug("Finished Appending to INI");

            Debug("Reading new Values...");
            Chance = inifile.ReadInt32("Settings", "Chance", 15);
            getOutAndShootEnabled = inifile.ReadBoolean("Settings", "Get Out And Shoot Outcome Enabled", getOutAndShootEnabled);
            ramEnabled = inifile.ReadBoolean("Settings", "Ramming Outcome Enabled", ramEnabled);
            fleeEnabled = inifile.ReadBoolean("Settings", "Flee Outcome Enabled", fleeEnabled);
            revEnabled = inifile.ReadBoolean("Settings", "Revving Outcome Enabled", revEnabled);
            yellEnabled = inifile.ReadBoolean("Settings", "Yelling Outcome Enabled", yellEnabled);
            yellInCarEnabled = inifile.ReadBoolean("Settings", "Yelling In Car Outcome Enabled", yellInCarEnabled);
            shootAndFleeEnabled = inifile.ReadBoolean("Settings", "Shoot And Flee Outcome Enabled", shootAndFleeEnabled);
            autoLogEnabled = inifile.ReadBoolean("Settings", "Automatic Error Reporting Enabled", autoLogEnabled);
            Debug("Finished reading new vaules");

            Debug("----INI Values---");
            Debug($"Chance: {Settings.Chance}");
            Debug($"Get Out And Shoot Outcome Enabled: {Settings.getOutAndShootEnabled}");
            Debug($"Ramming Outcome Enabled: {Settings.ramEnabled}");
            Debug($"Flee Outcome Enabled: {Settings.fleeEnabled}");
            Debug($"Revving Outcome Enabled: {Settings.revEnabled}");
            Debug($"Yelling Outcome Enabled: {Settings.yellEnabled}");
            Debug($"Yelling in Car Outcome Enabled: {Settings.yellInCarEnabled}");
            Debug($"Shoot And Flee Outcome Enabled: {Settings.shootAndFleeEnabled}");
            Debug($"Automatic Error Reporting Enabled: {Settings.autoLogEnabled}");
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