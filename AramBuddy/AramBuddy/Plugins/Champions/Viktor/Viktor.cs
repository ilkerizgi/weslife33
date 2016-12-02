using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace AramBuddy.Plugins.Champions.Viktor
{
    internal class Viktor : Base
    {
        private static string ViktorBaseRName = "ViktorChaosStorm";
        private static Obj_GeneralParticleEmitter ViktorRObj;

        private static bool IsCastingR
        {
            get
            {
                return user.HasBuff("ViktorChaosStormTimer") || (!R.Name.Equals(ViktorBaseRName) && ViktorRObj != null);
            }
        }
        
        static Viktor()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            SpellList.ForEach(
                i =>
                {
                    ComboMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                    if (i != R && i != W)
                    {
                        HarassMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                        HarassMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        HarassMenu.AddSeparator(0);
                        LaneClearMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                        LaneClearMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        LaneClearMenu.AddSeparator(0);
                    }
                    KillStealMenu.CreateCheckBox(i.Slot, i.Slot + " KillSteal");
                });

            AutoMenu.Add("Wmode", new ComboBox("GapCloser W Mode", 1, "Place On Self", "Place On Enemy"));
            AutoMenu.CreateCheckBox("GapW", "Auto W Anti-GapCloser");
            AutoMenu.CreateCheckBox("IntW", "Auto W Interrupter");
            AutoMenu.CreateCheckBox("IntR", "Auto R Interrupter");
            AutoMenu.CreateCheckBox("Qunk", "Auto Q UnKillable Minions");

            ComboMenu.CreateSlider("RAOE", "R AoE Hit Count {0}", 2, 1, 6);
            ComboMenu.CreateSlider("RMulti", "Mutilply R Damage By X{0} Times", 3, 1, 10);

            LaneClearMenu.CreateSlider("Ehits", "E Hit Count {0}", 3, 1, 20);

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var create = sender as Obj_GeneralParticleEmitter;
            if (create != null && create.Name.Equals("Viktor_ChaosStorm_green.troy", StringComparison.CurrentCultureIgnoreCase))
            {
                ViktorRObj = null;
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var create = sender as Obj_GeneralParticleEmitter;
            if(create != null && create.Name.Equals("Viktor_ChaosStorm_green.troy", StringComparison.CurrentCultureIgnoreCase))
            {
                ViktorRObj = create;
            }
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (target != null && AutoMenu.CheckBoxValue("Qunk") && target.IsKillable(Q.Range) && Q.WillKill(target) && Q.IsReady())
            {
                Q.Cast(target);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable() || e.End.Distance(user) > W.Range || !AutoMenu.CheckBoxValue("GapW") || !W.IsReady()) return;

            W.Cast(AutoMenu["Wmode"].Cast<ComboBox>().CurrentValue == 0 ? user : sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable()) return;

            if (sender.IsKillable(W.Range) && AutoMenu.CheckBoxValue("IntW") && W.IsReady())
            {
                W.Cast(sender);
                return;
            }
            if (sender.IsKillable(R.Range) && AutoMenu.CheckBoxValue("IntR") && R.IsReady() && e.DangerLevel >= DangerLevel.Medium)
            {
                R.Cast(sender);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.Q) Orbwalker.ResetAutoAttack();
        }

        public override void Active()
        {
            if (ViktorRObj != null && (ViktorRObj.IsDead || !ViktorRObj.IsValid))
            {
                ViktorRObj = null;
            }

            var target = TargetSelector.GetTarget(1250, DamageType.Magical) ?? EntityManager.Heroes.Enemies.OrderBy(e => e.Distance(ViktorRObj?.Position ?? Game.CursorPos)).FirstOrDefault(e => e.IsKillable());
            if (IsCastingR && target != null)
            {
                Player.Instance.Spellbook.CastSpell(SpellSlot.R, target.PredictPosition(), true);
            }
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range - 100, DamageType.Magical);
            if (target == null || !target.IsKillable(E.Range)) return;

            if (ComboMenu.CheckBoxValue(SpellSlot.E) && E.IsReady())
            {
                target.ECast();
            }

            if (target.IsKillable(user.GetAutoAttackRange()) && ComboMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                Q.Cast(target);
            }

            if (!Q.IsReady() && !E.IsReady())
            {
                if (target.IsKillable(W.Range) && ComboMenu.CheckBoxValue(SpellSlot.W) && W.IsReady())
                {
                    W.Cast(target, HitChance.Medium);
                }

                if (target.IsKillable(R.Range) && ComboMenu.CheckBoxValue(SpellSlot.R) && R.IsReady())
                {
                    if (R.WillKill(target, ComboMenu.SliderValue("RMulti")))
                    {
                        R.Cast(target);
                    }

                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(R.Range + R.SetSkillshot().Width)))
                    {
                        if (enemy.CountEnemyHeros(R.SetSkillshot().Width) >= ComboMenu.SliderValue("RAOE"))
                        {
                            R.Cast(enemy);
                        }
                    }
                }
            }
        }

        public override void Flee()
        {
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(E.Range)) return;

            if (HarassMenu.CheckBoxValue(SpellSlot.E) && HarassMenu.CompareSlider("Emana", user.ManaPercent) && E.IsReady())
            {
                target.ECast();
            }

            if (target.IsKillable(Q.Range) && HarassMenu.CompareSlider("Qmana", user.ManaPercent) && HarassMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                Q.Cast(target);
            }
        }

        public override void LaneClear()
        {
            if (LaneClearMenu.CheckBoxValue(SpellSlot.E) && E.IsReady())
            {
                Vectors.ECast(LaneClearMenu.SliderValue("Ehits"));
            }
            if (LaneClearMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                foreach (var mob in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsKillable(Q.Range) && Q.WillKill(m)))
                {
                    if (mob != null)
                        Q.Cast(mob);
                }
            }
        }

        public override void KillSteal()
        {
            if (KillStealMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(m => m.IsKillable(Q.Range) && Q.WillKill(m)))
                {
                    if (target != null)
                        Q.Cast(target);
                }
            }
            if (KillStealMenu.CheckBoxValue(SpellSlot.E) && E.IsReady())
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(E.Range) && E.WillKill(e)))
                {
                    target.ECast();
                }
            }
            if (KillStealMenu.CheckBoxValue(SpellSlot.R) && R.IsReady())
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(R.Range) && R.WillKill(e, ComboMenu.SliderValue("RMulti"))))
                {
                    R.Cast(target);
                }
            }
        }
    }

    internal static class Vectors
    {
        public static void ECast(this AIHeroClient target, int HitCount = 1)
        {
            if (!Base.E.IsReady()) return;

            var rectlist = new List<Geometry.Polygon.Rectangle>();
            rectlist.Clear();
            var pred = Base.E.GetPrediction(target);

            if (pred.HitChance < HitChance.Low) return;

            Vector3 Start = pred.CastPosition.Distance(Player.Instance) > 525 ? Player.Instance.ServerPosition.Extend(pred.CastPosition, 525).To3D() : target.ServerPosition;
            Vector3 End = pred.CastPosition;

            foreach (var A in EntityManager.Heroes.Enemies.OrderBy(o => o.PredictHealth()).Where(e => e.IsKillable(Base.E.Range) && e.NetworkId != target.NetworkId))
            {
                var predmobB = Base.E.GetPrediction(A);
                End = Start.Extend(predmobB.CastPosition, 600).To3D();
                rectlist.Add(new Geometry.Polygon.Rectangle(Start, End, Base.E.SetSkillshot().Width));
            }

            var bestpos = rectlist.OrderByDescending(r => EntityManager.Heroes.Enemies.OrderBy(o => o.PredictHealth()).Count(m => r.IsInside(m) && m.IsKillable(Base.E.Range))).FirstOrDefault();

            if (bestpos != null)
            {
                Start = bestpos.Start.To3D();
                End = bestpos.End.To3D();

                if (HitCount > 1)
                {
                    if (EntityManager.Heroes.Enemies.OrderBy(o => o.PredictHealth()).Count(m => bestpos.IsInside(m) && m.IsKillable(Base.E.Range)) >= HitCount)
                    {
                        Base.E.CastStartToEnd(End, Start);
                    }
                }
                else
                {
                    Base.E.CastStartToEnd(End, Start);
                }
            }
            else
            {
                    Base.E.CastStartToEnd(End, Start);
            }
        }


        public static void ECast(int HitCount = 1)
        {
            if (!Base.E.IsReady()) return;

            var rectlist = new List<Geometry.Polygon.Rectangle>();
            rectlist.Clear();
            Vector3 Start;
            Vector3 End;
            var mobs = EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(o => o.PredictHealth()).Where(e => e.IsKillable(Base.E.Range));
            
            foreach (var A in mobs)
            {
                var predmob = Base.E.GetPrediction(A);
                Start = predmob.CastPosition.Distance(Player.Instance) > 525 ? Player.Instance.ServerPosition.Extend(predmob.CastPosition, 525).To3D() : A.ServerPosition;
                var mobs2 = EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(o => o.PredictHealth()).Where(e => e.IsKillable(Base.E.Range) && e.NetworkId != A.NetworkId && e.IsInRange(A, 600));
                foreach (var B in mobs2)
                {
                    var predmobB = Base.E.GetPrediction(B);
                    End = Start.Extend(predmobB.CastPosition, 600).To3D();
                    rectlist.Add(new Geometry.Polygon.Rectangle(Start, End, Base.E.SetSkillshot().Width));
                }
            }

            var bestpos = rectlist.OrderByDescending(r => EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(o => o.PredictHealth()).Count(m => r.IsInside(m) && m.IsKillable(Base.E.Range))).FirstOrDefault();

            if (bestpos != null)
            {
                var mobs3 = EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(o => o.PredictHealth()).Count(m => bestpos.IsInside(m) && m.IsKillable(Base.E.Range));
                if (mobs3 >= HitCount)
                {
                    Start = bestpos.Start.To3D();
                    End = bestpos.End.To3D();
                    Base.E.CastStartToEnd(End, Start);
                }
            }
        }
    }
}
