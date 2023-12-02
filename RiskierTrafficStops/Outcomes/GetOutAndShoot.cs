using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Threading;
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
        private static RelationshipGroup _suspectRelateGroup = new("RTSGetOutAndShootSuspects");
        private static LHandle _pursuitLHandle;
        private static ShootOutcomes _chosenOutcome;

        private enum ShootOutcomes
        {
            Flee,
            KeepShooting
        }

        internal static void GoasOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Debug("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                Debug("Adding all suspect in the vehicle to a list");

                var pedsInVehicle = _suspectVehicle.Occupants;
                if (pedsInVehicle.Length < 1) throw new ArgumentNullException(nameof(pedsInVehicle));

                SetRelationshipGroups(_suspectRelateGroup);

                foreach (Ped ped in pedsInVehicle)
                {
                    var weapon = WeaponList[Rndm.Next(WeaponList.Length)];
                    if (ped.IsAvailable())
                    {
                        if (!ped.Inventory.HasLoadedWeapon) { ped.Inventory.GiveNewWeapon(weapon, 100, true); Debug($"Giving Suspect weapon: {weapon}"); }
                    }
                    
                    GameFiber.StartNew(() => GetPedOutOfVehicle(ped));
                }
                GameFiber.Wait(7010);

                Debug("Choosing outcome from shootOutcomes");
                var scenarioList = (ShootOutcomes[])Enum.GetValues(typeof(ShootOutcomes));
                _chosenOutcome = scenarioList[Rndm.Next(scenarioList.Length)];
                Debug($"Chosen Outcome: {_chosenOutcome}");

                switch (_chosenOutcome)
                {
                    case ShootOutcomes.Flee:
                        _pursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                        break;
                    case ShootOutcomes.KeepShooting:
                        for (var i = pedsInVehicle.Length - 1; i >= 0; i--)
                        {
                            if (pedsInVehicle[i].IsAvailable())
                            {
                                Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                                pedsInVehicle[i].Tasks.FightAgainstClosestHatedTarget(40f, -1);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(GoasOutcome));
            }
        }
        private static void GetPedOutOfVehicle(Ped ped)
        {
            Debug("Making Suspect leave vehicle");
            ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
            Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
            ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7001);
        }
    }
}