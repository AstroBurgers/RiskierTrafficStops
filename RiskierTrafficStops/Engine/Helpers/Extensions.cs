using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;

namespace RiskierTrafficStops.Engine.Helpers;

internal static class Extensions
{
    /// <summary>
    /// Checks if a ped both exists and is alive
    /// </summary>
    /// <param name="ped"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Ped ped)
    {
        return ped.Exists() && ped.IsAlive && ped != null;
    }

    internal static void GiveWeapon(this Ped ped)
    {
        if (ped.IsAvailable() && !ped.Inventory.HasLoadedWeapon)
        {
            var weapon = WeaponList[Rndm.Next(WeaponList.Length)];
            ped.Inventory.GiveNewWeapon(weapon, 100, true);
            Logger.Normal($"Giving {ped.Model.Name} {weapon}");
        }
        else if (ped.IsAvailable() && ped.Inventory.HasLoadedWeapon)
        {
            var pedWeapons = ped.Inventory.Weapons;
            var weapon = pedWeapons[Rndm.Next(pedWeapons.Count)];
            ped.Inventory.EquippedWeapon = weapon.ToString();
        }
    }

    internal static void GivePistol(this Ped ped)
    {
        if (ped.IsAvailable() && !ped.Inventory.HasLoadedWeapon)
        {
            var weapon = PistolList[Rndm.Next(PistolList.Length)];
            ped.Inventory.GiveNewWeapon(weapon, 100, true);
            Logger.Normal($"Giving {ped.Model.Name} {weapon}");
        }
        else if (ped.IsAvailable() && ped.Inventory.HasLoadedWeapon)
        {
            var pedWeapons = ped.Inventory.Weapons;
            var weapon = pedWeapons[Rndm.Next(pedWeapons.Count)];
            ped.Inventory.EquippedWeapon = weapon.ToString();
        }
    }
    
    /// <summary>
    /// Checks if a ped both exists and is alive
    /// </summary>
    /// <param name="ped"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Vehicle veh)
    {
        return veh.Exists() && veh.IsValid() && veh != null;
    }
}