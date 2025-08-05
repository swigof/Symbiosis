using Backdash;
using Backdash.Synchronizing.Random;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Symbiosis.Input;
using System.Threading;

namespace Symbiosis;

public class Game1 : Game
{
    public static ContentManager GameContent;
    public static INetcodeRandom Random;
    public static GraphicsDeviceManager Graphics;

    private SpriteBatch _spriteBatch;

    private INetcodeSession<PlayerInputs> _session;
    private GameSessionHandler _sessionHandler;
    private Thread _sessionThread;

    public Game1(INetcodeSession<PlayerInputs> netcodeSession)
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content/Assets";
        IsMouseVisible = false;
        _session = netcodeSession;
        GameContent = Content;
        Random = _session.Random;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _sessionHandler = new GameSessionHandler(_session);
        if (_sessionHandler.SessionGameState.IsLocalCursorPlayer())
            IsMouseVisible = true;
        else
            IsMouseVisible = false;

        _sessionThread = new Thread(() => 
        {
            _session.Start();
            _session.SetHandler(_sessionHandler);
            _sessionHandler.Run();
        });
        _sessionThread.Start();
    }

    protected override void Dispose(bool disposing)
    {
        _sessionHandler.Dispose();
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

        InputManager.Instance.UpdateLocalInput(IsActive);

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
