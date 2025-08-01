using Backdash;
using Backdash.Synchronizing.Random;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Symbiosis.Input;

namespace Symbiosis;

public class Game1 : Game
{
    public static ContentManager GameContent;
    public static INetcodeRandom Random;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private INetcodeSession<PlayerInputs> _session;
    private GameSessionHandler _sessionHandler;

    public Game1(INetcodeSession<PlayerInputs> netcodeSession)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content/Assets";
        IsMouseVisible = false;
        _session = netcodeSession;
        GameContent = Content;
        Random = _session.Random;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _session.Start();
        _sessionHandler = new GameSessionHandler(_session);
        _session.SetHandler(_sessionHandler);

        if (_sessionHandler.IsLocalCursorPlayer())
            IsMouseVisible = true;
        else
            IsMouseVisible = false;
    }

    protected override void Dispose(bool disposing)
    {
        _session.Dispose();
        base.Dispose(disposing);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Mouse.SetCursor(MouseCursor.FromTexture2D(GameContent.Load<Texture2D>("cursor_none"), 0, 0));
        GameContent.Load<Texture2D>("frog");
        GameContent.Load<Texture2D>("spider");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _sessionHandler.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _sessionHandler.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
