using System;
using System.Collections.Generic;
using System.IO;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using static AramBuddy.Config;

namespace AramBuddy.MainCore.Utility
{
    internal class Chatting
    {
        private static string Start;
        private static string End;

        private static readonly List<string> StartMsg = new List<string>
        {
            "Hi", "Hello", "Greetings", "GL", "HF", "GLHF", "GL HF"
        };

        private static readonly List<string> EndMsg = new List<string>
        {
            "GG", "WP", "GGWP", "GG WP"
        };

        public static void Init()
        {
            var startfile = Misc.AramBuddyFolder + "\\Chat\\Start.txt";
            var endfile = Misc.AramBuddyFolder + "\\Chat\\End.txt";
            var random = new Random();

            if (!Directory.Exists(Misc.AramBuddyFolder + "\\Chat\\"))
            {
                Directory.CreateDirectory(Misc.AramBuddyFolder + "\\Chat\\");
            }

            if (!File.Exists(startfile))
            {
                using (var sw = File.AppendText(startfile))
                {
                    StartMsg.ForEach(t => sw.WriteLine(t));
                }
            }

            if (!File.Exists(endfile))
            {
                using (var sw = File.AppendText(endfile))
                {
                    EndMsg.ForEach(t => sw.WriteLine(t));
                }
            }

            Start = File.ReadAllLines(startfile).Length == 0 ? StartMsg[random.Next(StartMsg.Count)] : File.ReadAllLines(startfile)[random.Next(File.ReadAllLines(startfile).Length)];
            End = File.ReadAllLines(endfile).Length == 0 ? EndMsg[random.Next(EndMsg.Count)] : File.ReadAllLines(endfile)[random.Next(File.ReadAllLines(endfile).Length)];

            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Events.OnGameEnd += Events_OnGameEnd;
            Chat.OnMessage += Chat_OnMessage;
            Chat.OnClientSideMessage += Chat_OnClientSideMessage;
        }

        private static void Chat_OnClientSideMessage(ChatClientSideMessageEventArgs args)
        {
            if (Enableff && args.Message.ToLower().Contains("/nosurrender."))
            {
                Core.DelayAction(
                    () =>
                        {
                            Chat.Say("/ff");
                            Logger.Send("Voted Surrender With Team", Logger.LogLevel.Event);
                        }, new Random().Next(1000, 5000));
            }
        }

        private static void Chat_OnMessage(AIHeroClient sender, ChatMessageEventArgs args)
        {
            var msg = args.Message.Replace("<font color=", "").Replace("\"#40c1ff\">", "").Replace("\"#ffffff\">", "").Replace("\"#ff3333\">", "").Replace("</font>", "");
            var ts = TimeSpan.FromSeconds(Game.Time);
            var time = $"{ts.Minutes}:{ts.Seconds:D2}";
            var finalmsg = "[" + time + "] " + msg;
            Misc.SaveLogs(finalmsg, Misc.AramBuddyDirectories.ChatLogs);
        }

        private static void Events_OnGameEnd(bool args)
        {
            if(EnableChat)
                Core.DelayAction(() => Chat.Say("/all " + End), new Random().Next(500 + Game.Ping, 2000 + Game.Ping));
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if(Game.Time <= 200 && EnableChat)
                Core.DelayAction(() => Chat.Say("/all " + Start), new Random().Next(500 + Game.Ping, (5000 + Game.Ping) * 2));
        }
    }
}
