using System;
using System.Linq;
using AramBuddy.Plugins.KappaEvade;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Garen
{
    internal class Garen : Base
    {
        static Garen()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
                        
            AutoMenu.CreateCheckBox("Q", "Flee Q");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");

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

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            var t = target as AIHeroClient;
            if (t == null) return;

            if (!Q.IsReady() || !ComboMenu.CheckBoxValue("Q") || !t.IsKillable(Player.Instance.GetAutoAttackRange()))
                return;

            Q.Cast();
            Player.IssueOrder(GameObjectOrder.AttackUnit, t);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Player.Instance.GetAutoAttackRange()) || !Q.IsReady() || !AutoMenu.CheckBoxValue("IntQ"))
                return;
            {
                Q.Cast();
                Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
            }
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    if (target.PredictHealth() <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    {
                        R.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.Q)
                {
                    //
                }
                else
                {
                    var spells = spell as Spell.Active;
                    if (!Player.Instance.HasBuff("GarenE"))
                        spells?.Cast();
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in
                SpellList.Where(s => s.IsReady() && HarassMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    R.Cast(target);
                }
                else
                {
                    var spells = spell as Spell.Active;
                    if (!Player.Instance.HasBuff("GarenE"))
                        spells?.Cast();
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var spell in
                EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget(Player.Instance.GetAutoAttackRange()))
                    .SelectMany(target => SpellList.Where(s => s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot))))
            {
                if (spell.Slot == SpellSlot.R)
                {
                    //
                }
                else
                {
                    var spells = spell as Spell.Active;
                    if (!Player.Instance.HasBuff("GarenE"))
                        spells?.Cast();
                }
            }
        }

        public override void Flee()
        {
            if (Q.IsReady() && AutoMenu.CheckBoxValue("Q"))
            {
                Q.Cast();
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (
                    var spell in
                        SpellList.Where(s => s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) && KillStealMenu.CheckBoxValue(s.Slot)).Where(spell => !Player.Instance.HasBuff("GarenE")))
                {
                    if (spell.Slot == SpellSlot.R)
                    {
                        spell.Cast(target);
                    }
                    else
                    {
                        var spells = spell as Spell.Active;
                        spells?.Cast();
                    }
                }
            }
        }
    }
}
