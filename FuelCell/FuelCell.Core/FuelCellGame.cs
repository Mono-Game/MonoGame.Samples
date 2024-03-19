#region File Description
//-----------------------------------------------------------------------------
// FuelCellGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using FuelCell.Core.Game;
using System.Threading;

namespace FuelCell
{
    /// <summary>
    /// The available states of the game
    /// </summary>
    public enum GameState { Loading, Running, Won, Lost }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FuelCellGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont statsFont;
        private GameObject ground;
        private Camera gameCamera;
        Random random;
        GameState currentGameState = GameState.Loading;

        // Game objects
        FuelCarrier fuelCarrier;
        FuelCell[] fuelCells;
        Barrier[] barriers;

        // States to store input values
        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        GamePadState currentGamePadState = new GamePadState();

        GameObject boundingSphere;

        int retrievedFuelCells = 0;
        TimeSpan startTime, roundTimer, roundTime;

        public FuelCellGame()
        {
            graphics = new GraphicsDeviceManager(this);
            random = new Random();
            roundTime = GameConstants.RoundTime;
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
        }

        protected override void Initialize()
        {
            // Initialize the Game objects
            ground = new GameObject();
            gameCamera = new Camera();
            boundingSphere = new GameObject();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.Content.RootDirectory = "Content";

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            statsFont = Content.Load<SpriteFont>("Fonts/StatsFont");

            ground.Model = Content.Load<Model>("Models/ground");
            boundingSphere.Model = Content.Load<Model>("Models/sphere1uR");

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
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if (currentGamePadState.Buttons.Start == ButtonState.Pressed)
            {
                roundTimer = roundTime;
            }

            fuelCarrier.Update(currentGamePadState, currentKeyboardState, barriers);
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            gameCamera.Update(fuelCarrier.ForwardDirection, fuelCarrier.Position, aspectRatio);

            foreach (FuelCell fuelCell in fuelCells)
            {
                fuelCell.Update(fuelCarrier.BoundingSphere);
            }

            // Update input from sources, Keyboard and GamePad
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            lastGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // Draw the ground terrain model
            DrawTerrain(ground.Model);

            // Draw the fuel cells on the map
            foreach (FuelCell fuelCell in fuelCells)
            {
                if (!fuelCell.Retrieved)
                {
                    fuelCell.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

                    // Draw the bounding sphere for the fuel cells
                    // ChangeRasterizerState(FillMode.WireFrame);
                    // fuelCell.DrawBoundingSphere(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix, boundingSphere);
                    // ChangeRasterizerState(FillMode.Solid);
                }
            }

            // Draw the barriers on the map
            foreach (Barrier barrier in barriers)
            {
                barrier.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

                // Draw the bounding sphere for the barriers
                // ChangeRasterizerState(FillMode.WireFrame);
                // barrier.DrawBoundingSphere(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix, boundingSphere);
                // ChangeRasterizerState(FillMode.Solid);
            }

            // Draw the player fuelcarrier on the map
            fuelCarrier.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            // Draw the bounding sphere for the fuel carrier
            // ChangeRasterizerState(FillMode.WireFrame);
            // fuelCarrier.DrawBoundingSphere(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix, boundingSphere);
            // ChangeRasterizerState(FillMode.Solid);

            DrawStats();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper function to change the rasterizer state for drawing the wireframe bounding spheres
        /// </summary>
        /// <param name="fillmode">The render `FillMode` to draw with, e.g. WireFrame</param>
        /// <param name="cullMode">The culling mode to draw with, e.g. None</param>
        /// <returns>Returns a new RasterizerState to apply to a graphics device</returns>
        private RasterizerState ChangeRasterizerState(FillMode fillmode, CullMode cullMode = CullMode.None)
        {
            RasterizerState rasterizerState = new RasterizerState()
            { 
                FillMode = fillmode,
                CullMode = cullMode 
            };
            graphics.GraphicsDevice.RasterizerState = rasterizerState;
            return rasterizerState;
        }

        private void DrawTerrain(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = Matrix.Identity;

                    // Use the matrices provided by the game camera
                    effect.View = gameCamera.ViewMatrix;
                    effect.Projection = gameCamera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }

        private void DrawStats()
        {
            float xOffsetText, yOffsetText;
            string str1 = GameConstants.StrTimeRemaining;
            string str2 = GameConstants.StrCellsFound + retrievedFuelCells.ToString() + " of " + GameConstants.NumFuelCells.ToString();
            Rectangle rectSafeArea;

            str1 += (roundTimer.Seconds).ToString();

            //Calculate str1 position
            rectSafeArea = GraphicsDevice.Viewport.TitleSafeArea;

            xOffsetText = rectSafeArea.X;
            yOffsetText = rectSafeArea.Y;

            Vector2 strSize = statsFont.MeasureString(str1);
            Vector2 strPosition = new Vector2(xOffsetText + 10, yOffsetText);

            spriteBatch.Begin();
            spriteBatch.DrawString(statsFont, str1, strPosition, Color.White);
            strPosition.Y += strSize.Y;
            spriteBatch.DrawString(statsFont, str2, strPosition, Color.White);
            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

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

        private bool IsOccupied(int xValue, int zValue)
        {
            foreach (GameObject currentObj in fuelCells)
            {
                if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
                    ((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
                    return true;
            }

            foreach (GameObject currentObj in barriers)
            {
                if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
                    ((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
                    return true;
            }
            return false;
        }
    }
}
