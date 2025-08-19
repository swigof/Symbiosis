using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using Symbiosis.Session;

namespace Symbiosis.UI;

public class Menu
{
    Button _retryButton;
    Button _creditsButton;
    Button _resumeButton;
    Button _backButton;
    String _creditsText;

    static readonly Vector2 _resumeButtonPosition = new(Game1.ResolutionWidth / 2f, Game1.ResolutionHeight * 0.7f);
    static readonly Vector2 _retryButtonPosition = new(Game1.ResolutionWidth / 2f, Game1.ResolutionHeight * 0.8f);
    static readonly Vector2 _creditsButtonPosition = new(Game1.ResolutionWidth / 2f, Game1.ResolutionHeight * 0.9f);
    static readonly Vector2 _creditsPosition = new(48, 48);
    static readonly Texture2D _pauseOverlay = Game1.GameContent.Load<Texture2D>("pause-overlay");
    static readonly Color _quarterTransparent = new Color(255, 255, 255, 255 / 4);
    static readonly SpriteFont _font = Game1.GameContent.Load<SpriteFont>("PublicPixel");

    public Menu(EventHandler<EventArgs> reset, EventHandler<EventArgs> credits, EventHandler<EventArgs> resume)
    {
        _resumeButton = new Button(_resumeButtonPosition, "Resume", resume);
        _retryButton = new Button(_retryButtonPosition, "Retry", reset);
        _creditsButton = new Button(_creditsButtonPosition, "Credits", credits);
        _backButton = new Button(_creditsButtonPosition, "Back", credits);
        var stream = TitleContainer.OpenStream("Content/Assets/credits.txt");
        using (StreamReader sr = new StreamReader(stream))
        {
            _creditsText = sr.ReadToEnd();
        }
    }

    public void Update(PlayerInputs mousePlayerInputs, ref GameState gameState)
    {
        if (gameState.ShowCredits)
        {
            _backButton.Update(mousePlayerInputs, gameState.PreviousInputs[0], ref gameState.BackButtonState);
        }
        else
        {
            _resumeButton.Update(mousePlayerInputs, gameState.PreviousInputs[0], ref gameState.ResumeButtonState);
            _retryButton.Update(mousePlayerInputs, gameState.PreviousInputs[0], ref gameState.RetryButtonState);
            _creditsButton.Update(mousePlayerInputs, gameState.PreviousInputs[0], ref gameState.CreditsButtonState);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameState gameState)
    {
        if (gameState.ShowCredits)
        {
            spriteBatch.Draw(_pauseOverlay, Game1.ScreenBounds, Color.White);
            _backButton.Draw(spriteBatch, gameState.BackButtonState);
            spriteBatch.DrawString(
                _font, 
                _creditsText, 
                _creditsPosition, 
                Color.White,
                0,
                Vector2.Zero,
                0.5f,
                SpriteEffects.None,
                0
            );
        }
        else
        {
            spriteBatch.Draw(_pauseOverlay, Game1.ScreenBounds, _quarterTransparent);
            if (gameState.EndedOnFrame == 0)
                _resumeButton.Draw(spriteBatch, gameState.ResumeButtonState);
            _retryButton.Draw(spriteBatch, gameState.RetryButtonState);
            _creditsButton.Draw(spriteBatch, gameState.CreditsButtonState);
        }
    }
}