using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Yell
    {
        internal enum YellScenarioOutcomes
        {
            GetBackInVehicle,
            ContinueYelling,
            PullOutKnife
        }

        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup suspectRelationshipGroup = new RelationshipGroup("Suspect");
        internal static YellScenarioOutcomes chosenOutcome;
        internal static bool hasPedGottenBackIntoVehicle = false;


        internal static void YellOutcome(LHandle handle)
        {
            try
            {
                Suspect = GetSuspectAndVehicle(handle).Item1;
                suspectVehicle = GetSuspectAndVehicle(handle).Item2;


                Debug("Making Suspect Leave Vehicle");
                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                Debug("Making Suspect Face Player");
                NativeFunction.Natives.x5AD23D40115353AC(Suspect, MainPlayer, -1);

                Debug("Making suspect Yell at Player");
                int timesSpoken = 0;
                while (Suspect.Exists() && timesSpoken < 4)
                {
                    GameFiber.Yield();
                    timesSpoken += 1;
                    Debug("Suspect Is Yelling");
                    Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                    GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
                }

                Debug("Choosing outome from YellScenarioOutcomes");
                YellScenarioOutcomes[] ScenarioList = (YellScenarioOutcomes[])Enum.GetValues(typeof(YellScenarioOutcomes));
                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                Debug($"Chosen Outcome: {chosenOutcome}");

                if (!Suspect.Exists()) { return; }

                switch (chosenOutcome)
                {
                    case YellScenarioOutcomes.GetBackInVehicle:
                        Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                        break;
                    case YellScenarioOutcomes.PullOutKnife:
                        OutcomePullKnife();
                        break;
                    case YellScenarioOutcomes.ContinueYelling:
                        GameFiber.StartNew(KeyPressed);
                        while (!hasPedGottenBackIntoVehicle)
                        {
                            GameFiber.Yield();
                            Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                            if (!Suspect.Exists()) { return; }
                            GameFiber.WaitUntil(() => !Suspect.IsAnySpeechPlaying);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Error(e, "Yell.cs");
            }
        }
        internal static void KeyPressed()
        {
            Game.DisplayHelp($"~BLIP_INFO_ICON~ Press {Settings.GetBackIn.ToString()} To to have the suspect get back in their vehicle");
            while (Suspect.Exists() && !hasPedGottenBackIntoVehicle)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.GetBackIn))
                {
                    hasPedGottenBackIntoVehicle = true;
                    Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                }
            }
        }

        internal static void OutcomePullKnife()
        {
            if (Suspect.Exists())
            {
                Suspect.Inventory.GiveNewWeapon(meleeWeapons[rndm.Next(meleeWeapons.Length)], -1, true);

                Debug("Setting Suspect relationship group");
                Suspect.RelationshipGroup = suspectRelationshipGroup;
                suspectRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                suspectRelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                Suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
            }
        }
    }
}