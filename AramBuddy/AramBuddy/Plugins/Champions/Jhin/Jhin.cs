using System;
using System.Linq;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace AramBuddy.Plugins.Champions.Jhin
{
    internal class Jhin : Base
    {
        internal static int CurrentRShot;
        private static bool IsCastingR;
        private static Vector3 LastRPosition;

        static Jhin()
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
            AutoMenu.CreateCheckBox("Qunk", "Q UnKillable Minions");
            AutoMenu.CreateCheckBox("AutoW", "Auto W Targets With Buff");
            AutoMenu.CreateCheckBox("WGap", "W Gap Closers");
            AutoMenu.AddGroupLabel("R Settings");
            AutoMenu.CreateCheckBox("R", "Use R");
            AutoMenu.CreateCheckBox("RKS", "R Kill Steal");
            AutoMenu.CreateSlider("RHit", "R HitChance {0}%", 45);

            ComboMenu.CreateCheckBox("WAA", "W If Target is Out Of AA Range");
            ComboMenu.CreateCheckBox("WBUFF", "W Snare Targets Only");

            Player.OnIssueOrder += Player_OnIssueOrder;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }
        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.E)
            {
                if (ObjectManager.Get<Obj_AI_Minion>().Any(o => !o.IsDead && o.IsValid && o.BaseSkinName.Equals("JhinTrap") && o.IsAlly && o.Distance(args.EndPosition) <= 400))
                {
                    args.Process = false;
                }
            }
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (target.IsKillable(Q.Range) && Q.IsReady() && Q.WillKill(target) && AutoMenu.CheckBoxValue("Qunk") && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Q.Cast(target);
            }
        }

        private static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender.IsMe && IsCastingR && EntityManager.Heroes.Enemies.Any(e => e.IsKillable() && JhinRSector(LastRPosition).IsInside(e)))
            {
                args.Process = false;
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsKillable(W.Range) || !W.IsReady() || !HasJhinEBuff(sender)) return;
            if (e.End.IsInRange(user, 600) && AutoMenu.CheckBoxValue("WGap"))
            {
                W.Cast(sender);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name.Equals(FirstR))
                {
                    IsCastingR = true;
                    LastRPosition = args.End;
                }
                if (args.SData.Name.Equals("JhinRShot"))
                {
                    CurrentRShot++;
                }
            }
        }

        public override void Active()
        {
            Orbwalker.DisableMovement = IsCastingR;
            Orbwalker.DisableAttacking = IsCastingR;

            if (!user.Spellbook.IsChanneling && !user.Spellbook.IsCharging && !user.Spellbook.IsCastingSpell)
            {
                IsCastingR = false;
                CurrentRShot = 0;
            }

            var RKillable = EntityManager.Heroes.Enemies.OrderBy(t => t.TotalShieldHealth() / TotalRDamage(t)).FirstOrDefault(e => e != null && TotalRDamage(e) >= e.TotalShieldHealth() && e.IsKillable(R.Range - 150));

            if (!IsCastingR && R.IsReady() && RKillable != null && user.CountEnemyHeros(Config.SafeValue) < 1)
            {
                R.Cast(RKillable);
            }

            if (IsCastingR && AutoMenu.CheckBoxValue("R") && LastRPosition != null)
            {
                var target = EntityManager.Heroes.Enemies.OrderBy(t => t.TotalShieldHealth() / TotalRDamage(t)).FirstOrDefault(e => e != null && e.IsKillable(R.Range) && JhinRSector(LastRPosition).IsInside(e));

                if (target != null && target.IsKillable(R.Range))
                {
                    R.Cast(target, AutoMenu.SliderValue("RHit"));
                }
            }

            if (IsCastingR) return;

            if (AutoMenu.CheckBoxValue("AutoW") && W.IsReady())
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(W.Range) && HasJhinEBuff(e)))
                {
                    W.Cast(target, 45);
                }
            }
        }

        public override void Combo()
        {
            if (IsCastingR || Orbwalker.IsAutoAttacking) return;

            var qtarget = Q.GetTarget();
            var wtarget = W.GetTarget();
            var etarget = E.GetTarget();

            if (qtarget != null && ComboMenu.CheckBoxValue(SpellSlot.Q))
            {
                if (Q.IsReady() && qtarget.IsKillable(Q.Range))
                {
                    Q.Cast(qtarget);
                }
            }
            if (etarget != null && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                if (E.IsReady() && etarget.IsKillable(E.Range) && etarget.IsCC())
                {
                    E.Cast(etarget, HitChance.High);
                }
            }

            if (wtarget != null && ComboMenu.CheckBoxValue(SpellSlot.W) && wtarget.IsKillable(W.Range))
            {
                var useW = ((ComboMenu.CheckBoxValue("WBUFF") && HasJhinEBuff(wtarget)) || !ComboMenu.CheckBoxValue("WBUFF"))
                           && ((!user.IsInAutoAttackRange(wtarget) && ComboMenu.CheckBoxValue("WAA")) || !ComboMenu.CheckBoxValue("WAA"));
                if (useW)
                {
                    W.Cast(wtarget, HitChance.Low);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void Harass()
        {
            if (IsCastingR || Orbwalker.IsAutoAttacking) return;

            var qtarget = Q.GetTarget();
            var wtarget = W.GetTarget();
            var etarget = E.GetTarget();

            if (qtarget != null && HarassMenu.CheckBoxValue(SpellSlot.Q) && HarassMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                if (Q.IsReady() && qtarget.IsKillable(Q.Range))
                {
                    Q.Cast(qtarget);
                }
            }
            if (etarget != null && HarassMenu.CheckBoxValue(SpellSlot.E) && HarassMenu.CompareSlider("Emana", user.ManaPercent))
            {
                if (E.IsReady() && etarget.IsKillable(E.Range) && etarget.IsCC())
                {
                    E.Cast(etarget, HitChance.High);
                }
            }
            if (wtarget != null && HarassMenu.CheckBoxValue(SpellSlot.W) && HarassMenu.CompareSlider("Wmana", user.ManaPercent) && wtarget.IsKillable(W.Range))
            {
                W.Cast(wtarget, HitChance.Low);
            }
        }

        public override void LaneClear()
        {
            if (IsCastingR || Orbwalker.IsAutoAttacking) return;

            if (W.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider("Wmana", user.ManaPercent))
            {
                var minions = W.LaneMinions();
                var farmloc = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, W.SetSkillshot().Width, (int)W.Range);
                if (farmloc.HitNumber >= 2)
                    W.Cast(farmloc.CastPosition);
            }

            if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider("Qmana", user.ManaPercent))
            {
                var qminion = Q.LaneMinions().OrderBy(m => m.PredictHealth() / user.GetSpellDamage(m, SpellSlot.Q)).FirstOrDefault(m => m.CountEnemyMinionsInRangeWithPrediction(450) >= 4);
                if (qminion != null)
                {
                    Q.Cast(qminion);
                }
            }

            if (E.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.E) && LaneClearMenu.CompareSlider("Emana", user.ManaPercent))
            {
                var minions = E.LaneMinions();
                var farmloc = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minions, E.SetSkillshot().Width, (int)E.Range);
                if (farmloc.HitNumber >= 3)
                    E.Cast(farmloc.CastPosition);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(t => t != null))
            {
                if (IsCastingR && R.IsReady() && AutoMenu.CheckBoxValue("RKS"))
                {
                    if (target.IsKillable(R.Range) && CurrentRDamage(target) >= target.TotalShieldHealth() && JhinRSector(LastRPosition).IsInside(target))
                    {
                        R.Cast(target, AutoMenu.SliderValue("RHit"));
                        return;
                    }
                }

                if (IsCastingR) return;

                if (W.IsReady() && KillStealMenu.CheckBoxValue(SpellSlot.W) && W.WillKill(target))
                {
                    W.Cast(target, 50);
                    return;
                }

                if (Q.IsReady() && KillStealMenu.CheckBoxValue(SpellSlot.Q) && Q.WillKill(target))
                {
                    Q.Cast(target);
                    return;
                }
            }
        }
        internal static string FirstR = "JhinR";
        internal static string JhinEBuffName = "JhinESpottedDebuff";

        internal static bool HasJhinEBuff(AIHeroClient target)
        {
            var traveltime = Player.Instance.Distance(target) / W.SetSkillshot().Speed * 1000 + W.CastDelay + Game.Ping;
            var buff = target.GetBuff(JhinEBuffName);
            return buff != null && !target.HasBuffOfType(BuffType.SpellShield) && buff.IsActive && (buff.EndTime - Game.Time) * 1000 >= traveltime;
        }

        private static float JhinRDamage(Obj_AI_Base target)
        {
            if (!R.IsLearned)
                return 0f;
            var index = Jhin.R.Level - 1;
            var MinRDmg = new float[] { 40, 100, 160 }[index];
            var MaxRDmg = new float[] { 140, 350, 560 }[index];

            var MHADP = 1 + (100 - target.PredictHealthPercent()) * 0.025f;

            var mindmg = (MinRDmg + 0.2f * Player.Instance.TotalAttackDamage) * MHADP;
            var maxdmg = MaxRDmg + 0.7f * Player.Instance.TotalAttackDamage;

            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, Math.Min(mindmg, maxdmg));
        }

        private static float FinalJhinRDamage(Obj_AI_Base target)
        {
            return JhinRDamage(target) * 2f * Player.Instance.FlatCritChanceMod;
        }

        private static float TotalRDamage(Obj_AI_Base target)
        {
            return FinalJhinRDamage(target) + JhinRDamage(target) * (3f - Jhin.CurrentRShot);
        }

        private static float CurrentRDamage(Obj_AI_Base target)
        {
            return Jhin.CurrentRShot >= 3 ? FinalJhinRDamage(target) : JhinRDamage(target);
        }

        private static Geometry.Polygon.Sector JhinRSector(Vector3 RCastedPos)
        {
            return new Geometry.Polygon.Sector(Player.Instance.ServerPosition, Player.Instance.ServerPosition.Extend(RCastedPos, Jhin.R.Range).To3D(), (float)(61f * Math.PI / 180), Jhin.R.Range - 175);
        }
    }
}
