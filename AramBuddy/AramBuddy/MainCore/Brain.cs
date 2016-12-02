using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Logics;
using AramBuddy.MainCore.Logics.Casting;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using AramBuddy.MainCore.Utility.MiscUtil.Caching;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using GenesisSpellLibrary;
using SharpDX;

namespace AramBuddy.MainCore
{
    internal class Brain
    {
        public static Vector3 LastPickPosition;
        public static bool RunningItDownMid;

        private static int PingUpdates = 1;
        private static int PingsStore = Game.Ping;
        private static int LastPing = Game.Ping;

        public static int NormalPing { get { return PingsStore / PingUpdates; } }

        public static bool Lagging { get { return Game.Ping > NormalPing * 2; } }

        /// <summary>
        ///     Init bot functions.
        /// </summary>
        public static void Init()
        {
            try
            {
                // Initialize Genesis Spell Library.
                SpellManager.Initialize();
                SpellLibrary.Initialize();

                // Initialize The ModesManager
                ModesManager.Init();

                // Initialize ObjectsManager.
                ObjectsManager.Init();

                // Initialize Special Champions Logic.
                SpecialChamps.Init();

                // Initialize Cache.
                Cache.Init();

                // Overrides Orbwalker Movements
                Orbwalker.OverrideOrbwalkPosition += OverrideOrbwalkPosition;

                // Initialize AutoLvlup.
                LvlupSpells.Init();

                // Initialize TeamFights Detector.
                TeamFightsDetection.Init();

                Spellbook.OnCastSpell += delegate (Spellbook sender, SpellbookCastSpellEventArgs args)
                {
                    if (sender.Owner.IsMe && RunningItDownMid)
                    {
                        args.Process = false;
                        Logger.Send("Blocked: " + args.Slot + " Reason: Running It Down Mid");
                    }
                };

                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Gapcloser.OnGapcloser += SpellsCasting.GapcloserOnOnGapcloser;
                Interrupter.OnInterruptableSpell += SpellsCasting.Interrupter_OnInterruptableSpell;
                //Obj_AI_Base.OnBasicAttack += SpellsCasting.Obj_AI_Base_OnBasicAttack;
                //Obj_AI_Base.OnProcessSpellCast += SpellsCasting.Obj_AI_Base_OnProcessSpellCast;
            }
            catch (Exception ex)
            {
                Logger.Send("There was an Error While Initialize Brain", ex, Logger.LogLevel.Error);
            }
        }

        /// <summary>
        ///     Returns LastUpdate for the bot current postion.
        /// </summary>
        public static float LastUpdate;

        /// <summary>
        ///     Returns LastTeamFight Time.
        /// </summary>
        public static float LastTeamFight;

        public static List<AIHeroClient> DetectedBots = new List<AIHeroClient>();

        /// <summary>
        ///     Decisions picking for the bot.
        /// </summary>
        public static void Decisions()
        {
            // Picks best position for the bot.
            if (Core.GameTickCount - LastUpdate > Misc.ProtectFPS)
            {
                // Ticks for the modes manager.
                ModesManager.OnTick();

                Pathing.BestPosition();
                LastPickPosition = Pathing.Position;
                LastUpdate = Core.GameTickCount;
            }
            
            foreach (var hero in EntityManager.Heroes.AllHeroes.Where(a => a.IsValidTarget() && !DetectedBots.Contains(a)))
            {
                if (ObjectsManager.HealthRelics.Any(hr => hero.Path.LastOrDefault().Distance(hr.Position) <= 3))
                {
                    DetectedBots.Add(hero);
                    //Logger.Send("BOT DETECTED: [" + hero.ChampionName + " - " + hero.Name + "] Case: Dead Center click on HR", Logger.LogLevel.Warn);
                }
                if (EntityManager.Heroes.AllHeroes.Count(b => !hero.IdEquals(b) && hero.Distance(b) <= 2) > 0)
                {
                    DetectedBots.Add(hero);
                    //Logger.Send("BOT DETECTED: [" + hero.ChampionName + " - " + hero.Name  + "] Case: Stacked bots", Logger.LogLevel.Warn);
                }
            }

            if (LastPing != Game.Ping)
            {
                PingUpdates++;
                PingsStore += Game.Ping;
                LastPing = Game.Ping;
            }

            if (Misc.TeamFight)
            {
                LastTeamFight = Core.GameTickCount;
            }

            if (Config.FixedKite && !(Program.Moveto.Contains("Enemy") || Program.Moveto.Contains("AllySpawn")) && !(ModesManager.Flee || ModesManager.None)
                && ObjectsManager.NearestEnemy != null && Pathing.Position.CountEnemyHeros((int)(Misc.KiteDistance(ObjectsManager.NearestEnemy) + Player.Instance.BoundingRadius)) > 0)
            {
                Program.Moveto = "FixedToKitingPosition";
                Pathing.Position = ObjectsManager.NearestEnemy.KitePos(ObjectsManager.AllySpawn);
            }

            if (Config.TryFixDive && Pathing.Position.UnderEnemyTurret() && !Misc.SafeToDive)
            {
                Program.Moveto = "FixedToAntiDivePosition";
                for (int i = 0; Pathing.Position.UnderEnemyTurret(); i+= 10)
                {
                    Pathing.Position = LastPickPosition.Extend(ObjectsManager.AllySpawn.Position.Random(), i + Player.Instance.BoundingRadius + 50).To3D();
                }
            }
            if (Config.CreateAzirTower && ObjectsManager.AzirTower != null)
            {
                Program.Moveto = "CreateAzirTower";
                Player.UseObject(ObjectsManager.AzirTower);
            }

            if (Config.EnableHighPing && Game.Ping > 666 && ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "Moving to AllySpawn HIGH PING";
                Pathing.Position = ObjectsManager.AllySpawn.Position.Random();
            }

            RunningItDownMid = Config.Tyler1 && Player.Instance.Gold >= Config.Tyler1g && !Player.Instance.IsZombie() && ObjectsManager.EnemySpawn != null && !AutoShop.Sequences.Buy.FullBuild && Core.GameTickCount - LastTeamFight > 1500
                && (ObjectsManager.AllySpawn != null && Player.Instance.Distance(ObjectsManager.AllySpawn) > 4000 || EntityManager.Heroes.Enemies.Count(e => !e.IsDead && e.IsValid) == 0)
                && EntityManager.Heroes.Allies.Count(a => a.IsActive()) > 2 && (Orbwalker.GetTarget() != null
                && !(Orbwalker.GetTarget().Type == GameObjectType.obj_HQ || Orbwalker.GetTarget().Type == GameObjectType.obj_BarracksDampener) || Orbwalker.GetTarget() == null);

            if (RunningItDownMid && ObjectsManager.EnemySpawn != null)
            {
                Program.Moveto = "RUNNING IT DOWN MID";
                Pathing.Position = ObjectsManager.EnemySpawn.Position.Random();
            }
            
            // Moves to the Bot selected Position.
            if (Pathing.Position.IsValid() && !Pathing.Position.IsZero)
            {
                Pathing.MoveTo(Pathing.Position);
            }
        }

        /// <summary>
        ///     Bool returns true if the bot is alone.
        /// </summary>
        public static bool Alone()
        {
            return Player.Instance.CountAllyHeros(4500) <= 1 || Player.Instance.Path.Any(p => p.IsInRange(Game.CursorPos, 45))
                   || EntityManager.Heroes.Allies.All(a => !a.IsMe && (a.IsInShopRange() || a.IsInFountainRange() || a.IsDead));
        }

        /// <summary>
        ///     Last Turret Attack Time.
        /// </summary>
        public static float LastTurretAttack;

        /// <summary>
        ///     Checks Turret Attacks And saves Heros AutoAttacks.
        /// </summary>
        public static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null)
            {
                if (args.Target.IsMe)
                {
                    if(sender is Obj_AI_Turret)
                        LastTurretAttack = Core.GameTickCount;
                }
                
                var target = args.Target as AIHeroClient;
                if (target != null && target.IsAlly && !target.IsMe)
                {
                    var lastAttack = new Misc.LastAttack(sender, target) { Attacker = sender, LastAttackTick = Core.GameTickCount, Target = target };
                    Misc.AutoAttacks.Add(lastAttack);
                }
            }
        }

        /// <summary>
        ///     Override orbwalker position.
        /// </summary>
        private static Vector3? OverrideOrbwalkPosition()
        {
            return Pathing.Position.Equals(Game.CursorPos) ? ObjectsManager.AllySpawn?.Position.Random() : Pathing.Position;
        }
    }
}
