using RiskierTrafficStops.Engine.InternalSystems;

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
                Logger.Normal("OnRTSOutcomeStarted Invoked");
                break;
            case RTSEventType.End:
                OnRTSOutcomeEnded?.Invoke();
                Logger.Normal("OnRTSOutcomeEnded Invoked");
                break;
        }
    }
}