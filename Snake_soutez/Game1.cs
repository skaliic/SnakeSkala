using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake_soutez
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // === Proměnné hada ===
        private Texture2D snakeTexture;
        private List<Vector2> snakeParts;
        private Vector2 direction;
        private Vector2 nextDirection; // Pro plynulejší ovládání
        private float moveTimer;
        private float baseMoveDelay = 0.1f;
        public float moveDelay = 0.1f;
        private int gridSize = 20;

        // === Jídlo ===
        private Texture2D foodTexture;
        private Vector2 foodPosition;
        private Random random = new Random();

        // === Herní stav ===
        private bool gameOver = false;
        private SpriteFont font;
        private int score = 0;
        private int scoreMultiplier = 1;

        // === Power-upy ===
        private List<ICollectible> collectibles = new List<ICollectible>();
        private float powerUpSpawnTimer = 0;
        private float powerUpSpawnInterval = 10f; // Co 10 sekund nový power-up
        private Dictionary<PowerUpType, float> activePowerUps = new Dictionary<PowerUpType, float>();

        // === Portály ===
        private Portal portal1;
        private Portal portal2;
        private float portalCooldown = 0;
        private float lastTeleportTime = 0;

        // === Zmrzlá podlaha ===
        private float floorTimer = 0;
        private float floorCycleTime = 60f; // Každých 60 sekund
        private float freezeDuration = 20f; // Na 20 sekund
        private bool isFloorFrozen = false;
        private Vector2 momentum = Vector2.Zero;
        private Texture2D iceTexture;

        // === Neprůstřelnost ===
        public bool isInvincible = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Textury
            snakeTexture = new Texture2D(GraphicsDevice, 1, 1);
            snakeTexture.SetData(new[] { Color.Green });

            foodTexture = new Texture2D(GraphicsDevice, 1, 1);
            foodTexture.SetData(new[] { Color.Red });

            iceTexture = new Texture2D(GraphicsDevice, 1, 1);
            iceTexture.SetData(new[] { Color.Cyan });

            // Font
            try
            {
                font = Content.Load<SpriteFont>("DefaultFont");
            }
            catch { font = null; }

            // Vytvoření hada
            snakeParts = new List<Vector2> { new Vector2(100, 100) };
            direction = new Vector2(1, 0);
            nextDirection = direction;

            SpawnFood();
            SpawnPortals();
        }

        private void SpawnFood()
        {
            int maxX = _graphics.PreferredBackBufferWidth / gridSize;
            int maxY = _graphics.PreferredBackBufferHeight / gridSize;

            Vector2 newPos;
            do
            {
                newPos = new Vector2(random.Next(maxX) * gridSize, random.Next(maxY) * gridSize);
            } while (snakeParts.Contains(newPos) || IsPositionOccupied(newPos));

            foodPosition = newPos;
        }

        private bool IsPositionOccupied(Vector2 pos)
        {
            foreach (var col in collectibles)
            {
                if (col.Position == pos) return true;
            }
            if (portal1 != null && portal1.Position == pos) return true;
            if (portal2 != null && portal2.Position == pos) return true;
            return false;
        }

        private void SpawnPortals()
        {
            portal1 = new Portal(GraphicsDevice, Color.Orange);
            portal2 = new Portal(GraphicsDevice, Color.Cyan);

            portal1.Spawn(gridSize, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            portal2.Spawn(gridSize, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            // Zajistit, že portály nejsou na stejné pozici
            while (portal1.Position == portal2.Position)
            {
                portal2.Spawn(gridSize, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            }

            portal1.LinkedPortal = portal2;
            portal2.LinkedPortal = portal1;
        }

        private void SpawnPowerUp()
        {
            PowerUpType[] types = (PowerUpType[])Enum.GetValues(typeof(PowerUpType));
            PowerUpType randomType = types[random.Next(types.Length)];

            PowerUp powerUp = new PowerUp(randomType, GraphicsDevice);
            powerUp.Spawn(gridSize, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            collectibles.Add(powerUp);
        }

        public void ActivatePowerUp(PowerUp powerUp)
        {
            activePowerUps[powerUp.Type] = powerUp.Duration;

            switch (powerUp.Type)
            {
                case PowerUpType.SpeedBoost:
                    moveDelay = baseMoveDelay * 0.5f;
                    break;
                case PowerUpType.SlowDown:
                    moveDelay = baseMoveDelay * 1.5f;
                    break;
                case PowerUpType.ScoreDouble:
                    scoreMultiplier = 2;
                    break;
                case PowerUpType.Invincibility:
                    isInvincible = true;
                    break;
            }
        }

        public void TeleportSnake(Vector2 targetPosition)
        {
            if (portalCooldown <= 0)
            {
                snakeParts[0] = targetPosition;
                portalCooldown = 0.5f; // Cooldown aby se neteleportoval neustále
            }
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    RestartGame();
                return;
            }

            // === Zmrzlá podlaha timer ===
            floorTimer += deltaTime;
            if (floorTimer >= floorCycleTime && !isFloorFrozen)
            {
                isFloorFrozen = true;
                floorTimer = 0;
            }
            else if (isFloorFrozen && floorTimer >= freezeDuration)
            {
                isFloorFrozen = false;
                momentum = Vector2.Zero;
                floorTimer = 0;
            }

            // === Power-up spawn timer ===
            powerUpSpawnTimer += deltaTime;
            if (powerUpSpawnTimer >= powerUpSpawnInterval)
            {
                SpawnPowerUp();
                powerUpSpawnTimer = 0;
            }

            // === Update power-upů ===
            List<PowerUpType> toRemove = new List<PowerUpType>();
            foreach (var kvp in activePowerUps.ToList())
            {
                activePowerUps[kvp.Key] -= deltaTime;
                if (activePowerUps[kvp.Key] <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var type in toRemove)
            {
                activePowerUps.Remove(type);
                switch (type)
                {
                    case PowerUpType.SpeedBoost:
                    case PowerUpType.SlowDown:
                        moveDelay = baseMoveDelay;
                        break;
                    case PowerUpType.ScoreDouble:
                        scoreMultiplier = 1;
                        break;
                    case PowerUpType.Invincibility:
                        isInvincible = false;
                        break;
                }
            }

            // Portal cooldown
            if (portalCooldown > 0)
                portalCooldown -= deltaTime;

            HandleInput();
            moveTimer += deltaTime;

            if (moveTimer >= moveDelay)
            {
                moveTimer = 0;
                direction = nextDirection;
                MoveSnake();
            }

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            var kstate = Keyboard.GetState();

            if (isFloorFrozen)
            {
                // Na ledě - klouzání s hybností
                if (kstate.IsKeyDown(Keys.Up) && direction.Y == 0)
                    nextDirection = new Vector2(0, -1);
                if (kstate.IsKeyDown(Keys.Down) && direction.Y == 0)
                    nextDirection = new Vector2(0, 1);
                if (kstate.IsKeyDown(Keys.Left) && direction.X == 0)
                    nextDirection = new Vector2(-1, 0);
                if (kstate.IsKeyDown(Keys.Right) && direction.X == 0)
                    nextDirection = new Vector2(1, 0);

                // Hybnost - had se snaží udržet směr
                momentum = Vector2.Lerp(momentum, nextDirection, 0.3f);
            }
            else
            {
                // Normální ovládání
                if (kstate.IsKeyDown(Keys.Up) && direction.Y == 0)
                    nextDirection = new Vector2(0, -1);
                if (kstate.IsKeyDown(Keys.Down) && direction.Y == 0)
                    nextDirection = new Vector2(0, 1);
                if (kstate.IsKeyDown(Keys.Left) && direction.X == 0)
                    nextDirection = new Vector2(-1, 0);
                if (kstate.IsKeyDown(Keys.Right) && direction.X == 0)
                    nextDirection = new Vector2(1, 0);
            }
        }

        private void MoveSnake()
        {
            Vector2 moveDirection = isFloorFrozen ?
                new Vector2(Math.Sign(momentum.X), Math.Sign(momentum.Y)) : direction;

            Vector2 newHead = snakeParts[0] + moveDirection * gridSize;

            // Kontrola portálů
            if (portal1 != null && newHead == portal1.Position && portalCooldown <= 0)
            {
                portal1.Apply(this);
                return;
            }
            if (portal2 != null && newHead == portal2.Position && portalCooldown <= 0)
            {
                portal2.Apply(this);
                return;
            }

            // Kontrola hranic a kolize se sebou
            bool hitWall = newHead.X < 0 || newHead.Y < 0 ||
                          newHead.X >= _graphics.PreferredBackBufferWidth ||
                          newHead.Y >= _graphics.PreferredBackBufferHeight;

            bool hitSelf = snakeParts.Contains(newHead);

            if ((hitWall || hitSelf) && !isInvincible)
            {
                gameOver = true;
                return;
            }
            else if ((hitWall || hitSelf) && isInvincible)
            {
                // S neprůstřelností projde zdí nebo sebou
                if (hitWall)
                {
                    // Wrap around na druhou stranu
                    if (newHead.X < 0) newHead.X = _graphics.PreferredBackBufferWidth - gridSize;
                    if (newHead.X >= _graphics.PreferredBackBufferWidth) newHead.X = 0;
                    if (newHead.Y < 0) newHead.Y = _graphics.PreferredBackBufferHeight - gridSize;
                    if (newHead.Y >= _graphics.PreferredBackBufferHeight) newHead.Y = 0;
                }
            }

            // Kontrola jídla
            if (newHead == foodPosition)
            {
                snakeParts.Insert(0, newHead);
                score += 10 * scoreMultiplier;
                SpawnFood();
            }
            else
            {
                snakeParts.Insert(0, newHead);
                snakeParts.RemoveAt(snakeParts.Count - 1);
            }

            // Kontrola power-upů
            for (int i = collectibles.Count - 1; i >= 0; i--)
            {
                if (collectibles[i].IsActive && collectibles[i].Position == newHead)
                {
                    collectibles[i].Apply(this);
                    collectibles[i].IsActive = false;
                    collectibles.RemoveAt(i);
                }
            }
        }

        private void RestartGame()
        {
            snakeParts = new List<Vector2> { new Vector2(100, 100) };
            direction = new Vector2(1, 0);
            nextDirection = direction;
            moveTimer = 0;
            moveDelay = baseMoveDelay;
            gameOver = false;
            score = 0;
            scoreMultiplier = 1;
            floorTimer = 0;
            isFloorFrozen = false;
            momentum = Vector2.Zero;
            isInvincible = false;
            activePowerUps.Clear();
            collectibles.Clear();
            powerUpSpawnTimer = 0;
            SpawnFood();
            SpawnPortals();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(isFloorFrozen ? new Color(200, 230, 255) : Color.Black);

            _spriteBatch.Begin();

            // Efekt zmrzlé podlahy
            if (isFloorFrozen)
            {
                for (int x = 0; x < _graphics.PreferredBackBufferWidth; x += gridSize * 2)
                {
                    for (int y = 0; y < _graphics.PreferredBackBufferHeight; y += gridSize * 2)
                    {
                        _spriteBatch.Draw(iceTexture,
                            new Rectangle(x, y, gridSize, gridSize),
                            Color.White * 0.1f);
                    }
                }
            }

            // Vykresli portály
            portal1?.Draw(_spriteBatch, gridSize);
            portal2?.Draw(_spriteBatch, gridSize);

            // Vykresli power-upy
            foreach (var collectible in collectibles)
            {
                collectible.Draw(_spriteBatch, gridSize);
            }

            // Vykresli hada
            Color snakeColor = isInvincible ? Color.Purple : Color.Green;
            foreach (var part in snakeParts)
            {
                _spriteBatch.Draw(snakeTexture,
                    new Rectangle((int)part.X, (int)part.Y, gridSize, gridSize),
                    snakeColor);
            }

            // Vykresli jídlo
            _spriteBatch.Draw(foodTexture,
                new Rectangle((int)foodPosition.X, (int)foodPosition.Y, gridSize, gridSize),
                Color.Red);

            // UI text
            if (font != null)
            {
                _spriteBatch.DrawString(font, $"Skóre: {score}", new Vector2(10, 10), Color.White);

                if (isFloorFrozen)
                {
                    float timeLeft = freezeDuration - floorTimer;
                    _spriteBatch.DrawString(font, $"LED! {(int)timeLeft}s",
                        new Vector2(10, 30), Color.Cyan);
                }

                // Aktivní power-upy
                int yOffset = 50;
                foreach (var kvp in activePowerUps)
                {
                    string powerUpName = kvp.Key.ToString();
                    _spriteBatch.DrawString(font, $"{powerUpName}: {(int)kvp.Value}s",
                        new Vector2(10, yOffset), Color.Yellow);
                    yOffset += 20;
                }

                // Game over
                if (gameOver)
                {
                    string text = $"Game Over! Skóre: {score}\nStiskni ENTER pro restart.";
                    Vector2 size = font.MeasureString(text);
                    _spriteBatch.DrawString(font, text,
                        new Vector2((_graphics.PreferredBackBufferWidth - size.X) / 2, 250),
                        Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}