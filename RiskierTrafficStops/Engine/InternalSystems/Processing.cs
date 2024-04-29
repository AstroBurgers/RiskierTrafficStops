using static RiskierTrafficStops.Mod.Outcome;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal class Processing
{
    internal static void HandleIgnoredPedsList()
    {
        while (Main.OnDuty)
        {
            GameFiber.Yield();
            GameFiber.Wait(600000);
            if (PedsToIgnore is null) continue;
            foreach (var ped in PedsToIgnore.Where(i => !i.IsAvailable()))
            {
                PedsToIgnore.Remove(ped);
            }
        }
    }
}

internal interface IUpdateable
{
    internal void Start();

    internal void Abort();
}