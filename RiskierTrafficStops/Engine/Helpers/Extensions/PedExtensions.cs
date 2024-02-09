using static RiskierTrafficStops.Engine.Helpers.MathHelper;
using static RiskierTrafficStops.Engine.Data.Arrays;

namespace RiskierTrafficStops.Engine.Helpers.Extensions;

internal static class PedExtensions
{
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
    /// Handles all relationship group changes
    /// </summary>
    /// <param name="suspectRelationshipGroup"></param>
    internal static void SetRelationshipGroups(RelationshipGroup suspectRelationshipGroup)
    {
        Normal("Setting up Suspect Relationship Group");
        suspectRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
        suspectRelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

        MainPlayer.RelationshipGroup.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate); //Relationship groups go both ways
        RelationshipGroup.Cop.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate);
    }
    
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
    
    /// <summary>
    /// Makes a ped rev their vehicles engine, the int list parameters each need a minimum and maximum value
    /// </summary>
    internal static void RevEngine(this Ped driver, Vehicle suspectVehicle, int[] timeBetweenRevs, int[] timeForRevsToLast, int totalNumberOfRevs)
    {
        Normal("Starting Rev Engine method");
        for (var i = 0; i < totalNumberOfRevs; i++)
        {
            GameFiber.Yield();
            var time = Rndm.Next(timeForRevsToLast[0], timeForRevsToLast[1]) * 1000;
            driver.Tasks.PerformDrivingManeuver(suspectVehicle, VehicleManeuver.RevEngine, time);
            GameFiber.Wait(time);
            var time2 = Rndm.Next(timeBetweenRevs[0], timeBetweenRevs[1]) * 1000;
            GameFiber.Wait(time2);
        }
    }

}