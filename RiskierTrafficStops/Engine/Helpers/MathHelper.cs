using System.Security.Cryptography;

namespace RiskierTrafficStops.Engine.Helpers;

internal static class MathHelper
{
    internal static readonly Random Rndm = new(DateTime.Now.Millisecond);
    private static readonly RNGCryptoServiceProvider ImprovedRandom = new();

    /// <summary>
    /// Converts MPH to meters per second which is what all tasks use, returns meters per second
    /// </summary>
    internal static float MphToMps(float speed) => Rage.MathHelper.ConvertMilesPerHourToMetersPerSecond(speed);

    internal static long GenerateChance()
    {
        byte[] randomBytes = new byte[8]; // Using 8 bytes for more randomization ig
        ImprovedRandom.GetBytes(randomBytes);

        long randomNumber = BitConverter.ToInt64(randomBytes, 0) & 0x7FFFFFFFFFFFFFFF; // Convert to positive integer

        long convertedChance = randomNumber % 100;

        return convertedChance;
    }
}