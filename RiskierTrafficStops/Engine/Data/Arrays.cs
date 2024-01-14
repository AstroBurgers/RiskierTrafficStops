namespace RiskierTrafficStops.Engine.Data;

public class Arrays
{
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