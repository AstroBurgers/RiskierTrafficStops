using System.IO;
using Newtonsoft.Json;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace RiskierTrafficStops.Engine.InternalSystems;

public static class Localization
{
    #region Variables

    public static string MenuTitle { get; private set; }
    public static string SetChanceMenuItem { get; private set; }
    public static string ChanceSettingMenuItem { get; private set; }
    public static string GoasMenuItem { get; private set; }
    public static string YellMenuItem { get; private set; }
    public static string RiyMenuItem { get; private set; }
    public static string FleeMenuItem { get; private set; }
    public static string SafMenuItem { get; private set; }
    public static string GoRoMenuItem { get; private set; }
    public static string SaveToIniMenuItem { get; private set; }

    public static string MenuDesc { get; private set; }
    public static string SetChanceMenuItemDescription { get; private set; }
    public static string ChanceSettingMenuItemDescription { get; private set; }
    public static string GoasMenuItemDescription { get; private set; }
    public static string YellMenuItemDescription { get; private set; }
    public static string RiyMenuItemDescription { get; private set; }
    public static string FleeMenuItemDescription { get; private set; }
    public static string SafMenuItemDescription { get; private set; }
    public static string GoRoMenuItemDescription { get; private set; }
    public static string SaveToIniMenuItemDescription { get; private set; }

    public static string YellingNotiText { get; private set; }

    #endregion

    internal static bool DoesJsonFileExist()
    {
        if (Directory.Exists(@"plugins\LSPDFR\RiskierTrafficStops") &&
            File.Exists(@"plugins\LSPDFR\RiskierTrafficStops\Localization.json"))
        {
            return true;
        }
        Normal($"Failed to load because Localization.json does not exist.");
        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "RiskierTrafficStops", "~r~Missing files!", $"Could not find ~y~Localization.json~s~!\nPlease verify you have installed RiskierTrafficStops properly.");
        return false;
    }
    
    internal static void ReadJson()
    {
        using (StreamReader sr = new(@"plugins\LSPDFR\RiskierTrafficStops\Localization.json"))
        {
            string json = sr.ReadToEnd();
            JSONStruct data = JsonConvert.DeserializeObject<JSONStruct>(json);

            MenuTitle = data.MenuTitle;
            SetChanceMenuItem = data.SetChanceMenuItem;
            ChanceSettingMenuItem = data.ChanceSettingMenuItem;
            GoasMenuItem = data.GoasMenuItem;
            YellMenuItem = data.YellMenuItem;
            RiyMenuItem = data.RiyMenuItem;
            FleeMenuItem = data.FleeMenuItem;
            SafMenuItem = data.SafMenuItem;
            GoRoMenuItem = data.GoRoMenuItem;
            SaveToIniMenuItem = data.SaveToIniMenuItem;

            MenuDesc = data.MenuDesc;
            SetChanceMenuItemDescription = data.SetChanceMenuItemDescription;
            ChanceSettingMenuItemDescription = data.ChanceSettingMenuItemDescription;
            GoasMenuItemDescription = data.GoasMenuItemDescription;
            YellMenuItemDescription = data.YellMenuItemDescription;
            RiyMenuItemDescription = data.RiyMenuItemDescription;
            FleeMenuItemDescription = data.FleeMenuItemDescription;
            SafMenuItemDescription = data.SafMenuItemDescription;
            GoRoMenuItemDescription = data.GoRoMenuItemDescription;
            SaveToIniMenuItemDescription = data.SaveToIniMenuItemDescription;
            YellingNotiText = data.YellingNotiText;
        }

        Normal($"MenuTitle: {MenuTitle}");
        Normal($"SetChanceMenuItem: {SetChanceMenuItem}");
        Normal($"ChanceSettingMenuItem: {ChanceSettingMenuItem}");
        Normal($"GoasMenuItem: {GoasMenuItem}");
        Normal($"YellMenuItem: {YellMenuItem}");
        Normal($"RiyMenuItem: {RiyMenuItem}");
        Normal($"FleeMenuItem: {FleeMenuItem}");
        Normal($"SafMenuItem: {SafMenuItem}");
        Normal($"SaveToIniMenuItem: {SaveToIniMenuItem}");

        Normal($"MenuDesc: {MenuDesc}");
        Normal($"SetChanceMenuItemDescription: {SetChanceMenuItemDescription}");
        Normal($"ChanceSettingMenuItemDescription: {ChanceSettingMenuItemDescription}");
        Normal($"GoasMenuItemDescription: {GoasMenuItemDescription}");
        Normal($"YellMenuItemDescription: {YellMenuItemDescription}");
        Normal($"RiyMenuItemDescription: {RiyMenuItemDescription}");
        Normal($"FleeMenuItemDescription: {FleeMenuItemDescription}");
        Normal($"SafMenuItemDescription: {SafMenuItemDescription}");
        Normal($"SaveToIniMenuItemDescription: {SaveToIniMenuItemDescription}");
        Normal($"YellingNotiText: {YellingNotiText}");
    }
}

public sealed class JSONStruct
{
    #region titles

    public string MenuTitle { get; set; }
    public string SetChanceMenuItem { get; set; }
    public string ChanceSettingMenuItem { get; set; }
    public string GoasMenuItem { get; set; }
    public string YellMenuItem { get; set; }
    public string RiyMenuItem { get; set; }
    public string FleeMenuItem { get; set; }
    public string RevMenuItem { get; set; }
    public string SafMenuItem { get; set; }
    public string GoRoMenuItem { get; set; }
    public string SaveToIniMenuItem { get; set; }

    #endregion

    #region Descs

    public string MenuDesc { get; set; }
    public string SetChanceMenuItemDescription { get; set; }
    public string ChanceSettingMenuItemDescription { get; set; }
    public string GoasMenuItemDescription { get; set; }
    public string YellMenuItemDescription { get; set; }
    public string RiyMenuItemDescription { get; set; }
    public string FleeMenuItemDescription { get; set; }
    public string RevMenuItemDescription { get; set; }
    public string SafMenuItemDescription { get; set; }
    public string GoRoMenuItemDescription { get; set; }
    public string SaveToIniMenuItemDescription { get; set; }

    #endregion

    public string YellingNotiText { get; set; }
}