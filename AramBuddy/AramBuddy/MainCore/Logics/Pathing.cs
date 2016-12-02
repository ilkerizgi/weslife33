using System;
using System.Linq;
using AramBuddy.MainCore.Logics.Casting;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;
using static AramBuddy.Config;

namespace AramBuddy.MainCore.Logics
{
    internal class Pathing
    {
        /// <summary>
        ///     Bot movements position.
        /// </summary>
        public static Vector3 Position;

        /// <summary>
        ///     Picking best Position to move to.
        /// </summary>
        public static void BestPosition()
        {
            if (EnableTeleport && ObjectsManager.ClosestAlly != null)
            {
                Program.Moveto = "Teleporting";
                Teleport.Cast();
            }
            
            // If player is Zombie moves follow nearest Enemy.
            if (Player.Instance.IsZombie())
            {
                var ZombieTarget = TargetSelector.GetTarget(1000, DamageType.True);
                if (ZombieTarget != null)
                {
                    Program.Moveto = "ZombieTarget";
                    Position = ZombieTarget.PredictPosition();
                    return;
                }
                if (ObjectsManager.NearestEnemy != null)
                {
                    Program.Moveto = "NearestEnemy";
                    Position = ObjectsManager.NearestEnemy.PredictPosition();
                    return;
                }
                if (ObjectsManager.NearestEnemyMinion != null)
                {
                    Program.Moveto = "NearestEnemyMinion";
                    Position = ObjectsManager.NearestEnemyMinion.PredictPosition();
                    return;
                }
            }

            // Feeding Poros
            if (ObjectsManager.ClosesetPoro != null)
            {
                var porosnax = new Item(2052);
                if (porosnax != null && porosnax.IsOwned(Player.Instance) && porosnax.IsReady())
                {
                    porosnax.Cast(ObjectsManager.ClosesetPoro);
                    Logger.Send("Feeding ClosesetPoro");
                }
            }

            // Moves to HealthRelic if the bot needs heal.
            var needHR = Player.Instance.PredictHealthPercent() <= HealthRelicHP || Player.Instance.ManaPercent <= HealthRelicMP && !Player.Instance.IsNoManaHero();
            if (needHR && ObjectsManager.HealthRelic != null)
            {
                var allyneedHR = EntityManager.Heroes.Allies.Any(a => !a.IsMe && Player.Instance.PredictHealth() > a.PredictHealth()
                        && a.Path.LastOrDefault(p => p.IsInRange(ObjectsManager.HealthRelic, a.BoundingRadius + ObjectsManager.HealthRelic.BoundingRadius)) != null);
                if (!allyneedHR && DontStealHR || !DontStealHR)
                {
                    var safeHR = Player.Instance.SafePath(ObjectsManager.HealthRelic) || Player.Instance.Distance(ObjectsManager.HealthRelic) <= 200;
                    if (safeHR)
                    {
                        var formana = Player.Instance.ManaPercent <= HealthRelicMP && !Player.Instance.IsNoManaHero();
                        if (ObjectsManager.HealthRelic.Name.Contains("Bard") && !formana)
                        {
                            Program.Moveto = "BardShrine";
                            Position = ObjectsManager.HealthRelic.Position;
                            return;
                        }
                        Program.Moveto = "HealthRelic";
                        Position = ObjectsManager.HealthRelic.Position;
                        return;
                    }
                }
            }

            // Hunting Bard chimes kappa.
            if (PickBardChimes && ObjectsManager.BardChime != null)
            {
                Program.Moveto = "BardChime";
                Position = ObjectsManager.BardChime.Position.Random();
                return;
            }

            // Pick Thresh Lantern
            if (ObjectsManager.ThreshLantern != null)
            {
                if (Player.Instance.Distance(ObjectsManager.ThreshLantern) > 300)
                {
                    Program.Moveto = "ThreshLantern";
                    Position = ObjectsManager.ThreshLantern.Position.Random();
                }
                else
                {
                    Program.Moveto = "ThreshLantern";
                    Player.UseObject(ObjectsManager.ThreshLantern);
                }
                return;
            }

            if (PickDravenAxe && ObjectsManager.DravenAxe != null)
            {
                Program.Moveto = "DravenAxe";
                Position = ObjectsManager.DravenAxe.Position;
                return;
            }

            if (PickOlafAxe && ObjectsManager.OlafAxe != null)
            {
                Program.Moveto = "OlafAxe";
                Position = ObjectsManager.OlafAxe.Position;
                return;
            }

            if (PickZacBlops && ObjectsManager.ZacBlop != null)
            {
                Program.Moveto = "ZacBlop";
                Position = ObjectsManager.ZacBlop.Position;
                return;
            }
            
            /* fix core pls not working :pepe:
            if (PickCorkiBomb && ObjectsManager.CorkiBomb != null)
            {
                Program.Moveto = "CorkiBomb";
                if (Player.Instance.IsInRange(ObjectsManager.CorkiBomb, 300))
                {
                    Program.Moveto = "UsingCorkiBomb";
                    Player.UseObject(ObjectsManager.CorkiBomb);
                }
                Position = ObjectsManager.CorkiBomb.Position;
                return;
            }*/

            // Moves to the Farthest Ally if the bot has Autsim
            if (Brain.Alone() && ObjectsManager.FarthestAllyToFollow != null && Player.Instance.Distance(ObjectsManager.AllySpawn) <= 3000)
            {
                Program.Moveto = "FarthestAllyToFollow";
                Position = ObjectsManager.FarthestAllyToFollow.PredictPosition().Random();
                return;
            }

            // Stays Under tower if the bot health under 10%.
            if ((ModesManager.CurrentMode == ModesManager.Modes.Flee || (Player.Instance.PredictHealthPercent() < 10 && Player.Instance.CountAllyHeros(SafeValue + 2000) < 3))
                && EntityManager.Heroes.Enemies.Count(e => e.IsValid && !e.IsDead && e.IsInRange(Player.Instance, SafeValue + 200)) > 0)
            {
                if (ObjectsManager.SafeAllyTurret != null)
                {
                    Program.Moveto = "SafeAllyTurretFlee";
                    Position = ObjectsManager.SafeAllyTurret.PredictPosition().Random().Extend(ObjectsManager.AllySpawn.Position.Random(), 400).To3D();
                    return;
                }
                if (ObjectsManager.AllySpawn != null)
                {
                    Program.Moveto = "AllySpawnFlee";
                    Position = ObjectsManager.AllySpawn.Position.Random();
                    return;
                }
            }

            // Moves to AllySpawn if the bot is diving and it's not safe to dive.
            if (((Player.Instance.UnderEnemyTurret() && !SafeToDive) || Core.GameTickCount - Brain.LastTurretAttack < 2000) && ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn2";
                Position = ObjectsManager.AllySpawn.Position.Random();
                return;
            }
            
            if (Player.Instance.GetAutoAttackRange() < 425)
            {
                MeleeLogic();
            }
            else
            {
                RangedLogic();
            }
        }

        /// <summary>
        ///     Melee Champions Logic.
        /// </summary>
        public static bool MeleeLogic()
        {
            // if there is a TeamFight follow NearestEnemy.
            if (Core.GameTickCount - Brain.LastTeamFight < 2000 && Player.Instance.PredictHealthPercent() > 20 && !(ModesManager.CurrentMode == ModesManager.Modes.None || ModesManager.CurrentMode == ModesManager.Modes.Flee)
                && ObjectsManager.NearestEnemy != null && TeamTotal(ObjectsManager.NearestEnemy.PredictPosition()) >= TeamTotal(ObjectsManager.NearestEnemy.PredictPosition(), true)
                && ObjectsManager.NearestEnemy.CountAllyHeros(SafeValue) > 1)
            {
                // if there is a TeamFight move from NearestEnemy to nearestally.
                if (ObjectsManager.SafestAllyToFollow != null)
                {
                    var pos = ObjectsManager.NearestEnemy.KitePos(ObjectsManager.SafestAllyToFollow);
                    if (Player.Instance.SafePath(pos))
                    {
                        Program.Moveto = "NearestEnemyToNearestAlly";
                        Position = pos;
                        return true;
                    }
                }
                // if there is a TeamFight move from NearestEnemy to AllySpawn.
                if (ObjectsManager.AllySpawn != null)
                {
                    var pos = ObjectsManager.NearestEnemy.KitePos(ObjectsManager.AllySpawn);
                    if (Player.Instance.SafePath(pos))
                    {
                        Program.Moveto = "NearestEnemyToAllySpawn";
                        Position = pos;
                        return true;
                    }
                }
            }

            // Tower Defence
            if (Player.Instance.IsUnderHisturret() && ObjectsManager.FarthestAllyTurret != null && Player.Instance.PredictHealthPercent() >= 20
                && !(ModesManager.CurrentMode == ModesManager.Modes.None || ModesManager.CurrentMode == ModesManager.Modes.Flee))
            {
                if (ObjectsManager.FarthestAllyTurret.CountEnemyHeros((int)ObjectsManager.FarthestAllyTurret.GetAutoAttackRange(Player.Instance) + 50) > 0)
                {
                    var enemy = EntityManager.Heroes.Enemies.OrderBy(o => o.Distance(ObjectsManager.AllySpawn)).FirstOrDefault(e => e.IsKillable(3000)
                    && TeamTotal(e.PredictPosition()) >= TeamTotal(e.PredictPosition(), true) && (e.CountAllyHeros(SafeValue) > 1 || e.CountEnemyHeros(SafeValue) < 2) && e.UnderEnemyTurret());
                    if (enemy != null && enemy.UnderEnemyTurret())
                    {
                        Program.Moveto = "EnemyUnderTurret";
                        Position = enemy.KitePos(ObjectsManager.AllySpawn);
                        return true;
                    }
                }
            }

            // if Can AttackObject then start attacking THE DAMN OBJECT FFS.
            if (ObjectsManager.NearestEnemyObject != null && Player.Instance.PredictHealthPercent() > 20 && ModesManager.AttackObject
                && (TeamTotal(ObjectsManager.NearestEnemyObject.Position) > TeamTotal(ObjectsManager.NearestEnemyObject.Position, true)
                || ObjectsManager.NearestEnemyObject.CountEnemyHeros(SafeValue + 100) == 0))
            {
                var extendto = new Vector3();
                if (ObjectsManager.AllySpawn != null)
                {
                    extendto = ObjectsManager.AllySpawn.Position;
                }
                if (ObjectsManager.NearestMinion != null)
                {
                    extendto = ObjectsManager.NearestMinion.Position;
                }
                if (ObjectsManager.NearestAlly != null)
                {
                    extendto = ObjectsManager.NearestAlly.Position;
                }
                var extendtopos = ObjectsManager.NearestEnemyObject.KitePos(extendto);
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, ObjectsManager.NearestEnemyObject.Position, 400);
                var Enemy = EntityManager.Heroes.Enemies.Any(a => a != null && a.IsValid && TeamTotal(a, true) > TeamTotal(a) && !a.IsDead && new Geometry.Polygon.Circle(a.PredictPosition(), a.GetAutoAttackRange(Player.Instance)).Points.Any(p => rect.IsInside(p)));
                if (!Enemy)
                {
                    if (ObjectsManager.EnemyTurret != null)
                    {
                        var TurretCircle = new Geometry.Polygon.Circle(ObjectsManager.EnemyTurret.ServerPosition, ObjectsManager.EnemyTurret.GetAutoAttackRange(Player.Instance));


                        if (ObjectsManager.NearestEnemyObject is Obj_AI_Turret)
                        {
                            if (SafeToDive)
                            {
                                Program.Moveto = "NearestEnemyObject";
                                Position = extendtopos;
                                return true;
                            }
                        }
                        if (!TurretCircle.Points.Any(p => rect.IsInside(p)))
                        {
                            Program.Moveto = "NearestEnemyObject2";
                            Position = extendtopos;
                            return true;
                        }
                    }
                    else
                    {
                        Program.Moveto = "NearestEnemyObject3";
                        Position = extendtopos;
                        return true;
                    }
                }
            }

            // if NearestEnemyMinion exsists moves to NearestEnemyMinion.
            if (ObjectsManager.NearestEnemyMinion != null && ObjectsManager.AllySpawn != null && ModesManager.LaneClear && Player.Instance.PredictHealthPercent() > 25)
            {
                Program.Moveto = "NearestEnemyMinion";
                Position = ObjectsManager.NearestEnemyMinion.PredictPosition().Extend(ObjectsManager.AllySpawn.Position.Random(), KiteDistance(ObjectsManager.NearestEnemyMinion)).To3D();
                return true;
            }

            // if SafestAllyToFollow not exsist picks other to follow.
            if (ObjectsManager.SafestAllyToFollow != null)
            {
                // if SafestAllyToFollow exsist follow BestAllyToFollow.
                Program.Moveto = "SafestAllyToFollow";
                Position = ObjectsManager.SafestAllyToFollow.PredictPosition().Random();
                return true;
            }
            
            // if Minion exsists moves to Minion.
            if (ObjectsManager.AllyMinion != null)
            {
                Program.Moveto = "AllyMinion";
                Position = ObjectsManager.AllyMinion.PredictPosition().Random();
                return true;
            }

            // if FarthestAllyToFollow exsists moves to FarthestAllyToFollow.
            if (ObjectsManager.FarthestAllyToFollow != null)
            {
                Program.Moveto = "FarthestAllyToFollow";
                Position = ObjectsManager.FarthestAllyToFollow.PredictPosition().Random();
                return true;
            }

            // if SecondTurret exsists moves to SecondTurret.
            if (ObjectsManager.SecondTurret != null)
            {
                Program.Moveto = "SecondTurret";
                Position = ObjectsManager.SecondTurret.PredictPosition().Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // if SafeAllyTurret exsists moves to SafeAllyTurret.
            if (ObjectsManager.SafeAllyTurret != null)
            {
                Program.Moveto = "SafeAllyTurret";
                Position = ObjectsManager.SafeAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // if ClosesetAllyTurret exsists moves to ClosesetAllyTurret.
            if (ObjectsManager.ClosesetAllyTurret != null)
            {
                Program.Moveto = "ClosesetAllyTurret";
                Position = ObjectsManager.ClosesetAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // Well if it ends up like this then best thing is to let it end.
            if (ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn3";
                Position = ObjectsManager.AllySpawn.Position.Random();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Ranged Champions Logic.
        /// </summary>
        public static bool RangedLogic()
        {
            // TeamFighting Logic.
            if (Core.GameTickCount - Brain.LastTeamFight < 1500 && Player.Instance.PredictHealthPercent() > 20 && !(ModesManager.CurrentMode == ModesManager.Modes.Flee || ModesManager.CurrentMode == ModesManager.Modes.None)
                && ObjectsManager.NearestEnemy != null && TeamTotal(ObjectsManager.NearestEnemy.PredictPosition()) >= TeamTotal(ObjectsManager.NearestEnemy.PredictPosition(), true) && ObjectsManager.NearestEnemy.CountAllyHeros(SafeValue) > 1)
            {
                // if there is a TeamFight move from NearestEnemy to nearestally.
                if (ObjectsManager.SafestAllyToFollow2 != null)
                {
                    var pos = ObjectsManager.NearestEnemy.KitePos(ObjectsManager.SafestAllyToFollow2);
                    if (Player.Instance.SafePath(pos))
                    {
                        Program.Moveto = "NearestEnemyToNearestAlly";
                        Position = pos;
                        return true;
                    }
                }
                // if there is a TeamFight move from NearestEnemy to AllySpawn.
                if (ObjectsManager.AllySpawn != null)
                {
                    var pos = ObjectsManager.NearestEnemy.KitePos(ObjectsManager.AllySpawn);
                    if (Player.Instance.SafePath(pos))
                    {
                        Program.Moveto = "NearestEnemyToAllySpawn";
                        Position = pos;
                        return true;
                    }
                }
            }

            // Tower Defence
            if (Player.Instance.IsUnderHisturret() && ObjectsManager.FarthestAllyTurret != null && Player.Instance.PredictHealthPercent() >= 20)
            {
                if (ObjectsManager.FarthestAllyTurret.CountEnemyHeros((int)ObjectsManager.FarthestAllyTurret.GetAutoAttackRange() + 50) > 0)
                {
                    var enemy = EntityManager.Heroes.Enemies.OrderBy(o => o.Distance(ObjectsManager.AllySpawn)).FirstOrDefault(e => e.IsKillable(3000)
                    && TeamTotal(e.PredictPosition()) >= TeamTotal(e.PredictPosition(), true) && (e.CountAllyHeros(SafeValue) > 1 || e.CountEnemyHeros(SafeValue) < 2) && e.UnderEnemyTurret());
                    if (enemy != null)
                    {
                        Program.Moveto = "DefendingTower";
                        Position = enemy.KitePos(ObjectsManager.AllySpawn);
                        return true;
                    }
                }
            }

            // if Can AttackObject then start attacking THE DAMN OBJECT FFS.
            if (ObjectsManager.NearestEnemyObject != null && Player.Instance.PredictHealthPercent() > 20 && ModesManager.AttackObject
                && (TeamTotal(ObjectsManager.NearestEnemyObject.Position) > TeamTotal(ObjectsManager.NearestEnemyObject.Position, true) || ObjectsManager.NearestEnemyObject.CountEnemyHeros(SafeValue + 100) < 1))
            {
                var extendto = new Vector3();
                if (ObjectsManager.AllySpawn != null)
                {
                    extendto = ObjectsManager.AllySpawn.Position;
                }
                if (ObjectsManager.NearestMinion != null)
                {
                    extendto = ObjectsManager.NearestMinion.Position;
                }
                if (ObjectsManager.NearestAlly != null)
                {
                    extendto = ObjectsManager.NearestAlly.Position;
                }
                var extendtopos = ObjectsManager.NearestEnemyObject.KitePos(extendto);
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, ObjectsManager.NearestEnemyObject.Position, 400);
                var Enemy = EntityManager.Heroes.Enemies.Any(a => a != null && a.IsValid && TeamTotal(a, true) > TeamTotal(a) && !a.IsDead && new Geometry.Polygon.Circle(a.PredictPosition(), a.GetAutoAttackRange(Player.Instance)).Points.Any(p => rect.IsInside(p)));
                if (!Enemy)
                {
                    if (ObjectsManager.EnemyTurret != null)
                    {
                        var TurretCircle = new Geometry.Polygon.Circle(ObjectsManager.EnemyTurret.ServerPosition, ObjectsManager.EnemyTurret.GetAutoAttackRange(Player.Instance));


                        if (ObjectsManager.NearestEnemyObject is Obj_AI_Turret)
                        {
                            if (SafeToDive)
                            {
                                Program.Moveto = "NearestEnemyObject";
                                Position = extendtopos;
                                return true;
                            }
                        }
                        if (!TurretCircle.Points.Any(p => rect.IsInside(p)))
                        {
                            Program.Moveto = "NearestEnemyObject2";
                            Position = extendtopos;
                            return true;
                        }
                    }
                    else
                    {
                        Program.Moveto = "NearestEnemyObject3";
                        Position = extendtopos;
                        return true;
                    }
                }
            }

            // if NearestEnemyMinion exsists moves to NearestEnemyMinion.
            if (ObjectsManager.NearestEnemyMinion != null && ObjectsManager.AllySpawn != null && ModesManager.LaneClear && Player.Instance.PredictHealthPercent() > 20)
            {
                Program.Moveto = "NearestEnemyMinion";
                Position = ObjectsManager.NearestEnemyMinion.PredictPosition().Extend(ObjectsManager.AllySpawn.Position.Random(), KiteDistance(ObjectsManager.NearestEnemyMinion)).To3D();
                return true;
            }

            // if SafestAllyToFollow2 exsists moves to SafestAllyToFollow2.
            if (ObjectsManager.SafestAllyToFollow2 != null)
            {
                Program.Moveto = "SafestAllyToFollow2";
                Position = ObjectsManager.SafestAllyToFollow2.PredictPosition().Extend(ObjectsManager.AllySpawn, 100).Random();
                return true;
            }
            
            // if Minion not exsist picks other to follow.
            if (ObjectsManager.AllyMinion != null)
            {
                Program.Moveto = "AllyMinion";
                Position = ObjectsManager.AllyMinion.PredictPosition().Extend(ObjectsManager.AllySpawn, 100).Random();
                return true;
            }
            
            // if SecondTurret exsists moves to SecondTurret.
            if (ObjectsManager.SecondTurret != null)
            {
                Program.Moveto = "SecondTurret";
                Position = ObjectsManager.SecondTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 425).To3D().Random();
                return true;
            }

            // if SafeAllyTurret exsists moves to SafeAllyTurret.
            if (ObjectsManager.SafeAllyTurret != null)
            {
                Program.Moveto = "SafeAllyTurret";
                Position = ObjectsManager.SafeAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 425).To3D().Random();
                return true;
            }

            // if ClosesetAllyTurret exsists moves to ClosesetAllyTurret.
            if (ObjectsManager.ClosesetAllyTurret != null)
            {
                Program.Moveto = "ClosesetAllyTurret";
                Position = ObjectsManager.ClosesetAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 425).To3D().Random();
                return true;
            }

            // Well if it ends up like this then best thing is to let it end.
            if (ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn3";
                Position = ObjectsManager.AllySpawn.Position.Random();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Sends movement commands.
        /// </summary>
        public static void MoveTo(Vector3 pos)
        {
            var pos2 = pos;
            var rnd = new Random().Next(750, 2000);
            if (Player.Instance.Distance(pos) > rnd)
            {
                pos2 = Player.Instance.ServerPosition.Extend(pos, rnd).To3D();
            }

            // This to prevent the bot from spamming unnecessary movements.
            var rnddis = new Random().Next(45, 75);
            if ((!Player.Instance.Path.LastOrDefault().IsInRange(pos2, rnddis) || !Player.Instance.IsMoving) && !Player.Instance.IsInRange(pos2, rnddis) /*&& Core.GameTickCount - lastmove >= new Random().Next(100 + Game.Ping, 600 + Game.Ping)*/)
            {
                // This to prevent diving.
                if (pos2.UnderEnemyTurret() && !SafeToDive)
                {
                    //return;
                }
                
                // This to prevent Walking into walls, buildings or traps.
                if ((pos2.IsWall() || pos2.IsBuilding() || ObjectsManager.EnemyTraps.Any(t => t.Trap.IsInRange(pos2, t.Trap.BoundingRadius * 2))) && !Brain.RunningItDownMid)
                {
                    return;
                }

                // Issues Movement Commands.
                Orbwalker.OrbwalkTo(pos2);
            }
        }
    }
}
