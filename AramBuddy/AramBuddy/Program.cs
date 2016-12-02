using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using AramBuddy.AutoShop;
using AramBuddy.MainCore;
using AramBuddy.MainCore.Logics;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using AramBuddy.Plugins.Champions;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Notifications;
using EloBuddy.SDK.Rendering;
using SharpDX;
using static AramBuddy.Config;
using Color = SharpDX.Color;
using Version = System.Version;

namespace AramBuddy
{
    internal class Program
    {
        public static Dictionary<string, string[]> CurrentPatchs = new Dictionary<string, string[]>
            {
            {"6.22.1", new []{ "MetaSrc", "LoLSkill", "KoreanBuilds", "Championgg", "User Builds" } },
            };

        public static List<BuildServices> BuildsServices = new List<BuildServices>();

        private static string Texturefile = Misc.AramBuddyFolder + "\\temp\\DisableTexture1.dat";

        public static bool CrashAIODetected;
        public static bool CustomChamp;
        public static bool Loaded;
        public static bool GameEnded;

        public static Version version = typeof(Program).Assembly.GetName().Version;
        public static int MoveToCommands;
        public static float Timer;
        private static float TimeToStart;

        public static string Moveto;

        public static Menu MenuIni, SpellsMenu, MiscMenu, BuildMenu, InfoMenu;

        private static float textsize;
        private static Text text;

        private static void Main()
        {
            if (File.Exists(Texturefile))
            {
                ManagedTexture.OnLoad += delegate(OnLoadTextureEventArgs texture)
                    {
                        if (Game.MapId == GameMapId.HowlingAbyss || Game.Type == GameType.ARAM)
                        {
                            Hacks.DisableTextures = true;
                            texture.Process = false;
                        }
                    };
            }

            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                if (Game.MapId != GameMapId.HowlingAbyss)
                {
                    Logger.Send(Game.MapId + " IS NOT Supported By AramBuddy !", Logger.LogLevel.Warn);
                    Chat.Print(Game.MapId + " IS NOT Supported By AramBuddy !");
                    return;
                }

                textsize = Drawing.Width <= 1280 || Drawing.Height <= 720 ? 10F : 40F;
                text = new Text("YOUR ORBWALKER IS DISABLED", new Font("Euphemia", textsize, FontStyle.Bold)) { Color = System.Drawing.Color.White, Position = new Vector2(Drawing.Width * 0.3f, Drawing.Height * 0.2f) };

                Chat.OnClientSideMessage += delegate (ChatClientSideMessageEventArgs eventArgs)
                {
                    if (eventArgs.Message.ToLower().Contains("portaio") && !CrashAIODetected)
                    {
                        var warnmsg = "AramBuddy Does not Work With CrashAIO\nDisable CrashAIO In order to use AramBuddy !";
                        Chat.Print(warnmsg);
                        Logger.Send(warnmsg, Logger.LogLevel.Warn);
                        Notifications.Show(new SimpleNotification("AramBuddy", warnmsg), 20000);
                        Drawing.OnEndScene += delegate
                            {
                                text.TextValue = warnmsg;
                                text.Position = new Vector2(Drawing.Width * 0.3f, Drawing.Height * 0.2f);
                                text.Draw();
                        };
                        CrashAIODetected = true;
                    }
                };
                
                // Creates The Menu
                CreateMenu();

                // Checks for updates
                //CheckVersion.Init();

                // Inits KappaEvade
                KappaEvade.Init();
                
                // Initialize the AutoShop.
                AutoShop.Setup.Init();
                
                Timer = Game.Time;
                TimeToStart = new Random().Next(10000, 30000) + Game.Ping;
                Game.OnTick += Game_OnTick;
                Events.OnGameEnd += Events_OnGameEnd;
                Player.OnPostIssueOrder += Player_OnPostIssueOrder;
                Drawing.OnEndScene += Drawing_OnEndScene;

                Logger.Send("Starting After: " + (TimeToStart / 1000).ToString("F1") + " Second/s", Logger.LogLevel.Event);
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Loading_OnLoadingComplete", ex, Logger.LogLevel.Error);
            }
        }
        
        private static void Player_OnPostIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            try
            {
                if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
                    MoveToCommands++;
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Player_OnPostIssueOrder", ex, Logger.LogLevel.Error);
            }
        }

        private static void Events_OnGameEnd(bool args)
        {
            try
            {
                GameEnded = true;
                if (QuitOnGameEnd)
                {
                    var rnd = new Random().Next(15000, 30000) + Game.Ping;
                    Logger.Send("Closing the Game in: " + (rnd / 1000).ToString("F1") + " Second/s", Logger.LogLevel.Event);
                    Core.DelayAction(() => Game.QuitGame(), rnd);
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Events_OnGameEnd", ex, Logger.LogLevel.Error);
            }
        }

        private static void Init()
        {
            try
            {
                if (Orbwalker.MovementDelay < 200)
                {
                    Orbwalker.MovementDelay += new Random().Next(200, 400);
                }
                
                if (AutoShop.Setup.CurrentChampionBuild.BuildData.Length > 0)
                {
                    var i = 0;
                    foreach (var item in AutoShop.Setup.CurrentChampionBuild.BuildData)
                    {
                        i++;
                        BuildMenu.AddLabel(i + " - " + item);
                    }
                }

                if (EnableCustomPlugins)
                {
                    try
                    {
                        if ((Base)Activator.CreateInstance(null, "AramBuddy.Plugins.Champions." + Player.Instance.Hero + "." + Player.Instance.Hero).Unwrap() != null)
                        {
                            CustomChamp = true;
                            Logger.Send("Loaded Custom Champion " + Player.Instance.Hero);
                        }
                    }
                    catch (Exception)
                    {
                        CustomChamp = false;
                        Logger.Send("There Is No Custom Plugin For " + Player.Instance.Hero, Logger.LogLevel.Warn);
                    }
                }

                // Sends Start / End Msg
                Chatting.Init();

                // Initialize Bot Functions.
                Brain.Init();
                
                // Inits Activator
                if (EnableActivator)
                    Plugins.Activator.Load.Init();
                
                Chat.Print("AramBuddy Loaded !");
                Chat.Print("AramBuddy Version: " + version);
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void CreateMenu()
        {
            try
            {
                foreach (var p in CurrentPatchs)
                {
                    BuildsServices.Add(new BuildServices(p.Key, p.Value));
                }

                MenuIni = MainMenu.AddMenu("AramBuddy", "AramBuddy");
                SpellsMenu = MenuIni.AddSubMenu("Spells");
                MenuIni.AddGroupLabel("AramBuddy Version: " + version);
                MenuIni.AddGroupLabel("AramBuddy Settings");

                BuildMenu = MenuIni.AddSubMenu("Current Build");
                InfoMenu = MenuIni.AddSubMenu("Extra Options");
                
                var lolversion = BuildMenu.Add("buildpatch", new ComboBox("Select Build Patch: ", 0, BuildsServices.Select(s => s.Patch).ToArray()));
                var buildsource = BuildMenu.Add("buildsource", new ComboBox("Builds Service: ", 0, BuildsServices[lolversion.CurrentValue].AvailableServices));

                lolversion.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        foreach (var i in BuildsServices[args.OldValue].AvailableServices)
                        {
                            buildsource.Remove(i);
                        }
                        foreach (var i in BuildsServices[args.NewValue].AvailableServices)
                        {
                            buildsource.Add(i);
                        }
                    };

                BuildMenu.AddLabel("MetaSrc: Very Good For Aram");
                BuildMenu.AddLabel("Championgg: Decent For Aram");
                BuildMenu.AddLabel("KoreanBuilds: Decent For Aram");
                BuildMenu.AddLabel("LoLSkill: Not Good For Aram");

                var debug = MenuIni.CreateCheckBox("debug", "Enable Debugging Mode");
                var activator = MenuIni.CreateCheckBox("activator", "Enable Built-In Activator");
                var DisableSpells = MenuIni.CreateCheckBox("DisableSpells", "Disable Built-in Casting Logic", false);
                var CustomPlugin = MenuIni.CreateCheckBox("CustomPlugin", "Enable Custom Plugins");
                var quit = MenuIni.CreateCheckBox("quit", "Quit On Game End");
                var stealhr = MenuIni.CreateCheckBox("stealhr", "Dont Steal Health Relics From Allies", false);
                var chat = MenuIni.CreateCheckBox("chat", "Send Start / End msg In-Game Chat", false);
                var texture = MenuIni.CreateCheckBox("texture1", "Disable In-Game Texture (Less RAM/CPU)", false);
                var evade = MenuIni.CreateCheckBox("evade", "Evade Integration[BETA]");
                var ff = MenuIni.CreateCheckBox("ff", "Vote Surrender With Team Always");
                var cameralock = MenuIni.CreateCheckBox("cameralock", "Lock Camera Always");

                MenuIni.AddSeparator(0);
                var Safe = MenuIni.CreateSlider("Safe1", "Safe Slider (Recommended 1250)", 1250, 0, 2500);
                //MenuIni.AddLabel("More Safe Value = more defensive playstyle");
                //MenuIni.AddSeparator(0);
                var HRHP = MenuIni.CreateSlider("HRHP", "Health Percent To Pick Health Relics (Recommended 75%)", 75);
                var HRMP = MenuIni.CreateSlider("HRMP", "Mana Percent To Pick Health Relics (Recommended 15%)", 15);
                MenuIni.AddSeparator(0);
                var Reset = MenuIni.CreateCheckBox("reset", "Reset All Settings To Default", false);

                // Misc Settings
                MiscMenu = MenuIni.AddSubMenu("Misc Settings");
                var autolvl = MiscMenu.CreateCheckBox("autolvl", "Enable AutoLvlUP");
                var autoshop = MiscMenu.CreateCheckBox("autoshop", "Enable AutoShop");
                var fixdive = MiscMenu.CreateCheckBox("fixdive", "Try to Fix Diving Towers");
                var kite = MiscMenu.CreateCheckBox("kite", "Try Kite Near Enemies");
                var ping = MiscMenu.CreateCheckBox("ping", "Move To AllySpawn On High Ping/Ping-Spikes");
                var bardchime = MiscMenu.CreateCheckBox("bardchime", "Pick Bard Chimes");
                var corkibomb = MiscMenu.CreateCheckBox("corkibomb", "Pick Corki Bomb");
                var dravenaxe = MiscMenu.CreateCheckBox("dravenaxe", "Pick Draves Axes");
                var olafaxe = MiscMenu.CreateCheckBox("olafaxe", "Pick Olaf Axe");
                var zacpassive = MiscMenu.CreateCheckBox("zacpassive", "Pick Zac Blops");
                var azirtower = MiscMenu.CreateCheckBox("azirtower", "Create Azir Towers");
                var teleport = MiscMenu.CreateCheckBox("tp", "Enable use of Teleporting Logic");
                var logs = MiscMenu.CreateCheckBox("logs", "Save AramBuddy Logs", false);
                var savechat = MiscMenu.CreateCheckBox("savechat", "Save In-Game Chat", false);
                var tyler1 = MiscMenu.CreateCheckBox("bigbrother", "Run it down mid", false);
                var tyler1g = MiscMenu.CreateSlider("gold", "Run it down mid if my Gold >= {0}", 3000, 500, 17500);

                Reset.OnValueChange += delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                {
                    if (args.NewValue)
                    {
                        Reset.CurrentValue = false;
                        debug.CurrentValue = true;
                        activator.CurrentValue = true;
                        DisableSpells.CurrentValue = false;
                        CustomPlugin.CurrentValue = true;
                        quit.CurrentValue = true;
                        stealhr.CurrentValue = false;
                        chat.CurrentValue = true;
                        texture.CurrentValue = false;
                        evade.CurrentValue = true;
                        ff.CurrentValue = true;
                        cameralock.CurrentValue = true;
                        Safe.CurrentValue = 1250;
                        HRHP.CurrentValue = 75;
                        HRMP.CurrentValue = 15;

                        // Misc
                        autolvl.CurrentValue = true;
                        autoshop.CurrentValue = true;
                        fixdive.CurrentValue = true;
                        kite.CurrentValue = true;
                        ping.CurrentValue = true;
                        bardchime.CurrentValue = true;
                        corkibomb.CurrentValue = true;
                        dravenaxe.CurrentValue = true;
                        olafaxe.CurrentValue = true;
                        zacpassive.CurrentValue = true;
                        azirtower.CurrentValue = true;
                        teleport.CurrentValue = true;
                        logs.CurrentValue = false;
                        savechat.CurrentValue = false;
                        tyler1.CurrentValue = false;
                        tyler1g.CurrentValue = 3000;
                    }
                };

                Safe.IsVisible = false; // disabled
                corkibomb.IsVisible = false; // disable for now

                SpellsMenu.AddGroupLabel("Built-In Casting Logic:");
                SpellsMenu.CreateCheckBox("combo", "Use Spells in Combo Mode");
                SpellsMenu.CreateCheckBox("harass", "Use Spells in Harass Mode");
                SpellsMenu.CreateCheckBox("flee", "Use Spells in Flee Mode");
                SpellsMenu.CreateCheckBox("laneclear", "Use Spells in Lane Clear Mode");
                SpellsMenu.AddSeparator(0);
                SpellsMenu.AddGroupLabel("SummonerSpells");
                SpellsMenu.Add("Heal", new CheckBox("Use Heal"));
                SpellsMenu.Add("Barrier", new CheckBox("Use Barrier"));
                SpellsMenu.Add("Clarity", new CheckBox("Use Clarity"));
                SpellsMenu.Add("Ghost", new CheckBox("Use Ghost"));
                SpellsMenu.Add("Flash", new CheckBox("Use Flash"));
                SpellsMenu.Add("Cleanse", new CheckBox("Use Cleanse"));

                InfoMenu.AddGroupLabel("Disabling In-Game Texture");
                InfoMenu.AddLabel("To Disable In-Game Texture Type In Chat \"Disable Texture\"");
                InfoMenu.AddLabel("To Enable In-Game Texture Type In Chat \"Enable Texture\"");
                InfoMenu.AddLabel("Takes affect after 1 or 2 games");

                Console.Title = $"{Drawing.Width}x{Drawing.Height}";

                texture.IsVisible = false;

                if (DisableTexture)
                    Disabletexture();
                else
                    Enabletexture();

                Chat.OnInput += delegate(ChatInputEventArgs args)
                    {
                        if (args.Input.ToLower().Contains("disable texture"))
                        {
                            Disabletexture();
                            texture.CurrentValue = true;
                            args.Process = false;
                        }
                        if (args.Input.ToLower().Contains("enable texture"))
                        {
                            Enabletexture();
                            texture.CurrentValue = false;
                            args.Process = false;
                        }
                    };
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At CreateMenu", ex, Logger.LogLevel.Error);
            }
        }

        private static void Enabletexture()
        {
            if (File.Exists(Texturefile))
            {
                File.Delete(Texturefile);
                Logger.Send("Texture Enabled !", Logger.LogLevel.Event);
            }
        }
        private static void Disabletexture()
        {
            Misc.CreateAramBuddyFile("DisableTexture1.dat", Misc.AramBuddyDirectories.Temp);
            Logger.Send("Texture Disabled !", Logger.LogLevel.Event);
        }
        
        private static void Drawing_OnEndScene(EventArgs args)
        {
            try
            {
                if(CrashAIODetected) return;

                if (!Loaded)
                {
                    text.TextValue = $"AramBuddy Starting in: {(int)((Timer*1000 + TimeToStart - Game.Time*1000)/1000) + 1}";
                    text.Position = new Vector2(Drawing.Width * 0.3f, Drawing.Height * 0.2f);
                    text.Draw();
                    return;
                }

                if (Orbwalker.DisableMovement && !MainCore.Logics.Casting.SpecialChamps.IsCastingImportantSpell)
                {
                    text.TextValue = "YOUR ORBWALKER IS DISABLED\nTHE BOT WILL NOT WORK\nMAKE SURE TO UNTICK\nDISABLE MOVING TO MOUSE";
                    text.Position = new Vector2(Drawing.Width * 0.3f, Drawing.Height * 0.2f);
                    text.Draw();
                }

                if (!EnableDebug)
                    return;

                var AllyTeamTotal = " | AllyTeamTotal: " + (int)Misc.TeamTotal(Player.Instance.PredictPosition());
                var EnemyTeamTotal = " | EnemyTeamTotal: " + (int)Misc.TeamTotal(Player.Instance.PredictPosition(), true);
                var MoveTo = " | MoveTo: " + Moveto;
                var ActiveMode = " | ActiveMode: " + ModesManager.CurrentMode;
                var Alone = " | Alone: " + Brain.Alone();
                var AttackObject = " | AttackObject: " + ModesManager.AttackObject;
                var LastTurretAttack = " | LastTurretAttack: " + (Core.GameTickCount - Brain.LastTurretAttack);
                var SafeToDive = " | SafeToDive: " + Misc.SafeToDive;
                var castingimportantspell = " | IsCastingImportantSpell: " + MainCore.Logics.Casting.SpecialChamps.IsCastingImportantSpell;
                var lagging = " | Lagging: " + Brain.Lagging;
                var normalbing = " | NormalPing: " + Brain.NormalPing;
                var SafeToAttack = " | SafeToAttack: " + Misc.SafeToAttack;
                var IsSafe = /*" | IsSafe: " + (Player.Instance.IsSafe() && Pathing.Position.IsSafe())*/ "";
                var LastTeamFight = " | LastTeamFight: " + (int)(Core.GameTickCount - Brain.LastTeamFight);
                var MovementCommands = " | Movement Commands Issued: " + MoveToCommands;
                var nextitem = " | Next Item: " + AutoShop.Sequences.Buy.CurrentItemIndex + " - " + AutoShop.Sequences.Buy.NextItem + " | Value: " + AutoShop.Sequences.Buy.NextItemValue;
                var fullbuild = " | FullBuild: " + AutoShop.Sequences.Buy.FullBuild;

                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0.025f, System.Drawing.Color.White,
                    AllyTeamTotal + EnemyTeamTotal + "\n"
                    + ActiveMode + Alone + AttackObject + "\n"
                    + SafeToDive + SafeToAttack + IsSafe + "\n"
                    + castingimportantspell + lagging + normalbing + "\n"
                    + LastTurretAttack + LastTeamFight + "\n"
                    + MovementCommands + MoveTo + "\n"
                    + nextitem + fullbuild + "\n");

                Drawing.DrawText(
                    Game.CursorPos.WorldToScreen().X + 50,
                    Game.CursorPos.WorldToScreen().Y,
                    System.Drawing.Color.Goldenrod,
                    (Misc.TeamTotal(Game.CursorPos) - Misc.TeamTotal(Game.CursorPos, true)).ToString(CultureInfo.CurrentCulture) + "\n" 
                    /*+ "KDA: " + Player.Instance.KDA() +" [" + Player.Instance.ChampionsKilled + ", " + Player.Instance.Assists + ", " + Player.Instance.Deaths + "]"*/,
                    5);

                foreach (var hr in ObjectsManager.HealthRelics.Where(h => h.IsValid && !h.IsDead))
                {
                    Circle.Draw(Color.GreenYellow, hr.BoundingRadius + Player.Instance.BoundingRadius, hr.Position);
                }
                
                if (Pathing.Position != null && Pathing.Position != Vector3.Zero && Pathing.Position.IsValid())
                {
                    Circle.Draw(Color.White, 100, Pathing.Position);
                }

                if (!DisableSpellsCasting && ModesManager.Spelllist != null)
                {
                    foreach (var spell in ModesManager.Spelllist.Where(s => s != null))
                    {
                        Circle.Draw(spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, (spell as Spell.Chargeable)?.MaximumRange ?? spell.Range, Player.Instance);
                    }
                }

                if (PickBardChimes)
                {
                    foreach (var chime in ObjectsManager.BardChimes.Where(c => Player.Instance.Hero == Champion.Bard && c.IsValid && !c.IsDead))
                    {
                        Circle.Draw(Color.Goldenrod, chime.BoundingRadius + Player.Instance.BoundingRadius, chime.Position);
                    }
                }

                if (EnableEvade)
                {
                    foreach (var trap in ObjectsManager.EnemyTraps)
                    {
                        Circle.Draw(Color.OrangeRed, trap.Trap.BoundingRadius * 3, trap.Trap.Position);
                    }
                    /*
                    foreach (var p in KappaEvade.dangerPolygons)
                    {
                        p.Draw(System.Drawing.Color.AliceBlue, 2);
                    }*/
                }

                if (Player.Instance.Hero == Champion.Zac)
                    ObjectsManager.ZacPassives.ForEach(p => Circle.Draw(Color.AliceBlue, 100, p));
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Drawing_OnEndScene", ex, Logger.LogLevel.Error);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            try
            {
                if (!Loaded)
                {
                    if ((Game.Time - Timer) * 1000 >= TimeToStart)
                    {
                        Loaded = true;

                        // Initialize The Bot.
                        Init();
                    }
                }
                else
                {
                    if ((!Player.Instance.IsZombie() && Player.Instance.IsDead) || GameEnded)
                    {
                        Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                        return;
                    }

                    Brain.Decisions();

                    /*
                    if (CameraLock && !Camera.Locked)
                    {
                        Camera.Locked = true;
                    }*/
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Game_OnTick", ex, Logger.LogLevel.Error);
            }
        }
    }
}
