using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Utility.MiscUtil
{
    internal class Objects
    {
        public class DravenAxe
        {
            public DravenAxe(GameObject axe)
            {
                this.Axe = axe;
                this.StartTick = Core.GameTickCount;
                this.EndTick = this.StartTick + 1200;
            }
            public GameObject Axe;
            public float StartTick;
            public float EndTick;
            public float TicksLeft { get { return Core.GameTickCount - this.EndTick; } }
            public bool Finished { get { return this.TicksLeft <= 0; } }
        }
        public class OlafAxe
        {
            public OlafAxe(GameObject axe)
            {
                this.Axe = axe;
                this.StartTick = Core.GameTickCount;
                this.EndTick = this.StartTick + 8000;
            }
            public GameObject Axe;
            public float StartTick;
            public float EndTick;
            public float TicksLeft { get { return Core.GameTickCount - this.EndTick; } }
            public bool Finished { get { return this.TicksLeft <= 0; } }
        }
    }
}
