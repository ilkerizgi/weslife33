using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using SharpDX;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;

namespace AramBuddy.Plugins.Champions.Gangplank
{
    using static BarrelsManager;
    internal class Gangplank : Base
    {
        internal static int ConnectionRange = 675;
        
        private static float Rdamage(Obj_AI_Base target)
        {
            return user.HasBuff("GangplankRUpgrade2") ? user.GetSpellDamage(target, SpellSlot.R) * 3F : 0;
        }

        static Gangplank()
        {
            Init();
            
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            //HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            SpellList.ForEach(
                i =>
                {
                    ComboMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                    if (i != R)
                    {
                        //HarassMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                        //HarassMenu.AddSeparator(0);
                        LaneClearMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                        LaneClearMenu.AddSeparator(0);
                        if (i != E)
                        {
                            //HarassMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                            LaneClearMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        }
                    }
                    KillStealMenu.CreateCheckBox(i.Slot, i.Slot + " KillSteal");
                });

            MenuIni.AddGroupLabel("For W CC Cleaner Check Activator > Qss");
            //AutoMenu.CreateCheckBox("CC", "Auto W CC Buffs");
            AutoMenu.CreateCheckBox("AutoQ", "Auto Q Barrels", false);
            AutoMenu.CreateCheckBox("Qunk", "Auto Q UnKillable Minions");

            ComboMenu.CreateCheckBox("FB", "Place First Barrel");
            ComboMenu.CreateSlider("RAOE", "R AoE Hit {0}", 3, 1, 6);

            KillStealMenu.CreateCheckBox("RSwitch", "Use Only Upgrades Damage");
            KillStealMenu.CreateSlider("Rdmg", "Multipy R Damage By X{0}", 3, 1, 12);

            LaneClearMenu.CreateCheckBox("QLH", "LastHit Mode Q");
            LaneClearMenu.CreateSlider("EKill", "Minions Kill Count {0}", 2, 0, 10);
            LaneClearMenu.CreateSlider("EHits", "Minions To Hit With E {0}", 3, 0, 10);

            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (target.IsKillable(Q.Range) && Q.IsReady() && Q.WillKill(target) && AutoMenu.CheckBoxValue("Qunk") && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Q.Cast(target);
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (BarrelsList.All(b => b?.Barrel.NetworkId != args.Target?.NetworkId)) return;

                var target =
                    EntityManager.Heroes.Enemies.FirstOrDefault(e => e.IsKillable() &&
                    BarrelsList.Any(b => b.Barrel.IsValidTarget(Q.Range) && (KillableBarrel(b)?.Distance(e) <= E.SetSkillshot().Width || BarrelsList.Any(a => KillableBarrel(b)?.Distance(a.Barrel) <= ConnectionRange && e.Distance(b.Barrel) <= E.SetSkillshot().Width))))
                    ?? TargetSelector.GetTarget(E.Range, DamageType.Physical);
                var position = Vector3.Zero;
                var startposition = Vector3.Zero;
                if (args.Slot == SpellSlot.Q && E.IsReady())
                {
                    var barrel = BarrelsList.FirstOrDefault(b => b.Barrel.NetworkId == args.Target.NetworkId);
                    var Secondbarrel = BarrelsList.FirstOrDefault(b => b.Barrel.NetworkId != barrel?.Barrel.NetworkId && b.Barrel.Distance(args.Target) <= ConnectionRange);
                    if (barrel != null)
                    {
                        startposition = Secondbarrel?.Barrel.ServerPosition ?? barrel.Barrel.ServerPosition;
                    }
                    if (startposition != Vector3.Zero)
                    {
                        if (target != null && target.IsKillable(E.Range + E.SetSkillshot().Radius))
                        {
                            if (target.Distance(startposition) <= ConnectionRange + E.SetSkillshot().Radius && target.Distance(startposition) > E.SetSkillshot().Width - 75)
                            {
                                var extended = startposition.Extend(E.GetPrediction(target).CastPosition, ConnectionRange).To3D();
                                position = !E.IsInRange(extended) ? E.GetPrediction(target).CastPosition : extended;
                            }
                        }
                        else
                        {
                            target = EntityManager.Heroes.Enemies.OrderBy(e => e.Distance(Game.CursorPos)).FirstOrDefault(e => e.IsKillable(E.Range));
                            if (target != null)
                            {
                                var extended = startposition.Extend(E.GetPrediction(target).CastPosition, ConnectionRange).To3D();
                                position = !E.IsInRange(extended) ? E.GetPrediction(target).CastPosition : extended;
                            }
                        }
                        if (position != Vector3.Zero)
                        {
                            if (BarrelsList.Count(b => b.Barrel.Distance(position) <= E.SetSkillshot().Width) < 1)
                            {
                                E.Cast(position);
                            }
                        }
                    }
                }
            }
        }

        public override void Active()
        {
            if (AutoMenu.CheckBoxValue("AutoQ") && Q.IsReady())
            {
                var target =
                    EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(e => e.IsKillable() &&
                    BarrelsList.Any(b => b.Barrel.IsValidTarget(Q.Range) && (KillableBarrel(b)?.Distance(e) <= E.SetSkillshot().Width || BarrelsList.Any(a => KillableBarrel(b)?.Distance(a.Barrel) <= ConnectionRange && e.Distance(b.Barrel) <= E.SetSkillshot().Width))))
                    ?? TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target == null) return;
                foreach (var A in BarrelsList.OrderBy(b => b.Barrel.Distance(target)))
                {
                    if (KillableBarrel(A) != null && KillableBarrel(A).IsValidTarget(Q.Range))
                    {
                        if (target.IsInRange(KillableBarrel(A), E.SetSkillshot().Width))
                        {
                            Q.Cast(KillableBarrel(A));
                        }

                        var Secondbarrel = BarrelsList.OrderBy(b => b.Barrel.Distance(target)).FirstOrDefault(b => b.Barrel.NetworkId != KillableBarrel(A).NetworkId && b.Barrel.Distance(KillableBarrel(A)) <= ConnectionRange);
                        if (Secondbarrel != null)
                        {
                            if (target.IsInRange(Secondbarrel.Barrel, E.SetSkillshot().Width))
                            {
                                Q.Cast(KillableBarrel(A));
                            }
                            if (BarrelsList.OrderBy(b => b.Barrel.Distance(target)).Any(b => b.Barrel.NetworkId != Secondbarrel.Barrel.NetworkId && b.Barrel.Distance(Secondbarrel.Barrel) <= ConnectionRange && b.Barrel.CountEnemyHeros(E.SetSkillshot().Width) > 0))
                            {
                                Q.Cast(KillableBarrel(A));
                            }
                        }
                    }
                }
            }
        }

        public override void Combo()
        {
            Orbwalker.ForcedTarget = null;
            if (R.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.R))
            {
                R.SetSkillshot().CastAOE(ComboMenu.SliderValue("RAOE"), 3000);
            }

            var target =
                EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(e => e.IsKillable() &&
                BarrelsList.Any(b => b.Barrel.IsValidTarget(Q.Range) && (KillableBarrel(b)?.Distance(e) <= E.SetSkillshot().Width || BarrelsList.Any(a => KillableBarrel(b)?.Distance(a.Barrel) <= ConnectionRange && e.Distance(b.Barrel) <= E.SetSkillshot().Width))))
                ?? TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null || !target.IsKillable()) return;

            var pred = target.PredictPosition();
            var castpos = E.GetPrediction(target).CastPosition;

            if (AABarrel(target) != null)
            {
                var extended = AABarrel(target).ServerPosition.Extend(pred, ConnectionRange).To3D();
                castpos = !E.IsInRange(extended) ? pred : extended;
                Orbwalker.ForcedTarget = AABarrel(target);
                if (E.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.E))
                {
                    if (BarrelsList.Count(b => b.Barrel.Distance(user) <= Q.Range) > 0 && BarrelsList.Count(b => b.Barrel.Distance(castpos) <= E.SetSkillshot().Width) < 1)
                    {
                        E.Cast(castpos);
                    }
                }
                Player.IssueOrder(GameObjectOrder.AttackUnit, AABarrel(target));
                return;
            }

            if (Q.IsReady())
            {
                if (ComboMenu.CheckBoxValue(SpellSlot.Q))
                {
                    if (((BarrelsList.Count(b => b.Barrel.IsInRange(target, E.SetSkillshot().Radius + ConnectionRange)) < 1 && (!E.IsReady() || E.Handle.Ammo < 1)) || Q.WillKill(target)) && target.IsKillable(Q.Range))
                    {
                        Q.Cast(target);
                    }

                    foreach (var A in BarrelsList.OrderBy(b => b.Barrel.Distance(target)))
                    {
                        if (KillableBarrel(A) != null && KillableBarrel(A).IsValidTarget(Q.Range))
                        {
                            if (pred.IsInRange(KillableBarrel(A), E.SetSkillshot().Width))
                            {
                                Q.Cast(KillableBarrel(A));
                            }

                            var Secondbarrel = BarrelsList.OrderBy(b => b.Barrel.Distance(target)).FirstOrDefault(b => b.Barrel.NetworkId != KillableBarrel(A).NetworkId && b.Barrel.Distance(KillableBarrel(A)) <= ConnectionRange);
                            if (Secondbarrel != null)
                            {
                                if (pred.IsInRange(Secondbarrel.Barrel, E.SetSkillshot().Width))
                                {
                                    Q.Cast(KillableBarrel(A));
                                }
                                if (BarrelsList.OrderBy(b => b.Barrel.Distance(target)).Any(b => b.Barrel.NetworkId != Secondbarrel.Barrel.NetworkId && b.Barrel.Distance(Secondbarrel.Barrel) <= ConnectionRange && b.Barrel.CountEnemyHeros(E.SetSkillshot().Width) > 0))
                                {
                                    Q.Cast(KillableBarrel(A));
                                }
                            }
                            else
                            {
                                if (BarrelsList.OrderBy(b => b.Barrel.Distance(target)).Any(b => b.Barrel.NetworkId != KillableBarrel(A).NetworkId && b.Barrel.Distance(KillableBarrel(A)) <= ConnectionRange && b.Barrel.CountEnemyHeros(E.SetSkillshot().Width) > 0))
                                {
                                    Q.Cast(KillableBarrel(A));
                                }
                            }
                        }
                    }
                }
                if (E.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.E))
                {
                    if (BarrelsList.OrderBy(b => b.Barrel.Distance(target)).Count(b => b.Barrel.IsInRange(target, E.SetSkillshot().Width)) < 1)
                    {
                        if (BarrelsList.OrderBy(b => b.Barrel.Distance(target)).Count(b => b.Barrel.IsInRange(target, E.SetSkillshot().Radius + ConnectionRange)) > 0)
                        {
                            var targetbarrel = BarrelsList.OrderBy(b => b.Barrel.Distance(target)).FirstOrDefault(b => KillableBarrel(b) != null && (b.Barrel.IsValidTarget(Q.Range) || b.Barrel.IsValidTarget(user.GetAutoAttackRange())) && b.Barrel.IsInRange(target, E.SetSkillshot().Radius + ConnectionRange));
                            if (KillableBarrel(targetbarrel) != null)
                            {
                                var Secondbarrel = BarrelsList.OrderBy(b => b.Barrel.Distance(target)).FirstOrDefault(b => b.Barrel.NetworkId != targetbarrel?.Barrel.NetworkId && b.Barrel.Distance(targetbarrel?.Barrel) <= ConnectionRange);

                                if (Secondbarrel != null)
                                {
                                    var extended = Secondbarrel.Barrel.ServerPosition.Extend(pred, ConnectionRange).To3D();
                                    castpos = !E.IsInRange(extended) ? pred : extended;
                                }
                                if ((castpos.Distance(KillableBarrel(targetbarrel)) <= ConnectionRange || Secondbarrel?.Barrel.Distance(castpos) <= ConnectionRange) && E.IsInRange(castpos))
                                {
                                    E.Cast(castpos);
                                }
                            }
                        }
                        else
                        {
                            if (E.Handle.Ammo > 1 && ComboMenu.CheckBoxValue("FB"))
                            {
                                if (Q.IsInRange(castpos))
                                {
                                    if (HPTiming() <= 1000 || target.IsCC())
                                    {
                                        E.Cast(castpos);
                                    }
                                }
                                else
                                {
                                    if (E.IsInRange(castpos))
                                    {
                                        E.Cast(castpos.Extend(user, ConnectionRange - 300).To3D());
                                    }
                                }

                                var circle = new Geometry.Polygon.Circle(castpos, ConnectionRange);
                                foreach (var point in circle.Points)
                                {
                                    circle = new Geometry.Polygon.Circle(point, E.SetSkillshot().Width);
                                    var grass = circle.Points.OrderBy(p => p.Distance(castpos)).FirstOrDefault(p => p.IsGrass() && Q.IsInRange(p.To3D()) && p.Distance(castpos) <= ConnectionRange);
                                    if (grass != null)
                                    {
                                        E.Cast(grass.To3D());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Flee()
        {
        }

        public override void Harass()
        {
        }

        public override void LaneClear()
        {
            Orbwalker.ForcedTarget = null;
            if (Q.IsReady())
            {
                if (E.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.E))
                {
                    foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.PredictHealth()).Where(m => m.IsKillable(E.Range)))
                    {
                        var pred = E.GetPrediction(minion);
                        if (EntityManager.MinionsAndMonsters.EnemyMinions.Count(e => e.Distance(pred.CastPosition) <= E.SetSkillshot().Width && BarrelKill(e)) >= LaneClearMenu.SliderValue("EKill"))
                        {
                            if (BarrelsList.Count(b => b.Barrel.IsInRange(pred.CastPosition, E.SetSkillshot().Width)) < 1
                                || (BarrelsList.Count(b => b.Barrel.IsInRange(pred.CastPosition, ConnectionRange)) > 0 && BarrelsList.Count(b => b.Barrel.IsInRange(pred.CastPosition, E.SetSkillshot().Width)) < 1))
                            {
                                E.Cast(pred.CastPosition);
                                return;
                            }
                        }
                    }
                }
                if (LaneClearMenu.CheckBoxValue(SpellSlot.Q))
                {
                    var barrel = BarrelsList.OrderByDescending(b => b.Barrel.CountEnemyMinionsInRangeWithPrediction(E.SetSkillshot().Width)).FirstOrDefault(m => KillableBarrel(m) != null && m.Barrel.CountEnemyMinionsInRangeWithPrediction(E.SetSkillshot().Width) > 0 && (KillableBarrel(m).IsValidTarget(Q.Range) || KillableBarrel(m).IsInRange(user, user.GetAutoAttackRange())));
                    if (barrel != null)
                    {
                        var EkillMinions = EntityManager.MinionsAndMonsters.EnemyMinions.Count(m => BarrelKill(m) && BarrelsList.Any(b => b.Barrel.IsInRange(m, E.SetSkillshot().Width)) && m.IsValidTarget())
                                           >= LaneClearMenu.SliderValue("EKill");
                        var EHitMinions = EntityManager.MinionsAndMonsters.EnemyMinions.Count(m => BarrelsList.Any(b => b.Barrel.IsInRange(m, E.SetSkillshot().Width)) && m.IsValidTarget())
                                           >= LaneClearMenu.SliderValue("EHits");
                        if (KillableBarrel(barrel).IsValidTarget(user.GetAutoAttackRange()))
                        {
                            Orbwalker.ForcedTarget = KillableBarrel(barrel);
                        }
                        else
                        {
                            if (KillableBarrel(barrel).IsValidTarget(Q.Range) && (EkillMinions || EHitMinions))
                            {
                                Q.Cast(barrel.Barrel);
                            }
                        }
                    }
                    else
                    {
                        if (LaneClearMenu.CompareSlider("Qmana", user.ManaPercent))
                        {
                            foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions.OrderByDescending(m => m.Distance(user)).Where(m => m.IsKillable(Q.Range) && Q.WillKill(m) && !BarrelsList.Any(b => b.Barrel.Distance(m) <= E.SetSkillshot().Width)))
                            {
                                Q.Cast(minion);
                            }
                        }
                    }
                }
            }
        }

        public override void KillSteal()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable()))
            {
                if (Q.IsReady() && Q.WillKill(enemy) && enemy.IsKillable(Q.Range) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(enemy);
                }
                if (R.IsReady() && enemy.CountEnemyHeros(1000) >= enemy.CountAllyHeros(1000) && enemy.Distance(user) >= Q.Range + 1000 && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    if (KillStealMenu.CheckBoxValue("RSwitch") && Rdamage(enemy) > 0 ? Rdamage(enemy) >= enemy.TotalShieldHealth() : R.WillKill(enemy, KillStealMenu.SliderValue("Rdmg"), Rdamage(enemy)))
                    {
                        if (KillStealMenu.CheckBoxValue("RSwitch") && Rdamage(enemy) > 0)
                        {
                            Player.CastSpell(SpellSlot.R, enemy.PredictPosition());
                        }
                        else
                        {
                            R.SetSkillshot().CastAOE(1, R.Range);
                        }
                    }
                }
                if (KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    foreach (var a in BarrelsList)
                    {
                        if (BarrelKill(enemy))
                        {
                            if (KillableBarrel(a) != null)
                            {
                                if (KillableBarrel(a)?.Distance(enemy) <= E.SetSkillshot().Width)
                                {
                                    Q.Cast(KillableBarrel(a));
                                }
                                if (BarrelsList.Any(b => b.Barrel.Distance(KillableBarrel(a)) <= ConnectionRange && enemy.Distance(b.Barrel) <= E.SetSkillshot().Width))
                                {
                                    Q.Cast(KillableBarrel(a));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal class BarrelsManager
    {
        internal static readonly List<Barrels> BarrelsList = new List<Barrels>();

        internal static void Init()
        {
            Game.OnTick += Game_OnTick;
        }

        private static float BarrelDamage(Obj_AI_Base target)
        {
            var Elvl = Gangplank.E.Level - 1;
            var floats = new float[] { 0, 0, 0, 0, 0 };
            if (target is AIHeroClient)
            {
                floats = new float[] { 60f, 90f, 120f, 150f, 180f };
            }
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, Player.Instance.TotalAttackDamage + floats[Elvl]);
        }

        internal static bool BarrelKill(AIHeroClient target)
        {
            //Chat.Print(BarrelDamage(target));
            return BarrelDamage(target) >= target.PredictHealth();
        }

        internal static bool BarrelKill(Obj_AI_Base target)
        {
            //Chat.Print(BarrelDamage(target));
            return BarrelDamage(target) >= target.PredictHealth();
        }

        private static void Game_OnTick(EventArgs args)
        {
            foreach (var barrel in ObjectManager.Get<Obj_AI_Minion>().Where(o => o.Buffs.Any(b => b.DisplayName.Equals("GangplankEBarrelActive") && b.Caster.IsMe)))
            {
                if (BarrelsList.All(b => b.Barrel.NetworkId != barrel.NetworkId))
                {
                    var newbarrel = new Barrels(barrel, Core.GameTickCount);
                    BarrelsList.Add(newbarrel);
                }
            }
            BarrelsList.RemoveAll(b => b?.Barrel == null || b.Barrel.IsDead || !b.Barrel.IsValid || b.Barrel.PredictHealth() <= 0);
        }

        internal class Barrels
        {
            public Obj_AI_Minion Barrel;
            public float StartTick;

            public Barrels(Obj_AI_Minion barrel, float tick)
            {
                this.Barrel = barrel;
                this.StartTick = tick;
            }
        }

        internal static Obj_AI_Minion KillableBarrel(Barrels b)
        {
            if (b == null)
            {
                return null;
            }
            if (Prediction.Health.GetPrediction(b.Barrel, (int)QTravelTime(b.Barrel)) < 2)
            {
                return b.Barrel;
            }

            //Chat.Print(Core.GameTickCount - b.StartTick);
            if (!b.Barrel.IsValidTarget(Player.Instance.GetAutoAttackRange() + 25) && b.Barrel.IsValidTarget(Gangplank.Q.Range))
            {
                if (Core.GameTickCount - (b.StartTick + HPTiming() - QTravelTime(b.Barrel)) >= 0)
                {
                    return b.Barrel;
                }
            }
            return null;
        }

        internal static int HPTiming()
        {
            if (Player.Instance.Level < 7)
            {
                return 4000;
            }

            return Player.Instance.Level < 13 ? 2000 : 1000;
        }

        internal static float QTravelTime(Obj_AI_Base Target)
        {
            return Player.Instance.Distance(Target) / (Player.Instance.Crit < 0.05f ? 2600f : 3000f) * 1000 + 250 + Game.Ping / 2f;
        }

        internal static Obj_AI_Minion AABarrel(Obj_AI_Base target)
        {
            foreach (var A in BarrelsList)
            {
                if (KillableBarrel(A) != null && KillableBarrel(A).IsValidTarget(Player.Instance.GetAutoAttackRange()))
                {
                    if (target.IsInRange(KillableBarrel(A), Gangplank.E.SetSkillshot().Width))
                    {
                        return KillableBarrel(A);
                    }

                    var Secondbarrel = BarrelsList.FirstOrDefault(b => b.Barrel.NetworkId != KillableBarrel(A).NetworkId && b.Barrel.Distance(KillableBarrel(A)) <= Gangplank.ConnectionRange && b.Barrel.Distance(target) <= Gangplank.E.SetSkillshot().Width);
                    if (Secondbarrel != null)
                    {
                        return BarrelsList.Any(b => b.Barrel.NetworkId != Secondbarrel.Barrel.NetworkId && b.Barrel.Distance(Secondbarrel.Barrel) <= Gangplank.ConnectionRange && b.Barrel.CountEnemyHeros(Gangplank.E.SetSkillshot().Width) > 0) ? KillableBarrel(A) : KillableBarrel(A);
                    }

                    if (BarrelsList.Any(b => b.Barrel.NetworkId != KillableBarrel(A).NetworkId && b.Barrel.Distance(KillableBarrel(A)) <= Gangplank.ConnectionRange && b.Barrel.CountEnemyHeros(Gangplank.E.SetSkillshot().Width) > 0))
                    {
                        return KillableBarrel(A);
                    }
                }
            }
            return BarrelsList.FirstOrDefault(b => KillableBarrel(b) != null && b.Barrel.IsValidTarget(Player.Instance.GetAutoAttackRange()) && target.IsInRange(KillableBarrel(b), Gangplank.E.SetSkillshot().Width))?.Barrel;
        }
    }
}
