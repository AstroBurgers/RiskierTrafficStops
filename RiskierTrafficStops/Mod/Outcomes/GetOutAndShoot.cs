using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.Extensions;

// ReSharper disable HeapView.BoxingAllocation

namespace RiskierTrafficStops.Mod.Outcomes
{
    internal class GetOutAndShoot : Outcome
    {
        private static GetOutAndShootOutcomes _chosenOutcome;
        private enum GetOutAndShootOutcomes
        {
            Flee,
            KeepShooting
        }

        // RTSGetOutAndShootSuspects
        public GetOutAndShoot(string RelateGroup, LHandle handle) : base(RelateGroup, handle)
        {
            try
            {
                if (MeetsRequirements(TrafficStopLHandle))
                {
                    GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(StartOutcome));
                CleanupOutcome();
            }
        }

        internal override void StartOutcome()
        {
            APIs.InvokeEvent(RTSEventType.Start);

            Normal("Adding all suspect in the vehicle to a list");
            var pedsInVehicle = SuspectVehicle.Occupants;
            if (pedsInVehicle.Length < 1) throw new ArgumentNullException(nameof(pedsInVehicle));

            SetRelationshipGroups(SuspectRelateGroup);

            foreach (Ped ped in pedsInVehicle)
            {
                ped.GiveWeapon();
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetPedOutOfVehicle(ped)));
            }

            GameFiber.Wait(7010);

            Normal("Choosing outcome from GetOutAndShootOutcomes");
            var scenarioList = (GetOutAndShootOutcomes[])Enum.GetValues(typeof(GetOutAndShootOutcomes));
            _chosenOutcome = scenarioList[Rndm.Next(scenarioList.Length)];
            Normal($"Chosen Outcome: {_chosenOutcome}");

            switch (_chosenOutcome)
            {
                case GetOutAndShootOutcomes.Flee:
                    if (Functions.GetCurrentPullover() == null)
                    {
                        CleanupEvent();
                        return;
                    }

                    PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                    break;
                case GetOutAndShootOutcomes.KeepShooting:
                    foreach (var i in pedsInVehicle)
                    {
                        if (i.IsAvailable())
                        {
                            Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
                            i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            APIs.InvokeEvent(RTSEventType.End);
        }

        private static void GetPedOutOfVehicle(Ped ped)
        {
            ped.RelationshipGroup = SuspectRelateGroup;
            if (ped.IsInVehicle(ped.LastVehicle, false) && ped.LastVehicle.IsAvailable())
            {
                Normal("Making Suspect leave vehicle");
                ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
            }
            Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
            ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7001);
        }
    }
}