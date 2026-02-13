using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace JediForceMod.Content.Items
{
    public class SithHolocron : ModItem
    {
        public int xpAmount = 500; // Базовое значение

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item4; // Звук использования (как кристалл маны)
        }

        public override void SaveData(TagCompound tag)
        {
            tag["xpAmount"] = xpAmount;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("xpAmount")) xpAmount = tag.GetInt("xpAmount");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "XPAmount", $"Дает {xpAmount} опыта Силы"));
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var modPlayer = player.GetModPlayer<ForcePlayer>();
                modPlayer.ReceiveSithHolocronXP(xpAmount); 
            }
            return true;
        }

        public override void AddRecipes()
        {
            // Рецепт 1: Пре-хардмод (Искажение)
            CreateRecipe()
                .AddIngredient(ItemID.Ruby, 2)
                .AddIngredient(ItemID.Obsidian, 10)
                .AddIngredient(ItemID.ShadowScale, 5)
                .AddTile(TileID.Anvils)
                .AddOnCraftCallback((recipe, item, list, destination) => {
                    if (item.ModItem is SithHolocron holocron) holocron.xpAmount = 500;
                })
                .Register();

            // Рецепт 2: Пре-хардмод (Багрянец)
            CreateRecipe()
                .AddIngredient(ItemID.Ruby, 2)
                .AddIngredient(ItemID.Obsidian, 10)
                .AddIngredient(ItemID.TissueSample, 5)
                .AddTile(TileID.Anvils)
                .AddOnCraftCallback((recipe, item, list, destination) => {
                    if (item.ModItem is SithHolocron holocron) holocron.xpAmount = 500;
                })
                .Register();

            // Рецепт 3: Хардмод (Души Ночи)
            CreateRecipe()
                .AddIngredient(ItemID.Ruby, 2)
                .AddIngredient(ItemID.Obsidian, 10)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddTile(TileID.MythrilAnvil)
                .AddOnCraftCallback((recipe, item, list, destination) => {
                    if (item.ModItem is SithHolocron holocron) holocron.xpAmount = 2500;
                })
                .Register();

            // Рецепт 4: Пост-Плантера (Эктоплазма)
            CreateRecipe()
                .AddIngredient(ItemID.Ruby, 2)
                .AddIngredient(ItemID.Obsidian, 10)
                .AddIngredient(ItemID.Ectoplasm, 2)
                .AddTile(TileID.MythrilAnvil)
                .AddOnCraftCallback((recipe, item, list, destination) => {
                    if (item.ModItem is SithHolocron holocron) holocron.xpAmount = 10000;
                })
                .Register();
        }
    }
}