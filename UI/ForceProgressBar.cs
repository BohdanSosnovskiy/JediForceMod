using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace JediForceMod.UI
{
    public class ForceProgressBar : UIElement
    {
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            // Мы берем данные локального игрока ПРЯМО во время отрисовки
            var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();

            // Рисуем фон
            // Используем Color.White * 0.2f, чтобы фон выделялся на темной панели (эффект слота)
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height),
                Color.White * 0.2f);

            // Расчет прогресса
            // Защита от деления на ноль, если MaxExp вдруг станет 0
            int maxExp = modPlayer.ForceExperienceMax > 0 ? modPlayer.ForceExperienceMax : 1;
            float progress = (float)modPlayer.ForceExperience / maxExp;
            progress = MathHelper.Clamp(progress, 0f, 1f);

            // Рисуем заполнение
            if (progress > 0)
            {
                spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                    new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)(dimensions.Width * progress), (int)dimensions.Height),
                    Color.Cyan);
            }
        }
    }
}
