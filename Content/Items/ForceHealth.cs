using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace JediForceMod.Content.Items
{
    public class ForceHealth : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.mana = 0; // Мана теперь тратится постепенно в ForcePlayer, а не при клике
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item4; // Магический звук
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            modPlayer.UseHeal();
            return true;
        }
    }
}