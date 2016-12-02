using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using SharpDX;

namespace AramBuddy.MainCore.Utility.MiscUtil.Caching
{
    internal static class Cache
    {
        public static List<Interuptables> InteruptablesCache = new List<Interuptables>();
        public static List<Gapclosers> GapclosersCache = new List<Gapclosers>();
        public static void Init()
        {
            Game.OnTick += Game_OnTick;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Game_OnTick(EventArgs args)
        {
            InteruptablesCache.RemoveAll(s => Game.Time - s.Args.EndTime >= 0
            || (s.Sender != null && (s.Sender.IsDead || (!s.Sender.Spellbook.IsCastingSpell && !s.Sender.Spellbook.IsChanneling && !s.Sender.Spellbook.IsCharging))));
            GapclosersCache.RemoveAll(
                s => Core.GameTickCount - s.Args.TickCount > 1000
                || s.Sender != null && (s.Args.End.Equals(s.Sender.Position) || s.Args.End.Equals(s.Sender.ServerPosition) || s.Sender.IsDead || (s.IsDash && !s.Sender.IsDashing())));
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender != null)
            {
                var info = new Gapclosers(sender, e, sender.IsDashing());
                if(!GapclosersCache.Contains(info))
                    GapclosersCache.Add(info);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender != null)
            {
                var info = new Interuptables(sender, e);
                if (!InteruptablesCache.Contains(info))
                    InteruptablesCache.Add(info);
            }
        }

        public static Vector3 GapCloseEndPos(this AIHeroClient target)
        {
            var end = GapclosersCache.FirstOrDefault(g => g.Sender.IdEquals(target))?.Args.End;
            if (end != null)
                return (Vector3)end;
            return Vector3.Zero;
        }
        public static bool IsGapClosing(this AIHeroClient target)
        {
            if (target == null)
                return false;

            return GapclosersCache.Any(g => g.Sender.IdEquals(target));
        }
        public static bool CanBeInterrupted(this Obj_AI_Base target)
        {
            return InteruptablesCache.Any(g => g.Sender.IdEquals(target));
        }
    }
}
