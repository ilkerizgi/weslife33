using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Rumble
{
    internal class Rumble : Base
    {
        static Rumble()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R && spell != W)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
            }
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var etarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (etarget == null || !etarget.IsKillable(E.Range))
                return;
            if (ComboMenu.CheckBoxValue(SpellSlot.E) && E.IsReady())
                E.Cast(etarget);
            var rtarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (rtarget == null || !rtarget.IsKillable(R.Range))
                return;
            if (ComboMenu.CheckBoxValue(SpellSlot.R) && R.IsReady())
                RCast(rtarget);
            var qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (qtarget == null || !qtarget.IsKillable(Q.Range))
                return;
            if (ComboMenu.CheckBoxValue(Q.Slot) && Q.IsReady())
                Q.Cast();
            if (ComboMenu.CheckBoxValue(W.Slot) && W.IsReady())
                W.Cast();
        }

        public override void Flee()
        {
            if (W.IsReady())
                W.Cast();
        }

        public override void Harass()
        {
            var etarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (etarget == null || !etarget.IsKillable(E.Range))
                return;
            if (HarassMenu.CheckBoxValue(SpellSlot.E) && E.IsReady())
                E.Cast(etarget);
            var qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (qtarget == null || !qtarget.IsKillable(Q.Range))
                return;
            if (HarassMenu.CheckBoxValue(Q.Slot) && Q.IsReady())
                Q.Cast();
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(Q.Range)))
            {
                if (KillStealMenu.CheckBoxValue(Q.Slot) && Q.IsReady() && Q.WillKill(target))
                {
                    Q.Cast();
                    break;
                }
            }
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(E.Range)))
            {
                if (KillStealMenu.CheckBoxValue(E.Slot) && E.IsReady() && E.WillKill(target))
                {
                    E.Cast(target);
                    break;
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                if (target.IsKillable(Q.Range) && Q.IsReady() && LaneClearMenu.CheckBoxValue(Q.Slot))
                    Q.Cast();
                if (target.IsKillable(E.Range) && E.IsReady() && LaneClearMenu.CheckBoxValue(E.Slot))
                    E.Cast(target);
            }
        }

        public static void RCast(AIHeroClient target, int HitCount = 1)
        {
            if (!R.IsReady())
                return;

            var rectlist = new List<Geometry.Polygon.Rectangle>();
            rectlist.Clear();
            var pred = R.GetPrediction(target);

            if (pred.HitChance <= HitChance.Low)
                return;

            var Start = pred.CastPosition.Distance(Player.Instance) > 1625 ? Player.Instance.ServerPosition.Extend(pred.CastPosition, 1625).To3D() : target.ServerPosition;
            var End = pred.CastPosition;

            foreach (var A in EntityManager.Heroes.Enemies.OrderBy(o => o.PredictHealth()).Where(e => e.IsKillable(R.Range) && e.NetworkId != target.NetworkId))
            {
                var predmobB = R.GetPrediction(A);
                End = Start.Extend(predmobB.CastPosition, 1700).To3D();
                rectlist.Add(new Geometry.Polygon.Rectangle(Start, End, R.SetSkillshot().Width));
            }

            var bestpos = rectlist.OrderByDescending(r => EntityManager.Heroes.Enemies.OrderBy(o => o.PredictHealth()).Count(m => r.IsInside(m) && m.IsKillable(R.Range))).FirstOrDefault();

            if (bestpos != null)
            {
                Start = bestpos.Start.To3D();
                End = bestpos.End.To3D();

                if (HitCount > 1)
                {
                    if (EntityManager.Heroes.Enemies.OrderBy(o => o.PredictHealth()).Count(m => bestpos.IsInside(m) && m.IsKillable(R.Range)) >= HitCount)
                    {
                        R.CastStartToEnd(End, Start);
                    }
                }
                else
                {
                    R.CastStartToEnd(End, Start);
                }
            }
            else
            {
                R.CastStartToEnd(End, Start);
            }
        }
    }
}
