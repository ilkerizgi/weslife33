using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Spells;

namespace AramBuddy.Plugins.Activator.Spells
{
    internal class Summoners
    {   
        private static Menu SummMenu;

        public static void Init()
        {
            SummMenu = Load.MenuIni.AddSubMenu("SummonerSpells");
            SummMenu.AddGroupLabel("Allies");
            SummMenu.CreateCheckBox("HealAllies", "Use Heal For Allies");
            SummMenu.CreateSlider("allyhp", "Ally HealthPercent {0}% To Use Heal", 30);
            SummMenu.AddSeparator(0);
            SummMenu.AddGroupLabel("Self");
            SummMenu.CreateCheckBox("HealSelf", "Use Heal For Self");
            SummMenu.CreateSlider("HealHP", "HealthPercent {0}% To Use Heal For ME", 30);
            SummMenu.CreateCheckBox("Barrier", "Use Barrier For Self");
            SummMenu.CreateSlider("BarrierHP", "Self HealthPercent {0}% To Use Heal", 30);

            Events.OnIncomingDamage += Events_OnIncomingDamage;
        }

        private static void Events_OnIncomingDamage(Events.InComingDamageEventArgs args)
        {
            if (args.Target == null || !args.Target.IsKillable() || args.Target.Distance(Player.Instance) > 800) return;

            var damagepercent = args.InComingDamage / args.Target.TotalShieldHealth() * 100;
            var death = args.InComingDamage >= args.Target.PredictHealth() && args.Target.PredictHealthPercent() < 99;

            if (SummMenu.CheckBoxValue("HealAllies") && args.Target.IsAlly && !args.Target.IsMe && (SummMenu.SliderValue("allyhp") >= args.Target.PredictHealthPercent() || death))
            {
                SummonerSpells.Heal.Cast();
                return;
            }

            if (args.Target.IsMe)
            {
                if (SummMenu.CheckBoxValue("HealSelf") && SummonerSpells.Heal.IsReady() && (SummMenu.SliderValue("HealHP") >= args.Target.PredictHealthPercent() || death))
                {
                    SummonerSpells.Heal.Cast();
                    return;
                }
                if (SummMenu.CheckBoxValue("Barrier") && SummonerSpells.Barrier.IsReady() && (SummMenu.SliderValue("BarrierHP") >= Player.Instance.PredictHealthPercent() || death))
                {
                    SummonerSpells.Barrier.Cast();
                }
            }
        }
    }
}
