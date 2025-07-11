﻿using System.Drawing;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using RiskierTrafficStops.Engine.InternalSystems.Settings;
using static RiskierTrafficStops.Engine.InternalSystems.Localization;

namespace RiskierTrafficStops.Engine.FrontendSystems;

internal static class ConfigMenu
{
    private static readonly UIMenuNumericScrollerItem<int> SetChance = new(SetChanceMenuItem,
        SetChanceMenuItemDescription, 0, 100, 1);

    private static readonly UIMenuListScrollerItem<bool> GoasOutcomeEnabled =
        new(GoasMenuItem, GoasMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> YicOutcomeEnabled =
        new(YicMenuItem, YicMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> YellOutcomeEnabled =
        new(YellMenuItem, YellMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> RiyOutcomeEnabled =
        new(RiyMenuItem, RiyMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> FleeOutcomeEnabled =
        new(FleeMenuItem, FleeMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> RevOutcomeEnabled =
        new(RevMenuItem, RevMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> SafOutcomeEnabled =
        new(SafMenuItem, SafMenuItemDescription, [true, false]);

    private static readonly UIMenuListScrollerItem<bool> SpitEnabled = new(SpitMenuItem, SpitMenuItemDescription,
        [true, false]);
    
    private static readonly UIMenuListScrollerItem<bool> GoRoEnabled = new(GoRoMenuItem, GoRoMenuItemDescription,
        [true, false]);

    private static readonly UIMenuItem SaveToIni = new(SaveToIniMenuItem, SaveToIniMenuItemDescription);

    internal static readonly UIMenu MainMenu = new(MenuTitle, MenuDesc);

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

        MainMenu.AddItems(SetChance, SafOutcomeEnabled, GoasOutcomeEnabled, YicOutcomeEnabled, RiyOutcomeEnabled,
            FleeOutcomeEnabled, RevOutcomeEnabled, YellOutcomeEnabled, SpitEnabled, GoRoEnabled, SaveToIni);
        SaveToIni.BackColor = Color.Green;

        MainMenu.OnItemSelect +=
            (_, selectedItem, _) => //Easier way to do simple things in RNUI that don't require a lot of code
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
        SetChance.Value = UserConfig.Chance;
        YellOutcomeEnabled.SelectedItem = UserConfig.YellEnabled;
        GoasOutcomeEnabled.SelectedItem = UserConfig.GetOutAndShootEnabled;
        YicOutcomeEnabled.SelectedItem = UserConfig.YellInCarEnabled;
        RiyOutcomeEnabled.SelectedItem = UserConfig.RamEnabled;
        FleeOutcomeEnabled.SelectedItem = UserConfig.FleeEnabled;
        RevOutcomeEnabled.SelectedItem = UserConfig.RevEnabled;
        SafOutcomeEnabled.SelectedItem = UserConfig.ShootAndFleeEnabled;
        SpitEnabled.SelectedItem = UserConfig.SpittingEnabled;
        GoRoEnabled.SelectedItem = UserConfig.GetOutROEnabled;
        Normal("Assigned Values");
    }

    private static void AppendToIni()
    {
        Normal("Appending to INI...");
        Settings.IniReflector.WriteSingle("Chance", SetChance.Value);
        Settings.IniReflector.WriteSingle("GetOutAndShootEnabled", GoasOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("RamEnabled", RiyOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("FleeEnabled", FleeOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("RevEnabled", RevOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("YellEnabled", YellOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("YellInCarEnabled", YicOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("ShootAndFleeEnabled", SafOutcomeEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("SpittingEnabled", SpitEnabled.SelectedItem);
        Settings.IniReflector.WriteSingle("GetOutROEnabled", GoRoEnabled.SelectedItem);
        Normal("Finished Appending to INI");

        Normal("Reading new Values...");
        Settings.IniReflector.Read(UserConfig, true);
        Normal("Finished reading new values");

        Normal("Reloading Enabled events...");
        FilterOutcomes();
        Normal("Finished Reloading Enabled events");
        Game.DisplayNotification("commonmenu", "shop_tick_icon", "Riskier Traffic Stops", "~b~INI Saving",
            "Saved to INI ~g~Successfully~w~!");
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
        return
            !UIMenu.IsAnyMenuVisible &&
            !TabView.IsAnyPauseMenuVisible; // Makes sure that the player is not paused/in a Compulite style menu. Checks if any other menus are open
    }
}