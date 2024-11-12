# Ship Game Starter Kit

![ShipGame 3D Sample](../Images/ShipGame.png)

This topic contains the following sections:

* Introduction to Ship Game
* Features
* Getting Started
* Extending Ship Game

## Introduction to Ship Game

The Ship Game Starter Kit is a complete XNA Game Studio game. The project comes ready to compile and run, and it's easy to customize with a little bit of C# programming. You are free to use the source code as the basis for your own XNA Game Studio game projects, and to share your work with others.

Ship Game is a 3D spaceship combat game set inside a complex tunnel system. Ship Game features advanced lighting and textures, a full GPU particle system, and advanced physics. Explore the tunnels on your own, or take on a friend head-to-head using split-screen mode.

> Note
> This documentation assumes that you have a basic knowledge of programming concepts and the Visual C# environment.

## Features

This starter kit provides a complete XNA Game Studio game, including source code and game content such as models, textures, and sounds. This starter kit documentation describes the general layout and controls for Ship Game. The Ship Game Starter Kit demonstrates the following features.

### Rendering

* Per-pixel lighting (including specular highlights)
* Normal and environment maps

### Special Effects

* Glow effect using glow and environment maps
* Animated sprites with frame blending, used for explosion and spawn effects
* Full GPU particle system with point sprites and rotation to velocity direction, used for explosions and trail effects

### Gameplay

* Rotation and movement physics using forces and damping
* Ship-to-environment and ship-to-ship collision detection
* Ship offsets, ship spawn points, and pickup positions serialized from XML
* Several types of gameplay modifier pickups:
  * Boost effect that triples player speed
  * Shield effect that defends against damage
  * Blaster weapon with low damage but unlimited projectiles
  * Missile weapon with high damage but limited projectiles
* Heads-up display (HUD) with score, missile count, and dynamic bars showing energy, shield, and boost charges

## Getting Started

### Requirements

To build this sample you need to have the [DotNet 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.

### Creating, Building, and Running a Ship Game Project

There are versions of ShipGame for the following platforms:

* Android
* iOS
* Windows GL
* Windows DX

Within each project, there are the following projects:

* Platform project (android/iOS/windows) - including a content reference to the content.
* Core project - containing the game code.
* BoxCollider - a collision library for the project.
* NormalMappingModelProcessor - A custom [content pipeline](https://monogame.net/articles/content_pipeline/why_content_pipeline.html) processor for the models in the Game.

> **NOTE** in order to run the project, the `NormalMappingModelProcessor` needs to be built in `Debug` mode at least once.  The [dependency path](https://github.com/MonoGame/MonoGame.Samples/blob/9e36bf676a8e825a1c1eb2afc41ba596e4a74720/ShipGame/ShipGame.Core/Content/ShipGame.mgcb#L13) to the processor dll in the `.mgcb` file is (for the moment) fixed.
>
> You can either build for `debug` or change the path to where a built version of the `NormalMappingModelProcessor` is located to run the project, else you will get "Processor" errors in the MGCB tool.

The code is split up this way in order to share the same central game code for all platforms.  Each platform initializes the `ShipGameGame` Game class in the core project to run the game.

## Playing Ship Game

### Ship Game Screens

Ship Game begins at the Main Menu.

#### Main Menu

From the Main Menu, you may choose to start a **Single Player** game, start a **Multiplayer** game, access **Help**, or **Quit**.

|Action|Controller|Keyboard|
|-|-|-|
|Highlight a menu option.|D-Pad UP or D-Pad DOWN|UP ARROW or DOWN ARROW|
|Select a highlighted option.|A|SPACEBAR|
|Quit the game.|BACK|ESC|

#### Single Player Menu

The Single Player game is your chance to explore the levels in Ship Game before taking on another player. Before the game starts, you have a chance to choose the ship you'd like to fly, and optionally, invert the y-axis of your controller (this can assist you if "normal" controller settings feel upside-down).

|Action|Controller|Keyboard|
|-|-|-|
|Change your ship type.|D-Pad UP or D-Pad DOWN|UP ARROW or DOWN ARROW|
|Toggle invert the y-axis option.|Y||
|Rotate the ship.|Left stick|LEFT ARROW or RIGHT ARROW|
|Start the game with the selected ship and settings.|A|SPACEBAR|
|Return to main menu|BACK|ESC|

#### Multiplayer Menu

In Multiplayer mode, you can go head-to-head with a friend in split-screen mode. Explore the tunnels, collect pickups, and destroy the other player's ship before they destroy you!

> Note
> To play Multiplayer mode, you must have either two Xbox 360 controllers plugged in, or an Xbox 360 controller and a keyboard. Two players cannot share a single keyboard to play Ship Game multiplayer.

Before the game starts, each player has a chance to choose the ship they would like to fly, and optionally, invert the Y axis of their controller.
Controls are the same as in the Single Player Menu. Both players must confirm their choices by pressing A or SPACE before the game begins.

### Ship Game In-Flight Controls

Once you start a single player or multiplayer game, use your keyboard or Xbox 360 Controller to move your ship and interact with the game world. The controls are mapped as follows:

|Action|Controller|Keyboard|
|-|-|-|
|Exit the game.|BACK|ESC|
|Move forward and backward.|Left stick UP and DOWN|W and S|
|Move left and right (slide).|Left stick LEFT and RIGHT|Q and E|
|Aim up or down, left or right.|Right stick|UP ARROW, DOWN ARROW, LEFT ARROW, and RIGHT ARROW|
|Bank left or right.|Left bumper and right bumper|A and D|
|Fire the blaster weapon.|Right trigger|SPACEBAR|
|Fire the missile weapon.|Left trigger|ENTER|
|Use the boost.|Y|SHIFT|
|Use the shield.|A|R|
|Toggle between first-person and third-person camera mode.|B|BACKSPACE|

## Extending Ship Game

There are many ways to add your own functionality to Ship Game. The following areas of the code are good areas to start.

### Game Options

```csharp
class GameOptions;
```

Many game constants are stored into a public object with several static members called GameOptions. You can change many game settings like ship movement, weapon speed, and shield duration by changing the constants found there.

### Screen Manager

```csharp
class ScreenManager;
```

The screen manager handles the available game screens (intro, player, level, game, end, and help). The screen manager supports a transition mechanism when switching screens by fading out the current screen into a solid color and then fading into the next screen. The screen manager passes input and rendering requests to its currently active screen though the abstract base class. Each screen derives from the abstract base screen class and loads and unloads its resources when activated and deactivated respectively.

### Blur Manager

```csharp
class BlurManager;
```

The screen manager also owns a blur manager object to support the glow effect available to all screens. After the current screen is drawn, the resulting screen alpha values are used as glow intensity to the blur manager horizontal and vertical passes. The normal map shader saves the specular highlights and glow intensities at the alpha of the shader output color.

### Screens

```csharp
abstract class Screen;

class ScreenIntro : Screen;
class ScreenPlayer : Screen;
class ScreenLevel : Screen;
class ScreenGame : Screen;
class ScreenEnd : Screen;
class ScreenHelp : Screen;
```

The intro screen implements a simple menu with four options (single player, multiplayer, help, quit). When one of the menu options is selected, it requests a transition to the next screen, the player configuration screen where you select your ship. It also sets the game mode (single player/multiplayer) in the game manager.

The player screen loads the available player ships and displays the current selected ship for each player. The player can rotate the ship and change the selected model. All geometry renders just like in the game with normal maps and glow. There is also an option to invert the Y rotations since many players use inverted Y. In multiplayer mode, it waits for both payers to confirm the selected ship until moving to the next screen, the level selection screen. This screen sets the ships selected by each player into the game manager as well as the Y rotation options.

The level screen shows the available maps to play. When a level is selected, it requests a transition to the next screen, the game screen. It also sets the selected level into the game manager.

The game screen is where most things happen. When the game screen is activated, it loads all game data and sets up the player ships as defined by the player selection screen. The game screen passes all input and draw requests into the game manager, and it acts as the game window.

The end screen shows the winning player and waits for any input to go back to the screen.

### Input Manager

```csharp
class InputState;
class InputManager;
```

The input manager is used to control each player input state. It always holds a player state for the current and last frames for each player. This allows it to test for key presses and not just key down/up flags. A key press happens when the key was up in the last frame and turns down in this frame. This is useful for events that happen only once and should not repeat every frame while the key is pressed.

### Game Manager

```csharp
class GameManager;
```

The game manager holds all game objects and is the largest code file in the project. It loads the game level model, the collision model, and all game effect resources, such as textures and models used by the animated sprites, particles, and projectiles.

The game manager also owns other effect-related managers such as the animated sprite manager, particle system manager, projectile manager, and powerup manager. Only through the game manager can you add new game effects to its respective manager.

### Player Ship

```csharp
class PlayerShip;
class PlayerMovement;
```

The PlayerShip object holds per player information during the game. It holds the player score and missile count and handles the abilities use and charging (boost and shield). It also controls the player movement using some basic physics for translation and rotation forces and damping factors.

### Animated Sprite Manager

```csharp
class AnimSprite;
class AnimSpriteManager;
```

The animated sprite manager groups all currently active animated sprites. An animated sprite is a 2D billboard aligned to the camera with an animated texture on it. The texture map plays like a small video, using multiple frames at 256×256 resolution packed into a texture grid. For example, a 1024×1024 image would contain 16 frames. When rendering, two frames are always sampled and interpolated to the current time for a smooth playback even at low speeds.

### Particle System Manager

```csharp
class Particle;
class ParticleSystem;
class ParticleManager;
```

The particle system manager groups all currently active particle systems. A particle system is a collection of point sprites that can be used as a burst (all particles emitted at same time) or loop (particles seem to continuously emit at a given rate). The point sprites rotate in 2D screen space to align to the current velocity vector so you can have more directional particles and not just round particles.

### Powerup Manager

```csharp
class Powerup;
class PowerupManager;
```

The powerup manager groups all currently active powerups. The powerups are loaded from an XML file with their locations and types. When a ship goes close to a powerup (their bounding boxes intersect), the powerup is picked up and an animated sprite replaces the powerup model. After some time, the powerup respawns and is available for picking up again.

### Projectile Manager

```csharp
class Projectile;
class ProjectileManager;
```

The projectile manager holds all currently active projectiles. A projectile is created when a ship fires one of its weapons. On creation, the projectile computes its intersection with the level model and starts to move to its target collision point. At every frame, it tests the projectiles against the player ships since they are the only dynamic objects in the scene. If the projectile reaches the target position without hitting any ships, it explodes on the level collision point.

## Credits

Several contributors have helped to support this sample:

* [Charles (NemoKrad) Humphrey](https://nemokrad.itch.io/) - Fixed and updated the Shaders from the original XNA samples.
* [Grace Martin](https://www.artstation.com/gracemartin) - Rebuilt and re-textured Level 1, as the XNA originals had several issues.
