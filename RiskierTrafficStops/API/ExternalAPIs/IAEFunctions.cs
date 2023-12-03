using System;
using System.IO;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.Engine.InternalSystems;

namespace RiskierTrafficStops.API.ExternalAPIs
{
    internal static class IaeFunctions
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
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return false;
            }
        }

        /// <summary>
        /// Checks if an entity is being used by an IAE BOLO event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static bool IsPedUsedByBoloEvent(Entity entity)
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
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return false;
            }
        }

        internal static bool IaeEventCheck()
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
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return false;
            }
        }

        /// <summary>
        /// Handles all compatibility checks for IAE, returns true if it passes the checks
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal static bool IaeCompatibilityCheck(LHandle handle)
        {
            try
            {
                Logger.Debug("Performing IAE compatibility check...");
                var ped = Functions.GetPulloverSuspect(handle);
                if (IsPedUsedByAmbientEvent(ped))
                {
                    return IaeEventCheck();
                }
                else if (IsPedUsedByBoloEvent(ped))
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
                Logger.Debug("Immersive Ambient Events cannot be found, user might not have it installed");
                return true;
            }
        }
    }
}