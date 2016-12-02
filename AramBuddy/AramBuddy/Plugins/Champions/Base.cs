using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using GenesisSpellLibrary;
using GenesisSpellLibrary.Spells;

namespace AramBuddy.Plugins.Champions
{
    public abstract class Base
    {
        /// <summary>
        ///     Gets the spells from the database.
        /// </summary>
        protected static SpellBase Spell => SpellManager.CurrentSpells;

        /// <summary>
        ///     List contains my hero spells.
        /// </summary>
        public static readonly List<Spell.SpellBase> SpellList = new List<Spell.SpellBase> { Spell.Q, Spell.W, Spell.E, Spell.R };

        public static Spell.SpellBase Q = Spell.Q;
        public static Spell.SpellBase W = Spell.W;
        public static Spell.SpellBase E = Spell.E;
        public static Spell.SpellBase R = Spell.R;
        public static AIHeroClient user = Player.Instance;
        public static string MenuName = "AB " + user.ChampionName;
        public static Menu MenuIni, AutoMenu, ComboMenu, HarassMenu, LaneClearMenu, KillStealMenu;
        public abstract void Active();
        public abstract void Combo();
        public abstract void Flee();
        public abstract void Harass();
        public abstract void LaneClear();
        public abstract void KillSteal();

        protected Base()
        {
            Game.OnTick += this.Game_OnTick;
        }

        public virtual void Game_OnTick(System.EventArgs args)
        {
            if (user.IsDead || Program.GameEnded)
                return;

            var activemode = Orbwalker.ActiveModesFlags;
            this.KillSteal();
            this.Active();
            switch (activemode)
            {
                case Orbwalker.ActiveModes.Combo:
                    this.Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    this.Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    this.LaneClear();
                    break;
                case Orbwalker.ActiveModes.Flee:
                    this.Flee();
                    break;
            }
        }
    }
}
