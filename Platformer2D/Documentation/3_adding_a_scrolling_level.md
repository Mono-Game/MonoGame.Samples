# Platformer: Adding a Scrolling Level

> 8 minutes to read

Extends the base Platformer starter kit code by adding a scrolling level. Specifically, it provides parallax scrolling.

One of the more impressive features of platformer games in the 80s was the scrolling level. This topic details the process for adding this feature to the Platformer game. You'll implement multiple scrolling backgrounds, with the back layer scrolling the slowest, and the front layer scrolling the fastest. This is called parallax scrolling. It provides an illusion of depth in the game.

> Tip
> 
> It is highly recommended that you are already familiar with the structure and features of the Platformer starter kit. This extension involves modifications to several files in the Platformer starter kit. For more information on the Platformer starter kit, see Starter Kit: Platformer.

This extension modifies two areas of the Platformer starter kit, and it adds a new class. It is recommended that you use the base Platformer starter kit solution as the starting point for your modifications.

Adding parallax scrolling involves the following major steps:

* Modifying PlatformerGame (game.cs) to call SpriteBatch.Begin and SpriteBatch.End at a different time.

* Modifying Level.cs to use a new object type for the background textures instead of Texture2D. In addition, a camera is implemented and used to draw a portion of the background instead of the entire background.

* Adding a new class called Layer that replaces the usage of Texture2D for background textures.

## Modifying the PlatformerGame Class (game.cs)

The only modification for this class involves modifying the Draw method. You'll move the SpriteBatch.Begin and SpriteBatch.End calls to the DrawHud method. This allows the Level.Draw method to set up its own batch for drawing the scrolling backgrounds.

In PlatformerGame.Draw, remove the spriteBatch.Begin(); and spriteBatch.Begin(); lines of code.

In the PlatformerGame.DrawHud method, add the following line before any existing code:

```csharp
spriteBatch.Begin();
```

At the end of the same method, add:

```csharp
spriteBatch.End();
```

The DrawHud method now implements a single batch.

That completes the modifications for PlatformerGame.cs. The next step adds support for a new class, called Layer to the Level class.

## Modifying the Level Class

The main point of these modifications is to support the usage of a new kind of texture class (Layer) that enables parallax scrolling. Each of the three background textures will use the new class; therefore, the surrounding code also needs to accommodate the new class.

First, change the type used by the layers array from Texture2D[] to Layer[]. This is the new background texture class, added later.

```csharp
private Layer[] layers;
```

Now, look for a variable block, commented as "Level game state," and add a new variable called cameraPosition.

```csharp
layers = new Texture2D[3];
    for (int i = 0; i < layers.Length; ++i)
    {
      // Choose a random segment if each background layer for level variety.
      int segmentIndex = random.Next(3);
      layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
    }
```

with the following:

```csharp
layers = new Layer[3];
    layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
    layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
    layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);
```

The new code initializes the array with three new Layer objects. Each of these objects loads a different texture, and has a different scrolling speed (the third parameter of the Layer constructor).

> Note
> 
> Platformer assumes that scrolling speed values have a range between 0 and 1. A value of 0 means no scrolling and 1 means scrolling at the same pace as the level tiles.

It is now time to modify the drawing code for the level. Locate the Level.Draw method, and replace it with the following method declaration:

```csharp
public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      spriteBatch.Begin();
      for (int i = 0; i <= EntityLayer; ++i)
        layers[i].Draw(spriteBatch, cameraPosition);
      spriteBatch.End();

      ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
      Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, transformMatrix: cameraTransform);

      DrawTiles(spriteBatch);

      foreach (Gem gem in gems)
        gem.Draw(gameTime, spriteBatch);

      Player.Draw(gameTime, spriteBatch);

      foreach (Enemy enemy in enemies)
        enemy.Draw(gameTime, spriteBatch);

      spriteBatch.End();

      spriteBatch.Begin();
      for (int i = EntityLayer + 1; i < layers.Length; ++i)
        layers[i].Draw(spriteBatch, cameraPosition);
      spriteBatch.End();
    }
```

Now, let's go over what just changed. Initially, the first sprite batch draws all three background layers. Then, instead of moving a camera throughout the world, you'll move the world backwards such that the camera is always at the origin. This greatly simplifies the drawing logic because you can now call a specialized SpriteBatch.Begin overload that uses a transform matrix (calculated earlier in the method).

You'll recognize the next chunk of code because it is unchanged from the original implementation. It draws the level elements: tiles, gems, enemies, and the player character. The last batch does nothing in the base implementation of Platformer. It is left in for drawing foreground textures. For example, if a foreground texture (such as trees or bushes) was drawn, it would obscure the player character when he "walked" behind the texture.

Because the scrolling extension draws tiles off-screen, you should be aware that this could impact the frame rate. To avoid any slowdown you'll need to implement a simple culling feature that limits the amount of tiles drawn to only those on the screen at the time. This reduces the drawing load, speeding up the game.

At the beginning of the DrawTiles method, add the following code:

```csharp
// Calculate the visible range of tiles.
    int left = (int)Math.Floor(cameraPosition / Tile.Width);
    int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
    right = Math.Min(right, Width - 1);
```

Below this modification, modify the for line of the inner loop (the one that loops '**x**' from 0 to Width) to match the following:

```csharp
for (int x = left; x <= right; ++x)
```

Now, only visible tiles are drawn but note that other items, such as gems and enemies, are still drawn even when off screen. The culling of non-tiles is another excellent place for extending Platformer!

The last modification in this file adds the new ScrollCamera method. This method calculates how much background is scrolled when the player reaches the screen's edge. When the begin scrolling is platform-dependent. Because mobile screens are the narrowest, it looks the farthest ahead. Desktop platforms don't look ahead as much. This factor is used to calculate the edges of the screen and how far to scroll when the player reaches that edge. Scrolling continues until either end of the level is reached. At that point, the camera position is clamped.

Add the following code, after the Draw method:

```csharp
private void ScrollCamera(Viewport viewport)
    {
#if MOBILE
      const float ViewMargin = 0.45f;
#else
      const float ViewMargin = 0.35f;
#endif

      // Calculate the edges of the screen.
      float marginWidth = viewport.Width * ViewMargin;
      float marginLeft = cameraPosition + marginWidth;
      float marginRight = cameraPosition + viewport.Width - marginWidth;

      // Calculate how far to scroll when the player is near the edges of the screen.
      float cameraMovement = 0.0f;
      if (Player.Position.X < marginLeft)
        cameraMovement = Player.Position.X - marginLeft;
      else if (Player.Position.X > marginRight)
        cameraMovement = Player.Position.X - marginRight;

      // Update the camera position, but prevent scrolling off the ends of the level.
      float maxCameraPosition = Tile.Width * Width - viewport.Width;
      cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
    }
```

The final step adds the new Layer class.

## Implementing the Layer Class

Because the backgrounds will be scrolling during gameplay, you'll need something more specialized than a Texture2D class to draw these textures. The background textures provided are divided into three segments that tile seamlessly into one scrolling background.

Using the Add Class dialog, add a new C# class, called Layer, to the PlatformerWindows solution. At the top of the file, add some useful XNA Framework references:

```csharp
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content;
```

Depending on how your project is setup, make sure the namespace is set the same as the rest of the classes in the project as follows:

```csharp
    namespace Platformer2D
```

In this new class, add the following properties:

```csharp
    class Layer
    {
        public Texture2D[] Textures { get; private set; }
        public float ScrollRate { get; private set; }
    }
```

These properties store the background texture of the layer and its scroll speed.

Now add the constructor:

```csharp
public Layer(ContentManager content, string basePath, float scrollRate)
    {
      // Assumes each layer only has 3 segments.
      Textures = new Texture2D[3];
      for (int i = 0; i < 3; ++i)
        Textures[i] = content.Load<Texture2D>(basePath + "_" + i);

      ScrollRate = scrollRate;
    }
```

This constructor accepts a content manager, a base path to the background asset, and the scroll speed of the background layer. Note that each layer has only three segments.

It loads each segment of the background in the Textures array, and then sets the scroll speed.

The final method to add is the Draw method. Add this code after the constructor method:

```csharp
public void Draw(SpriteBatch spriteBatch, float cameraPosition)
    {
      // Assume each segment is the same width.
      int segmentWidth = Textures[0].Width;

      // Calculate which segments to draw and how much to offset them.
      float x = cameraPosition * ScrollRate;
      int leftSegment = (int)Math.Floor(x / segmentWidth);
      int rightSegment = leftSegment + 1;
      x = (x / segmentWidth - leftSegment) * -segmentWidth;

      spriteBatch.Draw(Textures[leftSegment % Textures.Length], new Vector2(x, 0.0f), Color.White);
      spriteBatch.Draw(Textures[rightSegment % Textures.Length], new Vector2(x + segmentWidth, 0.0f), Color.White);
    }
```

This method first calculates which of the background segments to draw, and then draws them offset by the previously calculated amount. It is assumed that two segments are enough to cover any screen.

## Modifying the Level Structure File

At this point, the parallax scrolling extension is completely coded. However, if you recompile and run the game, you will see no difference. Like the Power-Up Gem extension, you'll need to modify an existing map to enable the scrolling. However, it won't be as easy as modifying an extra character or two. You would need to come up with a lot of new content, past the default edge of the level, to fully illustrate the scrolling. However, since the format of the level structure file is text-based, you can just use the following text block as a demonstration case.

```csharp
..............................................................................
..............................................................................
...........................G..............................................X...
..........................###.....................................############
......................G.......................................................
.....................###................G.GDG.............G.G.G...............
.................G....................#########..........#######..............
................###...........................................................
............G...................G.G...............G.G.........................
...........###.................#####.............#####........................
.......G......................................................................
......###...............................GDG.G.............G.G.G.G.G.G.G.G.G.G.
......................................#########.........##.G.G.G.G.G.G.G.G.G..
.1........................................................GCG.G.G.GCG.G.G.GCG.
####################......................................####################
```

Copy this text and then open the 0.txt file (located in the HighResolutionContent content project). Select all text in the level structure file, and then paste the new text in. Save the file and do the same for the equivalent low-resolution level structure file. Once you have made changes to both maps, recompile and run the game. You can now run far to the right in the first level, and the three background layers scroll at different speeds. Pretty cool effect, eh?!
