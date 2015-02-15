#region#region

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
    class Akali
    {
        public const string ChampionName = "Akali";


        public static Orbwalking.Orbwalker Orbwalker;


        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static SpellSlot IgniteSlot;

        public static Items.Item HEX;
        public static Items.Item DFG;
        public static Items.Item Cutlass;

        public static Menu Config;

        public static Obj_AI_Hero Player;

        public Akali()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            HEX = new Items.Item(3146, 700);
            DFG = new Items.Item(3128, 750);
            Cutlass = new Items.Item(3144, 450);


            Config = new Menu(ChampionName, ChampionName, true);


            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));


            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);


            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQF", "Lasthit Q").SetValue(true));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEF", "Lasthit E").SetValue(true));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClear", "Laneclear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Item", "Item"));
            Config.SubMenu("Item").AddItem(new MenuItem("HEX", "Use Hextech").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("DFG", "Use Deathfire Grasp").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("Cutlass", "Use Bilgewater Cutlass").SetValue(true));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));


            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(false));


            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#05F4F4'>Akali by Kysama loaded!</font>");
        }

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, 0);
            Orbwalker.SetAttack(!R.IsReady(0) && !Q.IsReady(0) && !E.IsReady(0) && Geometry.Distance(Player, target) < 800f);
            bool value = Config.Item("HEX").GetValue<bool>();
            bool value2 = Config.Item("DFG").GetValue<bool>();
            bool value3 = Config.Item("Cutlass").GetValue<bool>();
            bool useQ = Config.Item("UseQ").GetValue<bool>();
            bool useE = Config.Item("UseE").GetValue<bool>();
            bool useR = Config.Item("UseR").GetValue<bool>();
            if (target != null)
            {
                if (Geometry.Distance(Player, target) <= 800f)
                {
                    if (Geometry.Distance(Player, target) >= 630f && R.IsReady(0))
                    {
                        R.CastOnUnit(target, true);
                        if (useQ && Q.IsReady(0))
                        {
                            Q.CastOnUnit(target, true);
                        }
                        if (useE && E.IsReady(0))
                        {
                            E.CastOnUnit(target, true);
                        }
                    }
                    else
                    {
                        if (Q.IsReady(0) && Geometry.Distance(Player, target) <= 600f)
                        {
                            Q.CastOnUnit(target, true);
                            if (useR && R.IsReady(0))
                            {
                                R.CastOnUnit(target, true);
                            }
                            if (useE && E.IsReady(0))
                            {
                                E.CastOnUnit(target, true);
                            }
                        }
                        else
                        {
                            if (useR && R.IsReady(0))
                            {
                                R.CastOnUnit(target, true);
                                if (value && HEX.IsReady())
                                {
                                    HEX.Cast(target);
                                }
                            }
                            if (useE && E.IsReady(0))
                            {
                                E.CastOnUnit(target, true);
                            }
                            if (value2 && DFG.IsReady())
                            {
                                DFG.Cast(target);
                            }
                            if (value3 && Cutlass.IsReady())
                            {
                                Cutlass.Cast(target);
                            }
                        }
                    }
                }
                else
                {
                    if (Player.GetSpellDamage(target, SpellSlot.Q) > (double)target.Health)
                    {
                        Q.CastOnUnit(target, true);
                    }
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                Harass();
            if (Config.Item("LaneClear").GetValue<KeyBind>().Active)
                LaneClear();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (Config.Item("KillstealR").GetValue<bool>())
                Killsteal();
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
            var useR = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();
            if (useR)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
                {
                    if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                        Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                        R.CastOnUnit(hero, true);
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.E);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.R);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                Q.CastOnUnit(target);
            }
            if (Player.Distance(target) <= 325 && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }
        private static void LaneClear()
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            bool useQF = Config.Item("UseQF").GetValue<bool>();
            bool useEF = Config.Item("UseEF").GetValue<bool>();


            if (useQF && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance(minion) * 1000 / 1400)) <
                        0.75 * Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }
            }
            else if (useEF && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(E.Range) &&
                        minion.Health < 0.75 * Player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        E.CastOnUnit(minion);
                        return;
                    }
                }
            }
        }
    }
}