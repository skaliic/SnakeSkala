using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake_soutez
{
    // Třída pro portály - také implementuje ICollectible
    public class Portal : ICollectible
    {
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; }
        public float Duration => 0; // Portály nemají časové omezení
        public Portal LinkedPortal { get; set; } // Odkaz na druhý portál

        private Texture2D texture;
        private Color color;
        private Random random = new Random();

        public Portal(GraphicsDevice graphicsDevice, Color portalColor)
        {
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            color = portalColor;
            IsActive = true;
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
            // Teleportuje hada na druhý portál
            if (LinkedPortal != null && LinkedPortal.IsActive)
            {
                game.TeleportSnake(LinkedPortal.Position);
            }
        }

        public void Draw(SpriteBatch spriteBatch, int gridSize)
        {
            if (IsActive)
            {
                // Rotující efekt pro portály
                float rotation = (float)(DateTime.Now.Millisecond * 0.003f);
                float scale = (float)Math.Sin(rotation) * 0.2f + 1.0f;

                Rectangle rect = new Rectangle(
                    (int)(Position.X + gridSize / 2),
                    (int)(Position.Y + gridSize / 2),
                    (int)(gridSize * scale),
                    (int)(gridSize * scale));

                spriteBatch.Draw(texture, rect, null, color * 0.7f,
                    rotation, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0);
            }
        }
    }
}