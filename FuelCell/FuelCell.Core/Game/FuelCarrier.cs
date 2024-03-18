using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FuelCell.Core.Game
{
    public class FuelCarrier : GameObject
    {
        public float ForwardDirection { get; set; }
        public int MaxRange { get; set; }

        public FuelCarrier()
            : base()
        {
            ForwardDirection = 0.0f;
            MaxRange = GameConstants.MaxRange;
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            Model = content.Load<Model>(modelName);
            BoundingSphere = CalculateBoundingSphere();

            BoundingSphere scaledSphere;
            scaledSphere = BoundingSphere;
            scaledSphere.Radius *= GameConstants.FuelCarrierBoundingSphereFactor;
            BoundingSphere = new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
        }

        public void Update(GamePadState gamepadState, KeyboardState keyboardState, Barrier[] barriers)
        {
            Vector3 futurePosition = Position;
            float turnAmount = 0;

            if (keyboardState.IsKeyDown(Keys.A))
            {
                turnAmount = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                turnAmount = -1;
            }
            else if (gamepadState.ThumbSticks.Left.X != 0)
            {
                turnAmount = -gamepadState.ThumbSticks.Left.X;
            }
            ForwardDirection += turnAmount * GameConstants.TurnSpeed;
            Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);

            Vector3 movement = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
            {
                movement.Z = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                movement.Z = -1;
            }
            else if (gamepadState.ThumbSticks.Left.Y != 0)
            {
                movement.Z = gamepadState.ThumbSticks.Left.Y;
            }

            Vector3 speed = Vector3.Transform(movement, orientationMatrix);
            speed *= GameConstants.Velocity;
            futurePosition = Position + speed;

            if (ValidateMovement(futurePosition, barriers))
            {
                Position = futurePosition;

                BoundingSphere updatedSphere;
                updatedSphere = BoundingSphere;

                updatedSphere.Center.X = Position.X;
                updatedSphere.Center.Z = Position.Z;
                BoundingSphere = new BoundingSphere(updatedSphere.Center, updatedSphere.Radius);
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix worldMatrix = Matrix.Identity;
            Matrix rotationYMatrix = Matrix.CreateRotationY(ForwardDirection);
            Matrix translateMatrix = Matrix.CreateTranslation(Position);

            worldMatrix = rotationYMatrix * translateMatrix;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        private bool ValidateMovement(Vector3 futurePosition, Barrier[] barriers)
        {
            BoundingSphere futureBoundingSphere = BoundingSphere;
            futureBoundingSphere.Center.X = futurePosition.X;
            futureBoundingSphere.Center.Z = futurePosition.Z;

            //Do not allow off-terrain driving
            if ((Math.Abs(futurePosition.X) > MaxRange) || (Math.Abs(futurePosition.Z) > MaxRange))
                return false;

            //Do not allow driving through a barrier
            if (CheckForBarrierCollision(futureBoundingSphere, barriers))
            {
                return false;
            }

            return true;
        }

        private bool CheckForBarrierCollision(BoundingSphere vehicleBoundingSphere, Barrier[] barriers)
        {
            for (int curBarrier = 0; curBarrier < barriers.Length; curBarrier++)
            {
                if (vehicleBoundingSphere.Intersects(barriers[curBarrier].BoundingSphere))
                    return true;
            }
            return false;
        }
    }
}