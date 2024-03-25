# FuelCell: Bringing the Noise

## In this article

- [Overview of Game State Management](#overview-of-game-state-management)
- [Anybody Got the Time?](#anybody-got-the-time)

- [See Also](#see-also)

Discusses the addition of audio to the FuelCell game.

## It is a bit quiet

No gme worth its salt is silent, even those that go the extra mile and add accessibility  elements to narate the environment of the game bring some feeling into the environment.

Sound is a crucial component to the gameplay experience, from background music to sound effects and all round haptic responses. (yes, even vibration is an audio response).

In this chapter we will introduce the basics allowing you to explorer further on your own by adding background theme music, engine rumble while we power arond the area and a nifty alarm when we collect those fuel cells.

## Grab those files

It is possible to generate sounds at runtime and there are some fantasic libraries out there, even for MonoGame to do this, but let us keep things simple using the content pipeline.

For this section you will need the three sounds we discussed which are included in the source, these are:

- [Background Music]() to brighten the mood.
- [Engine Rumble]() to hear your machine roar aross the area.
- [Starteld Delight]() as we pick up those precious fuel cells to return to the colony.

Download these files and add them to your content project just as you did with the textures and models, preferably in a folder called "Audio".

> [!IWARNING]
> If you use your own sounds or store them in a different place/folder, make sure you update the paths in the `Content.Load()` calls.  Else they basically will not work.

Build your project to make sure the audio is loading as expected and then let us continue to load and play them.

## A little ambience

Let us start with the background music, this is a sound or track that effectively plays on a loop as the game plays, you can use different music for your game menus or even ramp up the music for dramatic events, but let us not get too far ahead of ourselves.

Loading the music is simple and uses the [Content Pipeline]() in the same process as Texture and Models but uses a different content type, for long running audio that is a "[Song]()".

First let us declare a varaible to store our loaded music, right after the `aspectRatio` property:

```csharp
private Song backgroundMusic;
```

Next, in the `LoadContent` method, add the following:

```csharp
backgroundMusic = Content.Load<Song>("Audio/background_music.mp3")
```

> [!TIP]
> It is possible to load content without using the Content Pipeline, as some do, but you will then lose the management capabilities and features the Content Pipeline brings, as well as any additional content processing capabilities.
> But it is a world of developers choice and you can use the following instead if so you wish as all the content classes support file loading too (but we will continue with the content pipeline for this tutorial):
> ```csharp
> ```

With the music loaded, all we now need to do is to set it going on a loop and forget about it (unless we want to stop it playing or change the track)

In the `Update` method when the game round begins, we ad a check to see if the audio is playing and restart/start it.

> [!TIP]
> **ALWAYS** check if music is already playing before hitting play, else on some platforms it can cause issue.

```csharp
if (backgroundMusic.IsPlaying()
{
    backgroundMusic.Stop();
}
backgroundMusic.Play();
```