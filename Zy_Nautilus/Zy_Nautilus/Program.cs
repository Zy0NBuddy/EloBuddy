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

namespace Zy_Nautilus
{
    internal class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Targeted R;
        private static Spell.Targeted Smite;
        private static Menu Menu, SettingsMenu, Jungle, Misc;
        private static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }
        private static float GetSmiteDamage()
        {
            int level = ObjectManager.Player.Level;
            float[] smitedamage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return smitedamage.Max();
        }

        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3724, 3723, 3933};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719, 3932};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714, 3931};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707, 3930};

        public static void SetSmiteSlot()
        {
            SpellSlot smiteSlot;
            if (SmiteBlue.Any(x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId) x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("s5_summonersmiteplayerganker");
            else if (
                SmiteRed.Any(
                    x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId) x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("s5_summonersmiteduel");
            else if (
                SmiteGrey.Any(
                    x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId) x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("s5_summonersmitequick");
            else if (
                SmitePurple.Any(
                    x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId) x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("itemsmiteaoe");
            else
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("summonersmite");
            Smite = new Spell.Targeted(smiteSlot, 500);
        }

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
            if (_Player.ChampionName != "Nautilus")
                return;

            Chat.Print("Zy - Nautilus Loaded", Color.GreenYellow);

            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E, 600);
            R = new Spell.Targeted(SpellSlot.R, 825);

            if (HasSpell("smite"))
            {
                Smite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonersmite"), 500);
            }

            Menu = MainMenu.AddMenu("Zy - Nautilus", "zynautilus");
            Menu.AddGroupLabel("ZyBuddy - Nautilus");
            Menu.AddSeparator();
            Menu.AddLabel("Developed By Zy0N");
            SettingsMenu = Menu.AddSubMenu("Skills", "Skills");
            SettingsMenu.AddGroupLabel("SKILL");
            SettingsMenu.AddLabel("COMBO");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q", true));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W", true));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E", true));
            SettingsMenu.Add("manu.ult", new CheckBox("Use R Manual", false));
            SettingsMenu.AddLabel("Use R on");
            foreach (var enemies in HeroManager.Enemies.Where(i => !i.IsMe))
            {
                SettingsMenu.Add("r.ult" + enemies.ChampionName, new CheckBox("" + enemies.ChampionName, false));
            }
            SettingsMenu.Add("hptoult", new Slider("Mim %HP to use R", 50, 0, 100));
            SettingsMenu.AddLabel("LANE CLEAR");
            SettingsMenu.Add("Elc", new CheckBox("Use E"));
            SettingsMenu.Add("minione", new Slider("Minions to use W", 3, 1, 20));
            SettingsMenu.Add("mana.lane", new Slider("Mim % Mana", 50, 0, 100));
            SettingsMenu.AddLabel("JUNGLE CLEAR");
            SettingsMenu.Add("Wjg", new CheckBox("Use W", true));
            SettingsMenu.Add("Ejg", new CheckBox("Use E", true));
            SettingsMenu.AddLabel("HARASS");
            SettingsMenu.Add("Eh", new CheckBox("Use E"));

            Jungle = Menu.AddSubMenu("Jungle", "jungle");
            Jungle.Add("use.smite", new CheckBox("Use SMITE", true));
            if (Game.MapId == GameMapId.SummonersRift)
            {
                Jungle.AddLabel("Epics");
                Jungle.Add("SRU_Baron", new CheckBox("Baron", true));
                Jungle.Add("SRU_Dragon", new CheckBox("Dragon", true));
                Jungle.AddLabel("Buffs");
                Jungle.Add("SRU_Blue", new CheckBox("Blue", true));
                Jungle.Add("SRU_Red", new CheckBox("Red", true));
                Jungle.AddLabel("Small Camps");
                Jungle.Add("SRU_Gromp", new CheckBox("Gromp", false));
                Jungle.Add("SRU_Murkwolf", new CheckBox("Murkwolf", false));
                Jungle.Add("SRU_Krug", new CheckBox("Krug", false));
                Jungle.Add("SRU_Razorbeak", new CheckBox("Razerbeak", false));
                Jungle.Add("Sru_Crab", new CheckBox("Skuttles", false));
            }

            Misc = Menu.AddSubMenu("Misc", "misc");
            Misc.AddGroupLabel("MISCELLANEOUS");
            Misc.AddLabel("DRAWING");
            Misc.Add("Qdrawn", new CheckBox("Drawn Q"));
            Misc.Add("Edrawn", new CheckBox("Drawn E"));
            Misc.Add("Rdrawn", new CheckBox("Drawn R"));
            Misc.AddLabel("HP info Ally");
            Misc.Add("allyhp", new CheckBox("Drawn HP info"));


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
    

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var manual = SettingsMenu["manu.ult"].Cast<CheckBox>().CurrentValue;
            var checkhp = SettingsMenu["hptoult"].Cast<Slider>().CurrentValue;
            
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && !target.IsDead)
            {
                Q.Cast(target);
                
            }

            if (E.IsReady() && useE && target.IsValidTarget() && !target.IsZombie && !target.IsDead && !target.IsInvulnerable)
            {
                E.Cast();
            }

            if (useW && W.IsReady() && target.IsValidTarget() && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                W.Cast();
            }
            

                if (R.IsReady() && !manual)
            {
                var enemies =
                   HeroManager.Enemies.OrderBy(a => a.HealthPercent).Where(a => !a.IsMe && !a.IsDead && !a.IsZombie && a.Distance(_Player) <= R.Range);
                foreach (var ultenemies in enemies)

                {
                    var useR = SettingsMenu["r.ult" + ultenemies.ChampionName].Cast<CheckBox>().CurrentValue;

                    if (ultenemies.HealthPercent < checkhp)
                    {
                        if (useR)
                        {
                            R.Cast(ultenemies);
                        }
                    }
                }
            }

        }

        private static void Harass()
        {
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsInvulnerable &&
                !target.IsDead)
            {
                E.Cast();
            }
        }

        private static void LaneClear()
        {

            var useE = SettingsMenu["Elc"].Cast<CheckBox>().CurrentValue;
            var minionlc = SettingsMenu["minione"].Cast<Slider>().CurrentValue;
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
                E.Cast();
            }

        }

        private static void JungleClear()
        {
            if (Game.MapId == GameMapId.SummonersRift)
            {
                var t =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(u => u.IsVisible && JungleMobsList.Contains(u.BaseSkinName))
                        .FirstOrDefault();

                if (t != null)
                {
                    if (!Jungle[t.BaseSkinName].Cast<CheckBox>().CurrentValue)
                    {
                        return;
                    }

                    var useSmite = Jungle["use.smite"].Cast<CheckBox>().CurrentValue;

                    if (useSmite)
                    {
                        if (t.IsValidTarget()
                            && t.Health <= GetSmiteDamage())
                        {
                            Smite.Cast(t);
                        }
                    }
                }
            }

            var useW = SettingsMenu["Wjg"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ejg"].Cast<CheckBox>().CurrentValue;
            var Target1 = EntityManager.GetJungleMonsters(_Player.ServerPosition.To2D(), E.Range, true).FirstOrDefault();
            var Target2 =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(u => u.IsVisible && JungleMobsList.Contains(u.BaseSkinName))
                    .FirstOrDefault();
            
            if (Target2 != null)
            {
                if (Target2.IsValidTarget() && useW && W.IsReady())
                {
                    W.Cast();
                }
                if (Target2.IsValidTarget() && E.IsReady() && useE && E.IsInRange(Target2))
                {
                    E.Cast();
                }
            }

            if (Target1 != null)
            {
                if (Target1.IsValidTarget() && useW && W.IsReady())
                {
                    W.Cast();
                }
                if (Target1.IsValidTarget() && E.IsReady() && useE && E.IsInRange(Target1))
                {
                    E.Cast();
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

