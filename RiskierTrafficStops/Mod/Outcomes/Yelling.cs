using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;
using static RiskierTrafficStops.Engine.InternalSystems.Localization;

namespace RiskierTrafficStops.Mod.Outcomes;

internal sealed class Yelling : Outcome, IProccessing
{
    private static readonly YellingScenarioOutcomes[] AllYellingOutcomes =
        (YellingScenarioOutcomes[])Enum.GetValues(typeof(YellingScenarioOutcomes));

    public Yelling(LHandle handle) : base(handle)
    {
        TryStartOutcomeFiber(StartOutcome);
        SuspectRelateGroup = new RelationshipGroup("RTSYellingSuspects");
    }

    private enum YellingScenarioOutcomes
    {
        GetBackInVehicle,
        ContinueYelling,
        PullOutKnife
    }

    private static YellingScenarioOutcomes _chosenOutcome;

    private void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(Start));
        Normal("Adding all suspect in the vehicle to a list");
        var pedsInVehicle = new List<Ped>();
        if (SuspectVehicle.IsAvailable())
        {
            pedsInVehicle = SuspectVehicle.Occupants.ToList();
        }

        if (pedsInVehicle.Count < 1)
        {
            CleanupOutcome(true);
            return;
        }

        RemoveIgnoredPedsAndBlockEvents(ref pedsInVehicle);

        Normal("Making Suspect Leave Vehicle");
        Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(30000);
        Normal("Making Suspect Face Player");
        NativeFunction.Natives.x5AD23D40115353AC(Suspect, MainPlayer, -1);

        Normal("Making suspect Yell at Player");
        const int timesToSpeak = 2;

        for (var i = 0; i < timesToSpeak; i++)
        {
            Normal($"Making Suspect Yell, time: {i}");
            Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
            GameFiber.WaitWhile(() => Suspect.IsAnySpeechPlaying, 30000);
        }

        Normal("Choosing outcome from possible Yelling outcomes");
        _chosenOutcome = AllYellingOutcomes[Rndm.Next(AllYellingOutcomes.Length)];
        Normal($"Chosen Outcome: {_chosenOutcome}");

        switch (_chosenOutcome)
        {
            case YellingScenarioOutcomes.GetBackInVehicle:
                Suspect.Tasks.EnterVehicle(SuspectVehicle, -1);
                break;
            case YellingScenarioOutcomes.PullOutKnife:
                OutcomePullKnife();
                break;
            case YellingScenarioOutcomes.ContinueYelling:
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(KeyPressed));
                while (!Suspect.IsInAnyVehicle(false))
                {
                    GameFiber.Yield();
                    Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                    GameFiber.WaitWhile(() => Suspect.IsAnySpeechPlaying);
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
            $"~BLIP_INFO_ICON~ Press ~{UserConfig.GetBackInKey.GetInstructionalId()}~ {YellingNotiText}",
            10000);
        while (SuspectVehicle.IsAvailable() && !Suspect.IsInAnyVehicle(false))
        {
            GameFiber.Yield();
            if (Game.IsKeyDown(UserConfig.GetBackInKey))
            {
                Suspect.Tasks.EnterVehicle(SuspectVehicle, -1).WaitForCompletion();
                break;
            }
        }
    }

    private static void OutcomePullKnife()
    {
        Suspect.Inventory.GiveNewWeapon(MeleeWeapons[Rndm.Next(MeleeWeapons.Length)], -1, true);

        SetRelationshipGroups(SuspectRelateGroup);
        Suspect.RelationshipGroup = SuspectRelateGroup;

        Normal("Giving Suspect FighAgainst Task");
        Suspect.Tasks.FightAgainst(MainPlayer, -1);
    }

    // Processing methods
    public void Start()
    {
        Normal($"Started checks for {ActiveOutcome}");

        while (ActiveOutcome is not null)
        {
            GameFiber.Yield();
            if (Functions.GetCurrentPullover() is null || !Suspect.IsAvailable() || Functions.IsPedArrested(Suspect) ||
                Functions.IsPedGettingArrested(Suspect) || !MainPlayer.IsAvailable())
            {
                Abort();
            }
        }
    }

    public void Abort()
    {
        CleanupOutcome(false);
    }
}