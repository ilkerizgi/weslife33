#region

using System;
using System.IO;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;

#endregion

namespace AramBuddy.AutoShop.Sequences
{
    internal class Buy
    {

        /// <summary>
        ///     Returns The Current Item Index
        /// </summary>
        public static int CurrentItemIndex;

        /// <summary>
        ///     Returns True if you can buy items from the shop
        /// </summary>
        public static bool CanShop;

        /// <summary>
        ///     Returns True if we have full build
        /// </summary>
        public static bool FullBuild;

        /// <summary>
        ///     Returns Next Item Name.
        /// </summary>
        public static string NextItem;

        /// <summary>
        ///     Returns Next Item Price.
        /// </summary>
        public static int NextItemValue;

        /// <summary>
        ///     Attempts to buy the next item, and continues to buy next items until
        ///     it is no longer allowed to do so.
        /// </summary>
        /// <param name="build">The parsed build</param>
        /// <returns></returns>
        public static bool BuyNextItem(Build build)
        {
            try
            {
                if (!Config.EnableAutoShop)
                {
                    Logger.Send("AutoShop Is Disabled from the menu !", Logger.LogLevel.Warn);
                    return false;
                }

                if (Player.Instance.IsZombie)
                {
                    var rndm = (float)new Random().Next(500, 3000);
                    Logger.Send("Cant Buy Items! - Case: Zombie, Trying Again After " + (rndm / 1000).ToString("F1") + " Second/s", Logger.LogLevel.Warn);
                    Core.DelayAction(() => BuyNextItem(build), (int)rndm);
                    return false;
                }

                // Check if we've reached the end of the build
                if (build.BuildData.Length < GetIndex() + 1)
                {
                    const ItemId Elixir_of_Wrath = ItemId.Elixir_of_Wrath;
                    const ItemId Elixir_of_Iron = ItemId.Elixir_of_Iron;
                    const ItemId Elixir_of_Sorcery = ItemId.Elixir_of_Sorcery;

                    // Buy Elixir
                    if (Player.Instance.Gold >= 500 && !(Player.HasBuff("ElixirOfIron") || Player.HasBuff("ElixirOfSorcery") || Player.HasBuff("ElixirOfWrath")))
                    {
                        var itembuy = Elixir_of_Iron;
                        if (Build.BuildName().Contains("AD"))
                        {
                            itembuy = Elixir_of_Wrath;
                        }

                        if (Build.BuildName().Contains("AP"))
                        {
                            itembuy = Elixir_of_Sorcery;
                        }

                        // Buy the actual item from the shop
                        Shop.BuyItem(itembuy);

                        // Notify the user that the item has been bought and of the value of the item
                        Logger.Send("Item bought: " + itembuy + " - Item Value: " + new Item(itembuy).ItemInfo.Gold.Total);

                        NextItem = new Item(itembuy).ItemInfo.Name;
                        NextItemValue = new Item(itembuy).ItemInfo.Gold.Total;
                    }

                    // Notify the user that the build is finished
                    Logger.Send("Build is finished - Cannot buy any more items!");

                    // Set Build to true
                    FullBuild = true;

                    // Return false because we could not buy items
                    return false;
                }

                // Get the item
                var itemname = build.BuildData.ElementAt(GetIndex());
                var item = Item.ItemData.FirstOrDefault(i => i.Value.Name == itemname);
                var theitem = new Item(item.Key);
                NextItem = theitem.ItemInfo.Name;
                NextItemValue = theitem.GoldRequired();
                CurrentItemIndex = GetIndex() + 1;

                var deathtime = Player.Instance.DeathTimer() * 1000;
                var rnd = (float)(new Random().Next(Math.Max(500, (int)(deathtime * 0.05f)), Math.Max(1000, (int)(deathtime * 0.1f))) + Game.Ping);

                if (!item.Value.AvailableForMap || !item.Value.InStore)
                {
                    // Increment the static item index
                    IncrementIndex();

                    // Notify the user that we skipped the item
                    Logger.Send("Item Skipped: " + item.Value.Name + " - Case: Not Available in Aram.", Logger.LogLevel.Warn);

                    // Try to buy more than one item if we can afford it
                    Core.DelayAction(() => BuyNextItem(build), (int)rnd);

                    // Success
                    return true;
                }

                // Check if we can buy the item
                if ((item.Value != null) && CanShop && (item.Key != ItemId.Unknown) && item.Value.ValidForPlayer && item.Value.Gold.Purchasable
                    && (Player.Instance.Gold >= NextItemValue))
                {
                    // Buy the actual item from the shop
                    Shop.BuyItem(item.Key);

                    // Increment the static item index
                    IncrementIndex();

                    // Notify the user that the item has been bought and of the value of the item
                    Logger.Send("Item bought: " + item.Value.Name + " - Item Value: " + NextItemValue);

                    // Try to buy more than one item if we can afford it
                    Core.DelayAction(() => BuyNextItem(build), (int)rnd);

                    // Success
                    return true;
                }

                // Fail
                return false;
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred in AutoShop on buying the next item: ", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
                return false;
            }
        }

        /// <summary>
        ///     Creates an index file in AppData location for storing
        ///     the index of the current item being bought.
        /// </summary>
        public static void CreateIndexFile()
        {
            try
            {
                // Create the temporary files path directory
                Directory.CreateDirectory(Setup.TempPath);

                // If the index file already exists, stop running the method as the user probably wants to continue
                // using their build sequence on its current index
                if (File.Exists(Setup.TempFile))
                {
                    return;
                }

                // Create the index file
                using (var sw = File.AppendText(Setup.TempFile))
                {
                    // Write the default value (0) to the index file
                    sw.Write(0);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred in AutoShop on creating build index file:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }

        /// <summary>
        ///     Increases the index of the index file by one.
        /// </summary>
        private static void IncrementIndex()
        {
            try
            {
                // Create the temporary files path directory
                Directory.CreateDirectory(Setup.TempPath);

                // The contents of the index file
                var data = File.ReadAllText(Setup.TempFile);

                // The incremented index of the index file
                var index = int.Parse(data) + 1;

                // Delete the index file
                File.Delete(Setup.TempFile);

                // Re-write the index file
                using (var sw = File.AppendText(Setup.TempFile))
                {
                    // Write the new, incremented index on the index file
                    sw.Write(index);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred in AutoShop on increment build index:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!", ex, Logger.LogLevel.Warn);
            }
        }

        /// <summary>
        ///     Retreives the index from the index file as an integer
        /// </summary>
        /// <returns>The integer stored in the index file.</returns>
        internal static int GetIndex()
        {
            try
            {
                // Get the data from the index file
                var data = File.ReadAllText(Setup.TempFile);

                // return the parsed data to an integer
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred in AutoShop on get build index:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
                return 0;
            }
        }

        /// <summary>
        ///     Resets the index file back to its default value.
        /// </summary>
        public static void ResetIndex()
        {
            try
            {
                // Create the temporary files path directory
                Directory.CreateDirectory(Setup.TempPath);

                // Return if the index file does not exist
                if (!File.Exists(Setup.TempFile))
                {
                    return;
                }

                // Delete the index file
                File.Delete(Setup.TempFile);

                // Rewrite to the index file
                using (var sw = File.AppendText(Setup.TempFile))
                {
                    // Write the default index file value (0) to the index file
                    sw.Write(0);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred in AutoShop reset build index:", ex, Logger.LogLevel.Error);

                // Warn the user that AutoShop may not be functioning correctly
                Logger.Send("Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }
    }
}
