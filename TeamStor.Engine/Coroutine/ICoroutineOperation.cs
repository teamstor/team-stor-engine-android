namespace TeamStor.Engine.Coroutine
{
	/// <summary>
	/// Coroutine operation.
	/// </summary>
	public interface ICoroutineOperation
	{
		/// <summary>
		/// If this operation has been completed.
		/// </summary>
		bool Completed { get; }
	}
}