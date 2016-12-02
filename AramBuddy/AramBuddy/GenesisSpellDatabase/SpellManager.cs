using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using GenesisSpellLibrary.Spells;

namespace GenesisSpellLibrary
{
    public static class SpellManager
    {
        static SpellManager()
        {
            try
            {
                CurrentSpells = SpellLibrary.GetSpells(Player.Instance.Hero);
                SpellsDictionary = new List<SpellBase>();
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on initialization of Genesis SpellManager.", ex, Logger.LogLevel.Error);
            }
        }

        public static SpellBase CurrentSpells { get; set; }

        public static List<SpellBase> SpellsDictionary { get; set; }

        public static float? GetRange(SpellSlot slot, AIHeroClient sender)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (slot)
            {
                case SpellSlot.Q:
                    return spells?.Q.Range;
                case SpellSlot.W:
                    return spells?.W.Range;
                case SpellSlot.E:
                    return spells?.E.Range;
                case SpellSlot.R:
                    return spells?.R.Range;
                default:
                    return 0;
            }
        }

        public static bool DontWaste(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QDontWaste != null && spells.QDontWaste;
                case SpellSlot.W:
                    return spells?.WDontWaste != null && spells.WDontWaste;
                case SpellSlot.E:
                    return spells?.EDontWaste != null && spells.EDontWaste;
                case SpellSlot.R:
                    return spells?.RDontWaste != null && spells.RDontWaste;
                default:
                    return false;
            }
        }

        public static bool IsTP(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QisTP != null && spells.QisTP;
                case SpellSlot.W:
                    return spells?.WisTP != null && spells.WisTP;
                case SpellSlot.E:
                    return spells?.EisTP != null && spells.EisTP;
                case SpellSlot.R:
                    return spells?.RisTP != null && spells.RisTP;
                default:
                    return false;
            }
        }

        public static bool IsCC(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QisCC != null && spells.QisCC;
                case SpellSlot.W:
                    return spells?.WisCC != null && spells.WisCC;
                case SpellSlot.E:
                    return spells?.EisCC != null && spells.EisCC;
                case SpellSlot.R:
                    return spells?.RisCC != null && spells.RisCC;
                default:
                    return false;
            }
        }

        public static bool IsDangerDash(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QisDangerDash != null && spells.QisDangerDash;
                case SpellSlot.W:
                    return spells?.WisDangerDash != null && spells.WisDangerDash;
                case SpellSlot.E:
                    return spells?.EisDangerDash != null && spells.EisDangerDash;
                case SpellSlot.R:
                    return spells?.RisDangerDash != null && spells.RisDangerDash;
                default:
                    return false;
            }
        }

        public static bool IsDash(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QisDash != null && spells.QisDash;
                case SpellSlot.W:
                    return spells?.WisDash != null && spells.WisDash;
                case SpellSlot.E:
                    return spells?.EisDash != null && spells.EisDash;
                case SpellSlot.R:
                    return spells?.RisDash != null && spells.RisDash;
                default:
                    return false;
            }
        }

        public static bool IsToggle(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QisToggle != null && spells.QisToggle;
                case SpellSlot.W:
                    return spells?.WisToggle != null && spells.WisToggle;
                case SpellSlot.E:
                    return spells?.EisToggle != null && spells.EisToggle;
                case SpellSlot.R:
                    return spells?.RisToggle != null && spells.RisToggle;
                default:
                    return false;
            }
        }

        public static bool IsSaver(this Spell.SpellBase spell)
        {
            var spells = SpellsDictionary.FirstOrDefault();
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return spells?.QisSaver != null && spells.QisSaver;
                case SpellSlot.W:
                    return spells?.WisSaver != null && spells.WisSaver;
                case SpellSlot.E:
                    return spells?.EisSaver != null && spells.EisSaver;
                case SpellSlot.R:
                    return spells?.RisSaver != null && spells.RisSaver;
                default:
                    return false;
            }
        }

        public static void Initialize()
        {
            try
            {
                PrepareSpells(Player.Instance);
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on PrepareSpells of Genesis SpellManager.", ex, Logger.LogLevel.Error);
            }
        }

        public static void PrepareSpells(AIHeroClient hero)
        {
            try
            {
                var spells = SpellLibrary.GetSpells(hero.Hero);
                //This only needs to be called once per champion, anymore is a memory leak.
                if (spells != null)
                {
                    SpellsDictionary.Add(spells);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send($"{Player.Instance.ChampionName} Is not Added to the Database yet.", ex, Logger.LogLevel.Error);
            }
        }
    }
}
