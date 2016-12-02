using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Aatrox
{
    internal class Aatrox : Base
    {
        static Aatrox()
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
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            AutoMenu.CreateCheckBox("flee", "Flee E");
            AutoMenu.CreateSlider("RAOE", "R AOE HIT {0}", 3, 0, 5);
            ComboMenu.CreateSlider("RAOE", "R AOE HIT {0}", 3, 0, 5);

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q.Range) || !Q.IsReady() || user.PredictHealthPercent() < 15 || !AutoMenu.CheckBoxValue("GapQ"))
                return;
            Q.Cast(sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q.Range) || !Q.IsReady() || user.PredictHealthPercent() < 15 || !AutoMenu.CheckBoxValue("IntQ"))
                return;
            Q.Cast(sender);
        }

        public override void Active()
        {
            if(R.IsReady())
            RAOE(AutoMenu);
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if(target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.Q))
            {
                QAOE(target);
            }
            if (W.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.W))
            {
                if (W.Handle.ToggleState == 1 && user.PredictHealthPercent() > 50)
                {
                    W.Cast();
                }
                else
                {
                    W.Cast();
                }
            }
            if (E.IsReady() && target.IsKillable(E.Range) && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast(target, HitChance.Medium);
            }
            if (R.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.R))
            {
                RAOE(ComboMenu);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.Q) && user.PredictHealthPercent() > 25)
            {
                QAOE(target);
            }
            if (W.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.W))
            {
                if (W.Handle.ToggleState == 1 && user.PredictHealthPercent() > 50)
                {
                    W.Cast();
                }
                else
                {
                    W.Cast();
                }
            }
            if (E.IsReady() && target.IsKillable(E.Range) && HarassMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast(target, HitChance.Medium);
            }
        }

        public override void LaneClear()
        {
            var Cirarmloc = Q.SetSkillshot().GetBestCircularCastPosition(Q.LaneMinions());
            if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && user.PredictHealthPercent() > 25)
            {
                if (user.CountEnemyHeros((int)(1000 + Q.Range)) < 2 && Cirarmloc.HitNumber > 2)
                {
                    var pos = Cirarmloc.CastPosition;
                    if(pos.IsSafe())
                        Q.Cast(pos);
                }
            }
            if (W.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.W))
            {
                if (W.Handle.ToggleState == 1 && user.PredictHealthPercent() > 50)
                {
                    W.Cast();
                }
                else
                {
                    W.Cast();
                }
            }
            
            var linefarmloc = E.SetSkillshot().GetBestLinearCastPosition(E.LaneMinions());
            if (E.IsReady() && linefarmloc.HitNumber > 1 && LaneClearMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast(linefarmloc.CastPosition);
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if(target == null) return;
            if (AutoMenu.CheckBoxValue("flee") && E.IsReady())
            {
                E.Cast(target);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable(Q.Range)))
            {
                if (Q.IsReady() && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    var pos = Q.GetPrediction(target).CastPosition;
                    if(pos.IsSafe())
                        Q.Cast(target);
                }
                if (W.IsReady() && W.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    if (W.Handle.ToggleState == 1 && user.PredictHealthPercent() > 50)
                    {
                        W.Cast();
                    }
                    else
                    {
                        W.Cast();
                    }
                }
                if (E.IsReady() && E.WillKill(target) && target.IsKillable(E.Range) && KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    E.Cast(target, HitChance.Medium);
                }
                if (R.IsReady() && R.WillKill(target) && target.IsKillable(R.Range) && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    R.Cast();
                }
            }
        }

        private static void QAOE(Obj_AI_Base target)
        {
            if (Q.GetPrediction(target).CastPosition.CountEnemyHeros(((Spell.Skillshot)Q).Width) >= 2)
            {
                var pos = Q.GetPrediction(target).CastPosition;
                if (pos.IsSafe())
                    Q.Cast(target, HitChance.Medium);
            }
        }

        private static void RAOE(Menu menu)
        {
            if(menu.CompareSlider("RAOE", user.CountEnemyHeros((int)R.Range)))
            {
                R.Cast();
            }
        }
    }
}
