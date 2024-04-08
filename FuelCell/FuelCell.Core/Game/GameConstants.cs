using System;

namespace FuelCell
{
    public class GameConstants
    {
        //camera constants
        public const float NearClip = 1.0f;
        public const float FarClip = 1000.0f;
        public const float ViewAngle = 45.0f;

        //ship constants
        public const float Velocity = 0.75f;
        public const float TurnSpeed = 0.025f;
        public const int MaxRange = 98;
        public const float HeightOffset = 2;

        // Game board setup
        public const int MaxRangeTerrain = 98;
        public const int NumBarriers = 40;
        public const int NumFuelCells = 12;
        public const int MinDistance = 10;
        public const int MaxDistance = 90;

        //bounding sphere scaling factors
        public const float FuelCarrierBoundingSphereFactor = .7f;
        public const float FuelCellBoundingSphereFactor = .5f;
        public const float BarrierBoundingSphereFactor = .7f;

        //Gameplay tracking variables
        public static readonly TimeSpan RoundTime = TimeSpan.FromSeconds(30.25);

        // Display Text
        public const string StrTimeRemaining = "Time Remaining: ";
        public const string StrCellsFound = "Fuel Cells Retrieved: ";
        public const string StrGameWon = "Game Won !";
        public const string StrGameLost = "Game Lost !";
        public const string StrPlayAgain = "Press Enter/Start to play again or Esc/Back to quit";
        public const string StrInstructions1 = "Retrieve all Fuel Cells before time runs out.";
        public const string StrInstructions2 = "Control ship using keyboard (A, D, W, S) or the left thumbstick.";
    }
}