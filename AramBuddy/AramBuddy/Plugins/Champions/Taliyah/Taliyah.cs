using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Taliyah
{
    internal class Taliyah : Base
    {
        static Taliyah()
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
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(W.Range) || !W.IsReady())
                return;

            W.Cast(sender);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            foreach (var spell in SpellList.Where(s => s.IsReady() && target.IsKillable(s.Range) && ComboMenu.CheckBoxValue(s.Slot)))
            {
                spell.Cast(target);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            foreach (var spell in SpellList.Where(s => s.IsReady() && target.IsKillable(s.Range) && HarassMenu.CheckBoxValue(s.Slot) && HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                spell.Cast(target);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(1000)))
            {
                foreach (
                    var spell in
                        SpellList.Where(s => s.IsReady() && target.IsKillable(s.Range) && LaneClearMenu.CheckBoxValue(s.Slot) && LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
                {
                    spell.Cast(target);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && KillStealMenu.CheckBoxValue(s.Slot)))
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(m => m != null && m.IsKillable(spell.Range) && spell.WillKill(m)))
                {
                    spell.Cast(target);
                }
            }
        }
    }
}
