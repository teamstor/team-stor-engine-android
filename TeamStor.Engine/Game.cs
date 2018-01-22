using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TeamStor.Engine.Graphics;
using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Engine
{
    /// <summary>
    /// Main game class.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GameState _state;
        private GameState _initialState;

        private double _lastUpdateTime;
        private double _accumTime;

        private long _framesSinceLastFpsReset;
        private double _accumTimeSinceLastFpsReset;
        
        private GraphicsDeviceManager _graphicsDeviceManager;

        /// <summary>
        /// The amount of time passed since the game started.
        /// </summary>
        public double Time
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of time since the current and last update.
        /// </summary>
        public double DeltaTime
        {
            get
            {
                return Time - _lastUpdateTime;
            }
        }

        /// <summary>
        /// The total amount of updates that occured since the game started.
        /// </summary>
        public long TotalUpdates
        {
            get;
            private set;
        }

        /// <summary>
        /// The total amount of fixed updates that occured since the game started.
        /// </summary>
        public long TotalFixedUpdates
        {
            get;
            private set;
        }

        /// <summary>
        /// Frames per second.
        /// </summary>
        public long FPS
        {
            get;
            private set;
        }

        /// <summary>
        /// If FPS should be drawn on screen.
        /// Toggle with F5.
        /// </summary>
        public bool DrawFPS
        {
            get;
            private set;
        }

        /// <summary>
        /// The default sprite batch used by the game.
        /// </summary>
        public SpriteBatch Batch
        {
            get;
            private set;
        }

        /// <summary>
        /// Current game state.
        /// </summary>
        public GameState CurrentState
        {
            get
            {
                return _state;
            }
            set
            {
                if(_state != null)
                    _state.OnLeave(value);
                
                if(OnStateChange != null)
                    OnStateChange(this, new ChangeStateEventArgs(_state, value));

                if(value != null)
                {
                    value.Game = this;
                    value.OnEnter(_state);
                }
                
                _state = value;
            }
        }

        /// <summary>
        /// The number of fixed updates per second.
        /// </summary>
        public double FixedUpdatesPerSecond = 60;

        /// <summary>
        /// The assets manager.
        /// </summary>
        public AssetsManager Assets
        {
            get;
            private set;
        }

        /// <summary>
        /// The input manager.
        /// </summary>
        public InputManager Input
        {
            get;
            private set;
        }

        public class DefaultFontsCollection
        {
            /// <summary>
            /// FreeSans
            /// </summary>
            public Font Normal;
            
            /// <summary>
            /// FreeSans bold
            /// </summary>
            public Font Bold;
            
            /// <summary>
            /// FreeSans italic
            /// </summary>
            public Font Italic;
            
            /// <summary>
            /// FreeSans bold italic
            /// </summary>
            public Font ItalicBold;
            
            /// <summary>
            /// Bitstream Vera Sans Mono
            /// </summary>
            public Font Mono;
            
            /// <summary>
            /// Bitstream Vera Sans Bold
            /// </summary>
            public Font MonoBold;
        }

        /// <summary>
        /// Default fonts used by the engine.
        /// </summary>
        public DefaultFontsCollection DefaultFonts;

        public class UpdateEventArgs : EventArgs
        {
            public UpdateEventArgs(double deltaTime, double totalTime, long count)
            {
                DeltaTime = deltaTime;
                TotalTime = totalTime;
                Count = count;
            }
            
            /// <summary>
            /// The amount of time since the last frame, in seconds.
            /// </summary>
            public double DeltaTime;
            
            /// <summary>
            /// The total amount of time (in seconds) since the game started.
            /// </summary>
            public double TotalTime;
            
            /// <summary>
            /// The total amount of updates since the game started.
            /// </summary>
            public long Count;
        }

        /// <summary>
        /// Called once every frame, before state update.
        /// </summary>
        public event EventHandler<UpdateEventArgs> OnUpdateBeforeState;
        
        /// <summary>
        /// Called once every frame, after state update.
        /// </summary>
        public event EventHandler<UpdateEventArgs> OnUpdateAfterState;
        
        public class FixedUpdateEventArgs : EventArgs
        {
            public FixedUpdateEventArgs(long count)
            {
                Count = count;
            }
            
            /// <summary>
            /// The total amount of fixed updates since the game started.
            /// </summary>
            public long Count;
        }

        /// <summary>
        /// Called 60 (change with <code>FixedUpdatesPerSecond</code>) times a second, before state update.
        /// </summary>
        public event EventHandler<FixedUpdateEventArgs> OnFixedUpdateBeforeState;
        
        /// <summary>
        /// Called 60 (change with <code>FixedUpdatesPerSecond</code>) times a second, after state update.
        /// </summary>
        public event EventHandler<FixedUpdateEventArgs> OnFixedUpdateAfterState;
        
        public class ChangeStateEventArgs : EventArgs
        {
            public ChangeStateEventArgs(GameState from, GameState to)
            {
                From = from;
                To = to;
            }

            /// <summary>
            /// The previous state, can be null.
            /// </summary>
            public GameState From;
            
            /// <summary>
            /// The next state, can be null.
            /// </summary>
            public GameState To;
        }
        
        /// <summary>
        /// Called when the game state changes.
        /// </summary>
        public event EventHandler<ChangeStateEventArgs> OnStateChange;

        /// <summary>
        /// If v-sync is enabled.
        /// </summary>
        public bool VSync
        {
            get { return _graphicsDeviceManager.SynchronizeWithVerticalRetrace; }
            set
            {
                if(_graphicsDeviceManager.SynchronizeWithVerticalRetrace != value)
                {
                    _graphicsDeviceManager.SynchronizeWithVerticalRetrace = value;
                    _graphicsDeviceManager.ApplyChanges();
                }
            }
        }
        
        /// <summary>
        /// If the game window is fullscreen.
        /// </summary>
        public bool Fullscreen
        {
            get { return _graphicsDeviceManager.IsFullScreen; }
            set
            {
                if(_graphicsDeviceManager.IsFullScreen != value)
                {
                    _graphicsDeviceManager.IsFullScreen = value;
                    
                    if(_graphicsDeviceManager.IsFullScreen)
                    {
                        _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                        _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                    }
                    else
                    {
                        _graphicsDeviceManager.PreferredBackBufferWidth = 960;
                        _graphicsDeviceManager.PreferredBackBufferHeight = 540;
                    }
                    
                    _graphicsDeviceManager.ApplyChanges();
                }
            }
        }
        
        /// <param name="initialState">The state to start the game on.</param>
        /// <param name="assetsDir">The assets directory.</param>
        /// <param name="showTeamStorLogo">If the Team Stor logo should be shown before starting the initial state.</param>
        public Game(GameState initialState, string assetsDir = "data", bool showTeamStorLogo = true)
        {
            Assets = new AssetsManager(this, assetsDir);
            Input = new InputManager(this);
            
            if(showTeamStorLogo)
                _initialState = new Internal.TeamStorLogoState(initialState);
            else
                _initialState = initialState;

            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
            
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;
            _graphicsDeviceManager.HardwareModeSwitch = false;
        }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.IsFullScreen = false;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 960;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 540;
            e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.Immediate;
        }

        protected override void LoadContent()
        {            
            Batch = new SpriteBatch(this);
            
            DefaultFonts.Normal = new Font(GraphicsDevice, Assets.Directory + "/engine/FreeSans.ttf");
            DefaultFonts.Bold = new Font(GraphicsDevice, Assets.Directory + "/engine/FreeSansBold.ttf");
            DefaultFonts.Italic = new Font(GraphicsDevice, Assets.Directory + "/engine/FreeSansOblique.ttf");
            DefaultFonts.ItalicBold = new Font(GraphicsDevice, Assets.Directory + "/engine/FreeSansBoldOblique.ttf");
            DefaultFonts.Mono = new Font(GraphicsDevice, Assets.Directory + "/engine/VeraMono.ttf");
            DefaultFonts.MonoBold = new Font(GraphicsDevice, Assets.Directory + "/engine/VeraMoBd.ttf");

            CurrentState = _initialState;
        }

        protected override void UnloadContent()
        {
            Assets.Dispose();
            
            DefaultFonts.Normal.Dispose();
            DefaultFonts.Bold.Dispose();
            DefaultFonts.Italic.Dispose();
            DefaultFonts.ItalicBold.Dispose();
            DefaultFonts.Mono.Dispose();
            DefaultFonts.MonoBold.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            Time = gameTime.TotalGameTime.TotalSeconds;

            if(_lastUpdateTime == 0)
                _lastUpdateTime = Time;

            if(CurrentState != null)
            {
                if(OnUpdateBeforeState != null)
                    OnUpdateBeforeState(this, new UpdateEventArgs(DeltaTime, Time, TotalUpdates));
                
                CurrentState.Update(DeltaTime, Time, TotalUpdates);
                TotalUpdates++;
                
                if(OnUpdateAfterState != null)
                    OnUpdateAfterState(this, new UpdateEventArgs(DeltaTime, Time, TotalUpdates));
            }
            
            if(Input.KeyPressed(Keys.F4))
                Fullscreen = !Fullscreen;

            if(Input.KeyPressed(Keys.F5))
                DrawFPS = !DrawFPS;

            _accumTime += DeltaTime;
            _framesSinceLastFpsReset++;
            _accumTimeSinceLastFpsReset += DeltaTime;

            if(_accumTimeSinceLastFpsReset >= 1.0)
            {
                FPS = _framesSinceLastFpsReset;
                _framesSinceLastFpsReset = 0;
                _accumTimeSinceLastFpsReset -= 1.0;
            }
            
            while(_accumTime >= (1.0 / FixedUpdatesPerSecond))
            {
                if(CurrentState != null)
                {
                    if(OnFixedUpdateBeforeState != null)
                        OnFixedUpdateBeforeState(this, new FixedUpdateEventArgs(TotalFixedUpdates));

                    CurrentState.FixedUpdate(TotalFixedUpdates);
                    TotalFixedUpdates++;
                    
                    if(OnFixedUpdateAfterState != null)
                        OnFixedUpdateAfterState(this, new FixedUpdateEventArgs(TotalFixedUpdates));
                }

                _accumTime -= (1.0 / FixedUpdatesPerSecond);
            }

            _lastUpdateTime = Time;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.1f, 0.1f, 0.1f));
            
            Vector2 viewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            if(CurrentState != null)
                CurrentState.Draw(Batch, viewport);
            
            Batch.Reset();

            if(CurrentState == null)
            {
                Vector2 measure = DefaultFonts.Italic.Measure(32, "No GameState set (fixa snälla)");
                
                Batch.Text(
                    SpriteBatch.FontStyle.Italic, 
                    32, 
                    "No GameState set (fixa snälla)", 
                    new Vector2(viewport.X / 2 - measure.X / 2, viewport.Y / 2 - measure.Y / 2),
                    new Color(0.9f, 0.9f, 0.9f));
            }
            
            if(DrawFPS)
                Batch.Text(SpriteBatch.FontStyle.Normal, 24, "FPS: " + FPS, new Vector2(10, 10), Color.GreenYellow);

            base.Draw(gameTime);
        }
    }
}
