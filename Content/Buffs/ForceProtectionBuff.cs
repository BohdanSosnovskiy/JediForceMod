using Terraria;
using Terraria.ModLoader;

namespace JediForceMod.Content.Buffs
{
    public class ForceProtectionBuff : ModBuff
    {
        // Используем иконку зелья выносливости (Endurance Potion) как заглушку
        public override string Texture => "Terraria/Images/Buff_114";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // Не сохраняется при выходе
            Main.debuff[Type] = false;    // Это положительный эффект
        }
    }
}