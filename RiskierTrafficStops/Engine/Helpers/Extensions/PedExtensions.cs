namespace RiskierTrafficStops.Engine.Helpers.Extensions;

internal static class PedExtensions
{
/*
    internal static bool HasLosOnEntity(this Entity entity, Entity entity2) =>
        NativeFunction.Natives.xFCDFF7B72D23A1AC<bool>(entity, entity2, 17); // HAS_ENTITY_CLEAR_LOS_TO_ENTITY
*/

    /// <summary>
    /// Handles all relationship group changes
    /// Makes passed in relationship group hate the main player and cop relationship groups
    /// </summary>
    /// <param name="suspectRelationshipGroup"></param>
    internal static void SetRelationshipGroups(RelationshipGroup suspectRelationshipGroup)
    {
        Normal("Setting up Suspect Relationship Group");
        suspectRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
        suspectRelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

        MainPlayer.RelationshipGroup.SetRelationshipWith(suspectRelationshipGroup,
            Relationship.Hate); //Relationship groups go both ways
        RelationshipGroup.Cop.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate);
    }

    /// <param name="ped"></param>
    extension(Ped ped)
    {
        /// <summary>
        /// Checks if a ped both exists and is alive
        /// </summary>
        /// <returns></returns>
        internal bool IsAvailable() => ped.Exists() && ped.IsAlive && ped.Model.IsValid;

        /// <summary>
        /// Makes ped drop their equipped weapon and put their hands up
        /// </summary>
        internal void Surrender()
        {
            if (!ped.IsAvailable())
                return;

            if (ped.Inventory.EquippedWeapon is not null && ped.Inventory.EquippedWeapon != "WEAPON_UNARMED")
            {
                NativeFunction.Natives.x6B7513D9966FBEC0(ped); // SET_PED_DROPS_WEAPON
            }

            ped.Tasks.PutHandsUp(-1, MainPlayer);
        }

        internal void GiveWeapon()
        {
            if (!ped.IsAvailable()) return;
            var pedWeapons = ped.Inventory.Weapons;
            var weapon = ped.Inventory.HasLoadedWeapon
                ? pedWeapons[Rndm.Next(pedWeapons.Count)]
                : WeaponList[Rndm.Next(WeaponList.Length)];
            Normal($"Giving {ped.Model.Name} weapon");
            if (ped.Inventory.Weapons.Contains(weapon))
            {
                ped.Inventory.EquippedWeapon = weapon;
            }
            else
            {
                ped.Inventory.GiveNewWeapon(weapon, -1, true);
            }
        }

        /// <summary>
        /// Makes a ped rev their vehicles engine, the int list parameters each need a minimum and maximum value
        /// </summary>
        internal void RevEngine(Vehicle suspectVehicle, int[] timeBetweenRevs,
            int[] timeForRevsToLast, int totalNumberOfRevs)
        {
            Normal("Starting Rev Engine method");
            for (var i = 0; i < totalNumberOfRevs; i++)
            {
                GameFiber.Yield();
                var time = Rndm.Next(timeForRevsToLast[0], timeForRevsToLast[1]) * 1000;
                ped.Tasks.PerformDrivingManeuver(suspectVehicle, VehicleManeuver.RevEngine, time);
                GameFiber.Wait(time);
                var time2 = Rndm.Next(timeBetweenRevs[0], timeBetweenRevs[1]) * 1000;
                GameFiber.Wait(time2);
            }
        }

        internal void GivePistol()
        {
            if (!ped.IsAvailable()) return;
            var pedWeapons = ped.Inventory.Weapons;
            var weapon = ped.Inventory.HasLoadedWeapon
                ? pedWeapons[Rndm.Next(pedWeapons.Count)]
                : PistolList[Rndm.Next(PistolList.Length)];
            Normal($"Giving {ped.Model.Name} {weapon}");
            if (ped.Inventory.Weapons.Contains(weapon))
            {
                ped.Inventory.EquippedWeapon = weapon;
            }
            else
            {
                ped.Inventory.GiveNewWeapon(weapon, -1, true);
            }
        }
    }
}