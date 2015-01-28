#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion


namespace KS
{
    class Amumu
    {
        public const string ChampionName = "Amumu";


        public static Orbwalking.Orbwalker Orbwalker;


        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static bool WAktiv = false;


        public static Menu Config;

        public static Obj_AI_Hero Player;

        public Amumu()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 525);

            Q.SetSkillshot(0.250f, 80, 2000, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);


            Config = new Menu(ChampionName, ChampionName, true);


            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));


            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);


            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoUlt", "Auto Ultimate").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("NR", "N. Enemy in Range to AutoUlt").SetValue(new Slider(1, 5, 0)));

            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWLaneClear", "Use W").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "Laneclear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseQJungleClear", "Use Q").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseWJungleClear", "Use W").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseEJungleClear", "Use E").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClearActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            Config.SubMenu("Interrupt").AddItem(new MenuItem("InterruptSpellsR", "Interrupt spells with R").SetValue(true));
            Config.SubMenu("Interrupt").AddItem(new MenuItem("InterruptSpellsQ", "Interrupt spells with Q").SetValue(true));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Qhitch", "Q HitChance")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));


            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();



            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Game.PrintChat("<font color='#05F4F4'>Amumu by Kysama loaded!</font>");
        }


        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpellsR").GetValue<bool>()) return;
            {

                if (Player.Distance(unit) < R.Range && R.IsReady())
                {
                    R.Cast();
                }
            }
            if (!Config.Item("InterruptSpellsQ").GetValue<bool>()) return;
            {

                if (Player.Distance(unit) < Q.Range && Q.IsReady())
                {
                    Q.CastIfHitchanceEquals(unit, HitChance.High);
                }
            }
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var qTarget = TargetSelector.GetTarget(Q.Range + Q.Width, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            bool useQ = Config.Item("UseQ").GetValue<bool>();
            bool useW = Config.Item("UseW").GetValue<bool>();
            bool useE = Config.Item("UseE").GetValue<bool>();
            bool useR = Config.Item("UseR").GetValue<bool>();


            var qLow = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 0;
            var qMedium = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 1;
            var qHigh = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 2;
            var qVeryHigh = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 3;

            if (qTarget != null && useQ && Q.IsReady() && Player.Distance(qTarget) < Q.Range)
            {
                if (qLow)
                    Q.Cast(qTarget);
                else if (qMedium)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                else if (qHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.High);
                else if (qVeryHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
            }

            if (eTarget != null && useE && E.IsReady())
                E.Cast();

            if (!WAktiv && wTarget != null && useW && W.IsReady())
                W.Cast();
        }

        static void LaneClear()
        {
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, 350, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);

            foreach (var vMinion in allMinionsQ)
            {
                var Qdamage = ObjectManager.Player.GetSpellDamage(vMinion, SpellSlot.Q) * 0.85;

                if (Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady() && Qdamage >= Q.GetHealthPrediction(vMinion))
                {
                    Q.Cast(vMinion.Position);
                }

                if (Config.Item("UseWLaneClear").GetValue<bool>() && W.IsReady() && !WAktiv && allMinionsW.Count > 2)
                {
                    W.Cast();
                }

                if (Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady() && allMinionsW.Count > 2)
                {
                    E.Cast();
                }
            }
        }

        static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                if (Config.Item("UseQJungleClear").GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast(mobs[0].Position);
                }
                if (!WAktiv && Config.Item("UseWJungleClear").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }
                if (Config.Item("UseEJungleClear").GetValue<bool>() && E.IsReady())
                {
                    E.Cast();
                }
            }

        }



        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);


            var useRks = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();
            var useQks = Config.Item("KillstealQ").GetValue<bool>() && Q.IsReady();


            if (Player.HasBuff("Despair"))
            {
                WAktiv = true;
            }
            else
            {
                WAktiv = false;
            }

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (Config.Item("JungleClearActive").GetValue<KeyBind>().Active)
                JungleClear();
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (useRks)
                RKillsteal();
            if (useQks)
                QKillsteal();
            if (Config.Item("AutoUlt").GetValue<bool>() && Utility.CountEnemiesInRange((int)R.Range) >= Config.Item("NR").GetValue<Slider>().Value && R.IsReady())
                R.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void RKillsteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                    R.Cast();
            }
        }
        private static void QKillsteal()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.High);
            }
        }
    }
}
