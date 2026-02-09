using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace JediForceMod.UI
{
    public class ForceUI : UIState
    {
        public UIPanel MainPanel;
        public static bool Visible = false;

        public override void OnInitialize()
        {
            MainPanel = new UIPanel();
            MainPanel.Width.Set(500, 0f);
            MainPanel.Height.Set(480, 0f); // Увеличили высоту для нового раздела
            MainPanel.HAlign = 0.5f; // Центр экрана
            MainPanel.VAlign = 0.5f;
            MainPanel.BackgroundColor = new Color(10, 20, 40, 220); // Темный "космический" цвет

            // Заголовок в стиле SW
            UIText title = new UIText("Навыки Силы", 1.2f);
            title.Top.Set(10, 0f);
            title.HAlign = 0.5f;
            MainPanel.Append(title);

            // Кнопка сброса (RESET) для тестов
            UITextPanel<string> resetButton = new UITextPanel<string>("RESET");
            resetButton.Width.Set(70, 0f);
            resetButton.Height.Set(30, 0f);
            resetButton.Top.Set(10, 0f);
            resetButton.Left.Set(-90, 1f); // Прижимаем к правому краю (отступ 90px от правой границы)
            resetButton.BackgroundColor = new Color(135, 0, 0); // Темно-красный цвет

            resetButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                modPlayer.PushLevel = 0;
                modPlayer.SpeedLevel = 0;
                modPlayer.SightLevel = 0;
                modPlayer.HealLevel = 0;
                modPlayer.LightningLevel = 0;
                modPlayer.JumpLevel = 0;
                modPlayer.ChokeLevel = 0;
                modPlayer.MindTrickLevel = 0;
                modPlayer.ProtectionLevel = 0;
                modPlayer.SaberDamageLevel = 0;
                modPlayer.SaberSpeedLevel = 0;
                modPlayer.SaberPenetrationLevel = 0;
                modPlayer.SaberSizeLevel = 0;
                // Сброс новой системы
                for(int i=0; i<10; i++) {
                    modPlayer.SkillLevels[i] = 0;
                    modPlayer.SkillExp[i] = 0;
                }
                modPlayer.ForcePoints = 5; // Возвращаем стартовые 5 очков (как при создании)
                modPlayer.ForceExperience = 0;
                modPlayer.ForceExperienceMax = 100;
                Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
            };
            MainPanel.Append(resetButton);

            // Добавляем иконку Толчка
            string[] pushDescs = {
                "Изучите, чтобы отталкивать слабых врагов.",
                "Увеличивает дистанцию и силу толчка.",
                "Позволяет сбивать врагов с ног.",
                "МАСТЕР: Огромная мощь, наносит урон при ударе об стену."
            };
            Asset<Texture2D> pushAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Push", AssetRequestMode.ImmediateLoad);
            ForceButton pushButton = new ForceButton(pushAsset.Value, 0, "Толчок Силы", pushDescs);
            pushButton.Left.Set(20, 0f);
            pushButton.Top.Set(80, 0f);

            pushButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[0] == 0)
                    {
                        // Открываем способность
                        modPlayer.SkillLevels[0] = 1; 
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[0] < 15)
                    {
                        // Улучшаем способность за очко (Мгновенный уровень)
                        int needed = modPlayer.SkillMaxExp[0] - modPlayer.SkillExp[0];
                        modPlayer.AddSkillXP(0, needed); // Добавляем ровно столько, сколько нужно для апа
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(pushButton);

            // Для Скорости (Тип 1)
            // Описания для Скорости
            string[] speedDescs = {
                "Ускоряет ваши движения на короткое время.",
                "Время замедляется, вы двигаетесь быстрее.",
                "Вы бежите так быстро, что можете ходить по воде.",
                "МАСТЕР: Время почти замирает для всех, кроме вас."
            };
            Asset<Texture2D> speedAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Speed", AssetRequestMode.ImmediateLoad);
            ForceButton speedButton = new ForceButton(speedAsset.Value, 1, "Скорость Силы", speedDescs);
            speedButton.Left.Set(80, 0f); // Сдвинь вправо, чтобы не накладывались
            speedButton.Top.Set(80, 0f);

            speedButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[1] == 0)
                    {
                        modPlayer.SkillLevels[1] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[1] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[1] - modPlayer.SkillExp[1];
                        modPlayer.AddSkillXP(1, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };

            MainPanel.Append(speedButton);

            string[] sightDescs = {
                "Позволяет чувствовать присутствие живых существ (Подсветка врагов).",
                "Вы чувствуете опасность вокруг (Подсветка ловушек).",
                "Вы видите сквозь камни (Подсветка руды).",
                "МАСТЕР: Полное единение с Силой."
            };
            Asset<Texture2D> sightAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Sight", AssetRequestMode.ImmediateLoad);
            // Тип 2 для Зрения
            ForceButton sightButton = new ForceButton(sightAsset.Value, 2, "Зрение Силы", sightDescs);
            sightButton.Left.Set(140, 0f); // Ставим рядом с Толчком и Скоростью
            sightButton.Top.Set(80, 0f);

            sightButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[2] == 0)
                    {
                        modPlayer.SkillLevels[2] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Unlock);
                    }
                    else if (modPlayer.SkillLevels[2] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[2] - modPlayer.SkillExp[2];
                        modPlayer.AddSkillXP(2, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(sightButton);

            // Тип 3 для Лечения
            string[] healDescs = {
                "Позволяет залечивать легкие раны.",
                "Усиливает эффективность лечения.",
                "Значительно восстанавливает здоровье.",
                "МАСТЕР: Быстрое восстановление и сниженный откат."
            };
            // Убедись, что у тебя есть картинка Heal.png или используй заглушку
            Asset<Texture2D> healAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Heal", AssetRequestMode.ImmediateLoad);
            ForceButton healButton = new ForceButton(healAsset.Value, 3, "Исцеление", healDescs);
            healButton.Left.Set(200, 0f); // Сдвигаем правее Зрения
            healButton.Top.Set(80, 0f);

            healButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[3] == 0)
                    {
                        modPlayer.SkillLevels[3] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[3] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[3] - modPlayer.SkillExp[3];
                        modPlayer.AddSkillXP(3, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(healButton);

            // Тип 4 для Молнии
            string[] lightningDescs = {
                "Выпускает разряд молнии в ближайшего врага.",
                "Молния перескакивает на большее число врагов.",
                "Увеличенный урон и радиус поражения.",
                "МАСТЕР: Шторм Силы, уничтожающий группы врагов."
            };
            // Убедись, что есть иконка Lightning.png
            Asset<Texture2D> lightningAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Lightning", AssetRequestMode.ImmediateLoad);
            ForceButton lightningButton = new ForceButton(lightningAsset.Value, 4, "Молния Силы", lightningDescs);
            lightningButton.Left.Set(260, 0f); // Сдвигаем правее Лечения
            lightningButton.Top.Set(80, 0f);

            lightningButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[4] == 0)
                    {
                        modPlayer.SkillLevels[4] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[4] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[4] - modPlayer.SkillExp[4];
                        modPlayer.AddSkillXP(4, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(lightningButton);

            // Тип 5 для Прыжка
            string[] jumpDescs = {
                "Позволяет совершать усиленный прыжок.",
                "Прыжок еще выше, нет урона от падения.",
                "Максимальная высота прыжка.",
                "МАСТЕР: Невероятная мобильность."
            };
            // Убедись, что есть иконка Jump.png
            Asset<Texture2D> jumpAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Jump", AssetRequestMode.ImmediateLoad);
            ForceButton jumpButton = new ForceButton(jumpAsset.Value, 5, "Прыжок Силы", jumpDescs);
            jumpButton.Left.Set(320, 0f); // Сдвигаем правее Молнии
            jumpButton.Top.Set(80, 0f);

            jumpButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[5] == 0)
                    {
                        modPlayer.SkillLevels[5] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[5] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[5] - modPlayer.SkillExp[5];
                        modPlayer.AddSkillXP(5, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(jumpButton);

            // Тип 6 для Удушения
            string[] chokeDescs = {
                "Поднимает врага в воздух и наносит урон.",
                "Увеличенный урон и дальность захвата.",
                "Быстрое уничтожение одиночных целей.",
                "МАСТЕР: Враг полностью беспомощен перед мощью Силы."
            };
            // Убедись, что есть иконка Choke.png
            Asset<Texture2D> chokeAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Choke", AssetRequestMode.ImmediateLoad);
            ForceButton chokeButton = new ForceButton(chokeAsset.Value, 6, "Удушение", chokeDescs);
            chokeButton.Left.Set(380, 0f); // Сдвигаем правее Прыжка
            chokeButton.Top.Set(80, 0f);

            chokeButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[6] == 0)
                    {
                        modPlayer.SkillLevels[6] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[6] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[6] - modPlayer.SkillExp[6];
                        modPlayer.AddSkillXP(6, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(chokeButton);

            // Тип 7 для Обмана Разума
            string[] mindTrickDescs = {
                "Заставляет врагов терять интерес к вам.",
                "Враги атакуют друг друга.",
                "Увеличенная длительность и радиус.",
                "МАСТЕР: Полный контроль над разумом слабых."
            };
            // Убедись, что есть иконка MindTrick.png
            Asset<Texture2D> mindTrickAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/MindTrick", AssetRequestMode.ImmediateLoad);
            ForceButton mindTrickButton = new ForceButton(mindTrickAsset.Value, 7, "Обман Разума", mindTrickDescs);
            mindTrickButton.Left.Set(440, 0f); // Сдвигаем правее Удушения
            mindTrickButton.Top.Set(80, 0f);

            mindTrickButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[7] == 0)
                    {
                        modPlayer.SkillLevels[7] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[7] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[7] - modPlayer.SkillExp[7];
                        modPlayer.AddSkillXP(7, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(mindTrickButton);

            // Тип 8 для Защиты
            string[] protectDescs = {
                "Поглощает часть урона за счет маны (50%).",
                "Поглощает 75% урона. Эффективнее расход маны.",
                "Поглощает 87.5% урона. Иммунитет к отбрасыванию.",
                "МАСТЕР: Вы почти неуязвимы, пока есть Сила."
            };
            // Убедись, что есть иконка Protection.png
            Asset<Texture2D> protectAsset = ModContent.Request<Texture2D>("JediForceMod/Assets/UI/Protection", AssetRequestMode.ImmediateLoad);
            ForceButton protectButton = new ForceButton(protectAsset.Value, 8, "Защита Силы", protectDescs);
            protectButton.Left.Set(20, 0f); // Новый ряд
            protectButton.Top.Set(150, 0f); // Ниже первого ряда

            protectButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    if (modPlayer.SkillLevels[8] == 0)
                    {
                        modPlayer.SkillLevels[8] = 1;
                        modPlayer.UpdateTiers();
                        modPlayer.ForcePoints--;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
                    }
                    else if (modPlayer.SkillLevels[8] < 15)
                    {
                        int needed = modPlayer.SkillMaxExp[8] - modPlayer.SkillExp[8];
                        modPlayer.AddSkillXP(8, needed);
                        modPlayer.ForcePoints--;
                    }
                }
            };
            MainPanel.Append(protectButton);

            // Тип 13 для Броска Меча
            string[] throwDescs = {
                "Бросает световой меч, который возвращается обратно.",
                "Увеличенный урон и дальность полета.",
                "Меч проходит сквозь врагов.",
                "МАСТЕР: Меч самонаводится на врагов при возвращении."
            };
            // Используем иконку Enchanted Boomerang как заглушку
            Asset<Texture2D> throwAsset = ModContent.Request<Texture2D>("Terraria/Images/Item_" + ItemID.EnchantedBoomerang, AssetRequestMode.ImmediateLoad);
            ForceButton throwButton = new ForceButton(throwAsset.Value, 13, "Бросок Меча", throwDescs);
            throwButton.Left.Set(80, 0f); // Второй ряд
            throwButton.Top.Set(150, 0f);

            throwButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0)
                {
                    HandleSkillClick(modPlayer, 9); // 9 - индекс в массиве SkillLevels
                }
            };
            MainPanel.Append(throwButton);

            // --- РАЗДЕЛ МАСТЕРСТВА МЕЧА ---
            UIText saberTitle = new UIText("Мастерство меча", 1.0f);
            saberTitle.Top.Set(220, 0f);
            saberTitle.HAlign = 0.5f;
            MainPanel.Append(saberTitle);

            int saberY = 250;

            // 1. Урон
            string[] dmgDescs = { 
                "Ур 1: Авто-атака для мечей.",
                "Ур 2: +15% дополнительного урона.",
                "Ур 3: Накладывает Горение при ударе.",
                "Ур 4: Накладывает Ихор (снижение защиты).",
                "Ур 5: Накладывает Проклятый Огонь."
            };
            // Используем иконку меча (заглушка или создайте SaberDamage.png)
            Asset<Texture2D> dmgAsset = ModContent.Request<Texture2D>("Terraria/Images/Item_" + ItemID.GoldBroadsword, AssetRequestMode.ImmediateLoad);
            ForceButton dmgButton = new ForceButton(dmgAsset.Value, 9, "Урон Меча", dmgDescs, 5);
            dmgButton.Left.Set(80, 0f);
            dmgButton.Top.Set(saberY, 0f);
            dmgButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0 && modPlayer.SaberDamageLevel < 5)
                {
                    modPlayer.SaberDamageLevel++;
                    modPlayer.ForcePoints--;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };
            MainPanel.Append(dmgButton);

            // 2. Скорость
            string[] spdDescs = { "Увеличивает скорость атаки (+5% за уровень)." };
            Asset<Texture2D> spdAsset = ModContent.Request<Texture2D>("Terraria/Images/Item_" + ItemID.HermesBoots, AssetRequestMode.ImmediateLoad);
            ForceButton spdButton = new ForceButton(spdAsset.Value, 10, "Скорость Атаки", spdDescs, 5);
            spdButton.Left.Set(170, 0f);
            spdButton.Top.Set(saberY, 0f);
            spdButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0 && modPlayer.SaberSpeedLevel < 5)
                {
                    modPlayer.SaberSpeedLevel++;
                    modPlayer.ForcePoints--;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };
            MainPanel.Append(spdButton);

            // 3. Пробитие
            string[] penDescs = { "Игнорирует броню врага (+3 за уровень)." };
            Asset<Texture2D> penAsset = ModContent.Request<Texture2D>("Terraria/Images/Item_" + ItemID.SharkToothNecklace, AssetRequestMode.ImmediateLoad);
            ForceButton penButton = new ForceButton(penAsset.Value, 11, "Пробитие Брони", penDescs, 5);
            penButton.Left.Set(260, 0f);
            penButton.Top.Set(saberY, 0f);
            penButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0 && modPlayer.SaberPenetrationLevel < 5)
                {
                    modPlayer.SaberPenetrationLevel++;
                    modPlayer.ForcePoints--;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };
            MainPanel.Append(penButton);

            // 4. Размер
            string[] sizeDescs = { "Увеличивает размер меча (+5% за уровень)." };
            Asset<Texture2D> sizeAsset = ModContent.Request<Texture2D>("Terraria/Images/Item_" + ItemID.BreakerBlade, AssetRequestMode.ImmediateLoad);
            ForceButton sizeButton = new ForceButton(sizeAsset.Value, 12, "Размер Меча", sizeDescs, 5);
            sizeButton.Left.Set(350, 0f);
            sizeButton.Top.Set(saberY, 0f);
            sizeButton.OnLeftClick += (evt, element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                if (modPlayer.ForcePoints > 0 && modPlayer.SaberSizeLevel < 5)
                {
                    modPlayer.SaberSizeLevel++;
                    modPlayer.ForcePoints--;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };
            MainPanel.Append(sizeButton);

            // 1. Текст "Force Points"
            UIText pointsText = new UIText("Очки Силы: 0", 1f);
            pointsText.Top.Set(40, 0f);
            pointsText.Left.Set(20, 0f);
            // Динамическое обновление текста
            pointsText.OnUpdate += (element) => {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                ((UIText)element).SetText($"Очки Силы: {modPlayer.ForcePoints}");
            };
            MainPanel.Append(pointsText);

            // 2. Текст "Experience"
            UIText expText = new UIText("Опыт", 0.8f);
            expText.Left.Set(20, 0f);
            expText.Top.Set(-55, 1f); // 55 пикселей от низа
            MainPanel.Append(expText);

            ForceProgressBar progressBar = new ForceProgressBar();
            progressBar.Width.Set(-40, 1f); // Ширина панели минус отступы
            progressBar.Height.Set(14, 0f);
            progressBar.Left.Set(20, 0f);
            progressBar.Top.Set(-35, 1f); // Почти в самом низу
            MainPanel.Append(progressBar);



            Append(MainPanel);


        }

        // Вспомогательный метод для обработки кликов по навыкам
        private void HandleSkillClick(ForcePlayer modPlayer, int skillIndex)
        {
            if (modPlayer.SkillLevels[skillIndex] == 0)
            {
                modPlayer.SkillLevels[skillIndex] = 1;
                modPlayer.UpdateTiers();
                modPlayer.ForcePoints--;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
            }
            else if (modPlayer.SkillLevels[skillIndex] < 15)
            {
                int needed = modPlayer.SkillMaxExp[skillIndex] - modPlayer.SkillExp[skillIndex];
                modPlayer.AddSkillXP(skillIndex, needed);
                modPlayer.ForcePoints--;
            }
        }
    }
}
