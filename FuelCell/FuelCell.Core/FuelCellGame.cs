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

namespace FuelCell
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FuelCellGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameObject ground;
        private Camera gameCamera;

        public FuelCellGame()
        {
            graphics = new GraphicsDeviceManager(this);

        }

        protected override void Initialize()
        {
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

            base.Draw(gameTime);
        }
    }
}
