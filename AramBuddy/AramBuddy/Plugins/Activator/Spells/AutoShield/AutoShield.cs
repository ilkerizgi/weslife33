using System;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Activator.Spells.AutoShield
{
    internal class AutoShield
    {
        private static Menu menu;

        private static bool ForAllies(SheildsDatabase.Shield sh)
        {
            return sh.Type.Equals(SheildsDatabase.Shield.SheildType.AllyShield) || sh.Type.Equals(SheildsDatabase.Shield.SheildType.AllySaveior) || sh.Type.Equals(SheildsDatabase.Shield.SheildType.AllyHeal);
        }

        private static bool SpellBlock(SheildsDatabase.Shield sh)
        {
            return sh.Type.Equals(SheildsDatabase.Shield.SheildType.SpellBlock) || sh.Type.Equals(SheildsDatabase.Shield.SheildType.Wall);
        }

        private static bool Self(SheildsDatabase.Shield sh)
        {
            return sh.Type.Equals(SheildsDatabase.Shield.SheildType.Self) || sh.Type.Equals(SheildsDatabase.Shield.SheildType.CastOnEnemy);
        }

        private static bool Saveior(SheildsDatabase.Shield sh)
        {
            return sh.Type.Equals(SheildsDatabase.Shield.SheildType.SelfSaveior) || sh.Type.Equals(SheildsDatabase.Shield.SheildType.AllySaveior);
        }

        public static void Init()
        {
            try
            {
                if (SheildsDatabase.Shields.All(s => s.Hero != Player.Instance.Hero))
                    return;
                menu = Load.MenuIni.AddSubMenu("AutoShield " + Player.Instance.Hero);
                menu.AddGroupLabel("Spells To Use");
                foreach (var shield in SheildsDatabase.Shields.Where(s => s.Hero.Equals(Player.Instance.Hero)))
                {
                    menu.CreateCheckBox("use" + shield.Hero + shield.Spell.Slot, "Use " + shield.Hero + " " + shield.Spell.Slot);
                }

                menu.AddSeparator(0);
                menu.AddGroupLabel("General Settings");
                menu.CreateSlider(Player.Instance.ChampionName + "hp", "Stop using under {0}% HP");
                menu.CreateSlider(Player.Instance.ChampionName + "mp", "Stop using under {0}% MP", 50);
                menu.AddSeparator(5);

                foreach (var shield in SheildsDatabase.Shields.Where(s => s.Hero.Equals(Player.Instance.Hero)))
                {
                    if (ForAllies(shield))
                    {
                        menu.AddGroupLabel("Allies To Use " + shield.Spell.Slot);
                        foreach (var ally in EntityManager.Heroes.Allies)
                        {
                            menu.CreateCheckBox(ally.Name() + shield.Spell.Slot, "Use " + shield.Spell.Slot + " for " + ally.Name());
                            menu.CreateSlider(ally.Name() + shield.Spell.Slot + "hp", "Use " + shield.Spell.Slot + " for " + ally.Name() + " On {0}% HP", TargetSelector.GetPriority(ally) * 15);
                            menu.AddSeparator(0);
                        }
                    }
                }
                Game.OnTick += Game_OnTick;
                Events.OnIncomingDamage += OnInComingDamage_OnIncomingDamage;
            }
            catch (Exception ex)
            {
                Logger.Send("Error At Brain.Activator.Spells.Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void OnInComingDamage_OnIncomingDamage(Events.InComingDamageEventArgs args)
        {
            if (menu.SliderValue(Player.Instance.ChampionName + "hp") >= Player.Instance.PredictHealthPercent() || (!Player.Instance.IsNoManaHero() && menu.SliderValue(Player.Instance.ChampionName + "mp") >= Player.Instance.ManaPercent))
                return;

            foreach (
                var shield in
                    SheildsDatabase.Shields.Where(
                        s =>
                        s.Hero.Equals(Player.Instance.Hero) && menu.CheckBoxValue("use" + s.Hero.ToString() + s.Spell.Slot) && s.Spell.IsReady() && (args.Target.IsKillable(s.Spell.Range) || args.Target.IsMe))
                )
            {
                if (ForAllies(shield))
                {
                    if (menu.CheckBoxValue(args.Target.Name() + shield.Spell.Slot))
                    {
                        if (Saveior(shield))
                        {
                            if (args.InComingDamage >= args.Target.PredictHealth())
                            {
                                //Logger.Send(shield.Type + " " + shield.Hero + shield.Spell.Slot + " CastedFor: [" + args.Target.ChampionName + "]");
                                shield.On(args.Target);
                            }
                            return;
                        }
                        if (menu.SliderValue(args.Target.Name() + shield.Spell.Slot + "hp") >= args.Target.PredictHealthPercent())
                        {
                            shield.On(args.Target);
                            //Logger.Send(shield.Type + " [" + shield.Hero + shield.Spell.Slot + "] CastedFor: [" + args.Target.ChampionName + "]");
                            return;
                        }
                    }
                }
                else
                {
                    if (args.Target.IsMe)
                    {
                        if (Saveior(shield) && args.InComingDamage >= args.Target.PredictHealth())
                        {
                            //Logger.Send(shield.Type + " [" + shield.Hero + shield.Spell.Slot + "] CastedFor: [" + args.Target.ChampionName + "]");
                            shield.On(args.Target);
                            return;
                        }
                        if (SpellBlock(shield))
                        {
                            var yasuo = (Player.Instance.Hero == Champion.Yasuo && Collision.NewSpells.Any(s => s.spell.Collisions.Contains(Database.SkillShotSpells.Collision.YasuoWall) && Player.Instance.IsInDanger(s)))
                                || Player.Instance.Hero != Champion.Yasuo && args.DamageType == Events.InComingDamageEventArgs.Type.SkillShot;
                            if (yasuo || args.DamageType == Events.InComingDamageEventArgs.Type.TargetedSpell)
                            {
                                //Logger.Send(shield.Type + " [" + shield.Hero + shield.Spell.Slot + "] CastedFor: [" + args.Target.ChampionName + "]");
                                shield.On(args.Sender);
                                return;
                            }
                        }
                        if (Self(shield))
                        {
                            //Logger.Send(shield.Type + " [" + shield.Hero + shield.Spell.Slot + "] CastedFor: [" + args.Target.ChampionName + "]");
                            shield.On(args.Sender);
                            return;
                        }
                    }
                }
                //Logger.Send("Sender [" + args.Sender.BaseSkinName + "] [IncDmg: " + (int)args.InComingDamage + "] DamageType[" + args.DamageType + "]");
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (menu.SliderValue(Player.Instance.ChampionName + "hp") >= Player.Instance.PredictHealthPercent() || menu.SliderValue(Player.Instance.ChampionName + "mp") >= Player.Instance.ManaPercent)
                return;

            foreach (
                var shield in
                    SheildsDatabase.Shields.Where(
                        s =>
                        s.Hero.Equals(Player.Instance.Hero) && menu.CheckBoxValue("use" + s.Hero + s.Spell.Slot) && !Saveior(s) && !SpellBlock(s) && ForAllies(s)
                        && !s.Type.Equals(SheildsDatabase.Shield.SheildType.AllyShield) && s.Spell.IsReady()))
            {
                foreach (var ally in EntityManager.Heroes.Allies.Where(a => a.IsKillable(shield.Spell.Range)))
                {
                    if (menu.CheckBoxValue(ally.Name() + shield.Spell.Slot) && menu.SliderValue(ally.Name() + shield.Spell.Slot + "hp") >= ally.PredictHealthPercent())
                    {
                        //Logger.Send(shield.Type + " [" + shield.Hero + shield.Spell.Slot + "] CastedFor: [" + ally.ChampionName + "] - Health: [" + (int)ally.PredictHealthPercent() + "]");
                        shield.On(ally);
                    }
                }
            }
        }
    }
}
