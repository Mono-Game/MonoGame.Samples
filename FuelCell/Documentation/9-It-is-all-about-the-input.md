# FuelCell: It is all about the Input

## In this article

- [Who said that](#who-said-that)
- [Setting up the Input Class](#setting-up-the-input-class)
- [Getting the raw input](#getting-the-raw-input)
- [Have I been pressed](#have-i-been-pressed)
- [Processing what we need the player to do](#processing-what-we-need-the-player-to-do)
- [Can I get a little service here](#can-i-get-a-little-service-here)
- [Fixing the interface](#fixing-the-interface)
- [Updating the Game's input implementation](#updating-the-games-input-implementation)
- [Updating FuelCarrier input](#updating-fuelcarrier-input)
- [Extra credit](#extra-credit)
- [See Also](#see-also)

Discusses refactoring input to a more sustainable and maintainable solution that is reusable across projects.

## Who said that

As games evolve, the many ways that the user can interact with a game expand, even more so the more "types" of control you support, from Keyboard, Mouse, and GamePads.  Handling these **Within** your code can become very messy, for example, in our `FuelCellGame.cs` we currently have the following code in our `Update` method:

```csharp
if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
{
    this.Exit();
}
```

Which listens for the player hitting the `Esc` key or the `Back` button then it exits the game.  But then what if you want to add another key/button, or another type of input.  Then you need to revisit this code to update it, which on it is own is fine, however, what if the code is for something the player does in the game, such as Jump? or more complicated like several key/button combinations for the ultimate fighting move?  This style of coding can become more and more complex.

To take this further, how do you then add player customization of controls, changing what each button does in the game, in which case this kind of pattern simply would not work.

The solution is "Abstraction", the method of defining a "known" term, such as `Jump`, setting out that Jump "does" and then providing a way to check if the player is "Jumping" or not, then separate it into a class that does all the work.  Finally, in your game you simply ask "Am I jumping" rather than all the input checking code, changing the above call to:

```csharp
if (inputState.IsExitRequested())
{
    this.Exit();
}
```

If you then change how you exit, you do not need to change all the places where exit can be called from.

## Setting up the Input Class

> [!TIP]
> The [GameStateManagement](https://github.com/SimonDarksideJ/GameStateManagementSample) sample has a well-defined [`InputState`](https://github.com/SimonDarksideJ/GameStateManagementSample/blob/3.8/GameStateManagement/InputState.cs) class you can use and extend from.

To begin with, we will setup a new class that is going to manage all the "raw" input that our game will receive data from, e.g. GamePads, Keyboards, and touchscreens.  If you want to add more input later, then this is where you would begin.

```csharp
namespace FuelCell
{
    /// <summary>
    /// The interface definition for Input
    /// </summary>
    public interface IInputState
    {
    }

    /// <summary>
    /// The current implementation for the InputState based on the IInputState interface
    /// </summary>
    public class InputState : IInputState
    {
    }
}
```

Now the first thing you will notice is the [Interface](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface) declaration, interfaces are a very powerful way of declaring exactly what your class can do and what operations it can perform.  What interfaces also allow, is you to implement very different concrete classes (implementations) of functionality that can be "swapped in" without changing the rest of the code in your project.

The reason we are using it here is so that we can define, upfront, several methods that our "Input" will provide, and then a specific class that will perform those operations.  If later on we want to change HOW we implement that input without changing our game, we can just swap in another class with minimal changes (one line in fact).

> [!TIP]
> [Interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface) are a powerful tool in any game development and I would recommend reading up on them if you are unfamiliar with how they are used.  The most common use is building different "attack" classes for different characters,  one "Attack" action can have completely different abilities depending on the weapon the character is wielding, and using interfaces means you can swap out which class is DOING the attack based on which weapon the character currently has assigned.

You will see more about how the interface is used as we progress in this chapter.

## Getting the raw input

The very next thing we need to do in our Input class is to actually get the raw input data, what we have already implemented in the main `FuelCellGame.cs` class, we will move this here for now and clean it up later.

1. Add the following using statements to the top of the `InputState.cs` class, in readiness for the rest of the code:

    ```csharp
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Input.Touch;
    using System;
    using System.Collections.Generic;
    ```

2. Next, add the following properties inside the `InputState.cs` class definition, each property is documented for clarity:

    ```csharp
        public class InputState : IInputState
        {
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
        }
    ```

3. With the states in place, we need to initialize them when the game starts (when the `InputState` class is created), add the following right after the properties shown above in the `InputState.cs` class:

    ```csharp
        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardState = new KeyboardState();
            CurrentGamePadStates = new GamePadState[MaxGamePadInputs];
            for (int i = 0; i < MaxGamePadInputs; i++)
            {
                GamePad.GetCapabilities(i);
            }

            LastKeyboardState = new KeyboardState();
            LastGamePadStates = new GamePadState[MaxGamePadInputs];

            GamePadWasConnected = new bool[MaxGamePadInputs];
        }
    ```

    This simply initializes each property with a new state and in the case of the gamepads, gets the setup of each gamepad and records whether a device was connected or not.

4. To finish up the state management of Input, we need an `Update` method that will be called during the game's `Update` call to refresh input each frame of the game, so add the following `Update` after the constructor we just added:

    ```csharp
        /// <summary>
        /// Reads the latest state of the keyboard, gamepads and touch.
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
    ```

5. Finally, as our `Update` call will need to be called by our `FuelCellGame.cs` class, we will need to declare it in our `IInputState` interface, only methods expressed in our interface can be "seen" by any code using the interface.

    > [!TIP]
    > If you know the concrete class that is using am interface, then yes, you can still see all the `Public` methods and properties it exposes.  But any "common" code using the interface properly will not.  It is like knowing a car has doors (the interface is for all cars) but you would not know if a BMW had an ejector seat door on the roof unless you knew it was that type of BMW.  Learn more about interfaces and it will all make sense :D

    To state whether a method or property is exposed by the interface, we just need to declare it in the interface, as follows, update the `interface` definition as per below:

    ```csharp
        public interface IInputState
        {
            /// <summary>
            /// Update the input state
            /// </summary>
            void Update();
        }
    ```

Now we know that ANY class that implements the `IInputState` interface is guaranteed to have an `Update` method.

We now have the framework for our input, so now let us get more interesting and start defining some way the input can be consumed by our game, rather than individually checking what is going on.

## Have I been pressed

We will start with the most simple expression of input, checking if a `Key` (on the keyboard) or `Button` (on a gamepad) has been pressed:

> [!NOTE]
> You may note that all the following methods are marked `private`, we will come back to that later, as the game only needs to know if the player has done 'something' but not necessarily HOW they did it, e.g. we want to know if they hit the "fire" button, not that it required either the "Enter" key or the "A" button.

```csharp
    /// <summary>
    /// Helper for checking if a key was pressed during this update.
    /// </summary>
    private bool IsKeyPressed(Keys key)
    {
        return CurrentKeyboardState.IsKeyDown(key);
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
            if ( i > MaxGamePadInputs)
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
```

As you can see, checking the gamepad for a button being pressed is a little more complicated than simply checking what key is pressed, because you potentially have multiple controllers connected, and if you are unsure which player is pressing an input, or if you want the first player who hit a button, you need to check them all.

The end result is that these methods will return `true` if the specified button or key is pressed.  However, what if you want to know if they have "just pressed" it or if they have "released" the key/button.  Then we need slightly different checks, for the keyboard it is simply a matter of checking the current state and comparing it to the previous state:

```csharp
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
```

For buttons, we have a similar pattern, but as with the button press, we still need to check all the gamepads that are connected:

```csharp
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

                if ( i > MaxGamePadInputs)
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

                if ( i > MaxGamePadInputs)
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

                if ( i > MaxGamePadInputs)
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
```

That handles keys and buttons, but what about other inputs, controllers also have triggers (which return a `float` value) and thumbsticks, which return two values in a `vector2` type (signifying up/down and left/right directions).  So let us handle those too with the following methods:

> [!NOTE]
> We do NOT however, need to check triggers or thumbsticks previous states as we do with keys and buttons, as they are a constant value and only report their CURRENT state.  If you wanted to track their value over time you would need to handle that in your game, rather than through the input.
>
> We also only check the requested player and NOT all players, as it would not really make sense with a stream of data.

```csharp
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
```

> [!NOTE]
> For extensibility, we define two methods for each input test, one that returns the `PlayerIndex` that the input came from and one that does not, this is so we can keep reusing this class in future games.
> The thumbstick and trigger methods are also public, because games just need their current values and do not need them processed.

We are almost there, now that we have the raw data we can start defining "Actions" that our game will need to preform and then we can evaluate exactly what conditions need to be met for that to happen.

## Processing what we need the player to do

As mentioned in the previous section, we know what the player pressed, now we just need to figure out if it is something we needed/wanted them to do.  Rather than (as is done currently in `FuelCellGame.cs`) checking each and every input directly in our game (which makes it a nightmare later if you want to remap those controls), we abstract this into "Actions" that the game is checking for.

As an example to make it clearer, when we want the player to start the game, we check all the inputs in our game code that we have allocated for that, e.g. (from `FuelCellGame.cs` )

```csharp
    if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
    {
        this.Exit();
    }
```

But all we really want to know in the game is if the player wants to Quit, rather than all this input checking.  So instead of this, we just need to define a method in our `InputState` Class to determine if the user wants to exit, e.g.:

```csharp
    public bool PlayerExit(PlayerIndex? controllingPlayer)
    {
        return IsNewKeyPress(Keys.Escape) ||
                IsNewButtonPress(Buttons.Back, controllingPlayer, out _);
    }
```

Then in our game, we can simplify our code to be:

```csharp
    if (inputState.PlayerExit())
    {
        this.Exit();
    }
```

This becomes exponentially cleaner in the more places you need to check input.  We define these states as "Actions" which can be anything the game needs the player to perform and separates this from what needs to happen for the "Action" to happen.

In the `FuelCell` game, the actions that the game needs are as follows:

- StartGame - When the game needs to start or restart.
- PlayerExit - When the player wishes to close the game.
- PlayerTurn - When the game needs to rotate the `FuelCarrier` left or right on the gameboard.
- PlayerMove - When the game needs to move the `FuelCarrier` forward or backward on the gameboard.

So let us define these methods with their corresponding checks on the `InputState`:

```csharp
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
```

With the building blocks in place, we have one final piece of the puzzle to solve before updating our game with the new input code.

## Can I get a little service here

Within any project, there are those elements that need to be accessed from anywhere within the project which creates a small but unique problem, how to share the running state of an instantiated class with other classes.

- `Referencing`
  The most common way is to pass around a reference to the class with its current state as part of a method call, which in most cases is fine.  The `Update` call does this by passing the current game time `Update(GameTime gameTime)`.  However, this gets problematic the more data you want to send and if you then need to pass that data to more and more nested classes, there is a possibility of data corruption or hitting some [Garbage Collection](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/) issues when cleaning up that data.  Ideally, only [value types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types) should be passed as references but this is more of an unwritten rule than law.
- `Static Classes`
  Another common method is to use [Static](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/static) types, these have a singular instance throughout your project (there can only be one of any type or data), this solves the access problem but can run into [race conditions](https://learn.microsoft.com/en-us/dotnet/standard/security/security-and-race-conditions) if many parts of the project end up updating the data, because there is only one instance, any update to the class updates ALL access to the class.
- `Injection`
  The last method is injection, where types are safely passed around a project, sometimes automatically, or using a container or registry that maintains the "master copy" of any data being shared.

This last method is provided by the MonoGame framework using the [Game "Services"](https://monogame.net/api/Microsoft.Xna.Framework.Game.html#Microsoft_Xna_Framework_Game_Services) collection (not to be confused with [Game Components](https://monogame.net/api/Microsoft.Xna.Framework.Game.html#Microsoft_Xna_Framework_Game_Components)) which allows you to record instances of classes for use by other parts of the project.

To this end, we will do a minor update to the constructor of the `InputState` to register the class automatically with the `Game.Services` registry when it is created, as well as a property to record which Game class is registering the class.

Add the following property at the top of the class:

```csharp
    private readonly Game _game;
```

Then update the `InputState` class constructor as follows:

```csharp
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
```

The changes we made are as follows:

- Altered the constructor signature to require a `Game` class instance, required for registering the class to the services registry.
- A null check to ensure you do not accidentally pass a bad game reference (always code as if it is going to break, then it will not)
- Cache the `Game` reference to the property in the class.
- Towards the end of the constructor, once everything is setup and good, we check if an existing `InputState` class has been registered already (just in case we accidentally register it twice :D )
- And finally, once all the checks are done, we finally register this instance of the `InputState` class with the `Game.Services` registry.

A lot of boilerplate to ensure the class is registered safely and securely, as well as checking there is only one (there can only be one).  Now whenever we need input we can just ask for the `InputState` class and start receiving data.  All will become more clear as we update our input in the game.

> [!TIP]
> This is all optional of course, you can continue to do input as it was done previously in the project, but you will start having growing pains as your game project gets larger and larger.

## Fixing the interface

Now our `InputState` class is defined, we just need to expose what methods are going to be available through our `IInputState` interface, which we will do by replacing the existing `IInputState`definition at the top of the `InputState.cs` class:

```csharp
    public interface IInputState
    {
        /// <summary>
        /// Update the input state
        /// </summary>
        void Update();

        // Thumbstick data
        Vector2 GetThumbStickLeft(PlayerIndex? controllingPlayer);
        Vector2 GetThumbStickLeft(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);
        Vector2 GetThumbStickRight(PlayerIndex? controllingPlayer);
        Vector2 GetThumbStickRight(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);
        float GetTriggerLeft(PlayerIndex? controllingPlayer);
        float GetTriggerLeft(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);
        float GetTriggerRight(PlayerIndex? controllingPlayer);
        float GetTriggerRight(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex);

        // Game Actions
        Vector3 GetPlayerMove(PlayerIndex? controllingPlayer);
        float GetPlayerTurn(PlayerIndex? controllingPlayer);
        bool PlayerExit(PlayerIndex? controllingPlayer);
        bool StartGame(PlayerIndex? controllingPlayer);
    }
```

This simply declares all the public methods we defined in the `InputState` class as accessible by any class implementing the `IInputState` interface.  This protects our Input implementation so if we want to swap it out with another implementation.

## Updating the Game's input implementation

Quickly wrapping up and cleaning up the `FuelCellGame.cs` class, we need to add and use our new `InputState` handler and also remove all the old input "stuff" we no longer need because the Input service is providing it all (makes things a lot cleaner).

Starting off, we need to add a new property for the `InputState` class, however, we are recording it using its interface, this will then allow us to swap out the concrete implementation if we wish without touching any other input code in our project.

Add the following after the `aspectRatio` property:

```csharp
    private IInputState inputState;
```

With the property in place, we just need to initialize the `InputState`, add the following at the end of the `FuelCellGame` class constructor:

```csharp
    inputState = new InputState(this);
```

Lastly, we need to ensure that the `inputState` class instance is updated every frame by adding the following at the top of the `Update` method in the `FuelCellGame.cs` class.

```csharp
    protected override void Update(GameTime gameTime)
    {
        // Update the InputState class.
        inputState.Update();
```

Now that you are up and running, we can start swapping out the input code, quick fire, and replace the "Input" calls in the `Update` method of the `FuelCellGame.cs` class.

Replace:

```csharp
    if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
```

With:

```csharp
    if (inputState.PlayerExit(PlayerIndex.One))
```

Next, replace the following: (Twice!!)

```csharp
    if ((lastKeyboardState.IsKeyDown(Keys.Enter) && (currentKeyboardState.IsKeyUp(Keys.Enter))) ||
        currentGamePadState.Buttons.Start == ButtonState.Pressed)
```

With:

```csharp
    if (inputState.StartGame(PlayerIndex.One))
```

Finally, Replace:

```csharp
fuelCarrier.Update(currentGamePadState, currentKeyboardState, barriers);
```

With:

```csharp
fuelCarrier.Update(inputState, barriers);
```

As a final clean-up step for the `FuelCellGame.cs` class, let us remove the old "GetState" code first, deleting following code blocks:

1. Remove the State properties at the top of the class

    ```csharp
        // States to store input values
        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        GamePadState currentGamePadState = new GamePadState();
    ```

2. Remove the state update code at the end of the `Update` method:

    ```csharp
        // Update input from sources, Keyboard and GamePad
        lastKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();
        lastGamePadState = currentGamePadState;
        currentGamePadState = GamePad.GetState(PlayerIndex.One);
    ```

## Updating FuelCarrier input

Switching over to the `FuelCarrier.cs` class, we need to update the player input in that class.

Replace the current `Update` method signature with the following:

```csharp
    public void Update(IInputState inputState, Barrier[] barriers)
```

And then swap out the following code blocks:

```csharp
    float turnAmount = 0;

    if (keyboardState.IsKeyDown(Keys.A))
    {
        turnAmount = 1;
    }
    else if (keyboardState.IsKeyDown(Keys.D))
    {
        turnAmount = -1;
    }
    else if (gamepadState.ThumbSticks.Left.X != 0)
    {
        turnAmount = -gamepadState.ThumbSticks.Left.X;
    }
    ForwardDirection += turnAmount * GameConstants.TurnSpeed;
```

With

```csharp
    ForwardDirection += inputState.GetPlayerTurn(PlayerIndex.One) * GameConstants.TurnSpeed;
```

And also:

```csharp
    Vector3 movement = Vector3.Zero;
    if (keyboardState.IsKeyDown(Keys.W))
    {
        movement.Z = 1;
    }
    else if (keyboardState.IsKeyDown(Keys.S))
    {
        movement.Z = -1;
    }
    else if (gamepadState.ThumbSticks.Left.Y != 0)
    {
        movement.Z = gamepadState.ThumbSticks.Left.Y;
    }

    Vector3 speed = Vector3.Transform(movement, orientationMatrix);
```

With:

```csharp
    Vector3 speed = Vector3.Transform(inputState.GetPlayerMove(PlayerIndex.One), orientationMatrix);
```

And we are done.  All the input checks, states and validations are now in their own class and adding new inputs only has to be done in a single place.  Run and build the game and although nothing has really changed, the whole project is more maintainable and extendable.

## Extra credit

As our input implementation is registered as a service, you could alternatively instead of passing the `InputState` instance to other classes from the `Game` class, you could initialize other classes, such as the `GameObject` class (and its dependents) with the `Game` class instance, then in your class code simply call:

```csharp
    var inputState = game..Services.GetService<IInputState>();
```

This gives you the instance of the `InputState` class that was initialized when the game started.  When we only have one service it might not make much sense, however, if you have several services registered for Leaderboards, networking, and input, then a single reference to Game gives you access to all services on demand.

## That's a Wrap!

After the (now) traditional rebuild and play session, you can now enjoy the introductory splash screen, a challenging game of retrieving the fuel cells, and (hopefully) a congratulatory winning screen with some kicking music and audio effects.

At this point, the FuelCell game is complete. Even though you wrote a large amount of code, the possibilities for expansion are endless. For example:

- Multiplayer - invite a competitor and whoever collects the most fuel cells wins!
- Powerups - Add another object, or two, that gives the player a boost of speed for 5 seconds or a "ghost" ability that ignores those pesky barriers!
- Advanced lighting and effects - improve the default lighting with a glow effect or motion blur.

If you need more ideas for expanding FuelCell or want to interact with a like-minded community of fellow game developers, check out the [MonoGame Framework](https://monogame.net) site. It is an excellent community-driven site that has active forums and helpful people who are designing new and exciting games.

Good luck in your game development future and, above all, have fun!

## See Also

### Conceptual

-[FuelCell: Introduction](../README.md)
