using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Arkanoid
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _background;
        private Texture2D _paddleTexture;
        private Texture2D _ballTexture;
        private Texture2D _blockTexture;

        private Vector2 _paddlePosition;
        private Vector2 _ballPosition;
        private Vector2 _ballVelocity;
        private bool _ballIsMoving;
        private List<Block> _blocks;

        private int _screenWidth;
        private int _screenHeight;

        private const int BlockWidth = 64;
        private const int BlockHeight = 32;
        private const float BlockSpacing = 0.001f; // Adjust spacing between blocks as needed

        private int _score;
        private SpriteFont _scoreFont;
        private int _lives;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set the preferred window size
            _graphics.PreferredBackBufferWidth = 1024; // Adjust as needed
            _graphics.PreferredBackBufferHeight = 600; // Adjust as needed

       
        }

        protected override void Initialize()
        {
            _screenWidth = _graphics.PreferredBackBufferWidth;
            _screenHeight = _graphics.PreferredBackBufferHeight;

            // Ensure the window size matches the preferred back buffer size
            _graphics.ApplyChanges();

            // Initialize game elements
            _paddlePosition = new Vector2(_screenWidth / 2 - 50, _screenHeight - 30);
            _ballPosition = _paddlePosition - new Vector2(0, 10);
            _ballVelocity = Vector2.Zero;
            _ballIsMoving = false;

            _blocks = new List<Block>();
            int totalBlockWidth = BlockWidth * 16; // 16 blocks per row
            int totalBlockHeight = BlockHeight * 8; // 8 rows of blocks
            int startX = (_screenWidth - totalBlockWidth) / 2; // Starting X position to center blocks
            int startY = 40; // Starting Y position at the top of the window

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Vector2 blockPosition = new Vector2(startX + x * BlockWidth, startY + y * BlockHeight);
                    _blocks.Add(new Block(blockPosition));
                }
            }

            _score = 0; // Initialize score to zero
            _lives = 3; // Initialize lives to 3

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background = Content.Load<Texture2D>("bg1");
            _paddleTexture = Content.Load<Texture2D>("paddle");
            _ballTexture = Content.Load<Texture2D>("ball");
            _blockTexture = Content.Load<Texture2D>("boxx");

            _scoreFont = Content.Load<SpriteFont>("ScoreFont"); // Load the score font
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                _paddlePosition.X -= 5f;
            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                _paddlePosition.X += 5f;

            _paddlePosition.X = MathHelper.Clamp(_paddlePosition.X, 0, _screenWidth - _paddleTexture.Width);

            if (!_ballIsMoving)
            {
                // Calculate the center of the paddle
                float paddleCenterX = _paddlePosition.X + _paddleTexture.Width / 2;
                float ballCenterX = paddleCenterX - _ballTexture.Width / 2;

                _ballPosition = new Vector2(ballCenterX, _paddlePosition.Y - _ballTexture.Height);

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    _ballIsMoving = true;
                    _ballVelocity = new Vector2(4f, -4f); // Adjust velocity as needed
                }
            }
            else
            {
                _ballPosition += _ballVelocity;

                if (_ballPosition.X <= 0 || _ballPosition.X >= _screenWidth - _ballTexture.Width)
                    _ballVelocity.X *= -1;
                if (_ballPosition.Y <= 0)
                    _ballVelocity.Y *= -1;
                if (_ballPosition.Y >= _screenHeight - _ballTexture.Height)
                {
                    _ballIsMoving = false;
                    _ballVelocity = Vector2.Zero;
                    _lives--;

                    if (_lives <= 0)
                    {
                        //Reset the Game
                        _lives = 3;
                        _score = 0;
                        _ballPosition = _paddlePosition - new Vector2(0, 10);
                    }
                }

                Rectangle ballRect = new Rectangle((int)_ballPosition.X, (int)_ballPosition.Y, _ballTexture.Width, _ballTexture.Height);
                Rectangle paddleRect = new Rectangle((int)_paddlePosition.X, (int)_paddlePosition.Y, _paddleTexture.Width, _paddleTexture.Height);

                if (ballRect.Intersects(paddleRect))
                {
                    _ballVelocity.Y *= -1;
                    _ballPosition.Y = paddleRect.Y - _ballTexture.Height;
                }

                for (int i = 0; i < _blocks.Count; i++)
                {
                    if (_blocks[i].IsDestroyed)
                        continue;

                    Rectangle blockRect = new Rectangle((int)_blocks[i].Position.X, (int)_blocks[i].Position.Y, BlockWidth, BlockHeight);

                    if (ballRect.Intersects(blockRect))
                    {
                        _ballVelocity.Y *= -1;
                        _blocks[i].Hits++;
                        if (_blocks[i].Hits >= 3)
                        {
                            _blocks[i].IsDestroyed = true;
                            _score += 100; // Increment score for each block destroyed
                        }
                        else
                        {
                            _blocks[i].ChangeColor();
                        }
                        break;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(_background, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);
            _spriteBatch.Draw(_paddleTexture, _paddlePosition, Color.White);
            _spriteBatch.Draw(_ballTexture, _ballPosition, Color.White);

            foreach (var block in _blocks)
            {
                if (!block.IsDestroyed)
                    _spriteBatch.Draw(_blockTexture, block.Position, block.Color);
            }

            // Draw score on the screen
            _spriteBatch.DrawString(_scoreFont, $"Score: {_score}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_scoreFont, $"Lives: {_lives}", new Vector2(80, 10), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class Block
    {
        public Vector2 Position { get; private set; }
        public int Hits { get; set; }
        public bool IsDestroyed { get; set; }
        public Color Color { get; private set; }

        public Block(Vector2 position)
        {
            Position = position;
            Hits = 0;
            IsDestroyed = false;
            Color = Color.Blue;
        }

        public void ChangeColor()
        {
            if (Hits == 1)
                Color = Color.Green;
            else if (Hits == 2)
                Color = Color.Red;
        }
    }
}
