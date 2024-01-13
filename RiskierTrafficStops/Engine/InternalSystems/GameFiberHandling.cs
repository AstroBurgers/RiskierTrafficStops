namespace RiskierTrafficStops.Engine.InternalSystems;

// Thanks Khori
public static class GameFiberHandling
{
    internal static readonly List<GameFiber> OutcomeGameFibers = new();

    internal static void CleanupFibers()
    {
        GameFiber.StartNew(() =>
        {
            Debug("Cleaning up running GameFibers...");
            OutcomeGameFibers.RemoveAll(fiber =>
            {
                if (fiber.IsAlive)
                {
                    fiber.Abort();
                    return true;
                }
                return false;
            });
        });
    }
}