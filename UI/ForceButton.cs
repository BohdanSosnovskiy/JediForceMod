using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace JediForceMod.UI
{
    public class ForceButton : UIElement
    {
        private Texture2D _icon;
        private int _powerType; // 0 - Push, 1 - Speed и т.д.
        private string _powerName;
        private string[] _descriptions;
        private int _maxLevel; // Максимальный уровень для отображения полосок

        public ForceButton(Texture2D icon, int type, string name, string[] descriptions, int maxLevel = 3)
        {
            _icon = icon;
            _powerType = type;
            _powerName = name;
            _descriptions = descriptions;
            _maxLevel = maxLevel;
            Width.Set(44, 0f);
            Height.Set(60, 0f); // Чуть выше, чтобы влезли полоски
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            CalculatedStyle dimensions = GetDimensions();
            var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();

            int currentLevel = 0;
            if (_powerType == 0) currentLevel = modPlayer.PushLevel;
            else if (_powerType == 1) currentLevel = modPlayer.SpeedLevel;
            else if (_powerType == 2) currentLevel = modPlayer.SightLevel; // Наше новое зрение
            else if (_powerType == 3) currentLevel = modPlayer.HealLevel; // Лечение
            else if (_powerType == 4) currentLevel = modPlayer.LightningLevel; // Молния
            else if (_powerType == 5) currentLevel = modPlayer.JumpLevel; // Прыжок
            else if (_powerType == 6) currentLevel = modPlayer.ChokeLevel; // Удушение
            else if (_powerType == 7) currentLevel = modPlayer.MindTrickLevel; // Обман Разума
            else if (_powerType == 8) currentLevel = modPlayer.ProtectionLevel; // Защита
            else if (_powerType == 9) currentLevel = modPlayer.SaberDamageLevel;
            else if (_powerType == 10) currentLevel = modPlayer.SaberSpeedLevel;
            else if (_powerType == 11) currentLevel = modPlayer.SaberPenetrationLevel;
            else if (_powerType == 12) currentLevel = modPlayer.SaberSizeLevel;
            else if (_powerType == 13) currentLevel = modPlayer.SaberThrowLevel; // Бросок Меча

            // Определяем индекс в массиве SkillLevels/SkillExp
            int skillArrayIndex = -1;
            if (_powerType >= 0 && _powerType <= 8) skillArrayIndex = _powerType;
            else if (_powerType == 13) skillArrayIndex = 9; // Бросок меча (тип 13) лежит в индексе 9

            // Получаем сырой уровень мастерства для проверки на легендарность (15+)
            int rawSkillLevel = 0;
            if (skillArrayIndex != -1 && skillArrayIndex < modPlayer.SkillLevels.Length)
            {
                rawSkillLevel = modPlayer.SkillLevels[skillArrayIndex];
            }

            // Если мышка наведена на эту кнопку
            if (IsMouseHovering)
            {
                // Ограничиваем индекс, чтобы не выйти за пределы массива (0-3)
                int safeLevel = (int)MathHelper.Clamp(currentLevel, 0, _descriptions.Length - 1);

                // Формируем текст подсказки
                string tooltip = $"[c/00FFFF:{_powerName}]\n"; // Имя силы бирюзовым цветом
                tooltip += $"Текущий уровень: {currentLevel}/{_maxLevel}\n";
                if (safeLevel < _descriptions.Length) tooltip += _descriptions[safeLevel];

                if (rawSkillLevel > 0)
                {
                    int currentExp = modPlayer.SkillExp[skillArrayIndex];
                    int maxExp = modPlayer.SkillMaxExp[skillArrayIndex];
                    tooltip += $"\nМастерство: {rawSkillLevel} (XP: {currentExp}/{maxExp})";
                }

                if (rawSkillLevel >= 15)
                {
                    tooltip += "\n[c/FFD700:ЛЕГЕНДАРНЫЙ НАВЫК!]";
                }

                // Проверяем, является ли навык "Мастерством меча" (покупается за очки, без XP)
                bool isSaberMastery = (_powerType >= 9 && _powerType <= 12);

                if (modPlayer.ForcePoints > 0)
                {
                    if (isSaberMastery && currentLevel < _maxLevel) tooltip += "\n[c/00FF00:Нажмите, чтобы улучшить (1 Очко)]";
                    else if (!isSaberMastery && rawSkillLevel == 0) tooltip += "\n[c/00FF00:Нажмите, чтобы изучить (1 Очко)]";
                    else if (!isSaberMastery && rawSkillLevel < 15) tooltip += "\n[c/00FF00:Нажмите, чтобы улучшить (1 Очко)]";
                }

                // Рисуем стандартную подсказку Terraria рядом с курсором
                Main.instance.MouseText(tooltip);
            }

            // Определяем цвет кнопки: если нет очков и уровень не макс, делаем серым
            Color drawColor = Color.White;
            if (modPlayer.ForcePoints <= 0 && currentLevel < _maxLevel)
            {
                drawColor = Color.Gray;
            }

            // 1. Рисуем РАМКУ (44x44)
            string framePath = "JediForceMod/Assets/UI/Frame";
            if (rawSkillLevel >= 15)
            {
                framePath = "JediForceMod/Assets/UI/Frame_use"; // Золотая рамка
            }

            var frameTexture = ModContent.Request<Texture2D>(framePath).Value;
            spriteBatch.Draw(frameTexture, new Rectangle((int)dimensions.X, (int)dimensions.Y, 44, 44), drawColor);

            // 2. Рисуем ИКОНКУ (32x32)
            // Чтобы отцентрировать 32x32 внутри 44x44, нужно добавить отступ 6 пикселей:
            // (44 - 32) / 2 = 6
            if (_icon != null)
            {
                spriteBatch.Draw(_icon,
                    new Rectangle((int)dimensions.X + 6, (int)dimensions.Y + 6, 32, 32),
                    drawColor);
            }

            // 3. Полоски уровня (чуть ниже рамки)
            int level = currentLevel;

            // Динамический расчет ширины полосок
            int barWidth = (44 - ((_maxLevel - 1) * 2)) / _maxLevel; 
            for (int i = 0; i < _maxLevel; i++)
            {
                Color barColor = (i < level) ? Color.Cyan : Color.DimGray;
                // Рисуем деления уровня под кнопкой
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value,
                    new Rectangle((int)dimensions.X + (i * (barWidth + 2)), (int)dimensions.Y + 48, barWidth, 6),
                    barColor);
            }

            // 4. Полоска опыта навыка (Skill XP) - только если навык изучен
            if (rawSkillLevel > 0 && skillArrayIndex != -1 && skillArrayIndex < modPlayer.SkillExp.Length)
            {
                int currentExp = modPlayer.SkillExp[skillArrayIndex];
                int maxExp = modPlayer.SkillMaxExp[skillArrayIndex];
                if (maxExp <= 0) maxExp = 1;

                float xpProgress = (float)currentExp / maxExp;
                xpProgress = MathHelper.Clamp(xpProgress, 0f, 1f);

                // Фон полоски (черный полупрозрачный)
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value,
                    new Rectangle((int)dimensions.X, (int)dimensions.Y + 56, 44, 4),
                    Color.Black * 0.6f);

                // Заполнение (Золотой цвет)
                if (xpProgress > 0)
                {
                    spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value,
                        new Rectangle((int)dimensions.X, (int)dimensions.Y + 56, (int)(44 * xpProgress), 4),
                        Color.Gold);
                }
            }
        }
    }
}
