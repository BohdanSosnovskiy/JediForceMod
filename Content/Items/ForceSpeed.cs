using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JediForceMod.Content.Buffs;
using JediForceMod;
using Microsoft.Xna.Framework; // Для доступа к ForcePlayer

namespace JediForceMod.Content.Items
{
    public class ForceSpeed : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.mana = 40; // Ускорение обычно дороже толчка
            Item.scale = 0.5f; // Если картинка предмета большая, уменьшаем её визуально (0.5 = 50% размера)
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item4; // Магический звук

            Item.noMelee = true; // Чтобы сам предмет не бил как меч
            Item.noUseGraphic = true; // СКРЫВАЕТ картинку предмета в руке при использовании
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            modPlayer.UseSpeed();
            return true;
        }
    }
}
