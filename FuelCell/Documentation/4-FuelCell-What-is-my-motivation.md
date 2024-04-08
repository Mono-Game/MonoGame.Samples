# FuelCell: What's My Motivation

## In this article

- [Overview](#overview)
- [GamePad States](#gamepad-states)
- [Updating the Fuel Carrier](#updating-the-fuel-carrier)
- [Calling the FuelCarrier Update](#calling-the-fuelcarrier-update)
- [See Also](#see-also)

Discusses the implementation of user control for the avatar (known as the fuel carrier).

## Overview

The goal for this part, is the implementation of a control schema for the fuel carrier. With this schema, the player can use either the keyboard (A or D for left/right rotation and W and S for forward/backward movement) or a standard gamepad (using the left thumbstick for rotation and forward/backward movement). In addition, we will check the `_maxRange` data member against the current position and only allow movement that keeps the player on the playing field. This prevents the player from driving off the playing field.

## GamePad States

In order to control the `FuelCarrier`, you need input from a `keyboard` or `gamepad`. There are two approaches to getting input from the player:

- Single-state
- Two-state.

### Single-state approach

In a single-state approach, input is determined from a single snapshot of the controller, taken during the execution of the `Update` method. Any actions that need to be taken by the game are initiated, thus enabling game play to move forward. This approach is demonstrated by [How To: Detect Whether a Controller Button Is Pressed]().

However, when discrete input is required, the single-state approach does not solve the problem. For instance, suppose a game is designed to fire one bullet for every press of a key or button. If you use the single-state approach, multiple bullets are fired per key or button press. This happens because human reflexes are slower than the standard update cycle of the game. Even a very quick player is going to have a key or button pressed for at least a few update cycles (unless the game uses a fixed-step approach). In order to fire a single bullet every time a key or button is pressed, you must look for a current state where a specific key or button is released and a previous state where that same key or button was pressed. This condition is only satisfied at the instant when the key or button transitions from pressed to released.

### Two-state approach

By using the two-state approach, we track both the current state of the controller and the previous state. This allows the game to determine single occurrences of player action, such as a key or button press. With this approach, it does not matter how slow (or fast) the player's reflexes are. The input is only valid at that moment when the previous and current input states match the criteria determined by the input code. This approach is demonstrated by [How To: Detect Whether a Controller Button Has Been Pressed]() This Frame and [How To: Detect Whether a Key Is Pressed]().

---

It turns out that if you do any amount of MonoGame game development, you often run into this controller update issue. The two-state approach is the best solution in most situations, and it is easy to implement, and the same code can also fulfill the needs of a single-state approach. In terms of `keyboard` and `gamepad` controls, FuelCell uses a two-state approach, but with a twist.

> [!NOTE]
> The support for two-state input checking is in the code but FuelCell needs only check for current key and thumbstick states. Therefore, there is no need to check for a discrete event.

This code is an example of "future-proofing." If you were to add the capability to blow up a barrier with a missile, you would already have the necessary code to use the two-state approach.

Implementation begins in the `FuelCellGame.cs` file. Add the following code after the declaration of the `barriers` data member that we added in the last chapter:

```csharp
// States to store input values
private KeyboardState lastKeyboardState = new KeyboardState();
private KeyboardState currentKeyboardState = new KeyboardState();
private GamePadState lastGamePadState = new GamePadState();
private GamePadState currentGamePadState = new GamePadState();
```

Now, in the existing `Update` method, initialize the variables at the beginning of the method, after the call to `Exit` the game:

```csharp
// Update input from sources, Keyboard and GamePad
lastKeyboardState = currentKeyboardState;
currentKeyboardState = Keyboard.GetState();
lastGamePadState = currentGamePadState;
currentGamePadState = GamePad.GetState(PlayerIndex.One);
```

You can now use either set of variables to determine exactly when a key or button is pressed or just use `currentGamePadState` if you only need the current position of the thumbstick. Your next step is to use this input to update the `position` and `direction` of the fuel carrier.

## Updating the Fuel Carrier

Currently, the `FuelCarrier` class does not have a method for updating its position and heading, you will add that with the following `Update` method, after the constructor (but before the `Draw` method).

> [!TIP]
> It is usually good practice to keep the MonoGame methods, Constructor, Initialize, Update and Draw, in that order, so it is easier to find then while building/debugging.

In the `FuelCarrier.cs` file, add the new `Update` method:

```csharp
public void Update(GamePadState gamepadState, KeyboardState keyboardState, Barrier[] barriers)
{
    Vector3 futurePosition = Position;
    float turnAmount = 0;

    if (keyboardState.IsKeyDown(Keys.A))
    {
        turnAmount = 1;
    }
    else if(keyboardState.IsKeyDown(Keys.D))
    {
        turnAmount = -1;
    }
    else if(gamepadState.ThumbSticks.Left.X != 0)
    {
        turnAmount = -gamepadState.ThumbSticks.Left.X;
    }
    ForwardDirection += turnAmount * GameConstants.TurnSpeed;
    Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);

    Vector3 movement = Vector3.Zero;
    if (keyboardState.IsKeyDown(Keys.W))
    {
        movement.Z = 1;
    }
    else if(keyboardState.IsKeyDown(Keys.S))
    {
        movement.Z = -1;
    }
    else if (gamepadState.ThumbSticks.Left.Y != 0)
    {
        movement.Z = gamepadState.ThumbSticks.Left.Y;
    }

    Vector3 speed = Vector3.Transform(movement, orientationMatrix);
    speed *= GameConstants.Velocity;
    futurePosition = Position + speed;

    if (ValidateMovement(futurePosition, barriers))
    {
        Position = futurePosition;
    }
}
```

This method is important, so let us go through it in detail.

- First, the `futurePosition` is set to the current `Position` of the FuelCarrier and the turn amount is set to 0.
    We know where the Player currently is and movement is reset to the players current forward direction.
- The amount of vehicle rotation is calculated, based on the current state of the A and D keys or the X-axis of the left thumbstick, a rotation matrix (`orientationMatrix`) is created.
- This `orientationMatrix` is then used to calculate the rotation of the player by the proper amount in world coordinates.
- The second chunk of code calculates how far the ship moved in either a forward or backward direction, based on the current state of the W and S keys or the Y-axis of the same thumbstick.
- This distance is then transformed using the `orientationMatrix` created earlier, and the result is multiplied by a constant velocity (GameConstants.Velocity).
- The end result is then added to the current position which results in a new `projected` future position.
- Finally, this result is passed to a private method called `ValidateMovement`. If it is valid (player is not moving outside the play area for now), the `Position` member is updated and control returns to the main `FuelCellGame.Update` method.

> [!TIP] Programming Tip
> You might be wondering why you could not just test the current position for validity instead of calculating a future position and testing that. The answer is that if you only tested the current position, it is already too late to prevent illegal movement. When the test is made, the vehicle has already had its position updated. Suppose that new position is illegal (past the edge of a boundary case). This causes further tests to fail, resulting in the vehicle "sticking" to the current position. Obviously, this is not optimal behavior for a player-controlled vehicle.
>
> It is better to check the future position (where the player is potentially going to be) and prevent any illegal moves (by refusing to move the player based in the input). This check allows the player to attempt something different (like backing up), and not get stuck because the player currently is in a legal position.

The remaining piece is the implementation of the `ValidateMovement` method. Add the following method after the `FuelCarrier.Update` method:

```csharp
private bool ValidateMovement(Vector3 futurePosition, Barrier[] barriers)
{
    //Do not allow off-terrain driving
    if ((Math.Abs(futurePosition.X) > MaxRange) || (Math.Abs(futurePosition.Z) > MaxRange))
        return false;

    return true;
}
```

Currently, this method only checks for the edge of the playing field. Any attempt to drive off the playing field will be ignored.  We will check for `Barriers` in this method [later on](6-FuelCell-Ships-passing-in-the-night.md).

## Calling the FuelCarrier Update

Returning to the `FuelCellGame.cs` class, change the following code in the `Update` function, replace:

```csharp
float rotation = 0.0f;
Vector3 position = Vector3.Zero;
gameCamera.Update(rotation, position, graphics.GraphicsDevice.Viewport.AspectRatio);
```

with the following:

```csharp
fuelCarrier.Update(currentGamePadState, currentKeyboardState, barriers);
gameCamera.Update(fuelCarrier.ForwardDirection, fuelCarrier.Position, graphics.GraphicsDevice.Viewport.AspectRatio);
```

The only difference is a call to the new update method, `FuelCarrier.Update`. This method takes as input the current keyboard and gamepad states and the barriers array. The method determines if the ship can move based on the current input and barrier locations. Ignore the barriers parameter for now; it is used in a later step. At this point, the function only prevents the vehicle from driving off the playing field. If an attempt is made to go over the playing field edge, the input is ignored.

Also in the `Update` method, replace the code that checks for a `Back` button press with the following:

```csharp
// Allows the game to exit
if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
{
    this.Exit();
}
```

The game now checks for both keyboard (`ESC` key) and gamepad input (`Back` button) when the player wishes to exit the game using he new state data we are polling in `Update`.

After the usual drill of rebuilding the project and running it, drive the fuel carrier freely around the map. Test out the boundary code by driving to the edge of the playing field. You will notice that you stop moving until you choose a new direction. The control schema implementation was pretty easy but, coming up, the game really starts to come together... which requires a lot of coding!

![Final Result](Images/04-01-final.gif)

## See Also

### Next

- [FuelCell: What's My Line?](5-FuelCell-What-is-my-line.md)

### Conceptual

- [FuelCell: Introduction](../README.md)

### Tasks

- [How To: Detect Whether a Controller Button Is Pressed]()
- [How To: Detect Whether a Controller Button Has Been Pressed This Frame]()
- [How To: Detect Whether a Key Is Pressed]()
