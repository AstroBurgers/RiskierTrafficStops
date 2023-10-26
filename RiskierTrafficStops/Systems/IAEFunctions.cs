using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.IO;

namespace RiskierTrafficStops.Systems
{
    internal class IAEFunctions
    {
        /// <summary>
        /// Checks if an entity is being used by a normal IAE event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static bool IsPedUsedByAmbientEvent(Entity entity)
        {
            try
            {
                return ImmersiveAmbientEvents.API.EventAPI.IsEntityUsedByAnyEvent(entity);
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
        /// Checks if an entity is being used by an IAE BOLO event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static bool IsPedUsedByBOLOEvent(Entity entity)
        {
            try
            {
                return ImmersiveAmbientEvents.API.BoloEventAPI.IsEntityUsedByAnyBOLOEvent(entity);
            }
            catch (FileNotFoundException)
            {
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "IAEFunctions.cs, IsPedUsedByBOLOEvent()");
                return false;
            }
        }

        internal static bool IAEEventCheck()
        {
            try
            {
                if (ImmersiveAmbientEvents.API.EventAPI.CheckIfEventIsRunning(ImmersiveAmbientEvents.API.EventAPI.Events.StreetRacing))
                {
                    Logger.Debug("Pullover is a part of an IAE street racing event, aborting RTS events...");
                    return false;
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "IAEFunctions.cs, IAEEventCheck()");
                return false;
            }
        }

        /// <summary>
        /// Handles all compatibility checks for IAE, returns true if it passes the checks
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal static bool IAECompatibilityCheck(LHandle handle)
        {
            try
            {
                Ped ped = Functions.GetPulloverSuspect(handle);
                if (IsPedUsedByAmbientEvent(ped))
                {
                    return IAEEventCheck();
                }
                else if (IsPedUsedByBOLOEvent(ped))
                {
                    Logger.Debug("Pullover is a part of an IAE BOLO event, aborting RTS events...");
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
