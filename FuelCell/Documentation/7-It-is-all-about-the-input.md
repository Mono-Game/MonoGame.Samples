# FuelCell: It is all about the Input

## In this article

- [Out of My Way!](#out-of-my-way)

- [See Also](#see-also)

Discusses collision detection in a 3D game and demonstrates basic collision checking between the fuel carrier vehicle and the other game objects on the playing field.

## Who said that

As games evolve, the many ways that the user can interact with a game expand, even more so the more "types" of control you support, from Keyboard, Mouse and GamePads.  Handling these **Within** your code can become very messy, for example, in our `FuelCellGame.cs` we currently have the following code in our `Update` method:

```csharp
if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
{
    this.Exit();
}
```

Which listens for the player hitting the `Esc` key or the `Back` button, then it exits the game.  But then what if you want to add another key/button, or another type of input.  THen you need to revisit this code to update it, which on it's own is fine, however, what if the code is for something the player does in the game, such as Jump? or more complicated like a combinations for the ultimate fighting move?  This style of coding can become more and more complex.

To take this further, how do you then add player customization of controls, changing what each button does in the game, in which case this kind of pattern simply would not work.

The solution is "Abstraction", the method of defining a "known" term, such as `Jump`, setting out that Jump "does" and then providing a way to check if the player is "Jumping" or not from a separate class that does all the work.  Then in your game you simply ask "Am I jumping" rather than all the input checking code, changing the above call to:

```csharp
if (InputState.IsExitRequested())
{
    this.Exit();
}
```

Then if you change how you exit, you then do not need to change all the places where exit can be called from.

## Setting up the Input State

> [!TIP]
> The [GameStateManagement](https://github.com/SimonDarksideJ/GameStateManagementSample) sample has a well defined [`InputState`](https://github.com/SimonDarksideJ/GameStateManagementSample/blob/3.8/GameStateManagement/InputState.cs) class you can use and extend from.

