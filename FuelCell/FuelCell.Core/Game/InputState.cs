using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace FuelCell
{
    /// <summary>
    /// The interface definition for Input
    /// </summary>
    public interface IInputState
    {
        /// <summary>
        /// Update the input state
        /// </summary>
        void Update();

        /// <summary>
        /// Get the state of the left thumbstick for a specific player
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>The <see cref="Vector2"/> directional state of the thumbstick</returns>
        Vector2 GetThumbStickLeft(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Get the state of the left thumbstick for a specific player and output the player index
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <param name="playerIndex">The output of the <see cref="PlayerIndex"/> instance</param>
        /// <returns>The <see cref="Vector2"/> directional state of the thumbstick</returns>
        Vector2 GetThumbStickLeft(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

        /// <summary>
        /// Get the state of the right thumbstick for a specific player
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>The <see cref="Vector2"/> directional state of the thumbstick</returns>
        Vector2 GetThumbStickRight(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Get the state of the right thumbstick for a specific player and output the player index
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <param name="playerIndex">The output of the <see cref="PlayerIndex"/> instance</param>
        /// <returns>The <see cref="Vector2"/> directional state of the thumbstick</returns>
        Vector2 GetThumbStickRight(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

        /// <summary>
        /// Get the state of the left trigger for a specific player
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>The <see cref="float"/> state of the trigger</returns>
        float GetTriggerLeft(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Get the state of the left trigger for a specific player and output the player index
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <param name="playerIndex">The output of the <see cref="PlayerIndex"/> instance</param>
        /// <returns>The <see cref="float"/> state of the trigger</returns>
        float GetTriggerLeft(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

        /// <summary>
        /// Get the state of the right trigger for a specific player
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>The <see cref="float"/> state of the trigger</returns>
        float GetTriggerRight(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Get the state of the right trigger for a specific player and output the player index
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <param name="playerIndex">The output of the <see cref="PlayerIndex"/> instance</param>
        /// <returns>The <see cref="float"/> state of the trigger</returns>
        float GetTriggerRight(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

        /// <summary>
        /// Get input from the player for movement forward and backwards in the game.
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>Returns a <see cref="Vector3"/> positional update for the player</returns>
        Vector3 GetPlayerMove(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Get input from the player for movement left and right in the game.
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>Returns the new turn ratio for the player.</returns>
        float GetPlayerTurn(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Respond to the player wanting to exit the game.
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>Returns true if the player has requested to exit the game.</returns>
        bool PlayerExit(PlayerIndex? controllingPlayer);

        /// <summary>
        /// Respond to the player wanting to start the game.
        /// </summary>
        /// <param name="controllingPlayer">The <see cref="PlayerIndex"/> of the player to request data for.</param>
        /// <returns>Returns true if the player has requested to start the game.</returns>
        bool StartGame(PlayerIndex? controllingPlayer);
    }

    /// <summary>
    /// The current implementation for the InputState based on the IInputState interface
    /// </summary>
    public class InputState : IInputState
    {
        private readonly Game _game;

        // A constant value to limit the maximum number of gamepads that can be connected at a time, 
        // `4` is usually sufficient but consoles can support more if you want to.
        public const int MaxGamePadInputs = 4;

        // The CURRENT state of input, values as they are read from the device.
        public KeyboardState CurrentKeyboardState;
        public readonly GamePadState[] CurrentGamePadStates;

        // The PREVIOUS state of the input, so we can compare if an input was just activated, or recently released.
        public KeyboardState LastKeyboardState;
        public readonly GamePadState[] LastGamePadStates;

        // Which gamepads are connected and active
        public readonly bool[] GamePadWasConnected;

        // Simple boolean to determine if ANY gamepads are connected.
        public bool GamePadsAvailable = false;

        // If we are on mobile, what is the state of any touchscreen input
        public TouchCollection TouchState;

        // If we are on mobile, what gestures have been detected.
        public readonly List<GestureSample> Gestures = new List<GestureSample>();

        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException("game", "Game cannot be null.");
            }

            _game = game;

            CurrentKeyboardState = new KeyboardState();
            CurrentGamePadStates = new GamePadState[MaxGamePadInputs];
            for (int i = 0; i < MaxGamePadInputs; i++)
            {
                GamePad.GetCapabilities(i);
            }

            LastKeyboardState = new KeyboardState();
            LastGamePadStates = new GamePadState[MaxGamePadInputs];

            GamePadWasConnected = new bool[MaxGamePadInputs];

            if (_game.Services.GetService(typeof(IInputState)) != null)
            {
                throw new ArgumentException("An Input State class is already registered.");
            }

            // Once the InputState class has been initialized, then register the current instance with the Game Services registry.
            _game.Services.AddService(typeof(IInputState), this);
        }

        /// <summary>
        /// Reads the latest state of the keyboard,  gamepads and touch.
        /// </summary>
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
            GamePadsAvailable = false;

            for (int i = 0; i < MaxGamePadInputs; i++)
            {
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                {
                    GamePadsAvailable = true;
                    GamePadWasConnected[i] = true;
                }
            }

            TouchState = TouchPanel.GetState();

            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }
        }

        /// <summary>
        /// Helper for checking if a key was pressed during this update.
        /// </summary>
        private bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Helper for checking if a key was newly pressed during this update.
        /// Key is pressed this frame but was not previously pressed.
        /// </summary>
        private bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) &&
                    LastKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        /// Helper for checking if a key was held down during this update,
        /// Key is pressed this frame and was already pressed.
        /// </summary>
        private bool IsKeyHeld(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) &&
                    LastKeyboardState.IsKeyDown(key));
        }

        /// <summary>
        /// Helper for checking if a key was released during this update,
        /// Key is not pressed this frame but was previously pressed.
        /// </summary>
        private bool IsKeyReleased(Keys key)
        {
            return (CurrentKeyboardState.IsKeyUp(key) &&
                    LastKeyboardState.IsKeyDown(key));
        }

        /// <summary>
        /// Helper for checking if a button was pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        private bool IsButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                // This should not happen, but if you request input from a player that is not connected, this will safely return false.
                // It could not have been pressed because the player is not connected.
                if (i > MaxGamePadInputs)
                {
                    return false;
                }

                return CurrentGamePadStates[i].IsButtonDown(button);
            }
            else
            {
                // Accept input from any player.
                return (IsButtonPressed(button, PlayerIndex.One, out playerIndex) ||
                        IsButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
                        IsButtonPressed(button, PlayerIndex.Three, out playerIndex) ||
                        IsButtonPressed(button, PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        private bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                if (i > MaxGamePadInputs)
                {
                    return false;
                }

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        private bool IsButtonHeld(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                if (i > MaxGamePadInputs)
                {
                    return false;
                }

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonDown(button));
            }
            else
            {
                // Accept input from any player.
                return (IsButtonHeld(button, PlayerIndex.One, out playerIndex) ||
                        IsButtonHeld(button, PlayerIndex.Two, out playerIndex) ||
                        IsButtonHeld(button, PlayerIndex.Three, out playerIndex) ||
                        IsButtonHeld(button, PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        private bool IsButtonReleased(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                if (i > MaxGamePadInputs)
                {
                    return false;
                }

                return (CurrentGamePadStates[i].IsButtonUp(button) &&
                        LastGamePadStates[i].IsButtonDown(button));
            }
            else
            {
                // Accept input from any player.
                return (IsButtonReleased(button, PlayerIndex.One, out playerIndex) ||
                        IsButtonReleased(button, PlayerIndex.Two, out playerIndex) ||
                        IsButtonReleased(button, PlayerIndex.Three, out playerIndex) ||
                        IsButtonReleased(button, PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Helper for checking the state of the left thumbstick during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public Vector2 GetThumbStickLeft(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            return GetThumbStickLeft(controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Helper for checking the state of the left thumbstick during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public Vector2 GetThumbStickLeft(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentGamePadStates[i].ThumbSticks.Left;
            }
            else
            {
                for (int i = 0; i < MaxGamePadInputs; i++)
                {
                    if (CurrentGamePadStates[i].IsConnected)
                    {
                        playerIndex = (PlayerIndex)i;
                        return CurrentGamePadStates[i].ThumbSticks.Left;
                    }
                }
                playerIndex = PlayerIndex.One;
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Helper for checking the state of the left thumbstick during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public Vector2 GetThumbStickRight(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            return GetThumbStickRight(controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Helper for checking the state of the right thumbstick during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public Vector2 GetThumbStickRight(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentGamePadStates[i].ThumbSticks.Right;
            }
            else
            {
                for (int i = 0; i < MaxGamePadInputs; i++)
                {
                    if (CurrentGamePadStates[i].IsConnected)
                    {
                        playerIndex = (PlayerIndex)i;
                        return CurrentGamePadStates[i].ThumbSticks.Right;
                    }
                }
                playerIndex = PlayerIndex.One;
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Helper for checking the state of the left trigger during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public float GetTriggerLeft(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            return GetTriggerLeft(controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Helper for checking the state of the left trigger during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public float GetTriggerLeft(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentGamePadStates[i].Triggers.Left;
            }
            else
            {
                for (int i = 0; i < MaxGamePadInputs; i++)
                {
                    if (CurrentGamePadStates[i].IsConnected)
                    {
                        playerIndex = (PlayerIndex)i;
                        return CurrentGamePadStates[i].Triggers.Left;
                    }
                }
                playerIndex = PlayerIndex.One;
                return 0;
            }
        }

        /// <summary>
        /// Helper for checking the state of the right trigger during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public float GetTriggerRight(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            return GetTriggerRight(controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Helper for checking the state of the right trigger during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// If no connected gamepad found, it will return Vector2.Zero
        /// </summary>
        public float GetTriggerRight(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentGamePadStates[i].Triggers.Right;
            }
            else
            {
                for (int i = 0; i < MaxGamePadInputs; i++)
                {
                    if (CurrentGamePadStates[i].IsConnected)
                    {
                        playerIndex = (PlayerIndex)i;
                        return CurrentGamePadStates[i].Triggers.Right;
                    }
                }
                playerIndex = PlayerIndex.One;
                return 0;
            }
        }

        public bool PlayerExit(PlayerIndex? controllingPlayer)
        {
            return IsNewKeyPress(Keys.Escape) ||
                    IsNewButtonPress(Buttons.Back, controllingPlayer, out _);
        }

        public bool StartGame(PlayerIndex? controllingPlayer)
        {
            return IsNewKeyPress(Keys.Enter) ||
                    IsNewButtonPress(Buttons.Start, controllingPlayer, out _);
        }

        public float GetPlayerTurn(PlayerIndex? controllingPlayer)
        {
            float turnAmount = 0;
            Vector2 thumbstickValue = GetThumbStickLeft(controllingPlayer);

            if (IsKeyHeld(Keys.A))
            {
                turnAmount = 1;
            }
            else if (IsKeyHeld(Keys.D))
            {
                turnAmount = -1;
            }
            else if (thumbstickValue.X != 0)
            {
                turnAmount = -thumbstickValue.X;
            }
            return turnAmount;
        }

        public Vector3 GetPlayerMove(PlayerIndex? controllingPlayer)
        {
            Vector3 movement = Vector3.Zero;
            Vector2 thumbstickValue = GetThumbStickLeft(controllingPlayer);

            if (IsKeyHeld(Keys.W))
            {
                movement.Z = 1;
            }
            else if (IsKeyHeld(Keys.S))
            {
                movement.Z = -1;
            }
            else if (thumbstickValue.Y != 0)
            {
                movement.Z = thumbstickValue.Y;
            }
            return movement;
        }
    }
}