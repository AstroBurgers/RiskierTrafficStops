using System;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class ShootAndFlee
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");
        internal static LHandle PursuitLHandle;
        internal static void SAFOutcome(LHandle handle)
        {
            // Asigning the Suspect and the Suspect vehicle
            Debug("Setting up Suspect");
            Suspect = Functions.GetPulloverSuspect(handle);
            Debug("Setting up suspectVehicle");
            suspectVehicle = Suspect.CurrentVehicle;
            Suspect.BlockPermanentEvents = true;
            Suspect.IsPersistent = true;
            suspectVehicle.IsPersistent = true;
            // Adding all suspects to the List
            Debug("Adding all suspect in the vehicle to a list");
            List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
            Debug($"Peds In Vehicle: {PedsInVehicle.Count}");
        }


        internal static void AllSuspects(List<Ped> Peds)
        {
            string Weapon = pistolList[rndm.Next(pistolList.Length)];
            foreach (Ped i in Peds)
            {
                if (i.Exists())
                {
                    Debug($"Giving Suspect weapon: {Weapon}");
                    i.Inventory.GiveNewWeapon(Weapon, 100, true);
                }
            }
        }
    }
}
