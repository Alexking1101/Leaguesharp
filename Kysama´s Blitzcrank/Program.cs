#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Blitzcrank
{
    internal class Program
    {
        public const string ChampionName = "Blitzcrank";

     
        public static Orbwalking.Orbwalker Orbwalker;

 
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell E;
        public static Spell R;


        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

    
            Q = new Spell(SpellSlot.Q, 1000);
            E = new Spell(SpellSlot.E, Player.AttackRange + 50);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(R);

         
            Config = new Menu("Kysama´s Blitzcrank", "Blitzcrank", true);


            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

       
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("Q", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("E", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("R", "Use R").SetValue(true));

            Config.SubMenu("Combo").AddItem(new MenuItem("space", "--- Additional ---"));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoUlti", "Auto Ultimate").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("GroupR", "N. Enemy in Range to AutoUlt").SetValue(new Slider(1, 5, 0)));

            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ActiveCombo", "Combo!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("KSR", "Killsteal with R").SetValue(false));

  
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();

          
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#08F5F8'>Blitzcrank by Kysama loaded!</font>");
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            bool useQ = Config.Item("Q").GetValue<bool>();
            bool useE = Config.Item("E").GetValue<bool>();
            bool useR = Config.Item("R").GetValue<bool>();

        
            if (qTarget != null && useQ && Q.IsReady())
                Q.Cast(qTarget);

     
            if (qTarget != null && useE && E.IsReady())
            {
                if (qTarget.HasBuff("RocketGrab"))
                    E.Cast();
            }


            if (eTarget != null && useE && E.IsReady() && !Q.IsReady()) E.Cast();

            if (rTarget != null && !Q.IsReady() && useR && R.IsReady()) R.Cast(rTarget, false, true);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);

            var useRKS = Config.Item("KSR").GetValue<bool>() && R.IsReady();

            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
                Combo();
            if (useRKS)
                Killsteal();
            if (Config.Item("AutoUlti").GetValue<bool>() && Utility.CountEnemysInRange((int)R.Range) >= Config.Item("GroupR").GetValue<Slider>().Value && R.IsReady())
                R.Cast();

            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
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
    }
}