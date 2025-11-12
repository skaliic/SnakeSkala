using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snake_soutez
{
    // Rozhraní pro všechny sbíratelné objekty ve hře
    public interface ICollectible
    {
        Vector2 Position { get; set; }
        bool IsActive { get; set; }
        float Duration { get; }

        void Spawn(int gridSize, int screenWidth, int screenHeight);
        void Apply(Game1 game);
        void Draw(SpriteBatch spriteBatch, int gridSize);
    }
}