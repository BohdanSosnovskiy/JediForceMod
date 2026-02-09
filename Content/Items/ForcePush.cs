using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using JediForceMod.Content.NPCs;

namespace JediForceMod.Content.Items
{
    public class ForcePush : ModItem
    {
        public override void SetDefaults()
        {
            // Базовые настройки предмета
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp; // Анимация использования (рука вверх)
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = false; // Нельзя спамить зажатием (как в игре)

            // Магические свойства
            Item.mana = ModContent.GetInstance<ForceConfig>().PushManaCost; // Потребление маны
            Item.DamageType = DamageClass.Magic;
            Item.damage = 1; // Минимальный урон, чтобы срабатывал knockback
            Item.knockBack = ModContent.GetInstance<ForceConfig>().PushKnockback; // Огромный импульс отбрасывания

            Item.noMelee = true; // Чтобы сам предмет не бил как меч
            Item.noUseGraphic = true; // СКРЫВАЕТ картинку предмета в руке при использовании

            // Звук и снаряд
            Item.UseSound = SoundID.Item8; // В будущем заменим на звук из SW
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 10f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();
            
            // Теперь предмет просто вызывает метод игрока
            modPlayer.UsePush(velocity);

            // Возвращаем false, чтобы стандартный снаряд (картинка) НЕ вылетал
            return false;
        }

        public override void AddRecipes()
        {
            // Рецепт создания (для теста: 10 дерева на верстаке)
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
