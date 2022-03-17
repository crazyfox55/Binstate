using System;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Exit<T> : Transitions<T>, IExit<T>
	{
		public Exit(StateConfig stateConfig) : base(stateConfig) { }

		public ITransitions<T> OnExit(Action exitAction)
		{
			if (exitAction == null) throw new ArgumentNullException(nameof(exitAction));
			
			return OnExit(WrapEnterExitAction(exitAction));
		}

		public ITransitions<T> OnExit(Func<Task> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			StateConfig.SetExitAction(exitAction);
			return this;
		}

		public ITransitions<T> OnExit(Action<T> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			return OnExit(WrapEnterExitAction(exitAction));
		}

		public ITransitions<T> OnExit(Func<T, Task> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			StateConfig.SetExitAction(exitAction);
			return this;
		}
	}
}