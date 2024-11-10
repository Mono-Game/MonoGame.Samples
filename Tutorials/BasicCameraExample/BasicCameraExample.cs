using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BasicCameraExample;

public class BasicCameraExample : Game
{
    enum CameraMode { Fixed, Tracking, FirstPerson, ThirdPerson, TopDownFixed, TopDownCentred };

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    #region Content
    // Drawing Text font
    private SpriteFont spriteFont;

    // Set the 3D player model to draw.
    private Model myModel;

    // Set the 3D ground model so that we get a sense of movement
    private Model groundModel;

    // Sound effects
    private SoundEffect engineSoundEffect;
    private SoundEffectInstance engineSound;
    private SoundEffect hyperspaceSoundEffect;
    #endregion Content

    #region Model controller properties
    // Set the velocity of the model, applied each frame to the model's position.
    private Vector3 modelVelocity = Vector3.Zero;

    // Set the position of the model in world space, and set the rotation.
    private Vector3 modelPosition = new Vector3(0.0f, 350.0f, 0.0f);

    // The current rotation of the model
    private float modelRotation = 0.0f;

    // The positional matrix for the model
    private Matrix modelWorldPosition;

    // Drag Co-Efficient
    float Drag = 0.97f;
    #endregion Model controller properties

    #region Camera Variables
    // Matrices required to correctly display our scene
    private Matrix currentCameraView;
    private Matrix currentCameraProjection;

    // Set the position of the camera in world space, for the fixed camera view matrix.
    private Vector3 cameraFixedPosition = new Vector3(0.0f, 1550.0f, 5000.0f);

    // 1st Person camera position relative to player model
    private Vector3 cameraFirstPersonPosition = new Vector3(0.0f, 50.0f, 500.0f);

    // 3rd Person camera position relative to player model
    private Vector3 cameraThirdPersonPosition = new Vector3(0.0f, 1550.0f, 5000.0f);

    // Top Down camera position relative to player model
    private Vector3 cameraTopDownPosition = new Vector3(0.0f, 25000.0f, 1.0f);

    // The aspect ratio determines how to scale 3d to 2d projection.
    private float aspectRatio;
    #endregion Camera Variables

    #region Camera Settings
    //Distance from the camera of the near and far clipping planes
    private float nearClip = 10.0f;
    private float farClip = 100000.0f;

    // Field of view of the camera in radians (pi/4 is 45 degrees).
    private float fieldOfView = MathHelper.ToRadians(45.0f);

    private CameraMode currentCameraMode = CameraMode.Fixed;

    // Camera Physics
    private float cameraStiffness = 1800.0f;
    private float cameraDamping = 600.0f;
    private float cameraMass = 50.0f;
    private Vector3 cameraVelocity = Vector3.Zero;
    private Vector3 currentCameraPosition = Vector3.Zero;

    private bool cameraSpringEnabled = true;
    #endregion Camera Settings

    public BasicCameraExample()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Debug.WriteLine("BasicCameraSample Initialize");
        aspectRatio = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;
        currentCameraProjection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        Debug.WriteLine("BasicCameraSample LoadContent");

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        groundModel = Content.Load<Model>("Models/Ground");

        myModel = Content.Load<Model>("Models/p1_wedge");

        spriteFont = Content.Load<SpriteFont>("Fonts/Tahoma");

        engineSoundEffect = Content.Load<SoundEffect>("Audio/Engine_2");
        engineSound = engineSoundEffect.CreateInstance();
        hyperspaceSoundEffect = Content.Load<SoundEffect>("Audio/hyperspace_activate");
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        if (InputManager.IsButtonPressed(Buttons.Back) || InputManager.IsKeyPressed(Keys.Escape))
            Exit();

        // Get some input.
        UpdateInput(gameTime);

        // Add velocity to the current position.
        modelPosition += modelVelocity;

        // Bleed off velocity over time.
        modelVelocity *= Drag;

        // Update the ships world position based on input
        modelWorldPosition = Matrix.CreateRotationY(modelRotation) * Matrix.CreateTranslation(modelPosition);

        switch (currentCameraMode)
        {
            case CameraMode.Fixed:
                UpdateFixedCamera();
                break;

            case CameraMode.Tracking:
                UpdateTrackingCamera();
                break;

            case CameraMode.FirstPerson:
                UpdateFirstPersonCamera();
                break;

            case CameraMode.ThirdPerson:
                UpdateThirdPersonCamera((float)gameTime.ElapsedGameTime.TotalSeconds);
                break;

            case CameraMode.TopDownFixed:
                UpdateTopDownFixedCamera();
                break;

            case CameraMode.TopDownCentred:
                UpdateTopDownCenteredCamera();
                break;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Crucial when drawing 3D models to set the GraphicsDevice to the correct state.
        // Especially when drawing both 3D and 2D/Text in the same scene.
        SetCameraDrawingState();

        // Ground drawn from the center of the scene
        groundModel.Draw(Matrix.Identity, currentCameraView, currentCameraProjection);

        // Ship model drawn from its current position / rotation
        if (currentCameraMode != CameraMode.FirstPerson) // Comment out this line if you want to see the inside of the ship :)
            myModel.Draw(modelWorldPosition, currentCameraView, currentCameraProjection);

        // Draw Help Text and other HUD stuff
        DrawHUD();

        base.Draw(gameTime);
    }

    private void SetCameraDrawingState()
    {
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
    }

    protected void UpdateInput(GameTime aGameTime)
    {
        if (InputManager.IsKeyDown(Keys.Left))
        {
            // Rotate Left
            modelRotation += (float)(aGameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f));
        }
        else if (InputManager.IsKeyDown(Keys.Right))
        {
            // Rotate Right
            modelRotation -= (float)(aGameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f));
        }
        else
        {
            // Rotate the model using the left thumbstick, and scale it down.
            modelRotation -= InputManager.LeftThumbStick.X * 0.10f;
        }

        // Create some velocity if the right trigger is down.
        Vector3 modelVelocityAdd = Vector3.Zero;

        // Find out what direction we should be thrusting, using rotation.
        modelVelocityAdd.X = -(float)Math.Sin(modelRotation);
        modelVelocityAdd.Z = -(float)Math.Cos(modelRotation);

        // Now scale our direction by how hard the trigger is down.
        if (InputManager.IsKeyDown(Keys.Up))
        {
            modelVelocityAdd /= (float)(aGameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f));
        }
        else
        {
            modelVelocityAdd *= InputManager.RightTriggerValue * 10;
        }

        // Finally, add this vector to our velocity.
        modelVelocity += modelVelocityAdd;

        GamePad.SetVibration(PlayerIndex.One, InputManager.RightTriggerValue,
            InputManager.RightTriggerValue);

        // Set some audio based on whether we're pressing a trigger.
        if ((InputManager.RightTriggerValue > 0) || InputManager.IsKeyDown(Keys.Up))
        {
            if (engineSound.State == SoundState.Stopped)
            {
                engineSound = engineSoundEffect.CreateInstance();
                engineSound.IsLooped = true;
                engineSound.Play();
            }
            else if (engineSound.State == SoundState.Paused)
            {
                engineSound.Resume();
            }
        }
        else
        {
            if (engineSound.State == SoundState.Playing)
            {
                engineSound.Pause();
            }
        }

        // In case you get lost, press A to warp back to the center.
        if (InputManager.IsButtonPressed(Buttons.A) || InputManager.IsKeyPressed(Keys.Space))
        {
            modelPosition = new Vector3(0.0f, 350.0f, 0.0f);
            modelVelocity = Vector3.Zero;
            modelRotation = 0.0f;

            // Make a sound when we warp.
            hyperspaceSoundEffect.Play();
        }

        // Toggle the state of the camera.
        if (InputManager.IsButtonPressed(Buttons.LeftShoulder) || InputManager.IsKeyPressed(Keys.Tab))
        {
            currentCameraMode++;
            currentCameraMode = (CameraMode)((int)currentCameraMode % 6);
        }

        // Pressing the A button or key toggles the spring behavior on and off
        if (InputManager.IsButtonPressed(Buttons.A) || InputManager.IsKeyPressed(Keys.A))
        {
            cameraSpringEnabled = !cameraSpringEnabled;
        }
    }

    #region Camera Modes
    /// <summary>
    /// The following methods update the camera view matrix based on the current camera mode.
    /// </summary>

    /// <summary>
    /// Helper method to update the current camera view matrix.
    /// </summary>
    void UpdateCameraView(Vector3 aCameraPosition, Vector3 aCameraTarget)
    {
        currentCameraView = Matrix.CreateLookAt(aCameraPosition, aCameraTarget, Vector3.Up);
    }

    void UpdateFixedCamera()
    {
        // Fixed view, the camera is always in the same position and looking the same way, no updates.
        UpdateCameraView(cameraFixedPosition, Vector3.Zero);
    }

    void UpdateTrackingCamera()
    {
        // Tracking view, the camera is always in the same position but changes the view matrix to "look" towards a target.

        // Set up our world matrix, view matrix and projection matrix.
        UpdateCameraView(cameraFixedPosition, modelPosition);
    }

    void UpdateFirstPersonCamera()
    {
        // First person view, the camera moves based on the Model's position (which is moved by input) and the view matrix is updated to always look "forward" from the model.
        Matrix rotationMatrix = Matrix.CreateRotationY(modelRotation);

        // Create a vector pointing the direction the camera is facing.
        Vector3 transformedReference = Vector3.Transform(cameraFirstPersonPosition, rotationMatrix);

        // Calculate the position the camera is looking from.
        currentCameraPosition = transformedReference + modelPosition;

        // Set up our world matrix, view matrix and projection matrix.
        UpdateCameraView(currentCameraPosition, modelPosition);
    }

    void UpdateThirdPersonCamera(float aElapsed)
    {
        // In Third person view the camera is offset behind and above the model and moves with it,the view matrix is updated to always look "forward" from the model.
        // It also includes an optional spring physics system to smooth out the camera movement.

        Matrix rotationMatrix = Matrix.CreateRotationY(modelRotation);

        // Create a vector pointing the direction the camera is facing.
        Vector3 transformedReference = Vector3.Transform(cameraThirdPersonPosition, rotationMatrix);

        // If camera spring is enabled, update the position and rotation of the camera over several frames
        if (cameraSpringEnabled)
        {
            // Calculate the position where we would like the camera to be looking from.
            Vector3 desiredPosition = transformedReference + modelPosition;

            // Calculate spring force            
            Vector3 stretch = currentCameraPosition - desiredPosition;
            Vector3 force = -cameraStiffness * stretch - cameraDamping * cameraVelocity;

            // Apply acceleration 
            Vector3 acceleration = force / cameraMass;
            cameraVelocity += acceleration * aElapsed;

            // Apply velocity
            currentCameraPosition += cameraVelocity * aElapsed;
        }
        else
        {
            // Calculate the position the camera is looking from.
            currentCameraPosition = transformedReference + modelPosition;
        }

        // Set up our world matrix, view matrix and projection matrix.
        UpdateCameraView(currentCameraPosition, modelPosition);
    }

    void UpdateTopDownFixedCamera()
    {
        // A Top-Down fixed view, the camera is always in the same position and looking down onto the game scene.
        // Note, there are no boundaries to prevent the model from moving out of view.
    
        // Set up our world matrix, view matrix and projection matrix.
        UpdateCameraView(cameraTopDownPosition, Vector3.Zero);
    }

    void UpdateTopDownCenteredCamera()
    {
        // A Top-Down view that moves according to two dimensional position of the model, looking down onto the model.

        Matrix rotationMatrix = Matrix.CreateRotationY(modelRotation);

        // Create a vector pointing the direction the camera is facing.
        Vector3 transformedReference = Vector3.Transform(cameraTopDownPosition, rotationMatrix);

        // Calculate the position the camera is looking from.
        currentCameraPosition = transformedReference + modelPosition;

        // Set up our world matrix, view matrix and projection matrix.
        UpdateCameraView(currentCameraPosition, modelPosition);
    }
    #endregion Camera Modes

    #region Draw Methods
    void DrawHUD()
    {
        _spriteBatch.Begin();

        string Helptext = "Toggle Camera Modes ( " + currentCameraMode.ToString() + " ) = Tab or LeftShoulder Button\n" +
                    "Steer = Left & Right Arrow keys or Left Thumbstick\n" +
                    "Accelerate = Up Arrow key or Right Trigger\n" +
                    "Reset = A Button or Spacebar";


        // Draw the string twice to create a drop shadow, first colored black
        // and offset one pixel to the bottom right, then again in white at the
        // intended position. This makes text easier to read over the background.
        _spriteBatch.DrawString(spriteFont, Helptext, new Vector2(20, 20), Color.Black);
        _spriteBatch.DrawString(spriteFont, Helptext, new Vector2(19, 19), Color.White);

        if (currentCameraMode == CameraMode.FirstPerson)
        {
            string HudText = "Velocity :" + modelVelocity.ToString() + "\n" +
                   "Position :" + modelPosition.ToString() + "\n" +
                   "Rotation :" + modelRotation.ToString() + "\n";

            _spriteBatch.DrawString(spriteFont, HudText, new Vector2(20, 400), Color.Blue);
        }

        _spriteBatch.End();
    }
    #endregion Draw Methods
}