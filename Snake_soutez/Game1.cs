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

        // === Neprůstřelnost ===
        public bool isInvincible = false;

        // === NOVÉ: Vizuální efekty ===
        private Texture2D pixelTexture;
        private List<Particle> particles = new List<Particle>();
        private float animationTimer = 0;
        private Texture2D gradientTexture;
        private List<Vector2> obstacles = new List<Vector2>();
        private Texture2D obstacleTexture;

        // === CYBERPUNK EFEKTY ===
        private Texture2D gridTexture;
        private Texture2D scanlineTexture;
        private List<Vector2> snakeTrail = new List<Vector2>(); // Trail za hadem
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

            // CYBERPUNK TEXTURY
            snakeTexture = CreateNeonSnakeTexture(gridSize);
            foodTexture = CreateNeonGlowTexture(gridSize, new Color(0, 255, 100)); // Magenta
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

        // === CYBERPUNK: Neonový had s outline ===
        // === CYBERPUNK: Neonový had s outline ===
        private Texture2D CreateNeonSnakeTexture(int size)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Color cyanColor = new Color(0, 255, 255); // Cyan
            Color outlineColor = new Color(100, 255, 255); // Světlejší cyan

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Neonový okraj (2 pixely od kraje)
                    if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                        data[y * size + x] = outlineColor;
                    else
                        data[y * size + x] = cyanColor;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === CYBERPUNK: Neonové glow jídlo ===
        private Texture2D CreateNeonGlowTexture(int size, Color neonColor)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size * 3, size * 3);
            Color[] data = new Color[size * 3 * size * 3];

            Vector2 center = new Vector2(size * 1.5f, size * 1.5f);

            for (int y = 0; y < size * 3; y++)
            {
                for (int x = 0; x < size * 3; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    float alpha = Math.Max(0, 1f - (dist / (size * 1.5f)));
                    alpha = (float)Math.Pow(alpha, 2); // Ostřejší glow
                    data[y * size * 3 + x] = neonColor * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === CYBERPUNK: Grid pozadí ===
        private Texture2D CreateCyberpunkGrid()
        {
            int width = _graphics.PreferredBackBufferWidth;
            int height = _graphics.PreferredBackBufferHeight;
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            Color gridColor = new Color(0, 150, 200, 80); // Cyan s průhledností
            int gridSpacing = 40;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Horizontální čáry
                    if (y % gridSpacing == 0 || y % gridSpacing == 1)
                    {
                        data[y * width + x] = gridColor;
                    }
                    // Vertikální čáry
                    else if (x % gridSpacing == 0 || x % gridSpacing == 1)
                    {
                        data[y * width + x] = gridColor;
                    }
                    else
                    {
                        data[y * width + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === CYBERPUNK: Scan lines pro CRT efekt ===
        private Texture2D CreateScanlines()
        {
            int width = _graphics.PreferredBackBufferWidth;
            int height = _graphics.PreferredBackBufferHeight;
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Každý druhý řádek tmavší
                    if (y % 2 == 0)
                    {
                        data[y * width + x] = new Color(0, 0, 0, 30);
                    }
                    else
                    {
                        data[y * width + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === CYBERPUNK: Glow kruh ===
        private Texture2D CreateGlowCircle(int size)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    float alpha = Math.Max(0, 1f - (dist / (size / 2f)));
                    alpha = (float)Math.Pow(alpha, 1.5);
                    data[y * size + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === CYBERPUNK: Gradient pozadí ===
        private Texture2D CreateCyberpunkGradient()
        {
            int width = _graphics.PreferredBackBufferWidth;
            int height = _graphics.PreferredBackBufferHeight;
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                // Tmavě modrá -> tmavě fialová
                Color topColor = new Color(5, 5, 25);
                Color bottomColor = new Color(20, 5, 30);
                Color gradient = Color.Lerp(topColor, bottomColor, t);

                for (int x = 0; x < width; x++)
                {
                    data[y * width + x] = gradient;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // === CYBERPUNK: Překážky ===
        private Texture2D CreateCyberpunkObstacleTexture(int size)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Color edgeColor = new Color(255, 0, 150); // Magenta
            Color centerColor = new Color(80, 0, 60);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Neonový okraj
                    if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                        data[y * size + x] = edgeColor;
                    else
                        data[y * size + x] = centerColor;
                }
            }

            texture.SetData(data);
            return texture;
        }

        private void CreateObstacles()
        {
            obstacles.Clear();

            // L tvar
            for (int x = 15; x < 25; x++)
                obstacles.Add(new Vector2(x * gridSize, 10 * gridSize));
            for (int y = 10; y < 20; y++)
                obstacles.Add(new Vector2(15 * gridSize, y * gridSize));

            // Křížek
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

            // Neonové částice
            for (int i = 0; i < 8; i++)
            {
                particles.Add(new Particle(foodPosition, new Color(255, 100, 200), random));
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
            portal1 = new Portal(GraphicsDevice, new Color(255, 150, 0)); // Orange
            portal2 = new Portal(GraphicsDevice, new Color(0, 255, 255)); // Cyan

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

            // Neonové částice
            for (int i = 0; i < 20; i++)
            {
                particles.Add(new Particle(powerUp.Position, new Color(255, 255, 0), random));
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
            if (portalCooldown <= )
            {
                // Teleportační částice
                for (int i = 0; i < 30; i++)
                {
                    particles.Add(new Particle(snakeParts[0], new Color(0, 255, 255), random));
                    particles.Add(new Particle(targetPosition, new Color(255, 150, 0), random));
                }

                snakeParts[0] = targetPosition;
                portalCooldown = 0.5f;

                // NOVÉ: Přemístit portály po použití
                SpawnPortals();
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

            // Power-up spawn
            powerUpSpawnTimer += deltaTime;
            if (powerUpSpawnTimer >= powerUpSpawnInterval)
            {
                SpawnPowerUp();
                powerUpSpawnTimer = 0;
            }

            // Update power-upů
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

            // Update trailu za hadem
            if (snakeParts.Count > 0)
            {
                snakeTrail.Insert(0, snakeParts[0]);
                if (snakeTrail.Count > 15)
                    snakeTrail.RemoveAt(snakeTrail.Count - 1);
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

            if (kstate.IsKeyDown(Keys.Up) && direction.Y == 0)
                nextDirection = new Vector2(0, -1);
            if (kstate.IsKeyDown(Keys.Down) && direction.Y == 0)
                nextDirection = new Vector2(0, 1);
            if (kstate.IsKeyDown(Keys.Left) && direction.X == 0)
                nextDirection = new Vector2(-1, 0);
            if (kstate.IsKeyDown(Keys.Right) && direction.X == 0)
                nextDirection = new Vector2(1, 0);
        }

        private void MoveSnake()
        {
            Vector2 newHead = snakeParts[0] + direction * gridSize;

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

            // Kontrola hranic
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

                // Neonové částice
                for (int i = 0; i < 15; i++)
                {
                    particles.Add(new Particle(newHead, new Color(255, 255, 0), random));
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
            isInvincible = false;
            activePowerUps.Clear();
            collectibles.Clear();
            powerUpSpawnTimer = 0;
            particles.Clear();
            snakeTrail.Clear();
            SpawnFood();
            SpawnPortals();
            CreateObstacles();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // 1. Gradient pozadí
            _spriteBatch.Draw(gradientTexture,
                new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                Color.White);

            // 2. Cyberpunk grid
            _spriteBatch.Draw(gridTexture,
                new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                Color.White * 0.4f);

            // 3. Překážky s glow
            foreach (var obstacle in obstacles)
            {
                float pulse = (float)Math.Sin(animationTimer * 3) * 0.3f + 0.7f;

                // Glow
                _spriteBatch.Draw(glowTexture,
                    new Rectangle((int)obstacle.X - gridSize / 2, (int)obstacle.Y - gridSize / 2,
                                 gridSize * 2, gridSize * 2),
                    new Color(255, 0, 150) * 0.3f * pulse);

                // Samotná překážka
                _spriteBatch.Draw(obstacleTexture,
                    new Rectangle((int)obstacle.X, (int)obstacle.Y, gridSize, gridSize),
                    Color.White * pulse);
            }

            // 4. Portály
            portal1?.Draw(_spriteBatch, gridSize);
            portal2?.Draw(_spriteBatch, gridSize);

            // 5. Power-upy
            foreach (var collectible in collectibles)
            {
                collectible.Draw(_spriteBatch, gridSize);
            }

            // 6. Jídlo s neonovým glow
            float foodPulse = (float)Math.Sin(animationTimer * 5) * 0.2f + 0.8f;
            _spriteBatch.Draw(foodTexture,
                new Rectangle((int)foodPosition.X - gridSize, (int)foodPosition.Y - gridSize,
                             (int)(gridSize * 3 * foodPulse), (int)(gridSize * 3 * foodPulse)),
                Color.White * 0.9f);

            // 7. Trail za hadem (světelná stopa)
            for (int i = snakeTrail.Count - 1; i >= 0; i--)
            {
                float trailAlpha = 1f - (i / (float)snakeTrail.Count);
                trailAlpha *= 0.4f;

                _spriteBatch.Draw(glowTexture,
                    new Rectangle((int)snakeTrail[i].X - gridSize / 2, (int)snakeTrail[i].Y - gridSize / 2,
                                 gridSize * 2, gridSize * 2),
                    new Color(0, 255, 255) * trailAlpha);
            }

            // 8. Had s glow efektem
            for (int i = snakeParts.Count - 1; i >= 0; i--)
            {
                float alpha = 1f - (i / (float)snakeParts.Count) * 0.3f;
                Color snakeColor = isInvincible ? new Color(255, 0, 255) : new Color(0, 255, 255); // Magenta/Cyan

                // Blikání při neprůstřelnosti
                if (isInvincible)
                {
                    alpha *= (float)Math.Sin(animationTimer * 12) * 0.4f + 0.6f;
                }

                // Glow kolem hada
                _spriteBatch.Draw(glowTexture,
                    new Rectangle((int)snakeParts[i].X - gridSize / 2, (int)snakeParts[i].Y - gridSize / 2,
                                 gridSize * 2, gridSize * 2),
                    snakeColor * alpha * 0.5f);

                // Samotný had
                _spriteBatch.Draw(snakeTexture,
                    new Rectangle((int)snakeParts[i].X, (int)snakeParts[i].Y, gridSize, gridSize),
                    Color.White * alpha);
            }

            // 9. Částice
            foreach (var particle in particles)
            {
                particle.Draw(_spriteBatch, pixelTexture);
            }

            // 10. Scan lines (CRT efekt)
            _spriteBatch.Draw(scanlineTexture,
                new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                Color.White);

            // 11. UI s neonovým stylem
            if (font != null)
            {
                // Pozadí pro skóre
                _spriteBatch.Draw(pixelTexture, new Rectangle(5, 5, 220, 35), new Color(0, 0, 0, 180));
                _spriteBatch.Draw(pixelTexture, new Rectangle(5, 5, 220, 2), new Color(0, 255, 255)); // Horní čára
                _spriteBatch.DrawString(font, $"SCORE: {score}", new Vector2(10, 12), new Color(0, 255, 255));

                // Aktivní power-upy
                int yOffset = 50;
                foreach (var kvp in activePowerUps)
                {
                    string powerUpName = kvp.Key.ToString().ToUpper();
                    _spriteBatch.Draw(pixelTexture, new Rectangle(5, yOffset - 3, 280, 28), new Color(0, 0, 0, 180));
                    _spriteBatch.Draw(pixelTexture, new Rectangle(5, yOffset - 3, 280, 2), new Color(255, 255, 0));
                    _spriteBatch.DrawString(font, $"> {powerUpName}: {(int)kvp.Value}s",
                        new Vector2(10, yOffset), new Color(255, 255, 0));
                    yOffset += 30;
                }

                // Game over
                if (gameOver)
                {
                    int boxWidth = 450;
                    int boxHeight = 120;
                    int boxX = (_graphics.PreferredBackBufferWidth - boxWidth) / 2;
                    int boxY = 200;

                    // Box s neonovým okrajem
                    // Box s neonovým okrajem
                    _spriteBatch.Draw(pixelTexture, new Rectangle(boxX, boxY, boxWidth, boxHeight),
                        new Color(0, 0, 0, 220));

                    // Neonový okraj
                    _spriteBatch.Draw(pixelTexture, new Rectangle(boxX, boxY, boxWidth, 3), new Color(255, 0, 255));
                    _spriteBatch.Draw(pixelTexture, new Rectangle(boxX, boxY + boxHeight - 3, boxWidth, 3), new Color(255, 0, 255));
                    _spriteBatch.Draw(pixelTexture, new Rectangle(boxX, boxY, 3, boxHeight), new Color(255, 0, 255));
                    _spriteBatch.Draw(pixelTexture, new Rectangle(boxX + boxWidth - 3, boxY, 3, boxHeight), new Color(255, 0, 255));

                    // Text
                    string gameOverText = "GAME OVER";
                    string scoreText = $"FINAL SCORE: {score}";
                    string restartText = "PRESS ENTER TO RESTART";

                    Vector2 gameOverSize = font.MeasureString(gameOverText);
                    Vector2 scoreSize = font.MeasureString(scoreText);
                    Vector2 restartSize = font.MeasureString(restartText);

                    _spriteBatch.DrawString(font, gameOverText,
                        new Vector2(boxX + (boxWidth - gameOverSize.X) / 2, boxY + 20),
                        new Color(255, 0, 255));

                    _spriteBatch.DrawString(font, scoreText,
                        new Vector2(boxX + (boxWidth - scoreSize.X) / 2, boxY + 55),
                        new Color(0, 255, 255));

                    _spriteBatch.DrawString(font, restartText,
                        new Vector2(boxX + (boxWidth - restartSize.X) / 2, boxY + 85),
                        new Color(255, 255, 0));
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}