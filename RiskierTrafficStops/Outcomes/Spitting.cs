using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Spitting
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;

        internal static string[] spittingText = new string[]
        {
            "~y~Suspect: ~w~*spits at you* Fuck you pig",
            "~y~Suspect: ~w~*spits at you* Bitch",
            "~y~Suspect: ~w~*spits at you* Come on lets fight!",
            "~y~Suspect: ~w~*spits at you* Motherfucker",
            "~y~Suspect: ~w~*spits at you* Shit I didn't mean to hit you officer",
            "~y~Suspect: ~w~*spits at you* Damnit I didn't see you there",
            "~y~Suspect: ~w~*spits at you* ACAB!",
            "~y~Suspect: ~w~*spits at you and misses* You little bitch",
            "~y~Suspect: ~w~*spits at you and misses* Agh what did I do now",
            "~y~Suspect: ~w~*spits at you and misses* Ope sorry!",
            "~y~Suspect: ~w~*spits at you and hits badge* Haha little bitch",
            "~y~Suspect: ~w~*spits at you and hits badge* I should shoot you for pulling me over",
            "~y~Suspect: ~w~*spits at you and hits badge* I AM SO SORRY OFFICER",
            "~y~Suspect: ~w~*spits at you and hits badge* Oh fuck off",
            "~y~Suspect: ~w~*spits at you and hits shoe* ACAB Bitch!",
            "~y~Suspect: ~w~*spits at you and hits shoe* Fuckin pig",
            "~y~Suspect: ~w~*spits at you and hits shoe* Fucking pig",
            "~y~Suspect: ~w~*spits at you and hits shoe* Your a bitch you know that?",
            "~y~Suspect: ~w~*spits at you and hits shoe* Screw you pig",
            "~y~Suspect: ~w~*spits at you and hits shoe* What are you gonna do now, huh?",
            "~y~Suspect: ~w~*spits at you and hits shoe* Whatcha gonna do you little bitch?",
            "~y~Suspect: ~w~*spits at you and hits shoe* Where's your little squad of bitches?",
        };

        internal static void SpittingOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                GameFiber.WaitUntil(() => Suspect.Exists() && MainPlayer.DistanceTo(Suspect) <= 2f && Suspect.IsInAnyVehicle(true), 120000);
                if (MainPlayer.DistanceTo(Suspect) <= 2f && Suspect.IsInAnyVehicle(true))
                {
                    Game.DisplaySubtitle(spittingText[rndm.Next(spittingText.Length)], 6000);
                    Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                }
                PulloverEventHandler.HasEventHappend = false;
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Error(e, "Spitting.cs");
            }
        }
    }
}