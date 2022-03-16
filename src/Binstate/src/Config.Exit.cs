using System;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Exit : Transitions, IExit
	{
		protected Exit(StateConfig stateConfig) : base(stateConfig) { }

		public ITransitionsEx OnExit(Action exitAction)
		{
			if (exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			return OnExit(WrapExitAction(exitAction));
		}

		public ITransitionsEx OnExit(Func<Task> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			StateConfig.SetExitAction(exitAction);
			return this;
		}

		public ITransitions<T> OnExit<T>(Action<T> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			return OnExit<T>(WrapExitAction(exitAction));
		}

		public ITransitions<T> OnExit<T>(Func<T, Task> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			StateConfig.SetExitAction(exitAction);
			StateConfig.Factory = new StateFactory<T>();
			return new Transitions<T>(StateConfig);
		}

		private static Func<Task> WrapExitAction(Action enterAction)
			=> () => Task.Run(enterAction);

		private static Func<TArgument, Task> WrapExitAction<TArgument>(Action<TArgument> enterAction)
			=> (argument) => Task.Run(() => enterAction(argument));
	}
}