using System;
using System.Linq;
using AramBuddy.MainCore.Utility.MiscUtil.Caching;
using AramBuddy.Plugins.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using static AramBuddy.MainCore.Utility.MiscUtil.Misc;
using static AramBuddy.Plugins.Champions.Orianna.BallManager;

namespace AramBuddy.Plugins.Champions.Orianna
{
    internal class Orianna : Base
    {
        private static float BallRange = 1200;
        public static Spell.Skillshot QR { get; }
        static Orianna()
        {
            QR = new Spell.Skillshot(SpellSlot.Q, 820, SkillShotType.Circular, 450, 1400, 350, DamageType.Magical) { AllowedCollisionCount = int.MaxValue };

            BallManager.Init();

            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            LaneClearMenu.Add("Qfarm", new ComboBox("Q Farm Logic", 0, "Circular Area", "Linear Logic"));

            SpellList.ForEach(
                i =>
                {
                    if (i != R)
                    {
                        ComboMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                        HarassMenu.CreateCheckBox(i.Slot, "Use " + i.Slot);
                        HarassMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        HarassMenu.AddSeparator(0);
                        LaneClearMenu.CreateCheckBox(i.Slot, "Use " + i.Slot, i != E);
                        LaneClearMenu.CreateSlider(i.Slot + "hit", i.Slot + " Hit {0} Minions", 3, 1, 20);
                        LaneClearMenu.CreateSlider(i.Slot + "mana", i.Slot + " Mana Manager {0}%", 60);
                        LaneClearMenu.AddSeparator(0);
                    }
                    KillStealMenu.CreateCheckBox(i.Slot, i.Slot + " KillSteal");
                });

            AutoMenu.CreateCheckBox("flee", "Enable Smart Flee");
            AutoMenu.CreateCheckBox("fleeenemy", "Use Flee Only When Enemies Near", false);
            AutoMenu.CreateCheckBox("blockR", "Block R if no Hits");
            AutoMenu.CreateCheckBox("Ehelp", "Use E For Anti-Gapcloser & Interrupter");
            AutoMenu.CreateCheckBox("Rgap", "Auto R Anti-Gapcloser");
            AutoMenu.CreateCheckBox("Rint", "Auto R Interrupter");
            AutoMenu.CreateCheckBox("aoeR", "Auto R AOE");
            AutoMenu.CreateSlider("Raoe", "Auto R AOE Hit {0} Enemies", 3, 1, 6);

            ComboMenu.CreateCheckBox("R", "R Combo Finisher");
            ComboMenu.CreateCheckBox("aoeR", "R AOE");
            ComboMenu.CreateCheckBox("QR", "Use Q for Positon R AoE");
            ComboMenu.CreateSlider("Whit", "W Hit {0} Enemies", 1, 1, 6);
            ComboMenu.CreateSlider("Raoe", "R AOE Hit {0} Enemies", 2, 1, 6);

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
                return;

            if (args.Slot == SpellSlot.R)
            {
                if (AutoMenu.CheckBoxValue("blockR") && !RWillHit())
                    args.Process = false;
            }
        }

        public override void Active()
        {
            if (Q.SetSkillshot().SourcePosition != MyBall?.ServerPosition)
                Q.SetSkillshot().SourcePosition = MyBall?.ServerPosition;
            QR.SourcePosition = Q.SetSkillshot().SourcePosition;

            BetaAntiGapCloser();
            BetInterrupter();

            if (R.IsReady() && AutoMenu.CheckBoxValue("aoeR"))
            {
                RAOE(AutoMenu.SliderValue("Raoe"));
            }
        }

        public override void Combo()
        {
            var qtarget = Q.GetTarget();
            var rtarget = MyBall == null ? null : EntityManager.Heroes.Enemies.FirstOrDefault(e => RWillHit(e) && e.IsKillable() && R.WillKill(e));
            var etarget = EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(e => e.IsKillable(BallRange) && EHit(e) != null);
            var rhit = ComboMenu.SliderValue("Raoe");
            var QReady = Q.IsReady() && ComboMenu.CheckBoxValue("Q");
            var WReady = W.IsReady() && ComboMenu.CheckBoxValue("W");
            var EReady = E.IsReady() && ComboMenu.CheckBoxValue("E");
            var RReady = R.IsReady() && ComboMenu.CheckBoxValue("R");

            if (R.IsReady())
            {
                if (rtarget != null && RReady)
                {
                    RCast(rtarget);
                    return;
                }
                if (ComboMenu.CheckBoxValue("aoeR"))
                {
                    if (RAOE(rhit))
                    {
                        // the check does everything
                    }
                    else if (AllyForR(rhit) != null && EReady)
                    {
                        E.Cast(AllyForR(rhit));
                        return;
                    }
                    else if (ComboMenu.CheckBoxValue("QR"))
                    {
                        QRAoE(rhit);
                    }
                }
            }

            if (qtarget != null)
            {
                if (QReady)
                {
                    QCast(qtarget);
                }
            }
            if (WReady && EntityManager.Heroes.Enemies.Count(e => WHit(e)) >= ComboMenu.SliderValue("Whit"))
            {
                W.Cast();
            }

            if (EReady && etarget != null)
            {
                if (EHit(etarget) != null)
                {
                    E.Cast(EHit(etarget));
                }
            }

            var WRtarget = EntityManager.Heroes.Enemies.Where(WRKill).FirstOrDefault(e => e.IsKillable() && WHit(e));
            if (WRtarget != null)
            {
                if (WReady && RReady && RCast(WRtarget))
                {
                    W.Cast();
                }
            }
        }

        public override void Harass()
        {
            var qtarget = Q.GetTarget();
            var etarget = EntityManager.Heroes.Enemies.OrderByDescending(TargetSelector.GetPriority).FirstOrDefault(e => e.IsKillable(BallRange) && EHit(e) != null);

            var Qmana = HarassMenu.CompareSlider("Qmana", user.ManaPercent);
            var Wmana = HarassMenu.CompareSlider("Wmana", user.ManaPercent);
            var Emana = HarassMenu.CompareSlider("Emana", user.ManaPercent);

            var QReady = Q.IsReady() && HarassMenu.CheckBoxValue("Q") && Qmana;
            var WReady = W.IsReady() && HarassMenu.CheckBoxValue("W") && Wmana;
            var EReady = E.IsReady() && HarassMenu.CheckBoxValue("E") && Emana;

            if (qtarget != null)
            {
                if (QReady)
                {
                    QCast(qtarget);
                }
            }
            if (WReady && EntityManager.Heroes.Enemies.Count(e => WHit(e)) >= 1)
            {
                W.Cast();
            }

            if (EReady && etarget != null)
            {
                if (EHit(etarget) != null)
                {
                    E.Cast(EHit(etarget));
                }
            }
        }

        public override void LaneClear()
        {
            var QReady = Q.IsReady() && LaneClearMenu.CheckBoxValue("Q") && LaneClearMenu.CompareSlider("Qmana", user.ManaPercent);
            var WReady = W.IsReady() && LaneClearMenu.CheckBoxValue("W") && LaneClearMenu.CompareSlider("Wmana", user.ManaPercent);
            var EReady = E.IsReady() && LaneClearMenu.CheckBoxValue("E") && LaneClearMenu.CompareSlider("Emana", user.ManaPercent);
            var qhits = LaneClearMenu.SliderValue("Qhit");
            var whits = LaneClearMenu.SliderValue("Whit");
            var ehits = LaneClearMenu.SliderValue("Ehit");

            if (QReady)
            {
                var qfarmloc = Q.SetSkillshot().GetBestCircularCastPosition(Q.Enemies());
                if (LaneClearMenu.ComboBoxValue("Qfarm") == 1)
                    qfarmloc = Q.SetSkillshot().GetBestLinearCastPosition(Q.Enemies(), 0, Q.SetSkillshot().SourcePosition.GetValueOrDefault(user.ServerPosition).To2D());
                if (qfarmloc.HitNumber >= qhits)
                {
                    Q.Cast(qfarmloc.CastPosition);
                }
            }
            if (WReady)
            {
                var whit = EntityManager.Enemies.Count(e => WHit(e));
                if (whit >= whits)
                {
                    W.Cast();
                }
            }
            if (EReady)
            {
                var etarget = EFarmTarget(ehits);
                if (etarget != null)
                {
                    E.Cast(etarget);
                }
            }
        }

        public override void Flee()
        {
            if (AutoMenu.CheckBoxValue("flee"))
            {
                if (AutoMenu.CheckBoxValue("fleeenemy") && user.CountEnemyHeroesInRangeWithPrediction(1250) > 0 || !AutoMenu.CheckBoxValue("fleeenemy"))
                {
                    if (W.IsReady())
                    {
                        if (WHit(user) || MyBall != null && MyBall.ServerPosition.Equals(user.ServerPosition))
                        {
                            W.Cast();
                        }
                        else
                        {
                            if (E.IsReady() && user.Mana > W.ManaCost + E.ManaCost)
                            {
                                E.Cast(user);
                            }
                        }
                    }
                }
            }
        }

        public override void KillSteal()
        {
            var Qtarget = Q.GetKillStealTarget();
            var qready = Q.IsReady() && KillStealMenu.CheckBoxValue("Q") && Qtarget != null;
            var Wtarget = W.GetKillStealTargets().FirstOrDefault(e => W.WillKill(e) && WHit(e));
            var wready = W.IsReady() && KillStealMenu.CheckBoxValue("W") && Wtarget != null;
            var Etarget = E.GetKillStealTargets().FirstOrDefault(e => EHit(e) != null);
            var eready = E.IsReady() && KillStealMenu.CheckBoxValue("E") && Etarget != null;
            var Rtarget = R.GetKillStealTargets().FirstOrDefault(RWillHit);
            var rready = R.IsReady() && KillStealMenu.CheckBoxValue("R") && Rtarget != null;

            if (qready)
            {
                Q.Cast(Qtarget);
            }
            if (wready)
            {
                W.Cast();
            }
            if (eready)
            {
                E.Cast(EHit(Etarget));
            }
            if (rready)
            {
                R.Cast();
            }
        }

        private static void BetaAntiGapCloser()
        {
            if (!AutoMenu.CheckBoxValue("Rgap") || !R.IsReady())
                return;

            if (user.HealthPercent > 50 && user.CountEnemyHeroesInRangeWithPrediction(1250) < 2)
                return;

            foreach (var gapcloser in Cache.GapclosersCache.Where(g => g.Sender.IsKillable(BallRange) && g.Sender.IsEnemy))
            {
                var caster = gapcloser.Sender;
                if (RWillHit(caster))
                {
                    RCast();
                    break;
                }

                bool CanER = false;
                AIHeroClient EAlly = AllyNear(gapcloser.Args.End);
                if (E.IsReady())
                {
                    CanER = user.Mana > E.Mana() + R.Mana() && EAlly != null && AutoMenu.CheckBoxValue("Ehelp");
                }
                if (CanER)
                {
                    E.Cast(EAlly);
                    break;
                }
            }
        }
        private static void BetInterrupter()
        {
            if (!AutoMenu.CheckBoxValue("Rint") || !R.IsReady())
                return;

            foreach (var interruptable in Cache.InteruptablesCache.Where(i => i.Sender.IsKillable() && i.Sender.IsEnemy && (i.Args.DangerLevel > DangerLevel.Low || user.WillDie())))
            {
                var caster = interruptable.Sender as AIHeroClient;
                if (caster == null)
                    break;

                if (RWillHit(caster))
                {
                    RCast();
                    break;
                }
                bool CanER = false;
                AIHeroClient EAlly = AllyNear(caster);
                if (E.IsReady())
                {
                    CanER = user.Mana > E.Mana() + R.Mana() && EAlly != null && AutoMenu.CheckBoxValue("Ehelp");
                }
                if (CanER)
                {
                    E.Cast(EAlly);
                    break;
                }
            }
        }
        private static bool RAOE(int hitcount)
        {
            if (RWillHit(null, hitcount) && R.IsReady())
            {
                return RCast();
            }
            return false;
        }

        private static AIHeroClient AllyForR(int hitcount)
        {
            if (Cache.GapclosersCache.Count(g => g.Sender.IsAlly) == 0)
            {
                return
                    EntityManager.Heroes.Allies.FirstOrDefault(
                        a => MyBall?.ServerPosition != a.ServerPosition && a.IsValidTarget(E.Range) && CurrentRHits(a.ServerPosition) > CurrentRHits(MyBall?.ServerPosition));
            }
            return Cache.GapclosersCache.Where(a => a.Sender.IsAlly)
                .OrderByDescending(a => a.Args.End.CountEnemyHeroesInRangeWithPrediction(250 + Game.Ping))
                .FirstOrDefault(a => a.Sender.IsValidTarget(E.Range) && RWillHit(a.Args.End, hitcount) && a.Args.End.IsInRange(user, BallRange))?.Sender;
        }

        private static AIHeroClient AllyNear(Vector3 pos)
        {
            return EntityManager.Heroes.Allies.OrderBy(a => a.Distance(pos)).FirstOrDefault(a => a.IsValidTarget(E.Range) && a.PredictPosition().IsInRange(user, BallRange) && a.PredictPosition().IsInRange(pos, R.Range));
        }

        private static AIHeroClient AllyNear(Obj_AI_Base target)
        {
            return AllyNear(target.PredictPosition());
        }

        private static int CurrentRHits(Vector3? pos)
        { return pos == null ? 0 : EntityManager.Heroes.Enemies.Count(e => e.IsKillable() && RHit(e)); }
        private static int PredictedRHits(Vector3? pos)
        { return pos == null ? 0 : EntityManager.Heroes.Enemies.Count(e => e.IsKillable() && RPredHit(e)); }

        private static bool RWillHit(Vector3? sourcepos = null, int hitcount = 0)
        {
            if (sourcepos == null)
                sourcepos = MyBall?.ServerPosition;
            if (sourcepos == null)
                return false;

            var pos = sourcepos.Value;
            var nohits = PredictedRHits(pos) == 0 && CurrentRHits(pos) == 0;
            if (nohits)
                return false;

            if (hitcount == 0)
                return !nohits;

            var totalhits = EntityManager.Heroes.Enemies.Count(e => e.IsKillable() && RWillHit(e));
            return CurrentRHits(pos) >= hitcount && PredictedRHits(pos) >= hitcount && totalhits >= hitcount && PredictedRHits(pos) >= CurrentRHits(pos);
        }

        private static bool RWillHit(AIHeroClient target)
        {
            if (MyBall == null || target.WillDie() || !target.IsKillable() || target.IsGapClosing() && !MyBall.IsInRange(target.GapCloseEndPos(), R.Range))
                return false;

            if (target.IsCC())
                return true;

            return RPredHit(target) && RHit(target);
        }
        private static bool RPredHit(AIHeroClient target)
        {
            if (MyBall == null || target == null || target.IsDashing())
                return false;

            return target.PredictPosition(400 + Game.Ping).IsInRange(MyBall, R.Range);
        }
        private static bool RHit(AIHeroClient target)
        {
            if (MyBall == null || target == null || target.IsDashing())
                return false;
            return target.ServerPosition.IsInRange(MyBall, R.Range);
        }

        private static bool WRKill(Obj_AI_Base target)
        {
            return WRDmg(target) > target.TotalShieldHealth();
        }
        private static float WRDmg(Obj_AI_Base target)
        {
            if (W.Mana() + R.Mana() > user.Mana || MyBall == null || target == null || !target.IsKillable() || !WHit(target))
                return 0;

            var dmg = 0f;

            if (W.IsReady())
            {
                dmg += user.GetSpellDamage(target, SpellSlot.W);
            }
            if (R.IsReady())
            {
                dmg += user.GetSpellDamage(target, SpellSlot.R);
            }
            return dmg;
        }

        private static bool WHit(Obj_AI_Base target, bool checkbuffs = true)
        {
            if (MyBall == null || target == null || (checkbuffs && !target.IsKillable() || !checkbuffs))
                return false;
            return target.PredictPosition().IsInRange(MyBall, W.Range) && target.ServerPosition.IsInRange(MyBall, W.Range);
        }

        private static bool ROverKill(AIHeroClient target)
        {
            if (user.WillDie())
            {
                return false;
            }

            if (target.HasBuff("SummonerDot") || target.HasBuff("SummonerIgnite"))
                return true;
            if (Q.IsReady() && Q.WillKill(target) && target.IsKillable(Q.Range))
                return true;
            if (W.IsReady() && W.WillKill(target) && WHit(target))
                return true;
            if (E.IsReady() && E.WillKill(target) && EHit(target) != null)
                return true;
            if (target.AlliesAADamage() > target.TotalShieldHealth())
                return true;

            return false;
        }

        private static bool RCast(AIHeroClient target = null)
        {
            if (target != null && ROverKill(target))
                return false;

            if (R.IsReady() && RWillHit())
            {
                return R.Cast();
            }
            return false;
        }

        private static AIHeroClient EHit(Obj_AI_Base target)
        {
            if (target == null || MyBall == null)
                return null;
            return EntityManager.Heroes.Allies.OrderBy(a => a.Distance(target))
                .FirstOrDefault(a => a.IsValidTarget(E.Range) && ERect(a).IsInside(target));
        }

        private static AIHeroClient EFarmTarget(int hits)
        {
            if (MyBall == null)
                return null;
            return EntityManager.Heroes.Allies.Where(a => E.IsInRange(a) && a.IsValidTarget()).FirstOrDefault(a => E.LaneMinions().Count(e => ERect(a).IsInside(e)) >= hits);
        }

        private static Geometry.Polygon.Rectangle ERect(Obj_AI_Base end)
        {
            return new Geometry.Polygon.Rectangle(MyBall.ServerPosition, end.ServerPosition, MyBall.BoundingRadius);
        }
        private static void QCast(Obj_AI_Base target, int hitchance = 30)
        {
            if (target == null)
                return;

            var qpred = QPred(target);
            if (qpred.HitChancePercent >= hitchance)
            {
                Q.Cast(qpred.CastPosition);
            }
            else
            {
                if (qpred.HitChance <= HitChance.Impossible
                    && (target.IsCC() || target.Distance(user) <= 250
                    || (MyBall != null && target.Distance(MyBall.ServerPosition) <= 250)))
                {
                    Q.Cast(target.ServerPosition);
                }
            }
        }

        private static PredictionResult QPred(Obj_AI_Base target)
        {
            var data = new Prediction.Position.PredictionData(Prediction.Position.PredictionData.PredictionType.Linear,
                (int)Q.Range, Q.SetSkillshot().Width, Q.SetSkillshot().ConeAngleDegrees, Q.CastDelay, Q.SetSkillshot().Speed, int.MaxValue, MyBall?.ServerPosition);
            var qpred = Prediction.Position.GetPrediction(target, data);
            return qpred;
        }

        private static bool QRAoE(int hits)
        {
            if (!Q.IsReady() || !R.IsReady())
                return false;
            var targets = EntityManager.Heroes.Enemies.Where(e => e.IsKillable(Q.Range + R.Range / 2f));
            var possiblepos = QR.GetBestCircularCastPosition(targets);
            if (possiblepos.HitNumber >= hits)
            {
                return Q.Cast(possiblepos.CastPosition);
            }
            return false;
        }
    }

    internal static class BallManager
    {
        private static string[] BuffNames = new[] { "OrianaRedactShield", "OrianaGhostSelf", "OrianaGhost" };
        private static string BallBaseSkinName = "OriannaBall";
        private static string BallMissileName = "OrianaIzuna";
        internal static Obj_AI_Base MyBall;
        internal static void Init()
        {
            MyBall =
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(o => o.HasBuff("OrianaGhostSelf") || o.HasBuff("OrianaGhost") && (o.GetBuff("OrianaGhost").Caster.IsMe || o.GetBuff("OrianaGhostSelf").Caster.IsMe));
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.Q)
                    MyBall = null;
                if (args.Slot == SpellSlot.E && args.Target != null)
                    MyBall = (Obj_AI_Base)args.Target;
            }
        }

        private static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (args.Buff.Caster.IsMe && BuffNames.Contains(args.Buff.DisplayName))
                MyBall = sender;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;
            if (missile != null && missile.SpellCaster.IsMe && missile.SData.Name.Equals(BallMissileName))
                MyBall = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)missile.NetworkId);

            if (EntityManager.Heroes.AllHeroes.Count(a => a.ChampionName.Equals("Orianna")) > 1)
                return;

            var ball = sender as Obj_AI_Base;
            if (ball != null && ball.IsAlly && ball.BaseSkinName.Equals(BallBaseSkinName))
            {
                MyBall = ball;
            }
        }
    }
}
