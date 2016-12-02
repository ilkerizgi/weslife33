using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using GenesisSpellLibrary;

namespace AramBuddy.MainCore.Logics.Casting
{
    internal class Teleport
    {
        public static void Cast()
        {
            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsReady() && s.IsTP()))
            {
                if (spell is Spell.Skillshot && ObjectsManager.ClosestAlly != null && ObjectsManager.AllySpawn != null
                    && ObjectsManager.ClosestAlly.Distance(ObjectsManager.AllySpawn) > 2000
                    && ObjectsManager.ClosestAlly.Distance(ObjectsManager.AllySpawn) > Player.Instance.Distance(ObjectsManager.AllySpawn) 
                    && Player.Instance.Distance(ObjectsManager.AllySpawn) < 3000 && ObjectsManager.ClosestAlly.IsValidTarget(spell.Range))
                {
                    Player.CastSpell(spell.Slot, ObjectsManager.ClosestAlly.PredictPosition().Random());
                }
            }
        }
    }
}
