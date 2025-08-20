using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Symbiosis.Graphics;
using Symbiosis.Manager;
using Symbiosis.UI;

namespace Symbiosis.Session;

// Provides thread protected game state access
public class SessionGameState
{
    GameState _gameState = new GameState();
    Mutex _stateMutex = new Mutex();

    float _endTextScale = 0;
    Vector2 _endTextPosition = new Vector2(Game1.ResolutionWidth / 2f, 0); 
    Menu _menu;
    Vector2 _remoteCursorPosition = Vector2.Zero;

    static readonly SpriteFont _font = Game1.GameContent.Load<SpriteFont>("PublicPixel");
    static readonly Rectangle _paddedBounds = new Rectangle(
        Game1.ScreenBounds.X - 100,
        Game1.ScreenBounds.Y - 100,
        Game1.ScreenBounds.Width + 200,
        Game1.ScreenBounds.Height + 200
    );
    static readonly Sprite _homeTexture = Game1.Atlas.CreateSprite("hole");
    static readonly Sprite _homeShadowTexture = Game1.Atlas.CreateSprite("hole-shadow");
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
    static readonly Color QuarterTransparent = new Color(255, 255, 255, 255 / 4);

    public SessionGameState(bool isLocal, int firstLocalPlayerIndex)
    {
        if (isLocal)
        {
            _gameState.Spider = new Spider(true);
            _gameState.Frog = new Frog(true);
        }
        else
        {
            // Default player 1 to spider and 2 to frog for now
            _gameState.Spider = new Spider(firstLocalPlayerIndex == 0);
            _gameState.Frog = new Frog(firstLocalPlayerIndex == 1);
        }
        _homeTexture.CenterOrigin();
        _homeShadowTexture.CenterOrigin();
        _menu = new Menu(Reset, ToggleCredits, Resume);
    }

    // Not thread protected. Not guaranteed to be state alligned. 
    public bool IsLocalCursorPlayer() => _gameState.Spider.IsLocalPlayer;

    private void Reset(object sender, EventArgs e)
    {
        _stateMutex.WaitOne();
        try
        {
            GameState gamestate = new GameState();
            gamestate.FrameNumber = _gameState.FrameNumber;
            gamestate.PreviousInputs = _gameState.PreviousInputs;
            gamestate.Frog = new Frog(_gameState.Frog.IsLocalPlayer);
            gamestate.Spider = new Spider(_gameState.Spider.IsLocalPlayer);
            _gameState = gamestate;
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    private void ToggleCredits(object sender, EventArgs e)
    {
        _stateMutex.WaitOne();
        try
        {
            _gameState.ShowCredits = !_gameState.ShowCredits;
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    private void Resume(object sender, EventArgs e)
    {
        _stateMutex.WaitOne();
        try
        {
            _gameState.Paused = false;
            _gameState.ShowCredits = false;
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void Update(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        _stateMutex.WaitOne();
        try
        {
            if (!IsLocalCursorPlayer())
            {
                _remoteCursorPosition.X = inputs[0].Input.CursorPosition.X;
                _remoteCursorPosition.Y = inputs[0].Input.CursorPosition.Y;
            }
            
            _gameState.FrameNumber++;

            if (_gameState.EndedOnFrame != 0)
            {
                _menu.Update(inputs[0].Input, ref _gameState);
                var endFrame = _gameState.FrameNumber - _gameState.EndedOnFrame;
                if (endFrame > 60) return;
                _endTextPosition.Y = MathHelper.CatmullRom(
                    -100, Game1.ResolutionHeight / 2f, Game1.ResolutionHeight * 0.3f, 0, endFrame / 60f
                );
                _endTextScale = MathHelper.CatmullRom(
                    0, 1, 5, 1, endFrame / 60f
                );
                return;
            }

            for (var i = 0; i < _gameState.PreviousInputs.Length; i++)
            {
                if (_gameState.PreviousInputs[i].DigitalInputs.HasFlag(DigitalInputs.Escape) &&
                    !inputs[i].Input.DigitalInputs.HasFlag(DigitalInputs.Escape))
                {
                    if (_gameState.ShowCredits && _gameState.Paused)
                        _gameState.ShowCredits = false;
                    else
                        _gameState.Paused = !_gameState.Paused;
                    break;
                }
            }

            if (_gameState.Paused)
            {
                _menu.Update(inputs[0].Input, ref _gameState);
                return;
            }

            _gameState.RoundFrame++;
            _gameState.Spider.Update(inputs[0].Input);
            _gameState.Frog.Update(inputs[1].Input);
            for (var i = 0; i < _gameState.Clusters.Length; i++)
            {
                if(_gameState.Clusters[i].Active)
                    _gameState.Clusters[i].Update();
            }
            for (var i = 0; i < _gameState.FrogEnemies.Length; i++)
            {
                if (!_gameState.FrogEnemies[i].Active) continue;
                _gameState.FrogEnemies[i].Update(_gameState.Frog.Position);
                if (!_gameState.FrogEnemies[i].Dead) continue;
                if (_paddedBounds.Contains(_gameState.FrogEnemies[i].Position)) continue;
                _gameState.FrogEnemies[i].Active = false;
                _gameState.NextFrogEnemyIndex = i;
            }
            SpawnManager.Update(ref _gameState);
            CollisionManager.Update(ref _gameState);
        }
        finally 
        {
            _gameState.PreviousInputs[0] = inputs[0];
            _gameState.PreviousInputs[1] = inputs[1];
            _stateMutex.ReleaseMutex();
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _stateMutex.WaitOne();
        try
        {
            spriteBatch.Draw(_grassTexture, _grassArea, _grassArea, Color.White);
            
            _homeTexture.Draw(spriteBatch, Spider.Home);
            
            if (_gameState.RoundLost)
            {
                var loseText = "FAILURE";
                Vector2 loseTextSize = _font.MeasureString(loseText);
                spriteBatch.DrawString(
                    _font, 
                    loseText, 
                    _endTextPosition, 
                    Color.Red, 
                    0, 
                    loseTextSize / 2, 
                    _endTextScale, 
                    SpriteEffects.None, 
                    0
                );
            }
            
            if (_gameState.RoundWon)
            {
                var winText = "SUCCESS";
                Vector2 winTextSize = _font.MeasureString(winText);
                spriteBatch.DrawString(
                    _font, 
                    winText, 
                    _endTextPosition, 
                    Color.Green, 
                    0, 
                    winTextSize / 2,
                    _endTextScale, 
                    SpriteEffects.None, 
                    0
                );
            }
            
            if (_gameState.Paused || _gameState.EndedOnFrame != 0)
                gameTime.ElapsedGameTime = new TimeSpan(0);
            
            for (var i = 0; i < _gameState.Clusters.Length; i++)
            {
                if(_gameState.Clusters[i].Active)
                    _gameState.Clusters[i].Draw(spriteBatch, gameTime);
            }
            _gameState.Frog.Draw(spriteBatch, gameTime);
            for (var i = 0; i < _gameState.FrogEnemies.Length; i++)
            {
                if(_gameState.FrogEnemies[i].Active)
                    _gameState.FrogEnemies[i].Draw(spriteBatch, gameTime);
            }
            _gameState.Spider.Draw(spriteBatch, gameTime);
            _homeShadowTexture.Draw(spriteBatch, Spider.Home);
            var scoreText = _gameState.EggCount.ToString();
            Vector2 textSize = _font.MeasureString(scoreText);
            spriteBatch.DrawString(_font, scoreText, Spider.Home - textSize / 2, Color.White);
            
            if (_gameState.Paused || _gameState.EndedOnFrame != 0)
                _menu.Draw(spriteBatch, _gameState);
            
            for (int i = 0; i < _shrubAreas.Length; i++)
                spriteBatch.Draw(_shrubsTexture, _shrubAreas[i], _shrubAreas[i], Color.White);
            if (!IsLocalCursorPlayer())
                spriteBatch.Draw(_cursorTexture, _remoteCursorPosition, null, QuarterTransparent);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void LoadState(ref readonly BinaryBufferReader reader)
    {
        _stateMutex.WaitOne();
        try
        {
            reader.Read(ref _gameState);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void SaveState(ref readonly BinaryBufferWriter writer)
    {
        _stateMutex.WaitOne();
        try
        {
            writer.Write(in _gameState);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }
}
