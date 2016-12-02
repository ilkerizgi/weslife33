using System;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Lulu
{
    internal class Lulu : Base
    {
        private static readonly Obj_AI_Base PixObject =
            ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(o => o.IsAlly && o.Name == "RobotBuddy");

        static Lulu()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            Q1 = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1450, 60);
            {
                if (Pix.IsValid && Pix != null)
                    Q1.SourcePosition = Pix.ServerPosition;
            }

            AutoMenu.CreateCheckBox("Rsave", "R Saver");
            ComboMenu.CreateSlider("RAOE", "R AOE {0}", 3, 1, 5);
            AutoMenu.CreateCheckBox("DashQ", "Q on dashing Targets");
            AutoMenu.CreateCheckBox("FleeQ", "Flee Q");
            AutoMenu.CreateCheckBox("FleeW", "Flee W");
            AutoMenu.CreateCheckBox("FleeE", "Flee E");
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("GapQ1", "Anti-GapCloser Q Pix");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("GapWally", "Shield Engaging Allys");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntW", "Interrupter W");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");
            AutoMenu.CreateCheckBox("Wself", "Shield self W");
            AutoMenu.CreateCheckBox("Wally", "Shield ally W");
            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
            }

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
            //SpellsDetector.OnTargetedSpellDetected += SpellsDetector_OnTargetedSpellDetected;
            //Game.OnTick += Lulu_SkillshotDetector;
        }
        
        private static Spell.Skillshot Q1 { get; }

        private static Obj_AI_Base Pix
        {
            get
            {
                if (PixObject != null && PixObject.IsValid)
                {
                    return PixObject;
                }

                return null;
            }
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q1.Range)) return;
            {
                if (AutoMenu.CheckBoxValue("DashQ") && Q.IsReady() &&
                    (e.EndPos.IsInRange(user, Q.Range) || Pix.IsInRange(e.EndPos, Q.Range)))
                {
                    Q.Cast(e.EndPos);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !W.IsReady()) return;

            if (AutoMenu.CheckBoxValue("IntW") && sender.IsKillable(W.Range))
            {
                W.Cast(sender);
            }

            if (!AutoMenu.CheckBoxValue("IntR") || !sender.IsKillable(R.Range)) return;
            {
                foreach (
                    var ally in
                        EntityManager.Heroes.Allies.Where(a => a.IsKillable(R.Range) && a.Distance(sender) <= 300))
                {
                    R.Cast(ally);
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null) return;
            {
                if (sender.IsEnemy && sender.IsKillable(1000))
                {
                    if (AutoMenu.CheckBoxValue("GapQ") && Q.IsReady() && e.End.IsInRange(user, Q.Range))
                    {
                        Q.Cast(e.End);
                    }
                    if (AutoMenu.CheckBoxValue("GapQ1") && Q.IsReady() && e.End.IsInRange(Pix.ServerPosition, Q1.Range))
                    {
                        Q1.Cast(e.End);
                    }
                    if (AutoMenu.CheckBoxValue("GapW") && W.IsReady() && e.End.IsInRange(user, W.Range))
                    {
                        if (sender.IsKillable(W.Range))
                            W.Cast(sender);
                    }
                    if (AutoMenu.CheckBoxValue("GapE") && E.IsReady() && e.End.IsInRange(user, E.Range))
                    {
                        if (sender.IsKillable(E.Range))
                            E.Cast(sender);
                    }
                    if (AutoMenu.CheckBoxValue("GapR") && R.IsReady() && e.End.IsInRange(user, 300) &&
                        user.PredictHealthPercent() < 15)
                    {
                        R.Cast(user);
                    }
                }
                if (!sender.IsAlly) return;
                {
                    if (!AutoMenu.CheckBoxValue("GapWally") || !W.IsReady() ||
                        !sender.IsInRange(user, W.Range))
                        return;
                    {
                        if (sender.IsKillable(W.Range))
                            W.Cast(sender);
                    }
                }
            }
        }

        public override void Active()
        {
            if (!R.IsReady()) return;
            {
                foreach (
                    var ally in
                        EntityManager.Heroes.Allies.Where(a => a.IsKillable(R.Range))
                            .Where(
                                ally =>
                                    ally.CountEnemyHeros(300) >= ComboMenu.SliderValue("RAOE") &&
                                    ComboMenu.CheckBoxValue("R")))
                {
                    R.Cast(ally);
                }
            }
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)
                                                       && s != R))
            {
                var target = Pix != null
                    ? TargetSelector.GetTarget(E.Range + Q.Range, DamageType.Magical)
                    : TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target == null) return;

                var qPredPlayer = Q.GetPrediction(target);
                var qPredPix = Q1.GetPrediction(target);

                if (spell.Slot == SpellSlot.Q)
                {
                    foreach (
                        var enemy in
                            from enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(E.Range + Q.Range))
                            let qPredPlayer1 = Q.GetPrediction(enemy)
                            let qPredPix1 = Q1.GetPrediction(enemy)
                            where qPredPlayer1.HitChance >= HitChance.Medium && qPredPix1.HitChance >= HitChance.Medium
                            select enemy)
                    {
                        Q.Cast(enemy);
                    }

                    if (qPredPlayer.HitChance >= HitChance.Medium || qPredPix.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.W)
                {
                    {
                        if (target.IsKillable(W.Range))
                            W.Cast(target);
                    }
                }
                if (spell.Slot != SpellSlot.E) continue;
                {
                    if (target.IsKillable(E.Range))
                        E.Cast(target);
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && s != R && HarassMenu.CheckBoxValue(s.Slot)))
            {
                var target = Pix != null
                    ? TargetSelector.GetTarget(E.Range + Q.Range, DamageType.Magical)
                    : TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target == null) return;

                var qPredPlayer = Q.GetPrediction(target);
                var qPredPix = Q1.GetPrediction(target);

                if (spell.Slot == SpellSlot.Q)
                {
                    foreach (
                        var enemy in
                            from enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(E.Range + Q.Range))
                            let qPredPlayer1 = Q.GetPrediction(enemy)
                            let qPredPix1 = Q1.GetPrediction(enemy)
                            where qPredPlayer1.HitChance >= HitChance.Medium && qPredPix1.HitChance >= HitChance.Medium
                            select enemy)
                    {
                        Q.Cast(enemy);
                    }

                    if (qPredPlayer.HitChance >= HitChance.Medium || qPredPix.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.W)
                {
                    {
                        if (target.IsKillable(W.Range))
                            W.Cast(target);
                    }
                }
                if (spell.Slot != SpellSlot.E) continue;
                {
                    if (target.IsKillable(E.Range))
                        E.Cast(target);
                }
            }
        }

        public override void LaneClear()
        {
            foreach (
                var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                foreach (
                    var spell in
                        SpellList.Where(
                            s =>
                                s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot) &&
                                LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        Q.SetSkillshot().CastOnBestFarmPosition();
                    }
                    else
                    {
                        var spells = spell as Spell.Targeted;
                        spells?.Cast(target);
                    }
                }
            }
        }

        public override void Flee()
        {
            if (W.IsReady() && AutoMenu.CheckBoxValue("FleeW") && user.ManaPercent >= 65)
            {
                W.Cast(user);
            }
            if (E.IsReady() && AutoMenu.CheckBoxValue("FleeE") && user.ManaPercent >= 65)
            {
                E.Cast(user);
            }
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range)) return;
            if (Q.IsReady() && AutoMenu.CheckBoxValue("FleeQ") && user.ManaPercent >= 65)
            {
                Q.Cast(target, HitChance.Medium);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (
                    var spell in
                        SpellList.Where(
                            s =>
                                s.WillKill(target) && s != R && s.IsReady() && target.IsKillable(s.Range) &&
                                KillStealMenu.CheckBoxValue(s.Slot)))

                    if (spell.Slot == SpellSlot.Q)
                    {
                        if (Q.GetPrediction(target).HitChance >= HitChance.Medium ||
                            Q1.GetPrediction(target).HitChance >= HitChance.Medium)
                            Q.Cast(target);
                    }
                    else
                    {
                        (spell as Spell.Targeted)?.Cast(target);
                    }
            }
        }
    }
}