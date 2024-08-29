using System.IO;
using Newtonsoft.Json;

// TODO
namespace RiskierTrafficStops.Engine. publicSystems;

 public static class Localization
{
    #region Variables

     public static string MenuTitle { get; set; }
     public static string SetChanceMenuItem { get; set; }
     public static string GoasMenuItem { get; set; }
     public static string YicMenuItem { get; set; }
     public static string YellMenuItem { get; set; }
     public static string RiyMenuItem { get; set; }
     public static string FleeMenuItem { get; set; }
     public static string RevMenuItem { get; set; }
     public static string SafMenuItem { get; set; }
     public static string SpitMenuItem { get; set; }
     public static string HostageTakingMenuItem { get; set; }
     public static string SaveToIniMenuItem { get; set; }
    
     public static string MenuDesc { get; set; }
     public static string SetChanceMenuItemDescription { get; set; }
     public static string GoasMenuItemDescription { get; set; }
     public static string YicMenuItemDescription { get; set; }
     public static string YellMenuItemDescription { get; set; }
     public static string RiyMenuItemDescription { get; set; }
     public static string FleeMenuItemDescription { get; set; }
     public static string RevMenuItemDescription { get; set; }
     public static string SafMenuItemDescription { get; set; }
     public static string SpitMenuItemDescription { get; set; }
     public static string HostageTakingMenuItemDescription { get; set; }
     public static string SaveToIniMenuItemDescription { get; set; }
    
     public static string YellingNotiText { get; set; }
    
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
        
        Logger.Normal($"MenuTitle: {MenuTitle}");
        Logger.Normal($"SetChanceMenuItem: {SetChanceMenuItem}");
        Logger.Normal($"GoasMenuItem: {GoasMenuItem}");
        Logger.Normal($"YicMenuItem: {YicMenuItem}");
        Logger.Normal($"YellMenuItem: {YellMenuItem}");
        Logger.Normal($"RiyMenuItem: {RiyMenuItem}");
        Logger.Normal($"FleeMenuItem: {FleeMenuItem}");
        Logger.Normal($"RevMenuItem: {RevMenuItem}");
        Logger.Normal($"SafMenuItem: {SafMenuItem}");
        Logger.Normal($"SpitMenuItem: {SpitMenuItem}");
        Logger.Normal($"HostageTakingMenuItem: {HostageTakingMenuItem}");
        Logger.Normal($"SaveToIniMenuItem: {SaveToIniMenuItem}");
        
        Logger.Normal($"MenuDesc: {MenuDesc}");
        Logger.Normal($"SetChanceMenuItemDescription: {SetChanceMenuItemDescription}");
        Logger.Normal($"GoasMenuItemDescription: {GoasMenuItemDescription}");
        Logger.Normal($"YicMenuItemDescription: {YicMenuItemDescription}");
        Logger.Normal($"YellMenuItemDescription: {YellMenuItemDescription}");
        Logger.Normal($"RiyMenuItemDescription: {RiyMenuItemDescription}");
        Logger.Normal($"FleeMenuItemDescription: {FleeMenuItemDescription}");
        Logger.Normal($"RevMenuItemDescription: {RevMenuItemDescription}");
        Logger.Normal($"SafMenuItemDescription: {SafMenuItemDescription}");
        Logger.Normal($"SpitMenuItemDescription: {SpitMenuItemDescription}");
        Logger.Normal($"HostageTakingMenuItemDescription: {HostageTakingMenuItemDescription}");
        Normal($"SaveToIniMenuItemDescription: {SaveToIniMenuItemDescription}");
        Logger.Normal($"YellingNotiText: {YellingNotiText}");
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
     public string HostageTakingMenuItem { get; set; }
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
     public string HostageTakingMenuItemDescription { get; set; }
     public string SaveToIniMenuItemDescription { get; set; }
    
    #endregion
    
     public string YellingNotiText { get; set; }
}