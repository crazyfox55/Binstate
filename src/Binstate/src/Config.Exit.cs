using System;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Exit : Transitions, IExit
	{
		protected Exit(StateConfig stateConfig) : base(stateConfig) { }

		public ITransitions OnExit(Action exitAction)
		{
			if (exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			return OnExit(WrapEnterExitAction(exitAction));
		}

		public ITransitions OnExit(Func<Task> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			StateConfig.SetExitAction(exitAction);
			return this;
		}

		public ITransitions<T> OnExit<T>(Action<T> exitAction)
		{
			StateConfig.Factory = new StateFactory<T>();
			return new Exit<T>(this).OnExit(exitAction);
		}

		public ITransitions<T> OnExit<T>(Func<T, Task> exitAction)
		{
			StateConfig.Factory = new StateFactory<T>();
			return new Exit<T>(this).OnExit(exitAction);
		}
	}
}