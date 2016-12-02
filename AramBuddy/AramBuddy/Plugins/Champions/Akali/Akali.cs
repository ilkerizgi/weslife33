using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Akali
{
    internal class Akali : Base
    {
        static Akali()
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
                if (spell != R)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.Q))
            {
                Q.Cast(target);
            }
            if (W.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.W) && user.CountEnemyHeros(1000) > 1)
            {
                W.Cast(user.PredictPosition());
            }
            if (E.IsReady() && target.IsKillable(E.Range) && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast();
            }
            if (R.IsReady() && target.IsKillable(R.Range) && ComboMenu.CheckBoxValue(SpellSlot.R) && target.CountAllyHeros(750) > 1)
            {
                R.Cast(target);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.Q))
            {
                Q.Cast(target);
            }
            if (W.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.W) && user.CountEnemyHeros(1000) > 1)
            {
                W.Cast(user.PredictPosition());
            }
            if (E.IsReady() && target.IsKillable(E.Range) && HarassMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast();
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(Q.Range)))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (W.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.W) && user.CountEnemyHeros(1000) > 1)
                {
                    W.Cast(user.PredictPosition());
                }
                if (E.IsReady() && target.IsKillable(E.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.E))
                {
                    E.Cast();
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable(Q.Range)))
            {
                if (Q.IsReady() && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (E.IsReady() && E.WillKill(target) && target.IsKillable(E.Range) && KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    E.Cast();
                }
                if (R.IsReady() && R.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    R.Cast(target);
                }
            }
        }
    }
}
