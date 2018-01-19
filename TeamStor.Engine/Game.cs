using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStor.Engine
{
    /// <summary>
    /// Main game class.
    /// </summary>
    public class Game
    {
        private Microsoft.Xna.Framework.Game _monogameGame;
        private GameState _state;

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
                if(value != null)
                {
                    value.Game = this;
                    value.OnEnter(_state);
                }

                _state = value;
            }
        }

        /// <param name="initialState">The state to start the game on.</param>
        /// <param name="showTeamStorLogo">If the Team Stor logo should be shown before starting the initial state.</param>
        public Game(GameState initialState, bool showTeamStorLogo = true)
        {
            if(showTeamStorLogo)
                CurrentState = new Internal.TeamStorLogoState(initialState);
            else
                CurrentState = initialState;
        }
    }
}
