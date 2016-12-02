using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Events;

namespace AramBuddy
{
    /// <summary>
    ///     A class containing all the globally used events in AutoBuddy
    /// </summary>
    internal static class Events
    {
        /// <summary>
        ///     A handler for the OnGameEnd event
        /// </summary>
        /// <param name="win">The arguments the event provides</param>
        public delegate void OnGameEndHandler(bool win);

        /// <summary>
        ///     A handler for the OnGameStart event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnGameStartHandler(EventArgs args);

        /// <summary>
        ///     A handler for the OnSurrenderEnd event
        /// </summary>
        /// <param name="win">The arguments the event provides</param>
        public delegate void OnSurrenderEndHandler(bool win);

        /// <summary>
        ///     A handler for the InComingDamage event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnInComingDamage(InComingDamageEventArgs args);

        public class InComingDamageEventArgs
        {
            public Obj_AI_Base Sender;
            public AIHeroClient Target;
            public float InComingDamage;
            public Type DamageType;

            public enum Type
            {
                TurretAttack,
                HeroAttack,
                MinionAttack,
                SkillShot,
                TargetedSpell
            }

            public InComingDamageEventArgs(Obj_AI_Base sender, AIHeroClient target, float Damage, Type type)
            {
                this.Sender = sender;
                this.Target = target;
                this.InComingDamage = Damage;
                this.DamageType = type;
            }
        }

        static Events()
        {
            // Invoke the OnGameEnd event

            #region OnGameEnd

            // Variable used to make sure that the event invoke isn't spammed and is only called once
            var gameEndNotified = false;

            // Every time the game ticks (1ms)
            Game.OnTick += delegate
                {
                    // Make sure we're not repeating the invoke
                    if (gameEndNotified)
                    {
                        return;
                    }

                    // Gets a dead nexus
                    // and the nexus is dead or its health is equal to 0
                    var nexus = ObjectManager.Get<Obj_HQ>().FirstOrDefault(n => n.Health <= 0 || n.IsDead);

                    // Check and return if the nexus is null
                    if (nexus == null)
                    {
                        return;
                    }
                    
                    // We win if the enemy nexues is dead
                    var win = nexus.IsEnemy;

                    // Invoke the event
                    OnGameEnd?.Invoke(win);

                    // Set gameEndNotified to true, as the event has been completed
                    gameEndNotified = true;

                    Logger.Send("Game finished [Nexus Destroyed]. " + (win ? "Victory!" : ""));
                };

            Chat.OnClientSideMessage += delegate(ChatClientSideMessageEventArgs eventArgs)
                {
                    var win = eventArgs.Message.Contains("Enemy");
                    if (eventArgs.Message.ToLower().Contains(" team agreed to a surrender with ") && !gameEndNotified)
                    {
                        OnSurrenderEnd?.Invoke(win);
                        OnGameEnd?.Invoke(win);
                        gameEndNotified = true;

                        Logger.Send("Game finished [Surrender Vote]." + (win ? "Victory!" : ""));
                    }
                };
            
            #endregion

            // Invoke the OnGameStart event

            #region OnGameStart

            // When the player object is created
            Loading.OnLoadingComplete += delegate
                {
                    if (Player.Instance.IsInShopRange())
                    {
                        //OnGameStart(EventArgs.Empty);

                        Logger.Send("Game started!");
                    }
                };

            #endregion

            #region OnInComingDamageEvent

            Game.OnUpdate += delegate
            {
                // Used to Invoke the Incoming Damage Event When there is SkillShot Incoming
                foreach (var ally in EntityManager.Heroes.Allies.Where(a => a.IsValidTarget()))
                {
                    foreach (var spell in Collision.NewSpells)
                    {
                        if (ally.IsInDanger(spell))
                            InvokeOnIncomingDamage(new InComingDamageEventArgs(spell.Caster, ally, spell.Caster.GetSpellDamage(ally, spell.spell.slot), InComingDamageEventArgs.Type.SkillShot));
                    }
                    foreach (var b in DamageBuffs)
                    {
                        var dmgbuff = ally.Buffs.FirstOrDefault(buff => buff.SourceName.Equals(b.Champion) && buff.Name.Equals(b.BuffName) && buff.IsActive && buff.EndTime - Game.Time < 0.25f);
                        var caster = dmgbuff?.Caster as AIHeroClient;
                        if (caster != null)
                            InvokeOnIncomingDamage(new InComingDamageEventArgs(caster, ally, caster.GetSpellDamage(ally, b.Slot), InComingDamageEventArgs.Type.TargetedSpell));
                    }
                }
            };

            SpellsDetector.OnTargetedSpellDetected += delegate (AIHeroClient sender, AIHeroClient target, GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spell)
            {
                // Used to Invoke the Incoming Damage Event When there is a TargetedSpell Incoming
                if (target.IsAlly)
                    InvokeOnIncomingDamage(new InComingDamageEventArgs(sender, target, sender.GetSpellDamage(target, spell.slot), InComingDamageEventArgs.Type.TargetedSpell));
            };

            Obj_AI_Base.OnBasicAttack += delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                // Used to Invoke the Incoming Damage Event When there is an AutoAttack Incoming
                var target = args.Target as AIHeroClient;
                var hero = sender as AIHeroClient;
                var turret = sender as Obj_AI_Turret;
                var minion = sender as Obj_AI_Minion;

                if (target == null || !target.IsAlly)
                    return;

                if (hero != null)
                    InvokeOnIncomingDamage(new InComingDamageEventArgs(hero, target, hero.GetAutoAttackDamage(target), InComingDamageEventArgs.Type.HeroAttack));
                if (turret != null)
                    InvokeOnIncomingDamage(new InComingDamageEventArgs(turret, target, turret.GetAutoAttackDamage(target), InComingDamageEventArgs.Type.TurretAttack));
                if (minion != null)
                    InvokeOnIncomingDamage(new InComingDamageEventArgs(minion, target, minion.GetAutoAttackDamage(target), InComingDamageEventArgs.Type.MinionAttack));
            };
            Obj_AI_Base.OnProcessSpellCast += delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                var caster = sender as AIHeroClient;
                var target = args.Target as AIHeroClient;
                if (caster == null || target == null || !caster.IsEnemy || !target.IsAlly || args.IsAutoAttack())
                    return;
                if (!Database.TargetedSpells.TargetedSpellsList.Any(s => s.hero == caster.Hero && s.slot == args.Slot))
                {
                    InvokeOnIncomingDamage(new InComingDamageEventArgs(caster, target, caster.GetSpellDamage(target, args.Slot), InComingDamageEventArgs.Type.TargetedSpell));
                }
            };

            #endregion
        }

        /// <summary>
        ///     Fires when the game has ended
        /// </summary>
        public static event OnGameEndHandler OnGameEnd;
        
        /// <summary>
        /// Fires when the game has started
        /// </summary>
        public static event OnGameStartHandler OnGameStart;

        /// <summary>
        ///     Fires when a team Surrender
        /// </summary>
        public static event OnGameEndHandler OnSurrenderEnd;

        /// <summary>
        /// Fires when There is In Coming Damage to an ally
        /// </summary>
        public static event OnInComingDamage OnIncomingDamage;

        private static void InvokeOnIncomingDamage(InComingDamageEventArgs args)
        {
            if (args?.InComingDamage < 1 || args == null)
                return;

            //Logger.Send("OnIcomingDamage: [Sender=" + args.Sender.BaseSkinName + "] [Target=" + args.Target.BaseSkinName + "] [ICD=" + args.InComingDamage.ToString("F1") + "] [DamageType=" + args.DamageType + "]");
            OnIncomingDamage?.Invoke(new InComingDamageEventArgs(args.Sender, args.Target, args.InComingDamage, args.DamageType));
        }

        private static List<DamageBuff> DamageBuffs = new List<DamageBuff>
            {
                new DamageBuff("Karthus", "karthusfallenonetarget", SpellSlot.R),
                new DamageBuff("Tristana", "tristanaechargesound", SpellSlot.E),
                new DamageBuff("Zilean", "ZileanQEnemyBomb", SpellSlot.Q),
            };

        internal class DamageBuff
        {
            public string Champion;
            public string BuffName;
            public SpellSlot Slot;
            public DamageBuff(string Caster, string buffname, SpellSlot slot)
            {
                this.Champion = Caster;
                this.BuffName = buffname;
                this.Slot = slot;
            }
        }
    }
}
