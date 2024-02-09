using RiskierTrafficStops.Engine.Helpers.Extensions;
using RiskierTrafficStops.Mod;

namespace RiskierTrafficStops.API;

internal enum RTSEventType
{
    Start,
    End,
}

public class APIs
{
    /// <summary>
    /// Disables RTS Outcomes for the current/next pullover
    /// </summary>
    public static bool DisableRTSForCurrentStop { get; set; }

    /// <summary>
    /// Disables RTS interaction for the input peds
    /// </summary>
    /// <param name="peds">Peds to have RTS ignore</param>
    public static void DisableRTSForPeds(params Ped[] peds)
    {
        foreach (var ped in peds)
        {
            if (ped.IsAvailable())
            {
                Outcome.PedsToIgnore.Add(ped);
            }
        }

        foreach (var ped in Outcome.PedsToIgnore)
        {
            if (!ped.IsAvailable())
            {
                Outcome.PedsToIgnore.Remove(ped);
            }
        }
    }
    
    public delegate void RTSEvent();

    /// <summary>
    /// Invoked when a RTS Outcome is started
    /// </summary>
    public static event RTSEvent OnRTSOutcomeStarted;

    /// <summary>
    /// Invoked when a RTS Outcome is ended
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
        }
    }
}