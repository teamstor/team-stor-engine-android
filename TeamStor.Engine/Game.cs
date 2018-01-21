using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TeamStor.Engine.Graphics;

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
                {
                    _state.OnLeave(value);
                    Assets.OnStateChange();
                }

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
        
        /// <param name="initialState">The state to start the game on.</param>
        /// <param name="assetsDir">The assets directory.</param>
        /// <param name="showTeamStorLogo">If the Team Stor logo should be shown before starting the initial state.</param>
        public Game(GameState initialState, string assetsDir = "data", bool showTeamStorLogo = true)
        {
            Assets = new AssetsManager(this, assetsDir);
            
            if(showTeamStorLogo)
                _initialState = new Internal.TeamStorLogoState(initialState);
            else
                _initialState = initialState;
        }

        protected override void LoadContent()
        {
            Batch = new SpriteBatch(this);
            CurrentState = _initialState;
        }

        protected override void UnloadContent()
        {
            Assets.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            Time = gameTime.TotalGameTime.TotalSeconds;

            if(_lastUpdateTime == 0)
                _lastUpdateTime = Time;

            if(CurrentState != null)
            {
                CurrentState.Update(DeltaTime, Time, TotalUpdates);
                TotalUpdates++;
            }

            _accumTime += DeltaTime;
            while(_accumTime >= (1.0 / FixedUpdatesPerSecond))
            {
                if(CurrentState != null)
                {
                    CurrentState.FixedUpdate(TotalFixedUpdates);
                    TotalFixedUpdates++;
                }

                _accumTime -= (1.0 / FixedUpdatesPerSecond);
            }

            _lastUpdateTime = Time;
        }

        protected override void Draw(GameTime gameTime)
        {
            Batch.Reset();
            
            if(CurrentState != null)
                CurrentState.Draw(Batch, new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
        }
    }
}
