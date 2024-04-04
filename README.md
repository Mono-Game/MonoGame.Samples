# MonoGame 3.8.1 Samples

<p align="center">
<br/>
  <img src="https://raw.githubusercontent.com/Mono-Game/MonoGame.Logo/master/FullColorOnLight/HorizontalLogo_128px.png"/>
<br/>
<br/>
</p>

A number of simple MonoGame samples for all the supported platforms:


[Platformer 2D Sample](Platformer2D/README.md) | [NeonShooter](NeonShooter/README.md)|
|-|-|
Supported on all platforms | Supported on all platforms |
[![Platformer 2D Sample](Images/Platformer2D-Sample.png)](Platformer2D/README.md) | [![NeonShooter Sample](Images/NeonShooter-Sample.png)](NeonShooter/README.md) |
The [Platformer 2D](Platformer2D/README.md) sample is a basic 2D platformer pulled from the original XNA samples and upgraded for MonoGame.| [Neon Shooter](NeonShooter/README.md) Is a graphically intensive twin-stick shooter with particle effects and save data from Michael Hoffman |
|||

| [Auto Pong Sample](AutoPong/README.md) | [Ship Game 3D](ShipGame/README.md) |
|-|-|
| Supported on all platforms | GL / DX/ iOS / Android |
| [![Auto Pong Sample](Images/AutoPong_1.gif)](AutoPong/README.md) | [![ShipGame 3D Sample](Images/ShipGame.gif)](ShipGame/README.md) |
| A short [sample project](AutoPong/README.md) showing you how to make the classic game of pong, with generated soundfx, in 300 lines of code. | 3D Ship Game (Descent clone) sample, pulled from the XNA archives and updated for MonoGame |
|||

## Building the samples

These samples can either be opened and built in Visual Studio for Mac or Windows, alternatively they can be built with the .NET Core tool chain (with the exception of the Windows UWP sample, which is Windows 10 only).

For more details on building projects with the .NET Core tools, please see [this guide on the MonoGame documentation site](https://docs.monogame.net/articles/packaging_games.html).

## Supported platforms

The MonoGame samples demonstrate all the public platforms available for use.

> Platforms such as XBox, Playstation, Switch and Stadia are private platforms which you need developer accounts with their corresponding vendors to access.

* Windows (Desktop GL, Windows DX, UWP)
* Mac (Desktop GL)
* Linux (Desktop GL)
* Android (Android)
* iOS (iOS)

> Note, to build a project for iOS or Mac, you will need a MAC.  They will build on Windows but cannot be published unless built on a Mac.

For more details on the platforms supported by MonoGame, please see the [Platforms Guide](https://docs.monogame.net/articles/platforms/0_platforms.html) on the MonoGame Documentation site
