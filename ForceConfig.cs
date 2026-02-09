﻿using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace JediForceMod
{
    public class ForceConfig : ModConfig
    {
        // Конфигурация будет сохраняться на клиенте (в файлах игрока)
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Force_Push_Settings")] // Заголовок в меню

        [DefaultValue(250f)] // Значение по умолчанию
        [Range(1f, 1000f)]  // Диапазон ползунка
        [Label("Дальность толчка (Range)")]
        [Tooltip("Радиус действия Силового Толчка в пикселях")]
        public float PushRange;

        [DefaultValue(20f)]
        [Range(0f, 100f)]
        [Label("Сила импульса (Knockback)")]
        public float PushKnockback;

        [DefaultValue(25)] // Было 15
        [Range(0, 100)]
        [Label("Затраты маны")]
        public int PushManaCost;

        [Header("ForceSpeedSettings")]
        [DefaultValue(1.5f)]
        [Range(1f, 3f)]
        [Label("Множитель скорости")]
        public float SpeedMultiplier;

        [DefaultValue(300)]
        [Range(60, 1800)]
        [Label("Длительность (в тиках)")]
        public int SpeedDuration;

        [Header("ForceHealSettings")]
        [DefaultValue(80)] // Было 50. Лечение должно быть дорогим.
        [Range(10, 200)]
        [Label("Затраты маны на лечение")]
        public int HealManaCost;

        [DefaultValue(15)] // Было 20. Снижаем базу, чтобы на 3 уровне не лечило слишком много.
        [Range(5, 100)]
        [Label("Базовое лечение (HP)")]
        public int HealAmount;

        [Header("ForceLightningSettings")]
        [DefaultValue(40)] // Было 30
        [Range(10, 100)]
        [Label("Затраты маны на молнию")]
        public int LightningManaCost;

        [DefaultValue(30)] // Было 40. Снижаем урон, так как она бьет по площади.
        [Range(10, 200)]
        [Label("Базовый урон молнии")]
        public int LightningDamage;

        [Header("ForceJumpSettings")]
        [DefaultValue(1.5f)]
        [Range(0.5f, 5f)]
        [Label("Сила прыжка за уровень")]
        public float JumpStrength;

        [Header("ForceChokeSettings")]
        [DefaultValue(3)] // Было 1. Удушение очень сильное, должно тратить ману быстрее.
        [Range(1, 20)]
        [Label("Затраты маны на удушение (за тик)")]
        public int ChokeManaCost;

        [DefaultValue(4)] // Было 5
        [Range(1, 50)]
        [Label("Урон удушения (базовый)")]
        public int ChokeDamage;

        [Header("ForceMindTrickSettings")]
        [DefaultValue(40)]
        [Range(10, 100)]
        [Label("Затраты маны на Обман Разума")]
        public int MindTrickManaCost;

        [DefaultValue(300)]
        [Range(60, 1200)]
        [Label("Длительность (в тиках)")]
        public int MindTrickDuration;

        [Header("ForceProtectionSettings")]
        [DefaultValue(100)]
        [Range(50, 500)]
        [Label("Базовый лимит блока (Ур 1)")]
        public int ProtectionBaseCap;

        [Header("ForceSaberThrowSettings")]
        [DefaultValue(30)]
        [Range(10, 100)]
        [Label("Затраты маны на Бросок Меча")]
        public int SaberThrowManaCost;

        [DefaultValue(50)]
        [Range(10, 200)]
        [Label("Базовый урон Броска Меча")]
        public int SaberThrowDamage;
    }
}
