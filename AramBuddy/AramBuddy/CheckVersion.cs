using System;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK.Notifications;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;
using Version = System.Version;

namespace AramBuddy
{
    /// <summary>
    ///     A class Used For Checking AramBuddy Version
    /// </summary>
    internal class CheckVersion
    {
        private static Text text;
        private static string UpdateMsg = string.Empty;
        private const string UpdateMsgPath = "https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/AramBuddy/AramBuddy/msg.txt";
        private const string WebVersionPath = "https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/AramBuddy/AramBuddy/Properties/AssemblyInfo.cs";
        private static readonly Version CurrentVersion = typeof(CheckVersion).Assembly.GetName().Version;
        public static bool Outdated;
        private static bool Sent;
        public static void Init()
        {
            try
            {
                Logger.Send("Checking For Updates..");
                var size = Drawing.Width <= 1280 || Drawing.Height <= 720 ? 10F : 40F;
                text = new Text("ARAMBUDDY OUTDATED! PLEASE UPDATE!", new Font("Euphemia", size, FontStyle.Bold)) { Color = Color.White };
                var WebClient = new WebClient();
                WebClient.DownloadStringTaskAsync(UpdateMsgPath);
                WebClient.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
                {
                    if (args.Cancelled || args.Error != null)
                    {
                        Logger.Send("Failed to get update Message.", Logger.LogLevel.Warn);
                        Logger.Send("Wrong response, or request was cancelled.", Logger.LogLevel.Warn);
                        Logger.Send(args.Error?.InnerException?.Message, Logger.LogLevel.Warn);
                        return;
                    }

                    UpdateMsg = args.Result;

                    WebClient.Dispose();
                };

                var WebClient2 = new WebClient();
                WebClient2.DownloadStringTaskAsync(WebVersionPath);
                WebClient2.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
                {
                    if (args.Cancelled || args.Error != null)
                    {
                        Logger.Send("Failed to check live version.", Logger.LogLevel.Warn);
                        Logger.Send("Wrong response, or request was cancelled.", Logger.LogLevel.Warn);
                        Logger.Send(args.Error?.InnerException?.Message, Logger.LogLevel.Warn);
                        return;
                    }
                    if (args.Cancelled)
                    {
                        Logger.Send("Wrong response, or request was cancelled.", Logger.LogLevel.Warn);
                        Logger.Send(args.Error?.InnerException?.Message, Logger.LogLevel.Warn);
                        Console.WriteLine(args.Result);
                    }
                    if (!args.Result.Contains(CurrentVersion.ToString()))
                    {
                        Drawing.OnEndScene += delegate
                        {
                            text.Position = new Vector2(Drawing.Width * 0.01f, Drawing.Height * 0.1f);
                            text.Draw();
                        };
                        Outdated = true;
                        Logger.Send("Update available for AramBuddy!", Logger.LogLevel.Warn);
                        Logger.Send("Update Log: " + UpdateMsg);
                    }
                    else
                    {
                        Logger.Send("AramBuddy is updated to the latest version!");
                    }
                    WebClient2.Dispose();
                };

                Game.OnTick += delegate
                {
                    if (UpdateMsg != string.Empty && !Sent && Outdated)
                    {
                        Chat.Print("<b>AramBuddy: Update available for AramBuddy!</b>");
                        Chat.Print("<b>AramBuddy Update Log: " + UpdateMsg + "</b>");
                        Notifications.Show(new SimpleNotification("ARAMBUDDY OUTDATED", "Update Log: " + UpdateMsg), 25000);
                        Sent = true;
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Send("Update check failed!", ex, Logger.LogLevel.Error);
            }
        }
    }
}
