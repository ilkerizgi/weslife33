using System;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using static AramBuddy.Plugins.Activator.Items.Database;

namespace AramBuddy.Plugins.Activator.Items
{
    internal class Defence
    {
        private static Menu Def;

        public static void Init()
        {
            try
            {
                Def = Load.MenuIni.AddSubMenu("Defence Items");
                Def.CreateCheckBox("ally", "Use For Allies");
                Def.AddSeparator();
                Def.CreateCheckBox(Zhonyas.Id.ToString(), "Use Zhonyas");
                Def.CreateSlider(Zhonyas.Id + "hp", "Zhonyas Health% {0}", 30);
                Def.CreateCheckBox(Seraphs.Id.ToString(), "Use Seraphs");
                Def.CreateSlider(Seraphs.Id + "hp", "Seraphs Health% {0}", 40);
                Def.CreateCheckBox(Solari.Id.ToString(), "Use Solari");
                Def.CreateSlider(Solari.Id + "hp", "Solari Health% {0}", 50);
                Def.CreateCheckBox(Randuins.Id.ToString(), "Use Randuins");
                Def.CreateSlider(Randuins.Id + "hp", "Randuins Health% {0}", 60);

                Events.OnIncomingDamage += Events_OnIncomingDamage;
                Game.OnTick += Game_OnTick;
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Defence Error While Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            try
            {
                if (Player.Instance.IsDead) return;

                if (Randuins.ItemReady(Def) && Def.CompareSlider(Randuins.Id + "hp", Player.Instance.PredictHealthPercent()) && Player.Instance.CountEnemyHeros((int)Randuins.Range) > 0
                    && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Randuins.Cast();
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Defence Error At Game_OnTick", ex, Logger.LogLevel.Error);
            }
        }

        private static void Events_OnIncomingDamage(Events.InComingDamageEventArgs args)
        {
            try
            {
                if (args.Target == null || !args.Target.IsKillable())
                    return;

                if (args.Target.IsAlly && !args.Target.IsMe && Def.CheckBoxValue("ally"))
                {
                    if (Solari.ItemReady(Def) && Def.SliderValue(Solari.Id + "hp") >= args.Target.PredictHealthPercent())
                    {
                        Solari.Cast();
                        return;
                    }
                }
                if (args.Target.IsMe)
                {
                    if (Zhonyas.ItemReady(Def) && Def.SliderValue(Zhonyas.Id + "hp") >= args.Target.PredictHealthPercent())
                    {
                        Zhonyas.Cast();
                        return;
                    }
                    if (Seraphs.ItemReady(Def) && Def.SliderValue(Seraphs.Id + "hp") >= args.Target.PredictHealthPercent())
                    {
                        Seraphs.Cast();
                        return;
                    }
                    if (Solari.ItemReady(Def) && Def.SliderValue(Solari.Id + "hp") >= args.Target.PredictHealthPercent())
                    {
                        Solari.Cast();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Defence Error At Events_OnIncomingDamage", ex, Logger.LogLevel.Error);
            }
        }
    }
}
