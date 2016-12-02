using EloBuddy;
using EloBuddy.SDK.Events;

namespace AramBuddy.MainCore.Utility.MiscUtil.Caching
{
    public class Gapclosers
    {
        public AIHeroClient Sender;
        public Gapcloser.GapcloserEventArgs Args;
        public bool IsDash;

        public Gapclosers(AIHeroClient sender, Gapcloser.GapcloserEventArgs args, bool isdash)
        {
            this.Sender = sender;
            this.Args = args;
            this.IsDash = isdash;
        }
    }
}
