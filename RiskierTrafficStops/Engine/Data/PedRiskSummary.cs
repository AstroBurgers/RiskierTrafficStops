namespace RiskierTrafficStops.Engine.Data;

/// <summary>
/// Represents a detailed breakdown of risk scores for a suspect ped,
/// including violent, neutral, and safe risk components.
/// </summary>
public readonly struct PedRiskSummary(int violentScore, int neutralScore, int safeScore)
{
    public int ViolentScore { get; } = violentScore;
    public int NeutralScore { get; } = neutralScore;
    public int SafeScore { get; } = safeScore;

    /// <summary>
    /// Gets the total combined risk score.
    /// <remarks>The total calculated score is used internally to select the suspect’s outcome.</remarks>
    /// </summary>
    public int TotalScore => ViolentScore + NeutralScore + SafeScore;
}