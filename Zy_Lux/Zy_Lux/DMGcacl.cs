using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;

namespace Zy_Lux
{
    class DMGcacl
    {
        public static float RDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 250, 375, 500 }[Spells.Q.Level - 1] + 1.2 * Brain._Player.FlatMagicDamageMod
                    ));
        }

        public static float WDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 120, 170, 220, 270, 320 }[Spells.E.Level - 1] + 1.0 * Brain._Player.FlatMagicDamageMod
                    ));
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 80, 125, 170, 215, 260 }[Spells.R.Level - 1] + 0.6 * Brain._Player.FlatMagicDamageMod
                    ));
        }
    }
}