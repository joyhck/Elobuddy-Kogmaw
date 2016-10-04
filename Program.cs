using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using GuTenTak.KogMaw;
using SharpDX;

namespace GuTenTak.KogMaw
{
    internal class Program
    {
        public const string ChampionName = "KogMaw";
        public static Menu Menu, ModesMenu1, ModesMenu2, ModesMenu3, DrawMenu;
        public static int SkinBase;
        public static Item Youmuu = new Item(ItemId.Youmuus_Ghostblade);
        public static Item Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
        public static Item Cutlass = new Item(ItemId.Bilgewater_Cutlass);
        public static Item Tear = new Item(ItemId.Tear_of_the_Goddess);
        public static Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static Item Simitar = new Item(ItemId.Mercurial_Scimitar);
        public static Item hextech = new Item(ItemId.Hextech_Gunblade, 700);
        //private static bool IsZombie;
        //private static bool wActive;
        //private static int LastAATick;

        //public static AIHeroClient PlayerInstance { get { return Player.Instance; } }
        private static float HealthPercent() { return (Player.Instance.Health / Player.Instance.MaxHealth) * 100; }
        
        //public static bool AutoQ { get; protected set; }
        //public static float Manaah { get; protected set; }
        //public static object GameEvent { get; private set; }

        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        //private static bool siegecount;

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }



        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != ChampionName)
            {
                return;
            }
            //IsZombie = PlayerInstance.HasBuff("kogmawicathiansurprise");
            //wActive = PlayerInstance.HasBuff("kogmawbioarcanebarrage");
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnBuffGain += Common.OnBuffGain;
            Game.OnTick += (EventArgs) => Common.Skinhack();
            Gapcloser.OnGapcloser += Common.Gapcloser_OnGapCloser;
            Game.OnUpdate += Common.zigzag;
            Obj_AI_Base.OnLevelUp += OnLevelUp;
            SkinBase = Player.Instance.SkinId;
            // Item
            try
            {
                Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1650, 70);
                Q.AllowedCollisionCount = 0;
                W = new Spell.Active(SpellSlot.W, 720);
                E = new Spell.Skillshot(SpellSlot.E, 1200, SkillShotType.Linear, 500, 1400, 120);
                E.AllowedCollisionCount = int.MaxValue;
                R = new Spell.Skillshot(SpellSlot.R, 1800, SkillShotType.Circular, 1200, int.MaxValue, 120);
                R.AllowedCollisionCount = int.MaxValue;



                Bootstrap.Init(null);
                Chat.Print("GuTenTak Addon Loading Success", Color.Green);


                Menu = MainMenu.AddMenu("GuTenTak KogMaw", "KogMaw");
                Menu.AddSeparator();
                Menu.AddLabel("GuTenTak KogMaw Addon");

                var Enemies = EntityManager.Heroes.Enemies.Where(a => !a.IsMe).OrderBy(a => a.BaseSkinName);
                ModesMenu1 = Menu.AddSubMenu("Menu", "Modes1KogMaw");
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Combo Configs");
                ModesMenu1.Add("ComboQ", new CheckBox("Use Q on Combo", true));
                ModesMenu1.AddLabel("Use Q Mana >= 80");
                ModesMenu1.Add("ComboW", new CheckBox("Use W on Combo", true));
                ModesMenu1.Add("ComboE", new CheckBox("Use E on Combo", true));
                ModesMenu1.Add("ComboR", new CheckBox("Use R on Combo", true));
                ModesMenu1.Add("LogicRn", new ComboBox(" Use R Enemy Health % <= ", 1, "100%", "55%", "35%"));
                ModesMenu1.Add("ManaCE", new Slider("Use E Mana %", 30));
                ModesMenu1.Add("ManaCR", new Slider("Use R Mana %", 80));
                ModesMenu1.Add("CRStack", new Slider("Combo R stack limit", 3, 1, 10));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("AutoHarass Configs");
                ModesMenu1.Add("AutoHarass", new CheckBox("Use R on AutoHarass", false));
                ModesMenu1.Add("ARStack", new Slider("Auto R stack limit", 2, 1, 6));
                ModesMenu1.Add("ManaAuto", new Slider("Mana %", 70));

                ModesMenu1.AddLabel("Harass Configs");
                ModesMenu1.Add("HarassQ", new CheckBox("Use Q on Harass", true));
                ModesMenu1.Add("HarassE", new CheckBox("Use E on Harass", true));
                ModesMenu1.Add("HarassR", new CheckBox("Use R on Harass", true));
                ModesMenu1.Add("ManaHE", new Slider("Use E Mana %", 60));
                ModesMenu1.Add("ManaHR", new Slider("Use R Mana %", 60));
                ModesMenu1.Add("HRStack", new Slider("Harass R stack limit", 1, 1, 6));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Kill Steal Configs");
                ModesMenu1.Add("KS", new CheckBox("Use KillSteal", true));
                ModesMenu1.Add("KQ", new CheckBox("Use Q on KillSteal", true));
                ModesMenu1.Add("KR", new CheckBox("Use R on KillSteal", true));

                ModesMenu2 = Menu.AddSubMenu("Farm", "Modes2KogMaw");
                ModesMenu2.AddLabel("Lane Clear Config");
                ModesMenu2.Add("ManaL", new Slider("Mana %", 40));
                ModesMenu2.Add("FarmQ", new CheckBox("Use Q on LaneClear", true));
                ModesMenu2.Add("ManaLR", new Slider("Mana %", 40));
                ModesMenu2.Add("FarmR", new CheckBox("Use R on LaneClear", true));
                ModesMenu2.Add("FRStack", new Slider("LaneClear R stack limit", 1, 1, 6));
                ModesMenu2.AddLabel("Jungle Clear Config");
                ModesMenu2.Add("ManaJ", new Slider("Mana %", 40));
                ModesMenu2.Add("JungleQ", new CheckBox("Use Q on JungleClear", true));
                ModesMenu2.Add("ManaJR", new Slider("Mana %", 40));
                ModesMenu2.Add("JungleR", new CheckBox("Use R on JungleClear", true));
                ModesMenu2.Add("JRStack", new Slider("JungleClear R stack limit", 2, 1, 6));

                ModesMenu3 = Menu.AddSubMenu("Misc", "Modes3KogMaw");
                ModesMenu3.AddLabel("Flee Configs");
                ModesMenu3.Add("FleeR", new CheckBox("Use R on Flee", true));
                ModesMenu3.Add("FleeE", new CheckBox("Use E on Flee", true));
                ModesMenu3.Add("ManaFlR", new Slider("R Mana %", 35));
                ModesMenu3.Add("FlRStack", new Slider("Flee R stack limit", 2, 1, 6));

                ModesMenu3.AddLabel("Item Usage on Combo");
                ModesMenu3.Add("useYoumuu", new CheckBox("Use Youmuu", true));
                ModesMenu3.Add("usehextech", new CheckBox("Use Hextech", true));
                ModesMenu3.Add("useBotrk", new CheckBox("Use Botrk & Cutlass", true));
                ModesMenu3.Add("useQss", new CheckBox("Use QuickSilver", true));
                ModesMenu3.Add("minHPBotrk", new Slider("Min health to use Botrk %", 80));
                ModesMenu3.Add("enemyMinHPBotrk", new Slider("Min enemy health to use Botrk %", 80));

                ModesMenu3.AddLabel("QSS Configs");
                ModesMenu3.Add("Qssmode", new ComboBox(" ", 0, "Auto", "Combo"));
                ModesMenu3.Add("Stun", new CheckBox("Stun", true));
                ModesMenu3.Add("Blind", new CheckBox("Blind", true));
                ModesMenu3.Add("Charm", new CheckBox("Charm", true));
                ModesMenu3.Add("Suppression", new CheckBox("Suppression", true));
                ModesMenu3.Add("Polymorph", new CheckBox("Polymorph", true));
                ModesMenu3.Add("Fear", new CheckBox("Fear", true));
                ModesMenu3.Add("Taunt", new CheckBox("Taunt", true));
                ModesMenu3.Add("Silence", new CheckBox("Silence", false));
                ModesMenu3.Add("QssDelay", new Slider("Use QSS Delay(ms)", 250, 0, 1000));

                ModesMenu3.AddLabel("QSS Ult Configs");
                ModesMenu3.Add("ZedUlt", new CheckBox("Zed R", true));
                ModesMenu3.Add("VladUlt", new CheckBox("Vladimir R", true));
                ModesMenu3.Add("FizzUlt", new CheckBox("Fizz R", true));
                ModesMenu3.Add("MordUlt", new CheckBox("Mordekaiser R", true));
                ModesMenu3.Add("PoppyUlt", new CheckBox("Poppy R", true));
                ModesMenu3.Add("QssUltDelay", new Slider("Use QSS Delay(ms) for Ult", 250, 0, 1000));

                ModesMenu3.AddLabel("Skin Hack");
                ModesMenu3.Add("skinhack", new CheckBox("Activate Skin hack", false));
                ModesMenu3.Add("skinId", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));

                DrawMenu = Menu.AddSubMenu("Draws", "DrawKogMaw");
                DrawMenu.Add("drawQ", new CheckBox(" Draw Q", true));
                DrawMenu.Add("drawW", new CheckBox(" Draw W", true));
                DrawMenu.Add("drawR", new CheckBox(" Draw R", false));
                DrawMenu.Add("drawXR", new CheckBox(" Draw Don't Use R", true));
                DrawMenu.Add("drawXFleeQ", new CheckBox(" Draw Don't Use Flee Q", false));

            }

            catch (Exception)
            {

            }

        }
        private static void Game_OnDraw(EventArgs args)
        {

            try
            {
                if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
                {
                    if (Q.IsReady() && Q.IsLearned)
                    {
                        Circle.Draw(Color.White, Q.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
                {
                    if (W.IsReady() && W.IsLearned)
                    {
                        Circle.Draw(Color.White, W.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
                {
                    if (R.IsReady() && R.IsLearned)
                    {
                        Circle.Draw(Color.White, R.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawXR"].Cast<CheckBox>().CurrentValue)
                {
                    if (R.IsReady() && R.IsLearned)
                    {
                        Circle.Draw(Color.Red, 700, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawXFleeQ"].Cast<CheckBox>().CurrentValue)
                {
                    if (Q.IsReady() && Q.IsLearned)
                    {
                        Circle.Draw(Color.Red, 400, Player.Instance.Position);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                var AutoHarass = ModesMenu1["AutoHarass"].Cast<CheckBox>().CurrentValue;
                var ManaAuto = ModesMenu1["ManaAuto"].Cast<Slider>().CurrentValue;
                Common.KillSteal();

                if (AutoHarass && ManaAuto <= ObjectManager.Player.ManaPercent)
                    {
                        Common.AutoR();
                    }
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        Common.Combo();
                        Common.ItemUsage();
                    }
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    {
                        Common.Harass();
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    {

                        Common.LaneClear();

                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    {

                        Common.JungleClear();
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                    {
                        Common.LastHit();

                    }
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                    {
                        Common.Flee();

                    }
            }
            catch (Exception e)
            {

            }
        }

        public static void OnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (!sender.IsMe || args.Level != 1) return;
            Game.OnTick += SetSkillshot;
        }

        public static void SetSkillshot(EventArgs args)
        {
            if (Q.Level + W.Level + E.Level + R.Level == Player.Instance.Level)
            {
                W = new Spell.Active(SpellSlot.W, (uint)(565 + 60 + W.Level * 30 + 65));
                R = new Spell.Skillshot(SpellSlot.R, (uint)(900 + R.Level * 300), SkillShotType.Circular, 1500, int.MaxValue, 225);
                Game.OnTick -= SetSkillshot; //improve fps
            }
        }

    }
}
