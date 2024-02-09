using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class Yelling : Outcome
{
    private static readonly YellingScenarioOutcomes[] AllYellingOutcomes =
        (YellingScenarioOutcomes[])Enum.GetValues(typeof(YellingScenarioOutcomes));
    
    public Yelling(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
                SuspectRelateGroup = new RelationshipGroup("RTSYellingSuspects");
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, nameof(StartOutcome));
            CleanupOutcome(true);
        }
    }

    private enum YellingScenarioOutcomes
    {
        GetBackInVehicle,
        ContinueYelling,
        PullOutKnife
    }

    private static YellingScenarioOutcomes _chosenOutcome;

    internal override void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);

        Normal("Making Suspect Leave Vehicle");
        Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(30000);
        Normal("Making Suspect Face Player");
        NativeFunction.Natives.x5AD23D40115353AC(Suspect, MainPlayer, -1);

        Normal("Making suspect Yell at Player");
        const int timesToSpeak = 2;

        for (var i = 0; i < timesToSpeak; i++)
        {
            if (Suspect.IsAvailable())
            {
                Normal($"Making Suspect Yell, time: {i}");
                Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                GameFiber.WaitWhile(() => Suspect.IsAvailable() && Suspect.IsAnySpeechPlaying, 30000);
            }
        }

        Normal("Choosing outcome from possible Yelling outcomes");
        _chosenOutcome = AllYellingOutcomes[Rndm.Next(AllYellingOutcomes.Length)];
        Normal($"Chosen Outcome: {_chosenOutcome}");

        switch (_chosenOutcome)
        {
            case YellingScenarioOutcomes.GetBackInVehicle:
                if (Suspect.IsAvailable() && !Functions.IsPedArrested(Suspect)) //Double checking if suspect exists
                {
                    Suspect.Tasks.EnterVehicle(SuspectVehicle, -1);
                }

                break;
            case YellingScenarioOutcomes.PullOutKnife:
                OutcomePullKnife();
                break;
            case YellingScenarioOutcomes.ContinueYelling:
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(KeyPressed));
                while (!Suspect.IsInAnyVehicle(false) && Suspect.IsAvailable() &&
                       (!Functions.IsPedArrested(Suspect) || Functions.IsPedGettingArrested(Suspect)))
                {
                    GameFiber.Yield();
                    Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                    GameFiber.WaitWhile(() => Suspect.IsAvailable() && Suspect.IsAnySpeechPlaying);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }

    private static void KeyPressed()
    {
        Game.DisplayHelp(
            $"~BLIP_INFO_ICON~ Press ~{GetBackInKey.GetInstructionalId()}~ to have the suspect get back in their vehicle",
            10000);
        while (Suspect.IsAvailable() && SuspectVehicle.IsAvailable() && !Suspect.IsInAnyVehicle(false))
        {
            GameFiber.Yield();
            if (Game.IsKeyDown(GetBackInKey))
            {
                Suspect.Tasks.EnterVehicle(SuspectVehicle, -1).WaitForCompletion();
                break;
            }
        }
    }

    private static void OutcomePullKnife()
    {
        if (!Suspect.IsAvailable() || Functions.IsPedArrested(Suspect) || Functions.IsPedGettingArrested(Suspect))
            return;

        Suspect.Inventory.GiveNewWeapon(MeleeWeapons[Rndm.Next(MeleeWeapons.Length)], -1, true);

        SetRelationshipGroups(SuspectRelateGroup);
        Suspect.RelationshipGroup = SuspectRelateGroup;

        Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
        Suspect.Tasks.FightAgainst(MainPlayer, -1);
    }
}