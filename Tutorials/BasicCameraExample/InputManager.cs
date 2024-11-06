using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public static class InputManager
{
    private static KeyboardState currentKeyState;
    private static KeyboardState previousKeyState;
    private static GamePadState currentGamePadState;
    private static GamePadState previousGamePadState;
    private static MouseState mouseState;

    public static void Update()
    {
        previousKeyState = currentKeyState;
        currentKeyState = Keyboard.GetState();
        previousGamePadState = currentGamePadState;
        currentGamePadState = GamePad.GetState(0);
        mouseState = Mouse.GetState();
    }

    public static bool IsKeyDown(Keys key)
    {
        return currentKeyState.IsKeyDown(key);
    }

    public static bool IsKeyPressed(Keys key)
    {
        return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
    }

    public static bool IsKeyReleased(Keys key)
    {
        return !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key);
    }

    public static float MouseX => mouseState.X;
    public static float MouseY => mouseState.Y;

    public static Vector2 MousePosition => new Vector2(mouseState.X, mouseState.Y);

    public static void SetMousePosition(int x, int y)
    {
        Mouse.SetPosition(x, y);
    }

    public static bool IsButtonDown(Buttons button)
    {
        return currentGamePadState.IsButtonDown(button);
    }

    public static bool IsButtonPressed(Buttons button)
    {
        return currentGamePadState.IsButtonDown(button) && !previousGamePadState.IsButtonDown(button);
    }

    public static bool IsButtonReleased(Buttons button)
    {
        return !currentGamePadState.IsButtonDown(button) && previousGamePadState.IsButtonDown(button);
    }

    
    public static float RightTriggerValue => currentGamePadState.Triggers.Right;
    public static Vector2 LeftThumbStick => currentGamePadState.ThumbSticks.Left;
    public static float LeftTriggerValue => currentGamePadState.Triggers.Left;
    public static Vector2 RightThumbStick => currentGamePadState.ThumbSticks.Right;

}
