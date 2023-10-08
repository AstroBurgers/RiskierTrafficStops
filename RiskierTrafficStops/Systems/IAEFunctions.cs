using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.IO;

namespace RiskierTrafficStops.Systems
{
    internal class IAEFunctions
    {
        /// <summary>
        /// Checks if an entity is being used by IAE
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static bool IsPedUsedByAnyAmbientEvent(Entity entity)
        {
            try
            {
                return ImmersiveAmbientEvents.API.EventAPI.IsEntityUsedByAnyEvent(entity) || ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(entity);
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
                if (IsPedUsedByAnyAmbientEvent(ped))
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
