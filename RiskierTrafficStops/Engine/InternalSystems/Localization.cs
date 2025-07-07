using System.IO;
using Newtonsoft.Json;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace RiskierTrafficStops.Engine.InternalSystems;

public static class Localization
{
    #region Variables

    public static string MenuTitle { get; private set; }
    public static string SetChanceMenuItem { get; private set; }
    public static string GoasMenuItem { get; private set; }
    public static string YicMenuItem { get; private set; }
    public static string YellMenuItem { get; private set; }
    public static string RiyMenuItem { get; private set; }
    public static string FleeMenuItem { get; private set; }
    public static string RevMenuItem { get; private set; }
    public static string SafMenuItem { get; private set; }
    public static string SpitMenuItem { get; private set; }
    public static string GoRoMenuItem { get; private set; }
    public static string SaveToIniMenuItem { get; private set; }

    public static string MenuDesc { get; private set; }
    public static string SetChanceMenuItemDescription { get; private set; }
    public static string GoasMenuItemDescription { get; private set; }
    public static string YicMenuItemDescription { get; private set; }
    public static string YellMenuItemDescription { get; private set; }
    public static string RiyMenuItemDescription { get; private set; }
    public static string FleeMenuItemDescription { get; private set; }
    public static string RevMenuItemDescription { get; private set; }
    public static string SafMenuItemDescription { get; private set; }
    public static string SpitMenuItemDescription { get; private set; }
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
        using (var sr = new StreamReader(@"plugins\LSPDFR\RiskierTrafficStops\Localization.json"))
        {
            var json = sr.ReadToEnd();
            var data = JsonConvert.DeserializeObject<JSONStruct>(json);

            MenuTitle = data.MenuTitle;
            SetChanceMenuItem = data.SetChanceMenuItem;
            GoasMenuItem = data.GoasMenuItem;
            YicMenuItem = data.YicMenuItem;
            YellMenuItem = data.YellMenuItem;
            RiyMenuItem = data.RiyMenuItem;
            FleeMenuItem = data.FleeMenuItem;
            RevMenuItem = data.RevMenuItem;
            SafMenuItem = data.SafMenuItem;
            SpitMenuItem = data.SpitMenuItem;
            GoRoMenuItem = data.GoRoMenuItem;
            SaveToIniMenuItem = data.SaveToIniMenuItem;

            MenuDesc = data.MenuDesc;
            SetChanceMenuItemDescription = data.SetChanceMenuItemDescription;
            GoasMenuItemDescription = data.GoasMenuItemDescription;
            YicMenuItemDescription = data.YicMenuItemDescription;
            YellMenuItemDescription = data.YellMenuItemDescription;
            RiyMenuItemDescription = data.RiyMenuItemDescription;
            FleeMenuItemDescription = data.FleeMenuItemDescription;
            RevMenuItemDescription = data.RevMenuItemDescription;
            SafMenuItemDescription = data.SafMenuItemDescription;
            SpitMenuItemDescription = data.SpitMenuItemDescription;
            GoRoMenuItemDescription = data.GoRoMenuItemDescription;
            SaveToIniMenuItemDescription = data.SaveToIniMenuItemDescription;
            YellingNotiText = data.YellingNotiText;
        }

        Normal($"MenuTitle: {MenuTitle}");
        Normal($"SetChanceMenuItem: {SetChanceMenuItem}");
        Normal($"GoasMenuItem: {GoasMenuItem}");
        Normal($"YicMenuItem: {YicMenuItem}");
        Normal($"YellMenuItem: {YellMenuItem}");
        Normal($"RiyMenuItem: {RiyMenuItem}");
        Normal($"FleeMenuItem: {FleeMenuItem}");
        Normal($"RevMenuItem: {RevMenuItem}");
        Normal($"SafMenuItem: {SafMenuItem}");
        Normal($"SpitMenuItem: {SpitMenuItem}");
        Normal($"SaveToIniMenuItem: {SaveToIniMenuItem}");

        Normal($"MenuDesc: {MenuDesc}");
        Normal($"SetChanceMenuItemDescription: {SetChanceMenuItemDescription}");
        Normal($"GoasMenuItemDescription: {GoasMenuItemDescription}");
        Normal($"YicMenuItemDescription: {YicMenuItemDescription}");
        Normal($"YellMenuItemDescription: {YellMenuItemDescription}");
        Normal($"RiyMenuItemDescription: {RiyMenuItemDescription}");
        Normal($"FleeMenuItemDescription: {FleeMenuItemDescription}");
        Normal($"RevMenuItemDescription: {RevMenuItemDescription}");
        Normal($"SafMenuItemDescription: {SafMenuItemDescription}");
        Normal($"SpitMenuItemDescription: {SpitMenuItemDescription}");
        Normal($"SaveToIniMenuItemDescription: {SaveToIniMenuItemDescription}");
        Normal($"YellingNotiText: {YellingNotiText}");
    }
}

public sealed class JSONStruct
{
    #region titles

    public string MenuTitle { get; }
    public string SetChanceMenuItem { get; }
    public string GoasMenuItem { get; }
    public string YicMenuItem { get; }
    public string YellMenuItem { get; }
    public string RiyMenuItem { get; }
    public string FleeMenuItem { get; }
    public string RevMenuItem { get; }
    public string SafMenuItem { get; }
    public string SpitMenuItem { get; }
    public string GoRoMenuItem { get; }
    public string SaveToIniMenuItem { get; }

    #endregion

    #region Descs

    public string MenuDesc { get; }
    public string SetChanceMenuItemDescription { get; }
    public string GoasMenuItemDescription { get; }
    public string YicMenuItemDescription { get; }
    public string YellMenuItemDescription { get; }
    public string RiyMenuItemDescription { get; }
    public string FleeMenuItemDescription { get; }
    public string RevMenuItemDescription { get; }
    public string SafMenuItemDescription { get; }
    public string SpitMenuItemDescription { get; }
    public string GoRoMenuItemDescription { get; }
    public string SaveToIniMenuItemDescription { get; }

    #endregion

    public string YellingNotiText { get; }
}