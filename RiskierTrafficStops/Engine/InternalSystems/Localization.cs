using System.IO;
using Newtonsoft.Json;

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

    public string MenuTitle { get; set; }
    public string SetChanceMenuItem { get; set; }
    public string GoasMenuItem { get; set; }
    public string YicMenuItem { get; set; }
    public string YellMenuItem { get; set; }
    public string RiyMenuItem { get; set; }
    public string FleeMenuItem { get; set; }
    public string RevMenuItem { get; set; }
    public string SafMenuItem { get; set; }
    public string SpitMenuItem { get; set; }
    public string SaveToIniMenuItem { get; set; }

    #endregion

    #region Descs

    public string MenuDesc { get; set; }
    public string SetChanceMenuItemDescription { get; set; }
    public string GoasMenuItemDescription { get; set; }
    public string YicMenuItemDescription { get; set; }
    public string YellMenuItemDescription { get; set; }
    public string RiyMenuItemDescription { get; set; }
    public string FleeMenuItemDescription { get; set; }
    public string RevMenuItemDescription { get; set; }
    public string SafMenuItemDescription { get; set; }
    public string SpitMenuItemDescription { get; set; }
    public string SaveToIniMenuItemDescription { get; set; }

    #endregion

    public string YellingNotiText { get; set; }
}