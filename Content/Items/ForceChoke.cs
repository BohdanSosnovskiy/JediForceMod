using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JediForceMod.Content.NPCs;

namespace JediForceMod.Content.Items
{
    public class ForceChoke : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp; // Рука вверх/вперед
            Item.useTime = 10; // Частота обновления (не влияет на урон напрямую, т.к. channel)
            Item.useAnimation = 10;
            Item.channel = true; // ВАЖНО: Предмет работает, пока зажата кнопка
            Item.mana = 0; // Ману тратим вручную в HoldItem
            Item.rare = ItemRarityID.Orange;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override void HoldItem(Player player)
        {
            // Работает только если игрок использует предмет (зажал кнопку)
            if (player.channel)
            {
                player.GetModPlayer<ForcePlayer>().ChokeInput = true;
            }
            else
            {
                // Если кнопку отпустили, сбрасываем цель
                player.GetModPlayer<ForcePlayer>().ChokeTargetIndex = -1;
            }
        }
    }
}