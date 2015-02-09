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
    class Program
    {
        public static string ChampionName;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            ChampionName = ObjectManager.Player.BaseSkinName;

            switch (ChampionName)
            {
                case "Blitzcrank":
                    new Blitzcrank();
                    break;

                case "Akali":
                    new Akali();
                    break;

                case "XinZhao":
                    new XinZhao();
                    break;

                case "Amumu":
                    new Amumu();
                    break;

                case "MonkeyKing":
                    new Wukong();
                    break;

                case "Olaf":
                    new Olaf();
                    break;

                default:
                    Game.PrintChat("<font color='#05F4F4'>[Kysama Series] This Champion is not supportet</font>");
                    break;
            }
        }
    }
}
