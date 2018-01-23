using System.Collections.Generic;
using CoroutineEnumerator = System.Collections.Generic.IEnumerator<TeamStor.Engine.Coroutine.ICoroutineOperation>;

namespace TeamStor.Engine.Coroutine
{
	/// <summary>
	/// Manages coroutine updating.
	/// </summary>
	public class CoroutineManager
	{
		public delegate CoroutineEnumerator CoroutineDelegate();
		public delegate CoroutineEnumerator CoroutineArgDelegate(object userState);

		/// <summary>
		/// Active coroutines.
		/// </summary>
		public List<CoroutineEnumerator> Active { get; } = new List<CoroutineEnumerator>();
		
		/// <summary>
		/// Adds an existing coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine enumerator.</param>
		/// <returns>The coroutine enumerator.</returns>
		public CoroutineEnumerator AddExisting(CoroutineEnumerator enumerator)
		{
			return enumerator;
		}
		
		/// <summary>
		/// Starts a coroutine.
		/// </summary>
		/// <param name="d">The coroutine function.</param>
		/// <returns>The coroutine enumerator.</returns>
		public CoroutineEnumerator Start(CoroutineDelegate d)
		{
			return AddExisting(d.Invoke());
		}
		
		/// <summary>
		/// Starts a coroutine.
		/// </summary>
		/// <param name="d">The coroutine function.</param>
		/// <param name="userState">User state to pass to the coroutine.</param>
		/// <returns>The coroutine enumerator.</returns>
		public CoroutineEnumerator Start(CoroutineArgDelegate d, object userState)
		{
			return AddExisting(d.Invoke(userState));
		}

		/// <summary>
		/// Advances coroutines.
		/// </summary>
		/// <param name="steps">The number of steps to advance.</param>
		public void Advance(int steps = 1)
		{
			foreach(CoroutineEnumerator enumerator in Active.ToArray())
			{
				for(int i = 0; i < steps; i++)
				{
					if(enumerator.Current == null || enumerator.Current != null && enumerator.Current.Completed)
					{
						if(!enumerator.MoveNext())
						{
							Active.Remove(enumerator);
							break;
						}
					}
				}
			}
		}
	}
}