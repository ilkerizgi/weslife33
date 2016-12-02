using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.Plugins.Activator.Spells.AutoShield
{
    internal static class Cast
    {
        internal static bool On(this SheildsDatabase.Shield shield, Obj_AI_Base target)
        {
            if (target == null)
                return false;

            switch (shield.Type)
            {
                case SheildsDatabase.Shield.SheildType.Wall:
                    return shield.Spell.Cast(Player.Instance.ServerPosition.Extend(target, shield.Spell.Range - 15).To3D());
                case SheildsDatabase.Shield.SheildType.SpellBlock:
                    return shield.Spell.Cast();
                case SheildsDatabase.Shield.SheildType.CastOnEnemy:
                    target = TargetSelector.GetTarget(shield.Spell.Range, DamageType.True);
                    break;
            }

            if (target == null || !target.IsKillable())
                return false;

            if (target.IsInRange(Player.Instance, shield.Spell.Range) || shield.Spell.Range == 0)
            {
                if (shield.Spell is Spell.Targeted || shield.Spell is Spell.Skillshot)
                {
                    return shield.Spell.Cast(target);
                }

                if (shield.Spell is Spell.Active)
                {
                    return shield.Spell.Cast();
                }
            }

            return false;
        }
    }
}
