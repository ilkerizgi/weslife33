using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Azir
{
    internal class Azir : Base
    {
        internal static bool Ehit(Obj_AI_Base target)
        {
            return
                Orbwalker.AzirSoldiers.Select(soldier => new Geometry.Polygon.Rectangle(ObjectManager.Player.ServerPosition, soldier.ServerPosition, target.BoundingRadius + 35))
                    .Any(rectangle => rectangle.IsInside(target));
        }

        static Azir()
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
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");

            ComboMenu.CreateCheckBox("R", "Use R");
            ComboMenu.CreateSlider("RAOE", "R AOE HIT {0}", 2, 1, 5);
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(R.Range) || !R.IsReady() || !AutoMenu.CheckBoxValue("GapR"))
                return;
            R.Cast(sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(R.Range) || !R.IsReady() || !AutoMenu.CheckBoxValue("IntR"))
                return;
            R.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(R.Range) || !R.IsReady() || !AutoMenu.CheckBoxValue("GapR"))
                return;
            R.Cast(sender);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (W.IsReady() && ComboMenu.CheckBoxValue(W.Slot))
            {
                if (!W.IsInRange(target))
                {
                    W.Cast(user.ServerPosition.Extend(target, W.Range).To3D());
                }
                else
                {
                    W.Cast(target);
                }
            }
            else
            {
                if (Q.IsReady() && ComboMenu.CheckBoxValue(Q.Slot))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (E.IsReady() && Ehit(target) && ComboMenu.CheckBoxValue(E.Slot))
                {
                    E.Cast(target);
                }
                if (R.IsReady() && ComboMenu.CheckBoxValue(R.Slot))
                {
                    R.CastAOE(ComboMenu.SliderValue("RAOE"), R.Range, target);
                }
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (W.IsReady() && HarassMenu.CheckBoxValue(W.Slot) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                if (!W.IsInRange(target))
                {
                    W.Cast(user.ServerPosition.Extend(target, W.Range).To3D());
                }
                else
                {
                    W.Cast(target);
                }
            }
            else
            {
                if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (E.IsReady() && Ehit(target) && HarassMenu.CheckBoxValue(E.Slot) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast(target);
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(e => e != null && e.IsKillable(Q.Range)))
            {
                if (W.IsReady() && LaneClearMenu.CheckBoxValue(W.Slot) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    if (!W.IsInRange(target))
                    {
                        W.Cast(user.ServerPosition.Extend(target, W.Range).To3D());
                    }
                    else
                    {
                        W.Cast(target);
                    }
                }
                else
                {
                    if (Q.IsReady() && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                    {
                        Q.Cast(target, HitChance.Low);
                    }
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var spell in SpellList.Where(s => s != W && s != E && s.IsReady() && KillStealMenu.CheckBoxValue(s.Slot)))
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable(spell.Range) && spell.WillKill(e)))
                {
                    spell.Cast(target);
                }
            }
        }
    }
}
