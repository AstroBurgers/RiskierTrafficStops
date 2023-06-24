using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Deployment.Internal;
using System.Runtime.CompilerServices;

namespace RiskierTrafficStops.Outcomes
{
    internal class GetOutAndShoot
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");
        internal static LHandle PursuitLHandle;

        internal static List<GameFiber> GameFibers = new List<GameFiber> { };

        internal static void GOASOutcome(LHandle handle)
        {
            try
            {
                Debug("Setting up Suspect");

                Suspect = Functions.GetPulloverSuspect(handle);
                Debug("Setting up suspectVehicle");
                suspectVehicle = Suspect.CurrentVehicle;
                Suspect.BlockPermanentEvents = true;
                Suspect.IsPersistent = true;
                suspectVehicle.IsPersistent = true;

                Debug("Adding all suspect in the vehicle to a list");

                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
                Debug($"Peds In Vehicle: {PedsInVehicle.Count}");

                Debug("Setting up SuspectRelateGroup");

                SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                foreach (Ped i in PedsInVehicle)
                {
                    string Weapon = WeaponList[rndm.Next(WeaponList.Length)];
                    if (i.Exists())
                    {
                        if (!i.Inventory.HasLoadedWeapon)
                        {
                            Debug($"Giving Suspect weapon: {Weapon}");
                            i.Inventory.GiveNewWeapon(Weapon, 100, true);

                            GameFiber.StartNew(() => GetPedOutOfVehicle(i));
                        }

                    }
                }

                GameFiber.Wait(7010);

                int Chance = rndm.Next(1, 101);

                foreach (Ped i in PedsInVehicle)
                {
                    if (Chance <= 45)
                    {
                        Debug("Making Suspect enter vehicle");
                        PursuitOutcome(PedsInVehicle);
                        break;
                    }
                    else if (Chance >= 45)
                    {
                        Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                        if (Functions.IsPlayerPerformingPullover())
                        {
                            Functions.ForceEndCurrentPullover();
                        }
                        i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                    }
                }
            }
            catch (System.Threading.ThreadAbortException TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error($"{ThrowHands}");
            }
            catch (Exception TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error($"{ThrowHands}");
            }

        }

        internal static void PursuitOutcome(List<Ped> PedList)
        {
            int Seat = -2;
            foreach (Ped i in PedList)
            {
                if (i.Exists())
                {
                    i.Tasks.EnterVehicle(suspectVehicle, (Seat + 1), 2f);
                    Debug($"{PedList.IndexOf(i)}");
                }
            }

            PursuitLHandle = SetupPursuitWithList(true, PedList);
        }

        internal static void GetPedOutOfVehicle(Ped ped)
        {
            Normal("Setting Suspect relationship group");
            ped.RelationshipGroup = SuspectRelateGroup;
            Normal("Making Suspect leave vehicle");
            ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
            ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7000);
        }
    }
}