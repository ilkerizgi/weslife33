#region

using System;
using AramBuddy.AutoShop.Sequences;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;

#endregion

namespace AramBuddy.AutoShop
{
    /// <summary>
    ///     The class where events are invoked, defined and tested
    /// </summary>
    internal static class Events
    {
        /// <summary>
        ///     A handler for the OnBuildReset event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnBuildResetHandler(EventArgs args);

        /// <summary>
        ///     A handler for the OnBuyAllow event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnBuyAllowHandler(EventArgs args);

        /// <summary>
        ///     A handler for the OnPlayerDeathHandler event
        /// </summary>
        /// <param name="sender">The Obj_AI_Base that caused this event to happen</param>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnPlayerDeathHandler(Obj_AI_Base sender, EventArgs args);

        /// <summary>
        ///     A static instance of the Events class, where the events are invoked
        /// </summary>
        static Events()
        {
            // Invoke the OnPlayerDeath event

            #region OnPlayerDeath

            try
            {
                // Every time the player plays an animation
                Obj_AI_Base.OnPlayAnimation += delegate(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
                    {
                        // If the player fired the event and the animation's name to lower is the death animation's name
                        if (sender.IsMe && args.Animation.ToLower().Contains("death"))
                        {
                            // The player has died; Invoke the OnPlayerDeath event
                            OnPlayerDeath?.Invoke(sender, EventArgs.Empty);
                        }
                    };
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop event OnPlayerDeath:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }

            #endregion

            // Invoke the OnBuyAllow event

            #region OnBuyAllow

            try
            {
                // When the game loads the first time, invoke the event
                Game.OnLoad += delegate { OnBuyAllow(EventArgs.Empty); };

                // When the player dies, invoke the event
                //OnPlayerDeath += delegate { OnBuyAllow(EventArgs.Empty); };

                // When the player losses the disable shopping buff
                Obj_AI_Base.OnBuffLose += delegate(Obj_AI_Base sender, Obj_AI_BaseBuffLoseEventArgs args)
                    {
                        if (sender.IsMe && args.Buff.DisplayName.Equals("aramshopdisableplayer", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Buy.CanShop = true;
                            OnBuyAllow(EventArgs.Empty);
                        }
                    };
                Obj_AI_Base.OnBuffGain += delegate(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
                    {
                        if (sender.IsMe && args.Buff.DisplayName.Equals("aramshopdisableplayer", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Buy.CanShop = false;
                        }
                    };
            }
            catch (NullReferenceException ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop event OnBuyAllow.", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop events initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }

            #endregion

            // Invoke the OnBuildReset event

            #region OnBuildReset

            try
            {
                // Every time the user inputs something into the chat
                Chat.OnInput += delegate(ChatInputEventArgs args)
                    {
                        // Proceed if the user's input in lower, sides trimmed of whitespace is "/buildmanager reset"
                        if (args.Input.ToLower().Trim() == "/buildmanager reset")
                        {
                            // Invoke the OnBuildReset event
                            OnBuildReset(EventArgs.Empty);

                            // Don't process the event, so the user doesn't get that annoying command not recognized message
                            args.Process = false;
                        }
                    };
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop event OnBuildReset:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }

            #endregion

            // Notify the user that events are functioning correctly
            Logger.Send("Events have been succesfully set up!");
        }

        /// <summary>
        ///     Fires when buying items is allowed
        /// </summary>
        public static event OnBuyAllowHandler OnBuyAllow;

        /// <summary>
        ///     Fires when a build reset is forced
        /// </summary>
        public static event OnBuildResetHandler OnBuildReset;

        /// <summary>
        ///     Fires when the player dies
        /// </summary>
        public static event OnPlayerDeathHandler OnPlayerDeath;
    }
}
