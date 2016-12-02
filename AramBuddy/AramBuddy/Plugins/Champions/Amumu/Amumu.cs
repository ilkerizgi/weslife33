using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Amumu
{
    internal class Amumu : Base
    {
        static Amumu()
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
                    HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                }
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            AutoMenu.CreateSlider("RAOE", "R AOE hit count {0}", 3, 1, 5);
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");

            ComboMenu.CreateSlider("RAOE", "R AOE hit count {0}", 3, 1, 5);

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ") && (sender.IsKillable(Q.Range) || e.EndPos.IsInRange(user, Q.Range)))
            {
                if(sender.Position.IsSafe())
                    Q.Cast(sender, HitChance.Low);
                return;
            }
            if (R.IsReady() && AutoMenu.CheckBoxValue("GapR") && (sender.IsKillable(R.Range) || e.EndPos.IsInRange(user, R.Range)))
            {
                R.Cast();
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("IntQ") && sender.IsKillable(Q.Range))
            {
                if (sender.Position.IsSafe())
                    Q.Cast(sender, HitChance.Low);
                return;
            }
            if (R.IsReady() && AutoMenu.CheckBoxValue("IntR") && e.DangerLevel > DangerLevel.Low && sender.IsKillable(R.Range))
            {
                R.Cast();
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ") && (sender.IsKillable(Q.Range) || e.End.IsInRange(user, Q.Range)))
            {
                if (sender.Position.IsSafe())
                    Q.Cast(sender, HitChance.Low);
                return;
            }
            if (R.IsReady() && AutoMenu.CheckBoxValue("GapR") && sender.IsKillable(R.Range))
            {
                R.Cast();
            }
        }

        public override void Active()
        {
            RAOE(AutoMenu.SliderValue("RAOE"));
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
            {
                if (W.Handle.ToggleState == 2)
                {
                    W.Cast();
                }
                return;
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                if (target.Position.IsSafe())
                    Q.Cast(target, HitChance.Low);
            }
            if (ComboMenu.CheckBoxValue(SpellSlot.W) && W.IsReady())
            {
                if (target.IsKillable(W.Range) && W.Handle.ToggleState == 1)
                {
                    W.Cast();
                }
                else
                {
                    W.Cast();
                }
            }
            if (ComboMenu.CheckBoxValue(SpellSlot.E) && E.IsReady() && target.IsKillable(E.Range))
            {
                E.Cast();
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.R) && R.IsReady() && target.IsKillable(R.Range))
            {
                RAOE(ComboMenu.SliderValue("RAOE"));
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
            {
                if (W.Handle.ToggleState == 2)
                {
                    W.Cast();
                }
                return;
            }

            if (HarassMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady() && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                if (target.Position.IsSafe())
                    Q.Cast(target, HitChance.Medium);
            }
            if (HarassMenu.CheckBoxValue(SpellSlot.W) && W.IsReady() && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                if (target.IsKillable(W.Range) && W.Handle.ToggleState == 1)
                {
                    W.Cast();
                }
                else
                {
                    W.Cast();
                }
            }
            if (HarassMenu.CheckBoxValue(SpellSlot.E) && E.IsReady() && target.IsKillable(E.Range) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
            {
                E.Cast();
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                if (W.IsReady() && target.IsKillable(W.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    if (target.IsKillable(W.Range) && W.Handle.ToggleState == 1)
                    {
                        W.Cast();
                    }
                    else
                    {
                        W.Cast();
                    }
                }
                if (E.IsReady() && target.IsKillable(E.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.E) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
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
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    if (target.Position.IsSafe())
                        Q.Cast(target, HitChance.Medium);
                }
                if (W.IsReady() && target.IsKillable(W.Range) && W.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    W.Cast();
                }
                if (E.IsReady() && target.IsKillable(E.Range) && E.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    E.Cast();
                }
                if (R.IsReady() && target.IsKillable(R.Range) && R.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    R.Cast();
                }
            }
        }

        private static void RAOE(int HitCount)
        {
            if (EntityManager.Heroes.Enemies.Count(e => e.IsKillable(R.Range) && e.PredictPosition().IsInRange(user, R.Range)) >= HitCount && R.IsReady())
            {
                R.Cast();
            }
        }
    }
}
