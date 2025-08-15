using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using System.Diagnostics;

namespace Symbiosis.Session;

public class GameSessionHandler : INetcodeSessionHandler, IDisposable
{
    readonly static Color QuarterTransparent = new Color(255, 255, 255, 255 / 4);

    public SessionGameState SessionGameState;

    INetcodeSession<PlayerInputs> _session;
    TimeSpan _sleepTime = new TimeSpan();
    PlayerInputs _localInput = new PlayerInputs();
    Vector2 _remoteCursorPosition = Vector2.Zero;
    bool _running = true;

    static readonly Rectangle _grassArea = Game1.ScreenBounds;
    static readonly Rectangle[] _shrubAreas =
    {
        new Rectangle(0, 0, Game1.ResolutionWidth, 32),
        new Rectangle(0, 0, 32, Game1.ResolutionHeight),
        new Rectangle(Game1.ResolutionWidth - 32, 0, 32, Game1.ResolutionHeight),
        new Rectangle(0, Game1.ResolutionHeight - 32, Game1.ResolutionWidth, 32)
    };
    static readonly Texture2D _cursorTexture = Game1.GameContent.Load<Texture2D>("cursor_none");
    static readonly Texture2D _grassTexture = Game1.GameContent.Load<Texture2D>("grass");
    static readonly Texture2D _shrubsTexture = Game1.GameContent.Load<Texture2D>("shrub");

    public GameSessionHandler(INetcodeSession<PlayerInputs> session)
    {
        _session = session;
        NetcodePlayer localPlayer;
        _session.TryGetLocalPlayer(out localPlayer);
        SessionGameState = new SessionGameState(_session.IsLocal(), localPlayer.Index);
    }

    public void Dispose()
    {
        _running = false;
    }

    public void Run()
    {
        TimeSpan accumulatedTime = TimeSpan.Zero;
        long previousTicks = 0;
        TimeSpan frameTime = FrameTime.Step;
        Stopwatch timer = new Stopwatch();
        timer.Start();
        while (_running)
        {
            var currentTicks = timer.Elapsed.Ticks;
            accumulatedTime += TimeSpan.FromTicks(currentTicks - previousTicks);
            previousTicks = currentTicks;

            if (accumulatedTime < frameTime)
            {
                var sleepTime = (frameTime - accumulatedTime).TotalMilliseconds;
                if (sleepTime >= 2.0)
                    System.Threading.Thread.Sleep(1);
            }
            else
            {
                while (accumulatedTime >= frameTime)
                {
                    accumulatedTime -= frameTime;
                    Update(frameTime);
                }
            }
        }
    }

    public void Update(TimeSpan elapsedTime)
    {
        if (_sleepTime > TimeSpan.Zero)
        {
            _sleepTime -= elapsedTime;
            return;
        }

        _session.BeginFrame();

        _localInput = InputManager.Instance.GetLocalInput();

        foreach (var player in _session.GetPlayers())
        {
            if (player.IsLocal())
            {
                if (_session.AddLocalInput(player, _localInput) is not ResultCode.Ok)
                    return;
            }
        }

        if (_session.SynchronizeInputs() is not ResultCode.Ok)
            return;

        var inputs = _session.CurrentSynchronizedInputs;
        if (!SessionGameState.IsLocalCursorPlayer())
        {
            _remoteCursorPosition.X = inputs[0].Input.CursorPosition.X;
            _remoteCursorPosition.Y = inputs[0].Input.CursorPosition.Y;
        }
        SessionGameState.Update(inputs);

        _session.AdvanceFrame();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_grassTexture, _grassArea, _grassArea, Color.White);
        SessionGameState.Draw(spriteBatch, gameTime);
        for (int i = 0; i < _shrubAreas.Length; i++)
            spriteBatch.Draw(_shrubsTexture, _shrubAreas[i], _shrubAreas[i], Color.White);
        if (!SessionGameState.IsLocalCursorPlayer())
            spriteBatch.Draw(_cursorTexture, _remoteCursorPosition, null, QuarterTransparent);
    }

    public void AdvanceFrame()
    {
        _session.SynchronizeInputs();
        SessionGameState.Update(_session.CurrentSynchronizedInputs);
        _session.AdvanceFrame(); 
    }

    public void OnPeerEvent(NetcodePlayer player, PeerEventInfo evt)
    {
        //throw new NotImplementedException();
    }

    public void OnSessionClose()
    {
        //throw new NotImplementedException();
    }

    public void OnSessionStart()
    {
        //throw new NotImplementedException();
    }

    public void LoadState(in Frame frame, ref readonly BinaryBufferReader reader)
    {
        SessionGameState.LoadState(in reader);
    }

    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer)
    {
        SessionGameState.SaveState(in writer);
    }

    public void TimeSync(FrameSpan framesAhead)
    {
        _sleepTime = framesAhead.Duration();
    }

    object INetcodeSessionHandler.CreateState(in Frame frame, ref readonly BinaryBufferReader reader)
    {
        GameState state = new GameState();
        state.Deserialize(in reader);
        return state;
    }
}
