using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Utility.MiscUtil
{
    internal class ObjectsManager
    {
        public static void Init()
        {
            // Clears and adds new HealthRelics and bardhealthshrines.
            HealthRelics.Clear();
            foreach (var hr in ObjectManager.Get<GameObject>().Where(o => o != null && o.Name.ToLower().Contains("healthrelic") && o.IsValid && !o.IsDead))
            {
                var validrelic = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)hr.NetworkId)?.PredictHealth() > 0;
                if(validrelic)
                    HealthRelics.Add(hr);
            }

            // Clears and adds new EnemyTraps.
            EnemyTraps.Clear();
            foreach (var trap in ObjectManager.Get<Obj_AI_Minion>().Where(trap => trap.IsEnemy && !trap.IsDead && trap.IsValid))
            {
                if (TrapsNames.Contains(trap.Name))
                {
                    var ttrap = new traps { Trap = trap, IsSpecial = false };
                    EnemyTraps.Add(ttrap);
                } /*
                if (SpecialTrapsNames.Contains(trap.Name))
                {
                    var ttrap = new traps { Trap = trap, IsSpecial = true };
                    EnemyTraps.Add(ttrap);
                }*/
            }

            var lastupdate = 0f;
            Game.OnTick += delegate
                {
                    if (Core.GameTickCount - lastupdate > Misc.ProtectFPS + 200)
                    {
                        foreach (var bardhs in ObjectManager.Get<GameObject>()
                                .Where(o => o.Name.Equals("bardhealthshrine", StringComparison.CurrentCultureIgnoreCase) && o.IsAlly && o.IsValid && !o.IsDead && !HealthRelics.Contains(o)))
                        {
                            HealthRelics.Add(bardhs);
                            Logger.Send("Added " + bardhs.Name);
                        }

                        // Removes HealthRelics and Enemy Traps.
                        HealthRelics.RemoveAll(h => h == null || !h.IsValid || h.IsDead || EntityManager.Heroes.AllHeroes.Any(a => a.IsValidTarget() && a.Distance(h) <= a.BoundingRadius + h.BoundingRadius));
                        EnemyTraps.RemoveAll(t => t.Trap == null || !t.Trap.IsValid || t.Trap.IsDead || EntityManager.Heroes.Allies.Any(a => a.IsValidTarget() && a.Distance(t.Trap) <= a.BoundingRadius + t.Trap.BoundingRadius));
                        lastupdate = Core.GameTickCount;
                        ZacPassives.RemoveAll(p => p.IsDead || !p.IsValid);
                    }
                };

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        /// <summary>
        ///     Checks if healthrelic or traps are created and add them to the list.
        /// </summary>
        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var @base = sender as Obj_AI_Base;
            if (@base != null && @base.BaseSkinName.Equals("OlafAxe") || sender.Name.Contains("Olaf_Base_Q_Axe_Ally"))
            {
                OlafAxeObject = new Objects.OlafAxe(sender);
            }

            if (sender.Name.Contains("Draven_Base_Q_reticle_self"))
            {
                if (!DravenAxes.Any(a => a.Axe.IdEquals(sender)))
                {
                    DravenAxes.Add(new Objects.DravenAxe(sender));
                }
            }

            if (sender.GetType() == typeof(Obj_GeneralParticleEmitter))
            {
                var gameObject = (Obj_GeneralParticleEmitter)sender;
                if (ZacPassiveNames.Contains(gameObject.Name) && !ZacPassives.Contains(gameObject) && Player.Instance.Hero == Champion.Zac)
                {
                    ZacPassives.Add(gameObject);
                    Logger.Send("Create " + gameObject.Name);
                }
            }

            var caster = sender as Obj_AI_Base;
            if (caster != null)
            {
                if (TrapsNames.Contains(sender.Name) && sender.IsEnemy)
                {
                    var trap = new traps { Trap = caster, IsSpecial = false };
                    EnemyTraps.Add(trap);
                    Logger.Send("Create " + sender.Name);
                } /*
                if (SpecialTrapsNames.Contains(caster.Name) && caster.IsEnemy)
                {
                    var trap = new traps { Trap = caster, IsSpecial = true };
                    EnemyTraps.Add(trap);
                    Logger.Send("Create " + sender.Name);
                }*/
            }
            if (sender.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Add(sender);
                Logger.Send("Create " + sender.Name);
            }
        }

        /// <summary>
        ///     Checks if healthrelic or traps are deleted and remove them from the list.
        /// </summary>
        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var @base = sender as Obj_AI_Base;
            if (@base != null && @base.BaseSkinName.Equals("OlafAxe") || sender.Name.Contains("Olaf_Base_Q_Axe_Ally"))
            {
                OlafAxeObject = null;
            }

            if (sender.Name.Contains("Draven_Base_Q_reticle_self"))
            {
                if (DravenAxes.Any(a => a.Axe.IdEquals(sender)))
                {
                    DravenAxes.RemoveAll(a => a.Axe.IdEquals(sender));
                }
            }

            if (sender.GetType() == typeof(Obj_GeneralParticleEmitter))
            {
                var gameObject = (Obj_GeneralParticleEmitter)sender;
                if (ZacPassiveNames.Contains(gameObject.Name) && ZacPassives.Contains(gameObject))
                {
                    ZacPassives.Remove(gameObject);
                    Logger.Send("Delete " + gameObject.Name);
                }
            }

            var caster = sender as Obj_AI_Base;
            if (caster != null)
            {
                var trap = new traps { Trap = caster, IsSpecial = false };
                //var Specialtrap = new traps { Trap = caster, IsSpecial = true };
                if (EnemyTraps.Contains(trap) && trap.Trap.IsEnemy)
                {
                    EnemyTraps.Remove(trap);
                    Logger.Send("Delete " + sender.Name);
                } /*
                if (EnemyTraps.Contains(Specialtrap) && caster.IsEnemy)
                {
                    EnemyTraps.Remove(Specialtrap);
                    Logger.Send("Delete " + sender.Name);
                }*/
            }
            if (sender.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Remove(sender);
                Logger.Send("Delete " + sender.Name);
            }
        }

        /// <summary>
        ///     traps struct.
        /// </summary>
        public struct traps
        {
            public Obj_AI_Base Trap;
            public bool IsSpecial;
        }

        /// <summary>
        ///     Traps Names.
        /// </summary>
        public static List<string> TrapsNames = new List<string> { "Cupcake Trap", "Noxious Trap", "Jack In The Box", "Ziggs_Base_E_placedMine.troy" };

        /// <summary>
        ///     Special Traps Names.
        /// </summary>
        public static List<string> SpecialTrapsNames = new List<string>
        {
            "Fizz_Base_R_OrbitFish.troy", "Gragas_Base_Q_Enemy", "Lux_Base_E_tar_aoe_green.troy", "Soraka_Base_E_rune.troy", "Ziggs_Base_W_aoe_green.troy",
            "Viktor_Catalyst_green.troy", "Viktor_base_W_AUG_green.troy", "Barrel"
        };

        /// <summary>
        ///     BardChimes list.
        /// </summary>
        public static IEnumerable<GameObject> BardChimes
        {
            get
            {
                return ObjectManager.Get<GameObject>().Where(o => o.Name.ToLower().Contains("bardchimeminion") && o.IsAlly && o.IsValid && !o.IsDead);
            }
        }

        /// <summary>
        ///     Zac Passive blops list.
        /// </summary>
        public static List<GameObject> ZacPassives = new List<GameObject>();

        public static string[] ZacPassiveNames = { "Zac_Base_P_Chunk.troy", "Zac_SKN1_Chunk.troy", "Zac_SKN2_Chunk.troy" };

        /// <summary>
        ///     HealthRelics and BardHealthShrines list.
        /// </summary>
        public static List<GameObject> HealthRelics = new List<GameObject>();

        /// <summary>
        ///     EnemyTraps list.
        /// </summary>
        public static List<traps> EnemyTraps = new List<traps>();

        /// <summary>
        ///     Returns Valid HealthRelic and BardHealthShrine.
        /// </summary>
        public static GameObject HealthRelic
        {
            get
            {
                return
                    HealthRelics.OrderBy(e => e.Distance(Player.Instance))
                        .FirstOrDefault(e => e.IsValid && ((e.Distance(Player.Instance) < 3000 && e.Position.IsSafe() && e.CountEnemyHeros(Config.SafeValue) < 1) || (e.Distance(Player.Instance) <= 500)));
            }
        }

        /// <summary>
        ///     Returns Azir Tower.
        /// </summary>
        public static GameObject AzirTower
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(o => o.IsValid && o.Name.ToLower().Contains("towerclicker") && o.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange()) &&
                    o.CountEnemyHeros((int)o.GetAutoAttackRange(Player.Instance)) > 1 && Player.Instance.Hero == Champion.Azir);
            }
        }

        /// <summary>
        ///     Returns ZacBlop.
        /// </summary>
        public static GameObject ZacBlop
        {
            get
            {
                return ZacPassives.FirstOrDefault(o => o.IsValid && !o.IsDead && o.IsInRange(Player.Instance, 600) && o.Position.IsSafe() && Misc.TeamTotal(o.Position) >= Misc.TeamTotal(o.Position, true) && Player.Instance.Hero == Champion.Zac);
            }
        }

        /// <summary>
        ///     Returns Corki Bomb.
        /// </summary>
        public static GameObject CorkiBomb
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(b => b.IsAlly && b.BaseSkinName.Equals("CorkiBombAlly") && b.IsSafe() && b.IsValid && !b.IsDead && Player.Instance.Hero == Champion.Corki);
            }
        }
             
        /// <summary>
        ///     Returns Thresh Lantern.
        /// </summary>
        public static Obj_AI_Base ThreshLantern
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Base>()
                        .FirstOrDefault(
                            l =>
                            l.IsValid && !l.IsDead && Player.Instance.Hero != Champion.Thresh && Player.Instance.PredictHealthPercent() <= 50 && l.Distance(Player.Instance) <= 800
                            && (l.CountEnemyHeros(1000) > 0 && Player.Instance.Distance(l) < 500 || l.CountEnemyHeros(Config.SafeValue) < 1)
                            && l.IsAlly && l.Name.Equals("ThreshLantern") && l.IsSafe());
            }
        }

        /// <summary>
        ///     Returns BardChime.
        /// </summary>
        public static GameObject BardChime
        {
            get
            {
                return
                    BardChimes.OrderBy(c => c.Distance(Player.Instance))
                        .FirstOrDefault(
                            l =>
                            l.IsValid && !l.IsDead && Player.Instance.Hero == Champion.Bard && l.Position.IsSafe() && l.IsAlly && l.Distance(Player.Instance) <= 600
                            && (l.CountEnemyHeros(1000) > 0 && Player.Instance.Distance(l) < 600 || l.CountEnemyHeros(Config.SafeValue) < 1));
            }
        }

        /// <summary>
        ///     Returns Aram Poros.
        /// </summary>
        public static IEnumerable<Obj_AI_Base> AramPoros
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsValid && !o.IsDead && o.BaseSkinName.Equals("HA_AP_Poro"));
            }
        }

        /// <summary>
        ///     Returns Closest Poro.
        /// </summary>
        public static Obj_AI_Base ClosesetPoro
        {
            get
            {
                return AramPoros.OrderBy(o => o.Distance(Player.Instance)).FirstOrDefault(o => o.IsInRange(Player.Instance, 500));
            }
        }

        /// <summary>
        ///     Returns Biggest Poro.
        /// </summary>
        public static Obj_AI_Base BiggestPoro
        {
            get
            {
                return AramPoros.OrderByDescending(o => o.BoundingRadius).FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns DravenAxe.
        /// </summary>
        public static GameObject DravenAxe
        {
            get
            {
                var axe = DravenAxes.Where(a => !a.Axe.IsDead && a.Axe.IsValid && !a.Finished && a.TicksLeft > a.Axe.Distance(Player.Instance) / Player.Instance.MoveSpeed * 1000)
                    .OrderBy(a => Misc.TeamTotal(a.Axe.Position)).FirstOrDefault(a => a.Axe.AlliesMoreThanEnemies() && a.Axe.Position.IsSafe());
                return Player.Instance.Hero == Champion.Draven ? axe?.Axe : null;
            }
        }

        /// <summary>
        ///     OlafAxeObject.
        /// </summary>
        public static Objects.OlafAxe OlafAxeObject { get; set; }

        /// <summary>
        ///     Returns Olaf Axe.
        /// </summary>
        public static GameObject OlafAxe
        {
            get
            {
                if (OlafAxeObject == null || Player.Instance.Hero != Champion.Olaf)
                    return null;

                if (!OlafAxeObject.Finished && OlafAxeObject.Axe.AlliesMoreThanEnemies() && OlafAxeObject.Axe.Position.IsSafe())
                    return OlafAxeObject.Axe;

                    return null;
            }
        }

        public static List<Objects.DravenAxe> DravenAxes = new List<Objects.DravenAxe>();

        /// <summary>
        ///     Returns Nearest Enemy.
        /// </summary>
        public static AIHeroClient NearestEnemy
        {
            get
            {
                return EntityManager.Heroes.Enemies.OrderBy(e => e.Distance(AllySpawn)).ThenByDescending(e => e.CountAllyHeros(Config.SafeValue + 100))
                    .FirstOrDefault(e => e.IsKillable() && e.CountAllyHeros(Config.SafeValue) > 1 && e.IsSafe());
            }
        }

        /// <summary>
        ///     Returns Nearest Ally.
        /// </summary>
        public static AIHeroClient NearestAlly
        {
            get
            {
                return EntityManager.Heroes.Allies.OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault(e => e.IsValidTarget() && !e.IsMe);
            }
        }

        /// <summary>
        ///     Returns a Melee Ally Fighting an Enemy.
        /// </summary>
        public static AIHeroClient MeleeAllyFighting
        {
            get
            {
                AIHeroClient ally = null;
                if (NearestEnemy != null)
                {
                    ally =
                        EntityManager.Heroes.Allies.OrderBy(a => a.Distance(NearestEnemy)).FirstOrDefault(a => a.IsValidTarget() && a.IsAttackPlayer() && !a.IsMe && a.IsMelee && a.PredictHealthPercent() > 15);
                }
                return ally;
            }
        }

        /// <summary>
        ///     Returns Best Allies To Follow.
        /// </summary>
        public static IEnumerable<AIHeroClient> BestAlliesToFollow
        {
            get
            {
                return
                    EntityManager.Heroes.Allies.OrderByDescending(a => a.Distance(AllySpawn))
                        //.ThenByDescending(Misc.KDA)
                        .ThenByDescending(a => Misc.TeamTotal(a.PredictPosition()))
                        .Where(
                            a => !a.IsMe && !a.StackedBots() && (Player.Instance.PredictHealthPercent() > 20 || a.CountEnemyHeros(Config.SafeValue) <= 1) && a.IsSafe() &&
                            a.IsValidTarget() && a.PredictHealthPercent() > 20 && !a.IsInFountainRange() && !a.IsZombie() && a.IsActive());
            }
        }

        /// <summary>
        ///     Returns a Safe Ally To Follow.
        /// </summary>
        public static Obj_AI_Base SafeAllyToFollow
        {
            get
            {
                return (Player.Instance.AttackRange < 400 ? SafeAllyToFollow : SafestAllyToFollow2) ?? NearestMinion;
            }
        }

        /// <summary>
        ///     Returns Farthest Ally To Follow.
        /// </summary>
        public static AIHeroClient FarthestAllyToFollow
        {
            get
            {
                return BestAlliesToFollow.OrderByDescending(a => a.Distance(AllySpawn)).FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns Closets Ally.
        /// </summary>
        public static AIHeroClient ClosestAlly
        {
            get
            {
                return EntityManager.Heroes.Allies.OrderBy(a => a.Distance(AllySpawn)).FirstOrDefault(a => a.Distance(AllySpawn) > 3000
                && !a.IsMe && a.Distance(Player.Instance) > 1750 && a.CountAllyHeros(Config.SafeValue) + 1 >= a.CountEnemyHeros(Config.SafeValue) && a.IsValidTarget());
            }
        }

        /// <summary>
        ///     Returns Best Safest Ally To Follow For Melee.
        /// </summary>
        public static AIHeroClient SafestAllyToFollow
        {
            get
            {
                return BestAlliesToFollow.OrderBy(a => a.Distance(Player.Instance)).ThenByDescending(a => Misc.TeamTotal(a)).FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns Best Safest Ally To Follow For Ranged.
        /// </summary>
        public static AIHeroClient SafestAllyToFollow2
        {
            get
            {
                return BestAlliesToFollow.OrderBy(a => a.DistanceFromAllHeros())//.ThenByDescending(a => Misc.TeamTotal(a.PredictPosition()) - Misc.TeamTotal(a.PredictPosition(), true))
                        .FirstOrDefault(a => a.CountAllyHeros(Config.SafeValue * 1.1f) + 1 >= a.CountEnemyHeros(Config.SafeValue) && a.Distance(Player.Instance) > 100 + Player.Instance.BoundingRadius + a.BoundingRadius);
            }
        }

        /// <summary>
        ///     Returns farthest Ally Minion.
        /// </summary>
        public static Obj_AI_Minion AllyMinion
        {
            get
            {
                return EntityManager.MinionsAndMonsters.AlliedMinions.OrderByDescending(a => a.Distance(AllyNexues))
                        .FirstOrDefault(m => Player.Instance.PredictHealthPercent() > 25 && (m.CountEnemyHeros(Config.SafeValue + 300) < 2
                        || m.AlliesMoreThanEnemies(Config.SafeValue + 300)) &&
                        Misc.TeamTotal(m.PredictPosition()) >= Misc.TeamTotal(m.PredictPosition(), true)
                        && m.IsSafe() && m.IsValidTarget(4500) && !m.IsZombie && m.PredictHealthPercent() > 15);
            }
        }

        /// <summary>
        ///     Returns Nearest Ally Minion.
        /// </summary>
        public static Obj_AI_Minion NearestMinion
        {
            get
            {
                return
                    EntityManager.MinionsAndMonsters.AlliedMinions.OrderBy(a => a.Distance(Player.Instance))
                        .FirstOrDefault(
                            m =>
                            m.AlliesMoreThanEnemies(Config.SafeValue) && m.IsSafe() && m.IsValidTarget(2500)
                            && m.IsValidTarget() && !m.IsZombie && m.PredictHealthPercent() > 25
                            && Misc.TeamTotal(m.PredictPosition()) - Misc.TeamTotal(m.PredictPosition(), true) >= 0);
            }
        }

        /// <summary>
        ///     Returns Nearest Enemy Minion.
        /// </summary>
        public static Obj_AI_Minion NearestEnemyMinion
        {
            get
            {
                if (Orbwalker.SupportMode)
                    return null;

                var lasthitableminion =
                    Orbwalker.PriorityLastHitWaitingMinionsList.OrderBy(m => m.Distance(Player.Instance))
                        .FirstOrDefault(
                            m =>
                            m.IsKillable(Player.Instance.GetAutoAttackRange(m) + 500) && (m.CountEnemyHeros() == 0 || m.AlliesMoreThanEnemies()) && Misc.TeamTotal(m) > Misc.TeamTotal(m, true)
                            && m.CountAllyMinionsInRangeWithPrediction(Config.SafeValue) > 0 && Player.Instance.SafePath(m));

                var lasthitminion = Orbwalker.LastHitMinionsList.OrderBy(m => m.Distance(Player.Instance)).FirstOrDefault(m => m.IsKillable(Player.Instance.GetAutoAttackRange(m) + 500) && (m.CountEnemyHeros() == 0 || m.AlliesMoreThanEnemies())
                && Misc.TeamTotal(m.PredictPosition()) >= Misc.TeamTotal(m.PredictPosition(), true) && m.CountAllyMinionsInRangeWithPrediction(Config.SafeValue) > 0 && Player.Instance.SafePath(m));

                var minionnear =
                    EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Distance(Player.Instance))
                        .FirstOrDefault(m => m.IsKillable() && Misc.TeamTotal(m.PredictPosition()) >= Misc.TeamTotal(m.PredictPosition(), true)
                        && m.CountAllyMinionsInRangeWithPrediction(Config.SafeValue) > 0 && Player.Instance.SafePath(m) && (m.CountEnemyHeros() == 0 || m.AlliesMoreThanEnemies()));

                return lasthitminion ?? lasthitableminion ?? minionnear;
            }
        }

        /// <summary>
        ///     Returns Second Tier Turret.
        /// </summary>
        public static Obj_AI_Turret SecondTurret
        {
            get
            {
                var name = Player.Instance.Team == GameObjectTeam.Order ? "ha_ap_orderturret" : "ha_ap_chaosturret";
                return EntityManager.Turrets.Allies.FirstOrDefault(t => t.IsValidTarget() && t.BaseSkinName.Equals(name, StringComparison.CurrentCultureIgnoreCase)
                && (t.PredictHealthPercent() > 25 && t.CountEnemyHeros(Config.SafeValue) < 3 || t.AlliesMoreThanEnemies(Config.SafeValue)));
            }
        }

        /// <summary>
        ///     Returns Closeset Ally Turret.
        /// </summary>
        public static Obj_AI_Turret ClosesetAllyTurret
        {
            get
            {
                return EntityManager.Turrets.Allies.OrderBy(t => t.Distance(Player.Instance)).FirstOrDefault(t => t.IsValidTarget() && t.AlliesMoreThanEnemies(Config.SafeValue));
            }
        }

        /// <summary>
        ///     Returns Safest Ally Turret.
        /// </summary>
        public static Obj_AI_Turret SafeAllyTurret
        {
            get
            {
                return
                    EntityManager.Turrets.Allies.OrderBy(t => t.Distance(Player.Instance))
                        .FirstOrDefault(t => t.IsValidTarget() && t.CountEnemyHeros(Config.SafeValue) < 2
                        && t.AlliesMoreThanEnemies(Config.SafeValue));
            }
        }

        /// <summary>
        ///     Returns Farthest ally turret from spawn.
        /// </summary>
        public static Obj_AI_Turret FarthestAllyTurret
        {
            get
            {
                return EntityManager.Turrets.Allies.OrderBy(t => t.Distance(AllySpawn)).FirstOrDefault(t => t.IsValidTarget() && t.PredictHealth() > 0 && t.PredictHealthPercent() > 5);
            }
        }

        /// <summary>
        ///     Returns Nearest Object.
        /// </summary>
        public static GameObject NearestEnemyObject
        {
            get
            {
                var list = new List<GameObject>();
                list.Clear();
                if (EnemyNexues != null)
                    list.Add(EnemyNexues);
                if (EnemyInhb != null)
                    list.Add(EnemyInhb);
                if (EnemyTurret != null)
                    list.Add(EnemyTurret);

                return list.OrderBy(o => o.Distance(AllySpawn)).FirstOrDefault(o => o.IsValid && !o.IsDead && o.Position.IsSafe());
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Turret.
        /// </summary>
        public static Obj_AI_Turret EnemyTurret
        {
            get
            {
                return EntityManager.Turrets.Enemies.OrderBy(t => t.Distance(Player.Instance)).FirstOrDefault(t => !t.IsDead && t.IsValid && t.PredictHealth() > 0);
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Turret.
        /// </summary>
        public static Obj_AI_Turret EnemyTurretNearSpawn
        {
            get
            {
                return EntityManager.Turrets.Enemies.OrderBy(t => t.Distance(AllySpawn)).FirstOrDefault(t => !t.IsDead && t.IsValid && t.PredictHealth() > 0);
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Inhbitor.
        /// </summary>
        public static Obj_BarracksDampener EnemyInhb
        {
            get
            {
                return ObjectManager.Get<Obj_BarracksDampener>().FirstOrDefault(i => i.IsEnemy && !i.IsDead && i.Health > 0);
            }
        }

        /// <summary>
        ///     Returns Enemy Nexues.
        /// </summary>
        public static Obj_HQ EnemyNexues
        {
            get
            {
                return ObjectManager.Get<Obj_HQ>().FirstOrDefault(i => i.IsEnemy);
            }
        }

        /// <summary>
        ///     Returns Ally Nexues.
        /// </summary>
        public static Obj_HQ AllyNexues
        {
            get
            {
                return ObjectManager.Get<Obj_HQ>().FirstOrDefault(i => i.IsAlly);
            }
        }

        /// <summary>
        ///     Returns Ally SpawnPoint.
        /// </summary>
        public static Obj_SpawnPoint AllySpawn
        {
            get
            {
                return ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(i => i.IsAlly);
            }
        }

        /// <summary>
        ///     Returns Enemy SpawnPoint.
        /// </summary>
        public static Obj_SpawnPoint EnemySpawn
        {
            get
            {
                return ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(i => i.IsEnemy);
            }
        }
    }
}
