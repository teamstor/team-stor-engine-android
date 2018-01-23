namespace TeamStor.Engine.TestGame
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using(Game game = Game.Run(null, "data", false))
				game.Run();
		}
	}
}