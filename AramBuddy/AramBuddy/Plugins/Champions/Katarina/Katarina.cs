using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Katarina
{
    class Katarina : Base
    {

        public static bool CastingUlt = false;
        public static int LastUltCast = 0;
        static Katarina()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            foreach (var spell in SpellList)
            {
                if (spell != R)
                {
                    ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
                ComboMenu.CreateCheckBox(R.Slot, "Use " + R.Slot);
            }
        }
        public override void Active()
        {
            
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed, Player.Instance.Position);
            if (target == null)
                return;

            Combo:
            if (ComboMenu.CheckBoxValue(Q.Slot) && !CastingUlt)
                if (Q.IsReady() && Player.Instance.IsInRange(target, Q.Range))
                    Q.Cast(target);

            if (ComboMenu.CheckBoxValue(E.Slot) && target.IsSafe() && Player.Instance.Distance(target) >= W.Range / 2f && !CastingUlt)
                if (E.IsReady() && Player.Instance.IsInRange(target, E.Range))
                    E.Cast(target);

            if (ComboMenu.CheckBoxValue(W.Slot) && !CastingUlt)
                if (W.IsReady() && Player.Instance.IsInRange(target, W.Range))
                    W.Cast();

            if (ComboMenu.CheckBoxValue(R.Slot))
                if (R.IsReady() && Player.Instance.IsInRange(target, R.Range))
                {
                    if (Player.Instance.CountEnemiesInRange(R.Range) == 1 && (Player.Instance.GetSpellDamage(target, SpellSlot.Q, DamageLibrary.SpellStages.Default) + Player.Instance.GetSpellDamage(target, SpellSlot.W, DamageLibrary.SpellStages.Default)
                       + Player.Instance.GetSpellDamage(target, SpellSlot.E, DamageLibrary.SpellStages.Default)) > target.TotalShieldHealth())
                        return;

                    Core.DelayAction(() => UnfreezePlayer(), 2500);
                    R.Cast();
                    FreezePlayer();
                }

            if (CastingUlt && Player.Instance.CountEnemiesInRange(W.Range) == 0 && R.IsReady() && ComboMenu.CheckBoxValue(R.Slot) && (Environment.TickCount - LastUltCast) > 1500)
            {
                if (ComboMenu.CheckBoxValue(E.Slot) && E.IsReady())
                {
                    UnfreezePlayer();
                    goto Combo;
                }
            }
        }


        public override void Flee()
        {
            
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed, Player.Instance.Position);
            if (target == null)
                return;
            if (HarassMenu.CheckBoxValue(Q.Slot) && !CastingUlt)
                if (Q.IsReady() && Player.Instance.IsInRange(target, Q.Range))
                    Q.Cast(target);
            if (HarassMenu.CheckBoxValue(W.Slot) && !CastingUlt)
                if (W.IsReady() && Player.Instance.IsInRange(target, W.Range))
                    W.Cast();
            if (HarassMenu.CheckBoxValue(E.Slot) && !CastingUlt)
                if (E.IsReady() && target.IsSafe() && Player.Instance.IsInRange(target, E.Range))
                    E.Cast(target);
        }

        public override void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed, Player.Instance.Position);
            if (target == null)
                return;
            if (KillStealMenu.CheckBoxValue(Q.Slot) && !CastingUlt)
                if (Q.IsReady() && Player.Instance.IsInRange(target, Q.Range) && Q.WillKill(target))
                    Q.Cast(target);
            if (KillStealMenu.CheckBoxValue(W.Slot) && !CastingUlt)
                if (W.IsReady() && Player.Instance.IsInRange(target, W.Range) && W.WillKill(target))
                    W.Cast();
            if (KillStealMenu.CheckBoxValue(E.Slot) && !CastingUlt)
                if (E.IsReady() && Player.Instance.IsInRange(target, E.Range) && E.WillKill(target))
                    E.Cast(target);
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable()))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q))
                    Q.Cast(target);

                if (W.IsReady() && target.IsKillable(W.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.W))
                    W.Cast();
            }
        }

        public static void FreezePlayer()
        { 
            CastingUlt = true;
            LastUltCast = Environment.TickCount;
        }

        public static void UnfreezePlayer()
        {
            CastingUlt = false;
        }
    }
}
