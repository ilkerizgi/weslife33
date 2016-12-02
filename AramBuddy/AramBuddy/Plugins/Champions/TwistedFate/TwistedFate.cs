using System;
using System.Linq;
using AramBuddy.MainCore.Logics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.TwistedFate
{
    internal class TwistedFate : Base
    {
        private static bool Selecting;
        private static int lastcasted;

        static TwistedFate()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            
            foreach (var spell in SpellList.Where(s => s != E && s != R))
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit target, EventArgs args)
        {
            var t = target as AIHeroClient;
            if(t == null) return;
            
            if (ComboMenu.CheckBoxValue(W.Slot))
            {
                SetectCard(t);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.Slot != SpellSlot.W)
                return;
            if (args.SData.Name.Equals("PickACard", StringComparison.CurrentCultureIgnoreCase))
            {
                Selecting = true;
            }
            if (args.SData.Name.Equals("GoldCardLock", StringComparison.CurrentCultureIgnoreCase) || args.SData.Name.Equals("RedCardLock", StringComparison.CurrentCultureIgnoreCase)
                || args.SData.Name.Equals("BlueCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                Selecting = false;
            }
        }

        private static void SetectCard(Obj_AI_Base target)
        {
            var card = "Blue";
            if (user.CountEnemyHeros(Config.SafeValue) > 0 && user.ManaPercent > 10)
            {
                card = "Gold";
            }
            if (target.CountEnemyHeros(300) > 1 && user.ManaPercent > 10 && user.PredictHealthPercent() > 40)
            {
                card = "Red";
            }
            if (user.CountEnemyHeros(Config.SafeValue) <= 1 && user.ManaPercent < 30 && user.PredictHealthPercent() > 50)
            {
                card = "Blue";
            }
            if (target is AIHeroClient && target.UnderTurret())
            {
                card = "Gold";
            }
            StartSelecting(card);
        }

        private static void StartSelecting(string str)
        {
            if (W.IsReady() && !Selecting && Core.GameTickCount - lastcasted > 300)
            {
                W.Cast();
                lastcasted = Core.GameTickCount;
            }

            if (!Selecting)
                return;

            if (str.Equals("Gold") && W.Name.Equals("GoldCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                W.Cast();
            }
            if (str.Equals("Red") && W.Name.Equals("RedCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                W.Cast();
            }
            if (str.Equals("Blue") && W.Name.Equals("BlueCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                W.Cast();
            }
        }

        public override void Active()
        {
            if (Core.GameTickCount - lastcasted > 7500)
            {
                Selecting = false;
            }
            if (Selecting && ModesManager.None)
            {
                var target = TargetSelector.GetTarget(1000, DamageType.Magical);
                if (target != null)
                {
                    SetectCard(target);
                }
            }
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(Q.Slot))
            {
                Q.CastAOE(1, Q.Range, target);
            }
            if (ComboMenu.CheckBoxValue(W.Slot) && target.IsKillable(W.Range))
            {
                SetectCard(target);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.CastAOE(1, Q.Range, target);
            }
            if (HarassMenu.CheckBoxValue(W.Slot) && target.IsKillable(W.Range))
            {
                SetectCard(target);
            }
        }

        public override void LaneClear()
        {
            var linefarmloc = Q.SetSkillshot().GetBestLinearCastPosition(Q.LaneMinions());
            if (Q.IsReady() && linefarmloc.HitNumber > 1 && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(linefarmloc.CastPosition);
            }
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null))
            {
                if (LaneClearMenu.CheckBoxValue(W.Slot) && target.IsKillable(W.Range))
                {
                    SetectCard(target);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(m => m != null))
            {
                if (Q.IsReady() && KillStealMenu.CheckBoxValue(Q.Slot) && target.IsKillable(Q.Range) && Q.WillKill(target))
                {
                    Q.CastAOE(1, Q.Range, target);
                }
                if (KillStealMenu.CheckBoxValue(W.Slot) && target.IsKillable(W.Range) && W.WillKill(target))
                {
                    SetectCard(target);
                }
            }
        }
    }
}
