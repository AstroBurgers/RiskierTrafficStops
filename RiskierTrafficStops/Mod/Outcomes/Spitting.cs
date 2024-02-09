using RiskierTrafficStops.Engine.Helpers.Extensions;
using static RiskierTrafficStops.Engine.Data.Arrays;
using static RiskierTrafficStops.Engine.Helpers.MathHelper;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class Spitting : Outcome
{
    public Spitting(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
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

    internal override void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);

        GameFiber.WaitWhile(
            () => Suspect.IsAvailable() && MainPlayer.DistanceTo(Suspect) >= 3f && Suspect.IsInAnyVehicle(true),
            120000);
        if (Functions.IsPlayerPerformingPullover() && Suspect.IsAvailable() &&
            MainPlayer.DistanceTo(Suspect) <= 2.5f && Suspect.IsInAnyVehicle(true))
        {
            Game.DisplaySubtitle(SpittingText[Rndm.Next(SpittingText.Length+1)], 6000);
            Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length+1)]);
        }

        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }
}