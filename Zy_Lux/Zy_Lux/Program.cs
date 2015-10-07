using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
            private static void Main(string[] args)
            {
                Loading.OnLoadingComplete += Loading_OnLoadingComplete;
                Bootstrap.Init(null);
            }
        
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
  /*      private static readonly Spell.Targeted Ignite =
            new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);*/

        public static Menu Menu, SettingsMenu, Misc;
     //  public static Vector3 castpos;
        public static GameObject LuxEGameObject;

        private static string[] JungleMobsList =
            {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Gromp",
            "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab"
            };


        public static AIHeroClient _Player
            {
                get { return ObjectManager.Player; }
            }

            private static void Loading_OnLoadingComplete(EventArgs args)
            {
                if (_Player.ChampionName != "Lux")
                    return;

                Chat.Print("Zy_Lux \t- LOADED", Color.GreenYellow);

                Q = new Spell.Skillshot(SpellSlot.Q, 1175, SkillShotType.Linear);
                W = new Spell.Skillshot(SpellSlot.W, 1075, SkillShotType.Linear);
                E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Circular);
                R = new Spell.Skillshot(SpellSlot.R, 3300, SkillShotType.Linear);


                Menu = MainMenu.AddMenu("Zy - Lux", "zylux");
                Menu.AddGroupLabel("ZyBuddy - Lux");
                Menu.AddSeparator();
                Menu.AddLabel("Developed By Zy0N");
                SettingsMenu = Menu.AddSubMenu("Skills", "Skills");
                SettingsMenu.AddGroupLabel("SKILL");
#region COMBO
                SettingsMenu.AddLabel("COMBO");
                SettingsMenu.Add("Qcombo", new CheckBox("Use Q", true));
         //       SettingsMenu.Add("Wcombo", new CheckBox("Use W", true));
          //      SettingsMenu.Add("autow", new CheckBox("Auto W on Turrets/Targetted Spells", true));
                SettingsMenu.Add("Ecombo", new CheckBox("Use E", true));
                SettingsMenu.Add("Rcombo", new CheckBox("Use R", true));
                
                
#endregion COMBO
#region FARM
            SettingsMenu.AddLabel("LANE CLEAR");
            SettingsMenu.Add("Elc", new CheckBox("Use E"));
            SettingsMenu.Add("minionw", new Slider("Minions to use E", 3, 1, 20));
            SettingsMenu.Add("mana.lane", new Slider("Mim % Mana", 50, 0, 100));
            SettingsMenu.AddLabel("JUNGLE CLEAR");
            SettingsMenu.Add("Qjg", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wjg", new CheckBox("Use W", true));
            SettingsMenu.Add("Ejg", new CheckBox("Use E", true));
            #endregion FARM
#region HARASS
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Eh", new CheckBox("Use E"));
#endregion HARASS
#region MISC
            Misc = Menu.AddSubMenu("Misc", "misc");
                Misc.AddGroupLabel("MISCELLANEOUS");
                Misc.AddLabel("DRAWING");
                Misc.Add("Qdrawn", new CheckBox("Drawn Q"));
                Misc.Add("Edrawn", new CheckBox("Drawn E"));
                Misc.Add("Rdrawn", new CheckBox("Drawn R"));
          //      Misc.Add("RdrawnMINI", new CheckBox("Drawn R on MiniMap", true));
                Misc.AddLabel("HP info Ally");
                Misc.Add("allyhp", new CheckBox("Drawn HP info"));
                Misc.AddLabel("SMART KS");
              //  Misc.Add("KSI", new CheckBox("Use Ignite", true));
                Misc.Add("KSQ", new CheckBox("Use Q", true));
                Misc.Add("KSE", new CheckBox("Use E", true));
                Misc.Add("KSR", new CheckBox("Use R", true));
      //          Misc.AddLabel("ANTI GAP");
//                  Misc.Add("AntiGapQ", new CheckBox("Use Q", true));
                Misc.AddLabel("JUNGLE STEAL");
                Misc.Add("ksjg", new KeyBind("ULT Steal Drag/Baron R", false, KeyBind.BindTypes.HoldActive));
            if (Game.MapId == GameMapId.SummonersRift)
            {
                Misc.Add("SRU_Baron", new CheckBox("Baron"));
                Misc.Add("SRU_Dragon", new CheckBox("Dragon"));
             
            }
            #endregion MISC

            Game.OnTick += Game_OnTick;
                Drawing.OnDraw += Drawing_OnDraw;
        }
        
        private static void Autospells()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null)
                return;
            if (target.IsInvulnerable)
                return;
            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Q.GetPrediction(target).HitChance >= HitChance.High)
            {
                if (target.HasBuffOfType(BuffType.Snare)
                    || target.HasBuffOfType(BuffType.Suppression)
                    || target.HasBuffOfType(BuffType.Taunt)
                    || target.HasBuffOfType(BuffType.Stun)
                    || target.HasBuffOfType(BuffType.Charm)
                    || target.HasBuffOfType(BuffType.Fear))

                    Q.Cast(target);
            }
        }
        private static void Game_OnTick(EventArgs args)
            {
                if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;


                KillSteal();
                Autospells();
                JgSteal();

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
        private static void Elogic()
        {
            var target = TargetSelector.GetTarget(E.Range + E.Width, DamageType.Magical);
            var epred = E.GetPrediction(target);
            

           /* if (LuxEGameObject != null && E.IsReady() && LuxEGameObject.Position.CountEnemiesInRange(E.Width) < 1)
                Utility.DelayAction.Add(2000, () => E.Cast()); */

            if (target.IsInvulnerable)
                return;

            if (target.HasBuff("luxilluminatingfraulein") && target.HasBuff("LuxLightBindingMis") &&
                _Player.Distance(target.Position) <= _Player.GetAutoAttackRange(_Player))
                return;

            if (LuxEGameObject != null
                && LuxEGameObject.Position.CountEnemiesInRange(300) >= 1)
                E.Cast();

            if (LuxEGameObject != null
                && target.HasBuffOfType(BuffType.Slow))
                E.Cast();

            if (LuxEGameObject != null)
                return;

            if (epred.HitChance >= HitChance.High)
                E.Cast(epred.CastPosition);
            else if (epred.HitChance >= HitChance.High)
                E.Cast(epred.CastPosition);
        }
        private static void Combo()
            {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var t = TargetSelector.GetTarget((W.Range/4), DamageType.Magical);
            var target2 = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && !target.IsDead)
            {
                Q.Cast(target);
            }

            if (E.IsReady() && useE && target.IsValidTarget() && !target.IsZombie && !target.IsDead && !target.IsInvulnerable)
            {
                E.Cast(target);
            }

            if (useW && W.IsReady() && t.IsValidTarget())
            {
                W.Cast(target);
            }
            if (!E.IsReady() && useR && R.IsReady() && target2.IsValidTarget(R.Range) && !target2.IsZombie && !target2.IsInvulnerable && target2.Health <= _Player.GetSpellDamage(target2, SpellSlot.R))
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
                    Elogic(); //check isso
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
                var targetq = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                var useQ = Misc["KSQ"].Cast<CheckBox>().CurrentValue;

                if (useQ && Q.IsReady() && targetq.IsValidTarget(Q.Range) && !targetq.IsZombie && !targetq.IsInvulnerable &&
                    targetq.Health <= _Player.GetSpellDamage(targetq, SpellSlot.Q))
                {
                    Q.Cast(targetq);
                } 
                var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                var useE = Misc["KSE"].Cast<CheckBox>().CurrentValue;

                if (useE && E.IsReady() && targetE.IsValidTarget(E.Range) && !targetE.IsZombie && !targetE.IsInvulnerable &&
                    targetE.Health <= _Player.GetSpellDamage(targetE, SpellSlot.E))
                {
                    E.Cast(targetE);
                }
                var targetR = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                var useR = Misc["KSR"].Cast<CheckBox>().CurrentValue;

                if (!targetE.IsValidTarget(E.Range) && !targetE.IsValidTarget(E.Range) && useR && R.IsReady() 
                && targetR.IsValidTarget(R.Range) && !targetR.IsZombie && !targetR.IsInvulnerable 
                && targetR.Health <= _Player.GetSpellDamage(targetR, SpellSlot.R))
                {
                    R.Cast(targetR);
                }
            }
        private static void JgSteal()
        {
            var useR = Misc["ksjg"].Cast<KeyBind>().CurrentValue;
            if (Game.MapId == GameMapId.SummonersRift)
            {
                var t =
                    EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(
                        u => R.IsInRange(u) && u.IsVisible && JungleMobsList.Contains(u.BaseSkinName));
                if (useR)
                {
                    if (t.IsValidTarget()
                        && R.IsReady()
                        && R.IsInRange(t)
                        && t.Health <= _Player.GetSpellDamage(t, SpellSlot.R))
                    {
                      R.Cast(t);
                    }
                }
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