using System;
using Backdash;
using Backdash.Synchronizing.Random;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Symbiosis.Input;
using Symbiosis.Session;
using System.Threading;

namespace Symbiosis;

public class Game1 : Game
{
    public static ContentManager GameContent;
    public static INetcodeRandom Random;
    public static GraphicsDeviceManager Graphics;
    public static float ScreenScale
    {
        get;
        private set;
    }

    private SpriteBatch _spriteBatch;

    private INetcodeSession<PlayerInputs> _session;
    private GameSessionHandler _sessionHandler;
    private Thread _sessionThread;

    private const int _resolutionWidth = 640;
    private const int _resolutionHeight = 480;
    private int _virtualResolutionWidth = _resolutionWidth;
    private int _virtualResolutionHeight = _resolutionHeight;
    private Matrix _screenScaleMatrix = Matrix.CreateScale(ScreenScale);

    public Game1(INetcodeSession<PlayerInputs> netcodeSession)
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content/Assets";
        IsMouseVisible = false;
        _session = netcodeSession;
        GameContent = Content;
        Random = _session.Random;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        UpdateScreenScaleMatrix();
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        Graphics.PreferredBackBufferWidth = _resolutionWidth;
        Graphics.PreferredBackBufferHeight = _resolutionHeight;
        Graphics.ApplyChanges();

        UpdateScreenScaleMatrix();
        
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

        _spriteBatch.Begin(transformMatrix: _screenScaleMatrix);
        _sessionHandler.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UpdateScreenScaleMatrix()
    {
        float screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        float screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

        if (screenWidth / _resolutionWidth > screenHeight / _resolutionHeight)
        {
            float aspect = screenHeight / _resolutionHeight;
            _virtualResolutionWidth = (int)(aspect * _resolutionWidth);
            _virtualResolutionHeight = (int)screenHeight;
        }
        else
        {
            float aspect = screenWidth / _resolutionWidth;
            _virtualResolutionWidth = (int)screenWidth;
            _virtualResolutionHeight = (int)(aspect * _resolutionHeight);
        }

        ScreenScale = _virtualResolutionWidth / (float)_resolutionWidth;
        _screenScaleMatrix = Matrix.CreateScale(ScreenScale);

        GraphicsDevice.Viewport = new Viewport
        {
            X = (int)(screenWidth / 2 - _virtualResolutionWidth / 2),
            Y = (int)(screenHeight / 2 - _virtualResolutionHeight / 2),
            Width = _virtualResolutionWidth,
            Height = _virtualResolutionHeight,
            MinDepth = 0,
            MaxDepth = 1
        };
    }
}
