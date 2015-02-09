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
    class Wukong
    {
        public const string ChampionName = "MonkeyKing";


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

        public Wukong()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 375);
            W = new Spell(SpellSlot.W, 20);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 500);


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
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
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
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt spells with R").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealE", "Killsteal with E").SetValue(false));


            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();



            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Game.PrintChat("<font color='#05F4F4'>Xin Zhao by Kysama loaded!</font>");
        }


        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && R.IsReady())
            {
                R.Cast();
            }
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);

            bool useQ = Config.Item("UseQ").GetValue<bool>();
            bool useT = Config.Item("Tiamat").GetValue<bool>();
            bool useH = Config.Item("Hydra").GetValue<bool>();
            bool useB = Config.Item("Blade").GetValue<bool>();
            bool useW = Config.Item("UseW").GetValue<bool>();
            bool useE = Config.Item("UseE").GetValue<bool>();
            bool useR = Config.Item("UseR").GetValue<bool>();


            if (target != null && useE && E.IsReady())
            {
                E.Cast(target);
            }

            if (target != null && useW && W.IsReady())
            {
                W.Cast();
            }

            if (target != null && useQ && Q.IsReady())
            {
                Q.Cast();
            }

            if (target != null && useT && tiamat.IsReady())
            {
                tiamat.Cast();
            }

            if (target != null && useH && hydra.IsReady())
            {
                hydra.Cast();
            }

            if (target != null && useB && blade.IsReady())
            {
                blade.Cast(target);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);


            var useRks = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (useRks)
                Killsteal();
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
        private static void Killsteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                    R.Cast();
            }
        }
        static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All,
                MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (minion.Count > 0)
            {
                var minions = minion[0];
                if (Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range))
                {
                    Q.Cast();
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
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var minions = mobs[0];
                if (Config.Item("UseQJungleClear").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range))
                {
                    Q.Cast();
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
