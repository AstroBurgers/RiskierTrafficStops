namespace RiskierTrafficStops.Engine.Helpers.Extensions;

public static class EnumExtensions
{
    // Thanks again, Khori
    public static T PickRandom<T>(this IEnumerable<T> source) => source.Any() ? source.PickRandom(1).Single() : default;

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count) => source.Shuffle().Take(count);

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.OrderBy(_ => Guid.NewGuid());
}