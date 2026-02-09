using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace JediForceMod.Content.Items
{
    public class ForceLightning : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Shoot; // Анимация стрельбы (рука вперед)
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.autoReuse = true; // Позволяет зажать кнопку для непрерывного использования
            Item.mana = ModContent.GetInstance<ForceConfig>().LightningManaCost;
            Item.damage = ModContent.GetInstance<ForceConfig>().LightningDamage;
            Item.DamageType = DamageClass.Magic;
            Item.knockBack = 2f;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item122; // Звук электрического оружия
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileID.PurificationPowder; // Заглушка
            Item.shootSpeed = 10f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            modPlayer.UseLightning(velocity);

            return false;
        }
    }
}