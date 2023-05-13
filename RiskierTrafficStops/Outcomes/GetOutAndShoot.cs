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

        internal static void GOASOutcome(LHandle handle)
        {
            Normal("GetOutAndShoot.cs", "Setting up Suspect and Suspect Vehicle");
            Suspect = Functions.GetPulloverSuspect(handle);
            suspectVehicle = Suspect.CurrentVehicle;
            Suspect.BlockPermanentEvents = true;
            suspectVehicle.IsPersistent = true;
            List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
            Game.DisplayNotification(PedsInVehicle.Count.ToString());
            foreach (Ped i in PedsInVehicle)
            {
                string Weapon = WeaponList[rndm.Next(WeaponList.Length)];
                if (!i.Inventory.HasLoadedWeapon)
                {
                    Normal("GetOutAndShoot.cs", $"Giving Suspect weapon: {Weapon}");
                    i.Inventory.GiveNewWeapon(Weapon, 100, true);
                }
            }
            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            foreach (Ped i in PedsInVehicle)
            {
                GameFiber.StartNew(delegate
                {
                    try
                    {
                        Normal("GetOutAndShoot.cs", "Setting Suspect relationship group");
                        i.RelationshipGroup = SuspectRelateGroup;
                        Normal("GetOutAndShoot.cs", "Making Suspect leave vehicle");
                        i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                        Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
                        i.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7000);
                    }
                    catch (Exception TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
                    }
                });
            }

            GameFiber.Wait(7000);

            int Chance = rndm.Next(1, 101);
            bool PursuitCreated = false;
            int Seat = -2;

            foreach (Ped i in PedsInVehicle)
            {
                GameFiber.StartNew(delegate
                {
                    try
                    {

                        if (Chance < 45)
                        {
                            Normal("GetOutAndShoot.cs", "Making Suspect enter vehicle");
                            i.Tasks.EnterVehicle(suspectVehicle, (Seat + 1), 2f);
                            GameFiber.Wait(2000);
                            Normal("GetOutAndShoot.cs", "Setting up pursuit");
                            if (!PursuitCreated)
                            {
                                PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
                                PursuitCreated = true;
                            }
                            else
                            {
                                Functions.AddPedToPursuit(PursuitLHandle, i);
                            }
                        }
                        else if (Chance > 45)
                        {
                            Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
                            Functions.ForceEndCurrentPullover();
                            i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                        }
                    }
                    catch (Exception TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
                    }
                });
            }

            /*for (int i = 0; i < PedsInVehicle.Count; i++)
            {
                GameFiber.StartNew(delegate
                {
                    try
                    {

                        if (Chance < 45)
                        {
                            Normal("GetOutAndShoot.cs", "Making Suspect enter vehicle");
                            PedsInVehicle[i].Tasks.EnterVehicle(suspectVehicle, (i - 1));
                            GameFiber.Wait(2000);
                            Normal("GetOutAndShoot.cs", "Setting up pursuit");
                            if (!PursuitCreated)
                            {
                                PursuitLHandle = SetupPursuitWithList(true, PedsInVehicle);
                                PursuitCreated = true;
                            }
                            else
                            {
                                Functions.AddPedToPursuit(PursuitLHandle, PedsInVehicle[i]);
                            }
                        }
                        else if (Chance > 45)
                        {
                            Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
                            Functions.ForceEndCurrentPullover();
                            PedsInVehicle[i].Tasks.FightAgainstClosestHatedTarget(40f, -1);
                        }
                    }
                    catch (Exception TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
                    }
                });

            }*/
        }
    }
}