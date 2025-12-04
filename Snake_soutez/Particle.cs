using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake_soutez
{
    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Color Color { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; set; }
        public float Size { get; set; }

        public Particle(Vector2 position, Color color, Random random)
        {
            Position = position;
            Color = color;

            // Náhodný směr a rychlost
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float speed = (float)(random.NextDouble() * 100 + 50);
            Velocity = new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
            );

            MaxLife = (float)(random.NextDouble() * 0.5 + 0.5); // 0.5-1 sekunda
            Life = MaxLife;
            Size = (float)(random.NextDouble() * 3 + 2); // 2-5 pixelů
        }

        public void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
            Life -= deltaTime;

            // Zpomalení částice
            Velocity *= 0.95f;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            float alpha = Life / MaxLife;
            float currentSize = Size * alpha;

            spriteBatch.Draw(
                texture,
                new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    (int)currentSize,
                    (int)currentSize
                ),
                Color * alpha
            );
        }
    }
}