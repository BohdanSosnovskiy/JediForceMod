using System;
using System.Collections.Generic;
using JediForceMod.Content.NPCs;
using JediForceMod.Content.Buffs;
using JediForceMod.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;

namespace JediForceMod
{
    public class ForcePlayer : ModPlayer
    {
        // В начало класса ForcePlayer добавь:
        public int SightLevel = 0;
        public int HealLevel = 0;
        public int LightningLevel = 0;
        public int JumpLevel = 0;
        public int ChokeLevel = 0;
        public int MindTrickLevel = 0;
        public int ProtectionLevel = 0;
        public int SaberThrowLevel = 0;
        private bool _wasProtectionActive = false; // Для отслеживания момента окончания баффа
        public int ChokeTargetIndex = -1; // Индекс NPC, которого мы душим
        private bool _sightActive = false;
        public bool HealActive = false; // Активно ли сейчас лечение
        public int HealTimer = 0;       // Таймер длительности текущего сеанса лечения

        // Уровни способностей (от 0 до 3, как в игре)
        public int PushLevel = 0;
        public int SpeedLevel = 0;
        public int ForcePoints = 0; // Очки для распределения

        public int ForceExperience = 0;
        public int ForceExperienceMax = 100; // Сколько нужно для нового очка

        // Таймер для эффекта скорости (вместо баффа)
        public int SpeedEffectTimer = 0;

        // Анимация руки при использовании способностей
        public int ForceAnimationTimer = 0;
        public int ForceAnimationStyle = 0; // 0 = None, 1 = HoldUp, 2 = Shoot

        // Флаг для удушения (удерживается ли кнопка/предмет)
        public bool ChokeInput = false;

        // Таймер для авто-стрельбы молнией через хоткей
        public int LightningTimer = 0;

        // Для отслеживания состояния в воздухе (для легендарного прыжка)
        public bool wasAirborne = false;

        // --- НОВАЯ СИСТЕМА ПРОКАЧКИ ---
        public int[] SkillLevels = new int[10]; // Уровни мастерства (0..9). Индекс 9 - Saber Throw
        public int[] SkillExp = new int[10];
        public int[] SkillMaxExp = new int[10];

        // --- МАСТЕРСТВО СВЕТОВОГО МЕЧА ---
        public int SaberDamageLevel = 0;
        public int SaberSpeedLevel = 0;
        public int SaberPenetrationLevel = 0;
        public int SaberSizeLevel = 0;

        // Сохранение данных персонажа
        public override void SaveData(TagCompound tag)
        {
            tag["PushLevel"] = PushLevel;
            tag["SpeedLevel"] = SpeedLevel;
            tag["ForcePoints"] = ForcePoints;
            tag["ForceExperience"] = ForceExperience;
            tag["ForceExperienceMax"] = ForceExperienceMax;
            tag["SightLevel"] = SightLevel;
            tag["HealLevel"] = HealLevel;
            tag["LightningLevel"] = LightningLevel;
            tag["JumpLevel"] = JumpLevel;
            tag["ChokeLevel"] = ChokeLevel;
            tag["MindTrickLevel"] = MindTrickLevel;
            tag["ProtectionLevel"] = ProtectionLevel;
            tag["SkillLevels"] = SkillLevels;
            tag["SkillExp"] = SkillExp;
            tag["SkillMaxExp"] = SkillMaxExp;
            tag["SaberDamageLevel"] = SaberDamageLevel;
            tag["SaberSpeedLevel"] = SaberSpeedLevel;
            tag["SaberPenetrationLevel"] = SaberPenetrationLevel;
            tag["SaberSizeLevel"] = SaberSizeLevel;
        }

        public override void LoadData(TagCompound tag)
        {
            PushLevel = tag.GetInt("PushLevel");
            SpeedLevel = tag.GetInt("SpeedLevel");
            ForcePoints = tag.GetInt("ForcePoints");
            ForceExperience = tag.GetInt("ForceExperience");
            ForceExperienceMax = tag.GetInt("ForceExperienceMax");
            SightLevel = tag.GetInt("SightLevel");
            HealLevel = tag.GetInt("HealLevel");
            LightningLevel = tag.GetInt("LightningLevel");
            JumpLevel = tag.GetInt("JumpLevel");
            ChokeLevel = tag.GetInt("ChokeLevel");
            MindTrickLevel = tag.GetInt("MindTrickLevel");
            ProtectionLevel = tag.GetInt("ProtectionLevel");
            
            // Загрузка массивов (с проверкой на null для старых сохранений)
            if (tag.ContainsKey("SkillLevels")) 
            {
                var loaded = tag.GetIntArray("SkillLevels");
                if (loaded.Length < 10) Array.Resize(ref loaded, 10);
                SkillLevels = loaded;
            }
            if (tag.ContainsKey("SkillExp"))
            {
                var loaded = tag.GetIntArray("SkillExp");
                if (loaded.Length < 10) Array.Resize(ref loaded, 10);
                SkillExp = loaded;
            }
            if (tag.ContainsKey("SkillMaxExp"))
            {
                var loaded = tag.GetIntArray("SkillMaxExp");
                if (loaded.Length < 10) Array.Resize(ref loaded, 10);
                SkillMaxExp = loaded;
            }
            SaberDamageLevel = tag.GetInt("SaberDamageLevel");
            SaberSpeedLevel = tag.GetInt("SaberSpeedLevel");
            SaberPenetrationLevel = tag.GetInt("SaberPenetrationLevel");
            SaberSizeLevel = tag.GetInt("SaberSizeLevel");

            // ИСПРАВЛЕНИЕ: Если загрузился старый персонаж или произошла ошибка, Max может быть 0.
            // Это вызывает мгновенное получение уровня при ударе. Ставим 100 по умолчанию.
            if (ForceExperienceMax <= 0) ForceExperienceMax = 100;

            // Инициализация MaxExp, если они 0 (для новых или старых персов)
            for (int i = 0; i < 10; i++)
            {
                if (SkillMaxExp[i] <= 0) SkillMaxExp[i] = 250; // Увеличили базу с 100 до 250
            }

            // МИГРАЦИЯ: Если у игрока есть старые уровни (PushLevel > 0), но SkillLevels == 0, синхронизируем
            if (SkillLevels[0] == 0 && PushLevel > 0) SyncLegacyLevels();
        }

        private void SyncLegacyLevels()
        {
            // Конвертируем старые уровни (1, 2, 3) в уровни мастерства (1, 5, 10)
            SkillLevels[0] = LevelToSkill(PushLevel);
            SkillLevels[1] = LevelToSkill(SpeedLevel);
            SkillLevels[2] = LevelToSkill(SightLevel);
            SkillLevels[3] = LevelToSkill(HealLevel);
            SkillLevels[4] = LevelToSkill(LightningLevel);
            SkillLevels[5] = LevelToSkill(JumpLevel);
            SkillLevels[6] = LevelToSkill(ChokeLevel);
            SkillLevels[7] = LevelToSkill(MindTrickLevel);
            SkillLevels[8] = LevelToSkill(ProtectionLevel);
        }

        private int LevelToSkill(int level)
        {
            if (level == 1) return 1;
            if (level == 2) return 5;
            if (level == 3) return 10;
            return 0;
        }

        // Обновляем "Тиры" способностей на основе мастерства
        public void UpdateTiers()
        {
            PushLevel = GetTier(SkillLevels[0]);
            SpeedLevel = GetTier(SkillLevels[1]);
            SightLevel = GetTier(SkillLevels[2]);
            HealLevel = GetTier(SkillLevels[3]);
            LightningLevel = GetTier(SkillLevels[4]);
            JumpLevel = GetTier(SkillLevels[5]);
            ChokeLevel = GetTier(SkillLevels[6]);
            MindTrickLevel = GetTier(SkillLevels[7]);
            ProtectionLevel = GetTier(SkillLevels[8]);
            SaberThrowLevel = GetTier(SkillLevels[9]);
        }

        private int GetTier(int skillLevel)
        {
            if (skillLevel <= 0) return 0; // Не изучено
            if (skillLevel < 5) return 1;  // Ур 1-4 -> Тир 1
            if (skillLevel < 10) return 2; // Ур 5-9 -> Тир 2
            return 3;                      // Ур 10+ -> Тир 3
        }

        // Используем PostUpdateEquips для изменения характеристик движения и визуальных эффектов
        public override void PostUpdateEquips()
        {
            if (SpeedEffectTimer > 0)
            {
                var config = ModContent.GetInstance<ForceConfig>();

                // Базовый множитель скорости
                float multiplier = config.SpeedMultiplier;
                // Добавляем бонус за уровень
                multiplier += (SpeedLevel * 0.2f);

                // Увеличиваем максимальную скорость бега и ускорение
                Player.accRunSpeed *= multiplier;
                Player.maxRunSpeed *= multiplier;
                Player.runAcceleration *= multiplier;

                // Включаем эффект шлейфа (Afterimage), как у брони Ниндзя или Теневой брони
                Player.armorEffectDrawShadow = true;

                // Если уровень 3 (Мастер), позволяем бегать по воде
                if (SpeedLevel >= 3)
                {
                    Player.waterWalk = true;
                    Player.noFallDmg = true;
                }

                // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Поток Силы
                // Дает шанс уклонения (как Shadow Dodge) и сверхскорость
                if (SkillLevels[1] >= 15)
                {
                    Player.onHitDodge = true; 
                    Player.moveSpeed += 0.5f;
                }
            }

            // --- ПРЫЖОК СИЛЫ (Пассивный) ---
            if (JumpLevel > 0)
            {
                // Увеличиваем высоту прыжка
                // Player.jumpSpeedBoost добавляет скорость к прыжку
                float boost = ModContent.GetInstance<ForceConfig>().JumpStrength * JumpLevel;
                Player.jumpSpeedBoost += boost;

                // Уровень 2+: Нет урона от падения
                if (JumpLevel >= 2)
                {
                    Player.noFallDmg = true;
                }

                // Прокачка прыжка: даем опыт, если игрок прыгает
                if (Player.velocity.Y != 0 && Player.controlJump && Main.rand.NextBool(30))
                {
                    AddSkillXP(5, 1); // 5 - индекс Прыжка
                }
            }

            // --- БОНУСЫ СВЕТОВОГО МЕЧА (Уровень 1: Авто-атака) ---
            if (SaberDamageLevel >= 1)
            {
                Player.autoReuseGlove = true; // Включает авто-атаку для оружия ближнего боя
            }

            // --- БОНУСЫ СВЕТОВОГО МЕЧА (Скорость) ---
            if (SaberSpeedLevel > 0)
            {
                // Увеличиваем скорость атаки для ближнего боя
                Player.GetAttackSpeed(DamageClass.Melee) += SaberSpeedLevel * 0.05f;
            }

            // --- БОНУСЫ СВЕТОВОГО МЕЧА ---

            // ЛЕГЕНДАРНЫЙ ЭФФЕКТ ЗРЕНИЯ (Ур 15+): Предвидение
            // Если зрение активно, повышает крит и урон
            if (_sightActive && SkillLevels[2] >= 15)
            {
                Player.GetCritChance(DamageClass.Generic) += 10;
                Player.GetDamage(DamageClass.Generic) += 0.10f;
            }

            // --- ЗАЩИТА СИЛЫ (Пассивный эффект при активации) ---
            if (Player.HasBuff(ModContent.BuffType<ForceProtectionBuff>()) && ProtectionLevel >= 3)
            {
                Player.noKnockback = true;
            }
        }

        // --- ЛОГИКА ЗАЩИТЫ СИЛЫ ---
        
        // 1. Уменьшаем входящий урон (ModifyHurt срабатывает ДО вычета защиты брони)
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // Применяем пробитие брони (Saber Penetration)
            if (SaberPenetrationLevel > 0)
            {
                Player.GetArmorPenetration(DamageClass.Melee) += SaberPenetrationLevel * 3;
            }

            if (Player.HasBuff(ModContent.BuffType<ForceProtectionBuff>()) && ProtectionLevel > 0)
            {
                // Проверяем, есть ли мана (хотя бы немного)
                if (Player.statMana > 10)
                {
                    float reduction = 0f;
                    if (ProtectionLevel == 1) reduction = 0.5f;        // 50%
                    else if (ProtectionLevel == 2) reduction = 0.75f;  // 75%
                    else if (ProtectionLevel >= 3) reduction = 0.875f; // 87.5%

                    modifiers.FinalDamage *= (1f - reduction);
                }
            }
        }

        // 2. Списываем ману за заблокированный урон (OnHurt срабатывает ПОСЛЕ получения урона)
        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.HasBuff(ModContent.BuffType<ForceProtectionBuff>()) && ProtectionLevel > 0)
            {
                // Пытаемся оценить, сколько урона мы заблокировали, основываясь на полученном уроне.
                // Это приблизительный расчет, так как точное значение "съедается" защитой брони.
                int damageTaken = info.Damage;
                int blockedEstimated = 0;
                float costRatio = 1f;
                int maxBlock = 100;

                if (ProtectionLevel == 1) 
                {
                    // Ур 1: Получили 50%, значит заблокировали столько же (1:1)
                    blockedEstimated = damageTaken; 
                    costRatio = 1f; // 100% от блока
                    maxBlock = 100;
                } 
                else if (ProtectionLevel == 2) 
                {
                    // Ур 2: Получили 25%, заблокировали 75% (3:1)
                    blockedEstimated = damageTaken * 3; 
                    costRatio = 0.5f; // 50% от блока
                    maxBlock = 200;
                } 
                else if (ProtectionLevel >= 3) 
                {
                    // Ур 3: Получили 12.5%, заблокировали 87.5% (7:1)
                    blockedEstimated = damageTaken * 7; 
                    costRatio = 0.25f; // 25% от блока
                    maxBlock = 400;
                }

                // Ограничиваем максимальный блок (как в описании способности)
                if (blockedEstimated > maxBlock) blockedEstimated = maxBlock;

                // Рассчитываем стоимость маны
                int manaCost = (int)(blockedEstimated * costRatio);
                if (manaCost < 1) manaCost = 1;

                // Прокачка Защиты: опыт за заблокированный урон
                AddSkillXP(8, manaCost / 5); // 8 - индекс Защиты. Снизили с /2 до /5.

                if (Player.statMana >= manaCost)
                {
                    Player.statMana -= manaCost;
                    
                    // Визуальный эффект щита (зеленая сфера)
                    for (int i = 0; i < 36; i++)
                    {
                        Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(i * 10)) * 45f;
                        Dust.NewDustPerfect(Player.Center + offset, Terraria.ID.DustID.GreenFairy, Vector2.Zero, 100, default, 1.5f).noGravity = true;
                    }
                }
                else
                {
                    // Если маны не хватило, отключаем защиту
                    Player.ClearBuff(ModContent.BuffType<ForceProtectionBuff>());
                    Main.NewText("Защита Силы истощена!", Color.Red);
                }

                // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Возмездие
                // При получении урона выпускает волну Силы
                if (SkillLevels[8] >= 15)
                {
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item14, Player.Center);
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 300f)
                        {
                            int retributionDamage = damageTaken * 2; // 200% от полученного урона
                            npc.StrikeNPC(npc.CalculateHitInfo(retributionDamage, 0));
                            Dust.NewDust(npc.position, npc.width, npc.height, Terraria.ID.DustID.GreenFairy, 0, 0, 100, default, 2f);
                        }
                    }
                }
            }
        }

        // --- МОДИФИКАТОРЫ ОРУЖИЯ ---

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // Применяем бонус урона к оружию ближнего боя (световым мечам)
            if (SaberDamageLevel > 0 && item.DamageType.CountsAsClass(DamageClass.Melee))
            {
                damage += SaberDamageLevel * 0.05f; // +5% за уровень

                // Уровень 2: +15% дополнительного урона
                if (SaberDamageLevel >= 2)
                {
                    damage += 0.15f;
                }
            }
        }

        public override void ModifyItemScale(Item item, ref float scale)
        {
            if (SaberSizeLevel > 0 && item.DamageType.CountsAsClass(DamageClass.Melee))
            {
                scale *= 1f + (SaberSizeLevel * 0.05f); // +5% размера за уровень
            }
        }

        // --- СКРЫТИЕ И БЛОКИРОВКА ПРЕДМЕТА ПРИ БРОСКЕ ---

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            // Если меч брошен, скрываем его в руках
            int projIndex = GetActiveSaberThrowProjectileIndex();
            if (projIndex != -1)
            {
                var proj = Main.projectile[projIndex].ModProjectile as Content.Projectiles.SaberThrowProjectile;
                if (proj != null && drawInfo.heldItem.type == proj.thrownItemType)
                {
                    drawInfo.heldItem = new Item(); // Визуально убираем предмет
                }
            }
        }

        public override bool CanUseItem(Item item)
        {
            // Нельзя использовать меч, пока он летит
            int projIndex = GetActiveSaberThrowProjectileIndex();
            if (projIndex != -1)
            {
                var proj = Main.projectile[projIndex].ModProjectile as Content.Projectiles.SaberThrowProjectile;
                if (proj != null && item.type == proj.thrownItemType)
                {
                    return false;
                }
            }
            return base.CanUseItem(item);
        }

        // В метод PostUpdateUpdateBadDebuffs или PostUpdate:
        public override void PostUpdate()
        {
            UpdateTiers(); // Синхронизируем уровни каждый кадр

            if (_sightActive)
            {
                // Рассчитываем параметры: чем выше уровень, тем чаще (interval меньше) и дороже
                int interval = 45 - (SightLevel * 10); // Ур 1: 35, Ур 2: 25, Ур 3: 15 тиков
                if (interval < 5) interval = 5;
                int cost = 2 + SightLevel; // Ур 1: 3, Ур 2: 4, Ур 3: 5

                // Проверяем, хватает ли маны перед списанием
                if (Player.statMana < cost)
                {
                    _sightActive = false; // Отключаем эффект
                    if (Player.whoAmI == Main.myPlayer) // Сообщение только для игрока
                        Main.NewText("Недостаточно маны для Зрения Силы!", Color.Red);
                }
                else
                {
                    // Списываем ману
                    if (Main.GameUpdateCount % interval == 0)
                    {
                        Player.statMana -= cost;

                        // Визуальный эффект: кольцо пыли вокруг игрока (импульс сканирования)
                        for (int i = 0; i < 36; i++)
                        {
                            Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(i * 10)) * 40f;
                            Dust d = Dust.NewDustPerfect(Player.Center + offset, Terraria.ID.DustID.BlueCrystalShard, Vector2.Zero, 150, default, 0.8f);
                            d.noGravity = true;
                        }
                    }

                    // Применяем баффы только если мана есть
                    if (SightLevel >= 1) Player.AddBuff(Terraria.ID.BuffID.Hunter, 2);
                    if (SightLevel >= 2) Player.AddBuff(Terraria.ID.BuffID.Dangersense, 2);
                    if (SightLevel >= 3) Player.AddBuff(Terraria.ID.BuffID.Spelunker, 2);

                    // Прокачка Зрения: опыт пока активно
                    if (Main.GameUpdateCount % 60 == 0) AddSkillXP(2, 1); // 2 - индекс Зрения. Снизили с 2 до 1 в секунду.
                }
            }

            // Уменьшаем таймер скорости
            if (SpeedEffectTimer > 0) SpeedEffectTimer--;

            if (LightningTimer > 0) LightningTimer--;

            // ЛЕГЕНДАРНЫЙ ЭФФЕКТ ПРЫЖКА (Ур 15+): Ударное приземление
            if (SkillLevels[5] >= 15)
            {
                // Если мы были в воздухе, падали быстро, а теперь на земле
                if (wasAirborne && Player.velocity.Y == 0 && Player.oldVelocity.Y > 5f)
                {
                    // Создаем ударную волну
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item14, Player.Center);
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(10f, 2f);
                        Dust.NewDustPerfect(Player.Bottom, Terraria.ID.DustID.Smoke, speed, 0, default, 2f).noGravity = true;
                    }

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 200f && !npc.dontTakeDamage)
                        {
                            npc.StrikeNPC(npc.CalculateHitInfo(50 + (JumpLevel * 10), 0));
                            npc.AddBuff(Terraria.ID.BuffID.Confused, 120);
                        }
                    }
                }
                wasAirborne = Player.velocity.Y != 0;
            }

            // --- ЛОГИКА ИСЦЕЛЕНИЯ ---
            if (HealActive)
            {
                // --- ВИЗУАЛЬНАЯ АУРА (зависит от уровня) ---
                if (HealLevel == 1)
                {
                    // Уровень 1: Легкие восходящие частицы
                    if (Main.rand.NextBool(5))
                    {
                        Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, Terraria.ID.DustID.GreenFairy, 0f, -1f, 100, default, 0.8f);
                        d.noGravity = true;
                        d.velocity *= 0.5f;
                    }
                }
                else if (HealLevel == 2)
                {
                    // Уровень 2: Вращающееся кольцо
                    float angle = (Main.GameUpdateCount % 120) / 120f * MathHelper.TwoPi;
                    Vector2 offset = new Vector2(30, 0).RotatedBy(angle);
                    
                    Dust d = Dust.NewDustPerfect(Player.Center + offset, Terraria.ID.DustID.GreenFairy, Vector2.Zero, 100, default, 1.2f);
                    d.noGravity = true;
                    
                    Dust d2 = Dust.NewDustPerfect(Player.Center - offset, Terraria.ID.DustID.GreenFairy, Vector2.Zero, 100, default, 1.2f);
                    d2.noGravity = true;
                }
                else if (HealLevel >= 3)
                {
                    // Уровень 3: Интенсивная спираль и энергия
                    float angle = (Main.GameUpdateCount % 60) / 60f * MathHelper.TwoPi;
                    Vector2 offset = new Vector2(40, 0).RotatedBy(angle * 2); // Быстрое вращение
                    
                    // Используем TerraBlade (зеленый) для более мощного эффекта
                    Dust d = Dust.NewDustPerfect(Player.Center + offset, Terraria.ID.DustID.TerraBlade, -offset * 0.05f, 100, default, 1.0f);
                    d.noGravity = true;

                    if (Main.rand.NextBool(3))
                    {
                        Dust d2 = Dust.NewDustDirect(Player.position, Player.width, Player.height, Terraria.ID.DustID.GreenFairy, 0f, -2f, 100, default, 1.5f);
                        d2.noGravity = true;
                    }
                }

                HealTimer++;
                bool stopHealing = false;

                // 1. Проверка условий остановки
                if (Player.statLife >= Player.statLifeMax2) stopHealing = true; // Полное здоровье
                if (Player.statMana <= 0) stopHealing = true;                   // Нет маны
                if (HealTimer >= 1800) stopHealing = true;                      // Прошло 30 секунд (30 * 60 тиков)

                if (stopHealing)
                {
                    StopHealing();
                }
                else
                {
                    // Лечим раз в секунду (60 тиков)
                    if (HealTimer % 60 == 0)
                    {
                        int cost = ModContent.GetInstance<ForceConfig>().HealManaCost;
                        int healAmount = ModContent.GetInstance<ForceConfig>().HealAmount * HealLevel;

                        if (Player.statMana >= cost)
                        {
                            Player.statMana -= cost;
                            Player.statLife += healAmount;
                            Player.HealEffect(healAmount);

                            // Визуальный эффект (зеленые частицы)
                            for (int i = 0; i < 15; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1f, 1f);
                                Dust d = Dust.NewDustPerfect(Player.Center, Terraria.ID.DustID.GreenFairy, speed * 3, 100, default, 1.2f);
                                d.noGravity = true;
                            }
                        }
                        else
                        {
                            // Если маны не хватило на тик - останавливаем
                            StopHealing();
                        }
                    }
                }
            }

            // --- ЛОГИКА ЗАЩИТЫ СИЛЫ (Таймеры) ---
            bool isProtectionActive = Player.HasBuff(ModContent.BuffType<ForceProtectionBuff>());

            if (_wasProtectionActive && !isProtectionActive)
            {
                // Бафф только что закончился (истекло время или кончилась мана)
                Player.AddBuff(ModContent.BuffType<ForceProtectionCooldown>(), 1800); // 30 секунд перезарядки
                Main.NewText("Защита Силы перезаряжается...", Color.Orange);
            }

            _wasProtectionActive = isProtectionActive;

            // --- БЛОКИРОВАНИЕ СНАРЯДОВ СВЕТОВЫМ МЕЧОМ ---
            if (Player.itemAnimation > 0 && !Player.ItemAnimationJustStarted)
            {
                Item heldItem = Player.HeldItem;
                if (IsLightsaber(heldItem))
                {
                    // Используем хитбокс предмета для проверки коллизии
                    // Ручной расчет хитбокса, так как GetItemHitbox может быть недоступен
                    Rectangle meleeHitbox = new Rectangle((int)Player.itemLocation.X, (int)Player.itemLocation.Y, 32, 32);
                    if (!heldItem.noMelee)
                    {
                        meleeHitbox.Width = (int)((float)heldItem.width * heldItem.scale);
                        meleeHitbox.Height = (int)((float)heldItem.height * heldItem.scale);
                    }
                    if (Player.direction == -1)
                    {
                        meleeHitbox.X -= meleeHitbox.Width;
                    }
                    if (Player.gravDir == 1f)
                    {
                        meleeHitbox.Y -= meleeHitbox.Height;
                    }

                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.hostile && proj.damage > 0 && meleeHitbox.Intersects(proj.Hitbox))
                        {
                            // Эффект блокирования
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15, Player.Center); // Звук светового меча
                            
                            // Искры
                            for (int j = 0; j < 10; j++)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                                Dust.NewDustPerfect(proj.Center, DustID.Electric, speed, 0, default, 1.0f).noGravity = true;
                            }

                            // Уничтожаем снаряд
                            proj.Kill();
                        }
                    }
                }
            }

            // --- АНИМАЦИЯ РУКИ ---
            if (ForceAnimationTimer > 0)
            {
                ForceAnimationTimer--;
                if (ForceAnimationStyle == 1) // HoldUp (Рука вверх)
                {
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * Player.gravDir);
                }
                else if (ForceAnimationStyle == 2) // Shoot (Рука к курсору)
                {
                    Vector2 diff = Main.MouseWorld - Player.Center;
                    float rot = diff.ToRotation();
                    if (Player.direction == -1) rot -= MathHelper.Pi;
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
                }
            }

            // --- ЛОГИКА УДУШЕНИЯ (Вынесена из предмета) ---
            if (ChokeInput)
            {
                UpdateChoke();
                ChokeInput = false; // Сбрасываем флаг каждый кадр, чтобы требовалось удержание
            }
            else
            {
                ChokeTargetIndex = -1; // Если кнопку отпустили, сбрасываем цель
            }
        }

        public bool ToggleSight() { _sightActive = !_sightActive; return _sightActive; }

        // Метод для включения/выключения лечения
        public void ToggleHeal()
        {
            if (HealActive)
            {
                StopHealing(); // Если уже активно - выключаем
            }
            else
            {
                // Включаем, только если нет дебаффа
                if (!Player.HasBuff(Terraria.ID.BuffID.PotionSickness))
                {
                    HealActive = true;
                    HealTimer = 0;
                }
            }
        }

        // Вспомогательный метод остановки
        private void StopHealing()
        {
            HealActive = false;
            HealTimer = 0;
            
            // Накладываем дебафф на 1.5 минуты (90 секунд * 60 тиков = 5400)
            Player.AddBuff(Terraria.ID.BuffID.PotionSickness, 5400);
        }

        public override void OnEnterWorld()
        {
            // ForcePoints = 5; // Убираем халявные очки, пусть игрок качается с нуля
        }

        public void AddExperience(int damageDone)
        {
                // БАЛАНС: Опыт зависит от нанесенного урона, но не более 10 за удар (чтобы скорострельность не ломала баланс)
                // Формула: 1 ед опыта + 1 за каждые 10 урона.
                int xpGain = 1 + (damageDone / 10);
                if (xpGain > 10) xpGain = 10;

                ForceExperience += xpGain;

                // Отладочное сообщение в чат (потом удалим)
                // Main.NewText($"Опыт: {ForceExperience} / {ForceExperienceMax}");

                if (ForceExperience >= ForceExperienceMax)
                {
                    ForceExperience -= ForceExperienceMax; // Сохраняем излишки опыта (например, если дали 105 из 100, останется 5)
                    ForcePoints++;
                    // БАЛАНС: Увеличиваем сложность быстрее (было 1.15, стало 1.25)
                    ForceExperienceMax = (int)(ForceExperienceMax * 1.25f); 

                    // Эффект и звук уровня
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4);
                    CombatText.NewText(Player.getRect(), Color.Cyan, "FORCE LEVEL UP!", true);
                    
                    // Восстанавливаем ману при повышении уровня
                    Player.statMana = Player.statManaMax2;
                    Player.ManaEffect(Player.statManaMax2);

                    // Визуальная анимация повышения уровня (Спираль Силы)
                    for (float i = 0; i < MathHelper.TwoPi * 3; i += 0.15f)
                    {
                        // Синяя спираль
                        Vector2 offset = Vector2.UnitX.RotatedBy(i) * 30f;
                        Vector2 pos = Player.Bottom + new Vector2(0, -i * 8); // Поднимаемся вверх
                        
                        Dust d = Dust.NewDustPerfect(pos + offset, Terraria.ID.DustID.BlueCrystalShard, new Vector2(0, -1f), 0, default, 1.5f);
                        d.noGravity = true;
                        d.velocity += offset * 0.05f; // Немного разлетается

                        // Электрическая спираль (противофаза)
                        Vector2 offset2 = Vector2.UnitX.RotatedBy(i + MathHelper.Pi) * 30f;
                        Dust d2 = Dust.NewDustPerfect(pos + offset2, Terraria.ID.DustID.Electric, new Vector2(0, -1f), 0, default, 1.0f);
                        d2.noGravity = true;
                        d2.velocity += offset2 * 0.05f;
                    }

                    // Взрыв в центре
                    for (int k = 0; k < 30; k++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1f, 1f) * 4f;
                        Dust d = Dust.NewDustPerfect(Player.Center, Terraria.ID.DustID.BlueCrystalShard, speed, 0, default, 2.0f);
                        d.noGravity = true;
                    }
                }
        }

        // Метод для начисления опыта конкретной способности
        public void AddSkillXP(int skillIndex, int amount)
        {
            if (SkillLevels[skillIndex] == 0) return; // Нельзя качать неизученную способность
            if (SkillLevels[skillIndex] >= 15) return; // Максимальный уровень (опционально)

            SkillExp[skillIndex] += amount;

            if (SkillExp[skillIndex] >= SkillMaxExp[skillIndex])
            {
                SkillExp[skillIndex] -= SkillMaxExp[skillIndex];
                SkillLevels[skillIndex]++;
                SkillMaxExp[skillIndex] = (int)(SkillMaxExp[skillIndex] * 1.35f); // Усложнение прокачки (было 1.2)

                // Визуальное оповещение
                CombatText.NewText(Player.getRect(), Color.Gold, "Skill Up!", true);
                Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MaxMana);

                // Проверка на повышение Тира (каждые 5 уровней)
                if (SkillLevels[skillIndex] == 5 || SkillLevels[skillIndex] == 10)
                {
                    Main.NewText($"Ваше мастерство Силы возросло! (Уровень {GetTier(SkillLevels[skillIndex])})", Color.Cyan);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Проверяем, что это наш персонаж наносит удар
            if (Player.whoAmI == Main.myPlayer)
            {
                AddExperience(damageDone);

                // --- ЭФФЕКТЫ МАСТЕРСТВА МЕЧА (Дебаффы) ---
                if (SaberDamageLevel >= 3 && hit.DamageType.CountsAsClass(DamageClass.Melee))
                {
                    // Уровень 3: Горение
                    target.AddBuff(Terraria.ID.BuffID.OnFire, 180); // 3 секунды

                    // Уровень 4: Ихор (Снижение защиты)
                    if (SaberDamageLevel >= 4)
                    {
                        target.AddBuff(Terraria.ID.BuffID.Ichor, 180);
                    }

                    // Уровень 5: Проклятый огонь
                    if (SaberDamageLevel >= 5)
                    {
                        target.AddBuff(Terraria.ID.BuffID.CursedInferno, 180);
                    }
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Проверяем, была ли нажата клавиша, которую мы зарегистрировали в ForceKeybinds
            if (ForceKeybinds.OpenForceMenu.JustPressed)
            {
                // Переключаем видимость окна
                ForceUI.Visible = !ForceUI.Visible;

                // Проигрываем звук открытия меню для атмосферности (как в Jedi Academy)
                Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuOpen);
            }

            // Обработка горячих клавиш способностей
            if (ForceKeybinds.ForcePush.JustPressed) UsePush(Main.MouseWorld - Player.Center);
            if (ForceKeybinds.ForceSpeed.JustPressed) UseSpeed();
            if (ForceKeybinds.ForceHealth.JustPressed) UseHeal();
            
            // Молния теперь стреляет непрерывно при удержании
            if (ForceKeybinds.ForceLightning.Current)
            {
                if (LightningTimer <= 0)
                {
                    UseLightning(Main.MouseWorld - Player.Center);
                    LightningTimer = 25; // Задержка между выстрелами (как у предмета)
                }
            }

            if (ForceKeybinds.ForceMindTrick.JustPressed) UseMindTrick();
            if (ForceKeybinds.ForceProtection.JustPressed) UseProtection();
            if (ForceKeybinds.ForceSaberThrow.JustPressed) UseSaberThrow(Main.MouseWorld - Player.Center);
            
            if (ForceKeybinds.ForceSight.JustPressed)
            {
                if (SightLevel > 0)
                {
                    ForceAnimationTimer = 20;
                    ForceAnimationStyle = 1;
                    string status = ToggleSight() ? "активировано" : "деактивировано";
                    Main.NewText($"Зрение Силы {status}", Color.Cyan);
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4);
                }
                else
                {
                    Main.NewText("Вы еще не обучены Зрению Силы!", Color.Red);
                }
            }

            // Удушение требует удержания клавиши
            if (ForceKeybinds.ForceChoke.Current)
            {
                ChokeInput = true;
            }
        }

        // --- МЕТОДЫ СПОСОБНОСТЕЙ (Логика перенесена из предметов) ---

        public void UsePush(Vector2 direction)
        {
            if (PushLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Толчку Силы!", Color.Red);
                return;
            }

            var config = ModContent.GetInstance<ForceConfig>();
            if (Player.statMana < config.PushManaCost) return; // Нет маны

            Player.statMana -= config.PushManaCost;
            Player.manaRegenDelay = 60;

            ForceAnimationTimer = 20;
            ForceAnimationStyle = 1;

            // Множитель от уровня
            float levelMultiplier = 1f + (PushLevel * 0.6f);
            float range = config.PushRange * levelMultiplier / 2;
            float currentKnockback = config.PushKnockback * levelMultiplier;

            // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Силовой Взрыв
            if (SkillLevels[0] >= 15)
            {
                range *= 1.5f; // Увеличенный радиус
                currentKnockback *= 1.5f;
            }

            // Визуальный эффект
            float coneHalfAngle = MathHelper.ToRadians(60);
            Vector2 baseDirection = Vector2.Normalize(direction);

            for (int i = 0; i < 40; i++)
            {
                Vector2 dustVel = baseDirection.RotatedByRandom(coneHalfAngle) * Main.rand.NextFloat(3f, 9f);
                Dust d = Dust.NewDustDirect(Player.MountedCenter, 0, 0, Terraria.ID.DustID.Cloud, dustVel.X, dustVel.Y, 100, default, 1.5f);
                d.noGravity = true;
                d.velocity = dustVel;
            }

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item8, Player.Center);

            // Логика толчка
            Vector2 aimDirection = Vector2.Normalize(direction);
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(Player.Center) < range)
                {
                    Vector2 pushDirection = (npc.Center - Player.Center);
                    if (pushDirection.Length() == 0) continue;
                    pushDirection.Normalize();

                    if (Vector2.Dot(aimDirection, pushDirection) > 0.5f)
                    {
                        npc.velocity = pushDirection * currentKnockback;
                        npc.StrikeNPC(npc.CalculateHitInfo(1, (int)Player.direction, false, currentKnockback));
                        AddExperience(1);
                        
                        AddSkillXP(0, 3); // 0 - индекс Толчка. Снизили с 5 до 3.

                        if (PushLevel >= 3)
                        {
                            var globalNPC = npc.GetGlobalNPC<ForceGlobalNPC>();
                            globalNPC.isForcePushed = true;
                            globalNPC.forcePushDamage = 20;
                            globalNPC.forcePushTimer = 60;
                        }

                        // Легендарный эффект: поджог
                        if (SkillLevels[0] >= 15)
                        {
                            npc.AddBuff(Terraria.ID.BuffID.ShadowFlame, 300);
                        }
                    }
                }
            }
        }

        public void UseSpeed()
        {
            if (SpeedLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Скорости Силы!", Color.Red);
                return;
            }

            var config = ModContent.GetInstance<ForceConfig>();
            // Стоимость маны для скорости (можно вынести в конфиг, пока хардкод 40 как в предмете)
            int manaCost = 40; 

            if (Player.statMana < manaCost) return;

            Player.statMana -= manaCost;
            Player.manaRegenDelay = 60;

            ForceAnimationTimer = 20;
            ForceAnimationStyle = 1;

            float durationMult = 1f + (SpeedLevel * 0.5f);
            int duration = (int)(config.SpeedDuration * durationMult);

            Player.AddBuff(ModContent.BuffType<ForceSpeedBuff>(), duration);
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Player.Center);

            AddSkillXP(1, 5); // 1 - индекс Скорости. Снизили с 10 до 5.
        }

        public void UseHeal()
        {
            if (HealLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Исцелению Силы!", Color.Red);
                return;
            }

            if (Player.HasBuff(Terraria.ID.BuffID.PotionSickness)) return;

            var config = ModContent.GetInstance<ForceConfig>();
            if (Player.statMana < config.HealManaCost) return;

            Player.statMana -= config.HealManaCost;
            Player.manaRegenDelay = 60;

            ForceAnimationTimer = 30;
            ForceAnimationStyle = 1;

            int healAmount = config.HealAmount * HealLevel;
            Player.statLife += healAmount;
            Player.HealEffect(healAmount);

            int cooldown = 1800;
            if (HealLevel >= 3) cooldown = 1200;
            Player.AddBuff(Terraria.ID.BuffID.PotionSickness, cooldown);

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Player.Center);

            AddSkillXP(3, 10); // 3 - индекс Лечения. Снизили с 15 до 10.

            // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Ревитализация
            if (SkillLevels[3] >= 15)
            {
                Player.AddBuff(Terraria.ID.BuffID.RapidHealing, 600); // Быстрая регенерация на 10 сек
            }

            for (int i = 0; i < 30; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(Player.Center, Terraria.ID.DustID.GreenFairy, speed * 4, 100, default, 1.5f);
                d.noGravity = true;
            }
        }

        public void UseLightning(Vector2 direction)
        {
            if (LightningLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Молнии Силы!", Color.Red);
                return;
            }

            var config = ModContent.GetInstance<ForceConfig>();
            if (Player.statMana < config.LightningManaCost) return;

            Player.statMana -= config.LightningManaCost;
            Player.manaRegenDelay = 60;

            ForceAnimationTimer = 25; // Увеличили до 25, чтобы совпадало с темпом стрельбы
            ForceAnimationStyle = 2;

            int damage = config.LightningDamage;
            int maxTargets = 2 + LightningLevel * 2;
            float chainRange = 300f + (LightningLevel * 100f);
            int finalDamage = damage * LightningLevel;

            // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Шторм Силы
            if (SkillLevels[4] >= 15)
            {
                maxTargets = 20; // Цепляет до 20 врагов
                finalDamage *= 2; // Удвоенный урон
            }

            List<int> hitTargets = new List<int>();
            Vector2 currentSourcePos = Player.Center;
            int firstTarget = -1;
            float distToMouse = 200f;

            // Поиск первой цели у курсора
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC npc = Main.npc[k];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float d = Vector2.Distance(Player.Center + direction, npc.Center); // direction здесь это смещение от игрока к курсору
                    // Или используем Main.MouseWorld, если direction это вектор
                    // Для универсальности используем Main.MouseWorld, так как direction передается как (Mouse - Player)
                    float dMouse = Vector2.Distance(Main.MouseWorld, npc.Center);

                    if (dMouse < distToMouse && Collision.CanHitLine(Player.Center, 0, 0, npc.Center, 0, 0))
                    {
                        distToMouse = dMouse;
                        firstTarget = k;
                    }
                }
            }

            // Если у курсора нет, ищем ближайшего
            if (firstTarget == -1)
            {
                float minDist = chainRange;
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC npc = Main.npc[k];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float d = Vector2.Distance(Player.Center, npc.Center);
                        if (d < minDist && Collision.CanHitLine(Player.Center, 0, 0, npc.Center, 0, 0))
                        {
                            minDist = d;
                            firstTarget = k;
                        }
                    }
                }
            }

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item122, Player.Center);

            if (firstTarget != -1)
            {
                int currentTarget = firstTarget;
                for (int i = 0; i < maxTargets; i++)
                {
                    if (currentTarget == -1) break;
                    NPC npc = Main.npc[currentTarget];
                    hitTargets.Add(currentTarget);

                    int hitDmg = (int)(finalDamage * (1f - i * 0.1f));
                    if (hitDmg < 1) hitDmg = 1;

                    npc.StrikeNPC(npc.CalculateHitInfo(hitDmg, 0, false, 0));
                    AddExperience(hitDmg);

                    AddSkillXP(4, 1); // 4 - индекс Молнии. Снизили с 2 до 1 за удар.

                    if (LightningLevel >= 3) npc.AddBuff(Terraria.ID.BuffID.Electrified, 180);

                    // Визуальный эффект молнии
                    DrawLightning(currentSourcePos, npc.Center, LightningLevel);

                    currentSourcePos = npc.Center;
                    int nextTarget = -1;
                    float minNextDist = chainRange;

                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        NPC n = Main.npc[k];
                        if (n.active && !n.friendly && !n.dontTakeDamage && !hitTargets.Contains(k))
                        {
                            float d = Vector2.Distance(currentSourcePos, n.Center);
                            if (d < minNextDist && Collision.CanHitLine(currentSourcePos, 0, 0, n.Center, 0, 0))
                            {
                                minNextDist = d;
                                nextTarget = k;
                            }
                        }
                    }
                    currentTarget = nextTarget;
                }
            }
            else
            {
                // В пустоту
                Vector2 dir = direction;
                dir.Normalize();
                
                Vector2 endPos = Player.Center + dir * chainRange;
                
                // Проверяем столкновение с блоками (Raycast), чтобы молния не проходила сквозь стены
                for (float f = 10f; f < chainRange; f += 10f)
                {
                    Vector2 checkPos = Player.Center + dir * f;
                    if (WorldGen.SolidTile(checkPos.ToTileCoordinates()))
                    {
                        endPos = checkPos;
                        break;
                    }
                }
                
                // Рисуем молнию в точку попадания (с небольшим разбросом для хаотичности)
                endPos += Main.rand.NextVector2Circular(20f, 20f);
                DrawLightning(currentSourcePos, endPos, LightningLevel);
                
                // Искры в месте удара
                Dust.NewDust(endPos, 10, 10, Terraria.ID.DustID.Electric);
            }
        }

        public void UseMindTrick()
        {
            if (MindTrickLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Обману Разума!", Color.Red);
                return;
            }

            var config = ModContent.GetInstance<ForceConfig>();
            if (Player.statMana < config.MindTrickManaCost) return;

            Player.statMana -= config.MindTrickManaCost;
            Player.manaRegenDelay = 60;

            ForceAnimationTimer = 30;
            ForceAnimationStyle = 1;

            float radius = 150f + (MindTrickLevel * 50f);
            int duration = config.MindTrickDuration + (MindTrickLevel * 120);
            Vector2 targetPos = Main.MouseWorld;

            // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Массовая истерия
            if (SkillLevels[7] >= 15)
            {
                radius = 1000f; // Действует почти на весь экран
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    if (npc.Distance(targetPos) <= radius)
                    {
                        if (npc.boss)
                        {
                            npc.AddBuff(Terraria.ID.BuffID.Slow, duration);
                            CombatText.NewText(npc.getRect(), Color.Purple, "Slowed!", true);
                        }
                        else
                        {
                            var globalNPC = npc.GetGlobalNPC<ForceGlobalNPC>();
                            globalNPC.mindTrickTimer = duration;
                            globalNPC.mindTrickLevel = MindTrickLevel;
                            CombatText.NewText(npc.getRect(), Color.Purple, "Mind Trick!", true);
                            
                            AddSkillXP(7, 5); // 7 - индекс Обмана. Снизили с 10 до 5.
                        }
                    }
                }
            }

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item8, targetPos);
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustPos = targetPos + Main.rand.NextVector2Circular(radius, radius);
                Dust.NewDustPerfect(dustPos, Terraria.ID.DustID.Shadowflame, Vector2.Zero, 150, default, 1.5f).noGravity = true;
            }
        }

        public void UseProtection()
        {
            if (ProtectionLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Защите Силы!", Color.Red);
                return;
            }

            if (Player.HasBuff(ModContent.BuffType<ForceProtectionBuff>())) return;
            if (Player.HasBuff(ModContent.BuffType<ForceProtectionCooldown>()))
            {
                Main.NewText("Защита Силы перезаряжается...", Color.Orange);
                return;
            }

            ForceAnimationTimer = 20;
            ForceAnimationStyle = 1;

            Player.AddBuff(ModContent.BuffType<ForceProtectionBuff>(), 3600);
            Main.NewText("Защита Силы активирована!", Color.LightGreen);
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Player.Center);

            AddSkillXP(8, 3); // 8 - индекс Защиты. Снизили с 5 до 3.
        }

        public void UseSaberThrow(Vector2 direction)
        {
            // Нельзя бросить меч, если он уже в полете
            if (GetActiveSaberThrowProjectileIndex() != -1)
            {
                return;
            }

            if (SaberThrowLevel <= 0)
            {
                Main.NewText("Вы еще не обучены Броску Меча!", Color.Red);
                return;
            }

            // Проверяем, держит ли игрок меч
            Item heldItem = Player.HeldItem;
            bool isSword = heldItem != null && !heldItem.IsAir &&
                           (heldItem.DamageType.CountsAsClass(DamageClass.Melee) || heldItem.type == ModContent.ItemType<Content.Items.ForceSaberThrow>()) &&
                           heldItem.useStyle == Terraria.ID.ItemUseStyleID.Swing;

            if (!isSword)
            {
                Main.NewText("Для этой способности нужен меч в руках!", Color.Red);
                return;
            }

            var config = ModContent.GetInstance<ForceConfig>();
            if (Player.statMana < config.SaberThrowManaCost) return;

            Player.statMana -= config.SaberThrowManaCost;
            Player.manaRegenDelay = 60;

            ForceAnimationTimer = 20;
            ForceAnimationStyle = 2; // Рука к курсору

            Vector2 velocity = Vector2.Normalize(direction) * 12f; // Скорость полета
            int weaponDamage = Player.GetWeaponDamage(heldItem); // Урон меча в руках с учетом брони и аксессуаров
            // Урон теперь зависит от оружия: 100% базы + 20% за каждый уровень навыка
            int damage = (int)(weaponDamage * (1f + (SaberThrowLevel * 0.2f)));

            int projIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, velocity, ModContent.ProjectileType<Content.Projectiles.SaberThrowProjectile>(), damage, 3f, Player.whoAmI);
            
            // Передаем тип предмета в снаряд, чтобы он выглядел как этот меч
            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                var proj = Main.projectile[projIndex].ModProjectile as Content.Projectiles.SaberThrowProjectile;
                if (proj != null)
                {
                    proj.thrownItemType = heldItem.type;
                    // Синхронизация для мультиплеера
                    if (Main.netMode != Terraria.ID.NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile, -1, -1, null, projIndex);
                    }
                }
            }

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item71, Player.Center);
            AddSkillXP(9, 5); // 9 - индекс Броска.
        }

        private void UpdateChoke()
        {
            if (ChokeLevel <= 0)
            {
                if (Main.GameUpdateCount % 60 == 0) Main.NewText("Вы еще не обучены Удушению Силы!", Color.Red);
                return;
            }

            ForceAnimationTimer = 2; // Поддерживаем анимацию пока держим
            ForceAnimationStyle = 1;

            var config = ModContent.GetInstance<ForceConfig>();
            float range = 300f + (ChokeLevel * 150f);
            int damage = config.ChokeDamage * ChokeLevel;
            int manaCost = config.ChokeManaCost;

            if (ChokeTargetIndex == -1 || !Main.npc[ChokeTargetIndex].active || Main.npc[ChokeTargetIndex].Distance(Player.Center) > range)
            {
                ChokeTargetIndex = -1;
                int bestTarget = -1;
                float bestDist = 200f;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float d = Vector2.Distance(Main.MouseWorld, npc.Center);
                        if (d < bestDist && Collision.CanHitLine(Player.Center, 0, 0, npc.Center, 0, 0))
                        {
                            bestDist = d;
                            bestTarget = i;
                        }
                    }
                }
                ChokeTargetIndex = bestTarget;
            }

            if (ChokeTargetIndex != -1)
            {
                NPC target = Main.npc[ChokeTargetIndex];
                if (Player.statMana >= manaCost)
                {
                    Player.statMana -= manaCost;
                    Player.manaRegenDelay = 60;

                    target.GetGlobalNPC<ForceGlobalNPC>().forceChokeTimer = 5;

                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        target.StrikeNPC(target.CalculateHitInfo(damage, 0, false, 0));
                        AddExperience(damage);
                        AddSkillXP(6, 1); // 6 - индекс Удушения. Опыт за тик урона.

                        if (Main.GameUpdateCount % 30 == 0) Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item15, target.Center);
                        if (ChokeLevel >= 3) target.AddBuff(Terraria.ID.BuffID.Suffocation, 120);
                    }

                    // ЛЕГЕНДАРНЫЙ ЭФФЕКТ (Ур 15+): Казнь
                    if (SkillLevels[6] >= 15 && !target.boss)
                    {
                        if (target.life < target.lifeMax * 0.2f) // Если меньше 20% HP
                        {
                            target.StrikeNPC(target.CalculateHitInfo(target.life + 999, 0, true)); // Мгновенная смерть (крит)
                            CombatText.NewText(target.getRect(), Color.Red, "EXECUTED!", true);
                        }
                    }

                    if (Main.rand.NextBool(2))
                    {
                        Vector2 dir = target.Center - Player.Center;
                        Vector2 dustPos = Player.Center + dir * Main.rand.NextFloat(0.1f, 0.9f);
                        Dust.NewDustPerfect(dustPos, Terraria.ID.DustID.RedTorch, Vector2.Zero, 150, default, 0.5f).noGravity = true;
                    }
                    Dust chokeDust = Dust.NewDustDirect(target.Top + new Vector2(-10, 0), 20, 20, Terraria.ID.DustID.RedTorch, 0, 0, 100, default, 1.2f);
                    chokeDust.noGravity = true;
                    chokeDust.velocity *= 0.5f;

                    for (int i = 0; i < 3; i++)
                    {
                        Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f), Terraria.ID.DustID.RedTorch, Vector2.Zero, 150, default, 2.0f);
                        d.noGravity = true;
                        d.velocity *= 0.1f;
                    }
                }
                else
                {
                    ChokeTargetIndex = -1;
                }
            }
        }

        private void DrawLightning(Vector2 start, Vector2 end, int level)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            direction.Normalize();

            // 1. Генерируем "узловые" точки для зигзага
            // Чем больше расстояние, тем больше изгибов (каждые 20 пикселей)
            int numPoints = (int)(distance / 20f);
            if (numPoints < 1) numPoints = 1;
            
            Vector2[] points = new Vector2[numPoints + 2];
            points[0] = start;
            points[numPoints + 1] = end;

            // Заполняем промежуточные точки со случайным смещением
            for (int i = 1; i <= numPoints; i++)
            {
                float progress = (float)i / (numPoints + 1);
                Vector2 basePos = Vector2.Lerp(start, end, progress);
                
                // Смещение перпендикулярно линии удара (зигзаг)
                Vector2 perp = new Vector2(-direction.Y, direction.X);
                // Амплитуда зависит от уровня: чем выше, тем сильнее разброс (17, 29, 41 пикселей)
                float amplitude = 5f + (level * 12f); 
                float offset = Main.rand.NextFloat(-amplitude, amplitude);
                points[i] = basePos + perp * offset;
            }

            // 2. Рисуем линии между узлами
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 segmentStart = points[i];
                Vector2 segmentEnd = points[i + 1];
                Vector2 segDir = segmentEnd - segmentStart;
                float segLen = segDir.Length();
                segDir.Normalize();

                // Плотное заполнение сегмента частицами
                for (float k = 0; k < segLen; k += 2f)
                {
                    Vector2 dustPos = segmentStart + segDir * k;
                    
                    // Основная молния (яркая, мелкая)
                    Dust d = Dust.NewDustPerfect(dustPos, Terraria.ID.DustID.Electric, Vector2.Zero, 50, default, 0.5f);
                    d.noGravity = true;
                    d.velocity = Vector2.Zero;

                    // Ореол (покрупнее, реже, синий)
                    if (Main.rand.NextBool(5))
                    {
                        Dust d2 = Dust.NewDustPerfect(dustPos, Terraria.ID.DustID.BlueCrystalShard, Vector2.Zero, 100, default, 1.0f);
                        d2.noGravity = true;
                        d2.velocity = Main.rand.NextVector2Circular(1f, 1f); // Легкое движение
                    }
                }

                // 3. Визуальные ответвления (ветви молнии, уходящие в никуда)
                // Шанс зависит от уровня: Ур 1 (1/5), Ур 2 (1/4), Ур 3 (1/3)
                int chance = 6 - level;
                if (chance < 2) chance = 2;

                if (Main.rand.NextBool(chance)) 
                {
                    Vector2 branchStart = segmentStart;
                    // Длина ветки тоже зависит от уровня (становится длиннее)
                    float minLen = 10f + (level * 10f);
                    float maxLen = 30f + (level * 25f);

                    // Случайное направление ветки
                    Vector2 branchDir = segDir.RotatedBy(Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(minLen, maxLen);
                    
                    float branchLen = branchDir.Length();
                    Vector2 branchNorm = Vector2.Normalize(branchDir);
                    
                    // Рисуем короткую ветку
                    for (float b = 0; b < branchLen; b += 4f)
                    {
                        Dust d = Dust.NewDustPerfect(branchStart + branchNorm * b, Terraria.ID.DustID.Electric, Vector2.Zero, 150, default, 0.4f);
                        d.noGravity = true;
                    }
                }
            }
        }

        private bool IsLightsaber(Item item)
        {
            if (item == null || item.IsAir) return false;
            
            // Проверяем ванильные световые мечи (Phaseblades и Phasesabers)
            if (item.type == ItemID.BluePhaseblade || item.type == ItemID.RedPhaseblade || item.type == ItemID.GreenPhaseblade || 
                item.type == ItemID.PurplePhaseblade || item.type == ItemID.WhitePhaseblade || item.type == ItemID.YellowPhaseblade || 
                item.type == ItemID.OrangePhaseblade) return true;
            
            if (item.type == ItemID.BluePhasesaber || item.type == ItemID.RedPhasesaber || item.type == ItemID.GreenPhasesaber || 
                item.type == ItemID.PurplePhasesaber || item.type == ItemID.WhitePhasesaber || item.type == ItemID.YellowPhasesaber || 
                item.type == ItemID.OrangePhasesaber) return true;
            
            return false;
        }

        private int GetActiveSaberThrowProjectileIndex()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<Content.Projectiles.SaberThrowProjectile>())
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
