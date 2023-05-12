using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskierTrafficStops
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
    }
}
