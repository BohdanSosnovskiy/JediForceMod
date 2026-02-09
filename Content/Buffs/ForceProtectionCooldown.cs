using Terraria;
using Terraria.ModLoader;

namespace JediForceMod.Content.Buffs
{
    public class ForceProtectionCooldown : ModBuff
    {
        // Используем иконку Хаоса (Chaos State) как заглушку для кулдауна
        public override string Texture => "Terraria/Images/Buff_88";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true; // Это негативный эффект (нельзя снять кликом)
        }
    }
}