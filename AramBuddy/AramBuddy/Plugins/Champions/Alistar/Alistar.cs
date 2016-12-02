using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Alistar
{
    internal class Alistar : Base
    {
        static Alistar()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R && spell != E)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
            }
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("IntW", "Interrupter W");

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
                return;
            if(sender.IsKillable(Q.Range) && Q.IsReady() && AutoMenu.CheckBoxValue("GapQ"))
                Q.Cast();

            if (sender.IsKillable(W.Range) && W.IsReady() && AutoMenu.CheckBoxValue("GapW"))
                W.Cast(sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
                return;
            if (sender.IsKillable(Q.Range) && Q.IsReady() && AutoMenu.CheckBoxValue("IntQ"))
                Q.Cast();

            if (sender.IsKillable(W.Range) && W.IsReady() && AutoMenu.CheckBoxValue("IntW"))
                W.Cast(sender);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(W.Range)) return;

            if (Q.IsReady() && target.IsKillable(Q.Range) && ComboMenu.CheckBoxValue(SpellSlot.Q))
            {
                Q.Cast();
            }
            if (W.IsReady() && target.IsKillable(W.Range) && ComboMenu.CheckBoxValue(SpellSlot.W))
            {
                W.Cast(target);
            }
            if (R.IsReady() && user.PredictHealthPercent() <= 15 && user.CountEnemyHeros(750) > 1 && ComboMenu.CheckBoxValue(SpellSlot.R))
            {
                R.Cast();
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(W.Range)) return;

            if (Q.IsReady() && target.IsKillable(Q.Range) && HarassMenu.CheckBoxValue(SpellSlot.Q) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast();
            }
            if (W.IsReady() && target.IsKillable(W.Range) && HarassMenu.CheckBoxValue(SpellSlot.W) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                W.Cast(target);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast();
                }
                if (W.IsReady() && target.IsKillable(W.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    W.Cast(target);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast();
                }
                if (W.IsReady() && target.IsKillable(W.Range) && W.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    W.Cast(target);
                }
            }
        }
    }
}
