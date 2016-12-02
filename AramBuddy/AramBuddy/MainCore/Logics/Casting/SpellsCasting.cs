using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using GenesisSpellLibrary;

namespace AramBuddy.MainCore.Logics.Casting
{
    internal class SpellsCasting
    {
        /// <summary>
        ///     Casting Logic.
        /// </summary>
        public static void Casting(Spell.SpellBase spellBase, Obj_AI_Base target, bool enabled = true)
        {
            if (spellBase == null || target == null || !enabled)
                return;

            if(spellBase.DontWaste() && ModesManager.CurrentMode == ModesManager.Modes.LaneClear || ModesManager.CurrentMode == ModesManager.Modes.Harass)
                return;

            if (spellBase.IsDangerDash() && target.CountAllyHeros(1000) >= target.CountEnemyHeros(1000) && ModesManager.CurrentMode != ModesManager.Modes.LaneClear
                && (target.PredictPosition().UnderEnemyTurret() && Misc.SafeToDive || !target.PredictPosition().UnderEnemyTurret()))
            {
                if(target.Position.IsSafe() && target.Position.SafeDive())
                    spellBase.Cast(target);
                return;
            }

            if (spellBase.IsDangerDash())
                return;

            if (spellBase.IsDash())
            {
                if (target.Distance(Player.Instance) > 400 && Player.Instance.PredictHealthPercent() > 50)
                {
                    var chargeable = spellBase as Spell.Chargeable;
                    if (chargeable != null)
                    {
                        if (!chargeable.IsCharging)
                        {
                            if(target.Position.IsSafe() && target.Position.SafeDive())
                                chargeable.StartCharging();
                            return;
                        }
                        if (chargeable.IsInRange(target))
                        {
                            if (target.Position.IsSafe() && target.Position.SafeDive())
                                chargeable.Cast(target);
                            return;
                        }
                        return;
                    }
                    var pos = target.PredictPosition().Extend(Player.Instance, 300).To3D();
                    if(pos.IsSafe() && pos.SafeDive())
                        spellBase.Cast(target.PredictPosition().Extend(Player.Instance, 300).To3D());
                }
                else
                {
                    var pos = Player.Instance.ServerPosition.Extend(ObjectsManager.AllySpawn, 300).To3D();
                    if (pos.IsSafe() && pos.SafeDive())
                        spellBase.Cast(pos);
                }
                return;
            }

            if (spellBase.IsToggle())
            {
                if (spellBase is Spell.Active)
                {
                    if (spellBase.Handle.ToggleState != 2 && target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast();
                        return;
                    }
                    if (spellBase.Handle.ToggleState == 2 && !target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast();
                        return;
                    }
                }
                else
                {
                    if (spellBase.Handle.ToggleState != 2 && target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast(target);
                        return;
                    }
                    if (spellBase.Handle.ToggleState == 2 && !target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast(Game.CursorPos);
                        return;
                    }
                }
            }

            if (spellBase is Spell.Active)
            {
                spellBase.Cast();
                return;
            }

            if ((spellBase is Spell.Skillshot || spellBase is Spell.Targeted || spellBase is Spell.Ranged) && !(spellBase is Spell.Chargeable))
            {
                spellBase.Cast(target);
                return;
            }

            if (spellBase is Spell.Chargeable)
            {
                var chargeable = spellBase as Spell.Chargeable;

                if (!chargeable.IsCharging)
                {
                    chargeable.StartCharging();
                    return;
                }
                if (chargeable.IsInRange(target))
                {
                    chargeable.Cast(target);
                }
            }
        }

        /// <summary>
        ///     Anti-Gapcloser Logic.
        /// </summary>
        public static void GapcloserOnOnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (sender == null || !sender.IsEnemy)
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsCC() && s.IsReady() && s.IsInRange(args.End)))
            {
                Casting(spell, sender);
            }
        }

        /// <summary>
        ///     Interrupter Logic.
        /// </summary>
        public static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsCC() && s.IsReady() && s.IsInRange(sender)))
            {
                Casting(spell, sender);
            }
        }

        /// <summary>
        ///     Obj_AI_Base_OnProcessSpellCast event, used to detect incoming spells.
        /// </summary>
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(args.Target is AIHeroClient) || !sender.IsEnemy)
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsSaver() && s.IsReady()))
            {
                var caster = sender;
                var enemy = sender as AIHeroClient;
                var target = (AIHeroClient)args.Target;
                var hit = EntityManager.Heroes.Allies.FirstOrDefault(a => a.IsInRange(args.End, 100) && a.IsValidTarget(spell.Range));

                if (!(caster is AIHeroClient || caster is Obj_AI_Turret) || !caster.IsEnemy || enemy == null)
                {
                    return;
                }

                if (hit != null)
                {
                    var spelldamage = enemy.GetSpellDamage(hit, args.Slot);
                    var damagepercent = (spelldamage / hit.PredictHealth()) * 100;
                    var death = damagepercent >= hit.PredictHealthPercent() || spelldamage >= hit.PredictHealth() || caster.GetAutoAttackDamage(hit, true) >= hit.PredictHealth();

                    if (death || damagepercent >= 40)
                    {
                        Casting(spell, hit);
                    }
                }

                if (target != null && target.IsValidTarget(spell.Range) && target.IsAlly)
                {
                    var spelldamage = enemy.GetSpellDamage(target, args.Slot);
                    var damagepercent = (spelldamage / target.PredictHealth()) * 100;
                    var death = damagepercent >= target.PredictHealthPercent() || spelldamage >= target.PredictHealth() || caster.GetAutoAttackDamage(target, true) >= target.PredictHealth();

                    if (death || damagepercent >= 10)
                    {
                        Casting(spell, target);
                    }
                }
            }
        }

        /// <summary>
        ///     Obj_AI_Base_OnBasicAttack event, used to detect incoming autoattacks.
        /// </summary>
        public static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(args.Target is AIHeroClient))
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsSaver() && s.IsReady()))
            {
                var caster = sender;
                var target = (AIHeroClient)args.Target;

                if (!(caster is AIHeroClient || caster is Obj_AI_Turret) || !caster.IsEnemy || target == null || !target.IsAlly)
                {
                    return;
                }

                var aaprecent = (caster.GetAutoAttackDamage(target, true) / target.PredictHealth()) * 100;
                var death = caster.GetAutoAttackDamage(target, true) >= target.PredictHealth() || aaprecent >= target.PredictHealthPercent();

                if ((death || aaprecent >= 10) && target.IsValidTarget(spell.Range))
                {
                    Casting(spell, target);
                }
            }
        }
    }
}
