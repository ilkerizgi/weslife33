using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Kalista
{
    internal class Kalista : Base
    {
        private static readonly string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\temp\\";
        private static string filename = Game.GameId + "Kalista.dat";
        private static readonly bool Created;
        private static float LastE;
        private static AIHeroClient BoundHero;

        private static readonly List<BallistaHeros> Ballistaheros = new List<BallistaHeros>
        {
            new BallistaHeros(Champion.Blitzcrank, "rocketgrab2"),
            new BallistaHeros(Champion.TahmKench, "tahmkenchdevoured"),
            new BallistaHeros(Champion.Skarner, "SkarnerImpale")
        };

        private class BallistaHeros
        {
            public Champion Hero;
            public string BuffName;
            public BallistaHeros(Champion hero, string buffname)
            {
                this.Hero = hero;
                this.BuffName = buffname;
            }
        }

        static Kalista()
        {
            if (!Directory.Exists(appdata))
            {
                Directory.CreateDirectory(appdata);
            }

            if (!File.Exists(appdata + filename))
            {
                File.Create(appdata + filename);
                Created = true;
            }

            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            AutoMenu.CreateCheckBox("SoulBound", "R Save Soul Bound");
            AutoMenu.CreateCheckBox("AutoR", "Auto R");
            AutoMenu.CreateCheckBox("EDeath", "E Before Death");
            AutoMenu.CreateCheckBox("AutoEBig", "Auto Use E Big Minions");
            AutoMenu.CreateCheckBox("AutoEUnKillable", "Auto Use E On UnKillable Minions", false);

            var balistahero = Ballistaheros.FirstOrDefault(a => EntityManager.Heroes.Allies.Any(b => a.Hero == b.Hero));

            if (balistahero != null)
            {
                AutoMenu.CreateCheckBox(balistahero.Hero.ToString(), "Use Ballista With " + balistahero.Hero);
                AutoMenu.CreateSlider(balistahero.Hero + "dis", "Min Distance To Use Ballista", 600, 0, 1100);
            }

            ComboMenu.CreateCheckBox("Gapclose", "Auto Attack Minions To GapClose");
            ComboMenu.CreateSlider("EKillCount", "Use E To Kill {0}+ Enemies Only", 1, 1, 6);

            HarassMenu.CreateCheckBox("Emin", "E Kill Minion For Harass");
            HarassMenu.CreateSlider("Estacks", "{0} Stacks to Use E", 5, 1, 25);
            
            LaneClearMenu.CreateSlider("Qhits", "Q Hit Count {0}", 3, 1, 15);
            LaneClearMenu.CreateSlider("Ekills", "E Kill Count {0}", 2, 1, 10);

            KillStealMenu.CreateCheckBox("ETransfer", "Stacks Transfer Killsteal (Q > E)");

            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Events.OnIncomingDamage += Events_OnIncomingDamage;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Events.OnGameEnd += Events_OnGameEnd;
        }

        private static void Events_OnGameEnd(bool args)
        {
            if (File.Exists(appdata + filename))
            {
                File.Delete(appdata + filename);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && sender.IsMe && args.Target != null && args.Target.IsAlly && args.SData.Name.Equals("KalistaPSpellCast", StringComparison.CurrentCultureIgnoreCase))
            {
                File.WriteAllText(appdata + filename, args.Target.NetworkId.ToString());
            }
        }

        private static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            var caster = sender as AIHeroClient;
            if (caster != null && R.IsReady() && caster.IsEnemy && BoundHero != null)
            {
                if (Ballistaheros.Any(b => b?.Hero == BoundHero?.Hero && AutoMenu.CheckBoxValue(b.Hero.ToString()) && args.Buff.Name == b.BuffName && AutoMenu.SliderValue(b.Hero + "dis") >= user.Distance(BoundHero)))
                {
                    R.Cast();
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || e.End.Distance(user) > 1000 || !R.IsReady()) return;

            if (user.PredictHealthPercent() <= 20 || user.CountEnemyHeros(1000) > user.CountAllyHeros(1000))
                R.Cast();
        }

        private static void Events_OnIncomingDamage(Events.InComingDamageEventArgs args)
        {
            if (AutoMenu.CheckBoxValue("EDeath") && args.Target.IsMe && args.InComingDamage >= user.TotalShieldHealth())
            {
                E.Cast();
            }

            if (args.Target?.NetworkId == BoundHero?.NetworkId && args.InComingDamage >= args.Target.TotalShieldHealth() && AutoMenu.CheckBoxValue("SoulBound") && R.IsReady())
            {
                R.Cast();
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.E)
            {
                if (Core.GameTickCount - LastE <= Game.Ping + 50)
                {
                    args.Process = false;
                }
                else
                {
                    LastE = Core.GameTickCount;
                }
            }
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (target != null && target.IsKillable(E.Range) && E.IsReady() && EKill(target) && AutoMenu.CheckBoxValue("AutoEUnKillable"))
            {
                E.Cast();
            }
        }

        public override void Active()
        {
            var spear = new Item(ItemId.The_Black_Spear);
            var ally = EntityManager.Heroes.Allies.OrderByDescending(a => a.MaxHealth).FirstOrDefault(a => a != null && a.IsValidTarget(600));
            if (ally != null && spear.IsOwned(user) && spear.IsReady())
            {
                spear.Cast(ally);
            }
            if (BoundHero == null && Created)
            {
                if (File.Exists(appdata + filename))
                {
                    var read = File.ReadAllLines(appdata + filename);
                    BoundHero = EntityManager.Heroes.Allies.FirstOrDefault(a => read.Contains(a.NetworkId.ToString()));
                }
            }

            if (!E.IsReady()) return;
            if (AutoMenu.CheckBoxValue("AutoEBig"))
            {
                foreach (var mob in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsBigMinion() && m.IsKillable(E.Range) && EKill(m)))
                {
                    if (mob != null)
                        E.Cast();
                    return;
                }
            }
        }

        public override void Combo()
        {
            if (ComboMenu.CheckBoxValue("Gapclose"))
            {
                Gapclose();
            }
            if (ComboMenu.CompareSlider("EKillCount", EntityManager.Heroes.Enemies.Count(e => e.IsKillable(E.Range) && ComboMenu.CheckBoxValue(E.Slot) && EKill(e))) && E.IsReady())
            {
                E.Cast();
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (ComboMenu.CheckBoxValue(Q.Slot) && Q.IsReady())
            {
                QCast(target);
            }
        }

        public override void Harass()
        {
            if (EntityManager.Heroes.Enemies.Any(e => RendCount(e) >= HarassMenu.SliderValue("Estacks") && e.IsKillable(E.Range)) && E.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.E) && HarassMenu.CompareSlider("Emana", user.ManaPercent))
            {
                if (HarassMenu.CheckBoxValue("Emin"))
                {
                    if (EntityManager.MinionsAndMonsters.EnemyMinions.Any(e => EKill(e) && e.IsKillable(E.Range)) || EntityManager.MinionsAndMonsters.GetJungleMonsters().Any(e => EKill(e) && e.IsKillable(E.Range)))
                    {
                        E.Cast();
                    }
                }
                else
                {
                    E.Cast();
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (HarassMenu.CheckBoxValue(Q.Slot) && Q.IsReady() && HarassMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                QCast(target);
            }
        }

        public override void LaneClear()
        {
            if (E.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.E) && LaneClearMenu.CompareSlider("Emana", user.ManaPercent) && LaneClearMenu.CompareSlider("Ekills", EntityManager.MinionsAndMonsters.EnemyMinions.Count(e => e.IsKillable(E.Range) && EKill(e))))
            {
                E.Cast();
            }

            if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                foreach (var mob in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsKillable(Q.Range)))
                {
                    QCast(mob, false, LaneClearMenu.SliderValue("Qhits"));
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable()))
            {
                if (Q.IsReady() && E.IsReady())
                {
                    QCast(enemy, KillStealMenu.CheckBoxValue("ETransfer"));
                }

                if (KillStealMenu.CheckBoxValue(E.Slot) && E.IsReady() && enemy.IsKillable(E.Range) && EKill(enemy))
                {
                    E.Cast();
                    return;
                }

                if (KillStealMenu.CheckBoxValue(Q.Slot) && Q.IsReady() && enemy.IsKillable(Q.Range) && Q.WillKill(enemy))
                {
                    QCast(enemy);
                    return;
                }
            }
        }

        private static void Gapclose()
        {
            Orbwalker.ForcedTarget = user.CountEnemyHeros((int)user.GetAutoAttackRange()) < 1 ?
                EntityManager.MinionsAndMonsters.CombinedAttackable.OrderBy(m => m.Distance(Game.CursorPos)).FirstOrDefault(m => !m.IsDead && m.IsEnemy && m.PredictHealth() > 0 && m.IsKillable(user.GetAutoAttackRange())) : null;
        }

        private static void QCast(Obj_AI_Base target, bool transfer = false, int HitCount = -1)
        {
            /*
            var pred = Prediction.Position.GetPrediction(
                new Prediction.Manager.PredictionInput
                    {
                        Range = Q.Range, Delay = Q.CastDelay, Radius = Q.Radius, Target = target, Type = SkillShotType.Linear, From = user.ServerPosition, Speed = Q.Speed,
                        CollisionTypes = new HashSet<CollisionType> { CollisionType.AiHeroClient, CollisionType.ObjAiMinion, CollisionType.YasuoWall }, RangeCheckFrom = user.ServerPosition
                    });*/
            var collidelist = new List<Obj_AI_Base>();
            collidelist.Clear();
            var pred = Q.GetPrediction(target);
            var CastPos = pred.CastPosition;
            var rect = new Geometry.Polygon.Rectangle(user.ServerPosition, CastPos, Q.SetSkillshot().Width);

            if (pred.HitChance < HitChance.Medium) return;

            collidelist.AddRange(EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => new Geometry.Polygon.Circle(m.ServerPosition, m.BoundingRadius).Points.Any(p => rect.IsInside(p)) && !m.IsDead && m.IsValidTarget()));
            if (HitCount == -1)
            {
                collidelist.AddRange(EntityManager.MinionsAndMonsters.GetJungleMonsters().Where(m => new Geometry.Polygon.Circle(m.ServerPosition, m.BoundingRadius).Points.Any(p => rect.IsInside(p)) && !m.IsDead && m.IsValidTarget()));
                collidelist.AddRange(EntityManager.Heroes.Enemies.Where(m => new Geometry.Polygon.Circle(m.ServerPosition, m.BoundingRadius).Points.Any(p => rect.IsInside(p)) && m.NetworkId != target.NetworkId && !m.IsDead && m.IsValidTarget()));
                //Chat.Print(collidelist.Count(o => Q.WillKill(o)) - collidelist.Count);
                if (collidelist.Count(o => Q.WillKill(o)) - collidelist.Count == 0)
                {
                    if (transfer)
                    {
                        if (collidelist.Any(o => EKill(o, target)))
                        {
                            Q.Cast(CastPos);
                        }
                    }
                    else
                    {
                        Q.Cast(CastPos);
                    }
                }
            }
            else
            {
                if (collidelist.Count(o => Q.WillKill(o)) >= HitCount)
                {
                    Q.Cast(CastPos);
                }
            }
        }

        public static bool EKill(Obj_AI_Base target)
        {
            return EDamage(target, RendCount(target)) >= target.TotalShieldHealth() && RendCount(target) > 0;
        }

        public static bool EKill(Obj_AI_Base From, Obj_AI_Base To)
        {
            return EDamage(To, RendCount(From) + RendCount(To)) + Player.Instance.GetSpellDamage(To, SpellSlot.Q) >= To.TotalShieldHealth() && Q.WillKill(From);
        }

        public static int RendCount(Obj_AI_Base target)
        {
            return target.GetBuffCount("KalistaExpungeMarker");
        }

        public static float RendDamage(Obj_AI_Base target, int stacks)
        {
            var flatAD = Player.Instance.FlatPhysicalDamageMod;
            var totalAD = Player.Instance.TotalAttackDamage;
            var index = E.Level - 1;
            var Edmg = new float[] { 20, 30, 40, 50, 60 }[index];
            var EdmgPS = new float[] { 10, 14, 19, 25, 32 }[index];
            var EdmgPSM = new[] { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f }[index];
            if (stacks == 0 || !E.IsLearned)
            {
                return 0;
            }
            return EdmgPS * stacks + (EdmgPSM * totalAD * stacks + Edmg + flatAD * 0.6f) + stacks;
        }

        public static float EDamage(Obj_AI_Base target, int stacks)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, RendDamage(target, stacks));
        }
    }
}
