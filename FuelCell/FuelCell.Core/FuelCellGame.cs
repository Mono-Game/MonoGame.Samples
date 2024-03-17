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

namespace FuelCell
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FuelCellGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameObject ground;
        private Camera gameCamera;
        Random random;
        FuelCarrier fuelCarrier;
        FuelCell[] fuelCells;
        Barrier[] barriers;

        public FuelCellGame()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            // Initialize the Game objects
            ground = new GameObject();
            gameCamera = new Camera();

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

            ground.Model = Content.Load<Model>("Models/ground");

            // Initialize and place fuel cell
            fuelCells = new FuelCell[1];
            fuelCells[0] = new FuelCell();
            fuelCells[0].LoadContent(Content, "Models/fuelcellmodel");
            fuelCells[0].Position = new Vector3(0, 0, 15);

            // Initialize and place barriers
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

            // Initialize and place fuel carrier
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
            float rotation = 0.0f;
            Vector3 position = Vector3.Zero;
            gameCamera.Update(rotation, position, graphics.GraphicsDevice.Viewport.AspectRatio);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            DrawTerrain(ground.Model);

            // Draw the fuel cell
            fuelCells[0].Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            // Draw the barriers
            foreach (Barrier barrier in barriers)
            {
                barrier.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);
            }

            // Draw the fuel carrier
            fuelCarrier.Draw(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            base.Draw(gameTime);
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
    }
}
