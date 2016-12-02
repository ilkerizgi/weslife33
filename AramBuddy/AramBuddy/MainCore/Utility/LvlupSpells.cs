using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AramBuddy.MainCore.Utility
{
    internal class LvlupSpells
    {
        public static Levelset CurrentLevelset = new Levelset();
        private static string LevelSetDirectory { get { return $"{Misc.AramBuddyFolder}\\LevelSets\\{Config.CurrentPatchUsed}"; } }
        private static string LevelSetFile { get { return $"{Misc.AramBuddyFolder}\\LevelSets\\{Config.CurrentPatchUsed}\\{Player.Instance.CleanChampionName()}.json"; } }
        private static string FileURL { get { return $"https://raw.githubusercontent.com/plsfixrito/AramBuddyBuilds/master/{Config.CurrentPatchUsed}/LevelSets/{Player.Instance.CleanChampionName()}.json"; } }

        internal static void Init()
        {
            try
            {
                if (File.Exists(LevelSetFile))
                {
                    var filecontant = File.ReadAllText(LevelSetFile);
                    if (filecontant.Contains("LevelSet"))
                    {
                        TryParseData(filecontant, out CurrentLevelset);
                    }
                    else
                    {
                        File.Delete(LevelSetFile);
                        DownLoadLevelSet();
                    }
                    Logger.Send($"Loaded LevelSet for {Player.Instance.ChampionName}");
                }
                else
                {
                    DownLoadLevelSet();
                }
            }
            catch (Exception ex)
            {
                Logger.Send($"ERROR Failed to create level set for {Player.Instance.ChampionName}", ex, Logger.LogLevel.Error);
            }
            Game.OnTick += Game_OnTick;
        }

        private static void DownLoadLevelSet()
        {
            try
            {
                if (!Directory.Exists(LevelSetDirectory))
                    Directory.CreateDirectory(LevelSetDirectory);

                var webclient = new WebClient();
                webclient.DownloadStringAsync(new Uri(FileURL));
                webclient.DownloadStringCompleted += delegate (object sender, DownloadStringCompletedEventArgs args)
                {
                    if (args.Cancelled || args.Error != null)
                    {
                        Logger.Send("Failed to create Levelset.", Logger.LogLevel.Warn);
                        Logger.Send("Wrong response, or request was cancelled.", Logger.LogLevel.Warn);
                        Logger.Send(args.Error?.InnerException?.Message, Logger.LogLevel.Warn);
                        return;
                    }

                    if (args.Result.Contains("LevelSet"))
                    {
                        File.WriteAllText(LevelSetFile, args.Result);
                        TryParseData(args.Result, out CurrentLevelset);
                        Logger.Send($"Created LevelSet For {Player.Instance.ChampionName}");
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Send("ERROR: ", ex, Logger.LogLevel.Error);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if(Config.EnableAutoLvlUP)
                LevelSpells();
        }

        private static int R1()
        {
            if (Player.Instance.ChampionName == "Karma" || Player.Instance.ChampionName == "Nidalee" || Player.Instance.ChampionName == "Jayce")
                return 1;
            return 0;
        }
        private static int I;

        private static void LevelSpells()
        {
            var qL = Player.Instance.Spellbook.GetSpell(SpellSlot.Q).Level - R1();
            var wL = Player.Instance.Spellbook.GetSpell(SpellSlot.W).Level - R1();
            var eL = Player.Instance.Spellbook.GetSpell(SpellSlot.E).Level - R1();
            var rL = Player.Instance.Spellbook.GetSpell(SpellSlot.R).Level - R1();

            var level = new[] { 0, 0, 0, 0 };
            if (qL + wL + eL + rL < Player.Instance.Level)
            {
                int[] LevelSet = CurrentLevelset.LevelsetData;

                if (LevelSet == null)
                {
                    if (Player.Instance.ChampionName.Equals("Ryze"))
                    {
                        LevelSet = MaxRyze;
                    }

                    if (MaxQChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        LevelSet = MaxQSequence;
                    }
                    if (MaxWChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        LevelSet = MaxWSequence;
                    }
                    if (MaxEChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        LevelSet = MaxESequence;
                    }
                }

                for (var i = 0; i < Player.Instance.Level; i++)
                {
                    if (LevelSet != null)
                    {
                        level[LevelSet[i] - 1] = level[LevelSet[i] - 1] + 1;
                    }
                }

                if (qL < level[0] && Player.Instance.Spellbook.CanSpellBeUpgraded(SpellSlot.Q))
                {
                    Player.LevelSpell(SpellSlot.Q);
                }
                if (wL < level[1] && Player.Instance.Spellbook.CanSpellBeUpgraded(SpellSlot.W))
                {
                    Player.LevelSpell(SpellSlot.W);
                }
                if (eL < level[2] && Player.Instance.Spellbook.CanSpellBeUpgraded(SpellSlot.E))
                {
                    Player.LevelSpell(SpellSlot.E);
                }
                if (rL < level[3] && Player.Instance.Spellbook.CanSpellBeUpgraded(SpellSlot.R))
                {
                    Player.LevelSpell(SpellSlot.R);
                }
            }
            
            if (Player.Instance.EvolvePoints > 0)
            {
                switch (I)
                {
                    case 0:
                        Player.EvolveSpell(SpellSlot.Q);
                        I++;
                        return;
                    case 1:
                        Player.EvolveSpell(SpellSlot.W);
                        I++;
                        return;
                    case 2:
                        Player.EvolveSpell(SpellSlot.E);
                        I++;
                        return;
                    case 3:
                        Player.EvolveSpell(SpellSlot.R);
                        I++;
                        return;
                }
            }
        }

        /// <summary>
        ///     Maxing Q Sequence.
        /// </summary>
        public static int[] MaxQSequence
        {
            get
            {
                return new[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 3, 2, 3, 4, 3, 3 };
            }
        }

        /// <summary>
        ///     Maxing W Sequence.
        /// </summary>
        public static int[] MaxWSequence
        {
            get
            {
                return new[] { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 3, 1, 3, 4, 3, 3 };
            }
        }

        /// <summary>
        ///     Maxing E Sequence.
        /// </summary>
        public static int[] MaxESequence
        {
            get
            {
                return new[] { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 2, 1, 2, 4, 2, 2 };
            }
        }

        public static int[] MaxRyze
        {
            get { return new[] { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 2, 1, 2, 1, 2, 2 }; }
        }

        /// <summary>
    ///     Maxing Q Champions.
    /// </summary>
    private static readonly List<string> MaxQChampions = new List<string>
        {
            "Ahri", "Akali", "Alistar", "Amumu", "Annie", "Ashe", "Azir", "Blitzcrank", "Bard", "Braum", "Caitlyn", "Cassiopeia", "ChoGath",
            "Corki", "Darius", "Diana", "DrMundo", "Draven", "Elise", "Ekko", "Evelynn", "Ezreal", "Fiora", "Fizz", "Galio", "Gangplank", "Gnar",
            "Gragas", "Graves", "Hecarim", "Heimerdinger", "Illaoi", "Irelia", "Ivern", "Janna", "JarvanIV", "Jax", "Jayce", "Jhin", "Jinx", "Karma", "Karthus",
            "Kassadin", "Katarina", "Kennen", "KhaZix", "Kindred", "Kled", "Leblanc", "LeeSin", "Leona", "Lissandra", "Lucian", "Lulu", "Malphite",
            "Malzahar", "MasterYi", "MissFortune", "Morgana", "Nami", "Nautilus", "Nidalee", "Nocturne", "Olaf", "Orianna", "Pantheon", "Poppy",
            "Quinn", "Rammus", "RekSai", "Renekton", "Rengar", "Riven", "Rumble", "Sejuani", "Shen", "Singed", "Sion", "Sivir", "Skarner",
            "Sona", "Soraka", "Swain", "Syndra", "TahmKench", "Taliyah", "Taric", "Teemo", "Thresh", "Tristana", "Trundle", "Tryndamere",
            "TwistedFate", "Udyr", "Urgot", "Varus", "Veigar", "Vi", "Vladimir", "VelKoz", "Warwick", "MonkeyKing", "Xerath", "XinZhao", "Yasuo",
            "Yorick", "Zac", "Zed", "Ziggs", "Zilean", "Zyra"
        };

        /// <summary>
        ///     Maxing W Champions.
        /// </summary>
        private static readonly List<string> MaxWChampions = new List<string> { "AurelionSol", "Brand", "KogMaw", "Malzahar", "Talon", "Vayne", "Volibear" };

        /// <summary>
        ///     Maxing E Champions.
        /// </summary>
        private static readonly List<string> MaxEChampions = new List<string>
        {
            "Aatrox", "Anivia", "Cassiopeia", "Fiddlesticks", "Garen", "Kalista", "Kayle", "Lux", "Maokai", "Mordekaiser", "Nasus", "Nunu",
            "Shaco", "Shyvana", "Twitch", "Viktor"
        };

        public static bool TryParseData(string data, out Levelset set)
        {
            try
            {
                dynamic parsed = JObject.Parse(data);
                
                int[] arr = parsed.LevelSet.ToObject<int[]>();
                set = new Levelset { LevelsetData = arr };
                return true;
            }
            catch (JsonSerializationException ex)
            {
                Logger.Send("Exception occurred in LevelSet on JSON parse:", ex, Logger.LogLevel.Error);
                
                Logger.Send("Exception occurred during LevelSet JSON parse. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
                set = null;
                if (Player.Instance.ChampionName.Equals("Ryze"))
                {
                    set = new Levelset(MaxRyze);
                }

                if (MaxQChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    set = new Levelset(MaxQSequence);
                }
                if (MaxWChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    set = new Levelset(MaxWSequence);
                }
                if (MaxEChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    set = new Levelset(MaxESequence);
                }
                return false;
            }
        }

        public class Levelset
        {
            public Levelset(int[] data = null)
            {
                if (data != null)
                    this.LevelsetData = data;
            }
            public int[] LevelsetData { get; set; }
        }
    }
}
