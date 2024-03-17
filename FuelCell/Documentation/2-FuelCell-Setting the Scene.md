# FuelCell: Setting the Scene

## In this article

- The Complete Sample
- Overview
- Objects in the Game
- The Camera
- Game Constants
- Getting a Grip
- Opening Your "Eye"
- See Also

Discusses the implementation of a playing field for the game and a simple, fixed camera.

> [!NOTE]
> You must download the above sample code in order to access the 3D models used in this tutorial step.

## Overview

One of the largest hurdles a game developer faces when moving from 2D to 3D is that third 'D': depth. In the 2D world, game objects (called sprites) have two dimensions and are positioned using literal screen coordinates. There is a concept of depth, but this is used only to determine if a sprite is partially or fully obscured by another.

In a 3D game, what you see on your screen is a projection of a 3D environment onto a 2D surface (that is, your screen). This translation of 3D space into 2D space is accomplished using transformation matrices. Specifically, we refer to these three matrices as world, view, and projection matrices. Transformation is just a fancy word for changing the value of a coordinate by multiplication. Using these matrices, the MonoGame Framework transforms the coordinates of a 3D model to a set of new coordinate values (through rotation, scaling, or translation) used by the projection matrix. In a separate but related step, a view matrix simulates a viewpoint (often called the camera) in the same 3D space as the model; it looks in a certain direction. With these two matrices, a third matrix is brought into the "picture" to perform a final transformation into 2D screen coordinates. This creates a realistic 2D picture of the 3D scene on your computer screen.

Earlier, we mentioned a camera. Even though this isn't a real camera, it fulfills the same role in the 3D game. This camera observes the 3D world and renders whatever it sees into a 2D representation. This representation appears on the computer screen. In a game, the camera class usually is implemented as a stand-alone class. It is one of two varieties: a first-person camera (used in this game and first-person shooters) and a third person camera (often used in RPGs or platform games). First-person cameras are great for games that focus on a single player or are trying to immerse the player in the game world. Third-person cameras are better suited to viewing a large playing field or controlling numerous entities in the game. For this step, you will implement a first-person camera using code from [How To: Make a First-Person Camera]().

We use a first-person camera because the player controls a small vehicle that can move around and collect fuel cells. The difficulty of the game is finding these items before time runs out. It's difficult because the playing field has opaque barriers randomly scattered across it. Since we use a first-person camera, the player must drive around to view previously-hidden areas.

In addition to the camera code, you will also use code from [How To: Render a Model]() to display the 3D playing field model, which is a simple two-tone grid floating in space.

## Objects in the Game

3D game development is all about position and the relation to other objects in the local coordinate system (that is, the game world). In addition to position, a 3D object usually has an associated model. Because this is a 3D game, the model has three dimensions. This means it can be viewed from all angles and has volume. In addition to these two properties, the 3D object should have a bounding sphere. The bounding sphere is a theoretical sphere that encapsulates the model volume. It is used for detecting collisions in the game world with other 3D objects. You can ignore this for now, but it becomes critical later in the development process.

A class is the obvious solution for storing and tracking all these variables. However, before we can add this class, you need to first create a new project for the FuelCell game.

- Create a new MonoGame project called FuelCell.
- In this project, create a new class called `GameObject`.

The `GameObject` class will contain all those properties mentioned earlier and a constructor that sets the various properties to known values. The file containing this new class only has a few references by default (located at the top). To grant easy access to the MonoGame Framework assemblies, you'll need to add some MonoGame specific ones. At the top of the file, add the following references:

C#

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
```

These new references make it possible to use the short form of reference for MonoGame Framework-specific classes. For instance, instead of declaring a variable of type `Vector2` by specifying `Microsoft.Xna.Framework.Vector2`, we can use `Vector2` instead. That will save a lot of typing during the development of FuelCell!

You're ready to modify the default class declaration to better fit your needs. Replace the existing `GameObject` class declaration with the following:

C#

```csharp
namespace FuelCell
{
    class GameObject
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

This new version now tracks the position, model, and bounding sphere of an object in the game using auto-implemented properties. The constructor is simple, and it initializes each property to a reasonable value – either `null` or `Vector3.Zero`.

## The Camera

The `GameObject.cs` file will also contain the camera class declaration, which is taken from the [How To: Make a First-Person Camera]() topic. As mentioned earlier, the main purpose behind this developer diary is to demonstrate how you (or any developer) can use various How To articles as stepping stones when developing an MonoGame Framework game. For this first usage, this concept is clearly illustrated by not changing any of the variable names or classes, whenever possible. This may cause a bit of confusion or head-scratching when you come across variable names like `_avatarHeadOffset` and `avatarYaw`, but it serves to tie the source How To more closely to the actual game code. This creates the ability to easily determine where the source code of a How To ends up in a typical game project by searching for the variable name used in the How To.

For example, in this step, some of the property names match the names used in the original sample code: `_avatarHeadOffset` is the camera's distance above the playing field and `_targetOffset` is the offset from the target. In this case, it is a fixed distance in front of the fuel carrier vehicle. These values are used when calculating the camera position from the current position of the fuel carrier vehicle (for example, `position`) in the world coordinate system.

The camera class is similar in structure to the `GraphicObject` class. It has a set of properties and a method. In this case, it is `Update`. For this game, the camera acts like a rigid chase camera. It follows behind, and slightly above, the actual vehicle and points in the same direction as the vehicle at all times.

All right, enough talk – let's start developing!

Create a new `Camera.cs` class and use the following code:

```csharp
using Microsoft.Xna.Framework;

namespace FuelCell
{
    class Camera
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

|Researching Transformation Matrices|
|-|
|The transformation matrices are used to rotate, move, or scale objects in a world coordinate system and then (along with the view matrix) to a perspective 2D coordinate system: your screen. The theory and application of this concept involves a truckload of math. However, you can read more about these concepts in other areas of the MonoGame documentation:

- [Math Overview]()
- [Step 4 (of Tutorial 1: Displaying a 3D Model on the Screen)]() discusses the usage of all three matrices.
- [Viewports and Frustums]()
|

The Update method is where the main math for updating the camera takes place. This function takes the current rotation of the vehicle and creates a transformation matrix, which in turn is used to transform the camera's offset values. These values are then added to the current vehicle position, creating a point, in the world coordinate system, where the camera "sits." The final step generates the view and perspective matrices, used when rendering the 3D game world view onto your 2D monitor screen.

## Game Constants

Did you notice that some of the method arguments were from a `GameConstants` class? Let's create this class and then I'll explain its purpose.

Add a new class to the project, called `GameConstants.cs`.

Since you will also be using MonoGame Framework references in this file, add the following references to the beginning of the file:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
```

Now add the following to the GameConstants class declaration:

```csharp
namespace FuelCell
{
    class GameConstants
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

Okay, that wraps up the camera class and constants implementation. Let's move on to the visually appealing stuff: drawing stuff on the screen!

## Getting a Grip

Up until now, the new code has focused on setting up a viewpoint in the game world and added some additional infrastructure that is used by the game and various components. Game assets, in the form of models, are a large part of any 3D game. Even though this is a simple game, FuelCell includes many different types of game assets: models that represent game objects, textures that clothe the models, and a font to display game information such as the current score and goal status. For this step, let's add a very basic model and get it on the screen so we can begin to understand how our game world will look.

Every project template created by MonoGame has a sub-project called Content. This project must contain all your game assets. Although it isn't required, it's a good idea to organize this content project such that similar assets are in the same folder. A common organization uses several folders: Models, Textures, Fonts, and Audio. These folders cover the main parts of a game. Let's add a Models folder, and a model, to our game.

> [!WARNING]
> This diary assumes you are using the game assets located in the FuelCell sample file downloaded earlier. These assets have been sized in relation to each other so that none are too small or too large. You can use other models, but their scale (the size in the world coordinate system) might be radically different from the FuelCell models. This can cause a model to be rendered as a massive or miniscule object in the game world. In some cases, the camera (due to its position) might not be able to see the model at all. Therefore, it is recommended that you use the included FuelCell models when following these steps. After gaining some experience working with the camera class and rendering a 3D scene, you can experiment by adding your own models.

- Select the `Content` folder icon and select New Folder from the context menu.

- Name this new folder Models.

- Select the Models folder icon and from the context menu, select Add and then Existing Item....

- Navigate to the folder containing the downloaded game assets and add the ground.x model.

You now have a working camera object, and a ground model, in your project. In the next step, you will add code declaring and initializing both these objects and use them to render a nice terrain in the game world. For the remainder of this step, you will be working exclusively in the Game1.cs file, which is the main file of an MonoGame Framework game.

Open the `Game1.cs` file.

Add the following code, after the existing declaration of the graphics member of `Game1`:

```csharp
GameObject ground;
Camera gameCamera;
```

In the existing `Initalize` method, initialize both game objects (using their default constructors) by adding the following code:

```csharp
ground = new GameObject();
gameCamera = new Camera();
```

Next, add the following code to the existing `LoadContent` method:

```csharp
ground.Model = Content.Load<Model>("Models/ground");
```

You've added code declaring and initializing your camera class and the terrain model. To see all this work on the screen, you must update the existing `Draw` method to render the terrain. This is also a good time to add code that updates, during each frame, the camera's position and orientation. Currently, this update code does nothing because the fuel carrier (the user-controlled avatar vehicle) isn't in the game yet. However, when the vehicle is added in a later step, the camera automatically updates, chasing the vehicle around as the player tries to find hidden fuel cells.

## Opening Your "Eye"

Updating the camera occurs in the aptly-named `Update` method. At this time, the information passed to the `Camera.Update` method is faked because there is no vehicle to focus on. Specifically, the position and rotation for the camera are zeroed out. This means the camera is centered slightly above the terrain model and aligned with the z-axis. This is the axis that represents the depth of the game world. Once you add the vehicle, the `Camera.Update` method will be passed the position and rotation of the vehicle, instead of zeros.

This modification is very simple because you already implemented the Camera.Update method. Now, you just need to call it at the proper time and pass some valid values.

Add the following code to the `Update` method of the `Game1.cs` file:

```csharp
float rotation = 0.0f;
Vector3 position = Vector3.Zero;
gameCamera.Update(rotation, position, graphics.GraphicsDevice.Viewport.AspectRatio);
```

The final step modifies the existing `Draw` method.

Modify the body of the `Draw` method of the `Game1.cs` file to match the following:

```csharp
graphics.GraphicsDevice.Clear(Color.Black);

DrawTerrain(ground.Model);
```

This code calls the non-existent `DrawTerrain` method. The method uses the approach detailed in [How To: Render a Model]() to render the terrain. Let's add that method now.

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

The `DrawTerrain` method uses a rendering technique commonly used by MonoGame Framework games – iterative draw calls on child meshes of the parent model. In this rather simple case, the ground model only has one mesh. But for more complex models, this approach is required to properly render the model on the screen. The calls to [EnableDefaultLighting]() and [PreferPerPixelLighting]() highlight the power of the MonoGame Framework because you'll get standard 3-source lighting and smoother model lighting for free, creating some great results with little work!

Go ahead and compile and build your project. You should be hovering over a gray and light-blue terrain under a black sky. It doesn't look like much now, but the [next](3-FuelCell-Casting%20Call.md) part adds the rest of the 3D models and displays them on the screen.

## See Also

### Conceptual

- [FuelCell: Introduction](../README.md)

### Tasks

- [How To: Make a First-Person Camera]()
- [How To: Render a Model]()
