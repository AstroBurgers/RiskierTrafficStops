using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Systems
{
    internal class Helper
    {
        internal static Ped MainPlayer => Game.LocalPlayer.Character;
        internal static Random rndm = new(DateTime.Now.Millisecond);

        /// <summary>
        /// Setup a Pursuit with an Array of suspects
        /// </summary>
        /// <param name="IsSuspectsPulledOver"></param>
        /// <param name="Suspects"></param>
        /// <returns>PursuitLHandle</returns>

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
                if (Suspect.Exists())
                {
                    GameFiber.Yield();
                    Functions.AddPedToPursuit(PursuitLHandle, Suspect);
                }
            }
            return PursuitLHandle;
        }

        /// <summary>
        /// Returns the Driver and its vehicle
        /// </summary>
        /// <returns>Ped, Vehicle</returns>

        internal static (Ped Suspect, Vehicle suspectVehicle) GetSuspectAndVehicle(LHandle handle)
        {
            Ped driver = null;
            Vehicle driverVehicle = null;
            if ((handle != null) && Functions.IsPlayerPerformingPullover())
            {
                Debug("Setting up Suspect");
                driver = Functions.GetPulloverSuspect(handle);
                Debug("Setting driver as persistent and Blocking permanent events");
                driver.IsPersistent = true;
                driver.BlockPermanentEvents = true;
            }
            if (driver && driver.IsInAnyVehicle(false) && !driver.IsInAnyPoliceVehicle)
            {
                Debug("Setting up Suspect Vehicle");
                driverVehicle = driver.LastVehicle;
                Debug("Setting driver vehicle as Persistent");
                driverVehicle.IsPersistent = true;
            }
            Debug($"Returning {driver} & {driverVehicle}");
            return (driver, driverVehicle);
        }


        internal static void CleanupEvent(List<Ped> Peds, Vehicle vehicle)
        {
            foreach (Ped i in Peds)
            {
                if (i.Exists())
                {
                    i.IsPersistent = false;
                }
                else if (vehicle.Exists())
                {
                    vehicle.IsPersistent = false;
                }
            }
        }

        internal static void CleanupEvent(Ped Suspect, Vehicle vehicle)
        {
            if (Suspect.Exists())
            {
                Suspect.IsPersistent = false;
            }
            else if (vehicle.Exists())
            {
                vehicle.IsPersistent = false;
            }

            PulloverEvents.HasEventHappend = false;
        }

        /// <summary>
        /// Same as SetupPursuit but with a suspect list
        /// </summary>
        /// <param name="IsSuspectsPulledOver">If the suspects are in a traffic stop</param>
        /// <param name="SuspectList">The list of Suspects, Type=Ped</param>
        /// <returns>PursuitLHandle</returns>

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
        /// List of all Weapons that can be fired from inside of a vehicle
        /// </summary>

        internal static String[] pistolList =
        {
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
            Logger.Debug("Starting Rev Engine method");
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
            int seatCount = vehicle.PassengerCount; //Testing rph method instead of NativeFunction.Natives.GET_VEHICLE_NUMBER_OF_PASSENGERS<int>(vehicle, true, false);
            List<Ped> occupantList = new();
            occupantList.Add(vehicle.GetPedOnSeat(-1)); //vehicle.PassengerCount does not include the driver, so driver is being added here
            for (int i = 0; i < seatCount; i++)
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
            Logger.Debug($"Peds In Vehicle: {occupantList.Count}");
            return occupantList;
        }

        /// <summary>
        /// Array of Used curse voicelines
        /// </summary>

        internal static String[] Voicelines = new String[]
        {
            "FIGHT",
            "GENERIC_INSULT_HIGH",
            "GENERIC_CURSE_MED",
            "CHALLENGE_THREATEN",
            "GENERIC_CURSE_HIGH",
            "GENERIC_INSULT_HIGH_01",
        };
    }
}