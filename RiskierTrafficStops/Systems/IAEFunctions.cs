using System;
using System.IO;
using Rage;

namespace RiskierTrafficStops.Systems
{
    internal class IAEFunctions
    {
        /// <summary>
        /// Checks if a ped is being used by IAE, if IncludeAllEvents is false, it will only check BOLO events
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="IncludeAllEvents"></param>
        /// <returns></returns>
        internal static bool IsPedUsedByAmbientEvent(Ped ped, bool IncludeAllEvents)
        {
            try
            {
                if (IncludeAllEvents)
                {
                    return ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(ped) && ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(ped);
                }
                return ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(ped);
            }
            catch (FileNotFoundException)
            {
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "IAEFunctions.cs, IsPedUsedByAmbientEvent()");
                return false;
            }
        }
    }
}
