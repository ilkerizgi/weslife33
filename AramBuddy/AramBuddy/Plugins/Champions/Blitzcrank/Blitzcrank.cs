using System;
using System.Linq;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Blitzcrank
{
    internal class Blitzcrank : Base
    {
        static Blitzcrank()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            ComboMenu.CreateCheckBox("UseRaoe", "Use R AOE");
            ComboMenu.CreateSlider("RAOE", "R AOE {0}", 3, 1, 5);
            AutoMenu.CreateCheckBox("DashQ", "Q on dashing Targets");
            AutoMenu.CreateCheckBox("FleeW", "Flee W");
            AutoMenu.CreateCheckBox("TurretQ", "Under Turret Q");
            AutoMenu.CreateCheckBox("TurretE", "Under Turret E");
            AutoMenu.CreateCheckBox("TurretR", "Under Turret R");
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            AutoMenu.CreateCheckBox("IntE", "Interrupter E");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");

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
            Game.OnTick += FuckEmUpUnderTurret;
        }
        
        private static void FuckEmUpUnderTurret(EventArgs args)
        {
            if (user.IsDead) return;
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e.UnderEnemyTurret()))
                {
                    if (Q.IsReady() && AutoMenu.CheckBoxValue("TurretQ") && enemy.IsKillable(Q.Range) &&
                        user.ServerPosition.UnderAlliedTurret())
                    {
                        Q.Cast(enemy, HitChance.Medium);
                    }
                    if (!enemy.ServerPosition.UnderAlliedTurret()) continue;
                    {
                        if (E.IsReady() && enemy.IsKillable(E.Range) && AutoMenu.CheckBoxValue("TurretE"))
                        {
                            E.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, enemy);
                        }
                        if (R.IsReady() && enemy.IsKillable(E.Range) && AutoMenu.CheckBoxValue("TurretR"))
                        {
                            R.Cast();
                        }
                    }
                }
            }
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q.Range)) return;
            {
                var col = Prediction.Position.Collision.LinearMissileCollision(sender, user.ServerPosition.To2D(),
                    e.EndPos.To2D(), Q.SetSkillshot().Speed, Q.SetSkillshot().Width, Q.CastDelay, 20);
                if (AutoMenu.CheckBoxValue("DashQ") && Q.IsReady() &&
                    (e.EndPos.IsInRange(user, Q.Range) && !col))
                {
                    Q.Cast(e.EndPos);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy) return;

            if (AutoMenu.CheckBoxValue("IntQ") && sender.IsKillable(Q.Range) && Q.IsReady())
            {
                Q.Cast(sender, HitChance.Medium);
            }

            if (AutoMenu.CheckBoxValue("IntE") && sender.IsKillable(E.Range) && E.IsReady())
            {
                E.Cast();
                Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
            }

            if (!AutoMenu.CheckBoxValue("IntR") || !sender.IsKillable(R.Range)) return;
            {
                R.Cast();
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null) return;
            {
                if (sender.IsEnemy && sender.IsKillable(1000))
                {
                    var col = Prediction.Position.Collision.LinearMissileCollision(sender, user.ServerPosition.To2D(),
                        e.End.To2D(), Q.SetSkillshot().Speed, Q.SetSkillshot().Width, Q.CastDelay, 20);

                    if (AutoMenu.CheckBoxValue("GapQ") && Q.IsReady() &&
                        (e.End.IsInRange(user, Q.Range) && !col))
                    {
                        Q.Cast(e.End);
                    }
                    if (AutoMenu.CheckBoxValue("GapE") && E.IsReady() && e.End.IsInRange(user, E.Range))
                    {
                        if (sender.IsKillable(E.Range))
                        {
                            E.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                        }
                    }
                    if (AutoMenu.CheckBoxValue("GapR") && R.IsReady() && e.End.IsInRange(user, R.Range))
                    {
                        R.Cast();
                    }
                }
            }
        }

        public override void Active()
        {
            if (!R.IsReady()) return;
            {
                foreach (
                    // ReSharper disable once UnusedVariable
                    var enemy in
                        EntityManager.Heroes.Enemies.Where(e => e.IsKillable(R.Range))
                            .Where(
                                e =>
                                    e.CountEnemyHeros(300) >= ComboMenu.SliderValue("RAOE") &&
                                    ComboMenu.CheckBoxValue("UseRaoe")))
                {
                    R.Cast();
                }
            }
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target == null) return;

                if (spell.Slot == SpellSlot.Q)

                {
                    Q.Cast(target, HitChance.Medium);
                }

                if (spell.Slot == SpellSlot.W)
                {
                    {
                        if (target.IsKillable(400))
                            W.Cast();
                    }
                }

                if (spell.Slot == SpellSlot.E)
                {
                    if (target.IsKillable(E.Range))
                    {
                        E.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                }

                if (spell.Slot != SpellSlot.R) continue;
                {
                    if (target.IsKillable(R.Range))
                        R.Cast();
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && s != R && HarassMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target == null) return;

                if (spell.Slot == SpellSlot.Q)

                {
                    Q.Cast(target, HitChance.Medium);
                }

                if (spell.Slot == SpellSlot.W)
                {
                    {
                        if (target.IsKillable(400))
                            W.Cast();
                    }
                }

                if (spell.Slot != SpellSlot.E) continue;
                {
                    if (target.IsKillable(E.Range))
                    {
                        E.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                }
            }
        }

        public override void LaneClear()
        {
            //??
        }

        public override void Flee()
        {
            if (W.IsReady() && AutoMenu.CheckBoxValue("FleeW") && user.ManaPercent >= 65)
            {
                W.Cast();
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
                                s.WillKill(target) && s != R && s.IsReady() && target.IsKillable(s.Range) && s != W &&
                                KillStealMenu.CheckBoxValue(s.Slot)
                            ))

                    switch (spell.Slot)
                    {
                        case SpellSlot.Q:
                            Q.Cast(target, HitChance.Medium);
                            break;
                        case SpellSlot.E:
                            E.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            break;
                        case SpellSlot.R:
                            R.Cast();
                            break;
                    }
            }
        }
    }
}