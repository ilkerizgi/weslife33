using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Logics.Casting
{
    internal class SpecialChamps
    {
        public static bool IsCastingImportantSpell;
        public static float LastStartTick;
        public static bool PluginChecks;

        public class ImportantSpells
        {
            public Champion champ;
            public SpellSlot slot;
            public bool ForceUseStacks;

            public ImportantSpells(Champion Champ, SpellSlot Slot, bool UseStacks)
            {
                this.champ = Champ;
                this.slot = Slot;
                this.ForceUseStacks = UseStacks;
            }
        }
        
        public static List<ImportantSpells> Importantspells = new List<ImportantSpells>
        {
            new ImportantSpells(Champion.Taliyah, SpellSlot.R, false),
            new ImportantSpells(Champion.TahmKench, SpellSlot.R, true),
            new ImportantSpells(Champion.TwistedFate, SpellSlot.R, false),
            new ImportantSpells(Champion.Ryze, SpellSlot.R, false),
            new ImportantSpells(Champion.AurelionSol, SpellSlot.E, false),
            new ImportantSpells(Champion.Jhin, SpellSlot.R, true),
            new ImportantSpells(Champion.Xerath, SpellSlot.R, true),
            new ImportantSpells(Champion.Katarina, SpellSlot.R, false),
            new ImportantSpells(Champion.Velkoz, SpellSlot.R, false),
            new ImportantSpells(Champion.Pantheon, SpellSlot.R, false),
            new ImportantSpells(Champion.Pantheon, SpellSlot.E, false),
            new ImportantSpells(Champion.Janna, SpellSlot.R, false),
            new ImportantSpells(Champion.RekSai, SpellSlot.R, false),
            new ImportantSpells(Champion.Nunu, SpellSlot.R, false),
            new ImportantSpells(Champion.MissFortune, SpellSlot.R, false),
            new ImportantSpells(Champion.Malzahar, SpellSlot.R, false),
            new ImportantSpells(Champion.FiddleSticks, SpellSlot.W, false),
            new ImportantSpells(Champion.FiddleSticks, SpellSlot.R, false),
            new ImportantSpells(Champion.Caitlyn, SpellSlot.R, false),
            new ImportantSpells(Champion.Galio, SpellSlot.R, false)
        };

        public static void Init()
        {
            if (PluginChecks)
                return;

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnTick += Game_OnTick;
            Player.OnIssueOrder += Player_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
                return;

            var me = Importantspells.FirstOrDefault(s => s.champ == Player.Instance.Hero);
            if (me != null)
            {
                if (args.Slot == me.slot)
                {
                    LastStartTick = Core.GameTickCount;
                    IsCastingImportantSpell = true;
                    Logger.Send("[" + Player.Instance.Hero + "] Player Is Channeling Important Spell");
                }
                else
                {
                    if (IsCastingImportantSpell)
                    {
                        args.Process = false;
                        Logger.Send("Blocked " + args.Slot + " - Case Player Channeling Important Spell " + Player.Instance.Hero);
                    }
                }
            }
        }

        private static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (!sender.IsMe || !IsCastingImportantSpell)
                return;

            args.Process = false;
            Logger.Send("Blocked Command - Case Player Channeling Important Spell " + Player.Instance.Hero);
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.DisableAttacking = IsCastingImportantSpell;
            Orbwalker.DisableMovement = IsCastingImportantSpell;
            if (IsCastingImportantSpell)
            {
                if (Player.Instance.Spellbook.IsChanneling && !Config.DisableSpellsCasting && !Program.CustomChamp)
                {
                    if (Importantspells.Any(s => s.champ == Player.Instance.Hero && s.ForceUseStacks))
                    {
                        var target = TargetSelector.GetTarget(3000, DamageType.Mixed);
                        if (target != null)
                        {
                            Player.CastSpell(SpellSlot.R, target.PredictPosition());
                        }
                    }

                    if (Importantspells.Any(s => s.champ == Player.Instance.Hero))
                    {
                        if (Player.Instance.Hero == Champion.Velkoz)
                        {
                            var target = TargetSelector.GetTarget(1750, DamageType.Magical);
                            if (target != null)
                            {
                                Player.UpdateChargeableSpell(SpellSlot.R, target.PredictPosition(), false, false);
                            }
                        }
                        if (Player.Instance.Hero == Champion.TahmKench)
                        {
                            Player.CastSpell(SpellSlot.R);
                        }
                    }
                }

                if (!Player.Instance.Spellbook.IsChanneling && !Player.Instance.Spellbook.IsCharging && !Player.Instance.Spellbook.IsCastingSpell && Core.GameTickCount - LastStartTick > 500 + Game.Ping)
                {
                    IsCastingImportantSpell = false;
                    Logger.Send("No Longer Channeling Important Spell " + Player.Instance.Hero);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (IsCastingImportantSpell || !sender.IsMe)
                return;

            var me = Importantspells.FirstOrDefault(s => s.champ == Player.Instance.Hero);
            if (me != null && me.slot == args.Slot)
            {
                IsCastingImportantSpell = true;
                LastStartTick = Core.GameTickCount;
                Logger.Send("[" + Player.Instance.Hero + "] Player Is Channeling Important Spell");
            }
        }
    }
}
