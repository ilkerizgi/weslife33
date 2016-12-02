// <summary>
//   The class containing the BuildData used by the interpreter to buy items in order
// </summary>
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;

namespace AramBuddy.AutoShop
{
    /// <summary>
    ///     The class containing the BuildData used by the interpreter to buy items in order
    /// </summary>
    public class Build
    {
        /// <summary>
        ///     An array of the item names
        /// </summary>
        public string[] BuildData { get; set; }

        /// <summary>
        /// returns The build name.
        /// </summary>
        public static string BuildName()
        {
            var ChampionName = Player.Instance.CleanChampionName();
            
            if (ADC.Any(s => s.Equals(ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return "ADC";
            }

            if (AD.Any(s => s.Equals(ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return "AD";
            }

            if (AP.Any(s => s.Equals(ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return "AP";
            }

            if (ManaAP.Any(s => s.Equals(ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return "ManaAP";
            }

            if (Tank.Any(s => s.Equals(ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return "Tank";
            }

            Logger.Send("Failed To Detect " + ChampionName, Logger.LogLevel.Warn);
            //Logger.Send("Using Default Build !");
            return "Default";
        }

        /// <summary>
        ///     Creates Builds
        /// </summary>
        public static void CreateDefualtBuild()
        {
            try
            {
                var filename = $"{BuildName()}.json";
                var WebClient = new WebClient();
                WebClient.DownloadStringTaskAsync($"https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/DefaultBuilds/{filename}");
                WebClient.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
                    {
                        try
                        {
                            if (args.Cancelled || args.Error != null)
                            {
                                Logger.Send(args.Error?.Message, Logger.LogLevel.Error);
                                Logger.Send("Wrong Response or was canceled, No Champion Build Created !", Logger.LogLevel.Warn);
                                Logger.Send("No Build is being used !", Logger.LogLevel.Warn);
                                return;
                            }
                            if (args.Result.Contains("data"))
                            {
                                File.WriteAllText(Setup.BuildPath + "\\" + filename, args.Result);
                                Setup.Builds.Add(BuildName(), File.ReadAllText(Setup.BuildPath + "\\" + filename));
                                Logger.Send(BuildName() + " Build Created for " + Player.Instance.ChampionName + " - " + BuildName());
                                Setup.UseDefaultBuild();
                            }
                            else
                            {
                                Logger.Send("Wrong Response, No Champion Build Created", Logger.LogLevel.Warn);
                                Console.WriteLine(args.Result);
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            Logger.Send("Failed to create default build for " + Player.Instance.ChampionName, ex, Logger.LogLevel.Error);
                            Logger.Send("No build is currently being used!", Logger.LogLevel.Error);
                            Logger.Send(ex?.InnerException?.Message, Logger.LogLevel.Error);
                        }
                    };
                /*
                using (var WebClient = new WebClient())
                {
                    using (var request = WebClient.DownloadStringTaskAsync("https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/DefaultBuilds/" + filename))
                    {
                        if (request.IsFaulted || request.IsCanceled)
                        {
                            Logger.Send("Wrong Response, Or Request Was Cancelled", Logger.LogLevel.Warn);
                            Logger.Send(request.Exception?.InnerException?.Message, Logger.LogLevel.Warn);
                            Console.WriteLine(request.Result);
                        }
                        else
                        {
                            if (request.Result != null && request.Result.Contains("data"))
                            {
                                File.WriteAllText(Setup.BuildPath + "\\" + filename, request.Result);
                                Setup.Builds.Add(BuildName(), File.ReadAllText(Setup.BuildPath + "\\" + filename));
                                Logger.Send(BuildName() + " Build Created for " + Player.Instance.ChampionName + " - " + BuildName());
                                Setup.UseDefaultBuild();
                            }
                            else
                            {
                                Logger.Send("Wrong Response, No Champion Build Created", Logger.LogLevel.Warn);
                                Console.WriteLine(request.Result);
                            }
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                // if faild to create build terminate the AutoShop
                Logger.Send("Failed to create default build for " + Player.Instance.ChampionName, ex, Logger.LogLevel.Error);
                Logger.Send("No build is currently being used!", Logger.LogLevel.Error);
            }
        }

        /// <summary>
        ///     Creates Builds
        /// </summary>
        public static void GetBuildFromService()
        {
            try
            {
                try
                {
                    var filename = $"{Player.Instance.CleanChampionName()}.json";
                    var WebClient = new WebClient();
                    WebClient.DownloadStringTaskAsync($"https://raw.githubusercontent.com/plsfixrito/AramBuddyBuilds/master/{Config.CurrentPatchUsed}/{Config.CurrentBuildService}/{filename}");
                    WebClient.DownloadStringCompleted += delegate (object sender, DownloadStringCompletedEventArgs args)
                    {
                        if (args.Cancelled || args.Error != null)
                        {
                            Logger.Send(args.Error?.Message, Logger.LogLevel.Error);
                            Logger.Send("Wrong Response or was canceled, No Champion Build Created !", Logger.LogLevel.Warn);
                            Logger.Send("Trying To Get Defualt Build !", Logger.LogLevel.Warn);
                            Setup.UseDefaultBuild();
                            return;
                        }
                        if (args.Result.Contains("data"))
                        {
                            var filepath = $"{Setup.BuildPath}/{Config.CurrentPatchUsed}/{Config.CurrentBuildService}/{filename}";
                            File.WriteAllText(filepath, args.Result);
                            Setup.Builds.Add(Player.Instance.CleanChampionName(), File.ReadAllText(filepath));
                            Logger.Send("Created Build for " + Player.Instance.ChampionName);
                            Setup.CustomBuildService();
                        }
                        else
                        {
                            Logger.Send("Wrong Response or was canceled, No Champion Build Created !", Logger.LogLevel.Warn);
                            Logger.Send("Trying To Get Defualt Build !", Logger.LogLevel.Warn);
                            Setup.UseDefaultBuild();
                        }
                    };
                }
                catch (TargetInvocationException ex)
                {
                    Logger.Send(ex?.InnerException?.Message, Logger.LogLevel.Error);
                    Setup.UseDefaultBuild();
                }
                /*
                using (var WebClient = new WebClient())
                {
                    using (var request = WebClient.DownloadStringTaskAsync("https://raw.githubusercontent.com/plsfixrito/AramBuddyBuilds/master/" + Config.CurrentPatchUsed + "\\" + Config.CurrentBuildService + "/" + filename))
                    {
                        if (request != null && !request.IsCanceled && !request.IsFaulted)
                        {
                            if (request.Result.Contains("data"))
                            {
                                File.WriteAllText(Setup.BuildPath + "\\" + Config.CurrentPatchUsed + "\\" + Config.CurrentBuildService + "\\" + filename, request.Result);
                                Setup.Builds.Add(Player.Instance.CleanChampionName(), File.ReadAllText(Setup.BuildPath + "\\" + Config.CurrentPatchUsed + "\\" + Config.CurrentBuildService + "\\" + filename));
                                Logger.Send("Created Build for " + Player.Instance.ChampionName);
                                Setup.CustomBuildService();
                            }
                            else
                            {
                                Logger.Send("Wrong Response, No Champion Build Created !", Logger.LogLevel.Warn);
                                Logger.Send("Trying To Get Defualt Build !", Logger.LogLevel.Warn);
                                Setup.UseDefaultBuild();
                                //Console.WriteLine(args.Result);
                            }
                        }
                        else
                        {
                            Logger.Send("Failed Getting build, No Response !", Logger.LogLevel.Warn);
                            Logger.Send("Trying To Get Defualt Build !", Logger.LogLevel.Warn);
                            Setup.UseDefaultBuild();
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                // if faild to create build terminate the AutoShop
                Logger.Send("Failed to create Build from service " + Config.CurrentBuildService + " " + Config.CurrentPatchUsed + " for " + Player.Instance.ChampionName, Logger.LogLevel.Error);
                Logger.Send(ex.InnerException?.Message, Logger.LogLevel.Error);
                Logger.Send("Trying To Get Defualt Build !", Logger.LogLevel.Warn);
                Setup.UseDefaultBuild();
            }
        }

        /// <summary>
        ///  ADC Champions.
        /// </summary>
        public static readonly string[] ADC =
            {
                "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jhin", "Jinx", "Kalista", "Kindred", "KogMaw", "Lucian", "MissFortune", "Sivir", "Quinn",
                "Tristana", "Twitch", "Urgot", "Varus", "Vayne"
            };

        /// <summary>
        ///  Mana AP Champions.
        /// </summary>
        public static readonly string[] ManaAP =
            {
                "Ahri", "Anivia", "Annie", "AurelionSol", "Azir", "Brand", "Cassiopeia", "Diana", "Elise", "Ekko", "Evelynn", "Fiddlesticks", "Fizz", "Galio",
                "Gragas", "Heimerdinger", "Janna", "Karma", "Karthus", "Kassadin", "Kayle", "Leblanc", "Lissandra", "Lulu", "Lux", "Malzahar", "Morgana", "Nami", "Nidalee", "Ryze", "Orianna", "Sona",
                "Soraka", "Swain", "Syndra", "Taliyah", "Teemo", "TwistedFate", "Veigar", "Viktor", "VelKoz", "Xerath", "Ziggs", "Zilean", "Zyra"
            };

        /// <summary>
        ///  AP no Mana Champions.
        /// </summary>
        public static readonly string[] AP = { "Akali", "Katarina", "Kennen", "Mordekaiser", "Rumble", "Vladimir" };

        /// <summary>
        ///  AD Champions.
        /// </summary>
        public static readonly string[] AD =
            {
                "Aatrox", "Fiora", "Gangplank", "Jax", "Jayce", "KhaZix", "LeeSin", "MasterYi", "Nocturne", "Olaf", "Pantheon", "Rengar", "Riven", "Talon", "Tryndamere",
                "Wukong", "XinZhao", "Yasuo", "Zed"
            };

        /// <summary>
        ///  Tank Champions.
        /// </summary>
        public static readonly string[] Tank =
            {
                "Alistar", "Amumu", "Blitzcrank", "Bard", "Braum", "ChoGath", "Darius", "DrMundo", "Garen", "Gnar", "Hecarim", "Kled", "Illaoi", "Irelia", "Ivern", "JarvanIV",
                "Leona", "Malphite", "Maokai", "Nasus", "Nautilus", "Nunu", "Poppy", "Rammus", "RekSai", "Renekton", "Sejuani", "Shaco", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "TahmKench",
                "Taric", "Thresh", "Trundle", "Udyr", "Vi", "Volibear", "Warwick", "Yorick", "Zac"
            };
    }
}
