using System;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response;
using Rage.Native;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Deployment.Internal;
using System.Runtime.Serialization;

namespace RiskierTrafficStops.Outcomes
{
    internal class Yell
    {
        internal enum YellScen
        {
            GetIn,
            YellMore,
            PullKnife
        }

        internal static List<string> Voicelines = new List<string>()
        {
            "FIGHT",
            "GENERIC_INSULT_HIGH",
            "GENERIC_CURSE_MED",
            "CHALLENGE_THREATEN",
            "GENERIC_CURSE_HIGH",
            "GENERIC_INSULT_HIGH_01",
        };

        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");
        internal static YellScen ChosenEnum;
        internal static bool GenericBoolean = false;


        internal static void YellOutcome(LHandle handle)
        {
            try
            {
                Debug("Setting up Suspect and Suspect Vehicle");
                Suspect = Functions.GetPulloverSuspect(handle);
                suspectVehicle = Suspect.CurrentVehicle;
                Suspect.BlockPermanentEvents = true;
                Suspect.IsPersistent = true;
                suspectVehicle.IsPersistent = true;

                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                NativeFunction.Natives.x5AD23D40115353AC(Suspect, MainPlayer, -1);
                Suspect.PlayAmbientSpeech("FIGHT");
                GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
                GameFiber.Wait(500);
                Suspect.PlayAmbientSpeech("GENERIC_INSULT_HIGH");
                GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
                GameFiber.Wait(500);
                Suspect.PlayAmbientSpeech("GENERIC_CURSE_MED_03");
                GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
                GameFiber.Wait(500);

                YellScen[] ScenarioList = (YellScen[])Enum.GetValues(typeof(YellScen));
                ChosenEnum = ScenarioList[rndm.Next(ScenarioList.Length)];

                if (ChosenEnum == YellScen.GetIn)
                {
                    Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                }

                else if (ChosenEnum == YellScen.PullKnife)
                {
                    Suspect.Inventory.GiveNewWeapon(meleeWeapons[rndm.Next(meleeWeapons.Length)], -1, true);

                    Debug("Setting Suspect relationship group");
                    Suspect.RelationshipGroup = SuspectRelateGroup;
                    SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                    SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                    Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                    Suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                }

                else if (ChosenEnum == YellScen.YellMore)
                {
                    GameFiber.StartNew(KeyPressed);
                    while (!GenericBoolean)
                    {
                        GameFiber.Yield();
                        Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Count)]);
                        GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
                    }
                }
            }
            catch (Exception TheseHands)
            {
                string ThrowHands = TheseHands.ToString();
                Error($"{ThrowHands}");
            }
        }
        internal static void KeyPressed()
        {
            Game.DisplayHelp($"~BLIP_INFO_ICON~ Press {Settings.GetBackIn.ToString()} To to have the suspect get back in their vehicle");
            while (!GenericBoolean)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.GetBackIn))
                {
                    GenericBoolean = true;
                    Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                }
            }
        }
    }
}