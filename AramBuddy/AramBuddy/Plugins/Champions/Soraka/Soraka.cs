using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Soraka
{
    internal class Soraka : Base
    {
        static Soraka()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            foreach (var spell in SpellList.Where(spell => spell != W && spell != R))
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            //AutoMenu.CreateCheckBox("AutoHeal", "Heal Allies");
            //AutoMenu.CreateCheckBox("AutoR", "Auto Ult saver");
            AutoMenu.CreateCheckBox("AutoRteam", "Auto Ult Team");
            AutoMenu.CreateSlider("AutoRteamHp", "Auto Ult at Team HP {0}", 20, 1);

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Dash.OnDash += Dash_OnDash;
            //Events.OnIncomingDamage += Events_OnIncomingDamage;
        }

        private static void Events_OnIncomingDamage(Events.InComingDamageEventArgs args)
        {
            if (args.Target.IsAlly &&
                (args.InComingDamage >= args.Target.PredictHealth() && AutoMenu.CheckBoxValue("AutoR") && R.IsReady()))
            {
                R.Cast();
            }
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ") && e.EndPos.IsInRange(user, Q.Range))
            {
                Q.Cast(e.EndPos);
                return;
            }

            if (E.IsReady() && AutoMenu.CheckBoxValue("GapE") && e.EndPos.IsInRange(user, E.Range))
            {
                E.Cast(e.EndPos);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ") && e.End.IsInRange(user, Q.Range))
            {
                Q.Cast(e.End);
                return;
            }

            if (E.IsReady() && AutoMenu.CheckBoxValue("GapE") && e.End.IsInRange(user, E.Range))
            {
                E.Cast(e.End);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(E.Range) || !E.IsReady())
                return;

            if (e.DangerLevel >= DangerLevel.Medium)
            {
                E.Cast(sender, HitChance.Medium);
            }
        }

        public override void Active()
        {
            var teamHp = EntityManager.Heroes.Allies.Where(a => a.IsKillable()).Sum(a => a.PredictHealthPercent())/5;

            if (AutoMenu.CheckBoxValue("AutoRteam") && teamHp <= AutoMenu.SliderValue("AutoRteamHp"))
            {
                R.Cast();
            }

            /*
            foreach (
                var ally in
                    EntityManager.Heroes.Allies.Where(
                        a => a.IsKillable(W.Range) && a.PredictHealthPercent() < 50 && user.PredictHealthPercent() >= 40)
                        .Where(ally => AutoMenu.CheckBoxValue("AutoHeal")))
            {
                W.Cast(ally);
            }*/
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            foreach (var spell in SpellList.Where(s => s.IsReady() && s != R && s != W && target.IsKillable(s.Range) && ComboMenu.CheckBoxValue(s.Slot)))
            {
                spell.Cast(target, HitChance.Medium);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            foreach (
                var spell in
                    SpellList.Where(
                        s =>
                            s.IsReady() && s != R && s != W && target.IsKillable(s.Range) && HarassMenu.CheckBoxValue(s.Slot) &&
                            HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                spell.Cast(target, HitChance.Medium);
            }
        }

        public override void LaneClear()
        {
            if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                var farmloc = Q.SetSkillshot().GetBestCircularCastPosition(Q.Enemies());
                if (farmloc.HitNumber > 1)
                {
                    Q.Cast(farmloc.CastPosition);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (
                var spell in
                    SpellList.Where(s => s != R && s != W && s.IsReady() && KillStealMenu.CheckBoxValue(s.Slot)))
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies.Where(
                            m => m != null && m.IsKillable(spell.Range) && spell.WillKill(m)))
                {
                    spell.Cast(target, HitChance.Medium);
                }
            }
        }
    }
}