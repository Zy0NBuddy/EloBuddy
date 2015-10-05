using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Zy_Lux
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Menu Menu, SettingsMenu, Misc;

        private static string[] JungleMobsList =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Gromp",
            "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab"
        };

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.ChampionName != "Lux")
                return;

            Chat.Print("Zy - Lux\tDeveloped by Zy0N", Color.GreenYellow);

            Q = new Spell.Skillshot(SpellSlot.Q, 1175, SkillShotType.Linear);
            W = new Spell.Skillshot(SpellSlot.W, 1075, SkillShotType.Linear);
            E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Circular);
            R = new Spell.Skillshot(SpellSlot.R, 3340, SkillShotType.Linear);


            Menu = MainMenu.AddMenu("Zy - Lux", "zylux");
            Menu.AddGroupLabel("ZyBuddy - Lux");
            Menu.AddSeparator();
            Menu.AddLabel("Developed By Zy0N");
            SettingsMenu = Menu.AddSubMenu("Skills", "Skills");
            SettingsMenu.AddGroupLabel("SKILL");
            SettingsMenu.AddLabel("COMBO");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W", true));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E", true));
            SettingsMenu.Add("Rcombo", new CheckBox("Use R to Kill", true));
            SettingsMenu.AddLabel("LANE CLEAR");
            SettingsMenu.Add("Elc", new CheckBox("Use E"));
            SettingsMenu.Add("minionw", new Slider("Minions to use E", 3, 1, 20));
            SettingsMenu.Add("mana.lane", new Slider("Mim % Mana", 50, 0, 100));
            SettingsMenu.AddLabel("JUNGLE CLEAR");
            SettingsMenu.Add("Qjg", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wjg", new CheckBox("Use W", true));
            SettingsMenu.Add("Ejg", new CheckBox("Use E", true));
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Eh", new CheckBox("Use E"));

            Misc = Menu.AddSubMenu("Misc", "misc");
            Misc.AddGroupLabel("MISCELLANEOUS");
            Misc.AddLabel("DRAWING");
            Misc.Add("Qdrawn", new CheckBox("Drawn Q"));
            Misc.Add("Edrawn", new CheckBox("Drawn E"));
            Misc.Add("Rdrawn", new CheckBox("Drawn R"));
            Misc.AddLabel("HP info Ally");
            Misc.Add("allyhp", new CheckBox("Drawn HP info"));
            Misc.AddLabel("KILLSTEAL");
            Misc.Add("ks.r.lux", new CheckBox("Use R to KS",true));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;


            KillSteal();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {

            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var target2 = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && !target.IsDead)
            {
                Q.Cast(target);
            }

            if (E.IsReady() && useE && target.IsValidTarget()  && !target.IsZombie && !target.IsDead && !target.IsInvulnerable )
            {
                E.Cast(target);
            }

            if (useW && W.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                W.Cast(target);
            }
            if (useR && R.IsReady() && target2.IsValidTarget(R.Range) && !target2.IsZombie && !target2.IsInvulnerable && target2.Health <= _Player.GetSpellDamage(target2, SpellSlot.R))
            {
                R.Cast(target2);
            }

        }

        private static void Harass()
        {
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                E.Cast(target);
            }
        }

        private static void LaneClear()
        {

            var useE = SettingsMenu["Elc"].Cast<CheckBox>().CurrentValue;
            var minionlc = SettingsMenu["minionw"].Cast<Slider>().CurrentValue;
            var manalc = SettingsMenu["mana.lane"].Cast<Slider>().CurrentValue;
            var minionCount =
                EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position.To2D(), E.Range)
                    .FirstOrDefault();
            if (minionCount == null)
                return;
            var minion = minionCount;
            var Efarm = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position.To2D(), E.Range);
            if (Efarm == null)
                return;
            if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && Efarm.Count >= minionlc &&
                    _Player.ManaPercent >= manalc)
            {
                E.Cast(Efarm.OrderBy(x => x.IsValid()).FirstOrDefault().ServerPosition);
            }

        }

        private static void JungleClear()
        {

            var useQ = SettingsMenu["Qjg"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wjg"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ejg"].Cast<CheckBox>().CurrentValue;
            var Target1 = EntityManager.GetJungleMonsters(_Player.ServerPosition.To2D(), Q.Range, true).FirstOrDefault();
            var Target2 =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(u => u.IsVisible && JungleMobsList.Contains(u.BaseSkinName))
                    .FirstOrDefault();
            // var eTarget = EntityManager.GetJungleMonsters(_Player.ServerPosition.To2D(), E.Radius, true);
            if (Target2 != null)
            {
                if (Target2.IsValidTarget() && useQ && Q.IsReady() && Q.IsInRange(Target2))
                {
                    Q.Cast(Target2);
                }
                if (Target2.IsValidTarget() && useW && W.IsReady())
                {
                    W.Cast(Target2);
                }
                if (Target2.IsValidTarget() && E.IsReady() && useE && E.IsInRange(Target2))
                {
                    E.Cast(Target2);
                }
            }

            if (Target1 != null)
            {
                if (Target1.IsValidTarget() && useQ && Q.IsReady() && Q.IsInRange(Target1))
                {
                    Q.Cast(Target1);
                }
                if (Target1.IsValidTarget() && useW && W.IsReady())
                {
                    W.Cast();
                }
                if (Target1.IsValidTarget() && E.IsReady() && useE && E.IsInRange(Target1))
                {
                    E.Cast(Target1.ServerPosition);
                }
            }




        }

        private static void KillSteal()
        {
         
            var target2 = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;

            if (useR && R.IsReady() && target2.IsValidTarget(R.Range) && !target2.IsZombie && !target2.IsInvulnerable &&
                target2.Health <= _Player.GetSpellDamage(target2, SpellSlot.R))
            {
                R.Cast(target2);
            }
        }
        
        private static void DrawHealths()
        {

            {
                float i = 0;
                foreach (
                    var hero in HeroManager.Allies.Where(hero => hero.IsAlly && !hero.IsMe && !hero.IsDead))
                {
                    var playername = hero.Name;
                    if (playername.Length > 13)
                    {
                        playername = playername.Remove(9) + "..";
                    }
                    var champion = hero.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }
                    var percent = (int)(hero.Health / hero.MaxHealth * 100);
                    // da pra usar isso pra chegar o hp do aliado //
                    var color = Color.Red;
                    if (percent > 25)
                    {
                        color = Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = Color.LimeGreen;
                    }
                    Drawing.DrawText(
                        Drawing.Width * 0.8f, Drawing.Height * 0.1f + i, color, playername + " (" + champion + ") ");
                    Drawing.DrawText(
                        Drawing.Width * 0.9f, Drawing.Height * 0.1f + i, color,
                        ((int)hero.Health).ToString() + " (" + percent.ToString() + "%)");
                    i += 20f;
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var Qdraw = Misc["Qdrawn"].Cast<CheckBox>().CurrentValue;
            var Edraw = Misc["Edrawn"].Cast<CheckBox>().CurrentValue;
            var Rdraw = Misc["Rdrawn"].Cast<CheckBox>().CurrentValue;
            var Allydrawn = Misc["allyhp"].Cast<CheckBox>().CurrentValue;
            if (Qdraw)
            {
                new Circle() { Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Edraw)
            {
                new Circle() { Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Rdraw)
            {
                new Circle() { Color = Color.Red, BorderWidth = 2, Radius = R.Range }.Draw(_Player.Position);
            }

            if (Allydrawn)
            {
                DrawHealths();
            }

        }

    }
}
