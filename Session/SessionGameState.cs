using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;
using System.Threading;
using Microsoft.Xna.Framework;
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
    Button _retryButton;

    static readonly Vector2 _retryButtonPosition = new Vector2(Game1.ResolutionWidth/2f, Game1.ResolutionHeight*0.8f);
    static readonly SpriteFont _font = Game1.GameContent.Load<SpriteFont>("PublicPixel");
    static readonly Texture2D _pauseOverlay = Game1.GameContent.Load<Texture2D>("pause-overlay");
    static readonly Rectangle _paddedBounds = new Rectangle(
        Game1.ScreenBounds.X - 100,
        Game1.ScreenBounds.Y - 100,
        Game1.ScreenBounds.Width + 200,
        Game1.ScreenBounds.Height + 200
    );

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

        _retryButton = new Button(_retryButtonPosition, "Retry", Reset);
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

    public void Update(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        _stateMutex.WaitOne();
        try
        {
            _gameState.FrameNumber++;

            if (_gameState.EndedOnFrame != 0)
            {
                _retryButton.Update(inputs[0].Input, _gameState.PreviousInputs[0], ref _gameState.RetryButtonState);
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
                    _gameState.Paused = !_gameState.Paused;
                    break;
                }
            }

            if (_gameState.Paused)
            {
                _retryButton.Update(inputs[0].Input, _gameState.PreviousInputs[0], ref _gameState.RetryButtonState);
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
                _gameState.FrogEnemies[i].Update(_gameState.Frog.Position, _gameState.Frog.Respawning);
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
            var scoreText = _gameState.EggCount.ToString();
            Vector2 textSize = _font.MeasureString(scoreText);
            spriteBatch.DrawString(_font, scoreText, Spider.Home - textSize / 2, Color.White);
            
            if (_gameState.Paused || _gameState.EndedOnFrame != 0)
            {
                spriteBatch.Draw(_pauseOverlay, Game1.ScreenBounds, Color.White);
                _retryButton.Draw(spriteBatch, _gameState.RetryButtonState);
            }
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
