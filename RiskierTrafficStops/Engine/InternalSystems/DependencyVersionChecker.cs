using System.IO;
using System.Reflection;

namespace RiskierTrafficStops.Engine.InternalSystems;

internal static class DependencyVersionChecker
{
    // Credit to Opus49 for this method
    internal static bool IsAssemblyAvailable(string assemblyName, string version)
    {
        try
        {
            var assemblyName2 =
                AssemblyName.GetAssemblyName(AppDomain.CurrentDomain.BaseDirectory + "/" + assemblyName);
            if (assemblyName2.Version >= new Version(version))
            {
                Normal($"{assemblyName} is available ({assemblyName2.Version}).");
                return true;
            }

            Normal($"{assemblyName} does not meet minimum requirements ({assemblyName2.Version} < {version}).");
            Game.DisplayNotification("3dtextures",
                "mpgroundlogo_cops",
                "Riskier Traffic Stops",
                "~b~By Astro",
                $"{assemblyName} does not meet minimum requirements ({assemblyName2.Version} < {version}), or is not installed.\n~r~Please install the latest version of {assemblyName}!~s~\n~y~Unloading RTS...~s~");
            return false;
        }
        catch (Exception ex) when (ex is FileNotFoundException or BadImageFormatException)
        {
            Normal(assemblyName + " is not available.");
            return false;
        }
    }
}