using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace JediForceMod.Content.Items
{
    public class ForceSight : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp; // Джедай поднимает руку к голове
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<ForcePlayer>();

            if (modPlayer.SightLevel > 0)
            {

                string status = modPlayer.ToggleSight() ? "активировано" : "деактивировано";
                Main.NewText($"Зрение Силы {status}", Color.Cyan);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
            }
            else
            {
                Main.NewText("Вы еще не обучены Зрению Силы!", Color.Red);
            }
            return true;
        }
    }
}
