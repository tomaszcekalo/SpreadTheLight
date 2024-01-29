using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monolights;
using Penumbra;
using SpreadTheLight.Components;
using SpreadTheLight.Systems;
using System;
using System.Collections.Generic;
using Undine.Core;
using Undine.DefaultEcs;
using Undine.MonoGame;
using Undine.MonoGame.Primitives2D;
using Undine.VelcroPhysics.MonoGame;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Utilities;

namespace SpreadTheLight
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ISystem _primitives2DSystem;
        private VelcroPhysicsSystem _velcroPhysicsSystem;
        private Monolights.Monolights _monoLights;
        private Effect _lightEffect;
        private Effect _deferrectLightEffect;
        private Texture2D _diffuse;
        private Texture2D _normal;
        private RenderTarget2D _frameBuffer;

        //private RenderTarget2D _frameBuffer2;
        private float _spotRotation;

        private Monolights.PointLight _pointlight;
        private Monolights.PointLight _floatinglight;
        private SpotLight _spotlight;
        private Rectangle _monolightsRectangle = new Rectangle(0, 0, 640, 360);
        private Texture2D _idleCaveExplorer;
        private Texture2D _crate;
        private Texture2D _hurtCaveExplorer;
        private Texture2D _walkingCaveExplorer;
        private Effect _fileEffect;
        private SpriteAnimationComponent _idleAnimation;
        private Texture2D _attackCaveExplorer;
        private Texture2D _deathCaveExplorer;
        private EcsContainer _ecsContainer;
        private GameTimeProvider _drawGameTimeProvider;
        private ISystem _spriteAnimationSystem;
        private ISystem _normalAnimationSystem;
        private IUnifiedEntity _box;
        private IUnifiedEntity _player;
        private GameTimeProvider _updateGameTimeProvider;
        private bool _drawDebugTargets;
        private ScaleProvider _scaleProvider;

        // Store reference to lighting system.
        private PenumbraComponent penumbra;

        // Create sample light source and shadow hull.
        private Penumbra.Light light = new Penumbra.PointLight
        {
            Scale = new Vector2(1000f), // Range of the light source (how far the light will travel)
            ShadowType = ShadowType.Solid // Will not lit hulls themselves
        };

        private Hull _hull;

        private Matrix _scale;
        private KeyboardState _previousState;
        private SpriteAnimationComponent _boxAnimation;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.PreferredBackBufferWidth = _monolightsRectangle.Width;
            _graphics.PreferredBackBufferHeight = _monolightsRectangle.Height;
            Content.RootDirectory = "Content";
            //IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            // Initialize the lighting system.
            penumbra.Initialize();
        }

        protected override void LoadContent()
        {
            float meterInPixels = 16;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(meterInPixels);

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _ecsContainer = new DefaultEcsContainer();
            _primitives2DSystem = _ecsContainer.GetSystem(new Primitives2DSystem()
            {
                SpriteBatch = _spriteBatch
            });
            _velcroPhysicsSystem = new VelcroPhysicsSystem();
            _ecsContainer.AddSystem(_velcroPhysicsSystem);
            _ecsContainer.AddSystem(new VelcroPhysicsTransformSystem());
            _drawGameTimeProvider = new GameTimeProvider();
            _updateGameTimeProvider = new GameTimeProvider();
            _spriteAnimationSystem = _ecsContainer.GetSystem(new SpriteAnimationSystem(_spriteBatch, _drawGameTimeProvider));
            _normalAnimationSystem = _ecsContainer.GetSystem(new NormalAnimationSystem(_spriteBatch));

            var physicsEntity = _ecsContainer.CreateNewEntity();
            var physicsWorld = new World(new Vector2(0, 10));
            physicsEntity.AddComponent(new VelcroWorldComponent()
            {
                World = physicsWorld
            });
            // TODO: use this.Content to load your game content here
            _lightEffect = Content.Load<Effect>("LightEffect");
            _deferrectLightEffect = Content.Load<Effect>("DeferredLightEffect");
            _monoLights = new Monolights.Monolights(GraphicsDevice, _lightEffect, _deferrectLightEffect);
            // --
            //_graphics.PreferredBackBufferWidth = 1280;
            //_graphics.PreferredBackBufferHeight = 720;
            //var scale = 2;
            //var fitsScreen = false;
            // --
            var widthUnits = this.GraphicsDevice.Adapter.CurrentDisplayMode.Width / _monolightsRectangle.Width;
            var heightUnits = this.GraphicsDevice.Adapter.CurrentDisplayMode.Height / _monolightsRectangle.Height;
            var scale = Math.Min(widthUnits, heightUnits);
            bool fitsScreen = this.GraphicsDevice.Adapter.CurrentDisplayMode.Width % _monolightsRectangle.Width == 0
                && this.GraphicsDevice.Adapter.CurrentDisplayMode.Height % _monolightsRectangle.Height == 0
                && widthUnits == heightUnits;
            // --
            _scale = Matrix.CreateScale(scale);
            _graphics.PreferredBackBufferWidth = _monolightsRectangle.Width * scale;
            _graphics.PreferredBackBufferHeight = _monolightsRectangle.Height * scale;
            _graphics.IsFullScreen = fitsScreen;
            _graphics.ApplyChanges();
            _scaleProvider = new ScaleProvider() { Scale = scale };
            _ecsContainer.AddSystem(new TransformHullPrimitivesSystem()
            {
                ScaleProvider = _scaleProvider
            });

            LoadBrick();

            _frameBuffer = new RenderTarget2D(this.GraphicsDevice, _monolightsRectangle.Width, _monolightsRectangle.Height);
            //_frameBuffer2 = new RenderTarget2D(this.GraphicsDevice, _monolightsRectangle.Width, _monolightsRectangle.Height);
            _monoLights.InvertYNormal = false; //this normalmap has the Y normal in the usual direction.
            _attackCaveExplorer = Content.Load<Texture2D>("Cave Explorer (Animated Pixel Art)\\Animation Sprite Sheets (PNG)\\AttackCaveExplorer-Sheet");
            _deathCaveExplorer = Content.Load<Texture2D>("Cave Explorer (Animated Pixel Art)\\Animation Sprite Sheets (PNG)\\DeathCaveExplorer-Sheet");
            _idleCaveExplorer = Content.Load<Texture2D>("Cave Explorer (Animated Pixel Art)\\Animation Sprite Sheets (PNG)\\IdleCaveExplorer-Sheet");
            _crate = Content.Load<Texture2D>("sCrate");
            Color[] tcolor = new Color[_idleCaveExplorer.Width * _idleCaveExplorer.Height];
            _idleCaveExplorer.GetData<Color>(tcolor);
            var idleNormals = new Texture2D(_idleCaveExplorer.GraphicsDevice, _idleCaveExplorer.Width, _idleCaveExplorer.Height);
            for (int i = 0; i < tcolor.Length; i++)
            {
                if (tcolor[i].A > 0)
                {
                    tcolor[i].R = 128;
                    tcolor[i].G = 127;
                    tcolor[i].B = 255;
                }
            }
            idleNormals.SetData<Color>(tcolor);
            //idleNormals.SaveAsPng(new FileStream("idleNormals.png", FileMode.Create), _idleCaveExplorer.Width,_idleCaveExplorer.Height);
            _hurtCaveExplorer = Content.Load<Texture2D>("Cave Explorer (Animated Pixel Art)\\Animation Sprite Sheets (PNG)\\HurtCaveExplorer-Sheet");
            _walkingCaveExplorer = Content.Load<Texture2D>("Cave Explorer (Animated Pixel Art)\\Animation Sprite Sheets (PNG)\\WalkingCaveExplorer-Sheet");
            _fileEffect = Content.Load<Effect>("File");

            _idleAnimation = new SpriteAnimationComponent()
            {
                FPS = 12,
                Frames = new List<SpriteComponent>()
            };
            for (int i = 0; i < 10; i++)
            {
                _idleAnimation.Frames.Add(new SpriteComponent(_idleCaveExplorer, new Rectangle(i * 48, 0, 32, 32)));
            }
            _hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
            {
                Position = new Vector2(32, 128),
                Scale = new Vector2(48f),//czyli 100
                Origin = new Vector2(0f),
            };
            penumbra = new PenumbraComponent(this);
            penumbra.Lights.Add(light);
            penumbra.Hulls.Add(_hull);
            _box = _ecsContainer.CreateNewEntity();
            _box.AddComponent(new VelcroPhysicsComponent()
            {
                Body = VelcroPhysics.Factories.BodyFactory.CreateRectangle(physicsWorld,
                ConvertUnits.ToSimUnits(48),
                ConvertUnits.ToSimUnits(48),
                123,
                ConvertUnits.ToSimUnits(_hull.Position),
                0, BodyType.Static)
            });
            _box.AddComponent(new HullComponent()
            {
                Hull = _hull,
                Scale = new Vector2(48, 48)
            });
            //_box.AddComponent(new Primitives2DComponent()
            //{
            //    Color = Color.Black,
            //    DrawType = Primitives2DDrawType.FillRectangle,
            //    Size = new Vector2(48, 48) // czyli razy dwa
            //});
            _box.AddComponent(new TransformComponent()
            {
                Origin = new Vector2(24, 24),
                Position = new Vector2(240, 120), // czyli razy dwa
                Scale = new Vector2(1, 1)
            });
            _boxAnimation = new SpriteAnimationComponent()
            {
                FPS = 1,
                Frames = new List<SpriteComponent>()
            };

            _boxAnimation.Frames.Add(new SpriteComponent(_crate, new Rectangle(0, 0, 48, 48)));

            _box.AddComponent(_boxAnimation);
            _box.AddComponent(new ColorComponent { Color = Color.White });

            _player = _ecsContainer.CreateNewEntity();
            var playerBody =  VelcroPhysics.Factories.BodyFactory.CreateRectangle(physicsWorld,
                ConvertUnits.ToSimUnits(32),
                ConvertUnits.ToSimUnits(32),
                123,
                ConvertUnits.ToSimUnits(new Vector2(16, 16)),
                0, BodyType.Dynamic);
            playerBody.FixedRotation = true;
            _player.AddComponent(new VelcroPhysicsComponent()
            {
                Body = playerBody
            });
            _player.AddComponent(_idleAnimation);
            _player.AddComponent(new ColorComponent() { Color = Color.White });
            _player.AddComponent(new TransformComponent()
            {
                Origin = new Vector2(16f, 16f),
                Position = new Vector2(16, 16),
                Rotation = 0,
                Scale = new Vector2(1, 1)
            });

            //Create a few lights:
            _spotlight = new SpotLight()
            {
                IsEnabled = false,
                Color = Color.White,
                Power = 0.5f,
                LightDecay = 100,
                Position = new Vector3(100, 100, 20),
                SpotBeamWidthExponent = 9,
            };
            _spotlight.DirectionZ = -0.25f; //point it slightly more steeper onto the surface.
            _spotRotation = MathHelper.PiOver2; //point the spot downwards.

            _pointlight = new Monolights.PointLight()
            {
                Color = Color.White,
                Power = 0.5f,
                LightDecay = 300,
                Position = new Vector3(0, 0, 20),
                IsEnabled = true
            };

            //This is a light, gently floating over the background.
            _floatinglight = new Monolights.PointLight()
            {
                Color = Color.Orange,
                Power = 0.5f,
                LightDecay = 100,
                Position = new Vector3(0, 0, 20),
                IsEnabled = true
            };

            //Add the lights to the scene.
            _monoLights.AddLight(_spotlight);
            _monoLights.AddLight(_pointlight);
            //_monoLights.AddLight(_floatinglight);
        }

        private void LoadBrick()
        {
            _diffuse = Content.Load<Texture2D>("brickwall"); //The diffuse map.
            _normal = Content.Load<Texture2D>("brickwall_normal"); //The normal map.
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            _updateGameTimeProvider.GameTime = gameTime;
            _velcroPhysicsSystem.ElapsedGameTimeTotalSeconds = (float)_updateGameTimeProvider.GameTime.ElapsedGameTime.TotalSeconds;

            // TODO: Add your update logic here
            _ecsContainer.Run();

            ref var pos = ref _player.GetComponent<TransformComponent>();
            ref var physics=ref _player.GetComponent<VelcroPhysicsComponent>();
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                //pos.Position += new Vector2(1, 0);
                physics.Body.LinearVelocity = new Vector2(1, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                //pos.Position -= new Vector2(1, 0);
                physics.Body.LinearVelocity = new Vector2(-1, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                pos.Position += new Vector2(0, 1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                pos.Position -= new Vector2(0, 1);
            }
            light.Position = Vector2.Transform(pos.Position, _scale);
            //pos.Position * GraphicsDevice.PresentationParameters.Bounds.Width / 320;
            _pointlight.Position = new Vector3(pos.Position, 20);

            if (_previousState.IsKeyUp(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.R))
            {
                LoadBrick();
            }
            _previousState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _drawGameTimeProvider.GameTime = gameTime;

            //base.Draw(gameTime);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //First we render the game, as one would normally.
            //the rendertarget is the one in the Monolights class, it is used to process the light effect later.
            GraphicsDevice.SetRenderTarget(_monoLights.Colormap);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap
                //, effect: _fileEffect
                );
            //_spriteBatch.Draw(_diffuse, _diffuse.Bounds, Color.White);
            _spriteBatch.Draw(_diffuse, _monolightsRectangle, _monolightsRectangle, Color.White);
            _primitives2DSystem.ProcessAll();

            _spriteAnimationSystem.ProcessAll();
            _spriteBatch.End();
            //_monoLights.Colormap.SaveAsPng(new System.IO.FileStream("colormap.png", System.IO.FileMode.Create), _monoLights.Colormap.Width, _monoLights.Colormap.Height);

            //Next we draw the game again, except the graphics use the normalmap data.
            GraphicsDevice.SetRenderTarget(_monoLights.Normalmap);
            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
            //_spriteBatch.Draw(_normal, _normal.Bounds, Color.White);
            _spriteBatch.Draw(_normal, _monolightsRectangle, _monolightsRectangle, Color.White);
            _normalAnimationSystem.ProcessAll();
            _spriteBatch.End();

            //Finally draw the combined scene.
            //the rendertarget is now 'null' to draw to the backbuffer. You can also draw to a rendertarget of your own if you want to postprocess it.
            _monoLights.Draw(_frameBuffer, _spriteBatch);
            //_frameBuffer.SaveAsPng(new System.IO.FileStream("framebuffer.png", System.IO.FileMode.Create), _frameBuffer.Width, _frameBuffer.Height);

            //Debug: show the rendertargets in the Monolights class.
            if (_drawDebugTargets)
                _monoLights.DrawDebugRenderTargets(_spriteBatch);

            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            penumbra.BeginDraw();
            //_spriteBatch.Begin(samplerState: SamplerState.PointClamp/*, transformMatrix: Matrix.CreateScale(6)*/);
            //_spriteBatch.Draw(_frameBuffer, GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _scale);
            _spriteBatch.Draw(_frameBuffer, _frameBuffer.Bounds, Color.White);
            _spriteBatch.End();//*/
            penumbra.Draw(gameTime);
        }
    }
}