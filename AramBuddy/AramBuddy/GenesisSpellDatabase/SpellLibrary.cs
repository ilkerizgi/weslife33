using System;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using GenesisSpellLibrary.Spells;

namespace GenesisSpellLibrary
{
    internal class SpellLibrary
    {
        public static SpellBase GetSpells(Champion heroChampion)
        {
            var championType = Type.GetType("GenesisSpellLibrary.Spells." + heroChampion);
            if (championType != null)
            {
                return Activator.CreateInstance(championType) as SpellBase;
            }

            else
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send(heroChampion + " is not supported Genesis Spell Library.", Logger.LogLevel.Error);
                return null;
            }
        }

        public static bool IsOnCooldown(AIHeroClient hero, SpellSlot slot)
        {
            if (!hero.Spellbook.GetSpell(slot).IsLearned)
            {
                return true;
            }

            var cooldown = hero.Spellbook.GetSpell(slot).CooldownExpires - Game.Time;
            return cooldown > 0;
        }

        public static void Initialize()
        {
        }
    }
}
