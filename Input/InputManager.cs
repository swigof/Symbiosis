using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;

namespace Symbiosis.Input;

public class InputManager
{
    public static InputManager Instance { get { return lazy.Value; } }
    private static readonly Lazy<InputManager> lazy = new Lazy<InputManager>(() => new InputManager());
    private InputManager() { }

    private PlayerInputs _localInput = new PlayerInputs();
    private Mutex _localInputMutex = new Mutex();
    
    public PlayerInputs GetLocalInput()
    {
        _localInputMutex.WaitOne();
        try
        {
            return _localInput;
        }
        finally
        {
            _localInputMutex.ReleaseMutex();
        }
    } 
    
    // Needs to be called on the main thread to see inputs
    public void UpdateLocalInput(bool isActive)
    {
        _localInputMutex.WaitOne();
        try
        {
            _localInput.DigitalInputs = DigitalInputs.None;

            if (isActive)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
                    _localInput.DigitalInputs |= DigitalInputs.Up;
                if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
                    _localInput.DigitalInputs |= DigitalInputs.Down;
                if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                    _localInput.DigitalInputs |= DigitalInputs.Left;
                if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                    _localInput.DigitalInputs |= DigitalInputs.Right;
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    _localInput.DigitalInputs |= DigitalInputs.Action;
                var mouseState = Mouse.GetState();
                var viewport = Game1.Graphics.GraphicsDevice.Viewport;
                if (viewport.Bounds.Contains(mouseState.Position))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                        _localInput.DigitalInputs |= DigitalInputs.Click;
                    _localInput.CursorPosition.X = (int)((mouseState.X - viewport.X) * (1 / Game1.ScreenScale));
                    _localInput.CursorPosition.Y = (int)((mouseState.Y - viewport.Y) * (1 / Game1.ScreenScale));
                }
            }
        }
        finally
        {
            _localInputMutex.ReleaseMutex();
        }
    }
}
