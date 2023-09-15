using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Systems;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class GetOutAndShoot
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");
        internal static LHandle PursuitLHandle;
        internal static shootOutcomes chosenOutcome;

        internal enum shootOutcomes
        {
            Flee,
            KeepShooting,
        }

        internal static void GOASOutcome(LHandle handle)
        {
            try
            {
                var e = GetSuspectAndVehicle(handle);
                Suspect = e.Suspect;
                suspectVehicle = e.suspectVehicle;

                if (!Suspect.Exists()) { CleanupEvent(Suspect, suspectVehicle); return; }

                Debug("Adding all suspect in the vehicle to a list");

                List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
                Debug($"Peds In Vehicle: {PedsInVehicle.Count}");

                Debug("Setting up SuspectRelateGroup");

                SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                MainPlayer.RelationshipGroup.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
                RelationshipGroup.Cop.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate);

                foreach (Ped i in PedsInVehicle)
                {
                    string Weapon = WeaponList[rndm.Next(WeaponList.Length)];
                    if (!i.Exists()) { Helper.CleanupEvent(PedsInVehicle, suspectVehicle); return; }
                    if (!i.Inventory.HasLoadedWeapon) { i.Inventory.GiveNewWeapon(Weapon, 100, true); Debug($"Giving Suspect weapon: {Weapon}"); }

                    GameFiber.StartNew(() => GetPedOutOfVehicle(i));
                }

                GameFiber.Wait(7010);

                Debug("Choosing outome from shootOutcomes");
                shootOutcomes[] ScenarioList = (shootOutcomes[])Enum.GetValues(typeof(shootOutcomes));
                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                Debug($"Chosen Outcome: {chosenOutcome}");

                switch (chosenOutcome)
                {
                    case shootOutcomes.Flee:
                        PursuitOutcome(PedsInVehicle);
                        break;
                    case shootOutcomes.KeepShooting:
                        if (Functions.IsPlayerPerformingPullover()) { Functions.ForceEndCurrentPullover(); }
                        foreach (Ped i in PedsInVehicle)
                        {
                            if (!i.Exists()) { Helper.CleanupEvent(PedsInVehicle, suspectVehicle); return; }
                            Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                            i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                        }
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "GetOutAndShoot.cs");
            }

        }

        internal static void PursuitOutcome(List<Ped> PedList)
        {
            int Seat = -2;
            foreach (Ped i in PedList)
            {
                if (!i.Exists()) { Helper.CleanupEvent(PedList, suspectVehicle); return; }

                Debug("Giving Ped task to enter vehicle");
                i.Tasks.EnterVehicle(suspectVehicle, (Seat + 1), 2f);
                Debug($"{PedList.IndexOf(i)}");
            }

            PursuitLHandle = SetupPursuitWithList(true, PedList);
        }

        internal static void GetPedOutOfVehicle(Ped ped)
        {
            if (ped.Exists())
            {
                Debug("Setting Suspect relationship group");
                ped.RelationshipGroup = SuspectRelateGroup;
                Debug("Making Suspect leave vehicle");
                ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7000);
            }
        }
    }
}