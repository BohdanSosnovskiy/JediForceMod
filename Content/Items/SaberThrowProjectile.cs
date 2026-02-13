using Microsoft.Xna.Framework;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Audio;

namespace JediForceMod.Content.Projectiles
{
    public class SaberThrowProjectile : ModProjectile
    {
        // Используем текстуру красного лазерного меча (как и у предмета)
        public override string Texture => "Terraria/Images/Item_" + ItemID.RedPhasesaber;

        public int thrownItemType = 0; // Тип предмета, который мы "бросили"

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1; // Кастомный AI
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1; // Бесконечное пробитие
            Projectile.tileCollide = true; // Теперь сталкивается со стенами
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
        }

        // Синхронизация типа предмета в мультиплеере
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(thrownItemType);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            thrownItemType = reader.ReadInt32();
        }

        // Отрисовка: рисуем текстуру предмета вместо текстуры снаряда
        public override bool PreDraw(ref Color lightColor)
        {
            if (thrownItemType > 0)
            {
                Main.instance.LoadItem(thrownItemType); // Убедимся, что текстура загружена
                Texture2D texture = TextureAssets.Item[thrownItemType].Value;
                Rectangle rect = Main.itemAnimations[thrownItemType] != null ? Main.itemAnimations[thrownItemType].GetFrame(texture) : texture.Frame();
                
                Vector2 origin = rect.Size() / 2f; // Центр вращения
                
                // Рисуем вращающийся меч
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, rect, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                return false; // Отключаем стандартную отрисовку
            }
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Вращение спрайта
            Projectile.rotation += 0.4f * Projectile.direction;

            // --- ЗВУК ГУДЕНИЯ ---
            // Используем soundDelay для цикличного воспроизведения звука
            if (Projectile.soundDelay == 0)
            {
                // Item15 - звук взмаха светового меча. Делаем его тише и чуть ниже по тону для эффекта гудения.
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.5f, Pitch = -0.2f, MaxInstances = 3 }, Projectile.position);
                Projectile.soundDelay = 15; // Каждые 15 тиков (4 раза в секунду)
            }

            // Визуальные эффекты (след)
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0, 0, 100, default, 0.8f);
                d.noGravity = true;
            }

            // Логика бумеранга
            // AI[0]: 0 = летит вперед, 1 = возвращается
            
            if (Projectile.ai[0] == 0)
            {
                Projectile.tileCollide = true; // Сталкиваемся со стенами пока летим вперед
                // Летим вперед некоторое время
                Projectile.ai[1]++;
                if (Projectile.ai[1] > 30) // Через 0.5 сек начинаем замедляться
                {
                    Projectile.velocity *= 0.95f;
                    if (Projectile.velocity.Length() < 2f)
                    {
                        Projectile.ai[0] = 1; // Переключаемся на возврат
                        Projectile.ai[1] = 0;
                    }
                }
            }
            else
            {
                Projectile.tileCollide = false; // При возвращении проходит сквозь стены
                // Возвращаемся к игроку
                Vector2 direction = player.Center - Projectile.Center;
                float dist = direction.Length();
                direction.Normalize();
                
                float speed = 15f;
                // Плавный поворот к игроку (инерция)
                Projectile.velocity = (Projectile.velocity * 20f + direction * speed) / 21f;

                if (dist < 20f)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // При ударе об стену начинаем возвращаться
            Projectile.ai[0] = 1;
            Projectile.ai[1] = 0;
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
            
            return false; // Не уничтожаем снаряд
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Начисляем опыт навыка при попадании
            if (Main.myPlayer == Projectile.owner)
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<ForcePlayer>();
                modPlayer.AddSkillXP(9, 2); // 9 - индекс Броска Меча
                if (target.life <= 0) modPlayer.AddExperience(30); // Бонус за убийство броском
            }
        }
    }
}