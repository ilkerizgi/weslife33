using System;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace AramBuddy.Plugins.Champions.Yasuo
{
    internal class Yasuo : Base
    {
        /*
        public static bool BeforeImpact
        {
            get
            {
                return LowestKnockUpTime < 125 + Game.Ping && LowestKnockUpTime > 0;
            }
        }
        public static float LowestKnockUpTime
        {
            get
            {
                var buffs =
                    EntityManager.Heroes.Enemies.Where(e => e.IsKillable(R.Range) && e.IsAirborne())
                        .Select(a => a.Buffs.FirstOrDefault(b => (b.Type == BuffType.Knockback || b.Type == BuffType.Knockup) && b.IsActive && b.IsValid))
                        .OrderBy(b => b.EndTime - Game.Time);
                var buff = buffs.FirstOrDefault();
                return buff != null ? (buff.EndTime - Game.Time) * 1000 : 200;
            }
        }*/

        public static Geometry.Polygon.Sector ESector(Obj_AI_Base target)
        {
            return new Geometry.Polygon.Sector(user.ServerPosition, target.PredictPosition(), (float)(60 * Math.PI / 180), 475);
        }

        public static Geometry.Polygon.Sector ESector(Vector3 target)
        {
            return new Geometry.Polygon.Sector(user.ServerPosition, target, (float)(60 * Math.PI / 180), 475);
        }

        public static uint QRange
        {
            get
            {
                return user.IsDashing() ? 375 : (uint)(Q3 ? 1000 : 475);
            }
        }
        public static int QWidth
        {
            get
            {
                return Q3 ? 100 : 50;
            }
        }
        private static bool Q3
        {
            get
            {
                return user.HasBuff("YasuoQ3W");
            }
        }
        static Yasuo()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            foreach (var spell in SpellList.Where(s => s != W))
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            AutoMenu.CreateCheckBox("IntQ3", "Interrupter Q3");
            AutoMenu.CreateCheckBox("GapQ3", "Anti-Gapcloser Q3");
            AutoMenu.CreateCheckBox("RAOE", "Enable Auto R AOE");
            AutoMenu.CreateSlider("Q3AOE", "Auto Q3 AOE Hit", 2, 1, 6);
            AutoMenu.CreateSlider("Rhits", "Auto R AOE Hit", 2, 1, 6);

            ComboMenu.CreateCheckBox("RAOE", "Enable Combo R AOE");
            ComboMenu.CreateSlider("Rhits", "Combo R AOE Hit", 2, 1, 6);

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            E.OnSpellCasted +=(spell, args) => args.Process = user.PredictHealthPercent() > 50;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            UpdateSpells();
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Q3 || !Q.IsReady() || !sender.IsKillable(Q.Range) || !AutoMenu.CheckBoxValue("GapQ3"))
                return;

            Q.Cast(sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if(!Q3 || !Q.IsReady() || !sender.IsKillable(Q.Range) || !AutoMenu.CheckBoxValue("IntQ3"))
                return;

            Q.Cast(sender);
        }

        public override void Active()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(Q.Range)))
            {
                Q3AOE(target, AutoMenu.SliderValue("Q3AOE"));
            }

            RAOE(AutoMenu.CheckBoxValue("RAOE"), AutoMenu.SliderValue("Rhits"));
        }

        public override void Combo()
        {
            RAOE(ComboMenu.CheckBoxValue("RAOE"), ComboMenu.SliderValue("Rhits"));

            var target = TargetSelector.GetTarget(Q.Range + 100, DamageType.Physical);

            if (ComboMenu.CheckBoxValue("E") && target != null && !target.IsValidTarget(user.GetAutoAttackRange()))
            {
                foreach (var obj in ObjectManager.Get<Obj_AI_Base>().OrderBy(e => EndPos(e).Distance(target)).Where(e => e != null && ESector(target).IsInside(e) && e.IsValidTarget()))
                {
                    if (obj == target)
                    {
                        if (EndPos(obj).IsInRange(target.PredictPosition(), user.GetAutoAttackRange()))
                            EQ(obj, target, true);
                    }
                    else
                    {
                        EQ(obj, target, true);
                    }
                }
            }
            if (target != null)
            {
                if (target.IsKillable(Q.Range + 25))
                {
                    if (Q3 && Q.IsReady())
                    {
                        Q3AOE(target, 2);
                    }

                    if (ComboMenu.CheckBoxValue("Q") && !user.IsDashing() && target.IsKillable(Q.Range) && Q.IsReady())
                    {
                        Q.Cast(target, 45);
                    }

                    if (ComboMenu.CheckBoxValue("E") && E.IsReady())
                    {
                        EQ(target, target, true);
                    }
                }
                if (target.IsKillable(R.Range) && R.IsReady() && target.IsAirborne() && ComboMenu.CheckBoxValue("R")
                    && R.WillKill(target))
                {
                    R.Cast(target);
                }
            }
        }

        public override void Flee()
        {
            if (Q3)
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(Q.Range)))
                {
                    Q3AOE(target, 1);
                }
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range + 100, DamageType.Physical);

            if (target == null)
                return;

            if (target.IsKillable(Q.Range + 25))
            {
                if (HarassMenu.CheckBoxValue("Q") && !user.IsDashing() && target.IsKillable(Q.Range) && Q.IsReady())
                {
                    Q.Cast(target, 45);
                }

                if (HarassMenu.CheckBoxValue("E") && E.IsReady())
                {
                    EQ(target, target, true);
                }
            }
        }

        public override void LaneClear()
        {
            if(Q3) return;

            foreach (var m in Q.LaneMinions())
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue("Q"))
                {
                    Q.Cast(m, 45);
                }
            }
        }

        public override void KillSteal()
        {
            foreach (var e in EntityManager.Heroes.Enemies)
            {
                if (Q.IsReady() && e.IsKillable(Q.Range) && Q.WillKill(e) && KillStealMenu.CheckBoxValue("Q"))
                {
                    Q.Cast(e, 30);
                    return;
                }
                if (E.IsReady() && e.IsKillable(E.Range) && E.WillKill(e) && KillStealMenu.CheckBoxValue("E"))
                {
                    ECast(e);
                    return;
                }
                if (R.IsReady() && e.IsAirborne() && e.IsKillable(R.Range) && R.WillKill(e) && KillStealMenu.CheckBoxValue("R"))
                {
                    R.Cast(e);
                    return;
                }
            }
        }

        private static void Q3AOE(AIHeroClient target, int HitNumber)
        {
            if (!user.IsDashing() && Q3)
            {
                Q.CastAOE(HitNumber, Q.Range, target);
            }
        }

        private static void EQ(Obj_AI_Base target1, Obj_AI_Base target2, bool Enabled)
        {
            if (ECast(target1) && E.IsReady() && CanDash(target1))
            {
                if (Enabled && target2 != null && Q.IsReady() && target2.IsInRange(EndPos(target1), 375))
                {
                    Q.Cast(target2);
                }
            }
            if (Enabled && target2 != null && Q.IsReady() && user.IsDashing())
            {
                Q.Cast(target2);
            }
        }

        private static bool ECast(Obj_AI_Base target)
        {
            if (((EndPos(target).UnderEnemyTurret() && Misc.SafeToDive) || !EndPos(target).UnderEnemyTurret())
                && EndPos(target).CountAllyHeros(Config.SafeValue) >= EndPos(target).CountEnemyHeros(Config.SafeValue) && EndPos(target).IsSafe())
            {
                return E.Cast(target);
            }
            return false;
        }

        private static void RAOE(bool Enable, int HitsNumber)
        {
            if (EntityManager.Heroes.Enemies.Count(e => e.IsAirborne() && e.IsKillable(R.Range)) >= HitsNumber && Enable && R.IsReady())
            {
                R.Cast(Game.CursorPos);
            }
        }

        private static bool CanDash(Obj_AI_Base target)
        {
            return !HasYasuoEBuff(target);
        }

        private static Vector3 EndPos(Obj_AI_Base target)
        {
            return Player.Instance.PredictPosition(100).Extend(target.PredictPosition(), 475).To3D();
        }

        private static bool HasYasuoEBuff(Obj_AI_Base target)
        {
            return target.HasBuff("YasuoDashWrapper");
        }

        private static void UpdateSpells()
        {
            Q.Range = QRange;
            Q.SetSkillshot().Width = QWidth;
            var attackspeed = Player.Instance.AttackSpeedMod / 2;
            var reduceddelay = 250 * (attackspeed * 0.017f) * 0.1f * 100;
            Q.CastDelay = (int)(250 - reduceddelay);
            Q.SetSkillshot().Speed = Q3 ? 1150 : int.MaxValue;
        }
    }
}
