using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JediForceMod.Content.Buffs;

namespace JediForceMod.Content.Items
{
    public class ForceProtection : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item4;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            modPlayer.UseProtection();
            return true;
        }
    }
}