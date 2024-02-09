namespace RiskierTrafficStops.Engine.Helpers;

internal static class PursuitHelper
{
    /// <summary>
    /// Setup a Pursuit with an Array of suspects
    /// </summary>
    /// <param name="isSuspectsPulledOver"></param>
    /// <param name="suspects"></param>
    /// <returns>PursuitLHandle</returns>

    internal static LHandle SetupPursuit(bool isSuspectsPulledOver, params Ped[] suspects)
    {
        if (isSuspectsPulledOver)
        {
            Functions.ForceEndCurrentPullover();
        }
        var pursuitLHandle = Functions.CreatePursuit();

        Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

        for (var i = suspects.Length - 1; i >= 0; i--)
        {
            if (!suspects[i].Exists()) { continue; }
            Functions.AddPedToPursuit(pursuitLHandle, suspects[i]);
            RandomizePursuitAttributes(suspects[i]);
        }

        return pursuitLHandle;
    }
    
    /// <summary>
    /// Same as SetupPursuit but with a suspect list
    /// </summary>
    /// <param name="isSuspectsPulledOver">If the suspects are in a traffic stop</param>
    /// <param name="suspectList">The list of Suspects, Type=Ped</param>
    /// <returns>PursuitLHandle</returns>
    internal static LHandle SetupPursuitWithList(bool isSuspectsPulledOver, List<Ped> suspectList)
    {
        if (isSuspectsPulledOver)
        {
            Functions.ForceEndCurrentPullover();
        }
        var pursuitLHandle = Functions.CreatePursuit();

        Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

        for (var i = suspectList.Count - 1; i >= 0; i--)
        {
            GameFiber.Yield();
            if (suspectList[i].IsAvailable())
            {
                Functions.AddPedToPursuit(pursuitLHandle, suspectList[i]);
                RandomizePursuitAttributes(suspectList[i]);
            }
        }
        return pursuitLHandle;
    }
    internal static LHandle SetupPursuitWithList(bool isSuspectsPulledOver, Ped[] suspectList)
    {
        if (isSuspectsPulledOver)
        {
            Functions.ForceEndCurrentPullover();
        }
        var pursuitLHandle = Functions.CreatePursuit();

        Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

        for (var i = suspectList.Length - 1; i >= 0; i--)
        {
            GameFiber.Yield();
            if (suspectList[i].IsAvailable())
            {
                Functions.AddPedToPursuit(pursuitLHandle, suspectList[i]);
                RandomizePursuitAttributes(suspectList[i]);
            }
        }
        return pursuitLHandle;
    }
    
    internal static void RandomizePursuitAttributes(Ped suspect)
    {
        try
        {
            static float GenerateRandomFloat() => (float)Math.Round((float)(Rndm.NextDouble() * (2.0 - 0.1) + 0.1), 1);
                
            PedPursuitAttributes attributes = Functions.GetPedPursuitAttributes(suspect);
                
            attributes.MinDrivingSpeed = MphToMps(Rndm.Next(35, 60));
            attributes.MaxDrivingSpeed = MphToMps(Rndm.Next(61, 201));

            attributes.HandlingAbility = GenerateRandomFloat();
            attributes.HandlingAbilityTurns = GenerateRandomFloat();

            attributes.BurstTireSurrenderMult = 2f;
            attributes.SurrenderChanceTireBurst = Rndm.Next(1, 31);
            attributes.SurrenderChanceTireBurstAndCrashed = Rndm.Next(1, 41);

            attributes.SurrenderChanceCarBadlyDamaged = Rndm.Next(1, 101);

            attributes.SurrenderChancePitted = Rndm.Next(1, 81);
            attributes.SurrenderChancePittedAndCrashed = Rndm.Next(1, 51);
            attributes.SurrenderChancePittedAndSlowedDown = Rndm.Next(1, 11);

            attributes.AverageBurstTireSurrenderTime = Rndm.Next(700, 2000);
            attributes.AverageSurrenderTime = Rndm.Next(1000, 3000);
                
            attributes.AverageFightTime = Rndm.Next(400, 2000);
                
            Normal($"MaxDrivingSpeed: {attributes.MaxDrivingSpeed}");
            Normal($"MinDrivingSpeed: {attributes.MinDrivingSpeed}");
                
            Normal($"HandlingAbility: {attributes.HandlingAbility}");
            Normal($"HandlingAbilityTurns: {attributes.HandlingAbilityTurns}");
                
            Normal($"BurstTireSurrenderMult: {attributes.BurstTireSurrenderMult}");
            Normal($"SurrenderChanceTireBurst: {attributes.SurrenderChanceTireBurst}");
            Normal($"SurrenderChanceTireBurstAndCrashed: {attributes.SurrenderChanceTireBurstAndCrashed}");
                
            Normal($"SurrenderChanceCarBadlyDamaged: {attributes.SurrenderChanceCarBadlyDamaged}");
                
            Normal($"SurrenderChancePitted: {attributes.SurrenderChancePitted}");
            Normal($"SurrenderChancePittedAndCrashed: {attributes.SurrenderChancePittedAndCrashed}");
            Normal($"SurrenderChancePittedAndSlowedDown: {attributes.SurrenderChancePittedAndSlowedDown}");
                
            Normal($"AverageBurstTireSurrenderTime: {attributes.AverageBurstTireSurrenderTime}");
            Normal($"AverageSurrenderTime: {attributes.AverageSurrenderTime}");
                
            Normal($"AverageFightTime: {attributes.AverageFightTime}");
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, nameof(RandomizePursuitAttributes));
        }
    }
}