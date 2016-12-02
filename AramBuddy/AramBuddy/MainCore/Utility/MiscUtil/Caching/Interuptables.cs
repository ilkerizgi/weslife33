using EloBuddy;
using EloBuddy.SDK.Events;

namespace AramBuddy.MainCore.Utility.MiscUtil.Caching
{
    public class Interuptables
    {
        public Obj_AI_Base Sender;
        public Interrupter.InterruptableSpellEventArgs Args;

        public Interuptables(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args)
        {
            this.Sender = sender;
            this.Args = args;
        }
    }
}
