using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;


namespace AramBuddy.Plugins.Champions.Leblanc
{
    class Leblanc : Base
    {
        public static Vector3 LastWPosition { get; set; }
        public static Vector3 LastWUltimatePosition { get; set; }
        public static Vector3 LastWEndPosition { get; set; }
        public static Vector3 LastWUltimateEndPosition { get; set; }
        public static Spell.Active WReturn = new Spell.Active(SpellSlot.W);
        public static Spell.Active RReturn = new Spell.Active(SpellSlot.R);
        public static Spell.Targeted QUltimate = new Spell.Targeted(SpellSlot.R, 700);
        public static Spell.Skillshot WUltimate = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 0, 1450, 250);
        public static Spell.Skillshot EUltimate = new Spell.Skillshot(SpellSlot.R, 950, SkillShotType.Linear, 0, 1750, 55) { AllowedCollisionCount = 0 };
        static Leblanc()
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
                    HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
            }
            foreach (var spell in SpellList)
            {
                if (spell != R)
                {
                    ComboMenu.CreateCheckBox("r"+spell.Slot, "Use Ult " + spell.Slot);
                    HarassMenu.CreateCheckBox("r" + spell.Slot, "Use Ult " + spell.Slot,false);
                    LaneClearMenu.CreateCheckBox("r" + spell.Slot, "Use Ult " + spell.Slot,false);
                    KillStealMenu.CreateCheckBox("r" + spell.Slot, "Use Ult " + spell.Slot,false);
                }
            }
            ComboMenu.CreateCheckBox("useReturn", "Use W Return");
            ComboMenu.CreateCheckBox("useReturn2", "Use WR Return", false);
            HarassMenu.CreateCheckBox("useReturn", "Use W Return");
            HarassMenu.CreateCheckBox("useReturn2", "Use WR Return", false);
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

        }



        public override void Active()
        {
            
        }

        public override void Combo()
        {
            if (Player.Instance.Spellbook.GetSpell(SpellSlot.R).Level < 1 || (!ComboMenu.CheckBoxValue("r"+Q.Slot) && !ComboMenu.CheckBoxValue("r" + W.Slot) && ComboMenu.CheckBoxValue("r" + E.Slot)))
            {
                Pre6Combo();
            }
            else
            {
                Post6Combo();
            }

        }

        public override void Flee()
        {
            
        }

        public override void Harass()
        {
            var enemiesBeingE =
                EntityManager.Heroes.Enemies.Where(t => t.IsValidTarget(E.Range) && IsBeingE(t))
                    .ToArray();

            if (!Q.IsLearned)
            {
                if (!enemiesBeingE.Any() && WReturn.IsReady() && HarassMenu.CheckBoxValue("usereturn") &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn")
                {
                    WReturn.Cast();
                }

                var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical, Player.Instance.Position);

                if (wTarget != null && HarassMenu.CheckBoxValue(SpellSlot.W) && !Q.IsLearned && W.IsReady() &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslide")
                {
                    W.Cast(wTarget);
                }

                var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical, Player.Instance.Position);

                if (eTarget != null && HarassMenu.CheckBoxValue(SpellSlot.E) && !Q.IsLearned && E.IsReady())
                {
                    E.Cast(eTarget);
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);

            if (target == null)
            {
                return;
            }

            if (HarassMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady() && Q.IsInRange(target))
            {
                Q.Cast(target);
            }

            if (HarassMenu.CheckBoxValue(SpellSlot.W) && !Q.IsReady() && W.IsReady() &&
                Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslide" &&
                IsMarked(target))
            {
                W.Cast(target);
            }

            if (HarassMenu.CheckBoxValue(SpellSlot.E) &&
                (Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn" ||
                 !W.IsReady()) && E.IsReady() && E.IsInRange(target))
            {
                E.Cast(target);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && Q.WillKill(e)))
            {
                if (KillStealMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (Q.WillKill(target))
                    {
                        Q.Cast(target);
                    }
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable()))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue("r" + SpellSlot.Q) && Player.Instance.Spellbook.GetSpell(SpellSlot.R).Name.ToLower() == "leblancchaosorbm")
                    QUltimate.Cast(target);
            }
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable()))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q))
                    Q.Cast(target);
            }
            foreach (
                var circFarmLoc in
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(1000)) .Select(
                            target =>
                                EntityManager.MinionsAndMonsters.GetCircularFarmLocation(
                                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsKillable(W.Range)),
                                    W.SetSkillshot().Width, (int)W.Range))
                        .Where(
                            circFarmLoc =>
                                W.IsReady() && circFarmLoc.HitNumber > 1 && LaneClearMenu.CheckBoxValue(SpellSlot.W) &&
                                LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent) && Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslide"))
            {
                W.Cast(circFarmLoc.CastPosition);
            }
            if (LaneClearMenu.CheckBoxValue(SpellSlot.W) && WReturn.IsReady() &&
                Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn")
            {
                WReturn.Cast();
            }
            foreach (
               var circFarmLoc in
                      EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(1000)).Select(
                target =>
                    EntityManager.MinionsAndMonsters.GetCircularFarmLocation(
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsKillable(WUltimate.Range)),
                        WUltimate.SetSkillshot().Width, (int)WUltimate.Range))
            .Where(
                circFarmLoc =>
                    WUltimate.IsReady() && circFarmLoc.HitNumber > 1 && LaneClearMenu.CheckBoxValue("r"+SpellSlot.W) 
                    && Player.Instance.Spellbook.GetSpell(SpellSlot.R).Name.ToLower() == "leblancslide"))
            {
                WUltimate.Cast(circFarmLoc.CastPosition);
            }
            if (LaneClearMenu.CheckBoxValue("r"+SpellSlot.W) && RReturn.IsReady() &&
                Player.Instance.Spellbook.GetSpell(SpellSlot.R).Name.ToLower() == "leblancslidereturn")
            {
                RReturn.Cast();
            }
        }
        private static void Pre6Combo()
        {
            var enemiesBeingE =
                EntityManager.Heroes.Enemies.Where(t => t.IsValidTarget(E.Range) && IsBeingE(t))
                    .ToArray();

            if (!Q.IsLearned)
            {
                if (!enemiesBeingE.Any() && WReturn.IsReady() && ComboMenu.CheckBoxValue("usereturn") &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn")
                {
                    WReturn.Cast();
                }

                var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical, Player.Instance.Position);

                if (wTarget != null && ComboMenu.CheckBoxValue(SpellSlot.W) && !Q.IsLearned && W.IsReady() &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslide")
                {
                   W.Cast(wTarget);
                }

                var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical, Player.Instance.Position);

                if (eTarget != null && ComboMenu.CheckBoxValue(SpellSlot.E) && !Q.IsLearned && E.IsReady())
                {
                    E.Cast(eTarget);
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);

            if (target == null)
            {
                return;
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady() && Q.IsInRange(target))
            {
                Q.Cast(target);
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.W) && !Q.IsReady() && W.IsReady() &&
                Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslide" &&
                IsMarked(target))
            {
                W.Cast(target);
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.E) &&
                (Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn" ||
                 !W.IsReady()) && E.IsReady() && E.IsInRange(target))
            {
                E.Cast(target);
            }
        }

        private static void Post6Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);

            if (Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn" &&
                !W.IsReady())
            {
                target = TargetSelector.GetTarget(E.Range, DamageType.Magical, Player.Instance.Position);

                if (target == null)
                {
                    return;
                }

                if (ComboMenu.CheckBoxValue(SpellSlot.E) && E.IsReady() && E.IsInRange(target))
                {
                    E.Cast(target);
                }
            }
            else if (!Q.IsReady() && !QUltimate.IsReady())
            {
                if (target == null)
                {
                    return;
                }

                if (ComboMenu.CheckBoxValue(SpellSlot.W) && W.IsReady() &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslide" &&
                    IsMarked(target))
                {
                    W.Cast(target);
                }
            }
            else
            {
                if (target == null)
                {
                    return;
                }

                if (ComboMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady() && Q.IsInRange(target))
                {
                    Q.Cast(target);
                }

                if (ComboMenu.CheckBoxValue("r"+SpellSlot.Q) && QUltimate.IsReady() && QUltimate.IsInRange(target) &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.R).Name.ToLower() == "leblancchaosorbm")
                {
                    QUltimate.Cast(target);
                }
            }
        }
        public static bool IsBeingE(Obj_AI_Base target)
        {
            return target.HasBuff("LeblancShackleBeam") || target.HasBuff("LeblancShackleBeamM");
        }
        public static bool LogicReturn(bool w2 = false)
        {
            var enemiesBeingE =
                EntityManager.Heroes.Enemies.Where(t => t.IsValidTarget(E.Range) && IsBeingE(t))
                    .ToArray();

            if (enemiesBeingE.Any())
            {
                return false;
            }

            if (!enemiesBeingE.Any() && E.IsReady() && Player.Instance.CountEnemiesInRange(E.Range) > 0)
            {
                return false;
            }

            var enemiesNearLastPosition = LastWPosition.CountEnemiesInRange(Player.Instance.AttackRange);
            var enemiesNearCurrentPosition = Player.Instance.CountEnemiesInRange(Player.Instance.AttackRange);
            var alliesNearLastPosition = LastWPosition.CountAlliesInRange(Player.Instance.AttackRange);
            var alliesNearCurrentPosition = Player.Instance.CountAlliesInRange(Player.Instance.AttackRange);

            if (enemiesNearCurrentPosition < enemiesNearLastPosition ||
                alliesNearCurrentPosition > alliesNearLastPosition ||
                !Player.Instance.IsUnderTurret() && LastWPosition.IsUnderTurret())
            {
                return false;
            }

            if (w2)
            {
                if (RReturn.IsReady() &&
                    Player.Instance.Spellbook.GetSpell(SpellSlot.R).Name.ToLower() != "leblancslidereturnm")
                {
                    RReturn.Cast();
                    return true;
                }
                return false;
            }

            if (WReturn.IsReady() &&
                Player.Instance.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn")
            {
                WReturn.Cast();
                return true;
            }
            return false;
        }
        public static bool IsMarked(Obj_AI_Base target)
        {
            return target.HasBuff("LeblancMarkOfSilence") || target.HasBuff("LeblancMarkOfSilenceM");
        }
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.SData.Name.ToLower() == "leblancslide")
            {
                LastWPosition = args.Start;
                LastWEndPosition = args.End;
            }

            if (args.SData.Name.ToLower() == "leblancslidem")
            {
                LastWUltimatePosition = args.Start;
                LastWUltimateEndPosition = args.End;
            }
        }

    }
}
