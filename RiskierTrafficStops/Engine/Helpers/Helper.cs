using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;

namespace RiskierTrafficStops.Engine.Helpers
{
    internal static class Helper
    {
        internal static Ped MainPlayer => Game.LocalPlayer.Character;
        internal static Random Rndm = new(DateTime.Now.Millisecond);

        private static string _missingFiles = string.Empty;
        
        internal static bool VerifyDependencies()
        {
            if (!File.Exists("RAGENativeUI.dll")) _missingFiles += "~n~- RAGENativeUI.dll";
            
            if (_missingFiles.Length > 0)
            {
                Normal($"Failed to load because of these required files were not found: {_missingFiles.Replace("~n~", "")}"); // note to astro: replacing ~n~ is important otherwise the log will look weird
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "RiskierTrafficStops", "~r~Missing files!", $"These files were not found: ~y~{_missingFiles}");
                //Game.UnloadActivePlugin(); // note to astro: prevents FileNotFoundException from being sent or textures not being seen.
                return false; // note to astro: returns the IsUpdateAvailable method to false, make sure this is the first thing in the if-statement otherwise other things will return true, or add '&& missingFiles.Length < 0' to those statements, it's personal preference
            }

            return true;
        }
        
        /// <summary>
        /// Setup a Pursuit with an Array of suspects
        /// </summary>
        /// <param name="isSuspectsPulledOver"></param>
        /// <param name="suspects"></param>
        /// <returns>PursuitLHandle</returns>

        internal static LHandle SetupPursuit(bool isSuspectsPulledOver, params Ped[] suspects)
        {
            if (isSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            var pursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

            for (var i = suspects.Length - 1; i >= 0; i--)
            {
                if (!suspects[i].Exists()) { continue; }
                Functions.AddPedToPursuit(pursuitLHandle, suspects[i]);
                RandomizePursuitAttributes(suspects[i]);
            }

            return pursuitLHandle;
        }
        
        internal static void RandomizePursuitAttributes(Ped suspect)
        {
            try
            {
                static float GenerateRandomFloat()
                {
                    return (float)Math.Round((float)(Rndm.NextDouble() * (2.0 - 0.1) + 0.1), 1);
                }
                
                PedPursuitAttributes attributes = Functions.GetPedPursuitAttributes(suspect);
                
                attributes.MinDrivingSpeed = MphToMps(Rndm.Next(35, 60));
                attributes.MaxDrivingSpeed = MphToMps(Rndm.Next(61, 201));

                attributes.HandlingAbility = GenerateRandomFloat();
                attributes.HandlingAbilityTurns = GenerateRandomFloat();

                attributes.BurstTireSurrenderMult = 2f;
                attributes.SurrenderChanceTireBurst = Rndm.Next(1, 31);
                attributes.SurrenderChanceTireBurstAndCrashed = Rndm.Next(1, 41);

                attributes.SurrenderChanceCarBadlyDamaged = Rndm.Next(1, 101);

                attributes.SurrenderChancePitted = Rndm.Next(1, 81);
                attributes.SurrenderChancePittedAndCrashed = Rndm.Next(1, 51);
                attributes.SurrenderChancePittedAndSlowedDown = Rndm.Next(1, 11);

                attributes.AverageBurstTireSurrenderTime = Rndm.Next(700, 2000);
                attributes.AverageSurrenderTime = Rndm.Next(1000, 3000);
                
                attributes.AverageFightTime = Rndm.Next(400, 2000);
                
                Logger.Normal($"MaxDrivingSpeed: {attributes.MaxDrivingSpeed}");
                Logger.Normal($"MinDrivingSpeed: {attributes.MinDrivingSpeed}");
                
                Logger.Normal($"HandlingAbility: {attributes.HandlingAbility}");
                Logger.Normal($"HandlingAbilityTurns: {attributes.HandlingAbilityTurns}");
                
                Logger.Normal($"BurstTireSurrenderMult: {attributes.BurstTireSurrenderMult}");
                Logger.Normal($"SurrenderChanceTireBurst: {attributes.SurrenderChanceTireBurst}");
                Logger.Normal($"SurrenderChanceTireBurstAndCrashed: {attributes.SurrenderChanceTireBurstAndCrashed}");
                
                Logger.Normal($"SurrenderChanceCarBadlyDamaged: {attributes.SurrenderChanceCarBadlyDamaged}");
                
                Logger.Normal($"SurrenderChancePitted: {attributes.SurrenderChancePitted}");
                Logger.Normal($"SurrenderChancePittedAndCrashed: {attributes.SurrenderChancePittedAndCrashed}");
                Logger.Normal($"SurrenderChancePittedAndSlowedDown: {attributes.SurrenderChancePittedAndSlowedDown}");
                
                Logger.Normal($"AverageBurstTireSurrenderTime: {attributes.AverageBurstTireSurrenderTime}");
                Logger.Normal($"AverageSurrenderTime: {attributes.AverageSurrenderTime}");
                
                Logger.Normal($"AverageFightTime: {attributes.AverageFightTime}");
            }
            catch (Exception e)
            {
                if (e is System.Threading.ThreadAbortException) return;
                Logger.Error(e, nameof(RandomizePursuitAttributes));
            }
        }
        
        /// <summary>
        /// Same as SetupPursuit but with a suspect list
        /// </summary>
        /// <param name="isSuspectsPulledOver">If the suspects are in a traffic stop</param>
        /// <param name="suspectList">The list of Suspects, Type=Ped</param>
        /// <returns>PursuitLHandle</returns>
        internal static LHandle SetupPursuitWithList(bool isSuspectsPulledOver, List<Ped> suspectList)
        {
            if (isSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            var pursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

            for (var i = suspectList.Count - 1; i >= 0; i--)
            {
                GameFiber.Yield();
                if (suspectList[i].IsAvailable())
                {
                    Functions.AddPedToPursuit(pursuitLHandle, suspectList[i]);
                    RandomizePursuitAttributes(suspectList[i]);
                }
            }
            return pursuitLHandle;
        }
        internal static LHandle SetupPursuitWithList(bool isSuspectsPulledOver, Ped[] suspectList)
        {
            if (isSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            var pursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

            for (var i = suspectList.Length - 1; i >= 0; i--)
            {
                GameFiber.Yield();
                if (suspectList[i].IsAvailable())
                {
                    Functions.AddPedToPursuit(pursuitLHandle, suspectList[i]);
                    RandomizePursuitAttributes(suspectList[i]);
                }
            }
            return pursuitLHandle;
        }

        /// <summary>
        /// Handles all relationship group changes
        /// </summary>
        /// <param name="suspectRelationshipGroup"></param>
        internal static void SetRelationshipGroups(RelationshipGroup suspectRelationshipGroup)
        {
            Normal("Setting up Suspect Relationship Group");
            suspectRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            suspectRelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            MainPlayer.RelationshipGroup.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate); //Relationship groups go both ways
            RelationshipGroup.Cop.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate);
        }

        internal static Vector3 GetRearOffset(Vehicle vehicle, float offset)
        {
            var backwardDirection = vehicle.RearPosition - vehicle.FrontPosition;
            backwardDirection.Normalize();
            return (backwardDirection * offset) + vehicle.RearPosition;
        }

        /// <summary>
        /// Returns the nearest vehicle to a position
        /// </summary>

        internal static Vehicle GetNearestVehicle(Vector3 position, float maxDistance = 40f)
        {
            var vehicles = MainPlayer.GetNearbyVehicles(16).ToList();
            if (vehicles.Count < 1)
                throw new ArgumentOutOfRangeException();
            var vehicle = vehicles.OrderBy(vehicles1 => vehicles1.DistanceTo(position)).ToList().First();
        
            return vehicle;
        }

        /// <summary>
        /// Returns the Driver and its vehicle
        /// </summary>
        /// <returns>Ped, Vehicle</returns>
        internal static bool GetSuspectAndSuspectVehicle(LHandle handle, out Ped suspect, out Vehicle suspectVehicle)
        {
            Ped driver = null;
            Vehicle driverVehicle = null;
            if ((handle != null) && Functions.IsPlayerPerformingPullover() && Functions.GetPulloverSuspect(handle).IsAvailable())
            {
                Normal("Setting up Suspect");
                driver = Functions.GetPulloverSuspect(handle);
                driver.BlockPermanentEvents = true;
            }
            if (driver.IsAvailable() && driver.IsInAnyVehicle(false) && !driver.IsInAnyPoliceVehicle)
            {
                Normal("Setting up Suspect Vehicle");
                driverVehicle = driver.LastVehicle;
            }
            Normal($"Returning Driver: {driver} & Driver Vehicle: {driverVehicle}");
            suspect = driver;
            suspectVehicle = driverVehicle;
            return suspect.IsAvailable() && suspectVehicle.IsAvailable();
        }

        internal static void CleanupEvent()
        {
            Normal("Cleaning up RTS Outcome...");
            PulloverEventHandler.HasEventHappened = false;
            GameFiberHandling.CleanupFibers();
            APIs.InvokeEvent(RTSEventType.End);
        }

        /// <summary>
        /// Converts MPH to meters per second which is what all tasks use, returns meters per second
        /// </summary>
        internal static float MphToMps(float speed)
        {
            return MathHelper.ConvertMilesPerHourToMetersPerSecond(speed);
        }

        /// <summary>
        /// List of (Almost) every weapon
        /// </summary>

        internal static readonly string[] WeaponList = {
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

        internal static readonly string[] PistolList =
        {
            "weapon_pistol",
            "weapon_pistol_mk2",
            "weapon_combatpistol",
            "weapon_appistol",
            "weapon_pistol50",
            "weapon_snspistol",
            "weapon_snspistol_mk2",
            "weapon_heavypistol",
            "weapon_microsmg",
        };

        /// <summary>
        /// List of all viable melee Weapons
        /// </summary>

        internal static readonly string[] MeleeWeapons = {
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
        internal static void RevEngine(Ped driver, Vehicle suspectVehicle, int[] timeBetweenRevs, int[] timeForRevsToLast, int totalNumberOfRevs)
        {
            Logger.Normal("Starting Rev Engine method");
            for (var i = 0; i < totalNumberOfRevs; i++)
            {
                GameFiber.Yield();
                var time = Rndm.Next(timeForRevsToLast[0], timeForRevsToLast[1]) * 1000;
                driver.Tasks.PerformDrivingManeuver(suspectVehicle, VehicleManeuver.RevEngine, time);
                GameFiber.Wait(time);
                var time2 = Rndm.Next(timeBetweenRevs[0], timeBetweenRevs[1]) * 1000;
                GameFiber.Wait(time2);
            }
        }

        internal static bool CheckZDistance(float z1, float z2, float range)
        {
            var difference = Math.Abs(z1 - z2);
            return difference <= range;
        }

        /// <summary>
        /// Array of Used curse voice-lines
        /// </summary>

        internal static readonly string[] VoiceLines = {
            "FIGHT",
            "GENERIC_INSULT_HIGH",
            "GENERIC_CURSE_MED",
            "CHALLENGE_THREATEN",
            "GENERIC_CURSE_HIGH",
            "GENERIC_INSULT_HIGH_01",
        };
    }
}