using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JediForceMod; // Для доступа к ForcePlayer

namespace JediForceMod.Content.Buffs
{
    public class ForceSpeedBuff : ModBuff
    {
        // ВРЕМЕННОЕ ИСПРАВЛЕНИЕ: Используем стандартную иконку "Swiftness", пока вы не уменьшите свою до 32x32.
        // Когда уменьшите файл ForceSpeedBuff.png, удалите эту строку.
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Swiftness;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // Бафф не сохраняется при выходе
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var config = ModContent.GetInstance<ForceConfig>();
            var modPlayer = player.GetModPlayer<ForcePlayer>();

            // Базовый множитель скорости
            float multiplier = config.SpeedMultiplier;
            // Добавляем бонус за уровень (например, +0.2 к множителю за каждый уровень)
            multiplier += (modPlayer.SpeedLevel * 0.2f);

            // Увеличиваем максимальную скорость бега и ускорение
            player.accRunSpeed *= multiplier;
            player.maxRunSpeed *= multiplier;
            player.runAcceleration *= multiplier;

            // Можно добавить визуальный эффект (пыль под ногами)
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(player.position, player.width, player.height, 15, 0, 0, 150, default, 1.3f);
            }
        }
    }
}
