using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStor.Engine.Graphics;

namespace TeamStor.Engine
{
    /// <summary>
    /// State of the game (such as main menu, in-game, etc)
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// Game class of this state.
        /// </summary>
        public Game Game;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        /// <param name="previousState">The previous state, can be null</param>
        public virtual void OnEnter(GameState previousState)
        {

        }

        /// <summary>
        /// Called when the state is left.
        /// </summary>
        /// <param name="previousState">The next state, can be null</param>
        public virtual void OnLeave(GameState nextState)
        {

        }

        /// <summary>
        /// Called once every frame.
        /// </summary>
        /// <param name="deltaTime">The amount of time since the last frame, in seconds.</param>
        /// <param name="totalTime">The total amount of time (in seconds) since the game started.</param>
        /// <param name="count">The total amount of updates since the game started.</param>
        public virtual void Update(double deltaTime, double totalTime, long count)
        {

        }

        /// <summary>
        /// Called 60 (change with <code>Game.FixedUpdatesPerSecond</code>) times a second.
        /// </summary>
        /// <param name="count">The total amount of fixed updates since the game started.</param>
        public virtual void FixedUpdate(long count)
        {

        }

        /// <summary>
        /// Called once every frame to draw.
        /// </summary>
        /// <param name="batch">The sprite batch to draw with.</param>
        /// <param name="screenSize">The size of the screen.</param>
        public virtual void Draw(SpriteBatch batch, Vector2 screenSize)
        {

        }
    }
}
