using System.Security.Cryptography;

namespace RiskierTrafficStops.Engine.Helpers;

internal static class MathHelper
{
    internal static readonly Random Rndm = new(DateTime.Now.Millisecond);
    private static readonly RNGCryptoServiceProvider ImprovedRandom = new();
    
    internal static bool CheckZDistance(float z1, float z2, float range)
    {
        var difference = Math.Abs(z1 - z2);
        return difference <= range;
    }
    
    internal static Vector3 GetRearOffset(Vehicle vehicle, float offset)
    {
        var backwardDirection = vehicle.RearPosition - vehicle.FrontPosition;
        backwardDirection.Normalize();
        return (backwardDirection * offset) + vehicle.RearPosition;
    }

    /// <summary>
    /// Returns the nearest vehicle to a position
    /// </summary>

    internal static Vehicle GetNearestVehicle(Vector3 position, float maxDistance = 40f)
    {
        var vehicles = MainPlayer.GetNearbyVehicles(16).ToList();
        if (vehicles.Count < 1)
            throw new ArgumentOutOfRangeException();
        var vehicle = vehicles.OrderBy(vehicles1 => vehicles1.DistanceTo(position)).ToList().First();
        
        return vehicle;
    }
    
    /// <summary>
    /// Converts MPH to meters per second which is what all tasks use, returns meters per second
    /// </summary>
    internal static float MphToMps(float speed) => Rage.MathHelper.ConvertMilesPerHourToMetersPerSecond(speed);

    internal static long GenerateChance()
    {
        var randomBytes = new byte[8]; // Using 8 bytes for more randomization ig
        ImprovedRandom.GetBytes(randomBytes);
 
        var randomNumber = BitConverter.ToInt64(randomBytes, 0) & 0x7FFFFFFFFFFFFFFF; // Convert to positive integer

        var convertedChance = randomNumber % 100;
        
        return convertedChance;
    }
}