namespace RiskierTrafficStops.Engine.Helpers;

internal static class Extensions
{
    /// <summary>
    /// Checks if a ped both exists and is alive
    /// </summary>
    /// <param name="ped"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Ped ped) => ped.Exists() && ped.IsAlive && ped.Model.IsValid;

    internal static void GiveWeapon(this Ped ped)
    {
        if (ped.IsAvailable() && !ped.Inventory.HasLoadedWeapon)
        {
            var weapon = WeaponList[Rndm.Next(WeaponList.Length)];
            ped.Inventory.GiveNewWeapon(weapon, 100, true);
            Normal($"Giving {ped.Model.Name} {weapon}");
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
            Normal($"Giving {ped.Model.Name} {weapon}");
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
    /// <param name="veh"></param>
    /// <returns></returns>
    internal static bool IsAvailable(this Vehicle veh) => veh.Exists() && veh.IsValid() && veh.Model.IsValid;
    
    // Thanks again, Khori
    public static T PickRandom<T>(this IEnumerable<T> source) => source.Any() ? source.PickRandom(1).Single() : default;

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count) => source.Shuffle().Take(count);

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.OrderBy(_ => Guid.NewGuid());
}