using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private double _timeInUpdate;
        private double _timeInFixedUpdate;
        private double _timeInDraw;

        private string _snälla = "snälla";
        
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
            get;
            private set;
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

        [Flags]
        public enum DebugStats : byte
        {
            FPS = 1,
            General = 2,
            SpriteBatch = 4
        }

        /// <summary>
        /// Debug stats to draw.
        /// </summary>
        public DebugStats Stats = 0;

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
            /// Proxima Nova
            /// </summary>
            public Font Normal;
            
            /// <summary>
            /// Proxima Nova bold
            /// </summary>
            public Font Bold;
            
            /// <summary>
            /// Proxima Nova italic
            /// </summary>
            public Font Italic;
            
            /// <summary>
            /// Proxima Nova bold italic
            /// </summary>
            public Font ItalicBold;
            
            /// <summary>
            /// Inconsolata
            /// </summary>
            public Font Mono;
            
            /// <summary>
            /// Inconsolata Bold
            /// </summary>
            public Font MonoBold;
        }

        /// <summary>
        /// Default fonts used by the engine.
        /// </summary>
        public DefaultFontsCollection DefaultFonts = new DefaultFontsCollection();

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
                    if(value)
                    {
                        _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                        _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                    }
                    else
                    {
                        _graphicsDeviceManager.PreferredBackBufferWidth = 960;
                        _graphicsDeviceManager.PreferredBackBufferHeight = 540;
                    }
                    
                    _graphicsDeviceManager.IsFullScreen = value;
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
            
            DefaultFonts.Normal = new Font(GraphicsDevice, Assets.Directory + "/engine/ProximaNova-Regular.otf");
            DefaultFonts.Bold = new Font(GraphicsDevice, Assets.Directory + "/engine/ProximaNova-Bold.otf");
            DefaultFonts.Italic = new Font(GraphicsDevice, Assets.Directory + "/engine/ProximaNova-RegularIt.otf");
            DefaultFonts.ItalicBold = new Font(GraphicsDevice, Assets.Directory + "/engine/ProximaNova-BoldIt.otf");
            DefaultFonts.Mono = new Font(GraphicsDevice, Assets.Directory + "/engine/Inconsolata-Regular.ttf");
            DefaultFonts.MonoBold = new Font(GraphicsDevice, Assets.Directory + "/engine/Inconsolata-Bold.ttf");

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
            Stopwatch watch = new Stopwatch();
            Time = gameTime.TotalGameTime.TotalSeconds;

            if(_lastUpdateTime == 0)
                _lastUpdateTime = Time;

            DeltaTime = Time - _lastUpdateTime;

            if(OnUpdateBeforeState != null)
                OnUpdateBeforeState(this, new UpdateEventArgs(DeltaTime, Time, TotalUpdates));

            if(CurrentState != null)        
                CurrentState.Update(DeltaTime, Time, TotalUpdates);
            
            TotalUpdates++;

            if(OnUpdateAfterState != null)
                OnUpdateAfterState(this, new UpdateEventArgs(DeltaTime, Time, TotalUpdates));
            
            if(Input.KeyPressed(Keys.F1))
            {
                if(Stats.HasFlag(DebugStats.FPS))
                    Stats &= ~DebugStats.FPS;
                else 
                    Stats |= DebugStats.FPS;
            }
            
            if(Input.KeyPressed(Keys.F2))
            {
                if(Stats.HasFlag(DebugStats.General))
                    Stats &= ~DebugStats.General;
                else 
                    Stats |= DebugStats.General;
            }
            
            if(Input.KeyPressed(Keys.F3))
            {
                if(Stats.HasFlag(DebugStats.SpriteBatch))
                    Stats &= ~DebugStats.SpriteBatch;
                else 
                    Stats |= DebugStats.SpriteBatch;
            }
            
            if(Input.KeyPressed(Keys.F4))
                Fullscreen = !Fullscreen;

            _accumTime += DeltaTime;
            _framesSinceLastFpsReset++;
            _accumTimeSinceLastFpsReset += DeltaTime;

            if(_accumTimeSinceLastFpsReset >= 1.0)
            {
                if(CurrentState == null)
                    _snälla = _snälla.Substring(0, _snälla.Length - 1) + "la";
                
                FPS = _framesSinceLastFpsReset;
                _framesSinceLastFpsReset = 0;
                _accumTimeSinceLastFpsReset -= 1.0;
            }
            
            watch.Stop();
            _timeInUpdate = watch.Elapsed.TotalMilliseconds;
            watch.Start();
            
            while(_accumTime >= (1.0 / FixedUpdatesPerSecond))
            {
                if(OnFixedUpdateBeforeState != null)
                    OnFixedUpdateBeforeState(this, new FixedUpdateEventArgs(TotalFixedUpdates));

                if(CurrentState != null)
                    CurrentState.FixedUpdate(TotalFixedUpdates);
                
                TotalFixedUpdates++;
                    
                if(OnFixedUpdateAfterState != null)
                    OnFixedUpdateAfterState(this, new FixedUpdateEventArgs(TotalFixedUpdates));
                
                _accumTime -= (1.0 / FixedUpdatesPerSecond);
            }

            watch.Stop();
            _timeInFixedUpdate = watch.Elapsed.TotalMilliseconds;

            _lastUpdateTime = Time;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            GraphicsDevice.Clear(new Color(0.15f, 0.15f, 0.15f));
            
            Vector2 viewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            if(CurrentState != null)
                CurrentState.Draw(Batch, viewport);
                        
            Batch.Reset();
            
            watch.Stop();
            _timeInDraw = watch.Elapsed.TotalMilliseconds;

            if(CurrentState == null)
            {
                Vector2 measure = DefaultFonts.Italic.Measure(24, "No GameState set (fixa " + _snälla + ")");

                Batch.Text(
                    SpriteBatch.FontStyle.Normal, 
                    24, 
                    "No GameState set (fixa " + _snälla + ")", 
                    new Vector2(viewport.X / 2 - measure.X / 2, viewport.Y / 2 - measure.Y / 2),
                    new Color(0.9f, 0.9f, 0.9f));
            }

            if(Stats != 0)
            {
                int y = 10;
                
                if(Stats.HasFlag(DebugStats.FPS))
                {
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "FPS: " + FPS, new Vector2(10, y), Color.PaleGoldenrod);
                    y += 24;
                }
                
                if(Stats.HasFlag(DebugStats.General))
                {
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, Math.Round(_timeInUpdate + _timeInFixedUpdate + _timeInDraw, 2) + " ms", new Vector2(10, y), Color.Aquamarine);
                    y += 24;
                    
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Update " + Math.Round(_timeInUpdate, 1) + " ms", new Vector2(10, y), Color.Aquamarine);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "FixedUpdate " + Math.Round(_timeInFixedUpdate, 1) + " ms", new Vector2(10, y), Color.Aquamarine);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Draw " + Math.Round(_timeInDraw, 1) + " ms", new Vector2(10, y), Color.Aquamarine);
                    y += 24;
                    
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Game state: " + (CurrentState == null ? "(none)" : CurrentState.GetType().Name), new Vector2(10, y), Color.Aquamarine);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Time since start: " + Math.Round(Time, 1) + " s", new Vector2(10, y), Color.Aquamarine);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Updates since start: " + TotalUpdates, new Vector2(10, y), Color.Aquamarine);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Fixed updates since start: " + TotalFixedUpdates, new Vector2(10, y), Color.Aquamarine);
                    y += 24;
                    
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Memory used: " + GC.GetTotalMemory(false) / 1024 / 1024 + " MB", new Vector2(10, y), Color.Aquamarine);
                    y += 24;
                }
                
                if(Stats.HasFlag(DebugStats.SpriteBatch))
                {                    
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Drawn textures: " + Batch.Stats.DrawnTextures, new Vector2(10, y), Color.CadetBlue);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "Start() count: " + Batch.Stats.BatchStarts, new Vector2(10, y), Color.CadetBlue);
                    y += 18;
                    Batch.Text(SpriteBatch.FontStyle.Mono, 16, "End() count: " + Batch.Stats.BatchEnds, new Vector2(10, y), Color.CadetBlue);
                }
            }
            
            Batch.Stats.Reset();

            base.Draw(gameTime);
        }
    }
}
