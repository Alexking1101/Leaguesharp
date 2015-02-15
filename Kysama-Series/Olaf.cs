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
    class Olaf
    {
        public const string ChampionName = "Olaf";


        public static Orbwalking.Orbwalker Orbwalker;


        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;


        public static Items.Item tiamat;
        public static Items.Item hydra;
        public static Items.Item blade;


        public static Menu Config;

        public static Obj_AI_Hero Player;

        public Olaf()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(.25f, 50f, 1600, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            tiamat = new Items.Item(3077, 375f);
            hydra = new Items.Item(3074, 375f);
            blade = new Items.Item(3153, 425f);


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
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(false));
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

            Config.AddSubMenu(new Menu("Item", "Item"));
            Config.SubMenu("Item").AddItem(new MenuItem("Hydra", "Use Hydra").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("Blade", "Use Blade").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("Tiamat", "Use Tiamat").SetValue(true));



            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Qhitch", "Q HitChance")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();



            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#05F4F4'>Olaf by Kysama loaded!</font>");
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            bool useQ = Config.Item("UseQ").GetValue<bool>();
            bool useT = Config.Item("Tiamat").GetValue<bool>();
            bool useH = Config.Item("Hydra").GetValue<bool>();
            bool useB = Config.Item("Blade").GetValue<bool>();
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
            {
                E.Cast(eTarget);
            }

            if (eTarget != null && useW && W.IsReady())
            {
                W.Cast();
            }

            if (qTarget != null && useR && R.IsReady())
            {
                R.Cast();
            }

            if (eTarget != null && useT && tiamat.IsReady())
            {
                tiamat.Cast();
            }

            if (eTarget != null && useH && hydra.IsReady())
            {
                hydra.Cast();
            }

            if (qTarget != null && useB && blade.IsReady())
            {
                blade.Cast(qTarget);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);


            var useQks = Config.Item("KillstealQ").GetValue<bool>() && Q.IsReady();
            var useEks = Config.Item("KillstealE").GetValue<bool>() && E.IsReady();

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (useQks)
                KillstealQ();
            if (useEks)
                KillstealE();
            if (Config.Item("JungleClearActive").GetValue<KeyBind>().Active)
                JungleClear();
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
        private static void KillstealQ()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health)
                    Q.Cast(hero);
            }
        }
        private static void KillstealE()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range)))
            {
                if (E.IsReady() && hero.Distance(ObjectManager.Player) <= E.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                    E.Cast(hero);
            }
        }
        static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (minion.Count > 0)
            {
                var minions = minion[0];
                if (Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range))
                {
                    Q.Cast(minions);
                }

                if (Config.Item("UseWLaneClear").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }
                if (Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions);
                }
                if (Config.Item("Tiamat").GetValue<bool>() && tiamat.IsReady())
                    tiamat.Cast();

                if (Config.Item("Hydra").GetValue<bool>() && hydra.IsReady())
                    hydra.Cast();
            }
        }
        static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var minions = mobs[0];
                if (Config.Item("UseQJungleClear").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range))
                {
                    Q.Cast(minions);
                }

                if (Config.Item("UseWJungleClear").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }
                if (Config.Item("UseEJungleClear").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions);
                }
                if (Config.Item("Tiamat").GetValue<bool>() && tiamat.IsReady())
                    tiamat.Cast();

                if (Config.Item("Hydra").GetValue<bool>() && hydra.IsReady())
                    hydra.Cast();
            }

        }

    }
}
