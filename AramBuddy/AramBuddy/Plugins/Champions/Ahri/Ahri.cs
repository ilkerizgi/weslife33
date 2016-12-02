using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Ahri
{
    internal class Ahri : Base
    {
        static Ahri()
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
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("IntE", "Interrupter E");
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(E.Range) || !E.IsReady() || !AutoMenu.CheckBoxValue("GapE"))
                return;
            E.Cast(sender, HitChance.Low);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(E.Range) || !E.IsReady() || !AutoMenu.CheckBoxValue("IntE"))
                return;
            E.Cast(sender, HitChance.Low);
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
                Q.Cast(target, HitChance.Medium);
            }
            if (W.IsReady() && target.IsKillable(W.Range) && ComboMenu.CheckBoxValue(SpellSlot.W))
            {
                W.Cast();
            }
            if (E.IsReady() && target.IsKillable(E.Range) && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast(target, HitChance.Medium);
            }
            if (R.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.R))
            {
                var pos = target.PredictPosition().Extend(user.PredictPosition(), 300).To3D();
                if(pos.IsSafe())
                    R.Cast(pos);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.Q) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(target, HitChance.Medium);
            }
            if (W.IsReady() && target.IsKillable(W.Range) && HarassMenu.CheckBoxValue(SpellSlot.W) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                W.Cast();
            }
            if (E.IsReady() && target.IsKillable(E.Range) && HarassMenu.CheckBoxValue(SpellSlot.E) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
            {
                E.Cast(target, HitChance.Medium);
            }
        }

        public override void LaneClear()
        {
            var linefarmloc = EntityManager.MinionsAndMonsters.GetLineFarmLocation(EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsKillable(Q.Range)), Q.SetSkillshot().Width, (int)Q.Range);
            if (Q.IsReady() && linefarmloc.HitNumber > 1 && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(linefarmloc.CastPosition);
            }
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(Q.Range)))
            {
                if (W.IsReady() && target.IsKillable(W.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    W.Cast();
                }
                /* Save E
                if (E.IsReady() && target.IsKillable(E.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.E) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast(target, HitChance.Medium);
                }*/
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
                    Q.Cast(target, HitChance.Medium);
                }
                if (W.IsReady() && W.WillKill(target) && target.IsKillable(W.Range) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    W.Cast();
                }
                if (E.IsReady() && E.WillKill(target) && target.IsKillable(E.Range) && KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    E.Cast(target, HitChance.Medium);
                }
                if (R.IsReady() && R.WillKill(target) && target.IsKillable(R.Range) && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    var pos = R.GetPrediction(target).CastPosition;
                    if(pos.IsSafe())
                        R.Cast(target, HitChance.Low);
                }
            }
        }
    }
}
