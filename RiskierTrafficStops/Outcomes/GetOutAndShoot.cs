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
            try
            {
                Normal("GetOutAndShoot.cs", "Setting up Suspect and Suspect Vehicle");
                Suspect = Functions.GetPulloverSuspect(handle);
                suspectVehicle = Suspect.CurrentVehicle;
                Suspect.BlockPermanentEvents = true;
                suspectVehicle.IsPersistent = true;
                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
                Normal("GetOutAndShoot.cs", $"[DEBUG] Peds In Vehicle: {PedsInVehicle.Count}");
                foreach (Ped i in PedsInVehicle)
                {
                    try
                    {
                        string Weapon = WeaponList[rndm.Next(WeaponList.Length)];
                        if (i.Exists())
                        {
                            if (!i.Inventory.HasLoadedWeapon)
                            {
                                Normal("GetOutAndShoot.cs", $"Giving Suspect weapon: {Weapon}");
                                i.Inventory.GiveNewWeapon(Weapon, 100, true);
                            }

                        }
                    }
                    catch (System.Threading.ThreadAbortException TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
                    }
                    catch (Exception TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
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
                            if (i.Exists())
                            {
                                Normal("GetOutAndShoot.cs", "Setting Suspect relationship group");
                                i.RelationshipGroup = SuspectRelateGroup;
                                Normal("GetOutAndShoot.cs", "Making Suspect leave vehicle");
                                i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                                Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
                                i.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7000);
                            }

                        }
                        catch (System.Threading.ThreadAbortException TheseHands)
                        {
                            string ThrowHands = TheseHands.ToString();
                            Error("GetOutAndShoot.cs", $"{ThrowHands}");
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
                bool PulloverEnded = false;

                foreach (Ped i in PedsInVehicle)
                {
                    try // Me
                    {

                        if (Chance < 45)
                        {
                            Normal("GetOutAndShoot.cs", "Making Suspect enter vehicle");
                            PursuitOutcome(PedsInVehicle);
                            break;
                        }
                        else if (Chance > 45)
                        {
                            Normal("GetOutAndShoot.cs", "Giving Suspect FightAgainstClosestHatedTarget Task");
                            if (!PulloverEnded)
                            {
                                Functions.ForceEndCurrentPullover();
                                PulloverEnded = true;
                            }

                            i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                        }
                    }
                    catch (System.Threading.ThreadAbortException TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
                    }
                    catch (Exception TheseHands)
                    {
                        string ThrowHands = TheseHands.ToString();
                        Error("GetOutAndShoot.cs", $"{ThrowHands}");
                    }
                }
            }
            catch (System.Threading.ThreadAbortException TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error("GetOutAndShoot.cs", $"{ThrowHands}");
            }
            catch (Exception TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error("GetOutAndShoot.cs", $"{ThrowHands}");
            }
            
        }

        internal static void PursuitOutcome(List<Ped> PedList)
        {
            try
            {
                int Seat = -2;
                foreach (Ped i in PedList)
                {
                    i.Tasks.EnterVehicle(suspectVehicle, (Seat + 1), 2f);
                }

                GameFiber.WaitUntil(() => PedList.Last().IsInAnyVehicle(true));
                PursuitLHandle = SetupPursuitWithList(true, PedList);
            }
            catch (System.Threading.ThreadAbortException TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error("GetOutAndShoot.cs", $"{ThrowHands}");
            }
            catch (Exception TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error("GetOutAndShoot.cs", $"{ThrowHands}");
            }
        }
    }
}