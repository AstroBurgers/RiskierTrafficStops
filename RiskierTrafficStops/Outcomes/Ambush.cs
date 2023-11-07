using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Ambush
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        private static RelationshipGroup _suspectRelateGroup = new RelationshipGroup("Suspect");

        internal static void AmbushOutcome(LHandle handle)
        {
            if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
            {
                CleanupEvent(_suspect, _suspectVehicle);
                return;
            }

            Debug("Adding all suspect in the vehicle to a list");

            var pedsInVehicle = GetAllVehicleOccupants(_suspectVehicle);
            Debug($"Peds In Vehicle: {pedsInVehicle.Count}");

            Debug("Setting Suspect Relationship Group");
            _suspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            _suspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            Debug("Setting Suspect Relationship Group relations...");
            MainPlayer.RelationshipGroup.SetRelationshipWith(_suspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
            RelationshipGroup.Cop.SetRelationshipWith(_suspectRelateGroup, Relationship.Hate);

            GoToFrontOfVehicle();
        }

        private static void GoToFrontOfVehicle()
        {
            Debug("Making suspect drive to front of vehicle");
            var vehicle = GetNearestVehicle(MainPlayer.Position);
            var driver = vehicle.Driver;
            var frontOffset = _suspectVehicle.GetOffsetPositionFront(5f);
            var pedsInVehicle = GetAllVehicleOccupants(_suspectVehicle);
            if (pedsInVehicle.Count < 1)
                throw new ArgumentOutOfRangeException();
            
            driver.Tasks.DriveToPosition(frontOffset, 40f, VehicleDrivingFlags.Emergency).WaitForCompletion();
            for (var i = pedsInVehicle.Count - 1; i >= 0; i--)
            {
                if (!pedsInVehicle[i].Exists()) continue;
                Debug("Giving ped random weapon");
                GiveRandomWeapon(pedsInVehicle[i]);
                pedsInVehicle[i].Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);

                pedsInVehicle[i].RelationshipGroup = _suspectRelateGroup;
                pedsInVehicle[i].Tasks.FightAgainstClosestHatedTarget(30f, -1);
            }
        }

        private static void GiveRandomWeapon(Ped ped)
        {
            if (ped.Inventory.HasLoadedWeapon) return;
            var weapon = WeaponList[Rndm.Next(WeaponList.Length)];
            ped.Inventory.GiveNewWeapon(weapon, 100, true);
        }
    }
}