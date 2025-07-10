using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using RiskierTrafficStops.Engine.Data;
using RiskierTrafficStops.Mod;

namespace RiskierTrafficStops.API;

internal enum RTSEventType
{
    Start,
    End
}

public static class APIs
{
    /// <summary>
    /// Disables RTS Outcomes for the current/next pullover
    /// Notes:
    ///     Reset after every pullover
    /// </summary>
    public static bool DisableRTSForCurrentStop { get; set; }

    /// <summary>
    /// Stops RTS from interfering with the supplied Suspects
    /// Also removes invalid peds from the list at the same time
    /// Notes:
    ///     The list is cleared every 10 minutes
    ///     If one of the supplied peds is the driver, the outcome is ended immediately 
    /// </summary>
    /// <param name="peds">Peds to be ignored</param>
    public static void DisableRTSForPeds(params Ped[] peds)
    {
        foreach (var ped in peds.ToList().Where(ped => ped.IsAvailable()))
        {
            Outcome.PedsToIgnore.Add(ped);
        }

        foreach (var ped in Outcome.PedsToIgnore.Where(ped => !ped.IsAvailable()).ToList())
        {
            Outcome.PedsToIgnore.Remove(ped);
        }
    }

    /// <summary>
    /// Calculates the overall risk score of a suspect ped based on their profile and vehicle data.
    /// </summary>
    /// <param name="suspect">The ped suspect to evaluate risk for.</param>
    /// <remarks>Requires that the suspect is in a vehicle, as RiskProfile is also based on the Suspect's LastVehicle data</remarks>
    /// <returns>
    /// An integer representing the combined risk score.
    /// Returns 0 if the suspect does not exist or data is invalid.
    /// </returns>
    public static int GetPedRisk(Ped suspect)
    {
        if (!suspect.Exists())
            return 0;

        var pedData = suspect.GetPedData();
        var vehicleData = suspect.LastVehicle?.GetVehicleData();

        if (pedData == null || vehicleData == null)
            return 0;

        var profile = new SuspectRiskProfile();
        profile.Evaluate(pedData, vehicleData);

        return profile.ViolentScore + profile.NeutralScore + profile.SafeScore;
    }

    /// <summary>
    /// Calculates a detailed risk summary for a suspect ped, breaking down the violent, neutral,
    /// and safe risk scores separately.
    /// </summary>
    /// <param name="suspect">The ped suspect to evaluate risk for.</param>
    /// <remarks>Requires that the suspect is in a vehicle, as RiskProfile is also based on the Suspect's LastVehicle data</remarks>
    /// <returns>
    /// A <see cref="PedRiskSummary"/> struct containing individual risk scores.
    /// Returns default (all zeros) if the suspect does not exist or data is invalid.
    /// </returns>
    public static PedRiskSummary GetPedRiskSummary(Ped suspect)
    {
        if (!suspect.Exists())
            return default;

        var pedData = suspect.GetPedData();
        var vehicleData = suspect.LastVehicle?.GetVehicleData();

        if (pedData == null || vehicleData == null)
            return default;

        var profile = new SuspectRiskProfile();
        profile.Evaluate(pedData, vehicleData);

        return new PedRiskSummary(profile.ViolentScore, profile.NeutralScore, profile.SafeScore);
    }
    
    public delegate void RTSEvent();

    /// <summary>
    /// Invoked when an RTS Outcome is started
    /// </summary>
    public static event RTSEvent OnRTSOutcomeStarted;

    /// <summary>
    /// Invoked when an RTS Outcome is ended
    /// Usually not invoked when an error is thrown.
    /// </summary>
    public static event RTSEvent OnRTSOutcomeEnded;
    
    internal static void InvokeEvent(RTSEventType typeToInvoke)
    {
        switch (typeToInvoke)
        {
            case RTSEventType.Start:
                OnRTSOutcomeStarted?.Invoke();
                Normal("OnRTSOutcomeStarted Invoked");
                break;
            case RTSEventType.End:
                OnRTSOutcomeEnded?.Invoke();
                Normal("OnRTSOutcomeEnded Invoked");
                break;
            default:
                Normal("How the fuck? Not a valid event type.");
                break;
        }
    }
}