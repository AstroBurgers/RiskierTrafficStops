using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    // TODO
    internal class Ambush
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        private static RelationshipGroup _suspectRelateGroup = new("RTSAmbushSuspects");

        internal static void AmbushOutcome(LHandle handle)
        {
            if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
            {
                Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                CleanupEvent();
                return;
            }

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
            var pedsInVehicle = _suspectVehicle.Occupants;
            if (pedsInVehicle.Length < 1)
                throw new ArgumentOutOfRangeException();
            
            driver.Tasks.DriveToPosition(frontOffset, 40f, VehicleDrivingFlags.Emergency).WaitForCompletion();
            for (var i = pedsInVehicle.Length - 1; i >= 0; i--)
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