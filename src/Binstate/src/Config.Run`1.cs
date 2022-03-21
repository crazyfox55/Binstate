using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Run<T> : Exit<T>, IRun<T>
	{
		private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
		private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

		private readonly Run _run;

		internal Run(Run run) : base(run)
		{
			_run = run;
		}

		public IExit<T> OnRun(Action runAction)
		{
			_run.OnRun(runAction);
			return this;
		}

		public IExit<T> OnRun(Action<IStateController<TEvent>> runAction)
		{
			_run.OnRun(runAction);
			return this;
		}

		public IExit<T> OnRun(Func<Task> runAction)
		{
			_run.OnRun(runAction);
			return this;
		}

		public IExit<T> OnRun(Func<IStateController<TEvent>, Task> runAction)
		{
			_run.OnRun(runAction);
			return this;
		}

		public IExit<T> OnRun(Action<T> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun((_, argument) => runAction(argument));
		}

		public IExit<T> OnRun(Action<IStateController<TEvent>, T> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun(Transitions.WrapRunAction(runAction));
		}

		public IExit<T> OnRun(Func<T, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			return OnRun((_, argument) => runAction(argument));
		}

		public IExit<T> OnRun(Func<IStateController<TEvent>, T, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			StateConfig.SetRunAction(runAction);
			return this;
		}
	}
}
