using RiskierTrafficStops.Engine.InternalSystems;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class HostageTaking : Outcome
{
    public HostageTaking(LHandle handle) : base(handle)
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
            
        Normal("Getting all vehicle occupants");
        var pedsInVehicle = SuspectVehicle.Occupants.ToList();

        foreach (var ped in pedsInVehicle)
        {
            if (ped.IsAvailable() && PedsToIgnore.Contains(ped))
            {
                pedsInVehicle.Remove(ped);
            }
        }
        
        if (pedsInVehicle.Count <= 1) CleanupOutcome(true);
    }
}

internal class Suspect : Ped
{
    internal Ped suspect;
    
    internal bool IsSuicidal = false;
    internal bool WantToSurvive = false;
    internal bool HatesHostage = false;
    internal bool HasAutomaticWeapon = false;
    
    internal Suspect(Ped ped)
    {
        suspect = ped;
        IsSuicidal = GenerateChance() < 40;
        WantToSurvive = GenerateChance() < 30;
        HatesHostage = GenerateChance() < 20;
    }
}