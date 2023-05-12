using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Helper;
using static RiskierTrafficStops.Logger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Deployment.Internal;


namespace RiskierTrafficStops.Outcomes
{
    internal class GetOutAndShoot
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");
        internal static Random rndm = new Random();
        internal static LHandle PursuitLHandle;

        internal static void GOASOutcome(LHandle handle, String weapon)
        {
            Normal("GetOutAndShoot.cs", "Setting up Suspect and Suspect Vehicle");
            Suspect = Functions.GetPulloverSuspect(handle);
            suspectVehicle = Suspect.CurrentVehicle;
            Suspect.BlockPermanentEvents = true;
            suspectVehicle.IsPersistent = true;

            if (!Suspect.Inventory.HasLoadedWeapon)
            {
                Normal("GetOutAndShoot.cs", $"Giving Suspect weapon: {weapon}");
                Suspect.Inventory.GiveNewWeapon(weapon, 100, true);
            }

            Normal("GetOutAndShoot.cs", "Making Suspect leave vehicle");
            Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            Normal("GetOutAndShoot.cs", "Setting Suspect relationship group");
            Suspect.RelationshipGroup = SuspectRelateGroup;
            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
            Suspect.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7000);

            if (Suspect.IsAlive && Suspect.Exists())
            {
                int Chance = rndm.Next(1, 101);

                if (Chance < 45)
                {
                    Normal("GetOutAndShoot.cs", "Making Suspect enter vehicle");
                    Suspect.Tasks.EnterVehicle(suspectVehicle, -1, 2f).WaitForCompletion();
                    Normal("GetOutAndShoot.cs", "Setting up pursuit");
                    PursuitLHandle = SetupPursuit(true, Suspect);
                }
                else if(Chance > 45)
                {
                    Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
                    Suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                }
            }
        }
    }
}