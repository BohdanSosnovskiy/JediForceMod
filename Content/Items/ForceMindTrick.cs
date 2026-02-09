using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JediForceMod.Content.NPCs;

namespace JediForceMod.Content.Items
{
    public class ForceMindTrick : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp; // Джедайский жест рукой
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.mana = ModContent.GetInstance<ForceConfig>().MindTrickManaCost;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item8; // Магический звук
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            modPlayer.UseMindTrick();

            return true;
        }
    }
}