namespace RiskierTrafficStops.Engine.Data;

internal static class Arrays
{
    internal static readonly string[] PluginLoadText =
    [
        "Watch your back out there, Officer!",
            "Time for chicago simulator...",
            "IS THAT A GUN!?",
            "Make sure your ready to draw at all times, Officer.",
            "Hope you don't get shot.",
            "POCKET FENTANYL!",
            "Thanks for installing!",
            "Looks like you corrupted the INI file. *jk*",
            "Watch out for the guy with an RPG.",
            "Don't get sued for police brutality!",
            "Current Objective: Survive."
    ];

    internal static readonly string[] PluginUnloadText =
    [
        "Let me guess, you crashed",
            "Is that you Echooo?",
            "Is that you Marcel?",
            "Hope you had a good patrol!",
            "Looks like you weren't shot at.",
            "Good policing, Officer!",
            "You have a pending brutality lawsuit against you.",
            "Everyone disliked that.",
            "Everyone liked that"
    ];

    internal static readonly string[] HostageSituationText =
    [
        "~y~Suspect(s)~s~: Move and their dead!",
            "~y~Suspect(s)~s~: Don't move or I'll shoot!",
            "~y~Suspect(s)~s~: If you value their life I'd stay put!",
            "~y~Suspect(s)~s~: We will fucking shoot them!",
            "~y~Suspect(s)~s~: I'd stay put if I were you!",
            "~y~Suspect(s)~s~: Do your job and fucking stay put!",
            "~y~Suspect(s)~s~: I'm not afraid to shoot them!",
            "~y~Suspect(s)~s~: I'll fucking kill them! Don't think I won't!",
            "~y~Suspect(s)~s~: I'll fucking do it!",
            "~y~Suspect(s)~s~: Listen here pig, I'll do it, so I suggest you stay put!",
            "~y~Suspect(s)~s~: You move and they are dead!"
    ];

    /// <summary>
    /// Text used in the spitting outcome
    /// </summary>
    internal static readonly string[] SpittingText =
    [
        "~y~Suspect: ~s~*spits at you* Fuck you pig",
            "~y~Suspect: ~s~*spits at you* Bitch",
            "~y~Suspect: ~s~*spits at you* Come on lets fight!",
            "~y~Suspect: ~s~*spits at you* Motherfucker",
            "~y~Suspect: ~s~*spits at you* Shit I didn't mean to hit you officer",
            "~y~Suspect: ~s~*spits at you* Damnit I didn't see you there",
            "~y~Suspect: ~s~*spits at you* ACAB!",
            "~y~Suspect: ~s~*spits at you and misses* You little bitch",
            "~y~Suspect: ~s~*spits at you and misses* Agh what did I do now",
            "~y~Suspect: ~s~*spits at you and misses* Ope sorry!",
            "~y~Suspect: ~s~*spits at you and hits badge* Haha little bitch",
            "~y~Suspect: ~s~*spits at you and hits badge* I should shoot you for pulling me over",
            "~y~Suspect: ~s~*spits at you and hits badge* I AM SO SORRY OFFICER",
            "~y~Suspect: ~s~*spits at you and hits badge* Oh fuck off",
            "~y~Suspect: ~s~*spits at you and hits shoe* ACAB Bitch!",
            "~y~Suspect: ~s~*spits at you and hits shoe* Fuckin pig",
            "~y~Suspect: ~s~*spits at you and hits shoe* Fucking pig",
            "~y~Suspect: ~s~*spits at you and hits shoe* Your a bitch you know that?",
            "~y~Suspect: ~s~*spits at you and hits shoe* Screw you pig",
            "~y~Suspect: ~s~*spits at you and hits shoe* What are you gonna do now, huh?",
            "~y~Suspect: ~s~*spits at you and hits shoe* Watcha gonna do you little bitch?",
            "~y~Suspect: ~s~*spits at you and hits shoe* Where's your little squad of bitches?",
            "~y~Suspect: ~s~*spits at you and hits gun* Oh look the bitch patrol!",
            "~y~Suspect: ~s~*spits at you and hits taser* Oh look the bitch patrol!"
    ];
    
    /// <summary>
    /// List of (Almost) every weapon
    /// </summary>
    internal static readonly string[] WeaponList =
    [
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
            "weapon_tacticalrifle"
    ];

    /// <summary>
    /// List of all Weapons that can be fired from inside a vehicle
    /// </summary>
    internal static readonly string[] PistolList =
    [
        "weapon_pistol",
            "weapon_pistol_mk2",
            "weapon_combatpistol",
            "weapon_appistol",
            "weapon_pistol50",
            "weapon_snspistol",
            "weapon_snspistol_mk2",
            "weapon_heavypistol",
            "weapon_microsmg"
    ];

    /// <summary>
    /// List of all viable melee Weapons
    /// </summary>
    internal static readonly string[] MeleeWeapons =
    [
        "weapon_dagger",
            "weapon_bat",
            "weapon_bottle",
            "weapon_crowbar",
            "weapon_hammer",
            "weapon_hatchet",
            "weapon_knife",
            "weapon_switchblade",
            "weapon_machete",
            "weapon_wrench"
    ];
    

    /// <summary>
    /// Array of Used curse voice-lines
    /// </summary>
    internal static readonly string[] VoiceLines =
    [
        "FIGHT",
            "GENERIC_INSULT_HIGH",
            "GENERIC_CURSE_MED",
            "CHALLENGE_THREATEN",
            "GENERIC_CURSE_HIGH",
            "GENERIC_INSULT_HIGH_01"
    ];
}