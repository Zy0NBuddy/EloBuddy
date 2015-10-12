using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Zy_Brand
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Targeted R;
        public static Menu Menu, SettingsMenu, Misc;

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
            if (_Player.ChampionName != "Brand")
                return;

            Chat.Print("Zy - Brand Loaded", Color.GreenYellow);

            Q = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 845, SkillShotType.Circular);
            E = new Spell.Targeted(SpellSlot.E, 585);
            R = new Spell.Targeted(SpellSlot.R, 700);


            Menu = MainMenu.AddMenu("Zy - Brand", "zybrand");
            Menu.AddGroupLabel("ZyBuddy - Brand");
            Menu.AddSeparator();
            Menu.AddLabel("Developed By Zy0N");
            SettingsMenu = Menu.AddSubMenu("Skills", "Skills");
            SettingsMenu.AddGroupLabel("SKILL");
#region COMBO
            SettingsMenu.AddLabel("COMBO");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W", true));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E", true));
            SettingsMenu.Add("Rcombo", new CheckBox("Use R", true));
            #endregion
#region LANE CLEAR
            SettingsMenu.AddLabel("LANE CLEAR");
            SettingsMenu.Add("Wlc", new CheckBox("Use W"));
            SettingsMenu.Add("minionw", new Slider("Minions to use W", 3, 1, 20));
            SettingsMenu.Add("mana.lane", new Slider("Mim % Mana", 50, 0, 100));
            #endregion
#region JG
            SettingsMenu.AddLabel("JUNGLE CLEAR");
            SettingsMenu.Add("Qjg", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wjg", new CheckBox("Use W", true));
            SettingsMenu.Add("Ejg", new CheckBox("Use E", true));
            #endregion
#region HARASS
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Wh", new CheckBox("Use W"));
            SettingsMenu.Add("Eh", new CheckBox("Use E"));
            #endregion
#region Misc
            Misc = Menu.AddSubMenu("Misc", "misc");
            Misc.AddGroupLabel("MISCELLANEOUS");
            Misc.AddLabel("DRAWING");
            Misc.Add("Qdrawn", new CheckBox("Drawn Q"));
            Misc.Add("Wdrawn", new CheckBox("Drawn W"));
            Misc.Add("Edrawn", new CheckBox("Drawn E"));
            Misc.Add("Rdrawn", new CheckBox("Drawn R"));
            Misc.AddLabel("HP info Ally");
            Misc.Add("allyhp", new CheckBox("Drawn HP info"));
#endregion

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

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
        #region DMG calc
        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 80, 120, 160, 200, 240 }[Program.Q.Level] + 0.65 * _Player.FlatMagicDamageMod));
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 75, 120, 165, 210, 255 }[Program.E.Level] + 0.6 * _Player.FlatMagicDamageMod));
        }
        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 70, 105, 140, 175, 210 }[Program.E.Level] + 0.55 * _Player.FlatMagicDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 150, 250, 350 }[Program.R.Level] + 0.5 * _Player.FlatMagicDamageMod));
        }
        #endregion

        private static void Combo()
        {
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (useW && W.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsDead)
            {
                W.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsDead)
            {
                Q.Cast(target);
            }
            

            if (E.IsReady() && useE && target.IsValidTarget() && !target.IsZombie && !target.IsDead)
            {
                E.Cast(target);
            }


            if (useR && R.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsDead && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }

        }
        private static void LaneClear()
        {
            var useW = SettingsMenu["Wlc"].Cast<CheckBox>().CurrentValue;
            var minionlc = SettingsMenu["minionw"].Cast<Slider>().CurrentValue;
            var manalc = SettingsMenu["mana.lane"].Cast<Slider>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, 845, true);
            if (useW && W.IsReady() && minions.Count >= minionlc && _Player.ManaPercent >= manalc)
            {
                W.Cast(minions.OrderBy(x => x.IsValid()).FirstOrDefault().ServerPosition);
            }
            
        }
        private static void JungleClear()
        {
            var useQ = SettingsMenu["Qjg"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wjg"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ejg"].Cast<CheckBox>().CurrentValue;
            var jg = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(a => a.MaxHealth).FirstOrDefault(b => b.Distance(Player.Instance) < 1300);

            if (jg == null)
                return;
            if (useQ && Q.IsReady() && jg.IsVisible && jg.Distance(_Player) < Q.Range)
            {
                Q.Cast(jg);
            }
            if (useW && W.IsReady() && jg.IsVisible && jg.Distance(_Player) < W.Range)
            {
                W.Cast(jg);
            }
            if (useE && E.IsReady() && jg.IsVisible && jg.Distance(_Player) < E.Range)
            {
                E.Cast(jg);
            }
        }
        private static void Harass()
        {
            var useW = SettingsMenu["Wh"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (useW && W.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsDead && !target.IsInvulnerable)
            {
                W.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsDead && !target.IsInvulnerable)
            {
                E.Cast(target);
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
            var qdraw = Misc["Qdrawn"].Cast<CheckBox>().CurrentValue;
            var wdraw = Misc["Wdrawn"].Cast<CheckBox>().CurrentValue;
            var edraw = Misc["Edrawn"].Cast<CheckBox>().CurrentValue;
            var rdraw = Misc["Rdrawn"].Cast<CheckBox>().CurrentValue;
            var allydrawn = Misc["allyhp"].Cast<CheckBox>().CurrentValue;

            if (qdraw)
            {
                new Circle() { Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (wdraw)
            {
                new Circle() { Color = Color.DodgerBlue, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
            if (edraw)
            {
                new Circle() { Color = Color.LightSkyBlue, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
            if (rdraw)
            {
                new Circle() { Color = Color.Red, BorderWidth = 2, Radius = R.Range }.Draw(_Player.Position);
            }

            if (allydrawn)
            {
                DrawHealths();
            }

        }
    }
}
