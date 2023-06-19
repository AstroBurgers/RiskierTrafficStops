using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskierTrafficStops.Systems
{
    internal class Helper
    {
        internal static Ped MainPlayer => Game.LocalPlayer.Character;
        internal static LHandle SetupPursuit(bool IsSuspectsPulledOver, params Ped[] Suspects)
        {
            if (IsSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            LHandle PursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(PursuitLHandle, true);

            foreach (Ped Suspect in Suspects)
            {
                GameFiber.Yield();
                Functions.AddPedToPursuit(PursuitLHandle, Suspect);
            }
            return PursuitLHandle;
        }

        internal static LHandle SetupPursuitWithList(bool IsSuspectsPulledOver, List<Ped> SuspectList)
        {
            if (IsSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            LHandle PursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(PursuitLHandle, true);

            for (int i = 0; i < SuspectList.Count; i++)
            {
                GameFiber.Yield();
                if (SuspectList[i].Exists())
                {
                    Functions.AddPedToPursuit(PursuitLHandle, SuspectList[i]);
                }
            }
            return PursuitLHandle;
        }

        /// <summary>
        /// Converts MPH to meters per second which is what all tasks use, returns meters per second
        /// </summary>
        internal static float MphToMps(float speed)
        {
            float newSpeed = MathHelper.ConvertMilesPerHourToMetersPerSecond(speed);
            return newSpeed;
        }

        /// <summary>
        /// List of (Almost) every weapon
        /// </summary>

        internal static String[] WeaponList = {
            "weapon_pistol",
            "weapon_pistol_mk2",
            "weapon_combatpistol",
            "weapon_appistol",
            "weapon_pistol50",
            "weapon_snspistol",
            "weapon_snspistol_mk2",
            "weapon_heavypistol",
            "weapon_vintagepistol",
            "weapon_microsmg",
            "weapon_smg",
            "weapon_smg_mk2",
            "weapon_assaultsmg",
            "weapon_combatpdw",
            "weapon_machinepistol",
            "weapon_minismg",
            "weapon_pumpshotgun",
            "weapon_pumpshotgun_mk2",
            "weapon_sawnoffshotgun",
            "weapon_assaultshotgun",
            "weapon_bullpupshotgun",
            "weapon_combatshotgun",
            "weapon_carbinerifle",
            "weapon_carbinerifle_mk2",
            "weapon_advancedrifle",
            "weapon_specialcarbine",
            "weapon_specialcarbine_mk2",
            "weapon_bullpuprifle",
            "weapon_bullpuprifle_mk2",
            "weapon_compactrifle",
            "weapon_militaryrifle",
            "weapon_tacticalrifle",
        };

        /// <summary>
        /// List of all viable melee Weapons
        /// </summary>

        internal static String[] meleeWeapons = new String[]
{
            "weapon_dagger",
            "weapon_bat",
            "weapon_bottle",
            "weapon_crowbar",
            "weapon_hammer",
            "weapon_hatchet",
            "weapon_knife",
            "weapon_switchblade",
            "weapon_machete",
            "weapon_wrench",
};

        /// <summary>
        /// Makes a ped rev their vehicles engine, the int list parameters each need a minimum and maximum value
        /// </summary>
        internal static void RevEngine(Ped driver, Vehicle SuspectVehicle, int[] timeBetweenRevs, int[] timeForRevsToLast, int TotalNumberOfRevs)
        {
            Random rndm = new Random();
            Logger.Normal("Starting Rev Engine method");
            for (int i = 0; i < TotalNumberOfRevs; i++)
            {
                GameFiber.Yield();
                int time = rndm.Next(timeForRevsToLast[0], timeForRevsToLast[1]) * 1000;
                driver.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.RevEngine, time);
                GameFiber.Wait(time);
                int time2 = rndm.Next(timeBetweenRevs[0], timeBetweenRevs[1]) * 1000;
                GameFiber.Wait(time2);
            }
        }

        internal static List<Ped> GetAllVehicleOccupants(Vehicle vehicle)
        {
            int seatCount = NativeFunction.Natives.GET_VEHICLE_NUMBER_OF_PASSENGERS<int>(vehicle, true, false);
            List<Ped> occupantList = new List<Ped>();
            for (int i = -1; i < seatCount; i++)
            {
                if (!vehicle.IsSeatFree(i))
                {
                    Ped ped = vehicle.GetPedOnSeat(i);
                    if (ped.Exists())
                    {
                        occupantList.Add(ped);
                    }
                }
            }
            return occupantList;
        }
    }
}