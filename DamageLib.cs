using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using GuTenTak.KogMaw;

namespace GuTenTak.KogMaw
{
    internal class DamageLib
    {
        private static readonly AIHeroClient _Player = ObjectManager.Player;
        private static readonly float[] RDmg = new float []{ 0, 100, 140, 180 };
        public static float QCalc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 130, 180, 230, 280 }[Program.Q.Level] + 0.5f * _Player.FlatMagicDamageMod
                    ));
        }
        public static float R1Calc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                RDmg[Program.R.Level] + 0.65f * _Player.FlatPhysicalDamageMod + 0.25f * _Player.FlatMagicDamageMod
                    );
        }
        public static float R2Calc(Obj_AI_Base target)
        {
            float ch = 100f - target.HealthPercent;
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                RDmg[Program.R.Level] + RDmg[Program.R.Level] / 120f * ch + (0.65f + 0.65f / 120f * ch ) * _Player.FlatPhysicalDamageMod + (0.25f + 0.25f / 120f * ch) * _Player.FlatMagicDamageMod
                    );
        }
        public static float R3Calc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 200, 280, 360 }[Program.R.Level] + 1.3f * _Player.FlatPhysicalDamageMod + 0.5f * _Player.FlatMagicDamageMod
                    ));
        }
        public static float DmgCalc(AIHeroClient target)
        {
            var damage = 0f;
            if (Program.Q.IsReady() && target.IsValidTarget(Program.Q.Range))
                damage += QCalc(target);
            if (Program.R.IsReady() && target.IsValidTarget(Program.R.Range))
                damage += R1Calc(target);
            if (Program.R.IsReady() && target.IsValidTarget(Program.R.Range))
                damage += R2Calc(target);
            if (Program.R.IsReady() && target.IsValidTarget(Program.R.Range))
                damage += R3Calc(target);
            damage += _Player.GetAutoAttackDamage(target, true) * 2;
            return damage;
        }
    }
}
