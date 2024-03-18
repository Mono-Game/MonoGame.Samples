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

        // Game board setup
        public const int MaxRangeTerrain = 98;
        public const int NumBarriers = 40;
        public const int NumFuelCells = 12;
        public const int MinDistance = 10;
        public const int MaxDistance = 90;

        //bounding sphere scaling factors
        public const float FuelCarrierBoundingSphereFactor = .8f;
        public const float FuelCellBoundingSphereFactor = .5f;
        public const float BarrierBoundingSphereFactor = .7f;
    }
}