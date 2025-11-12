using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake_soutez
{
    public enum PowerUpType
    {
        SpeedBoost,    // Zrychlí hada
        SlowDown,      // Zpomalí hada
        ScoreDouble,   // Dvojnásobné body
        Invincibility  // Neprůstřelnost
    }

    // Třída pro power-upy - implementuje rozhraní ICollectible
    public class PowerUp : ICollectible
    {
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; }
        public float Duration { get; private set; }
        public PowerUpType Type { get; private set; }

        private Texture2D texture;
        private Color color;
        private Random random = new Random();

        public PowerUp(PowerUpType type, GraphicsDevice graphicsDevice)
        {
            Type = type;
            IsActive = true;

            // Vytvoření textury
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });

            // Nastavení vlastností podle typu
            switch (type)
            {
                case PowerUpType.SpeedBoost:
                    color = Color.Yellow;
                    Duration = 5f;
                    break;
                case PowerUpType.SlowDown:
                    color = Color.Blue;
                    Duration = 5f;
                    break;
                case PowerUpType.ScoreDouble:
                    color = Color.Gold;
                    Duration = 10f;
                    break;
                case PowerUpType.Invincibility:
                    color = Color.Purple;
                    Duration = 7f;
                    break;
            }
        }

        public void Spawn(int gridSize, int screenWidth, int screenHeight)
        {
            int maxX = screenWidth / gridSize;
            int maxY = screenHeight / gridSize;
            Position = new Vector2(random.Next(maxX) * gridSize, random.Next(maxY) * gridSize);
            IsActive = true;
        }

        public void Apply(Game1 game)
        {
            game.ActivatePowerUp(this);
        }

        public void Draw(SpriteBatch spriteBatch, int gridSize)
        {
            if (IsActive)
            {
                // Pulsující efekt
                float pulse = (float)Math.Sin(DateTime.Now.Millisecond * 0.01f) * 0.3f + 0.7f;
                spriteBatch.Draw(texture,
                    new Rectangle((int)Position.X, (int)Position.Y, gridSize, gridSize),
                    color * pulse);
            }
        }
    }
}