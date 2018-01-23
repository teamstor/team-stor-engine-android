namespace TeamStor.Engine.Coroutine
{
	/// <summary>
	/// Coroutine wait operation.
	/// </summary>
	public class Wait : ICoroutineOperation
	{
		private Game _game;
		private double _completedValue;

		private Wait(Game game, double waitValue)
		{
			_game = game;
			_completedValue = game.Time + waitValue;
		}

		/// <summary>
		/// Waits for the specified amount of seconds.
		/// </summary>
		/// <param name="game">Game to use for timing.</param>
		/// <param name="seconds">The amount of seconds to wait for</param>
		public static Wait Seconds(Game game, double seconds)
		{
			return new Wait(game, seconds);
		}
		
		/// <summary>
		/// Waits for a frame.
		/// </summary>
		/// <param name="game">Game to use for timing.</param>
		public static Wait Frame(Game game)
		{
			return new Wait(game, -1.0);
		}

		public bool Completed
		{
			get { return _game.Time >= _completedValue; }
		}
	}
}