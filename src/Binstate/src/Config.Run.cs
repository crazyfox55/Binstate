using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Run : Exit, IRun
	{
		private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
		private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

		internal Run(StateConfig stateConfig) : base(stateConfig) { }

		public IExit OnRun(Action runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun(_ => runAction());
		}

		public IExit OnRun(Action<IStateController<TEvent>> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun(WrapRunAction(runAction));
		}

		public IExit OnRun(Func<Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			return OnRun(_ => runAction());
		}

		public IExit OnRun(Func<IStateController<TEvent>, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			StateConfig.SetRunAction(runAction);
			return this;
		}

		public IExit<TArgument> OnRun<TArgument>(Action<TArgument> runAction)
		{
			StateConfig.Factory = new StateFactory<TArgument>();
			return new Run<TArgument>(this).OnRun(runAction);
		}

		public IExit<TArgument> OnRun<TArgument>(Action<IStateController<TEvent>, TArgument> runAction)
		{
			StateConfig.Factory = new StateFactory<TArgument>();
			return new Run<TArgument>(this).OnRun(runAction);
		}

		public IExit<TArgument> OnRun<TArgument>(Func<TArgument, Task> runAction)
		{
			StateConfig.Factory = new StateFactory<TArgument>();
			return new Run<TArgument>(this).OnRun(runAction);
		}

		public IExit<TArgument> OnRun<TArgument>(Func<IStateController<TEvent>, TArgument, Task> runAction)
		{
			StateConfig.Factory = new StateFactory<TArgument>();
			return new Run<TArgument>(this).OnRun(runAction);
		}

		public IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Action<TArgument, TRelay> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun<ITuple<TArgument, TRelay>>(tuple => runAction(tuple!.ItemX, tuple.ItemY));
		}

		public IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Action<IStateController<TEvent>, TArgument, TRelay> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun<ITuple<TArgument, TRelay>>((stateMachine, tuple) => runAction(stateMachine, tuple!.ItemX, tuple.ItemY));
		}

		public IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Func<TArgument, TRelay, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			return OnRun<ITuple<TArgument, TRelay>>(tuple => runAction(tuple!.ItemX, tuple.ItemY));
		}

		public IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Func<IStateController<TEvent>, TArgument, TRelay, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			return OnRun<ITuple<TArgument, TRelay>>((stateMachine, tuple) => runAction(stateMachine, tuple!.ItemX, tuple.ItemY));
		}
	}
}
