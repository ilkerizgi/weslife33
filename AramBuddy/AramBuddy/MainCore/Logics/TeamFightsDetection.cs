using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Logics
{
    internal class TeamFightsDetection
    {
        public static void Init()
        {
            // Used for detecting targeted spells for TeamFights Detection
            Obj_AI_Base.OnProcessSpellCast += delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                var from = sender as AIHeroClient;
                var target = args.Target as AIHeroClient;
                if (from != null)
                {
                    if (args.Slot == SpellSlot.R)
                    {
                        var lastAttack = new Misc.LastAttack(from, target) { Attacker = from, LastAttackTick = Core.GameTickCount, Target = target };
                        Misc.AutoAttacks.Add(lastAttack);
                        return;
                    }
                    if (target != null && from.Team != target.Team)
                    {
                        var lastAttack = new Misc.LastAttack(from, target) { Attacker = from, LastAttackTick = Core.GameTickCount, Target = target };
                        Misc.AutoAttacks.Add(lastAttack);
                    }
                }
            };

            // Used for detecting AutoAttacks for TeamFights Detection
            Obj_AI_Base.OnBasicAttack += delegate(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
                {
                    var target = args.Target as AIHeroClient;
                    if (sender != null && target != null && sender.Team != target.Team)
                    {
                        var lastAttack = new Misc.LastAttack(sender, target) { Attacker = sender, LastAttackTick = Core.GameTickCount, Target = target };
                        Misc.AutoAttacks.Add(lastAttack);
                    }
                };
        }
    }
}
