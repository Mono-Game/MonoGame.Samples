# FuelCell: Game Design

## In this article

- [The Purpose Behind Game Design](#the-purpose-behind-game-design)
- [FuelCell Features](#fuelcell-features)
- [See Also](#see-also)

Discusses the importance of designing a game using a feature-based approach.

## The Purpose Behind Game Design

Before every good game, there was game design. Notice that part about "good games"? A common pitfall for game developers is that they might have great ideas in mind, but the translation to the screen turns out to be impossible. It is hard not to lose heart when the developer must focus on developing basic game and code structure before displaying the defense of a planet against hordes of 3D alien enemies. Game design can seem boring and needless when compared to getting the developer's vision onto the monitor.

Unfortunately, a poor game design can lead to the situation where a person sits down with the latest demo, plays it for ten minutes, and gets lost or frustrated.

Like computer games, books are another example of a product that requires careful design. Every good book has a "hook" line: an opening sentence that immediately piques the reader's curiosity or interest. If the reader finishes the first page (or even the first paragraph) and does not have at least one story-related question rattling around in the brain, that reader probably will not finish the book.

Game design is a technique that creates the "hook" for the player. Your purpose is to capture the gamer's attention in the first couple of minutes. Otherwise, the player may walk away. A good set of game features enrols your audience into the game's story. It convinces a player to finish the game. However, a good opening is not enough. You must have equally good content that continues to engage or challenge the gamer.

With that in mind, in addition to a good opening, games typically benefit from the following features:

- A clear and achievable goal. But what about the cries of outrage from the 80's arcade crowd: "And what was the achievable goal of `<insert favorite 80's arcade game here>?!` There was no goal – you died no matter how good you were!" Okay, they may have a point there, but times have changed. Today's games no longer require a constant flow of quarters to survive; they require a consistent paycheck (or a consenting adult with a consistent paycheck).
- Replayability. This can range from a randomly generated dungeon level or treasure pile to an ending that depends on the actions of the player. It can be argued that replayability is not a requirement of a good game, but it helps to lengthen the span of interest in a game, and it provides more entertainment for the player without the need for extra content or expansion packs.
- One or more obstacles to overcome. No one wants a game where the player just wanders over to the end-level boss, it falls dead at your feet, and the end credits roll. A little challenge (or a lot) is necessary to keep the player engaged. This can be, and usually is, the main content of a game. Common challenges include collecting special items, defeating enemies to reach a specific place in the game, or solving a complex puzzle using simple in-game items collected (or taken by force) earlier.
If a game provides at least these features, it is probably a game worth playing at least once. This is the part where game design comes into play. When you design your game, keep these features in mind. Providing content for these features is difficult, but worth the effort when it all comes together.

## FuelCell Features

To illustrate this point, the game design for FuelCell focused on fulfilling these three criteria:

|Game Feature|FuelCell Feature|
|-|-|
|**Clear Goal**|Collect 12 fuel cells (green canisters of goo) before time runs out. Fuel cells are collected using a player-controlled model (the fuel carrier). The game is won when the player collects all fuel cells and time remains. The game is lost if less than 12 fuel cells are found and time runs out.|
|**Replayability**|The playing field is a basic, level grid with fuel cells scattered at random. In addition, a large amount of barriers are also scattered about the playing field. These barriers are opaque and (eventually) cannot be driven through. The replayability comes from the random placement of the fuel cells and barriers for each new game.|
|**Challenges**|The fuel carrier always starts in the middle of the field, but the fuel cells and barriers are scattered randomly. There are enough barriers to prevent the player from initially seeing all available fuel cells. The barriers have two purposes: slowing the player down and obscuring some of the fuel cells at all times. In some cases, a fair bit of hunting is required by the player to retrieve all fuel cells. In later versions of the game, existing factors could easily be tweaked to increase the difficulty and introduce new sub-goals (such as a power-up allowing a clearer view or a faster vehicle).|

With the FuelCell feature set explained, let us move on to the actual coding of the game. The next step focuses on a critical part of any 3D game – the camera.

## See Also

### Next

- [FuelCell: Setting the Scene](2-FuelCell-Setting-the-scene.md)

### Conceptual

- [FuelCell: Introduction](../README.md)
