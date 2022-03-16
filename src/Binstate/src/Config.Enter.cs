using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Enter : Run, IEnter
	{
		private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
		private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

		internal Enter(StateConfig stateConfig) : base(stateConfig) { }

		public IRun OnEnter(Action enterAction)
		{
			if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
			if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnEnter(WrapEnterAction(enterAction));
		}

		public IRun OnEnter(Func<Task> enterAction)
		{
			if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

			StateConfig.SetEnterAction(enterAction);
			return this;
		}

		public IRun<TArgument> OnEnter<TArgument>(Action<TArgument> enterAction)
		{
			if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
			if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnEnter(WrapEnterAction(enterAction));
		}

		public IRun<TArgument> OnEnter<TArgument>(Func<TArgument, Task> enterAction)
		{
			if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

			StateConfig.SetEnterAction(enterAction);
			StateConfig.Factory = new StateFactory<TArgument>();
			return new Run<TArgument>(StateConfig);
		}

		public IRun<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<TArgument, TRelay> enterAction)
		{
			if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
			if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnEnter<ITuple<TArgument, TRelay>>((tuple) => enterAction(tuple!.ItemX, tuple.ItemY));
		}

		public IRun<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<TArgument, TRelay, Task> enterAction)
		{
			if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

			return OnEnter<ITuple<TArgument, TRelay>>((tuple) => enterAction(tuple!.ItemX, tuple.ItemY));
		}

		private static Func<Task> WrapEnterAction(Action enterAction)
			=> () => Task.Run(enterAction);

		private static Func<TArgument, Task> WrapEnterAction<TArgument>(Action<TArgument> enterAction)
			=> (argument) => Task.Run(() => enterAction(argument));
	}
}