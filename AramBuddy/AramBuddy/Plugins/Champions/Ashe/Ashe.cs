using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Ashe
{
    internal class Ashe : Base
    {
        static Ashe()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            AutoMenu.CreateCheckBox("W", "Flee W");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");
            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R && spell != E)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    if (spell != Q)
                    {
                        HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                        LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    }
                }
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack; ;
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, System.EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !ComboMenu.CheckBoxValue(SpellSlot.Q))
                return;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && !HarassMenu.CheckBoxValue(SpellSlot.Q))
                return;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && !HarassMenu.CheckBoxValue(SpellSlot.Q))
                return;
            if (target.IsValidTarget(Config.SafeValue) && Q.IsReady())
            {
                Q.Cast();
            }
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000) || !R.IsReady() || !AutoMenu.CheckBoxValue("GapR"))
                return;
            R.Cast(sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || e.DangerLevel < DangerLevel.Medium || !sender.IsKillable(1000) || !R.IsReady() || !AutoMenu.CheckBoxValue("IntR"))
                return;
            R.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000) || !R.IsReady() || !AutoMenu.CheckBoxValue("GapR"))
                return;
            R.Cast(sender);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && s != E && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                var skillshot = spell as Spell.Skillshot;
                if (skillshot == R)
                {
                    if (user.PredictHealthPercent() <= 35)
                    {
                        skillshot?.Cast(target, HitChance.Medium);
                    }
                }
                else
                {
                    skillshot?.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && s == W && HarassMenu.CheckBoxValue(s.Slot) && HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                var skillshot = spell as Spell.Skillshot;
                skillshot?.Cast(target, HitChance.Medium);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                foreach (var spell in SpellList.Where(s => s.IsReady() && s == W && LaneClearMenu.CheckBoxValue(s.Slot) && LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
                {
                    var skillshot = spell as Spell.Skillshot;
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(W.Range))
                return;
            if (W.IsReady() && AutoMenu.CheckBoxValue("W") && user.ManaPercent >= 65)
            {
                W.Cast(target, HitChance.Medium);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (var spell in SpellList.Where(s => s.WillKill(target) && s != E && s.IsReady() && target.IsKillable(s.Range) && KillStealMenu.CheckBoxValue(s.Slot) && s.Slot != SpellSlot.E))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        spell.Cast();
                    }
                    else
                    {
                        var skillshot = spell as Spell.Skillshot;
                        skillshot?.Cast(target, HitChance.Medium);
                    }
                }
            }
        }
    }
}
