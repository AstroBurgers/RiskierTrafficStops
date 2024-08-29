using System.IO;
using Newtonsoft.Json;

// TODO
namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class Localization
{
    #region Variables

    internal static string MenuTitle { get; set; }
    internal static string SetChanceMenuItem { get; set; }
    internal static string GoasMenuItem { get; set; }
    internal static string YicMenuItem { get; set; }
    internal static string YellMenuItem { get; set; }
    internal static string RiyMenuItem { get; set; }
    internal static string FleeMenuItem { get; set; }
    internal static string RevMenuItem { get; set; }
    internal static string SafMenuItem { get; set; }
    internal static string SpitMenuItem { get; set; }
    internal static string HostageTakingMenuItem { get; set; }
    internal static string SaveToIniMenuItem { get; set; }
    
    internal static string MenuDesc { get; set; }
    internal static string SetChanceMenuItemDescription { get; set; }
    internal static string GoasMenuItemDescription { get; set; }
    internal static string YicMenuItemDescription { get; set; }
    internal static string YellMenuItemDescription { get; set; }
    internal static string RiyMenuItemDescription { get; set; }
    internal static string FleeMenuItemDescription { get; set; }
    internal static string RevMenuItemDescription { get; set; }
    internal static string SafMenuItemDescription { get; set; }
    internal static string SpitMenuItemDescription { get; set; }
    internal static string HostageTakingMenuItemDescription { get; set; }
    internal static string SaveToIniMenuItemDescription { get; set; }
    
    internal static string YellingNotiText { get; set; }
    
    #endregion

    internal static void ReadJson()
    {
        using (StreamReader sr = new StreamReader(@"plugins\LSPDFR\RiskierTrafficStops\Localization.json"))
        {
            string json = sr.ReadToEnd();
            JSONStruct data = JsonConvert.DeserializeObject<JSONStruct>(json);

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
            HostageTakingMenuItem = data.HostageTakingMenuItem;
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
            HostageTakingMenuItemDescription = data.HostageTakingMenuItemDescription;
            SaveToIniMenuItemDescription = data.SaveToIniMenuItemDescription;
            YellingNotiText = data.YellingNotiText;
        }
    }
}

internal sealed class JSONStruct
{
    #region titles

    internal string MenuTitle { get; set; }
    internal string SetChanceMenuItem { get; set; }
    internal string GoasMenuItem { get; set; }
    internal string YicMenuItem { get; set; }
    internal string YellMenuItem { get; set; }
    internal string RiyMenuItem { get; set; }
    internal string FleeMenuItem { get; set; }
    internal string RevMenuItem { get; set; }
    internal string SafMenuItem { get; set; }
    internal string SpitMenuItem { get; set; }
    internal string HostageTakingMenuItem { get; set; }
    internal string SaveToIniMenuItem { get; set; }
    
    #endregion

    #region Descs

    internal string MenuDesc { get; set; }
    internal string SetChanceMenuItemDescription { get; set; }
    internal string GoasMenuItemDescription { get; set; }
    internal string YicMenuItemDescription { get; set; }
    internal string YellMenuItemDescription { get; set; }
    internal string RiyMenuItemDescription { get; set; }
    internal string FleeMenuItemDescription { get; set; }
    internal string RevMenuItemDescription { get; set; }
    internal string SafMenuItemDescription { get; set; }
    internal string SpitMenuItemDescription { get; set; }
    internal string HostageTakingMenuItemDescription { get; set; }
    internal string SaveToIniMenuItemDescription { get; set; }
    
    #endregion
    
    internal string YellingNotiText { get; set; }
}