﻿using System;
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

		internal Run(StateConfig stateConfig) : base(stateConfig) { }

		public IExit<T> OnRun(Action runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			return OnRun((IStateController<TEvent> _) => runAction());
		}

		public IExit<T> OnRun(Action<IStateController<TEvent>> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));
			if(IsAsyncMethod(runAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

			StateConfig.EnterAction = WrapAction(runAction);
			return this;
		}

		public IExit<T> OnRun(Func<Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			return OnRun((IStateController<TEvent> _) => runAction());
		}

		public IExit<T> OnRun(Func<IStateController<TEvent>, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			StateConfig.EnterAction = WrapAction(runAction);
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

			StateConfig.EnterAction = WrapAction(runAction);
			StateConfig.Factory = new StateFactory<T>();
			return this;
		}

		public IExit<T> OnRun(Func<T, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			return OnRun((_, argument) => runAction(argument));
		}

		public IExit<T> OnRun(Func<IStateController<TEvent>, T, Task> runAction)
		{
			if(runAction is null) throw new ArgumentNullException(nameof(runAction));

			StateConfig.EnterAction = runAction;
			StateConfig.Factory = new StateFactory<T>();
			return this;
		}

		private static Func<IStateController<TEvent>, Unit, Task> WrapAction(Action<IStateController<TEvent>> runAction) =>
			(controller, _) =>
			{
				return Task.Run(() => runAction(controller));
			};

		private static Func<IStateController<TEvent>, T, Task> WrapAction(Action<IStateController<TEvent>, T> runAction) =>
			(controller, argument) =>
			{
				return Task.Run(() => runAction(controller, argument));
			};

		private static Func<IStateController<TEvent>, Unit, Task> WrapAction(Func<IStateController<TEvent>, Task> runAction) =>
			(controller, argument) => runAction(controller);
	}
}
