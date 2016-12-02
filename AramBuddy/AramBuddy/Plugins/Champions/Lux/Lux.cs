using System;
using System.Linq;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Lux
{
    internal class Lux : Base
    {
        static Lux()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            AutoMenu.CreateCheckBox("FleeQ", "Flee Q");
            AutoMenu.CreateCheckBox("FleeW", "Flee W");
            AutoMenu.CreateCheckBox("FleeE", "Flee E");
            //AutoMenu.CreateCheckBox("W", "W incoming Dmg self");
            //AutoMenu.CreateCheckBox("Wallies", "W incoming Dmg allies");
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            ComboMenu.CreateSlider("RAOE", "R AOE HIT {0}", 3, 1, 5);

            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R && spell != W)
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
            //Game.OnTick += Lux_SkillshotDetector;
            Game.OnTick += Lux_PopE;
            //SpellsDetector.OnTargetedSpellDetected += SpellsDetector_OnTargetedSpellDetected;
        }
        
        private static void Lux_PopE(EventArgs args)
        {
            if (user.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2 ||
                user.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1)
            {
                E.Cast();
            }
        }

        private static void SpellsDetector_OnTargetedSpellDetected(Obj_AI_Base sender, Obj_AI_Base target,
            GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spell)
        {
            if (target.IsMe && spell.DangerLevel >= 3 && AutoMenu.CheckBoxValue("W") && W.IsReady())
            {
                W.Cast(user);
            }
            if (!AutoMenu.CheckBoxValue("Wallies") || !W.IsReady() || user.ManaPercent < 65)
                return;
            foreach (var ally in
                EntityManager.Heroes.Allies.Where(
                    a => !a.IsDead && !a.IsZombie && a.Distance(user) <= W.Range)
                    .Where(ally => target.NetworkId.Equals(ally.NetworkId)))
            {
                W.Cast(ally);
            }
        }

        private static void Lux_SkillshotDetector(EventArgs args)
        {
            if (AutoMenu.CheckBoxValue("W") && W.IsReady())
            {
                foreach (var spell in Collision.NewSpells.Where(spell => user.IsInDanger(spell)))
                {
                    W.Cast(spell.Caster);
                }
            }
            if (!AutoMenu.CheckBoxValue("Wallies") || !W.IsReady() || user.ManaPercent < 65)
                return;
            {
                foreach (var ally in
                    Collision.NewSpells.Where(spell => user.IsInDanger(spell))
                        .SelectMany(
                            spell =>
                                EntityManager.Heroes.Allies.Where(
                                    a => a.IsInRange(user, W.Range) && a.IsInDanger(spell)))
                    )
                {
                    W.Cast(ally);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q.Range) || !Q.IsReady() ||
                !AutoMenu.CheckBoxValue("IntQ"))
                return;
            Q.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
                return;
            if (Q.IsReady() && (e.End.IsInRange(user, Q.Range)) && AutoMenu.CheckBoxValue("GapQ"))
                Q.Cast(e.End);
            if (W.IsReady() && (e.End.IsInRange(user, Q.Range)) && AutoMenu.CheckBoxValue("GapW"))
                W.Cast(sender);
            if (E.IsReady() && (e.End.IsInRange(user, Q.Range)) && AutoMenu.CheckBoxValue("GapE"))
                E.Cast(sender, HitChance.Medium);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    var rTarget = target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                    if (target != null && target.IsKillable(R.Range))
                    {
                        R.CastAOE(ComboMenu.SliderValue("RAOE"), R.Range, rTarget); // still testing

                        if (R.WillKill(rTarget))
                        {
                            R.Cast(rTarget, HitChance.Medium);
                        }
                    }
                }
                if (spell.Slot == SpellSlot.W)
                {
                    //
                }
                else
                {
                    var skillshot = spell as Spell.Skillshot;
                    {
                        skillshot.Cast(target, HitChance.Medium);
                    }
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in
                SpellList.Where(
                    s =>
                        s.IsReady() && HarassMenu.CheckBoxValue(s.Slot) &&
                        HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent) && s != W && s != R))
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                var skillshot = spell as Spell.Skillshot;
                {
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void LaneClear()
        {
            foreach (
                var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                foreach (
                    var skillshot in
                        SpellList.Where(
                            s =>
                                s.IsReady() && LaneClearMenu.CheckBoxValue(s.Slot) &&
                                LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent) && s != W && s != R)
                            .Select(spell => spell as Spell.Skillshot))
                {
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(W.Range))
                return;
            if (W.IsReady() && AutoMenu.CheckBoxValue("FleeW") && user.ManaPercent >= 65)
            {
                W.Cast(target);
            }
            if (Q.IsReady() && AutoMenu.CheckBoxValue("FleeQ") && user.ManaPercent >= 65)
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget(Q.Range)))
                {
                    Q.Cast(enemy, HitChance.Medium);
                }
            if (!E.IsReady() || !AutoMenu.CheckBoxValue("FleeE") || !(user.ManaPercent >= 65))
                return;
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget(E.Range)))
                {
                    E.Cast(enemy, HitChance.Medium);
                }
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (
                    var skillshot in
                        SpellList.Where(
                            s =>
                                s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) && s.Slot != SpellSlot.W &&
                                KillStealMenu.CheckBoxValue(s.Slot))
                            .Select(spell => spell as Spell.Skillshot))
                {
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }
    }
}