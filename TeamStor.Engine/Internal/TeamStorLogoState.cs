namespace TeamStor.Engine.Internal
{
    public class TeamStorLogoState : GameState
    {
        private GameState _initialState;

        public TeamStorLogoState(GameState initialState)
        {
            _initialState = initialState;
        }
    }
}
