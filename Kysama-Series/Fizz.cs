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
    class Fizz
    {
        public const string ChampionName = "Fizz";


        public static Orbwalking.Orbwalker Orbwalker;


        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell E2;
        public static Spell R;

        private static SpellSlot IgniteSlot;

        private static Vector3 _movePos;
        public static Dictionary<Vector3, Vector3> positions;
        public static int JumpState;
        public static float Time;   // Credits to hydralol
        public static bool Called;

        public static Menu Config;

        public static Obj_AI_Hero Player;

        public Fizz()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 400);
            E2 = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1150);

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

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

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("LastHit", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("UseQLastHit", "Use Q").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("UseELastHit", "Use E").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitActive", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "Laneclear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseQJungleClear", "Use Q").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseWJungleClear", "Use W").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseEJungleClear", "Use E").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClearActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Coord", "Coordinates").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealE", "Killsteal with E").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealI", "Killsteal with Ignite").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Rhitch", "R HitChance")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));
            Config.SubMenu("Misc").AddItem(new MenuItem("FleeActive", "Flee!(In Work)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(
                new MenuItem("drawFlee", "Draw Flee Spots(In Work)").SetValue(new Circle(true, Color.Cyan)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Drawing_damage", "Combo Damage Indicator").SetValue(true));
            Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;


            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#05F4F4'>Fizz by Kysama loaded!</font>");
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range + E2.Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var comboDamage = GetComboDamage(qTarget);

            bool useQ = Config.Item("UseQ").GetValue<bool>();
            bool useW = Config.Item("UseW").GetValue<bool>();
            bool useE = Config.Item("UseE").GetValue<bool>();
            bool useR = Config.Item("UseR").GetValue<bool>();

            var rLow = Config.Item("Rhitch").GetValue<StringList>().SelectedIndex == 0;
            var rMedium = Config.Item("Rhitch").GetValue<StringList>().SelectedIndex == 1;
            var rHigh = Config.Item("Rhitch").GetValue<StringList>().SelectedIndex == 2;
            var rVeryHigh = Config.Item("Rhitch").GetValue<StringList>().SelectedIndex == 3;

            if (comboDamage > rTarget.Health && IgniteSlot != SpellSlot.Unknown &&
              Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
             {
                Player.Spellbook.CastSpell(IgniteSlot, qTarget);
             }

            if (rTarget != null && useR && R.IsReady() && Player.Distance(rTarget) < R.Range)
                {
                    if (rLow)
                        R.Cast(qTarget);
                    else if (rMedium)
                        R.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                    else if (rHigh)
                        R.CastIfHitchanceEquals(qTarget, HitChance.High);
                    else if (rVeryHigh)
                        R.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
                }
            

            if (qTarget != null && useQ &&
                (qTarget.IsValidTarget(Q.Range) && Q.IsReady()))
            {
                Q.Cast(qTarget);
            }

            if (eTarget != null && useW && W.IsReady())
            {
                W.Cast();
            }

            if (eTarget != null && useE && (eTarget.IsValidTarget(800) && E.IsReady()))
            {
                if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < 800 && JumpState != 1 &&
                    E.GetPrediction(eTarget).Hitchance >= HitChance.High)
                {
                    E.Cast(eTarget, true);
                }

                if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range &&
                    Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) > 300 && JumpState == 1 &&
                    E2.GetPrediction(eTarget).Hitchance >= HitChance.High)
                {
                    E2.Cast(eTarget, true);
                }
            }

        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);

            fillPositions();

            if (Time + 1f < Game.Time && !Called)
            {
                Called = true;
                JumpState = 0;
            }

            var useQks = Config.Item("KillstealQ").GetValue<bool>() && Q.IsReady();
            var useEks = Config.Item("KillstealE").GetValue<bool>() && E.IsReady();
            var useRks = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();
            var useIks = Config.Item("KillstealI").GetValue<bool>() && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                Harass();
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (Config.Item("LastHitActive").GetValue<KeyBind>().Active)
                LastHit();
            if (Config.Item("Coord").GetValue<KeyBind>().Active)
                Coord();
            if (useQks)
                KillstealQ();
            if (useEks)
                KillstealE();
            if (useRks)
                KillstealR();
            if (useIks)
                KillstealI();
            if (Config.Item("JungleClearActive").GetValue<KeyBind>().Active)
                JungleClear();
            if (Config.Item("FleeActive").GetValue<KeyBind>().Active)
                Flee();
       //         QFlee();
        }

        private static void Coord()
        {
            Game.PrintChat(ObjectManager.Player.Position.ToString());
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var pos0 = new Vector3(8216f, 3146f, 51.59694f);

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
            Render.Circle.DrawCircle(pos0, 100, Color.Red, 50);

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
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range + E2.Range)))
            {
                if (E.IsReady() && hero.Distance(ObjectManager.Player) <= E.Range + E2.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                {
                    if (JumpState == 0 && Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump")
                    {
                        E.Cast(hero, true);
                    }
                    if (JumpState != 0 && Player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo")
                    {
                        E2.Cast(hero, true);
                    }
                }
            }
        }
        private static void KillstealR()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                    R.Cast(hero);
            }
        }
        private static void KillstealI()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && hero.Distance(ObjectManager.Player) <= Q.Range &&
                    Player.GetSpellDamage(hero, IgniteSlot) >= hero.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, hero);
                }
            }
        }
        static void LaneClear()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, E.Range);
            if (Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady())
            {
                foreach (
                    Obj_AI_Base minion in
                        allMinions.Where(minion => minion.IsValidTarget()).Where(
                            minion => Player.Distance(minion) < Q.Range))
                {
                    Q.Cast(minion, true);
                }
            }
            if (Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady())
            {
                MinionManager.FarmLocation bestLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Player.Position, 800).Select(minion => minion.ServerPosition.To2D())
                            .ToList(), E.Width, 800);
                if (Player.Distance(bestLocation.Position) < E.Range)
                {
                    if (JumpState == 1 && Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump")
                    {
                        E.Cast(bestLocation.Position, true);
                    }
                    if (JumpState != 1 && Player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo")
                    {
                        E2.Cast(bestLocation.Position, true);
                    }
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.W);

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

            Obj_AI_Turret closestTower =
            ObjectManager.Get<Obj_AI_Turret>()
            .Where(tur => tur.IsAlly)
            .OrderBy(tur => tur.Distance(Player.Position))
            .First();

            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (qTarget != null && Config.Item("UseQHarass").GetValue<bool>() &&
                (qTarget.IsValidTarget(Q.Range) && Q.IsReady()))
            {
                Q.Cast(qTarget);
            }

            if (qTarget != null && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
            }
            if (E.IsReady() && Config.Item("UseEHarass").GetValue<bool>())
               {
                   sendMovementPacket(closestTower.ServerPosition.To2D());
                   if (JumpState != 1)
                   {
                       E.Cast(closestTower.ServerPosition);
                   }
                   if (JumpState == 1)
                   {
                       E2.Cast(closestTower.ServerPosition);
                   }
                }
              // Had same Harass idea but didn´t know how to script it. Credits to DZ191
        }


        static void LastHit()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, 800);
            var useQlh = Config.Item("UseQLastHit").GetValue<bool>();
            var useElh = Config.Item("UseELastHit").GetValue<bool>();

            foreach (var min in minions.Where(min => Q.IsReady() && useQlh && Q.GetDamage(min) >= min.Health))
            {
                Q.Cast(min);
            }
            foreach (var min in minions.Where(min => E.IsReady() && useElh && E.GetDamage(min) >= min.Health))
            {
                E.Cast(min);
                E2.Cast(min);
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
                if (Config.Item("UseEJungleClear").GetValue<bool>() && E.IsReady())
                    if (Player.Distance(minions.Position) < E.Range)
                    {
                        if (JumpState == 0 && Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump")
                        {
                            E.Cast(minions.Position, true);
                        }
                        if (JumpState != 0 && Player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo")
                        {
                            E2.Cast(minions.Position, true);
                        }
                    }
                
            }

        }
         // Flee iJavai, i know but why i should do work if we have a good assembly which is not updated ? :(
        private static void QFlee()
        {
            sendMovementPacket(Game.CursorPos.To2D());
            List<Obj_AI_Base> minions = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All,
                MinionTeam.Enemy,
                MinionOrderTypes.None); 
            Obj_AI_Base FarthestMinion = minions.FirstOrDefault();
            foreach (
                Obj_AI_Base Minion in
                    minions.Where(
                        minion => minion.IsValidTarget(Q.Range) && minion.Distance(Game.CursorPos.To2D()) < Q.Range))
            {
                if (Player.Distance(Minion) > Player.Distance(FarthestMinion))
                {
                    FarthestMinion = Minion;
                }
            }
            Q.Cast(FarthestMinion, true);
        }

        private static void Flee(){
            sendMovementPacket(Game.CursorPos.To2D());
            foreach (var entry in positions) {
                if (Player.Distance(entry.Key) <= E.Range || Player.Distance(entry.Value) <= E.Range) {
                    Vector3 closest = entry.Key;
                    Vector3 furthest = entry.Value;
                    if (Player.Distance(entry.Key) < Player.Distance(entry.Value)) {
                        closest = entry.Key;
                        furthest = entry.Value;
                    }
                    if (Player.Distance(entry.Key) > Player.Distance(entry.Value)) {
                        closest = entry.Value;
                        furthest = entry.Key;
                    }
                    sendMovementPacket(new Vector2(closest.X, closest.Y));
                    E.Cast(closest, true);
                    E2.Cast(furthest, true);
                }
            }
        }

        private static void sendMovementPacket(Vector2 position)
        {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(position.X, position.Y)).Send();
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, position.To3D());
        }

        private static void OnProcSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Name != Player.Name) return;
            if (args.SData.Name != "FizzJump") return;
            JumpState = 1;
            Time = Game.Time;
            Called = false;
        }

        public static void fillPositions()
        {
            positions = new Dictionary<Vector3, Vector3>();
            Vector3 pos0 = new Vector3(8216f, 3146f, 51.59694f);
            Vector3 pos1 = new Vector3(8376f, 2698f, 51.01983f);
            positions.Add(pos0, pos1);

        }

    }
}
