using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using static AramBuddy.Plugins.Activator.Items.Database;

namespace AramBuddy.Plugins.Activator.Cleanse
{
    internal class Qss
    {
        private static readonly List<Item> SelfQss = new List<Item> { Quicksilver_Sash, Mercurial_Scimitar };

        private static readonly List<Item> AllyQss = new List<Item> { Mikaels };

        private static readonly List<BuffType> BuffsToQss = new List<BuffType>
        {
            BuffType.Blind, BuffType.Charm, BuffType.Fear, BuffType.Flee, BuffType.Knockback, BuffType.Knockup, BuffType.NearSight,
            BuffType.Poison, BuffType.Polymorph, BuffType.Sleep, BuffType.Slow, BuffType.Snare, BuffType.Silence, BuffType.Stun,
            BuffType.Suppression, BuffType.Taunt
        };

        private static Menu Clean;

        internal static void Init()
        {
            try
            {
                Clean = Load.MenuIni.AddSubMenu("Cleanse");

                Clean.AddGroupLabel("Cleanse Settings");
                Clean.CreateCheckBox("enable", "Enable Cleanse");
                foreach (var item in SelfQss)
                {
                    Clean.CreateCheckBox(item.ItemInfo.Name, "Use " + item.ItemInfo.Name);
                }
                foreach (var item in AllyQss)
                {
                    Clean.CreateCheckBox(item.ItemInfo.Name, "Use " + item.ItemInfo.Name);
                }

                Clean.AddGroupLabel("Allies Settings");
                foreach (var ally in EntityManager.Heroes.Allies)
                {
                    Clean.CreateCheckBox(ally.Name(), "Use For " + ally.Name());
                    Clean.CreateSlider(ally.Name() + "hp", "Use For " + ally.Name() + " Under {0}% HP", TargetSelector.GetPriority(ally) * 10);
                }

                Clean.AddGroupLabel("Buffs to Qss");
                foreach (var buff in BuffsToQss)
                {
                    Clean.CreateCheckBox(buff.ToString(), "Qss " + buff);
                }

                Clean.AddGroupLabel("Humanizer Settings");
                var min = Clean.CreateSlider("QssMin", "Qss Min Delay {0}", 100, 0, 400);
                var max = Clean.CreateSlider("QssMax", "Qss Max Delay {0}", 500, 0, 1500);
                min.OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                {
                    if (args.NewValue >= max.CurrentValue)
                    {
                        max.MinValue = args.NewValue + 100;
                    }
                };
                max.OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                {
                    if (args.NewValue >= min.MaxValue)
                    {
                        min.MaxValue = args.NewValue - 100;
                    }
                };

                Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
            }
            catch (Exception ex)
            {
                Logger.Send("Error At AramBuddy.Plugins.Activator.Cleanse.Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            var caster = sender as AIHeroClient;
            if (!Clean.CheckBoxValue("enable") || caster == null || !BuffsToQss.Contains(args.Buff.Type))
                return;

            if (!caster.IsAlly || caster.Distance(Player.Instance) > 1000 || !Clean.CheckBoxValue(args.Buff.Type.ToString())
                || !Clean.CheckBoxValue(caster.Name()) || caster.HealthPercent > Clean.SliderValue(caster.Name() + "hp"))
                return;

            var delay = new Random().Next(Clean.SliderValue("QssMin"), Clean.SliderValue("QssMax"));
            if (caster.IsMe)
            {
                foreach (var item in SelfQss.Where(i => i.ItemReady(Clean)))
                {
                    Core.DelayAction(() => item.Cast(), delay);
                    return;
                }
                foreach (var item in AllyQss.Where(i => i.ItemReady(Clean)))
                {
                    Core.DelayAction(() => item.Cast(caster), delay);
                    return;
                }
            }
            else
                foreach (var item in AllyQss.Where(i => i.ItemReady(Clean)))
                {
                    Core.DelayAction(() => item.Cast(caster), delay);
                    return;
                }
        }
    }
}
