using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake_soutez
{
    public class Portal
    {
        private static Random random = new Random();

        public Vector2 Position { get; set; }
        public Portal LinkedPortal { get; set; }

        private Texture2D coreTexture;     // střed portálu
        private Texture2D glowTexture;     // glow kolem portálu
        private GraphicsDevice graphicsDevice;
        private Color color;

        public Portal(GraphicsDevice graphicsDevice, Color color)
        {
            this.graphicsDevice = graphicsDevice;
            this.color = color;

            coreTexture = CreateCore(24);
            glowTexture = CreateGlow(60);
        }

        public void Spawn(int gridSize, int screenWidth, int screenHeight)
        {
            int maxX = screenWidth / gridSize;
            int maxY = screenHeight / gridSize;

            Position = new Vector2(
                random.Next(maxX) * gridSize,
                random.Next(maxY) * gridSize
            );
        }

        public void Apply(Game1 game)
        {
            if (LinkedPortal == null) return;

            game.TeleportSnake(LinkedPortal.Position);
        }

        public void Draw(SpriteBatch spriteBatch, int gridSize)
        {
            // Glow (větší než samotný portál)
            spriteBatch.Draw(glowTexture,
                new Rectangle(
                    (int)Position.X - gridSize,
                    (int)Position.Y - gridSize,
                    gridSize * 3,
                    gridSize * 3),
                color * 0.45f);

            // Střed portálu
            spriteBatch.Draw(coreTexture,
                new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    gridSize,
                    gridSize),
                Color.White);
        }

        // === Vytvoření neonového jádra ===
        private Texture2D CreateCore(int size)
        {
            Texture2D tex = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Math.Max(0, 1f - dist / radius);
                    alpha = (float)Math.Pow(alpha, 1.4f);

                    data[y * size + x] = Color.White * alpha;
                }
            }

            tex.SetData(data);
            return tex;
        }

        // === Glow kolem portálu ===
        private Texture2D CreateGlow(int size)
        {
            Texture2D tex = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Math.Max(0, 1f - dist / radius);
                    alpha = (float)Math.Pow(alpha, 2.2f);

                    data[y * size + x] = color * alpha;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
