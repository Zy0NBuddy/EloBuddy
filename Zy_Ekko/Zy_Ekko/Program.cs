using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
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
using Zy_Ekko;
using Color = System.Drawing.Color;

namespace Zy_Ekko
{
    class Program
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);
        }

        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Menu Menu, SettingsMenu, Misc;




        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.ChampionName != "Ekko")
                return;



            Chat.Print("Zy_Ekko - LOADED", Color.GreenYellow);


            Q = new Spell.Skillshot(SpellSlot.Q, 750, SkillShotType.Linear);
            W = new Spell.Skillshot(SpellSlot.W, 1620, SkillShotType.Circular);
            E = new Spell.Skillshot(SpellSlot.E, 350, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 375, SkillShotType.Circular);


            Menu = MainMenu.AddMenu("Zy - Ekko", "zyEkko");
            Menu.AddGroupLabel("ZyBuddy - Ekko");
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
            SettingsMenu.Add("rhp", new CheckBox("Use R to recovery", true));
            SettingsMenu.Add("Rdmg", new Slider("% HP to R", 20, 0, 100));
            #endregion COMBO
            #region FARM
            SettingsMenu.AddLabel("LANE CLEAR");
            SettingsMenu.Add("Qlc", new CheckBox("Use Q"));
            SettingsMenu.Add("minione", new Slider("Minions to use E", 3, 1, 20));
            SettingsMenu.Add("mana.lane", new Slider("Mim % Mana", 50, 0, 100));
            SettingsMenu.AddLabel("JUNGLE CLEAR");
            SettingsMenu.Add("Qjg", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wjg", new CheckBox("Use W", true));
            #endregion FARM
            #region HARASS
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Qh", new CheckBox("Use Q"));
            #endregion HARASS
            #region MISC
            Misc = Menu.AddSubMenu("Misc", "misc");
            Misc.AddGroupLabel("MISCELLANEOUS");
            Misc.AddLabel("DRAWING");
            Misc.Add("Qdrawn", new CheckBox("Drawn Q"));
            Misc.Add("Wdrawn", new CheckBox("Drawn W"));
            Misc.Add("Edrawn", new CheckBox("Drawn E"));
            Misc.Add("Rdrawn", new CheckBox("Drawn R"));
            Misc.AddLabel("HP info Ally");
            Misc.Add("allyhp", new CheckBox("Drawn HP info"));
            /* Misc.AddLabel("SMART KS");
             Misc.Add("ksqe", new CheckBox("Use Q/E", true));
             Misc.Add("KSR", new CheckBox("Use R", true));*/

            #endregion MISC

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static Obj_AI_Base EkkoGhost
        {
            get
            {
                return
                ObjectManager.Get<Obj_AI_Base>()
                                .FirstOrDefault(ghost => !ghost.IsEnemy && ghost.Name.Contains("Ekko"));
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

            RL();
            //KS();

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
        #region DMG  
        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 60, 75, 90, 105, 120 }[Program.Q.Level] + 0.1 * _Player.FlatMagicDamageMod));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 50, 80, 110, 140, 170 }[Program.E.Level] + 0.2 * _Player.FlatMagicDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 200, 350, 500 }[Program.R.Level] + 1.3 * _Player.FlatMagicDamageMod));
        }
        #endregion DMG
        private static void Combo()
        {
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var target1 = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var target2 = TargetSelector.GetTarget(1000, DamageType.Magical);
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);


            if (useQ && Q.IsReady() && target1.IsValidTarget() && !target1.IsZombie && !target1.IsDead)
            {
                Q.Cast(target1);
            }
            if (useW && W.IsReady() && target2.IsValidTarget() && !target2.IsZombie && !target2.IsDead)
            {
                W.Cast(target2);
            }

            if (E.IsReady() && useE && target1.IsValidTarget() && !target1.IsZombie && !target1.IsDead)
            {
                E.Cast(target1);
            }


            if (useR && R.IsReady() && target2.IsValidTarget() && !target2.IsZombie && !target2.IsDead && target2.Distance(EkkoGhost.Position) <= 375 && target2.Health <= _Player.GetSpellDamage(target2, SpellSlot.R))
            {
                R.Cast(_Player);
            }

        }

        private static void Harass()
        {
            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var tq = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (useQ && Q.IsReady() && tq.IsValidTarget() && !tq.IsZombie && !tq.IsDead && !tq.IsInvulnerable)
            {
                Q.Cast(tq);
            }

        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var minionlc = SettingsMenu["minione"].Cast<Slider>().CurrentValue;
            var manalc = SettingsMenu["mana.lane"].Cast<Slider>().CurrentValue;
            var minionCount =
                    EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position.To2D(), Q.Range)
                        .FirstOrDefault();
            if (minionCount == null)
                return;
            var minion = minionCount;
            var Qfarm = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position.To2D(), Q.Range);
            if (Qfarm == null)
                return;
            if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Qfarm.Count >= minionlc &&
                    _Player.ManaPercent >= manalc)
            {
                Q.Cast(Qfarm.OrderBy(x => x.IsValid()).FirstOrDefault().ServerPosition);
            }

        }
        private static void JungleClear()
        {

            var useQ = SettingsMenu["Qjg"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wjg"].Cast<CheckBox>().CurrentValue;
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
            var Wdraw = Misc["Wdrawn"].Cast<CheckBox>().CurrentValue;
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
            if (Wdraw)
            {
                new Circle() { Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Rdraw && R.IsReady())
            {

                new Circle() { Color = Color.Red, BorderWidth = 3, Radius = 375 }.Draw(EkkoGhost.Position);
            }

            if (Allydrawn)
            {
                DrawHealths();
            }

        }
        /*     private static void KS()
             {
                 var ksqe = Misc["ksqe"].Cast<CheckBox>().CurrentValue;
                 var ultks = Misc["ultks"].Cast<CheckBox>().CurrentValue;
                 if (ksqe)
                 {
                     if (Q.IsReady() && E.IsReady())
                     {
                         foreach (
                             var target in
                                 EntityManager.Heroes.Enemies.Where(o => o.IsValidTarget(1250) && !o.IsDead && !o.IsZombie))
                         {
                             var ksDmgQe = QDamage(target) + EDamage(target);
                             if (target.Health <= ksDmgQe)
                             {
                                 E.Cast(target);
                                 Q.Cast(target);
                             }
                         }
                     }

                     if (Q.IsReady() && E.IsOnCooldown)
                     {
                         foreach (
                             var target in
                                 EntityManager.Heroes.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie)
                             )
                         {
                             var qDmg = QDamage(target);
                             if (target.Health <= qDmg)
                             {
                                 Q.Cast(target);
                             }
                         }
                     }

                     if (Q.IsOnCooldown && E.IsReady())
                     {
                         foreach (
                             var target in
                                 EntityManager.Heroes.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie)
                             )
                         {
                             var eDmg = EDamage(target);
                             if (target.Health <= eDmg)
                             {
                                 E.Cast(Game.CursorPos);
                             }
                         }
                     }
                 }
                 var t = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                 var ultDmg = RDamage(t) + EDamage(t) + QDamage(t);

                 if (t.Distance(EkkoGhost.Position) <= 375 && R.IsReady() && ultks)
                 {
                     if (t.Health <= ultDmg)
                     {
                         R.Cast();
                     }

                 }
             }*/
        private static void RL()
        {
            var useR = SettingsMenu["rhp"].Cast<CheckBox>().CurrentValue;
            var hpr = SettingsMenu["Rdmg"].Cast<Slider>().CurrentValue;

            if (R.IsReady() && useR && _Player.HealthPercent <= hpr)
            {
                R.Cast(_Player);
            }
        }
    }
}
