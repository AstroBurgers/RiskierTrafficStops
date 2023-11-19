using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;
// ReSharper disable HeapView.BoxingAllocation

namespace RiskierTrafficStops.Outcomes
{
    internal static class GetOutAndShoot
    {
        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        private static RelationshipGroup _suspectRelateGroup = new RelationshipGroup("Suspect");
        private static LHandle _pursuitLHandle;
        private static ShootOutcomes _chosenOutcome;

        private enum ShootOutcomes
        {
            Flee,
            KeepShooting,
        }

        internal static void GoasOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    CleanupEvent(_suspect, _suspectVehicle);
                    return;
                }

                Debug("Adding all suspect in the vehicle to a list");

                var pedsInVehicle = GetAllVehicleOccupants(_suspectVehicle) ?? throw new ArgumentNullException(nameof(handle));
                if (pedsInVehicle == null) throw new ArgumentNullException(nameof(pedsInVehicle));
                Debug($"Peds In Vehicle: {pedsInVehicle.Count}");

                Debug("Setting up Suspect Relationship Group");
                _suspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                _suspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                MainPlayer.RelationshipGroup.SetRelationshipWith(_suspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
                RelationshipGroup.Cop.SetRelationshipWith(_suspectRelateGroup, Relationship.Hate);
                
                foreach (var i in pedsInVehicle)
                {
                    var weapon = WeaponList[Rndm.Next(WeaponList.Length)];
                    if (i.Exists()) { CleanupEvent(i); continue; }
                    if (!i.Inventory.HasLoadedWeapon) { i.Inventory.GiveNewWeapon(weapon, 100, true); Debug($"Giving Suspect weapon: {weapon}"); }
                    
                    GameFiber.StartNew(() => GetPedOutOfVehicle(i));
                }
                
                /*for (var i = pedsInVehicle.Count - 1; i >= 0; i--)
                {
                    var weapon = WeaponList[Rndm.Next(WeaponList.Length)];
                    if (!pedsInVehicle[i].Exists()) { CleanupEvent(pedsInVehicle[i]); continue; }
                    if (!pedsInVehicle[i].Inventory.HasLoadedWeapon) { pedsInVehicle[i].Inventory.GiveNewWeapon(weapon, 100, true); Debug($"Giving Suspect weapon: {weapon}"); }
                    
                    GameFiber.StartNew(() => GetPedOutOfVehicle(pedsInVehicle[i]));
                }*/
                GameFiber.Wait(7010);

                Debug("Choosing outcome from shootOutcomes");
                var scenarioList = (ShootOutcomes[])Enum.GetValues(typeof(ShootOutcomes));
                _chosenOutcome = scenarioList[Rndm.Next(scenarioList.Length)];
                Debug($"Chosen Outcome: {_chosenOutcome}");

                switch (_chosenOutcome)
                {
                    case ShootOutcomes.Flee:
                        PursuitOutcome(pedsInVehicle);
                        break;

                    case ShootOutcomes.KeepShooting:
                        if (Functions.IsPlayerPerformingPullover()) { Functions.ForceEndCurrentPullover(); }
                        for (var i = pedsInVehicle.Count - 1; i >= 0; i--)
                        {
                            if (!pedsInVehicle[i].Exists()) { CleanupEvent(pedsInVehicle[i]); continue; }
                            pedsInVehicle[i].Tasks.Clear();
                            Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                            pedsInVehicle[i].Tasks.FightAgainstClosestHatedTarget(40f, -1);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, nameof(GoasOutcome));
            }
        }

        private static void PursuitOutcome(List<Ped> pedList)
        {
            const int seat = -2;
            for (var i = 0; i < pedList.Count; i++)
            {
                if (pedList[i].Exists()) { CleanupEvent(pedList[i]); continue; }
                Debug($"Giving Ped task to enter vehicle: {i}");
                pedList[i].Tasks.Clear();

                pedList[i].Tasks.EnterVehicle(_suspectVehicle, (seat + 1), 2f);
            }

            _pursuitLHandle = SetupPursuitWithList(true, pedList);
        }

        private static void GetPedOutOfVehicle(Ped ped)
        {
            if (!ped.Exists()) return;
            Debug("Setting Suspect relationship group");
            ped.RelationshipGroup = _suspectRelateGroup;
            Debug("Making Suspect leave vehicle");
            ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
            Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
            ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7001);
        }
    }
}