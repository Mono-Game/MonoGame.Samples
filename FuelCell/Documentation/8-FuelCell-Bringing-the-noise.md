# FuelCell: Bringing the Noise

## In this article

- [Overview of Game State Management](#overview-of-game-state-management)
- [Anybody Got the Time?](#anybody-got-the-time)

- [See Also](#see-also)

Discusses the addition of audio to the FuelCell game.

## It is a bit quiet

No game worth its salt is silent (although there are exceptions), even those that go the extra mile and add accessibility elements to narrate the environment of the game, bring some feeling into the environment.

Sound is a crucial component to the gameplay experience, from background music to sound effects and all round haptic responses. (yes, even vibration is an audio response).

In this chapter we will introduce the basics allowing you to explorer further on your own by adding background theme music, engine rumble while we power around the area and a nifty alarm when we collect those fuel cells.

## Grab those files

It is possible to generate sounds at runtime and there are some fantastic libraries out there, even for MonoGame to do this, but let us keep things simple using the content pipeline.

> [!NOTE]
> All the audio files used here were sourced from [MixKit](https://mixkit.co/free-sound-effects/game/), but feel free to use your own if you wish.

For this section you will need the three sounds we discussed which are included in the source, these are:

- [Background Music]() to brighten the mood.
- [Engine Rumble]() to hear your machine roar across the area.
- [Startled Delight]() as we pick up those precious fuel cells to return to the colony.

Download these files and add them to your content project just as you did with the textures and models, preferably in a folder called "**Audio**".

> [!WARNING]
> If you use your own sounds or store them in a different place/folder, make sure you update the paths in the `Content.Load()` calls.  Else they basically will not work.

Build your project to make sure the audio is loading as expected and then let us continue to load and play them.

## A little ambience

Let us start with the background music, this is a sound or track that effectively plays on a loop as the game plays, you can use different music for your game menus or even ramp up the music for dramatic events, but let us not get too far ahead of ourselves.

Loading the music is simple and uses the [Content Pipeline](https://monogame.net/api/Microsoft.Xna.Framework.Content.Pipeline.html) in the same process as Texture and Models but uses a different processor type, for long running audio that is a "[Song](https://monogame.net/api/Microsoft.Xna.Framework.Media.Song.html)".

> [!WARNING]
> Make sure to set the "**Processor**" type in the properties for the `background-music.mp3` file to **SONG** in the MGCB Editor.  By default, mp3 files default to use the "Song" processor and wav files default to the "SoundEffect" processor. (which we will use later)

First let us declare a variable to store our loaded music, right after the `aspectRatio` property:

```csharp
private Song backgroundMusic;
```

Next, in the `LoadContent` method, add the following  after loading the bounding sphere models:

```csharp
backgroundMusic = Content.Load<Song>("Audio/background_music");
```

> [!TIP]
> It is possible to load content without using the Content Pipeline, as some do, but you will then lose the management capabilities and features the Content Pipeline brings, as well as any additional content processing capabilities.
> But it is a world of developers choice and you can use the following instead if so you wish as all the content classes support file loading too (but we will continue with the content pipeline for this tutorial):
>
> ```csharp
> ```

With the music loaded, all we now need to do is to set it going on a loop and forget about it (unless we want to stop it playing or change the track)

At the end of the `ResetGame` method when the game round begins, we ad a check to see if the audio is playing and restart/start it.

> [!TIP]
> It is best practice to surround the use of the `MediaPlayer` class with a [Try/Catch](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements) block, this is due to a known issue when a debugger is connected which "can" cause an exception, this code simply prevents this breaking your game during testing.

```csharp
            try
            {
                MediaPlayer.Stop();
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.5f;
                MediaPlayer.Play(backgroundMusic);
            }
            catch { }
```

> [!WARNING]
> If you are targeting **iOS** for your MonoGame project, then when using the `MediaPlayer` you will need to define an extra `using` statement (shown below) to specifically identify you are using MonoGame's MediaPlayer, this is because iOS natively has another class called MediaPlayer and C# can get confused if you are not specific:
>
> ```csharp
> using MediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
> ```

And to finish off the background audio, if the game has finished, ideally we want the game background music to also stop playing, so at the end of the `FuelCellGame.cs` class in the `Update` section, change the `Won/Lost` to also stop the audio if it is playing:

```csharp
    if ((currentGameState == GameState.Won) || (currentGameState == GameState.Lost))
    {
        // Gameplay has stopped and if audio is still playing, stop it.
        if(MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        // Reset the world for a new game
        if (inputState.StartGame(PlayerIndex.One))
        {
            ResetGame(gameTime, aspectRatio);
        }
    }
```

Now we have some, slightly, annoying background music, let us also play with some sound effects.

## Celebrating the collection of the cells

It is always good to reward the player, not just with points, but also with a nice rewarding sound to announce the event, for this we will open the `FuelCell.cs` class to declare a [SoundEffect](), load it and then play it on demand.

> [!NOTE]
> Unlike [Songs](https://monogame.net/api/Microsoft.Xna.Framework.Media.Song.html) played with the [MediaPlayer](https://monogame.net/api/Microsoft.Xna.Framework.Media.MediaPlayer.html), [SoundEffects](https://monogame.net/api/Microsoft.Xna.Framework.Audio.SoundEffect.html) are one-shot sounds, playing once until they have finished.  There are more advanced things you can do with [SoundEffectInstances](https://monogame.net/api/Microsoft.Xna.Framework.Audio.SoundEffectInstance.html) but for now we are keeping things simple.

Add a new [SoundEffect]() property at the top of the `FuelCell.cs` class:

```csharp
    private SoundEffect fuelCellCollect;
```

In the `LoadContent` method, load the sound file from the content project.

```csharp
    fuelCellCollect = content.Load<SoundEffect>("Audio/fuelcell-collect");
```

And finally, in the `Update` method after the `this.Retrieved = true;` line, we play the sound effect:

```csharp
    internal void Update(BoundingSphere vehicleBoundingSphere)
    {
        if (vehicleBoundingSphere.Intersects(this.BoundingSphere))
        {
            this.Retrieved = true;
            fuelCellCollect.Play();
        }
    }
```

As stated, this is a one shot effect and the effect stops as soon as it finishes, but now whenever we gain a point, we also play the effect.

> [!NOTE]
> You will see this pattern repeated with any MonoGame Project, have a reference, Load the content and then play/use it.  Rinse and repeat.

## The rumbling of the engine

When your FuelCarrier is moving, let us make the player hear the not so distant rumbling of its engines.  We only want this to play when the player is moving, so we also need to check this.

In the `FuelCarrier.cs` class, as before we start by declaring a variable for the Sound Effect:

```csharp
        private SoundEffect engineRumble;
```

Continuing the trend, in the `LoadContent` method, we "shockingly" load the audio file to the variable:

```csharp
    engineRumble = content.Load<SoundEffect>("Audio/engine-rumble");
```

And to finish, in the `Update` method, straight after getting the keyboard input for the player, we check if we are moving and then play the sound:

```csharp
    Vector3 speed = Vector3.Transform(movement, orientationMatrix);
    if (speed != Vector3.Zero)
    {
        engineRumble.Play();
    }
```

This actually is not the most efficient way of playing an engine rumble, but it is certainly the easiest, because we are playing a new sound EVERY update frame, so we end up with multiple instances of the sound playing continuously.  But the effect is only a second long, keeping it short so we do not run into issues (there is a limit to how many sound effects you can play in a single frame).

Run the game and now you have a running game complete with audio.

## A working game

As the core of our game is up and running, it is still a little far too quiet in here.  In the next chapter we will add some audio to our title and bring in the noise!

## See Also

### Conceptual

-[FuelCell: Introduction](../README.md)

### Tasks

-[How To: Play a Song]()
-[How To: Play a Sound]()
