using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Exit<T> : Transitions<T>, IExit<T>
	{
		private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
		private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

		private readonly Exit _exit;

		public Exit(Exit exit) : base(exit)
		{
			_exit = exit;
		}

		public ITransitions<T> OnExit(Action exitAction)
		{
			_exit.OnExit(exitAction);
			return this;
		}

		public ITransitions<T> OnExit(Func<Task> exitAction)
		{
			_exit.OnExit(exitAction);
			return this;
		}

		public ITransitions<T> OnExit(Action<T> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));
			if(IsAsyncMethod(exitAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnExit(Transitions.WrapEnterExitAction(exitAction));
		}

		public ITransitions<T> OnExit(Func<T, Task> exitAction)
		{
			if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));

			StateConfig.SetExitAction(exitAction);
			return this;
		}
	}
}