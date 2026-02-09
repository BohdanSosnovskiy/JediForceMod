using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JediForceMod;

namespace JediForceMod.Content.NPCs
{
    public class ForceGlobalNPC : GlobalNPC
    {
        // Важно: InstancePerEntity = true означает, что переменные ниже будут своими для каждого моба
        public override bool InstancePerEntity => true;

        public bool isForcePushed = false;
        public int forcePushDamage = 0;
        public int forcePushTimer = 0;
        public int stunTimer = 0;
        public int forceChokeTimer = 0;
        public int mindTrickTimer = 0;
        public int mindTrickLevel = 0;
        public int mindTrickAttackCooldown = 0;

        public override bool PreAI(NPC npc)
        {
            if (mindTrickAttackCooldown > 0) mindTrickAttackCooldown--;

            // --- ЛОГИКА ОБМАНА РАЗУМА ---
            if (mindTrickTimer > 0)
            {
                mindTrickTimer--;

                // Включаем замешательство для всех уровней, чтобы был визуальный индикатор (знак вопроса)
                npc.confused = true;

                // Уровень 2+: Враги становятся дружелюбными (атакуют других врагов)
                if (mindTrickLevel >= 2)
                {
                    npc.friendly = true;
                    npc.dontTakeDamageFromHostiles = false; // Позволяем другим врагам атаковать эту цель
                }
                else
                {
                    // Уровень 1: Враги игнорируют игрока и путаются
                    npc.target = -1; // Сбрасываем цель
                }

                // Визуальный эффект (фиолетовая дымка)
                if (Main.rand.NextBool(15))
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame, 0, 0, 150, default, 0.5f);
                }

                if (mindTrickTimer == 0)
                {
                    npc.friendly = false; // Возвращаем враждебность
                }
            }

            // --- ЛОГИКА УДУШЕНИЯ ---
            if (forceChokeTimer > 0)
            {
                forceChokeTimer--;
                
                // Физика удушения: удерживаем врага на определенной высоте над землей
                npc.velocity.X *= 0.9f; // Сильное замедление по горизонтали
                
                // 1. Ищем землю под NPC
                float hoverHeight = 80f; // Высота зависания (5 блоков = 80 пикселей)
                bool foundGround = false;
                float groundY = npc.position.Y;

                int tileX = (int)(npc.Center.X / 16f);
                int startY = (int)(npc.Bottom.Y / 16f);
                
                // Ограничиваем поиск земли (например, 50 блоков вниз)
                for (int y = startY; y < startY + 50; y++)
                {
                    if (WorldGen.SolidTile(tileX, y))
                    {
                        groundY = y * 16f;
                        foundGround = true;
                        break;
                    }
                }

                if (foundGround)
                {
                    // Целевая позиция Y (земля - высота NPC - высота зависания)
                    float targetY = groundY - npc.height - hoverHeight;
                    
                    // Разница между текущей и целевой позицией
                    float diff = targetY - npc.position.Y;
                    
                    // Плавно меняем скорость для достижения цели
                    float targetVelocity = MathHelper.Clamp(diff * 0.1f, -12f, 12f);
                    npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, targetVelocity, 0.1f);
                }
                else
                {
                    // Если земли нет (пропасть), просто гасим вертикальную скорость
                    npc.velocity.Y *= 0.9f;
                }
                
                npc.noGravity = true; // Отключаем гравитацию

                // Если таймер истек (последний тик), сбрасываем состояние
                if (forceChokeTimer == 0)
                {
                    npc.noGravity = false; // Возвращаем гравитацию, чтобы враг упал/приземлился
                    npc.velocity *= 0.1f;  // Гасим инерцию, чтобы враг перестал "плыть" по воздуху
                }

                // Возвращаем false, чтобы отключить стандартный ИИ (враг не может атаковать)
                return false; 
            }

            // --- ЛОГИКА ОГЛУШЕНИЯ (от толчка) ---
            if (stunTimer > 0)
            {
                stunTimer--;
                npc.velocity.X = 0; // Полностью останавливаем горизонтальное движение

                // Визуальный эффект: желтые "звездочки" над головой
                if (Main.rand.NextBool(4)) // Не каждый тик, чтобы не перегружать
                {
                    // Позиция чуть выше спрайта (над головой)
                    Vector2 dustPos = npc.Top + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-15, -5));
                    
                    // DustID.GoldCoin создает эффект золотых искорок
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, new Vector2(0, -0.5f), 150, default, 0.8f);
                    d.noGravity = true;
                }

                return false; // Возвращаем false, чтобы ОТКЛЮЧИТЬ стандартный ИИ (враг замирает)
            }
            return true;
        }

        public override void PostAI(NPC npc)
        {
            if (isForcePushed)
            {
                // Проверяем столкновение: collideX (стены) или collideY (пол/потолок)
                // Обычно при сильном толчке нас интересует collideX, но collideY тоже подойдет для удара об пол/потолок
                if (npc.collideX || npc.collideY)
                {
                    // Рассчитываем дополнительный урон от скорости (oldVelocity - скорость до столкновения)
                    // Чем быстрее летел враг, тем сильнее удар. Множитель 4f можно настроить под баланс.
                    int speedDamage = (int)(npc.oldVelocity.Length() * 4f);

                    // Наносим урон от удара об поверхность
                    npc.StrikeNPC(npc.CalculateHitInfo(forcePushDamage + speedDamage, 0, false, 0));

                    // Начисляем опыт игроку (если это клиент)
                    if (Main.netMode != NetmodeID.Server)
                    {
                        Main.LocalPlayer.GetModPlayer<ForcePlayer>().AddExperience(forcePushDamage + speedDamage);
                        Main.LocalPlayer.GetModPlayer<ForcePlayer>().AddSkillXP(0, 5); // 0 - Толчок. Снизили бонус с 10 до 5.
                    }

                    // Визуальные эффекты удара (звук и пыль)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, npc.position);
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Smoke, 0, 0, 100, default, 1.5f);
                    }

                    // Проверяем, является ли враг боссом
                    if (npc.boss)
                    {
                        // Боссы получают иммунитет к оглушению (чтобы не ломать их ИИ)
                        // Если хотите просто уменьшить время, поставьте здесь, например, 30 (0.5 сек)
                        stunTimer = 0;
                    }
                    else
                    {
                        // Оглушаем обычных врагов на случайное время от 1 до 2 секунд (60 - 120 тиков)
                        stunTimer = Main.rand.Next(60, 121);
                    }

                    // Сбрасываем флаг, чтобы урон прошел только один раз
                    isForcePushed = false;
                    forcePushTimer = 0;
                }
                else
                {
                    // Уменьшаем таймер. Если моб летит слишком долго и не врезается, эффект спадает.
                    forcePushTimer--;
                    if (forcePushTimer <= 0)
                    {
                        isForcePushed = false;
                    }
                }
            }

            // --- ЛОГИКА ДВИЖЕНИЯ ДЛЯ ОБМАНА РАЗУМА (Уровень 2+) ---
            // Переопределяем движение в PostAI, чтобы враг шел к другим мобам, а не к игроку
            if (mindTrickTimer > 0 && mindTrickLevel >= 2)
            {
                NPC targetEnemy = null;
                float minDist = 1000f;

                // Ищем ближайшего врага
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && !other.friendly && other.whoAmI != npc.whoAmI && !other.dontTakeDamage && other.life > 0)
                    {
                        float d = Vector2.Distance(npc.Center, other.Center);
                        if (d < minDist)
                        {
                            minDist = d;
                            targetEnemy = other;
                        }
                    }
                }

                if (targetEnemy != null)
                {
                    // Направляем врага к цели
                    Vector2 dir = targetEnemy.Center - npc.Center;
                    dir.Normalize();

                    if (npc.noGravity)
                    {
                        // Для летающих
                        npc.velocity = Vector2.Lerp(npc.velocity, dir * 5f, 0.1f);
                    }
                    else
                    {
                        // Для наземных
                        npc.velocity.X = (dir.X > 0) ? 3f : -3f;
                        npc.direction = (dir.X > 0) ? 1 : -1;
                        npc.spriteDirection = npc.direction;

                        // Простой прыжок через препятствия
                        if (npc.velocity.Y == 0 && Collision.SolidCollision(npc.position + new Vector2(npc.velocity.X, 0), npc.width, npc.height))
                        {
                            npc.velocity.Y = -7f;
                        }
                    }

                    // АТАКА: Если враг под контролем касается цели, наносим урон вручную
                    if (mindTrickAttackCooldown <= 0 && npc.getRect().Intersects(targetEnemy.getRect()))
                    {
                        int damage = npc.damage > 0 ? npc.damage : 10; // Используем базовый урон моба
                        targetEnemy.StrikeNPC(targetEnemy.CalculateHitInfo(damage, 0));
                        
                        // Начисляем опыт игроку за успешную атаку марионетки
                        if (Main.netMode != NetmodeID.Server)
                        {
                            Main.LocalPlayer.GetModPlayer<ForcePlayer>().AddExperience(damage);
                            Main.LocalPlayer.GetModPlayer<ForcePlayer>().AddSkillXP(7, 5); // 7 - Обман. Бонус за атаку марионетки.
                        }

                        mindTrickAttackCooldown = 30; // Кулдаун 0.5 сек (чтобы не убивали мгновенно)
                        
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit1, targetEnemy.position);
                    }
                }
            }
        }
    }
}
