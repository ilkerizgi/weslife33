using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Syndra
{
    internal class Syndra : Base
    {
        internal static IEnumerable<Obj_AI_Minion> BallsList
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Minion>().Where(o => o != null && !o.IsDead && o.IsValid && o.PredictHealth() > 0 && o.BaseSkinName.Equals("SyndraSphere"));
            }
        }

        private static float LastE;
        private static float LastW;
        public static Spell.Skillshot Eball { get; set; }

        static Syndra()
        {
            Eball = new Spell.Skillshot(SpellSlot.E, 1100, SkillShotType.Linear, 600, 2400, 80) { AllowedCollisionCount = int.MaxValue, DamageType = DamageType.Magical };

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(Eball);
            SpellList.Add(R);

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
                    if (i != R)
                    {
                        HarassMenu.CreateCheckBox(i.Slot, "Use " + i.Slot, i != E);
                        HarassMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        HarassMenu.AddSeparator(0);
                        LaneClearMenu.CreateCheckBox(i.Slot, "Use " + i.Slot, i != E);
                        LaneClearMenu.CreateSlider(i.Slot + "hit", i.Slot + " Hit {0} Minions", 3, 1, 20);
                        LaneClearMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        LaneClearMenu.AddSeparator(0);
                    }
                    KillStealMenu.CreateCheckBox(i.Slot, i.Slot + " KillSteal");
                });

            AutoMenu.CreateCheckBox("QEgap", "Auto QE Anti-Gapcloser");
            AutoMenu.CreateCheckBox("QEint", "Auto QE Interrupter");
            AutoMenu.CreateCheckBox("Egap", "Auto E Anti-Gapcloser");
            AutoMenu.CreateCheckBox("Eint", "Auto E Interrupter");
            AutoMenu.CreateCheckBox("Wunk", "Auto W Unkillable Minions");

            ComboMenu.CreateCheckBox("QE", "Use QE");
            HarassMenu.CreateCheckBox("QE", "Use QE");
            KillStealMenu.CreateCheckBox("QE", "QE KillSteal");

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (W.IsReady() && W.WillKill(target) && target.IsKillable(W.Range) && AutoMenu.CheckBoxValue("Wunk"))
            {
                W.Cast(target);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable()) return;

            if (AutoMenu.CheckBoxValue("QEint") && Q.IsReady() && E.IsReady() && sender.IsKillable(1200))
            {
                QE(sender);
            }
            else
            {
                if (E.IsReady() && AutoMenu.CheckBoxValue("Eint"))
                {
                    if (SelectBall(sender) != null && E.IsInRange(SelectBall(sender)))
                    {
                        Eball.Cast(SelectBall(sender));
                        return;
                    }
                    if (sender.IsKillable(E.Range))
                    {
                        E.Cast(sender, 25);
                    }
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable()) return;

            if (AutoMenu.CheckBoxValue("QEgap") && Q.IsReady() && E.IsReady() && sender.IsKillable(1200))
            {
                QE(sender);
            }
            else
            {
                if (E.IsReady() && AutoMenu.CheckBoxValue("Egap"))
                {
                    if (SelectBall(sender) != null && E.IsInRange(SelectBall(sender)))
                    {
                        Eball.Cast(SelectBall(sender));
                        return;
                    }
                    if (sender.IsKillable(E.Range))
                    {
                        E.Cast(sender, 25);
                    }
                }
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe) return;

            if (args.Slot == SpellSlot.W && W.Handle.ToggleState == 1)
                args.Process = Core.GameTickCount - LastE > 150 + Game.Ping;
            if (args.Slot == SpellSlot.W)
                args.Process = Core.GameTickCount - LastW > 100 + Game.Ping / 2;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.Slot == SpellSlot.E)
                LastE = Core.GameTickCount;
            if (args.Slot == SpellSlot.W)
                LastW = Core.GameTickCount;
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var Qtarget = Q.GetTarget();
            var Wtarget = W.GetTarget();
            var Etarget = E.GetTarget();
            var Rtarget = EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(t => t.IsKillable(R.Range) && RDamage(t) >= t.TotalShieldHealth());
            if (SelectBall(Etarget) == null)
            {
                Etarget = EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(t => (BallsList.Any() ? BallsList.Any(b => b.IsInRange(t, Eball.Range) && E.IsInRange(b)) : t.IsKillable(1200)) && t.IsKillable());
            }
            var FullCombotarget = EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(e => ComboDamage(e, true) >= e.TotalShieldHealth() && e.IsKillable(R.Range));

            if (W.Handle.ToggleState != 1 && Wtarget != null && W.IsReady() && Wtarget.IsKillable(W.Range))
            {
                W.Cast(Wtarget);
            }

            if (FullCombotarget != null && FullCombotarget.IsKillable())
            {
                if (Q.IsReady() && FullCombotarget.IsKillable(Q.Range) && ComboMenu.CheckBoxValue(SpellSlot.Q) && user.Mana >= Q.Mana() + W.Mana() + E.Mana() + R.Mana())
                {
                    Q.Cast(FullCombotarget, 45);
                }
                if (E.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.E) && user.Mana >= W.Mana() + E.Mana() + R.Mana())
                {
                    if (SelectBall(FullCombotarget) != null && E.IsInRange(SelectBall(FullCombotarget)))
                    {
                        Eball.Cast(SelectBall(FullCombotarget));
                        return;
                    }
                    if (FullCombotarget.IsKillable(E.Range))
                    {
                        E.Cast(FullCombotarget, 25);
                        return;
                    }
                }
                if (W.IsReady() && FullCombotarget.IsKillable(W.Range) && ComboMenu.CheckBoxValue(SpellSlot.W) && user.Mana >= W.Mana() + R.Mana())
                {
                    WCast(FullCombotarget);
                }
                if (R.IsReady() && FullCombotarget.IsKillable(R.Range) && ComboMenu.CheckBoxValue(SpellSlot.R) && !(Q.IsReady() && W.IsReady() && E.IsReady()))
                {
                    R.Cast(FullCombotarget);
                }
            }

            if (E.IsReady() && Etarget != null && SelectBall(Etarget) != null && E.IsInRange(SelectBall(Etarget)) && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                Eball.Cast(SelectBall(Etarget));
                return;
            }

            if (Etarget != null && Q.IsReady() && E.IsReady() && ComboMenu.CheckBoxValue("QE"))
            {
                QE(Etarget);
            }

            if (Qtarget != null && Q.IsReady() && Qtarget.IsKillable(Q.Range) && ComboMenu.CheckBoxValue(SpellSlot.Q))
            {
                Q.Cast(Qtarget, 30);
            }

            if (Etarget != null && E.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                if (Etarget.IsKillable(E.Range) && user.PredictHealthPercent() <= 20)
                {
                    E.Cast(Etarget, 25);
                    return;
                }
            }

            if (Wtarget != null && W.IsReady() && Wtarget.IsKillable(W.Range) && ComboMenu.CheckBoxValue(SpellSlot.W))
            {
                W.Cast(Wtarget);
            }

            if (R.IsReady() && Rtarget != null && ComboMenu.CheckBoxValue(SpellSlot.R))
            {
                R.Cast(Rtarget);
            }
        }

        public override void Harass()
        {
            var Qtarget = Q.GetTarget();
            var Wtarget = W.GetTarget();
            var Etarget = E.GetTarget();

            if (SelectBall(Etarget) == null)
            {
                Etarget = EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(t => (BallsList.Any() ? BallsList.Any(b => b.IsInRange(t, Eball.Range) && E.IsInRange(b)) : t.IsKillable(1200)) && t.IsKillable());
            }

            if (Etarget != null && Q.IsReady() && E.IsReady() && HarassMenu.CheckBoxValue("QE") && HarassMenu.CompareSlider("Emana", user.ManaPercent))
            {
                QE(Etarget);
            }

            if (Wtarget != null && W.IsReady() && Wtarget.IsKillable(W.Range) && HarassMenu.CheckBoxValue(SpellSlot.W) && HarassMenu.CompareSlider("Wmana", user.ManaPercent))
            {
                WCast(Wtarget);
                return;
            }
            if (Qtarget != null && Q.IsReady() && Qtarget.IsKillable(Q.Range) && HarassMenu.CheckBoxValue(SpellSlot.Q) && HarassMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                Q.Cast(Qtarget, 30);
                return;
            }
            if (Etarget != null && E.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.E) && HarassMenu.CompareSlider("Emana", user.ManaPercent))
            {
                if (SelectBall(Etarget) != null && E.IsInRange(SelectBall(Etarget)))
                {
                    Eball.Cast(SelectBall(Etarget));
                    return;
                }
                if (Etarget.IsKillable(E.Range) && user.PredictHealthPercent() <= 20)
                {
                    E.Cast(Etarget, 25);
                }
            }
        }

        public override void LaneClear()
        {
            if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                var qminions = Q.SetSkillshot().GetBestCircularCastPosition(Q.LaneMinions());
                if (qminions.HitNumber >= LaneClearMenu.SliderValue("Qhit"))
                {
                    Q.Cast(qminions.CastPosition);
                }
            }

            if (W.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider("Wmana", user.ManaPercent))
            {
                var wminions = W.SetSkillshot().GetBestCircularCastPosition(W.LaneMinions());
                if (wminions.HitNumber + 1 >= LaneClearMenu.SliderValue("Whit"))
                {
                    WCast(wminions.CastPosition);
                }
            }

            if (E.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.E) && LaneClearMenu.CompareSlider("Emana", user.ManaPercent))
            {
                foreach (var ball in BallsList)
                {
                    var Eminions = Eball.SetSkillshot().GetBestLinearCastPosition(Eball.LaneMinions(), 0, ball.ServerPosition.To2D());
                    if (Eminions.HitNumber >= LaneClearMenu.SliderValue("Ehit"))
                    {
                        Eball.Cast(ball.ServerPosition);
                    }
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target);
            }

            if (SelectBall(target) == null)
            {
                if (Q.IsReady() && E.IsReady())
                {
                    var pos = user.ServerPosition.Extend(target.ServerPosition, 100).To3D();
                    Q.Cast(pos);
                    E.Cast(pos);
                }
            }
            else
            {
                E.Cast(SelectBall(target));
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).Where(e => e.IsKillable()))
            {
                if (Q.IsReady() && E.IsReady() && target.IsKillable(1200) && KillStealMenu.CheckBoxValue("QE") && Eball.WillKill(target))
                {
                    QE(target);
                }

                if (W.IsReady() && W.WillKill(target) && target.IsKillable(W.Range) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    WCast(target);
                    return;
                }
                if (Q.IsReady() && Q.WillKill(target) && target.IsKillable(Q.Range) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(target, 30);
                    return;
                }
                if (E.IsReady() && E.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    if (SelectBall(target) != null)
                    {
                        Eball.Cast(SelectBall(target));
                        return;
                    }
                    if (target.IsKillable(E.Range))
                    {
                        E.Cast(target, 25);
                        return;
                    }
                }
                if (R.IsReady() && target.IsKillable(R.Range) && RDamage(target) >= target.PredictHealth() && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    R.Cast(target);
                    return;
                }
            }
        }
        
        internal static Obj_AI_Minion SelectBall(Obj_AI_Base target)
        {
            if (target == null)
                return null;

            Obj_AI_Minion theball = null;
            var CastPosition = Syndra.Q.GetPrediction(target).CastPosition;
            foreach (var ball in BallsList.Where(b => b != null && Syndra.E.IsInRange(b)))
            {
                var source = Player.Instance.PredictPosition();
                var start = ball.ServerPosition.Extend(source, 100).To3D();
                var end = source.Extend(ball.ServerPosition, Syndra.Eball.Range).To3D();
                var rect = new Geometry.Polygon.Rectangle(start, end, Syndra.Eball.Width);
                if (rect.IsInside(CastPosition))
                {
                    theball = ball;
                }
            }
            return theball;
        }

        internal static float ComboDamage(Obj_AI_Base target, bool R = false)
        {
            if (target == null)
                return 0;

            var AAdmg = Player.Instance.IsInAutoAttackRange(target) ? Player.Instance.GetAutoAttackDamage(target) : 0;
            var Qdmg = target.IsKillable(Syndra.Q.Range) ? Syndra.Q.IsReady() ? Player.Instance.GetSpellDamage(target, SpellSlot.Q) : 0 : 0;
            var Wdmg = target.IsKillable(Syndra.W.Range) ? Syndra.W.IsReady() ? Player.Instance.GetSpellDamage(target, SpellSlot.W) : 0 : 0;
            var Edmg = (target.IsKillable(Syndra.E.Range) || SelectBall(target) != null) ? Syndra.E.IsReady() ? Player.Instance.GetSpellDamage(target, SpellSlot.E) : 0 : 0;
            var Rdmg = target.IsKillable(Syndra.R.Range) ? Syndra.R.IsReady() ? R ? RDamage(target) : 0 : 0 : 0;

            return (AAdmg + Qdmg + Wdmg + Edmg + Rdmg) - target.HPRegenRate;
        }

        internal static float RDamage(Obj_AI_Base target)
        {
            if (target == null || !Syndra.R.IsLearned)
                return 0;

            var ap = Player.Instance.FlatMagicDamageMod;
            var index = Player.GetSpell(SpellSlot.R).Level - 1;
            var mindmg = new float[] { 270, 405, 540 }[index] + 0.6f * ap;
            var maxdmg = new float[] { 630, 975, 1260 }[index] + 1.4f * ap;
            var perballdmg = (new float[] { 90, 135, 180 }[index] + 0.2f * ap) * R.Handle.Ammo;

            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical, Math.Max(mindmg, maxdmg) + perballdmg) - 15;
        }

        protected static void QE(Obj_AI_Base target)
        {
            if (Q.IsReady() && E.IsReady() && user.Mana >= Q.Handle.SData.Mana + E.Handle.SData.Mana)
            {
                var castpos = Q.GetPrediction(target).CastPosition;
                if (Q.Cast(Q.IsInRange(castpos) ? castpos : user.ServerPosition.Extend(castpos, E.Range).To3D()))
                {
                    Eball.Cast(castpos);
                }
            }
        }

        protected static void WCast(Obj_AI_Base target)
        {
            if (W.Handle.ToggleState == 1)
            {
                var pick = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(m => m.IsValidTarget(W.Range) && m.PredictHealth() > 5) ?? BallsList.FirstOrDefault(b => W.IsInRange(b));
                if (pick != null)
                {
                    W.Cast(pick);
                }
            }
            else
            {
                W.Cast(target);
            }
        }

        protected static void WCast(Vector3 target)
        {
            if (W.Handle.ToggleState == 1)
            {
                var pick = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(m => m.IsValidTarget(W.Range) && m.PredictHealth() > 5) ?? BallsList.FirstOrDefault(b => W.IsInRange(b));
                if (pick != null)
                {
                    W.Cast(pick);
                }
            }
            else
            {
                W.Cast(target);
            }
        }
    }
}
