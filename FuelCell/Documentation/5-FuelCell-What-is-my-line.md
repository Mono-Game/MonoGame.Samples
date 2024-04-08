# FuelCell: What is My Line?

## In this article

- [Overview](#overview)
- [Initializing the Random Number Generator](#initializing-the-random-number-generator)
- [New Game Constants](#new-game-constants)
- [Modifying the LoadContent Method](#modifying-the-loadcontent-method)
- [Fuel Cell and Barrier Initialization, Part 2](#fuel-cell-and-barrier-initialization-part-2)
- [Fuel Cell and Barrier Initialization, Part 3](#fuel-cell-and-barrier-initialization-part-3)
- [See Also](#see-also)

Demonstrates a simple, random technique for placing barriers and fuel cells on the playing field.

## Overview

The goal of this topic is the implementation of a good algorithm for placing a set number of barriers and fuel cells randomly throughout the playing field. The valid placement area (on a 100 x 100 unit playing field) is 90 x 90. A barrier centered on this limit does not overhang the playing field. In addition to an outer limit, you need an inner limit. This prevents a situation where the fuel carrier (always placed at the origin of the playing field: 0,0,0) is trapped by barriers. This still would not be a problem at this stage because the game implements no collision detection, but in the finished product it would make for a frustrating game experience. Finally, another limit is used when placing a barrier, which is the minimum distance a new barrier must be from existing barriers that prevents a collision with an existing barrier.

After testing with different values, 12 fuel cells and 40 barriers produces a challenging enough field for most players (but feel free to add a HARD mode later). They are nicely cluttered with a good probability that many fuel cells are initially obscured by one or more barriers. You are encouraged to experiment with these numbers but be warned that a higher total makes the random placement of the barriers more difficult. The game could begin "thrashing," which means it is endlessly generating new random locations (to resolve a collision) only to find the new locations currently occupied.

A good example of unforeseen development problems was the coordinate system of the playing field. Because the playing field origin is at `0, 0, 0`, the `X` (left) and `Z` (forward)axes have both positive and negative values. This "automatically" divides the playing field into four quadrants because the sign can be either positive or negative values for the X and Z coordinates (the `Y` coordinate is clamped to `0` as the player cannot fly, at least for now). Therefore, randomly generating two positive numbers between the minimum and maximum distances populates only one-quarter of the playing field. This is another bad gaming experience!

The solution used by `FuelCell` is to randomly assign positive and negative values to the randomly generated `X` and `Z` coordinates, this decently scatters the barriers around the playing field, however, due to the random nature of coordinate generation and limits imposed by a minimum and maximum. If you observe closely, you will notice that there is a bit of a "corridor" along the X and Z axes.

> [!TIP]
> **Pick Two: High Quality, Fast, and Cheap.**
> You might know (or at least heard) about the [Trilemma](https://en.wikipedia.org/wiki/Trilemma) above. You are given three characteristics, but you can only choose two because of their interdependency. Any two of the characteristics negates the possibility of the third. For example, you can have something made cheaply and quickly, but it will be of poor quality.
>
> Trilemmas can also be applied to software algorithms. A cheap, fast algorithm wastes a lot of resources compared to the result. On the other hand, a high-quality algorithm that is fast is not cheap in terms of resources (in this case, development hours).
>
> The algorithm used by FuelCell is a fast, cheap algorithm. It took very little design time, and code, to implement, and it is as fast as other possible algorithms because the problem set is pretty small. However, the quality is very poor. It performs many unnecessary checks, it uses a brute-force approach when placing a new game object, and the randomness of the playing field is compromised in certain aspects (such as the axes corridors).
>
> Initially, during the early development of this game, a different algorithm was developed after observing the performance of the simple one used here. This algorithm was designed to prevent clustering and to generate an even spread of game objects throughout the playing field.
>
> The solution was to treat the playing field as four separate 2D grids (or quadrants). This approach prevented the axes from fluctuating as the algorithm moved around the playing field grid. Each quadrant was populated individually using nested for loops: a loop for each axis. In addition, each quadrant was allocated a fourth of the total fuel cells and barriers. Once the buckets emptied, the algorithm moved to the next quadrant. As the loops incremented, the algorithm automatically walked through each square in the quadrant (like iterating through a 2D array). As it walked through the possible placement areas, a random number was generated. If the random number matched a simple rule, it called another routine to place an object and zero out a weight counter. If there was no match, the algorithm placed nothing, incremented the weight counter, and moved on to the next grid location. If the weight counter reached a certain level, and pieces still remained for placement, it forced a piece placement. This ensured an even distribution throughout the quadrant.
>
> The secondary function triggered when a piece needed to be placed. A random number (representing a percentile) was generated. Forty percent of the time a fuel cell was placed (if fuel cells remained in the quadrant allocation); otherwise, a barrier was placed (if barriers remained in the quadrant allocation). This matched the frequency of fuel cells to barriers and provided a general distribution. If the object being placed was a barrier, another random number determined the barrier type.
>
> Finally, the algorithm verified that all game objects were placed before exiting the quadrant and that some type of object was placed when required.
>
> The end result was a consistently populated playing field that hardly ever had clusters or axes corridors. However, when this tutorial was written, the algorithm was overkill and introduced needless complexity to the main purpose: designing a 3D game. Therefore, a fast and cheap algorithm was used, making the code easier to understand.

## Initializing the Random Number Generator

Since we are going to randomly populate the playing field with game objects, an obvious first step is to set up a [random number generator](https://learn.microsoft.com/en-us/dotnet/api/system.random?view=net-8.0). Let us keep it simple and declare a file-level random variable (in `FuelCellGame.cs`) that can be accessed by any `FuelCellGame` method.

After the `input` state declarations, add the following:

```csharp
private Random random;
```

Initialize the random number generator in the FuelCellGame `constructor`:

```csharp
random = new Random();
```

## New Game Constants

Before we continue, you need to add some additional constants to the `GameConstants.cs` file. Add the following code after any existing constants in the GameConstants class:

```csharp
// Game board setup
public const int MaxRangeTerrain = 98;
public const int NumBarriers = 40;
public const int NumFuelCells = 12;
public const int MinDistance = 10;
public const int MaxDistance = 90;
```

These provide a few extra settings to control the generation of barriers and the distances on the game board.

## Modifying the LoadContent Method

In [FuelCell: What is My Motivation](4-FuelCell-What-is-my-motivation.md), we added some temporary code that created the `FuelCell` models on the playing field. You will remove that code now, and initialize the arrays properly.

Remove the following from the `LoadContent` method:

```csharp
//Initialize and place fuel cell
fuelCells = new FuelCell[1];
fuelCells[0] = new FuelCell();
fuelCells[0].LoadContent(Content, "Models/fuelcellmodel");
fuelCells[0].Position = new Vector3(0, 0, 15);

//Initialize and place barriers
barriers = new Barrier[3];

barriers[0] = new Barrier();
barriers[0].LoadContent(Content, "Models/cube10uR");
barriers[0].Position = new Vector3(0, 0, 30);
barriers[1] = new Barrier();
barriers[1].LoadContent(Content, "Models/cylinder10uR");
barriers[1].Position = new Vector3(15, 0, 30);
barriers[2] = new Barrier();
barriers[2].LoadContent(Content, "Models/pyramid10uR");
barriers[2].Position = new Vector3(-15, 0, 30);

//Initialize and place fuel carrier
fuelCarrier = new FuelCarrier();
fuelCarrier.LoadContent(Content, "Models/fuelcarrier");
```

Add replace it with this code, still in the `LoadContent` method, after the loading of the `ground` game asset:

```csharp
//Initialize fuel cells
fuelCells = new FuelCell[GameConstants.NumFuelCells];
for (int index = 0; index < fuelCells.Length; index++)
{
    fuelCells[index] = new FuelCell();
    fuelCells[index].LoadContent(Content, "Models/fuelcellmodel");
}

//Initialize barriers
barriers = new Barrier[GameConstants.NumBarriers];
int randomBarrier = random.Next(3);
string barrierName = null;

for (int index = 0; index < barriers.Length; index++)
{
    switch (randomBarrier)
    {
        case 0:
            barrierName = "Models/cube10uR";
            break;
        case 1:
            barrierName = "Models/cylinder10uR";
            break;
        case 2:
            barrierName = "Models/pyramid10uR";
            break;
    }
    barriers[index] = new Barrier();
    barriers[index].LoadContent(Content, barrierName);
    randomBarrier = random.Next(3);
}

PlaceFuelCellsAndBarriers();

//Initialize fuel carrier
fuelCarrier = new FuelCarrier();
fuelCarrier.LoadContent(Content, "Models/fuelcarrier");
```

Let us examine this code before moving on.

The first block initializes the array of fuel cells, loading each with the model for the fuel cell.

Barrier initialization is next. This code is a bit more complicated because there are three available barrier models. This looks like a job for the random number variable! A random number is generated and the corresponding model is loaded into the current barrier element using a [switch](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression) statement. The rest of the barrier object is initialized and a new random number is generated before moving to the next array element. After barrier initialization, the fuel cells and barriers are placed on the playing field with a call to `PlaceFuelCellsAndBarriers`, which we will discuss in detail more detail in the next section later.

The final bit of code initializes and loads the model for the fuel carrier.

## Fuel Cell and Barrier Initialization, Part 2

Now it is time to take a closer look at the `PlaceFuelCellsAndBarriers` method and its helper method, `GenerateRandomPosition`.

Add the following code to the game (after the `DrawTerrain` method) in the `FuelCellGame.cs` class, and then we will walk through it.

```csharp
private void PlaceFuelCellsAndBarriers()
{
    int min = GameConstants.MinDistance;
    int max = GameConstants.MaxDistance;
    Vector3 tempCenter;

    //place fuel cells
    foreach (FuelCell cell in fuelCells)
    {
        cell.Position = GenerateRandomPosition(min, max);
        tempCenter = cell.BoundingSphere.Center;
        tempCenter.X = cell.Position.X;
        tempCenter.Z = cell.Position.Z;
        cell.BoundingSphere = new BoundingSphere(tempCenter, cell.BoundingSphere.Radius);
        cell.Retrieved = false;
    }

    //place barriers
    foreach (Barrier barrier in barriers)
    {
        barrier.Position = GenerateRandomPosition(min, max);
        tempCenter = barrier.BoundingSphere.Center;
        tempCenter.X = barrier.Position.X;
        tempCenter.Z = barrier.Position.Z;
        barrier.BoundingSphere = new BoundingSphere(tempCenter, barrier.BoundingSphere.Radius);
    }
}

private Vector3 GenerateRandomPosition(int min, int max)
{
    int xValue, zValue;
    do
    {
        xValue = random.Next(min, max);
        zValue = random.Next(min, max);
        if (random.Next(100) % 2 == 0)
            xValue *= -1;
        if (random.Next(100) % 2 == 0)
            zValue *= -1;

    } while (IsOccupied(xValue, zValue));

    return new Vector3(xValue, 0, zValue);
}
```

It is not complicated, but it is also one of the bigger functions in the game. First, a few variables are declared, making the code more reader-friendly. The next part is a foreach loop that places the fuel cells.

The algorithm is as follows:

1. Generate random values for the X and Z coordinates, verify that the new random location is not already occupied, and update the fuel cell position with this new position.

    Possible values are limited by the minimum and maximum placement values (defined in `GameConstants.cs`).

2. Initialize the bounding sphere property to the current fuel cell location.

3. Mark the fuel cell as un-retrieved.

You follow the same process when you place the barriers.

> [!NOTE]
> We will cover [Collision Detection](https://happycoding.io/tutorials/processing/collision-detection) and the use of the BoundingSpheres in the next section [FuelCell: Ships passing in the night](6-FuelCell-Ships-passing-in-the-night.md), for now just enjoy the code.

The `GenerateRandomPosition` helper method makes up the remaining portion of the newly added code. This method generates two random numbers, then another random number is generated and depending on the result of the modulus operation (50% chance of negation), the `X` coordinate is negated. The same is done for the `Z` coordinate. The new position is then checked for existing occupants. If occupied, a new position is generated and the loop continues until a vacant location is found.

Let us add the new helper method, `IsOccupied`, to the project next.

## Fuel Cell and Barrier Initialization, Part 3

To complete this part of the sample, we just need to add a helper method after the `PlaceFuelCellsAndBarriers` method, its job is just to check we are not placing any content on top of an existing object:

```csharp
private bool IsOccupied(int xValue, int zValue)
{
    foreach (GameObject currentObj in fuelCells)
    {
        if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
            ((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
            {
                return true;
            }
    }

    foreach (GameObject currentObj in barriers)
    {
        if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
            ((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
            {
                return true;
            }
    }
    return false;
}
```

This method uses the nifty [Distance](https://monogame.net/api/Microsoft.Xna.Framework.MathHelper.html#Microsoft_Xna_Framework_MathHelper_Distance_System_Single_System_Single_) method when checking for collision with an existing game object (fuel cell or barrier). As you can see from the code, if the new object is closer than 15 units from an existing object, it is not placed. This can be modified, but keep in mind that the higher the distance, the more the placement method churns. It is just a cheap but fast effect to check if things overlap, a more complex process could be used but, as by definition, it would be more complex and this just "does the job".

The final change to `FuelCellGame.cs` occurs in the `Draw` method. The new code draws all our wonderful fuel cells and barriers.

Modify the `Draw` method to match the following:

```csharp
protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);

    // Draw the ground terrain model
    DrawTerrain(ground.Model);

    // Draw the fuel cells on the map
    foreach (FuelCell fuelCell in fuelCells)
    {
        fuelCell.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);
    }

    // Draw the barriers on the map
    foreach (Barrier barrier in barriers)
    {
        barrier.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);
    }

    // Draw the player fuelcarrier on the map
    fuelCarrier.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

    base.Draw(gameTime);
}
```

After a successful rebuild of the project, you now have fuel cells to find and barriers to avoid. Go ahead and take a spin around the new digs before moving on to adding a critical game feature: collision detection.

![Game Status](Images/05-01-final.gif)

## See Also

### Next

- [FuelCell: "Ships" Passing in the Night](6-FuelCell-Ships-passing-in-the-night.md)

### Conceptual

- [FuelCell: Introduction](../README.md)
