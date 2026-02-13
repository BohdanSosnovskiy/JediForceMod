using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace JediForceMod.Content.Items
{
    public class ForceSaberThrow : ModItem
    {
        public override string Texture => "JediForceMod/Assets/UI/ForceSaberthrow";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.mana = ModContent.GetInstance<ForceConfig>().SaberThrowManaCost;
            Item.damage = ModContent.GetInstance<ForceConfig>().SaberThrowDamage;
            Item.DamageType = DamageClass.Magic;
            Item.knockBack = 3f;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item71;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            modPlayer.UseSaberThrow(Main.MouseWorld - player.Center);
            return true;
        }
    }
}