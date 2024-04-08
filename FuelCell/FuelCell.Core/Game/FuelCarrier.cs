using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FuelCell
{
    public class FuelCarrier : GameObject
    {
        public float ForwardDirection { get; set; }
        public int MaxRange { get; set; }
        private Vector3 startPosition = new Vector3(0, GameConstants.HeightOffset, 0);
        private SoundEffect engineRumble;

        public FuelCarrier()
            : base()
        {
            ForwardDirection = 0.0f;
            Position = startPosition;
            MaxRange = GameConstants.MaxRange;
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            Model = content.Load<Model>(modelName);
            BoundingSphere = CalculateBoundingSphere();

            engineRumble = content.Load<SoundEffect>("Audio/engine-rumble");

            BoundingSphere scaledSphere;
            scaledSphere = BoundingSphere;
            scaledSphere.Radius *= GameConstants.FuelCarrierBoundingSphereFactor;
            BoundingSphere = new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
        }

        internal void Reset()
        {
            Position = startPosition;
            ForwardDirection = 0f;
        }

        public void Update(IInputState inputState, Barrier[] barriers)
        {
            Vector3 futurePosition = Position;

            ForwardDirection += inputState.GetPlayerTurn(PlayerIndex.One) * GameConstants.TurnSpeed;
            Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);

            Vector3 speed = Vector3.Transform(inputState.GetPlayerMove(PlayerIndex.One), orientationMatrix);
            if (speed != Vector3.Zero)
            {
                engineRumble.Play();
            }
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
                {
                    return true;
                }
            }
            return false;
        }
    }
}