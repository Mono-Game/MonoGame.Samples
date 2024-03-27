# Platformer: Adding a Power-Up

> 9 minutes to read

Extends the base Platformer starter kit code by adding a power-up gem and player character effect.

> Tip
> 
> It is highly recommended that you are already familiar with the structure and features of the Platformer starter kit. This extension involves modifications to several files in the Platformer starter kit. For more information on the Platformer starter kit, see Starter Kit: Platformer.

This extension modifies five areas of the Platformer starter kit. It is recommended that you use the base Platformer starter kit solution as the starting point for your modifications.

Adding a power-up gem involves the following major steps:

* Modifying Gem.cs to support a power-up attribute and draw the power-up gem in a special way.

* Modifying AnimationPlayer.cs to draw a special effect on the player when invincible.

* Modifying Player.cs to support and monitor a power-up state, drawing a special effect when the character is powered-up.

* Modifying Enemy.cs to support being killed by a powered-up player character and to draw a Die animation.

* Modifying Level.cs to recognize and load a power-up gem from a level structure file. The level code must also notify other game objects that a power-up gem has been collected.

## Modifying the Gem Class

Modification of the Gem class begins by changing some existing properties and adding a IsPowerUp property. Modify the existing code for the PointValue and Color properties to match the following:

```csharp
    public readonly int PointValue = 30;
    public bool IsPowerUp { get; private set; }
    public readonly Color Color;
```

This modification also adds the new property, IsPowerUp. Modify the existing Gem constructor to accept a new isPowerUp parameter. After the modification, it should match the following:

```csharp
    public Gem(Level level, Vector2 position, bool isPowerUp)
    {
      ...
    }
```

In the existing constructor, after the code setting the level and position of the gem object (***before LoadContent()***), add the following:

```csharp
    IsPowerUp = isPowerUp;
    if (IsPowerUp)
    {
      PointValue = 100;
      Color = Color.Red;
    }
    else
    {
      PointValue = 30;
      Color = Color.Yellow;
    }
```

This code sets the IsPowerUp property of the gem, and then raises the point value if it is a power-up.

The last modification to the Gem class is to call the PowerUp method (a method added later) of the proper player character. Add the following code after any existing code in the OnCollected method:

```csharp
    if (IsPowerUp)
      collectedBy.PowerUp();
```

> You will get an error at this point as we have not yet updated the **Player** class, this will be resolved shortly and can be ignored.

## Modifying the AnimationPlayer Class

Your modifications in this class are small. You'll add code using the new color being passed in to tint the player character sprite, and you'll add a new Draw method.

Modify the existing Draw method declaration to accept an additional parameter called color. This indicates the current color to use as a tint when drawing the invincible player character. After the modification, it should look like the following:

```csharp
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color color)
    {
      ...
    }
```

Now that we have a tint color being passed in, let's use it in our Draw call. Staying within this method, modify the final call to Draw to match the following:

```csharp
    spriteBatch.Draw(Animation.Texture, position, source, color, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
```

Now add a new Draw method, after the previous one, to be called when the power-up effect is not needed:

```csharp
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
    {
      Draw(gameTime, spriteBatch, position, spriteEffects, Color.White);
    }
```

That was the last of the code for the strobing tint effect. Let's move on to the Player class, found in the Player.cs file, and add that PowerUp method, among other modifications.

## Modifying the Player Class

Modification of the Player class begins by adding support for a power-up state. After the IsAlive property, add the following code:

```csharp
    // Powerup state
    private const float MaxPowerUpTime = 6.0f;
    private float powerUpTime;
    public bool IsPoweredUp
    {
      get { return powerUpTime > 0.0f; }
    }
    private readonly Color[] poweredUpColors = {
                               Color.Red,
                               Color.Blue,
                               Color.Orange,
                               Color.Yellow,
                                               };
    private SoundEffect powerUpSound;
```

This code adds some properties to monitor the power-up time, an IsPoweredUp method, a Color array that stores the colors used to produce the special tinting of the player character, and a sound effect that plays while the character is powered up.

While you're here, check the PowerUp sound effect has been added to the content project.

1. Open the content project from the Platformer2D.Core solution.

2. Check the PowerUp.wav file is located in the Sounds folder.

3. Select the sound effect, and verify the Content Processor is set to SoundEffect (in the Properties window).

Next, Load the new sound effect by adding the following line to the LoadContent method:

```csharp
    powerUpSound = Level.Content.Load<SoundEffect>("Sounds/PowerUp");
```

Modify the Reset method to set the powerUpTime property to 0 after the rest of the property resets.

```csharp
    powerUpTime = 0;
```

In the Update method, you'll need to check to see if the player character is powered up and, if so, update the time remaining. Add the following code after the ApplyPhysics call:

```csharp
    if (IsPoweredUp)
    {
        powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
    }
```

We'll use the Draw method to indicate that the player is temporarily invincible. A strobing tint is applied while the player is invincible. Add the following code after the code that flips the player character sprite:

```csharp
    // Calculate a tint color based on power up state.
    Color color;
    if (IsPoweredUp)
    {
      float t = ((float)gameTime.TotalGameTime.TotalSeconds + powerUpTime / MaxPowerUpTime) * 20.0f;
      int colorIndex = (int)t % poweredUpColors.Length;
      color = poweredUpColors[colorIndex];
    }
    else
    {
      color = Color.White;
    }
```

If the player is invincible, a color is selected from the array you added earlier and used to tint the character sprite when it is later drawn. The final modification to this function is to add the tint color to the call to AnimationPlayer.Draw. Modify the call to match the following:

```csharp
    sprite.Draw(gameTime, spriteBatch, Position, flip, color);
```

The final bit of code is the new PowerUp method. Add this new code after the Draw method:

```csharp
    public void PowerUp()
    {
      powerUpTime = MaxPowerUpTime;
      powerUpSound.Play();
    }
```

> The error you had previously when editing the Gem class should now be gone as we have added the **PowerUp** method to the Player class.

This is a simple bit of code that sets the powerup time to a predefined value, which is 3 seconds in this case.

You're done with Player.cs. It is now time to modify his fiendish enemies.

## Modifying the Enemy Class

Modification of the Enemy class begins by adding and loading the player death animation. The Die animation (Die.png) is already included in the sample Content project for you.

As we will be adding audio handling to the Enemy class, begin by adding a new using to the top of the Enemy class file with the rest of the using statements.

```csharp
    using Microsoft.Xna.Framework.Audio;
```

You'll we need to load the sound effect to play when an enemy dies, so we will add a new variable for storing that sound effect. Add the following code after the animation variables:

```csharp
    // Sounds
    private SoundEffect killedSound;
```

Add another property after the existing BoundingRectangle variable:

```csharp
    public bool IsAlive { get; private set; }
```

Now that an enemy can be killed by the player character, you need to track whether enemies are alive or dead. In the Enemy constructor (right below the last modification), initialize IsAlive to true.

In the LoadContent method, load the sound effect:

```csharp
    // Load sounds.
    killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");
```

Now that enemies can be dead, optimize the update code a bit, and only update living enemies. In the **Update** method, add the following code after calculating the elapsed time:

```csharp
    if (!IsAlive)
      return;
```

As we now require our Animation to pass a Color when drawing, we also need to update our Enemy's **Draw** call send that color, so update the **sprite.Draw** call to the following in the **Draw** Method:

```csharp
    sprite.Draw(gameTime, spriteBatch, Position, flip, Color.White);
```

Let's add the OnKilled method to play a sound when the enemy dies, place this after the Draw method:

```csharp
public void OnKilled(Player killedBy)
    {
      IsAlive = false;
      killedSound.Play();
    }
```

That completes the modification of Enemy.cs.

All right, we are coming up on the home stretch now. There is one final file to modify before you can compile and check out the power-up gem in action.

## Modifying the Level Class

The modifications for the Level class are pretty extensive. You'll need to add support for a new gem type, and also support for killing an enemy if the invincible character collides with it.

First, let's add support for the new gem type. In the Tile LoadTile(char tileType, int x, int y) method, modify the code for the 'G' case to match the following:

```csharp
    // Gem
    case 'G':
      return LoadGemTile(x, y, false);

    // Power-up gem
    case 'P':
      return LoadGemTile(x, y, true);
```

This updates the code for a normal gem, passing false, and adds the ability to read in power-up gems. 

Now, you'll need to update the LoadGemTile method to accept a new parameter. Modify LoadGemTile to accept an additional parameter (of type bool) called isPowerUp.

```csharp
    private Tile LoadGemTile(int x, int y, bool isPowerUp)
```

Make sure that the new parameter is last in the method declaration. In this same function, modify the call to gem.Add to match the following:

```csharp
    gems.Add(new Gem(this, new Vector2(position.X, position.Y), isPowerUp));
```

This modification now calls the recently modified Gem constructor.

If you remember, you earlier changed the implementation of the point value for a gem. Now is a good time to update the OnGemCollected method to use the new PointValue field. In this method, modify the score assignment to match the following:

```csharp
    score += gem.PointValue;
```

Now that you can recognize and load the power-up gem, let's add support for its effect when the player character collects it.

Modify the UpdateEnemies method to first check that the current enemy is alive and, if so, upon collision check for a powered up player. If the player is powered up, you need to "kill" the enemy.

Change this code:

```csharp
if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
    {
      OnPlayerKilled(enemy);
    }
```

to match the following:

```csharp
if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
    {
      if (Player.IsPoweredUp)
      {
        OnEnemyKilled(enemy, Player);
      }
      else
      {
        OnPlayerKilled(enemy);
      }
    }
```

Next, add the implementation of the OnEnemyKilled method, after the UpdateEnemies method:

```csharp
private void OnEnemyKilled(Enemy enemy, Player killedBy)
    {
      enemy.OnKilled(killedBy);
    }
```

This follows the practice of Platformer. It implements an OnX method to announce an important event has occurred.

## Testing the Power-Up Extension

Before you can see the power-up gem in action, you'll need to add one to the second level structure file. Open this file now (Levels/1.txt) and replace any G with a P. Recompile and check out the Invincibility power-up!
