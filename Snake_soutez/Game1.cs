using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
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
        private Vector2 nextDirection;
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
        private float powerUpSpawnInterval = 10f;
        private Dictionary<PowerUpType, float> activePowerUps = new Dictionary<PowerUpType, float>();

        // === Portály ===
        private Portal portal1;
        private Portal portal2;
        private float portalCooldown = 0;

        // === Zmrzlá podlaha ===
        private float floorTimer = 0;
        private float floorCycleTime = 60f;
        private float freezeDuration = 20f;
        private bool isFloorFrozen = false;
        private Vector2 momentum = Vector2.Zero;
        private Texture2D iceTexture;

        // === Neprůstřelnost ===
        public bool isInvincible = false;

        // === NOVÉ: Vizuální efekty ===
        private Texture2D pixelTexture;
        private List<Particle> particles = new List<Particle>();
        private float animationTimer = 0;
        private Texture2D gradientTexture;
        private List<Vector2> obstacles = new List<Vector2>(); // Překážky ve hře
        private Texture2D obstacleTexture;

        // Další efekty
        private Texture2D gridTexture;
        private Texture2D scanlineTexture;
        private List<Vector2> snakeTrail = new List<Vector2>();
        private Texture2D glowTexture;


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

            // Základní pixel textura
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Textury
            snakeTexture = CreateNeonSnakeTexture(gridSize);
            foodTexture = CreateNeonGlowTexture(gridSize, new Color(255, 0, 100)); // Magenta
            obstacleTexture = CreateCyberpunkObstacleTexture(gridSize);
            gradientTexture = CreateCyberpunkGradient();
            gridTexture = CreateCyberpunkGrid();
            scanlineTexture = CreateScanlines();
            glowTexture = CreateGlowCircle(gridSize * 2);

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
            CreateObstacles();
        }

        // === NOVÉ: Vytvoření zakulacené textury pro hada ===
        private Texture2D CreateNeonSnakeTexture(int size)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);

                    if (dist <= radius)
                    {
                        data[y * size + x] = new Color(0, 255, 255);
                    }
                    else if (dist <= radius + 2)
                    {
                        data[y * size + x] = new Color(0, 255, 255)
                    }
                    else
                    {
                        data[y * size + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === NOVÉ: Vytvoření svítící textury pro jídlo ===
        private Texture2D CreateNeonGlowTexture(int size, Color neonColor)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size * 3, size * 3);
            Color[] data = new Color[size * 3 * size * 3];

            Vector2 center = new Vector2(size * 1,5f, size * 1,5f);

            for (int y = 0; y < size * 3; y++)
            {
                for (int x = 0; x < size * 3; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    float alpha = Math.Max(0, 1f - (dist / size));
                    data[y * size * 3 + x] = neonColor * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === NOVÉ: Ledová textura ===
        private Texture2D CreateCyberpunkgrid()
        {
            int width = _graphics.PreferredBackBufferWidth;
            int height = _graphics.PreferredBackBufferHeight;
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];

            Color gridColor = new Color(0, 150, 200, 80);
            int gridSpacing = 40;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y % gridSpacing == 0 || x % gridSpacing == 1)
                    {
                        data[y * gridSpacing + x] = gridColor;
                    }

                    else if (x % gridSpacing == 0 || y % gridSpacing == 1)
                    {
                        data[y * width + x] = gridColor;
                    }
                    else
                    {
                        data[y * width + x] = gridColor;
                    }

                }
            }

            texture.SetData(data);
            return texture;
        }

        // === NOVÉ: Textura pro překážky ===
        private Texture2D CreateObstacleTexture(int size)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                        data[y * size + x] = new Color(80, 80, 90);
                    else
                        data[y * size + x] = new Color(50, 50, 60);
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === NOVÉ: Gradient pro pozadí ===
        private Texture2D CreateGradientTexture()
        {
            int width = _graphics.PreferredBackBufferWidth;
            int height = _graphics.PreferredBackBufferHeight;
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color topColor = new Color(10, 15, 35);
                Color bottomColor = new Color(25, 15, 45);
                Color gradient = Color.Lerp(topColor, bottomColor, t);

                for (int x = 0; x < width; x++)
                {
                    data[y * width + x] = gradient;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === NOVÉ: Vytvoření překážek (neobvyklý tvar hrací plochy) ===
        private void CreateObstacles()
        {
            obstacles.Clear();

            // Vytvoření "L" tvaru překážek
            for (int x = 15; x < 25; x++)
            {
                obstacles.Add(new Vector2(x * gridSize, 10 * gridSize));
            }
            for (int y = 10; y < 20; y++)
            {
                obstacles.Add(new Vector2(15 * gridSize, y * gridSize));
            }

            // Křížek uprostřed
            for (int i = -3; i <= 3; i++)
            {
                obstacles.Add(new Vector2((20 + i) * gridSize, 15 * gridSize));
                obstacles.Add(new Vector2(20 * gridSize, (15 + i) * gridSize));
            }

            // Rohové bloky
            for (int x = 35; x < 38; x++)
            {
                for (int y = 5; y < 8; y++)
                {
                    obstacles.Add(new Vector2(x * gridSize, y * gridSize));
                }
            }
        }

        private void SpawnFood()
        {
            int maxX = _graphics.PreferredBackBufferWidth / gridSize;
            int maxY = _graphics.PreferredBackBufferHeight / gridSize;

            Vector2 newPos;
            do
            {
                newPos = new Vector2(random.Next(maxX) * gridSize, random.Next(maxY) * gridSize);
            } while (snakeParts.Contains(newPos) || IsPositionOccupied(newPos) || obstacles.Contains(newPos));

            foodPosition = newPos;

            // Přidat částice kolem jídla
            for (int i = 0; i < 5; i++)
            {
                particles.Add(new Particle(foodPosition, Color.OrangeRed, random));
            }
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

            while (portal1.Position == portal2.Position || obstacles.Contains(portal1.Position) || obstacles.Contains(portal2.Position))
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

            Vector2 spawnPos;
            do
            {
                powerUp.Spawn(gridSize, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                spawnPos = powerUp.Position;
            } while (obstacles.Contains(spawnPos));

            collectibles.Add(powerUp);
        }

        public void ActivatePowerUp(PowerUp powerUp)
        {
            activePowerUps[powerUp.Type] = powerUp.Duration;

            // Efekt částic
            for (int i = 0; i < 15; i++)
            {
                particles.Add(new Particle(powerUp.Position, Color.Yellow, random));
            }

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
                // Efekt částic při teleportaci
                for (int i = 0; i < 20; i++)
                {
                    particles.Add(new Particle(snakeParts[0], Color.Cyan, random));
                    particles.Add(new Particle(targetPosition, Color.Orange, random));
                }

                snakeParts[0] = targetPosition;
                portalCooldown = 0.5f;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            animationTimer += deltaTime;

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

                // Efekt mrazu
                for (int i = 0; i < 30; i++)
                {
                    particles.Add(new Particle(
                        new Vector2(random.Next(_graphics.PreferredBackBufferWidth),
                                  random.Next(_graphics.PreferredBackBufferHeight)),
                        Color.LightBlue, random));
                }
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

            // Update částic
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(deltaTime);
                if (particles[i].Life <= 0)
                    particles.RemoveAt(i);
            }

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
                if (kstate.IsKeyDown(Keys.Up) && direction.Y == 0)
                    nextDirection = new Vector2(0, -1);
                if (kstate.IsKeyDown(Keys.Down) && direction.Y == 0)
                    nextDirection = new Vector2(0, 1);
                if (kstate.IsKeyDown(Keys.Left) && direction.X == 0)
                    nextDirection = new Vector2(-1, 0);
                if (kstate.IsKeyDown(Keys.Right) && direction.X == 0)
                    nextDirection = new Vector2(1, 0);

                momentum = Vector2.Lerp(momentum, nextDirection, 0.3f);
            }
            else
            {
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

            // Kontrola překážek
            if (obstacles.Contains(newHead) && !isInvincible)
            {
                gameOver = true;
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
                if (hitWall)
                {
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

                // Trail částice
                for (int i = 0; i < 10; i++)
                {
                    particles.Add(new Particle(newHead, Color.Gold, random));
                }

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
            particles.Clear();
            SpawnFood();
            SpawnPortals();
            CreateObstacles();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Gradient pozadí
            _spriteBatch.Draw(gradientTexture, new Rectangle(0, 0,
                _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                Color.White);

            // Animované hvězdy na pozadí
            for (int i = 0; i < 50; i++)
            {
                float x = (i * 137.5f) % _graphics.PreferredBackBufferWidth;
                float y = (i * 217.3f) % _graphics.PreferredBackBufferHeight;
                float twinkle = (float)Math.Sin(animationTimer * 2 + i) * 0.5f + 0.5f;
                _spriteBatch.Draw(pixelTexture, new Rectangle((int)x, (int)y, 2, 2),
                    Color.White * twinkle * 0.6f);
            }

            // Efekt zmrzlé podlahy
            if (isFloorFrozen)
            {
                for (int x = 0; x < _graphics.PreferredBackBufferWidth; x += gridSize)
                {
                    for (int y = 0; y < _graphics.PreferredBackBufferHeight; y += gridSize)
                    {
                        float wave = (float)Math.Sin(animationTimer * 3 + x * 0.1f + y * 0.1f);
                        _spriteBatch.Draw(iceTexture,
                            new Rectangle(x, y, gridSize, gridSize),
                            Color.White * (0.2f + wave * 0.1f));
                    }
                }
            }

            // Překážky
            foreach (var obstacle in obstacles)
            {
                float pulse = (float)Math.Sin(animationTimer * 2) * 0.1f + 0.9f;
                _spriteBatch.Draw(obstacleTexture,
                    new Rectangle((int)obstacle.X, (int)obstacle.Y, gridSize, gridSize),
                    Color.White * pulse);
            }

            // Portály
            portal1?.Draw(_spriteBatch, gridSize);
            portal2?.Draw(_spriteBatch, gridSize);

            // Power-upy
            foreach (var collectible in collectibles)
            {
                collectible.Draw(_spriteBatch, gridSize);
            }

            // Částice
            foreach (var particle in particles)
            {
                particle.Draw(_spriteBatch, pixelTexture);
            }

            // Jídlo s pulzací
            float foodPulse = (float)Math.Sin(animationTimer * 4) * 0.15f + 0.85f;
            _spriteBatch.Draw(foodTexture,
                new Rectangle((int)foodPosition.X - gridSize / 2, (int)foodPosition.Y - gridSize / 2,
                              (int)(gridSize * 2 * foodPulse), (int)(gridSize * 2 * foodPulse)),
                Color.White);

            // Had s trail efektem
            for (int i = snakeParts.Count - 1; i >= 0; i--)
            {
                float alpha = 1f - (i / (float)snakeParts.Count) * 0.5f;
                Color snakeColor = isInvincible ? Color.Purple : Color.LimeGreen;

                // Blikání při neprůstřelnosti
                if (isInvincible)
                {
                    alpha *= (float)Math.Sin(animationTimer * 10) * 0.3f + 0.7f;
                }

                _spriteBatch.Draw(snakeTexture,
                    new Rectangle((int)snakeParts[i].X, (int)snakeParts[i].Y, gridSize, gridSize),
                    snakeColor * alpha);
            }

            // UI s poloprůhledným pozadím
            if (font != null)
            {
                // Pozadí pro skóre
                _spriteBatch.Draw(pixelTexture, new Rectangle(5, 5, 200, 30), Color.Black * 0.6f);
                _spriteBatch.DrawString(font, $"Skóre: {score}", new Vector2(10, 10), Color.White);

                if (isFloorFrozen)
                {
                    float timeLeft = freezeDuration - floorTimer;
                    _spriteBatch.Draw(pixelTexture, new Rectangle(5, 40, 150, 25), Color.Black * 0.6f);
                    _spriteBatch.DrawString(font, $"❄ LED! {(int)timeLeft}s",
                        new Vector2(10, 43), Color.Cyan);
                }

                // Aktivní power-upy
                int yOffset = isFloorFrozen ? 70 : 40;
                foreach (var kvp in activePowerUps)
                {
                    string powerUpName = kvp.Key.ToString();
                    _spriteBatch.Draw(pixelTexture, new Rectangle(5, yOffset - 3, 250, 25), Color.Black * 0.6f);
                    _spriteBatch.DrawString(font, $"⚡ {powerUpName}: {(int)kvp.Value}s",
                        new Vector2(10, yOffset), Color.Yellow);
                    yOffset += 25;
                }

                // Game over
                if (gameOver)
                {
                    int boxWidth = 400;
                    int boxHeight = 100;
                    int boxX = (_graphics.PreferredBackBufferWidth - boxWidth) / 2;
                    int boxY = 220;

                    _spriteBatch.Draw(pixelTexture,
                        new Rectangle(boxX, boxY, boxWidth, boxHeight),
                        Color.Black * 0.8f);

                    string text = $"GAME OVER!\nSkóre: {score}\n\nStiskni ENTER";
                    Vector2 size = font.MeasureString(text);
                    _spriteBatch.DrawString(font, text,
                        new Vector2((_graphics.PreferredBackBufferWidth - size.X) / 2, 240),
                        Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    // === NOVÁ TŘÍDA: Částice pro vizuální efekty ===
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float MaxLife;

        public Particle(Vector2 position, Color color, Random random)
        {
            Position = position;
            Color = color;
            Life = 1f;
            MaxLife = 1f;

            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float speed = 30 + (float)random.NextDouble() * 50;
            Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
        }

        public void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
            Velocity *= 0.95f; // Zpomalení
            Life -= deltaTime * 2;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (Life > 0)
            {
                float alpha = Life / MaxLife;
                spriteBatch.Draw(texture,
                    new Rectangle((int)Position.X, (int)Position.Y, 3, 3),
                    Color * alpha);
            }
        }
    }
}