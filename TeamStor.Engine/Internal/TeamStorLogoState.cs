using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Engine.Internal
{
    public class TeamStorLogoState : GameState
    {
        private GameState _initialState;

        public TeamStorLogoState(GameState initialState)
        {
            _initialState = initialState;
        }

        public override void Draw(SpriteBatch batch, Vector2 screenSize)
        {
        }
    }
}
