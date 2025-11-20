using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake_soutez
{
    public class PowerUp : ICollectible
    {
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; } = true;
        public float Duration { get; private set; }
        public PowerUpType Type { get; private set; }

        private Texture2D texture;
        private Color color;
        private Random random = new Random();

        public PowerUp(PowerUpType type, GraphicsDevice graphicsDevice)
        {
            Type = type;

            // 1x1 pixel textura
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });

            // Nastavení barvy + doby trvání
            switch (type)
            {
                case PowerUpType.SpeedBoost:
                    color = new Color(0, 255, 255);   // CYAN
                    Duration = 5f;
                    break;

                case PowerUpType.SlowDown:
                    color = new Color(0, 120, 255);   // MODRÁ
                    Duration = 5f;
                    break;

                case PowerUpType.ScoreDouble:
                    color = new Color(255, 255, 0);   // ŽLUTÁ
                    Duration = 10f;
                    break;

                case PowerUpType.Invincibility:
                    color = new Color(255, 0, 255);   // MAGENTA
                    Duration = 6f;
                    break;
            }
        }

        public void Spawn(int gridSize, int screenWidth, int screenHeight)
        {
            int maxX = screenWidth / gridSize;
            int maxY = screenHeight / gridSize;

            Position = new Vector2(
                random.Next(maxX) * gridSize,
                random.Next(maxY) * gridSize
            );

            IsActive = true;
        }

        public void Apply(Game1 game)
        {
            game.ActivatePowerUp(this);
            IsActive = false;
        }

        public void Draw(SpriteBatch spriteBatch, int gridSize)
        {
            if (!IsActive) return;

            // Cyberpunk pulsování
            float t = (float)(DateTime.Now.Millisecond * 0.005f);
            float scale = (float)Math.Sin(t) * 0.25f + 1.1f;

            Rectangle rect = new Rectangle(
                (int)(Position.X + gridSize / 2),
                (int)(Position.Y + gridSize / 2),
                (int)(gridSize * scale),
                (int)(gridSize * scale)
            );

            spriteBatch.Draw(
                texture,
                rect,
                null,
                color * 0.8f,
                t * 0.5f,
                new Vector2(0.5f, 0.5f),
                SpriteEffects.None,
                0f
            );
        }
    }
}
