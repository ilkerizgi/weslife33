using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;

namespace GenesisSpellLibrary.Spells
{
    public abstract class SpellBase
    {
        public abstract Spell.SpellBase Q { get; set; }

        public abstract Spell.SpellBase W { get; set; }

        public abstract Spell.SpellBase E { get; set; }

        public abstract Spell.SpellBase R { get; set; }

        public Dictionary<string, object> Options;

        public Dictionary<string, Func<AIHeroClient, Obj_AI_Base, bool>> LogicDictionary;

        public bool QDontWaste = false;

        public bool QisCC = false;

        public bool QisDash = false;

        public bool QisDangerDash = false;

        public bool QisToggle = false;

        public bool QisSaver = false;

        public bool QisTP = false;

        public bool WisCC = false;

        public bool WDontWaste = false;

        public bool WisDash = false;

        public bool WisDangerDash = false;

        public bool WisToggle = false;

        public bool WisSaver = false;

        public bool WisTP = false;

        public bool EDontWaste = false;

        public bool EisCC = false;

        public bool EisDash = false;

        public bool EisDangerDash = false;

        public bool EisToggle = false;

        public bool EisSaver = false;

        public bool EisTP = false;

        public bool RDontWaste = false;

        public bool RisCC = false;

        public bool RisDash = false;

        public bool RisDangerDash = false;

        public bool RisToggle = false;

        public bool RisSaver = false;

        public bool RisTP = false;
    }
}
