#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AramBuddy.AutoShop.Sequences;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;

#endregion

namespace AramBuddy.AutoShop
{
    /// <summary>
    ///     The class where AutoShop is set-up
    /// </summary>
    internal class Setup
    {
        /// <summary>
        ///     Path to the build folder, containing all the champion builds
        /// </summary>
        public static string BuildPath = Misc.AramBuddyFolder + "\\Builds";

        /// <summary>
        ///     Path to the temporary folder which contains the in-game cache
        /// </summary>
        public static readonly string TempPath = Misc.AramBuddyFolder + "\\temp";

        /// <summary>
        ///     Path to the temporary file which contains the in-game cache
        /// </summary>
        public static readonly string TempFile = TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat";

        /// <summary>
        ///     A Dictionary that contains all the builds detected
        ///     in ChampionName:BuildData format
        /// </summary>
        public static Dictionary<string, string> Builds = new Dictionary<string, string>();

        /// <summary>
        ///     The build detected for the current champion that
        ///     is being played.
        /// </summary>
        public static Build CurrentChampionBuild = new Build();

        /// <summary>
        ///     Initializes the AutoShop system
        /// </summary>
        public static void Init()
        {
            try
            {
                var service = Config.CurrentBuildService == "" ? "" : "\\" + Config.CurrentPatchUsed + "\\" + Config.CurrentBuildService;
                Buy.CanShop = !Player.Instance.Buffs.Any(b => b.DisplayName.Equals("aramshopdisableplayer", StringComparison.CurrentCultureIgnoreCase)) || Player.Instance.IsDead;
                //var useDefaultBuild = false;

                // When the game starts
                AramBuddy.Events.OnGameStart += Events_OnGameStart;

                // Check if the index file exists
                if (!File.Exists(TempFile))
                {
                    // If not, create the index file
                    Buy.CreateIndexFile();
                }

                if (!Directory.Exists(BuildPath))
                {
                    Directory.CreateDirectory(BuildPath);
                }

                // Loop through all the builds in the build path directory
                foreach (var build in Directory.GetFiles(BuildPath))
                {
                    // Get the name of the champion from the build
                    var parsed = build.Replace(".json", "").Replace(BuildPath + "\\", "");

                    // Add the build to the Builds dictionary in a ChampionName : BuildData format
                    Builds.Add(parsed, File.ReadAllText(build));
                }

                if (!Directory.Exists(BuildPath + service))
                {
                    Directory.CreateDirectory(BuildPath + service);
                }

                // Loop through all the builds in the build path directory
                foreach (var build in Directory.GetFiles(BuildPath + service))
                {
                    // Get the name of the champion from the build
                    var parsed = build.Replace(".json", "").Replace(BuildPath + service + "\\", "");

                    // Add the build to the Builds dictionary in a ChampionName : BuildData format
                    if(!Builds.ContainsKey(parsed))
                        Builds.Add(parsed, File.ReadAllText(build));
                }

                // Check if there are any builds for our champion
                if (Builds.Keys.All(b => b.CleanChampionName() != Player.Instance.CleanChampionName()))
                {
                    // If not, warn the user
                    Logger.Send("There are no builds for your champion. " + Player.Instance.ChampionName, Logger.LogLevel.Warn);
                    if(service != "")
                        Build.GetBuildFromService();
                    else
                        UseDefaultBuild();
                }

                /*if (useDefaultBuild)
                    return;*/

                // Check if the parse of the build for the champion completed successfully and output it to public
                // variable CurrentChampionBuild
                if (Builds.Any(b => b.Key.CleanChampionName() == Player.Instance.CleanChampionName()) && Builds.FirstOrDefault(b => b.Key.CleanChampionName() == Player.Instance.CleanChampionName()).Value.TryParseData(out CurrentChampionBuild))
                {
                    // If the parse is successful, notify the user that the initialization process is finished
                    Logger.Send(Config.CurrentPatchUsed + " " + Player.Instance.ChampionName + " Build Loaded !");
                    Logger.Send("AutoShop has been fully and succesfully initialized!");
                    
                    // and set up event listeners
                    SetUpEventListeners();
                    Buy.BuyNextItem(CurrentChampionBuild);
                }
            }
            catch (Exception ex)
            {
                // An exception occured somewhere else. Notify the user of the error, and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop:", ex, Logger.LogLevel.Error);

                // Warn the user about the exception
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }

        public static void UseDefaultBuild()
        {
            // and Use Default build
            if (Builds.Keys.Any(b => b.Equals(Build.BuildName())))
            {
                DefaultBuild();
                Logger.Send("Using default build path!", Logger.LogLevel.Warn);
            }
            else
            {
                // Creates Default Build for the AutoShop
                Logger.Send("Creating default build path!", Logger.LogLevel.Warn);
                Build.CreateDefualtBuild();
            }
        }

        /// <summary>
        ///     Method that sets up event listeners
        /// </summary>
        /// 
        public static void DefaultBuild()
        {
            try
            {
                // Use Default build
                if (Builds.Keys.Any(b => b.Equals(Build.BuildName())) && Builds.FirstOrDefault(b => b.Key.Equals(Build.BuildName())).Value.TryParseData(out CurrentChampionBuild))
                {
                    Logger.Send(Build.BuildName() + " build Loaded!");

                    // and set up event listeners
                    SetUpEventListeners();
                    if (Player.Instance.IsInShopRange())
                    {
                        Buy.BuyNextItem(CurrentChampionBuild);
                    }
                }
                else
                {
                    // An error occured during parsing. Catch the error and print it in the console
                    Logger.Send("The selected AutoShop JSON could not be parsed.", Logger.LogLevel.Error);

                    Logger.Send("No build is currently used!", Logger.LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                // An exception occured somewhere else. Notify the user of the error, and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop:", ex, Logger.LogLevel.Error);

                // Warn the user about the exception
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }

        public static void CustomBuildService()
        {
            try
            {
                if (Builds.Keys.Any(b => b.Equals(Player.Instance.CleanChampionName()))
                && Builds.FirstOrDefault(b => b.Key.Equals(Player.Instance.CleanChampionName())).Value.TryParseData(out CurrentChampionBuild))
                {
                    Logger.Send(Player.Instance.CleanChampionName() + " build Loaded!");

                    // and set up event listeners
                    SetUpEventListeners();
                    if (Player.Instance.IsInShopRange())
                    {
                        Buy.BuyNextItem(CurrentChampionBuild);
                    }
                }
                else
                {
                    // An error occured during parsing. Catch the error and print it in the console
                    Logger.Send("The selected AutoShop JSON could not be parsed.", Logger.LogLevel.Error);
                    Logger.Send("Using Default Build!", Logger.LogLevel.Warn);
                    UseDefaultBuild();

                    //Logger.Send("No build is currently used!", Logger.LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                // An exception occured somewhere else. Notify the user of the error, and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop:", ex, Logger.LogLevel.Error);

                // Warn the user about the exception
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }

        /// <summary>
        ///     Method that sets up event listeners
        /// </summary>
        private static void SetUpEventListeners()
        {
            // When we can buy items
            Events.OnBuyAllow += Events_OnBuyAllow;

            // When the user forced a build reset
            Events.OnBuildReset += Events_OnBuildReset;

            // When the game ends
            AramBuddy.Events.OnGameEnd += Events_OnGameEnd;
        }

        /// <summary>
        ///     Fired when the game starts
        /// </summary>
        /// <param name="args">Arguments providing with information about the GameOnLoad</param>
        private static void Events_OnGameStart(EventArgs args)
        {
            // Delete the index file if it exists
            if (File.Exists(TempFile))
            {
                File.Delete(TempFile);
            }
        }

        /// <summary>
        ///     Fired when the game ends
        /// </summary>
        /// <param name="args">Arguments providing with information about the GameEnd</param>
        private static void Events_OnGameEnd(bool args)
        {
            // Delete the index file if it exists
            if (File.Exists(TempFile))
            {
                File.Delete(TempFile);
            }

            Buy.CanShop = false;
        }

        /// <summary>
        ///     Fired when a build reset is forced
        /// </summary>
        /// <param name="args">Arguments of the event</param>
        private static void Events_OnBuildReset(EventArgs args)
        {
            // Notify the user that the build has been reset
            Logger.Send("Build has been reset!", Logger.LogLevel.Event);

            // Reset the build index, restarting the build process from the start
            Buy.ResetIndex();
        }

        private static float LastBuyAllow;

        /// <summary>
        ///     Fired when buying is allowed
        /// </summary>
        /// <param name="args">Arguments of the event</param>
        private static void Events_OnBuyAllow(EventArgs args)
        {
            if (Core.GameTickCount - LastBuyAllow > 500)
            {
                var deathtime = Player.Instance.DeathTimer() * 1000;
                // To Prevent Instantly buying item when event is fired
                var rnd = (float)(new Random().Next(Math.Max(500, (int)(deathtime * 0.05f)) / 3, Math.Max(1000, (int)(deathtime * 0.1f))) + Game.Ping);

                // Notify the user that we are going to try to buy items now
                Logger.Send("Can buy items: " + (rnd / 1000).ToString("F1") + " Second/s Delay", Logger.LogLevel.Event);

                // Attempt to buy as many consecutive items on the build as we can
                Core.DelayAction(() => Buy.BuyNextItem(CurrentChampionBuild), (int)rnd);
                LastBuyAllow = Core.GameTickCount;
            }
        }
    }
}
