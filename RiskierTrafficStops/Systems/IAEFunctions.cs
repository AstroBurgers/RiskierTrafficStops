using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.IO;

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
                    return ImmersiveAmbientEvents.API.EventAPI.IsEntityUsedByAnyEvent(ped) && ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(ped);
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

        /// <summary>
        /// Checks if a pullover is a part of an IAE event, returns false if so
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal static bool IAECompatibilityCheck(LHandle handle)
        {
            try
            {
                Ped ped = Functions.GetPulloverSuspect(handle);
                if (IsPedUsedByAmbientEvent(ped, true))
                {
                    Logger.Debug("Pullover is a part of an IAE event, aborting RTS events...");
                    return false;
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "IAEFunctions.cs, IAECompatibilityCheck()");
                return true;
            }
        }
    }
}
