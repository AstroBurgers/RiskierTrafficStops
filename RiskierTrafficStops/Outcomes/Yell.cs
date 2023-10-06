using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RiskierTrafficStops.Systems;
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
        internal static RelationshipGroup suspectRelationshipGroup = new("Suspect");
        internal static YellScenarioOutcomes chosenOutcome;
        internal static bool hasPedGottenBackIntoVehicle = false;


        internal static void YellOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                Debug("Making Suspect Leave Vehicle");
                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                Debug("Making Suspect Face Player");
                NativeFunction.Natives.x5AD23D40115353AC(Suspect, MainPlayer, -1);

                Debug("Making suspect Yell at Player");
                int timesSpoken = 0;
                while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && timesSpoken < 3)
                {
                    GameFiber.Yield();
                    timesSpoken += 1;
                    Debug("Suspect Is Yelling");
                    Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                    GameFiber.WaitWhile(() => Suspect.Exists() && Suspect.IsAnySpeechPlaying);
                }

                Debug("Choosing outome from YellScenarioOutcomes");
                YellScenarioOutcomes[] ScenarioList = (YellScenarioOutcomes[])Enum.GetValues(typeof(YellScenarioOutcomes));
                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                Debug($"Chosen Outcome: {chosenOutcome}");

                switch (chosenOutcome)
                {
                    case YellScenarioOutcomes.GetBackInVehicle:
                        if (Suspect.Exists() && !Functions.IsPedArrested(Suspect)) //Double checking if suspect exists
                        {
                            Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                        }
                        break;
                    case YellScenarioOutcomes.PullOutKnife:
                        OutcomePullKnife();
                        break;
                    case YellScenarioOutcomes.ContinueYelling:
                        GameFiber.StartNew(KeyPressed);
                        while (!hasPedGottenBackIntoVehicle && Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                        {
                            GameFiber.Yield();
                            Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                            GameFiber.WaitWhile(() => Suspect.Exists() && Suspect.IsAnySpeechPlaying);
                        }
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "Yell.cs");
            }
        }
        internal static void KeyPressed()
        {
            Game.DisplayHelp($"~BLIP_INFO_ICON~ Press {Settings.GetBackIn} To to have the suspect get back in their vehicle");
            while (Suspect.Exists() && !hasPedGottenBackIntoVehicle)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.GetBackIn))
                {
                    hasPedGottenBackIntoVehicle = true;
                    Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                    break;
                }
            }
        }

        internal static void OutcomePullKnife()
        {
            if (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && !Functions.IsPedGettingArrested(Suspect))
            {
                Suspect.Inventory.GiveNewWeapon(meleeWeapons[rndm.Next(meleeWeapons.Length)], -1, true);

                Debug("Setting Suspect relationship group");
                Suspect.RelationshipGroup = suspectRelationshipGroup;
                suspectRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                suspectRelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                MainPlayer.RelationshipGroup.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate);
                RelationshipGroup.Cop.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate); //Relationship groups work both ways

                Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                Suspect.BlockPermanentEvents = true;
                Suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
            }
        }
    }
}