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

namespace Zy_Shen
{
    internal class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;
        public static Menu Menu, SettingsMenu, UltSettings, Misc;
       

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
            if (_Player.ChampionName != "Shen")
                return;

            Chat.Print("Zy - Shen\tDeveloped by Zy0N");

            Q = new Spell.Targeted(SpellSlot.Q, 440);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Linear);
            R = new Spell.Targeted(SpellSlot.R, 25000);
            

            Menu = MainMenu.AddMenu("Zy - Shen", "zyshen");
            Menu.AddGroupLabel("ZyBuddy - Shen");
            Menu.AddSeparator();
            Menu.AddLabel("Developed By Zy0N");
            SettingsMenu = Menu.AddSubMenu("Skills", "Skills");
            SettingsMenu.AddGroupLabel("SKILL");
            SettingsMenu.AddLabel("COMBO");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W", true));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E", true));
            SettingsMenu.AddLabel("LANE CLEAR");
            SettingsMenu.Add("Qlc", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wlc", new CheckBox("Use W"));
            SettingsMenu.AddLabel("JUNGLE CLEAR");
            SettingsMenu.Add("Qjg", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wjg", new CheckBox("Use W", true));
            SettingsMenu.Add("Ejg", new CheckBox("Use E", true));
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Qh", new CheckBox("Use Q"));
            SettingsMenu.Add("Ehunder", new CheckBox("Use E if Enemy under Tower", true));
            SettingsMenu.AddLabel("LAST HIT");
            SettingsMenu.Add("Qlh", new CheckBox("Use Q"));

            UltSettings = Menu.AddSubMenu("Ult", "Ult");
            UltSettings.AddGroupLabel("ULT SETTINGS");
            UltSettings.AddLabel("Dont Use R on");
            foreach (var allies in HeroManager.Allies.Where(i => !i.IsMe))
            {
                UltSettings.Add("dont.r.ult" + allies.ChampionName, new CheckBox("" + allies.ChampionName, false));
            }
            UltSettings.Add("hptoult", new Slider("Mim %HP to use R", 50, 0, 100));
            UltSettings.AddLabel("R hotkey");
            UltSettings.Add("ultonalli", new KeyBind("Stand United", false, KeyBind.BindTypes.HoldActive));
            UltSettings.AddSeparator();
            UltSettings.AddLabel("#### WIP ####");
            UltSettings.AddLabel("Use R to save yourself");  // working on logic to use ult on ally most HP
            UltSettings.Add("Rsave", new CheckBox("Use R", false));
            UltSettings.AddSeparator();
            UltSettings.AddLabel("#### WIP ####");

            Misc = Menu.AddSubMenu("Misc", "misc");
            Misc.AddGroupLabel("MISCELLANEOUS");
            Misc.AddLabel("DRAWING");
            Misc.Add("Qdrawn", new CheckBox("Drawn Q"));
            Misc.Add("Edrawn", new CheckBox("Drawn E"));
            Misc.AddLabel("HP info Ally"); //novo
            Misc.Add("allyhp", new CheckBox("Drawn HP info")); //novo

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

            Ult();
            AutoEonTower();

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
                LastHit();
            }
            
            //  KillSteal();
       
           
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;

            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsDead)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                Q.Cast(target);
            }
            if (useW && Q.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                W.Cast();
            }

        }

        private static void Harass()
        {
            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                Q.Cast(target);
            }
        }

        private static void LaneClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);
            if (minions == null) return;
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wlc"].Cast<CheckBox>().CurrentValue;

            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);

                }
                if (useW && W.IsReady())
                {
                    W.Cast();
                }
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
            var eTarget = EntityManager.GetJungleMonsters(_Player.ServerPosition.To2D(), E.Radius, true);
            if (Target2 != null)
            {
                if (Target2.IsValidTarget() && useQ && Q.IsReady() && Q.IsInRange(Target2))
                {
                    Q.Cast(Target2);
                }
                if (Target2.IsValidTarget() && useW && W.IsReady())
                {
                    W.Cast();
                }
                if (Target2.IsValidTarget() && E.IsReady() && useE && E.IsInRange(Target2))
                {
                    E.Cast(Target2.ServerPosition);
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

        private static void LastHit()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);
            // if (minions == null) return;
            var useQ = SettingsMenu["Qlh"].Cast<CheckBox>().CurrentValue;

            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void AutoEonTower() 
        {
            var useE = SettingsMenu["Ehunder"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var tower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(a => a.IsAlly && !a.IsDead && a.Distance(_Player) <= 850);
            if (useE && E.IsReady() && target != null && tower != null && target.Distance(tower) <= 850)
            {
                E.Cast(target);
            }
        }
        // Credtis to Fluzy to help me fix ult
        private static void Ult()
        {
            var useR = UltSettings["ultonalli"].Cast<KeyBind>().CurrentValue;
            var checkhp = UltSettings["hptoult"].Cast<Slider>().CurrentValue;

            var allies =
                HeroManager.Allies.OrderBy(a => a.HealthPercent).Where(a => !a.IsMe && !a.IsDead && !a.IsInShopRange()
                                                                            && !a.IsZombie &&
                                                                            a.Distance(_Player) <= R.Range);


            if (R.IsReady())
            {

                foreach (var ally in allies)
                {
                    var dontR = UltSettings["dont.r.ult" + ally.ChampionName].Cast<CheckBox>().CurrentValue;

                    if (ally.HealthPercent < checkhp)
                    {
                        if (!dontR && useR)
                        {
                            Player.CastSpell(SpellSlot.R, ally);
                        }
                    }
                }
            }
        }
        
        /*private static void KillSteal(){}
              
        {
            
        }*/

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
            var Allydrawn = Misc["allyhp"].Cast<CheckBox>().CurrentValue;
            if (Qdraw)
            {
                new Circle() {Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = Q.Range}.Draw(_Player.Position);
            }
            if (Edraw)
            {
                new Circle() {Color = Color.DeepSkyBlue, BorderWidth = 2, Radius = E.Range}.Draw(_Player.Position);
            }
            
            if (Allydrawn)
             {
                 DrawHealths();
             }

        }
      
        }
}
