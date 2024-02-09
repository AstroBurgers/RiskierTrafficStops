namespace RiskierTrafficStops.Engine.InternalSystems;

// Thanks for the help making this better Khori
internal static class GameFiberHandling
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