using System.Linq;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace AramBuddy.Plugins.Champions.Caitlyn
{
    internal class Caitlyn : Base
    {
        static Caitlyn()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            AutoMenu.CreateCheckBox("E", "Flee E");
            AutoMenu.CreateCheckBox("DashW", "Anti-Dash W");
            AutoMenu.CreateCheckBox("DashE", "Anti-Dash E");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("IntW", "Interrupter W");
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

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
        }
        
        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000))
                return;
            {
                if (AutoMenu.CheckBoxValue("DashW") && W.IsReady() && e.EndPos.IsInRange(Player.Instance, W.Range))
                {
                    W.Cast(e.EndPos);
                }
                if (!Player.HasBuff("caitlynheadshot") && !Player.HasBuff("caitlynheadshotrangecheck") && AutoMenu.CheckBoxValue("DashE") && E.IsReady() && e.EndPos.IsInRange(Player.Instance, E.Range))
                {
                    E.Cast(sender as AIHeroClient, HitChance.Medium);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(W.Range) || !W.IsReady() || !AutoMenu.CheckBoxValue("IntW"))
                return;
            W.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000))
                return;
            if (AutoMenu.CheckBoxValue("GapE") && E.IsReady() && e.End.IsInRange(Player.Instance, E.Range))
            {
                E.Cast((Vector3)Player.Instance.ServerPosition.Extend(e.End, E.Range));
            }
            if (AutoMenu.CheckBoxValue("GapW") && W.IsReady() && e.End.IsInRange(Player.Instance, W.Range))
            {
                W.Cast(e.End);
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
                    R.Cast(target);
                }
                if (spell.Slot == SpellSlot.E)
                {
                    if (!Player.HasBuff("caitlynheadshot") && !Player.HasBuff("caitlynheadshotrangecheck"))
                    {
                        E.Cast(target, HitChance.Medium);
                    }
                }
                if (spell.Slot == SpellSlot.Q)
                {
                    if (Player.Instance.GetAutoAttackDamage(target) < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target, HitChance.Medium);
                    }
                }
                else
                {
                    var skillshot = spell as Spell.Skillshot;
                    skillshot?.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in
                SpellList.Where(s => s.IsReady() && s != R && HarassMenu.CheckBoxValue(s.Slot) && HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    if (target.CountEnemyHeros(300) == 0)
                    {
                        R.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.E)
                {
                    if (!Player.HasBuff("caitlynheadshot") && !Player.HasBuff("caitlynheadshotrangecheck"))
                    {
                        E.Cast(target, HitChance.Medium);
                    }
                }
                else
                {
                    var skillshot = spell as Spell.Skillshot;
                    skillshot?.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void LaneClear()
        {
            var linefarmloc = Q.SetSkillshot().GetBestLinearCastPosition(Q.LaneMinions());
            if (Q.IsReady() && linefarmloc.HitNumber > 1 && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(linefarmloc.CastPosition);
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(E.Range))
                return;
            if (E.IsReady() && AutoMenu.CheckBoxValue("E") && user.ManaPercent >= 65)
            {
                E.Cast(target, HitChance.Medium);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (var spell in
                    SpellList.Where(s => s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) && KillStealMenu.CheckBoxValue(s.Slot)))
                {
                    if (spell.Slot == SpellSlot.R)
                    {
                        if (target.CountEnemyHeros(300) == 0)
                            spell.Cast(target);
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
