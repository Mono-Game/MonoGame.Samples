# FuelCell: Setting the Scene

## In this article

- [Overview](#overview)
- [Objects in the Game](#objects-in-the-game)
- [The Camera](#the-camera)
- [Game Constants](#game-constants)
- [Getting a Grip](#getting-a-grip)
- [Adding the Ground Content](#adding-the-ground-content)
- [Time to start building the Game](#time-to-start-building-the-game)
- [Opening Your "Eye"](#opening-your-eye)
- [See Also](#see-also)

Discusses the implementation of a playing field for the game and a simple, fixed camera.

## Overview

One of the largest hurdles a game developer faces when moving from 2D to 3D is that third 'D': depth. In the 2D world, game objects (called sprites) have two dimensions and are positioned using literal screen coordinates. There is a concept of depth, but this is used only to determine if a sprite is partially or fully obscured by another.

In a 3D game, what you see on your screen is a projection of a 3D environment onto a 2D surface (that is, your screen). This translation of 3D space into 2D space is accomplished using transformation matrices. Specifically, we refer to these three matrices as world, view, and projection matrices. Transformation is just a fancy word for changing the value of a coordinate by multiplication. Using these matrices, the MonoGame Framework transforms the coordinates of a 3D model to a set of new coordinate values (through rotation, scaling, or translation) used by the projection matrix. In a separate but related step, a view matrix simulates a viewpoint (often called the camera) in the same 3D space as the model; it looks in a certain direction. With these two matrices, a third matrix is brought into the "picture" to perform a final transformation into 2D screen coordinates. This creates a realistic 2D picture of the 3D scene on your computer screen.

Earlier, we mentioned a camera. Even though this is not a real camera, it fulfils the same role in the 3D game. This camera observes the 3D world and renders whatever it sees into a 2D representation. This representation appears on the computer screen. In a game, the camera class usually is implemented as a stand-alone class. It is one of two varieties: a first-person camera (used in this game and first-person shooters) and a third-person camera (often used in RPGs or platform games). First-person cameras are great for games that focus on a single player or are trying to immerse the player in the game world. Third-person cameras are better suited to viewing a large playing field or controlling numerous entities in the game. For this step, you will implement a first-person camera using code from [How To: Make a First-Person Camera]().

We use a first-person camera because the player controls a small vehicle that can move around and collect fuel cells. The difficulty of the game is finding these items before time runs out. It is difficult because the playing field has opaque barriers randomly scattered across it. Since we use a first-person camera, the player must drive around to view previously hidden areas.

In addition to the camera code, you will also use code from [How To: Render a Model]() to display the 3D playing field model, which is a simple two-tone grid floating in space.

## Objects in the Game

3D game development is all about position and the relation to other objects in the local coordinate system (that is, the game world). In addition to position, a 3D object usually has an associated model. Because this is a 3D game, the model has three dimensions. This means it can be viewed from all angles and has volume. In addition to these two properties, the 3D object should have a bounding sphere. The bounding sphere is a theoretical sphere that encapsulates the model volume. It is used for detecting collisions in the game world with other 3D objects. You can ignore this for now, but it becomes critical later in the development process.

A class is the obvious solution for storing and tracking all these variables. However, before we can add this class, you need to first create a new project for the FuelCell game.

- Create a new MonoGame project called FuelCell.
- In this project, create a new class file called `GameObject.cs`.

The `GameObject` class will contain all those properties mentioned earlier and a constructor that sets the various properties to known values. The file containing this new class only has a few references by default (located at the top). To grant easy access to the MonoGame Framework assemblies, you will need to add some MonoGame specific ones. At the top of the file, add the following references:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
```

> [!NOTE]
> [Usings](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using) are a shortcut way of declaring the API of a library upfront so you do not have to use the full path to a type or class, for instance being able to use just the type `Vector2` instead of having to always use the full path of a Vector 2 (In MonoGame) which is `Microsoft.Xna.Framework.Vector2` (not to be confused with the Vector2 type in `System.Numerics`, which is different).

You are ready to modify the default class declaration to better fit your needs. Add or Replace the `GameObject` class declaration with the following:

> [!NOTE]
> If you are using Visual Studio, some of the usings will appear unused at this point, but by the end we will have used them all.

```csharp
namespace FuelCell
{
    public class GameObject
    {
        public Model Model { get; set; }
        public Vector3 Position { get; set; }
        public bool IsActive { get; set; }
        public BoundingSphere BoundingSphere { get; set; }

        public GameObject()
        {
            Model = null;
            Position = Vector3.Zero;
            IsActive = false;
            BoundingSphere = new BoundingSphere();
        }
    }
}
```

This class tracks the position, model, and bounding sphere of an object in the game using [auto-implemented properties](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties). The constructor is simple, and it initializes each property to a reasonable value – either `null` or `Vector3.Zero`.

## The Camera

The `Camera.cs` contains the camera class declaration, which is taken from the [How To: Make a First-Person Camera]() topic. As mentioned earlier, the main purpose behind this developer diary is to demonstrate how you (or any developer) can use various How To articles as stepping stones when developing a MonoGame Framework game. For this first usage, this concept is clearly illustrated by not changing any of the variable names or classes, whenever possible. This may cause a bit of confusion or head-scratching when you come across variable names like `_avatarHeadOffset` and `avatarYaw`, but it serves to tie the source How To more closely to the actual game code. This creates the ability to easily determine where the source code of a How To ends up in a typical game project by searching for the variable name used in the How To.

> [!TIP]
> For example, in this step, some of the property names match the names used in the original sample code: `_avatarHeadOffset` is the camera's distance above the playing field and `_targetOffset` is the offset from the target. In this case, it is a fixed distance in front of the fuel carrier vehicle. These values are used when calculating the camera position from the current position of the fuel carrier vehicle (for example, `position`) in the world coordinate system.

The camera class is similar in structure to the `FuelCellGame` class. It has a set of properties and a method. In this case, it is `Update`. For this game, the camera acts like a rigid chase camera. It follows behind, and slightly above, the actual vehicle and points in the same direction as the vehicle at all times.

All right, enough talk – let us start developing!

Create a new `Camera.cs` class and use the following code:

```csharp
using Microsoft.Xna.Framework;

namespace FuelCell
{
    public class Camera
    {
        public Vector3 AvatarHeadOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public Camera()
        {
            AvatarHeadOffset = new Vector3(0, 7, -15);
            TargetOffset = new Vector3(0, 5, 0);
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
        }

        public void Update(float avatarYaw, Vector3 position, float aspectRatio)
        {
            Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

            Vector3 transformedheadOffset = Vector3.Transform(AvatarHeadOffset, rotationMatrix);
            Vector3 transformedReference = Vector3.Transform(TargetOffset, rotationMatrix);

            Vector3 cameraPosition = position + transformedheadOffset;
            Vector3 cameraTarget = position + transformedReference;

            //Calculate the camera's view and projection matrices based on current values.
            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameConstants.ViewAngle),
                aspectRatio, GameConstants.NearClip, GameConstants.FarClip);
        }
    }
}
```

This is the camera class declaration. The major difference between this declaration and its appearance in the original How To is that the camera's functionality has been internalized into a class. This means that previously global variables that tracked camera position, the transformation matrices, and other properties are now stored within the class. These properties can be divided into two parts: offset variables and transform matrices. The offset variables (`AvatarHeadOffset` and `TargetOffset`) force the camera to a specific position behind and above the vehicle's current position. Hence, the name chase camera.

> [!TIP]
> **_Researching Transformation Matrices_**
>
> The transformation matrices are used to rotate, move, or scale objects in a world coordinate system and then (along with the view matrix) to a perspective 2D coordinate system: your screen. The theory and application of this concept involves a truckload of math. However, you can read more about these concepts in other areas of the MonoGame documentation:
>
> - [Math Overview]()
> - [Step 4 (of How To: How to render a Model using a Basic Effect)]() discusses the usage of all three matrices.
> - [Viewports and Frustums]()

The Update method is where the main math for updating the camera takes place. This function takes the current rotation of the vehicle and creates a transformation matrix, which in turn is used to transform the camera's offset values. These values are then added to the current vehicle position, creating a point, in the world coordinate system, where the camera "sits." The final step generates the view and perspective matrices, used when rendering the 3D game world view onto your 2D monitor screen.

## Game Constants

Did you notice that some of the method arguments were from a `GameConstants` class? Let us create this class and then I'll explain its purpose.

Add a new class to the project, called `GameConstants.cs` and replace its contents with the following:

```csharp
namespace FuelCell
{
    public class GameConstants
    {
        //camera constants
        public const float NearClip = 1.0f;
        public const float FarClip = 1000.0f;
        public const float ViewAngle = 45.0f;
    }
}
```

You will use this class to gather common game variables into a single location. You can then easily and quickly alter the value of any game constant and have the new value affect the entire game, or at least those areas where the game constant was used. At this point, you have three candidates for game constants: the near and far clipping planes of the camera and the angle of view used by the camera. The camera's clipping planes determine the distance (in world coordinates) when objects approaching the screen or receding from it are no longer drawn.

It is a good idea to give them informative names so another person, looking at the code, easily understands their purpose.

Okay, that wraps up the camera class and constants implementation. Let us move on to the visually appealing stuff: drawing stuff on the screen!

## Getting a Grip

Up until now, the new code has focused on setting up a viewpoint in the game world and added some additional infrastructure that is used by the game and various components. Game assets, in the form of models, are a large part of any 3D game. Even though this is a simple game, FuelCell includes many different types of game assets: models that represent game objects, textures that clothe the models, and a font to display game information such as the current score and goal status. For this step, let us add a very basic model and get it on the screen so we can begin to understand how our game world will look.

Every project template created by MonoGame has a sub-project called Content. This project must contain all your game assets. Although it is not required, it is a good idea to organize this content project such that similar assets are in the same folder. A common organization uses several folders: `Models`, `Textures`, `Fonts`, and `Audio`. These folders cover the main parts of a game. Let us add a `Models` folder, and a model, to our game.

> [!WARNING]
> This tutorial assumes you are using the game assets provided in this FuelCell sample. These assets have been sized in relation to each other so that none are too small or too large. You can use other models, but their scale (the size in the world coordinate system) might be radically different from the FuelCell models. This can cause a model to be rendered as a massive or minuscule object in the game world. In some cases, the camera (due to its position) might not be able to see the model at all. Therefore, it is recommended that you use the included FuelCell models when following these steps.
>
> After gaining some experience working with the camera class and rendering a 3D scene, you can experiment by adding your own models.

## Adding the Ground Content

Before we can draw the ground, we need a 3D model and its corresponding texture to load and then draw to the screen.  For this section we will use:

- [A Ground mesh model](../FuelCell.Core/Content/Models/ground.x)
- [A grid texture for the ground model](../FuelCell.Core/Content/Models/ground.png)

Download these files (`right-click -> save as`) ready for use in your content project, then continue.

> [!TIP]
> For a more detailed walkthrough on adding content to your game using the MGCB Editor, check the [Getting Started](https://monogame.net/articles/getting_started/4_adding_content.html) guide.

- Open the `.mgcb` content project with the [**MonoGame Content Builder Editor**](https://monogame.net/articles/tools/mgcb_editor.html) by double-clicking on the `.mgcb` file in the `Content` folder, or running `mgcb-editor` in the VSCode terminal.

    > [!TIP]
    > If you are using one of the VSCode extensions for MonoGame, you can then `right-click` the `.mgcb` file and select `Open in MGCB Editor`.

- Add a New Folder from the context menu using either `right-click -> add -> new folder` or from the `Edit` menu option.

- Name this new folder `Models`.

- Select the Models folder icon and from the context menu, select `Add -> Existing Item....`

- Navigate to the folder containing the downloaded game assets and add the `ground.x` model and the ground.png texture, then click `Open`.

Your content project should now look as follows:

![MGCB editor with Ground model and texture](Images/02-01-mgcb-editor.png)

## Time to start building the Game

You now have a working camera object, and a ground model, in your project. In the next step, you will add code declaring and initializing both of these objects and use them to render a nice terrain in the game world. For the remainder of this step, you will be working exclusively in the `Game1.cs` file, which is the main file of a MonoGame Framework game.

Before you start modifying the `Game1.cs` file, rename it to `FuelCellGame.cs`. This follows the naming format of the other project files.  You will also have to rename the class definition inside the `FuelCellGame.cs` to `FuelCellGame` to match, as follows:

```csharp
public class FuelCellGame : Game
```

Then add the following code, replacing the existing declaration of the `GraphicsDeviceManager` and `SpriteBatch` members of the `Game` class:

```csharp
private GraphicsDeviceManager graphics;
private GameObject ground;
private Camera gameCamera;
```

Now, replace the existing constructor to make use of the properties above (since the default MonoGame template uses different names):

```csharp
public FuelCellGame()
{
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
}
```

Update the existing `Initalize` method to initialize both game objects (using their default constructors) with the following code:

```csharp
protected override void Initialize()
{
    // Initialize the Game objects
    ground = new GameObject();
    gameCamera = new Camera();

    base.Initialize();
}
```

Next, update the existing `LoadContent` method to the following:

```csharp
protected override void LoadContent()
{
    ground.Model = Content.Load<Model>("Models/ground");
}
```

You have added code declaring and initializing your camera class and the terrain model. To see all this work on the screen, you must update the existing `Draw` method to render the terrain. This is also a good time to add code that updates, during each frame, the camera's position and orientation. Currently, this update code does nothing because the fuel carrier (the user-controlled avatar vehicle) is not in the game yet. However, when the vehicle is added in a later step, the camera automatically updates, chasing the vehicle around as the player tries to find hidden fuel cells.

## Opening Your "Eye"

Updating the camera occurs in the aptly named `Update` method. At this time, the information passed to the `Camera.Update` method is faked because there is no vehicle to focus on. Specifically, the position and rotation for the camera are zeroed out. This means the camera is centered slightly above the terrain model and aligned with the z-axis. This is the axis that represents the depth of the game world. Once you add the vehicle, the `Camera.Update` method will be passed the position and rotation of the vehicle, instead of zeros.

This modification is very simple because you already implemented the `Camera.Update` method (In the `Camera.cs` class). Now, you just need to call it at the proper time and pass some valid values.

Add the following code to the `Update` method of the `FuelCellGame.cs` file:

```csharp
float rotation = 0.0f;
Vector3 position = Vector3.Zero;
gameCamera.Update(rotation, position, graphics.GraphicsDevice.Viewport.AspectRatio);
```

The final step modifies the existing `Draw` method, replace the `Draw` method of the `FuelCellGame.cs` file to match the following:

```csharp
protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);

    DrawTerrain(ground.Model);

    base.Draw(gameTime);
}
```

This code calls a non-existent `DrawTerrain` method, that will use the approach detailed in [How To: Render a Model]() to render the terrain. Let us add that method now.

Add the following method after the `Draw` method:

```csharp
private void DrawTerrain(Model model)
{
    foreach (ModelMesh mesh in model.Meshes)
    {
        foreach (BasicEffect effect in mesh.Effects)
        {
            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;
            effect.World = Matrix.Identity;

            // Use the matrices provided by the game camera
            effect.View = gameCamera.ViewMatrix;
            effect.Projection = gameCamera.ProjectionMatrix;
        }
        mesh.Draw();
    }
}
```

The `DrawTerrain` method uses a rendering technique commonly used by MonoGame Framework games – by iteratively using draw calls on all the child meshes of the parent model (Models are always made up of many parts, so this loop simply draws them all). In this rather simple case, the ground model only has one mesh. But for more complex models, this approach is required to properly render the model on the screen. The calls to [EnableDefaultLighting](https://monogame.net/api/Microsoft.Xna.Framework.Graphics.BasicEffect.html#Microsoft_Xna_Framework_Graphics_BasicEffect_EnableDefaultLighting) and [PreferPerPixelLighting](https://monogame.net/api/Microsoft.Xna.Framework.Graphics.BasicEffect.html#Microsoft_Xna_Framework_Graphics_BasicEffect_PreferPerPixelLighting) highlight the power of the MonoGame Framework because you will get a standard 3-source lighting setup and smoother model lighting for free, creating some great results with little work!

> [!TIP]
> You can read more about lighting in MonoGame in the [What is: Default Lighting]() article on the MonoGame docs site.

![End of Step 2](Images/02-02-status-sep%202.png)

Go ahead and compile and build your project. You should be hovering over a gray and light-blue terrain under a black sky. It does not look like much now, but the [next](3-FuelCell-Casting-call.md) part adds the rest of the 3D models and displays them on the screen.

## See Also

### Next

- [FuelCell: Casting Call](3-FuelCell-Casting-call.md)

### Conceptual

- [FuelCell: Introduction](../README.md)

### Tasks

- [How To: Make a First-Person Camera]()
- [How To: Render a Model]()
