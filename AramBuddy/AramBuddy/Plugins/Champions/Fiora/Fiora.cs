using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace AramBuddy.Plugins.Champions.Fiora
{
    internal class Fiora : Base
    {
        static Fiora()
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
                if (spell != R)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                }
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            //Events.OnIncomingDamage += Events_OnIncomingDamage;
            //SpellsDetector.OnTargetedSpellDetected += SpellsDetector_OnTargetedSpellDetected;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        private static void Events_OnIncomingDamage(Events.InComingDamageEventArgs args)
        {
            if(args.Target == null || !args.Target.IsMe || !W.IsReady() || args.DamageType.Equals(Events.InComingDamageEventArgs.Type.TurretAttack)) return;

            var CastPos = user.Direction.To2D().Perpendicular().To3D();
            
            if (args.Sender != null && args.Sender.IsKillable(W.Range))
            {
                CastPos = W.GetPrediction(args.Sender).CastPosition;
            }
            else
            {
                var target = EntityManager.Heroes.Enemies.OrderBy(a => a.PredictHealth()).FirstOrDefault(e => e != null && e.IsKillable(W.Range));
                if (target != null)
                {
                    CastPos = W.GetPrediction(target).CastPosition;
                }
            }

            var DamagePercent = (args.InComingDamage / user.PredictHealth()) * 100;
            if (args.InComingDamage >= user.PredictHealth() || DamagePercent > 20)
            {
                W.Cast(CastPos);
            }
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, System.EventArgs args)
        {
            if (target == null || !E.IsReady() || !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;

            if (ComboMenu.CheckBoxValue(E.Slot))
                E.Cast();
        }

        private static void SpellsDetector_OnTargetedSpellDetected(Obj_AI_Base sender, Obj_AI_Base target, GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spell)
        {
            if (sender == null || !sender.IsEnemy || !target.IsMe || spell.DangerLevel < 2 || !W.IsReady())
                return;

            var enemy = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (enemy != null && enemy.IsKillable(W.Range))
            {
                W.Cast(enemy);
            }
            else
            {
                W.Cast(user.Direction.To2D().Perpendicular().To3D());
            }
        }

        public override void Active()
        {
            UpdateFioraPassives();
            /*
            foreach (var spell in Collision.NewSpells.Where(s => s.spell.DangerLevel >= 2))
            {
                if (user.IsInDanger(spell) && W.IsReady())
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target != null && target.IsKillable(W.Range))
                    {
                        W.Cast(target);
                    }
                    else
                    {
                        W.Cast(user.Direction.To2D().Perpendicular().To3D());
                    }
                }
            }*/
        }

        private static void UpdateFioraPassives()
        {
            PassiveList.Clear();
            foreach (var vital in FioraVitals)
            {
                var target = EntityManager.Heroes.Enemies.OrderBy(h => h.Distance(vital)).FirstOrDefault();
                if (target != null)
                {
                    var passive = new FioraPassives(target, vital);
                    if (!PassiveList.Contains(passive))
                        PassiveList.Add(new FioraPassives(target, vital));
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
                foreach (var passive in PassiveList.Where(p => p.Caster.Equals(target)))
                {
                    var pos = target.ServerPosition.Extend(VitalPos(passive.Vital), 150).To3D();
                    Q.Cast(pos);
                }
            }
            if (target.PredictHealthPercent() <= 50 && target.IsKillable(R.Range) && R.IsReady() && ComboMenu.CheckBoxValue(R.Slot))
            {
                R.Cast(target);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                foreach (var passive in PassiveList.Where(p => p.Caster != null && p.Vital != null && p.Vital.IsValid && p.Caster.Equals(target)))
                {
                    var pos = target.ServerPosition.Extend(VitalPos(passive.Vital), 100).To3D();
                    Q.Cast(pos);
                }
            }
            if (target.IsKillable(E.Range) && E.IsReady() && HarassMenu.CheckBoxValue(E.Slot) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
            {
                E.Cast();
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target);
                }
                if (target.IsKillable(E.Range) && E.IsReady() && LaneClearMenu.CheckBoxValue(E.Slot) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast();
                }
            }
        }

        public override void Flee()
        {
            if (user.CountEnemyHeros(1000) > 0 && Q.IsReady())
            {
                Q.Cast(user.ServerPosition.Extend(ObjectsManager.AllySpawn.Position.Random(), Q.RangeSquared).To3D());
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(m => m != null))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && Q.WillKill(target) && KillStealMenu.CheckBoxValue(Q.Slot))
                {
                    Q.Cast(target);
                }
            }
        }

        private static IEnumerable<Obj_GeneralParticleEmitter> FioraVitals
        {
            get
            {
                return ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(FioraPassive);
            }
        }

        private static readonly List<FioraPassives> PassiveList = new List<FioraPassives>();

        public static bool FioraPassive(Obj_GeneralParticleEmitter emitter)
        {
            return emitter != null && emitter.IsValid
                   && (emitter.Name.Contains("Fiora_Base_R_Mark") || (emitter.Name.Contains("Fiora_Base_R") && emitter.Name.Contains("Timeout"))
                       || (emitter.Name.Contains("Fiora_Base_Passive") && Directions.Any(emitter.Name.Contains)));
        }

        /// <summary>
        ///     Class for getting Fiora Passives.
        /// </summary>
        private class FioraPassives
        {
            internal readonly AIHeroClient Caster;
            internal readonly Obj_GeneralParticleEmitter Vital;

            public FioraPassives(AIHeroClient from, Obj_GeneralParticleEmitter target)
            {
                this.Caster = from;
                this.Vital = target;
            }
        }

        private static readonly List<string> Directions = new List<string> { "NE", "NW", "SE", "SW" };

        private static Vector3 VitalPos(Obj_GeneralParticleEmitter vital)
        {
            var pos = new Vector3();
            if (vital.Name.Contains("_NE"))
            {
                pos = new Vector3(vital.Position.X, vital.Position.Y + 50, vital.Position.Z);
            }
            if (vital.Name.Contains("_NW"))
            {
                pos = new Vector3(vital.Position.X + 50, vital.Position.Y, vital.Position.Z);
            }
            if (vital.Name.Contains("_SE"))
            {
                pos = new Vector3(vital.Position.X - 50, vital.Position.Y, vital.Position.Z);
            }
            if (vital.Name.Contains("_SW"))
            {
                pos = new Vector3(vital.Position.X, vital.Position.Y + 50, vital.Position.Z);
            }
            return pos;
        }
    }
}
