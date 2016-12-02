#region Credits

//=====================================================================
//+ Massive thanks to the entire community of EB for making this
//+ Spell library possible. Special thanks to: Coman3, MarioGK, 
//+ KarmaPanda, Bloodimir, Hellsing, iRaxe, plebsot, Chaos, 
//+ zpitty and many others!
//+
//+ This spell database was last updated 6/19/2016
//+ Created By Genesis.
//======================================================================

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace GenesisSpellLibrary.Spells
{
    public class Aatrox : SpellBase // Quality Tested, Genesis Approved
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Aatrox()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 650, SkillShotType.Circular, 250, 450, 285) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear, 250, 1200, 100) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Active(SpellSlot.R, 500);
            this.QisCC = true;
            this.QisDash = true;
            this.WisToggle = true;
            this.EisCC = true;
            this.LogicDictionary = new Dictionary<string, Func<AIHeroClient, Obj_AI_Base, bool>>();
            this.LogicDictionary.Add("RLogic", RLogic);
        }

        public static bool RLogic(AIHeroClient player, object _)
        {
            if (player == null)
            {
                return false;
            }
            return EntityManager.Heroes.Enemies.Count(e => e.Distance(player) < 500) >= 1;
        }
    }

    public class Ahri : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ahri()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1750, 100) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 550);
            this.E = new Spell.Skillshot(SpellSlot.E, 950, SkillShotType.Linear, 250, 1550, 60) { AllowedCollisionCount = 0 };
            this.R = new Spell.Active(SpellSlot.R, 600);
            this.EisCC = true;
            this.EDontWaste = true;
            //this.Options.Clear();
            //this.Options.Add("EisCC", true);
            //this.Options.Add("RisDash", true);
        }
    }

    public class Akali : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Akali()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular);
            this.E = new Spell.Active(SpellSlot.E, 325);
            this.R = new Spell.Targeted(SpellSlot.R, 700);
            this.WisCC = true;
            this.RisDash = true;
            this.RisDangerDash = true;
        }
    }

    public class Alistar : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Alistar()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 315);
            this.W = new Spell.Targeted(SpellSlot.W, 625);
            this.E = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Active(SpellSlot.R);
            this.QisCC = true;
            this.WisDash = true;
            this.WisDangerDash = true;
            this.WisCC = true;
            this.EisSaver = true;
            this.LogicDictionary = new Dictionary<string, Func<AIHeroClient, Obj_AI_Base, bool>>();
            this.LogicDictionary.Add("RLogic", RLogic);
        }

        public static bool RLogic(AIHeroClient player, object _)
        {
            if (player == null)
            {
                return false;
            }
            var x = EntityManager.Heroes.Enemies.Count(e => e.Distance(player) < 1000);
            return (player.HasBuffOfType(BuffType.Fear) || player.HasBuffOfType(BuffType.Silence) || player.HasBuffOfType(BuffType.Snare) || player.HasBuffOfType(BuffType.Stun)
                    || player.HasBuffOfType(BuffType.Charm) || player.HasBuffOfType(BuffType.Blind) || player.HasBuffOfType(BuffType.Taunt)) || (x > 3);
        }
    }

    public class Amumu : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Amumu()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 2000, 80) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 300);
            this.E = new Spell.Active(SpellSlot.E, 350);
            this.R = new Spell.Active(SpellSlot.R, 550);
            this.QisDangerDash = true;
            this.QDontWaste = true;
            this.QisCC = true;
            this.QisDash = true;
            this.RisCC = true;
        }
    }

    public class Anivia : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Anivia()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 850, 100) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 0, int.MaxValue, 20);
            this.E = new Spell.Targeted(SpellSlot.E, 650);
            this.R = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 0, int.MaxValue, 200);
            this.QisCC = true;
            this.QDontWaste = true;
        }
    }

    public class Annie : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Annie()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Skillshot(SpellSlot.W, 600, SkillShotType.Cone, 200, int.MaxValue, 80) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Active(SpellSlot.E, 0);
            this.R = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 250, int.MaxValue, 290);
            this.LogicDictionary = new Dictionary<string, Func<AIHeroClient, Obj_AI_Base, bool>>();
            this.LogicDictionary.Add("RLogic", this.RLogic);
        }

        public bool RLogic(AIHeroClient player, Obj_AI_Base target)
        {
            if (player == null)
            {
                return false;
            }
            if (target.CountEnemyHeros(R.SetSkillshot().Width, R.CastDelay) > 1)
            {
                R.Cast(target, 45);
            }
            return target.CountEnemyHeros(R.SetSkillshot().Width, R.CastDelay) > 1;
        }
    }

    public class Ashe : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ashe()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 600, DamageType.Physical);
            this.W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Cone, 250, 1250, 20, DamageType.Physical) { AllowedCollisionCount = 2 };
            this.E = new Spell.Active(SpellSlot.E, 1000);
            this.R = new Spell.Skillshot(SpellSlot.R, 10000, SkillShotType.Linear, 250, 1600, 100, DamageType.Magical) { AllowedCollisionCount = 0 };
            this.RisCC = true;
        }
    }

    public class AurelionSol : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public AurelionSol()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 600, SkillShotType.Circular, 250, 850, 160);
            this.W = new Spell.Active(SpellSlot.W, 600);
            this.E = new Spell.Skillshot(SpellSlot.E, 3000, SkillShotType.Circular);
            this.R = new Spell.Skillshot(SpellSlot.R, 1475, SkillShotType.Linear, 250, 1750, 180);
            this.QisCC = true;
            this.EisTP = true;
            this.RisCC = true;
        }
    }

    public class Azir : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Azir()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 450, SkillShotType.Circular);
            this.E = new Spell.Skillshot(SpellSlot.E, 1200, SkillShotType.Linear, 250, 1600, 100);
            this.R = new Spell.Skillshot(SpellSlot.R, 300, SkillShotType.Linear, 500, 1000, 532) { AllowedCollisionCount = int.MaxValue };
            this.RisCC = true;
            this.EisDash = true;
        }
    }

    public class Bard : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Bard()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 860, SkillShotType.Linear, 250, 1600, 65) { AllowedCollisionCount = 1 };
            //Q2 = new Spell.Skillshot(SpellSlot.Q, 1310, SkillShotType.Linear, 250, 1600, 65);
            this.W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular);
            this.E = new Spell.Skillshot(SpellSlot.E, int.MaxValue, SkillShotType.Linear, 250, 1000, 70);
            this.R = new Spell.Skillshot(SpellSlot.R, 3400, SkillShotType.Circular, 250, int.MaxValue, 650);
            this.QisCC = true;
            this.WisSaver = true;
            this.RisSaver = true;
            this.RisCC = true;
        }
    }

    public class Blitzcrank : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Blitzcrank()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 980, SkillShotType.Linear, 250, 1800, 70) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 0);
            this.E = new Spell.Active(SpellSlot.E, 150);
            this.R = new Spell.Active(SpellSlot.R, 550);
            this.QDontWaste = true;
            this.QisCC = true;
            this.WisSaver = true;
        }
    }

    public class Brand : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Brand()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1600, 120) { AllowedCollisionCount = 0 };
            this.W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 850, int.MaxValue, 250);
            this.E = new Spell.Targeted(SpellSlot.E, 640);
            this.R = new Spell.Targeted(SpellSlot.R, 750);
        }
    }

    public class Braum : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Braum()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1700, 60) { AllowedCollisionCount = 0 };
            this.W = new Spell.Targeted(SpellSlot.W, 650);
            this.E = new Spell.Skillshot(SpellSlot.E, 500, SkillShotType.Cone, 250, 2000, 250);
            this.R = new Spell.Skillshot(SpellSlot.R, 1300, SkillShotType.Linear, 250, 1300, 115) { AllowedCollisionCount = int.MaxValue };
            this.QisCC = true;
            this.WisSaver = true;
            this.EisSaver = true;
            this.RisCC = true;
        }
    }

    public class Caitlyn : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Caitlyn()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, 250, 2000, 60) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, int.MaxValue, 80);
            this.E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, 250, 1600, 80) { AllowedCollisionCount = 0 };
            this.R = new Spell.Targeted(SpellSlot.R, 2000);
            this.EisDash = true;
            this.EDontWaste = true;
        }
    }

    public class Cassiopeia : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Cassiopeia()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, castDelay: 400, spellWidth: 75);
            this.W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, spellWidth: 125);
            this.E = new Spell.Targeted(SpellSlot.E, 700);
            this.R = new Spell.Skillshot(SpellSlot.R, 825, SkillShotType.Cone, spellWidth: 80) { AllowedCollisionCount = int.MaxValue };
            this.RisCC = true;
        }
    }

    public class Chogath : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Chogath()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Circular, 750, int.MaxValue, 175);
            this.W = new Spell.Skillshot(SpellSlot.W, 575, SkillShotType.Cone, 250, 1750, 100);
            this.E = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Targeted(SpellSlot.R, 500);
            this.QisCC = true;
            this.WisCC = true;
        }
    }

    public class Corki : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Corki()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 300, 1000, 250);
            this.W = new Spell.Skillshot(SpellSlot.W, 600, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            //W2 = new Spell.Skillshot(SpellSlot.W, 1800, SkillShotType.Linear, 250, 1000, 70);
            this.E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone);
            this.R = new Spell.Skillshot(SpellSlot.R, 1300, SkillShotType.Linear, 200, 1950, 40) { AllowedCollisionCount = 0 };
            this.EisDash = true;
        }
    }

    public class Darius : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Darius()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 400);
            this.W = new Spell.Active(SpellSlot.W, 145);
            this.E = new Spell.Skillshot(SpellSlot.E, 540, SkillShotType.Cone, 250, 100, 120) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Targeted(SpellSlot.R, 460);
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Diana : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Diana()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 830, SkillShotType.Cone, 500, 1600, 195) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 350);
            this.E = new Spell.Active(SpellSlot.E, 200);
            this.R = new Spell.Targeted(SpellSlot.R, 825);
            this.WisSaver = true;
            this.EisCC = true;
            this.RisDangerDash = true;
        }
    }

    public class DrMundo : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public DrMundo()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 50, 2000, 60) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 162);
            this.E = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Active(SpellSlot.R);
            this.RisSaver = true;
        }
    }

    public class Draven : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Draven()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.WisSaver = true;
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Ekko : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ekko()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 750, SkillShotType.Linear, 250, 2200, 60) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 1620, SkillShotType.Circular, 500, 1000, 500);
            this.E = new Spell.Skillshot(SpellSlot.E, 400, SkillShotType.Linear, 250, int.MaxValue, 1);
            this.R = new Spell.Active(SpellSlot.R, 400);
            this.WisCC = true;
            this.RisSaver = true;
            this.WDontWaste = true;
        }
    }

    public class Elise : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public Elise()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625);
            this.W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular);
            this.E = new Spell.Skillshot(SpellSlot.E, 1075, SkillShotType.Linear, 250, 1600, 80) { AllowedCollisionCount = 0 }; // TODO: Support Elise
            this.R = null;
            this.EDontWaste = true;
            this.EisCC = true;
        }
    }

    public class Evelynn : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Evelynn()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 475);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 225);
            this.R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Circular, 250, 1200, 150);
            this.WisSaver = true;
            this.WDontWaste = true;
        }
    }

    public class Ezreal : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ezreal()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 2000, 60) { AllowedCollisionCount = 0 };
            this.W = new Spell.Skillshot(SpellSlot.W, 1050, SkillShotType.Linear, 250, 1600, 80) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Linear, 250, 2000, 80) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 5000, SkillShotType.Linear, 1000, 2000, 160) { AllowedCollisionCount = int.MaxValue };
            this.EisDash = true;
        }
    }

    public class FiddleSticks : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public FiddleSticks()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 575);
            this.W = new Spell.Targeted(SpellSlot.W, 575);
            this.E = new Spell.Targeted(SpellSlot.E, 750);
            this.R = new Spell.Skillshot(SpellSlot.R, 800, SkillShotType.Circular, 1750, int.MaxValue, 600);
            this.QisCC = true;
            this.EisCC = true;
            this.QDontWaste = true;
        }
    }

    public class Fiora : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Fiora()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 750, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 750, SkillShotType.Linear, 500, 3200, 70) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Active(SpellSlot.E, 200);
            this.R = new Spell.Targeted(SpellSlot.R, 500);
            this.WisCC = true;
            this.WisSaver = true;
            this.WDontWaste = true;
        }
    }

    public class Fizz : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Fizz()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 550);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 400, SkillShotType.Circular, 250, int.MaxValue, 330);
            this.R = new Spell.Skillshot(SpellSlot.R, 1300, SkillShotType.Linear, 250, 1200, 80) { AllowedCollisionCount = 0 };
            this.EisSaver = true;
            this.RisCC = true;
            this.QisDangerDash = true;
        }
    }

    public class Galio : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Galio()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 940, SkillShotType.Circular, 250, 1300, 120);
            this.W = new Spell.Targeted(SpellSlot.W, 830);
            this.E = new Spell.Skillshot(SpellSlot.E, 1180, SkillShotType.Linear, 250, 1200, 140) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Active(SpellSlot.R, 560);
            this.QisCC = true;
            this.WisSaver = true;
            this.RisCC = true;
        }
    }

    public class Gangplank : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Gangplank()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 1150, SkillShotType.Circular, 450, 2000, 390);
            this.R = new Spell.Skillshot(SpellSlot.R, int.MaxValue, SkillShotType.Circular, 250, int.MaxValue, 600);
            this.WisSaver = true;
            this.WDontWaste = true;
            this.RisCC = true;
        }
    }

    public class Garen : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Garen()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E, 300);
            this.R = new Spell.Targeted(SpellSlot.R, 400);
            this.QisCC = true;
            this.WisSaver = true;
        }
    }

    public class Gnar : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public Gnar()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1200, 55) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Circular, 500, int.MaxValue, 150);
            this.R = new Spell.Skillshot(SpellSlot.R, 500, SkillShotType.Linear, 250, 1000, 200) { AllowedCollisionCount = int.MaxValue };
            this.QisCC = true;
            this.EisDash = true;
            this.RisCC = true;
        }
    } // TODO: Same boat as Elise

    public class Gragas : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Gragas()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 775, SkillShotType.Circular, 1, 1000, 110);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 675, SkillShotType.Linear, 0, 1000, 50) { AllowedCollisionCount = 0 };
            this.R = new Spell.Skillshot(SpellSlot.R, 1100, SkillShotType.Circular, 1, 1000, 700);
            this.EisDash = true;
            this.RisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Graves : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Graves()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 0, 3000, 40) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 500, 1500, 120);
            this.E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Linear, 500, 0, 50);
            this.R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 500, 2100, 100) { AllowedCollisionCount = int.MaxValue };
            this.WisCC = true;
            this.EisDash = true;
        }
    }

    public class Hecarim : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Hecarim()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 350);
            this.W = new Spell.Active(SpellSlot.W, 525);
            this.E = new Spell.Active(SpellSlot.E, 450);
            this.R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 800, 200);
        }
    }

    public class Heimerdinger : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Heimerdinger()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 350, SkillShotType.Linear, (int)0.5f, 1450, (int)40f);
            this.W = new Spell.Skillshot(SpellSlot.W, 1325, SkillShotType.Cone, (int)0.5f, 902, 200);
            this.E = new Spell.Skillshot(SpellSlot.E, 970, SkillShotType.Circular, (int)0.5f, 2500, 120);
            this.R = new Spell.Active(SpellSlot.R, 350);
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Illaoi : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Illaoi()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Linear, 750, int.MaxValue, 100) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 950, SkillShotType.Linear, 250, 1900, 50) { AllowedCollisionCount = 0 };
            this.R = new Spell.Active(SpellSlot.R, 450);
        }
    }

    public class Irelia : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Irelia()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 425);
            this.R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Linear, 250, 1600, 120) { AllowedCollisionCount = int.MaxValue };
            this.QisDangerDash = true;
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Ivern : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ivern()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1300, 65, DamageType.Magical) { AllowedCollisionCount = 0};
            W = new Spell.SimpleSkillshot(SpellSlot.W, 1200, DamageType.Magical);
            E = new Spell.Targeted(SpellSlot.E, 750, DamageType.Magical);
            R = new Spell.Targeted(SpellSlot.R, 750, DamageType.Magical);
            this.QisCC = true;
            this.WisSaver = true;
            this.EisSaver = true;
        }
    }

    public class Janna : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Janna()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 300) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Targeted(SpellSlot.W, 600);
            this.E = new Spell.Targeted(SpellSlot.E, 800);
            this.R = new Spell.Active(SpellSlot.R, 725);
            this.QisCC = true;
            this.EisSaver = true;
            this.RisSaver = true;
            this.RisCC = true;
        }
    }

    public class JarvanIV : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public JarvanIV()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 830, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 520);
            this.E = new Spell.Skillshot(SpellSlot.E, 860, SkillShotType.Circular);
            this.R = new Spell.Targeted(SpellSlot.R, 650);
        }
    }

    public class Jax : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Jax()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 700);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E, 187);
            this.R = new Spell.Active(SpellSlot.R);
            this.QisDangerDash = true;
            this.EisCC = true;
            this.EDontWaste = true;
            this.RisSaver = true;
        }
    }

    public class Jayce : SpellBase //todo
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public Jayce()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Active(SpellSlot.W, 285);
            this.E = new Spell.Targeted(SpellSlot.E, 240);
            this.EisCC = true;
        }
    } // TODO: FUCK THERE ARE 3 OF YOU?!

    public class Jhin : SpellBase // todo
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public Jhin()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600) { DamageType = DamageType.Physical };
            this.W = new Spell.Skillshot(SpellSlot.W, 2500, SkillShotType.Linear, 750, 5000, 40) { AllowedCollisionCount = -1, DamageType = DamageType.Physical };
            this.E = new Spell.Skillshot(SpellSlot.E, 750, SkillShotType.Circular, 250, 1600, 300) { DamageType = DamageType.Magical };
            this.R = new Spell.Skillshot(SpellSlot.R, 3500, SkillShotType.Linear, 250, 4500, 80) { AllowedCollisionCount = -1, DamageType = DamageType.Physical };
        }
    } // Just fuck you jhin.

    public class Jinx : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Jinx()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Skillshot(SpellSlot.W, 1450, SkillShotType.Linear, 500, 1500, 60) { AllowedCollisionCount = 0 };
            this.E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 100);
            this.R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 700, 1500, 140) { AllowedCollisionCount = 0 };
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Kalista : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Kalista()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 2100, 60) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 5000, SkillShotType.Circular, 250, 2100, 80);
            this.E = new Spell.Active(SpellSlot.E, 1000);
            this.R = new Spell.Active(SpellSlot.R, 1100); //You are gonna suck until you get logic
            this.RisCC = true;
            this.RisSaver = true;
        }
    }

    public class Karma : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Karma()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 1500, 100) { AllowedCollisionCount = 0 };
            this.W = new Spell.Targeted(SpellSlot.W, 675);
            this.E = new Spell.Targeted(SpellSlot.E, 800);
            this.R = new Spell.Active(SpellSlot.R);
            this.QisCC = true;
            this.WisCC = true;
            this.EisSaver = true;
        }
    }

    public class Karthus : SpellBase //Want to try
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Karthus()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Circular, 1000, int.MaxValue, 160);
            this.W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 500, int.MaxValue, 70);
            this.E = new Spell.Active(SpellSlot.E, 505);
            this.R = new Spell.Skillshot(SpellSlot.R, 25000, SkillShotType.Circular, 3000, int.MaxValue, int.MaxValue);
            this.WisCC = true;
            this.WDontWaste = true;
        }
    }

    public class Kassadin : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Kassadin()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 650);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 400, SkillShotType.Cone, (int)0.5f, int.MaxValue, 10);
            this.R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, (int)0.5f, int.MaxValue, 150);
            this.EisCC = true;
        }
    }

    public class Katarina : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Katarina()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625, DamageType.Magical);
            this.W = new Spell.Active(SpellSlot.W, 375, DamageType.Magical);
            this.E = new Spell.Skillshot(SpellSlot.E, 72, SkillShotType.Circular, 0, int.MaxValue, 50, DamageType.Magical);
            this.R = new Spell.Active(SpellSlot.R, 550, DamageType.Magical);
            this.EisDangerDash = true;
        }
    }

    public class Kayle : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Kayle()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 650);
            this.W = new Spell.Targeted(SpellSlot.W, 900);
            this.E = new Spell.Skillshot(SpellSlot.E, 650, SkillShotType.Circular, 1, 50, 400);
            this.R = new Spell.Targeted(SpellSlot.R, 900);
            this.QisCC = true;
            this.WisSaver = true;
            this.RisSaver = true;
        }
    }

    public class Kennen : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Kennen()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 125, 1700, 50) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 900);
            this.E = new Spell.Active(SpellSlot.E, 500); //Kappa ;)
            this.R = new Spell.Active(SpellSlot.R, 500);
        }
    }

    public class Khazix : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Khazix()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 325);
            this.W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, 225, 828, 80) { AllowedCollisionCount = 0 };
            this.E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Circular, 25, 1000, 100);
            this.R = new Spell.Active(SpellSlot.R);
            this.WisCC = true;
        }
    }

    public class Kindred : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Kindred()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1125, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 500);
            this.R = new Spell.Active(SpellSlot.R, 500);
            this.EisCC = true;
            this.RisSaver = true;
        }
    }

    public class Kled : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Kled()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 600, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Linear);
            this.R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear);
            this.QisCC = true;
            this.EisDash = true;
            this.RisCC = true;
            this.RDontWaste = true;
            this.RisTP = true;
        }
    }

    public class KogMaw : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public KogMaw()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 980, SkillShotType.Linear, 250, 2000, 50) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 700);
            this.E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear, 250, 1530, 60) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Circular, 750, int.MaxValue, 30) { AllowedCollisionCount = int.MaxValue };
            this.RDontWaste = true;

        }
    }

    public class Leblanc : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Leblanc()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 700, DamageType.Magical);
            this.W = new Spell.Skillshot(SpellSlot.W, 600, SkillShotType.Circular, 250, 1450, 250, DamageType.Magical);
            this.E = new Spell.Skillshot(SpellSlot.E, 950, SkillShotType.Linear, 250, 1550, 55, DamageType.Magical) { AllowedCollisionCount = 0 };
            this.R = new Spell.Skillshot(SpellSlot.R, int.MaxValue, SkillShotType.Circular);
            this.EisCC = true;
        }
    }

    public class LeeSin : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public LeeSin()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1800, 60) { AllowedCollisionCount = 0 };
            //Q2 = new Spell.Active(SpellSlot.Q, 1300);
            this.W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 50, 1500, 100);
            //W2 = new Spell.Active(SpellSlot.W, 700);
            this.E = new Spell.Skillshot(SpellSlot.E, 350, SkillShotType.Linear, 250, 2500, 100);
            //E2 = new Spell.Skillshot(SpellSlot.E, 675, SkillShotType.Linear, 250, 2500, 100)
            this.R = new Spell.Targeted(SpellSlot.R, 375);
            this.WisSaver = true;
            this.RisCC = true;
        }
    }

    public class Leona : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Leona()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Active(SpellSlot.W, 275);
            this.E = new Spell.Skillshot(SpellSlot.E, 875, SkillShotType.Linear, 250, 2000, 70) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Circular, 1000, int.MaxValue, 250);
            this.EDontWaste = true;
            this.RisCC = true;
        }
    }

    public class Lissandra : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Lissandra()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 725, SkillShotType.Linear, 250, 1000, 90) { AllowedCollisionCount = int.MaxValue };
            //Q1 = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Linear, 250, 1000, 70);
            this.W = new Spell.Active(SpellSlot.W, 450);
            this.E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 250, 1250, 100) { AllowedCollisionCount = int.MaxValue };
            //E1 = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Targeted(SpellSlot.R, 550);
            this.WisCC = true;
            this.RisCC = true;
        }
    }

    public class Lucian : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Lucian()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 675);
            this.W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, 250, 1600, 80);
            this.E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 1400, SkillShotType.Linear, 500, 2800, 110);
        }
    }

    public class Lulu : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Lulu()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 1450, 60) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Targeted(SpellSlot.W, 650);
            this.E = new Spell.Targeted(SpellSlot.E, 650);
            this.R = new Spell.Targeted(SpellSlot.R, 900);
            this.QisCC = true;
            this.WisCC = true;
            this.EisSaver = true;
            this.RisSaver = true;
        }
    }

    public class Lux : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Lux()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1175, SkillShotType.Linear, 250, 1200, 65) { AllowedCollisionCount = 1 };
            this.W = new Spell.Skillshot(SpellSlot.W, 1075, SkillShotType.Linear, 0, 1400, 85);
            this.E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Circular, 250, 1300, 330);
            this.R = new Spell.Skillshot(SpellSlot.R, 3200, SkillShotType.Circular, 500, int.MaxValue, 160) { AllowedCollisionCount = int.MaxValue };
            this.QisCC = true;
            this.WisSaver = true;
        }
    }

    public class Malphite : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Malphite()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E, 400);
            this.R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Circular, 250, 700, 270);
            this.QisCC = true;
            this.RisCC = true;
        }
    }

    public class Malzahar : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Malzahar()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 500, int.MaxValue, 100) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 500, int.MaxValue, 250);
            this.E = new Spell.Targeted(SpellSlot.E, 650);
            this.R = new Spell.Targeted(SpellSlot.R, 700);
            this.QisCC = true;
            this.RisCC = true;
        }
    }

    public class Maokai : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Maokai()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 600, SkillShotType.Linear, 500, 1200, 110) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Targeted(SpellSlot.W, 525);
            this.E = new Spell.Skillshot(SpellSlot.E, 1075, SkillShotType.Circular, 1000, 1500, 225);
            this.R = new Spell.Active(SpellSlot.R, 475);
            this.QisCC = true;
            this.WisDangerDash = true;
            this.RisToggle = true;
        }
    }

    public class MasterYi : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public MasterYi()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Active(SpellSlot.R);
            this.QisDangerDash = true;
            this.WisSaver = true;
        }
    }

    public class MissFortune : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public MissFortune()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 650);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Circular, 500, int.MaxValue, 200);
            this.R = new Spell.Skillshot(SpellSlot.R, 1400, SkillShotType.Cone, 0, int.MaxValue) { AllowedCollisionCount = int.MaxValue };
            this.EisCC = true;
        }
    }

    public class Mordekaiser : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Mordekaiser()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Targeted(SpellSlot.W, 1000);
            this.E = new Spell.Skillshot(SpellSlot.E, 670, SkillShotType.Cone, (int)0.25f, 2000, 12 * 2 * (int)Math.PI / 180) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Targeted(SpellSlot.R, 1500);
            this.WisToggle = true;
        }
    }

    public class Morgana : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Morgana()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1200, 80) { AllowedCollisionCount = 0 };
            this.W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 250, 2200, 400);
            this.E = new Spell.Targeted(SpellSlot.E, 750);
            this.R = new Spell.Active(SpellSlot.R, 600);
            this.QisCC = true;
            this.EisSaver = true;
            this.RisCC = true;
        }
    }

    public class Nami : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Nami()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 875, SkillShotType.Circular, 1, int.MaxValue, 150);
            this.W = new Spell.Targeted(SpellSlot.W, 725);
            this.E = new Spell.Targeted(SpellSlot.E, 800);
            this.R = new Spell.Skillshot(SpellSlot.R, 2750, SkillShotType.Linear, 250, 500, 160) { AllowedCollisionCount = int.MaxValue };
            this.QisCC = true;
            this.QDontWaste = true;
            this.WisSaver = true;
            this.RisCC = true;
        }
    }

    public class Nasus : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Nasus()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 150);
            this.W = new Spell.Targeted(SpellSlot.W, 600);
            this.E = new Spell.Skillshot(SpellSlot.E, 650, SkillShotType.Circular, 250, 190, int.MaxValue);
            this.R = new Spell.Active(SpellSlot.R);
            this.WisCC = true;
            this.RisSaver = true;
        }
    }

    public class Nautilus : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Nautilus()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E, (uint)ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange);
            this.R = new Spell.Targeted(SpellSlot.R, (uint)ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange);
            this.QisCC = true;
            this.QDontWaste = true;
            this.WisSaver = true;
            this.EisCC = true;
            this.RisCC = true;
        }
    }

    public class Nidalee : SpellBase // todo
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public Nidalee()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular);
            this.E = new Spell.Targeted(SpellSlot.E, 325);
            this.R = null;
            this.QDontWaste = true;
            this.EisSaver = true;
        }
    }

    public class Nocturne : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Nocturne()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1125, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 425);
            this.R = new Spell.Targeted(SpellSlot.R, 2500);
            // R1 = new Spell.Targeted(SpellSlot.R, R.Range);
            this.WisSaver = true;
            this.RisDangerDash = true;
        }
    }

    public class Nunu : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Nunu()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 350);
            this.W = new Spell.Targeted(SpellSlot.W, 700);
            this.E = new Spell.Targeted(SpellSlot.E, 550);
            this.R = new Spell.Active(SpellSlot.R, 650);
            this.EisCC = true;
        }
    }

    public class Olaf : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Olaf()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1550, 75) { AllowedCollisionCount = int.MaxValue };
            //Q2 = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1550, 75)     
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 325);
            this.R = new Spell.Active(SpellSlot.R);
            this.QisCC = true;
        }
    }

    public class Orianna : SpellBase // todo
    {
        public sealed override Spell.SpellBase Q { get; set; }
        public sealed override Spell.SpellBase W { get; set; }
        public sealed override Spell.SpellBase E { get; set; }
        public sealed override Spell.SpellBase R { get; set; }

        public Orianna()
        {
            {
                this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1550, 75) { AllowedCollisionCount = int.MaxValue };
                this.W = new Spell.Active(SpellSlot.W);
                this.E = new Spell.Targeted(SpellSlot.E, 325);
                this.R = new Spell.Active(SpellSlot.R);
                this.EisSaver = true;
            }
        }
    }

    public class Pantheon : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Pantheon()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Targeted(SpellSlot.W, 600);
            this.E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone, 250, 2000, 70) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Circular);
            this.WisCC = true;
            this.WDontWaste = true;
            this.WisDangerDash = true;
        }
    }

    public class Poppy : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Poppy()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 430, SkillShotType.Linear, 250, null, 100) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 400);
            this.E = new Spell.Targeted(SpellSlot.E, 525);
            this.R = new Spell.Chargeable(SpellSlot.R, 500, 1200, 4000, 250, int.MaxValue, 90) { AllowedCollisionCount = int.MaxValue };
            this.EisCC = true;
            this.RisCC = true;
            this.EisDangerDash = true;
        }
    }

    public class Quinn : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Quinn()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1025, SkillShotType.Linear, 0, 750, 210) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 2100);
            this.E = new Spell.Targeted(SpellSlot.E, 675);
            this.EDontWaste = true;
            this.EisCC = true;
            this.EisDangerDash = true;
        }
    }

    public class Rammus : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Rammus()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 200);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 325);
            this.R = new Spell.Active(SpellSlot.R, 300);
            this.EisCC = true;
        }
    }

    public class RekSai : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public RekSai()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 325);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 250);
            this.R = new Spell.Targeted(SpellSlot.R, 0);
        }
    }

    public class Renekton : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Renekton()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 225);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 450, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Active(SpellSlot.R);
            this.WisCC = true;
            this.EisDash = true;
            this.RisSaver = true;
        }
    }

    public class Rengar : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Rengar()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 700, SkillShotType.Cone, 250, 3000, 200, DamageType.Physical) { ConeAngleDegrees = 180, AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 500, DamageType.Magical);
            this.E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear, 250, 1500, 140, DamageType.Physical) { AllowedCollisionCount = 0 };
            this.R = new Spell.Active(SpellSlot.R, 2500);
            this.EisCC = true;
        }
    }

    public class Riven : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Riven()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular);
            this.E = new Spell.Active(SpellSlot.E, 325);
            this.R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Cone, 250, 1000, 250) { AllowedCollisionCount = int.MaxValue };
            this.WisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Rumble : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Rumble()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 600);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 840, SkillShotType.Linear, 250, 2000, 70) { AllowedCollisionCount = 0 };
            this.R = new Spell.Skillshot(SpellSlot.R, 1700, SkillShotType.Linear, 400, 2500, 120) { AllowedCollisionCount = int.MaxValue };
            this.WDontWaste = true;
            this.WisSaver = true;
            this.EisCC = true;
        }
    }

    public class Ryze : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ryze()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1700, 100) { AllowedCollisionCount = 0 };
            //Q2 = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1700, 100);
            this.W = new Spell.Targeted(SpellSlot.W, 600);
            this.E = new Spell.Targeted(SpellSlot.E, 600);
            this.R = new Spell.Skillshot(SpellSlot.R, 1500, SkillShotType.Circular);
            this.WisCC = true;
            this.RisTP = true;
        }
    }

    public class Sejuani : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Sejuani()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 650, SkillShotType.Linear, 0, 1600, 70) { AllowedCollisionCount = 0 };
            this.W = new Spell.Active(SpellSlot.W, 350);
            this.E = new Spell.Active(SpellSlot.E, 1000);
            this.R = new Spell.Skillshot(SpellSlot.R, 1175, SkillShotType.Linear, 250, 1600, 110) { AllowedCollisionCount = int.MaxValue };
            this.QisDash = true;
            this.RisCC = true;
        }
    }

    public class Shaco : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Shaco()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 400);
            //Q2 = new Spell.Targeted(SpellSlot.Q, 1100);
            this.W = new Spell.Skillshot(SpellSlot.W, 425, SkillShotType.Circular, 250, 1000, 70);
            this.E = new Spell.Targeted(SpellSlot.E, 625);
            this.R = new Spell.Active(SpellSlot.R, 200);
            this.EisCC = true;
            this.RisSaver = true;
        }
    }

    public class Shen : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Shen()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 2000, SkillShotType.Linear, 500, 2500, 150) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 610, SkillShotType.Linear, 500, 1600, 50) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Targeted(SpellSlot.R, 31000);
            this.WisSaver = true;
            this.EisCC = true;
            this.EisDash = true;
            this.RisSaver = true;
        }
    }

    public class Shyvana : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Shyvana()
        {
            this.Q = new Spell.Active(SpellSlot.Q, (uint)Player.Instance.GetAutoAttackRange());
            this.W = new Spell.Active(SpellSlot.W, 425);
            this.E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Linear, 250, 1500, 60) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 1500, 150) { AllowedCollisionCount = int.MaxValue };
            this.RisCC = true;
            this.RisDash = true;
        }
    }

    public class Singed : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Singed()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 500, 700, 350);
            this.E = new Spell.Targeted(SpellSlot.E, 125);
            this.R = new Spell.Active(SpellSlot.R);
            this.WDontWaste = true;
            this.EDontWaste = true;
            this.EisCC = true;
            this.WisCC = true;
            this.RisSaver = true;
        }
    }

    public class Sion : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Sion()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 740, SkillShotType.Cone, 250, 100, 500);
            //Q2 = new Spell.Active(SpellSlot.Q, 680);
            this.W = new Spell.Active(SpellSlot.W, 490);
            this.E = new Spell.Skillshot(SpellSlot.E, 755, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Active(SpellSlot.R, 800);
            this.QisCC = true;
            this.EisCC = true;
            this.RisCC = true;
        }
    }

    public class Sivir : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Sivir()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1245, SkillShotType.Linear, (int)0.25, 1030, 90) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = null; //new Spell.Active(SpellSlot.E);
            this.R = new Spell.Active(SpellSlot.R, 1000);
            //this.EisSaver = true;
            this.RisSaver = true;
        }
    }

    public class Skarner : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Skarner()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular);
            this.E = new Spell.Active(SpellSlot.E, 325);
            this.R = new Spell.Targeted(SpellSlot.R, 700);
            this.WisCC = true;
            this.RisCC = true;
        }
    }

    public class Sona : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Sona()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 850);
            this.W = new Spell.Active(SpellSlot.W, 1000);
            this.E = new Spell.Active(SpellSlot.E, 350);
            this.R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 2400, 140) { AllowedCollisionCount = int.MaxValue };
            this.WisSaver = true;
            this.RisCC = true;
        }
    }

    public class Soraka : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Soraka()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Circular, (int)0.283f, 1100, (int)210f);
            this.W = new Spell.Targeted(SpellSlot.W, 550);
            this.E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, (int)0.5f, 1750, (int)70f);
            this.R = new Spell.Active(SpellSlot.R);
            this.QisCC = true;
            this.WisSaver = true;
            this.EisCC = true;
            this.EDontWaste = true;
            this.RisSaver = true;
        }
    }

    public class Swain : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Swain()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 625);
            this.W = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, 1250, 275);
            this.E = new Spell.Targeted(SpellSlot.E, 625);
            this.R = new Spell.Active(SpellSlot.R);
            this.WisCC = true;
        }
    }

    public class Syndra : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Syndra()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 810, SkillShotType.Circular, 600, int.MaxValue, 125) { AllowedCollisionCount = int.MaxValue, DamageType = DamageType.Magical };
            this.W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 350, 1500, 140) { AllowedCollisionCount = int.MaxValue, DamageType = DamageType.Magical };
            this.E = new Spell.Skillshot(SpellSlot.E, 680, SkillShotType.Cone, 250, 2500, 50) { AllowedCollisionCount = int.MaxValue, DamageType = DamageType.Magical };
            this.R = new Spell.Targeted(SpellSlot.R, 675);
            this.EisCC = true;
            this.EDontWaste = true;
            //EQ = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 500, 2500, 55);
        }
    }

    public class TahmKench : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public TahmKench()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Linear, 250, 1000, 80);
            this.W = new Spell.Targeted(SpellSlot.W, 250);
            this.E = new Spell.Active(SpellSlot.E, 325);
            this.R = new Spell.Skillshot(SpellSlot.R, 4500, SkillShotType.Circular);
            this.QisCC = true;
            this.WisSaver = true;
            this.EisSaver = true;
            this.RisTP = true;
        }
    }

    public class Taliyah : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Taliyah()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 2000, 60) { AllowedCollisionCount = 0 };
            this.W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 250, int.MaxValue, 180);
            this.E = new Spell.Skillshot(SpellSlot.E, 700, SkillShotType.Cone);
            this.R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear);
            this.WisCC = true;
            this.WDontWaste = true;
            this.RisTP = true;
        }
    }

    public class Talon : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Talon()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 550, DamageType.Physical);
            this.W = new Spell.Skillshot(SpellSlot.W, 600, SkillShotType.Cone, 1, 2300, 75, DamageType.Physical) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Skillshot(SpellSlot.E, 700, SkillShotType.Linear);
            this.R = new Spell.Active(SpellSlot.R, 600, DamageType.Physical);
            this.EisDangerDash = true;
            this.WisCC = true;
        }
    }

    public class Taric : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Taric()
        {
            Q = new Spell.Active(SpellSlot.Q, 350);
            W = new Spell.Targeted(SpellSlot.W, 800);
            E = new Spell.Skillshot(SpellSlot.E, 580, SkillShotType.Linear, 250, int.MaxValue, 140);
            R = new Spell.Active(SpellSlot.R, 400);
            this.QisSaver = true;
            this.WisSaver = true;
            this.EisCC = true;
            this.RisSaver = true;
        }
    }

    public class Teemo : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Teemo()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 680);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Skillshot(SpellSlot.R, 300, SkillShotType.Circular, 500, 1000, 120);
            this.QisCC = true;
            this.RisCC = true;
        }
    }

    public class Thresh : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Thresh()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1040, SkillShotType.Linear, 500, 1900, 60) { AllowedCollisionCount = 0 };
            this.W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 250, 1800, 300) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Skillshot(SpellSlot.E, 480, SkillShotType.Linear, 0, 2000, 110) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Active(SpellSlot.R, 450);
            this.QDontWaste = true;
            this.QisCC = true;
            this.WisSaver = true;
            this.EisCC = true;
            this.RisCC = true;
        }
    }

    public class Tristana : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Tristana()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 550);
            this.W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 450, int.MaxValue, 180);
            this.E = new Spell.Targeted(SpellSlot.E, 550);
            this.R = new Spell.Targeted(SpellSlot.R, 550);
            this.WisDash = true;
            this.RisCC = true;
        }
    }

    public class Trundle : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Trundle()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 0, int.MaxValue, 1000);
            this.E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Circular, 250, int.MaxValue, 225);
            this.R = new Spell.Targeted(SpellSlot.R, 700);
            this.EisCC = true;
        }
    }

    public class Tryndamere : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Tryndamere()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Active(SpellSlot.W, 400);
            this.E = new Spell.Skillshot(SpellSlot.E, 660, SkillShotType.Linear, 250, 700, (int)92.5) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Active(SpellSlot.R);
            this.QisSaver = true;
            this.WisCC = true;
            this.EisDash = true;
            this.RisSaver = true;
        }
    }

    public class TwistedFate : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public TwistedFate()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1450, SkillShotType.Linear, 0, 1000, 40) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 800);
            this.E = new Spell.Active(SpellSlot.E);
            this.R = new Spell.Skillshot(SpellSlot.R, 5500, SkillShotType.Circular);
            this.RisTP = true;
        }
    }

    public class Twitch : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Twitch()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Skillshot(SpellSlot.W, 925, SkillShotType.Circular, 250, 1400, 275) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Active(SpellSlot.E, 1200);
            this.R = new Spell.Active(SpellSlot.R, 900);
            //R2 = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 0, 3000, 100)
            this.WisCC = true;
        }
    }

    public class Udyr : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Udyr()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 250);
            this.W = new Spell.Active(SpellSlot.W, 250);
            this.E = new Spell.Active(SpellSlot.E, 250);
            this.R = new Spell.Active(SpellSlot.R, 500);
            this.WisSaver = true;
            this.EisCC = true;
        }
    }

    public class Urgot : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Urgot()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 125, 1600, 60) { AllowedCollisionCount = 0 };
            //Q2 = new Spell.Targeted(SpellSlot.Q, 1200);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 250, 1500, 210);
            this.R = new Spell.Targeted(SpellSlot.R, 850);
            this.WisSaver = true;
            this.RisCC = true;
        }
    }

    public class Varus : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Varus()
        {
            //Q2 = new Spell.Skillshot(SpellSlot.Q, 925, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 0, 1900, 100);
            //Q2.AllowedCollisionCount = int.MaxValue;
            this.Q = new Spell.Chargeable(SpellSlot.Q, 925, 1625, 2000, 0, 1900, 100) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, 500, int.MaxValue, 750);
            this.R = new Spell.Skillshot(SpellSlot.R, 1075, SkillShotType.Linear, 0, 1200, 120) { AllowedCollisionCount = int.MaxValue };
            this.EisCC = true;
            this.RisCC = true;
        }
    }

    public class Vayne : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Vayne()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 300);
            //Q2 = new Spell.Skillshot(SpellSlot.Q, 300, SkillShotType.Linear, 250, 1000, 70);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 590);
            //E2 = new Spell.Skillshot(SpellSlot.E, 590, SkillShotType.Linear, 250, 1250);
            this.R = new Spell.Active(SpellSlot.R);
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Veigar : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Veigar()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 2000, 70) { AllowedCollisionCount = 1 };
            this.W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 1350, 0, 225);
            this.E = new Spell.Skillshot(SpellSlot.E, 700, SkillShotType.Circular, 500, 0, 425);
            this.R = new Spell.Targeted(SpellSlot.R, 650);
            this.EDontWaste = true;
            this.EisCC = true;
        }
    }

    public class Velkoz : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Velkoz()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear, 250, 1300, 50) { MinimumHitChance = HitChance.High, AllowedCollisionCount = 0 };
            this.W = new Spell.Skillshot(SpellSlot.W, 1050, SkillShotType.Linear, 250, 1700, 80) { MinimumHitChance = HitChance.High, AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Skillshot(SpellSlot.E, 850, SkillShotType.Circular, 500, 1500, 120) { MinimumHitChance = HitChance.High, AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 1550, SkillShotType.Linear) { MinimumHitChance = HitChance.High, AllowedCollisionCount = int.MaxValue };
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class Vi : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Vi()
        {
            this.Q = new Spell.Chargeable(SpellSlot.Q, 250, 750, 1250, 0, 1400, 55) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Active(SpellSlot.E, 600);
            //E2 = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone);
            this.R = new Spell.Targeted(SpellSlot.R, 800);
            this.QisCC = true;
            this.QDontWaste = true;
            this.RisCC = true;
            this.RDontWaste = true;
            this.RisDangerDash = true;
        }
    }

    public class Viktor : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Viktor()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 670);
            this.W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular, 500, int.MaxValue, 250) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Skillshot(SpellSlot.E, 1225, SkillShotType.Linear, 250, int.MaxValue, 100) { AllowedCollisionCount = int.MaxValue };
            this.R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, 250, int.MaxValue, 300) { AllowedCollisionCount = int.MaxValue };
            this.WisCC = true;
            this.WDontWaste = true;
        }
    }

    public class Vladimir : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Vladimir()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Active(SpellSlot.W, 150);
            this.E = new Spell.Active(SpellSlot.E, 600);
            this.R = new Spell.Skillshot(SpellSlot.R, 750, SkillShotType.Circular, 250, int.MaxValue, 170);
            this.WisSaver = true;
        }
    }

    public class Volibear : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Volibear()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 750);
            this.W = new Spell.Targeted(SpellSlot.W, 395);
            this.E = new Spell.Active(SpellSlot.E, 415);
            this.R = new Spell.Active(SpellSlot.R);
            this.EisCC = true;
        }
    }

    public class Warwick : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Warwick()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 400);
            this.W = new Spell.Active(SpellSlot.W, 1250);
            this.E = new Spell.Active(SpellSlot.E, 1500);
            this.R = new Spell.Targeted(SpellSlot.R, 700);
            this.RisCC = true;
        }
    }

    public class MonkeyKing : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public MonkeyKing()
        {
            this.Q = new Spell.Targeted(SpellSlot.Q, 600);
            this.W = new Spell.Active(SpellSlot.W, 700);
            this.E = new Spell.Targeted(SpellSlot.E, 325);
            this.R = new Spell.Active(SpellSlot.R, 350);
            this.RisCC = true;
            this.EisDangerDash = true;
        }
    }

    public class Xerath : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Xerath()
        {
            this.Q = new Spell.Chargeable(SpellSlot.Q, 750, 1500, 1500, 500, int.MaxValue, 100) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 1100, SkillShotType.Circular, 250, int.MaxValue, 100);
            this.E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 250, 1600, 70) { AllowedCollisionCount = 0 };
            this.R = new Spell.Skillshot(SpellSlot.R, 3200, SkillShotType.Circular, 500, int.MaxValue, 120);
            this.EisCC = true;
            this.EDontWaste = true;
        }
    }

    public class XinZhao : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public XinZhao()
        {
            this.Q = new Spell.Active(SpellSlot.Q);
            this.W = new Spell.Active(SpellSlot.W);
            this.E = new Spell.Targeted(SpellSlot.E, 650);
            this.R = new Spell.Active(SpellSlot.R, 500);
            this.RisCC = true;
            this.EisDangerDash = true;
        }
    }

    public class Yasuo : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Yasuo()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 475, SkillShotType.Linear, 250, int.MaxValue, 50) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 400, SkillShotType.Linear, 250, int.MaxValue, 150) { AllowedCollisionCount = int.MaxValue };
            this.E = new Spell.Targeted(SpellSlot.E, 475);
            this.R = new Spell.Targeted(SpellSlot.R, 1200);
            this.QisCC = true;
            this.WisSaver = true;
            this.EisDangerDash = true;
        }
    }

    public class Yorick : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Yorick()
        {
            this.Q = new Spell.Active(SpellSlot.Q, 125);
            this.W = new Spell.Skillshot(SpellSlot.W, 585, SkillShotType.Circular, 250, int.MaxValue, 200);
            this.E = new Spell.Targeted(SpellSlot.E, 540);
            this.R = new Spell.Targeted(SpellSlot.R, 835);
            this.RisSaver = true;
        }
    }

    public class Zac : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Zac()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 550, SkillShotType.Linear, 500, int.MaxValue, 120) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Active(SpellSlot.W, 350);
            this.E = new Spell.Chargeable(SpellSlot.E, 0, 1750, 1500, 500, 1500, 250);
            this.R = new Spell.Active(SpellSlot.R, 300);
            this.QisCC = true;
            this.EisCC = true;
            this.EisDash = true;
            this.RisCC = true;
        }
    }

    public class Zed : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Zed()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 600, SkillShotType.Linear, 250, 1000, 70) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Circular);
            this.E = new Spell.Active(SpellSlot.E, 325);
            this.R = new Spell.Targeted(SpellSlot.R, 700);
            this.RisDangerDash = true;
        }
    }

    public class Ziggs : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Ziggs()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Linear, 250, 1700, 130, DamageType.Magical) { AllowedCollisionCount = int.MaxValue };
            this.W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 250, 550, 250, DamageType.Magical);
            this.E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 250, 1530, 60, DamageType.Magical);
            this.R = new Spell.Skillshot(SpellSlot.R, 5300, SkillShotType.Circular, 1000, 2800, 500, DamageType.Magical);
            this.WisCC = true;
            this.EisCC = true;
        }
    }

    public class Zilean : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Zilean()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Circular, 300, 2000, 150);
            this.W = new Spell.Active(SpellSlot.W, 700);
            this.E = new Spell.Targeted(SpellSlot.E, 1000);
            this.R = new Spell.Targeted(SpellSlot.R, 410);
            this.QisCC = true;
            this.RisSaver = true;
        }
    }

    public class Zyra : SpellBase
    {
        public sealed override Spell.SpellBase Q { get; set; }

        public sealed override Spell.SpellBase W { get; set; }

        public sealed override Spell.SpellBase E { get; set; }

        public sealed override Spell.SpellBase R { get; set; }

        public Zyra()
        {
            this.Q = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Linear, 250, int.MaxValue, 85);
            this.W = new Spell.Skillshot(SpellSlot.W, 825, SkillShotType.Circular, 250, int.MaxValue, 20);
            this.E = new Spell.Skillshot(SpellSlot.E, 1100, SkillShotType.Linear, 250, 1150, 70);
            this.R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, 250, 1200, 500);

            this.QisCC = true;
            this.EisCC = true;
            this.RisCC = true;
        }
    }
}
