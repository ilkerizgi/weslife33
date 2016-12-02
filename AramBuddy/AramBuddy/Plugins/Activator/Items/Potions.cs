using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using static AramBuddy.Plugins.Activator.Items.Database;

namespace AramBuddy.Plugins.Activator.Items
{
    internal class Potions
    {
        private static readonly List<Item> Pots = new List<Item> { HealthPotion, Biscuit, CorruptingPotion, RefillablePotion };

        private static readonly List<string> PotBuffs = new List<string> { "Health Potion", "ItemCrystalFlask", "ItemDarkCrystalFlask", "ItemMiniRegenPotion" };

        private static Menu PotionsMenu;

        public static void Init()
        {
            try
            {
                PotionsMenu = Load.MenuIni.AddSubMenu("Potions");
                Pots.ForEach(
                    p =>
                        {
                            PotionsMenu.CreateCheckBox(p.Id.ToString(), "Use " + p.ItemInfo.Name);
                            PotionsMenu.CreateSlider(p.Id + "hp", p.ItemInfo.Name + " Health% {0}", 60);
                        });
                Game.OnTick += Game_OnTick;
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Potions Error While Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            try
            {
                if (Player.Instance.IsDead) return;
                foreach (var pot in Pots.Where(p => p.ItemReady(PotionsMenu) && PotionsMenu.SliderValue(p.Id + "hp") >= Player.Instance.PredictHealthPercent()))
                {
                    if (!Player.Instance.Buffs.Any(a => PotBuffs.Any(b => a.DisplayName.Equals(b))))
                    {
                        pot.Cast();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Potions Error At Game_OnTick", ex, Logger.LogLevel.Error);
            }
        }
    }
}
