using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Symbiosis
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture;
        private Vector2 _playerPosition;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content/Assets";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(0, 0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _playerTexture = Content.Load<Texture2D>("player");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                _playerPosition.Y -= (float) gameTime.ElapsedGameTime.TotalMilliseconds / 4;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                _playerPosition.Y += (float) gameTime.ElapsedGameTime.TotalMilliseconds / 4;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                _playerPosition.X -= (float) gameTime.ElapsedGameTime.TotalMilliseconds / 4;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                _playerPosition.X += (float) gameTime.ElapsedGameTime.TotalMilliseconds / 4;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_playerTexture, _playerPosition, null, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
