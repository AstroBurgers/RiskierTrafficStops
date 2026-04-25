namespace RiskierTrafficStops.Engine.Helpers.Extensions;

internal static class EnumExtensions
{
    // Thanks again, Khori
    extension<T>(IEnumerable<T> source)
    {
        internal T PickRandom()
        {
            var list = source.ToList();
            return list.Any() ? list.PickRandom(1).Single() : default;
        }

        internal IEnumerable<T> PickRandom(int count) => source.Shuffle().Take(count);
        internal IEnumerable<T> Shuffle() => source.OrderBy(_ => Guid.NewGuid());
    }
}