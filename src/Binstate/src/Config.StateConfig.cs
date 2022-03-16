using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class StateConfig
	{
		public readonly TState StateId;
		public readonly Dictionary<TEvent, Transition<TState, TEvent>> TransitionList = new();

		public IStateFactory Factory = new StateFactory();
		public Maybe<TState> ParentStateId = Maybe<TState>.Nothing;

		private Func<Task>? EnterActionNoArg;
		private Func<IStateController<TEvent>, Task>? RunActionNoArg;
		private Func<Task>? ExitActionNoArg;

		public object? EnterAction { get; private set; }
		public object? RunAction { get; private set; }
		public object? ExitAction { get; private set; }

		public StateConfig(TState stateId) => StateId = stateId;

		public void ActivateArgument<T>()
		{
			EnterAction ??= EnterActionNoArg == null ? null : (T _) => EnterActionNoArg();
			RunAction ??= RunActionNoArg == null ? null : (IStateController<TEvent> sc, T _) => RunActionNoArg(sc);
			ExitAction ??= ExitActionNoArg == null ? null : (T _) => ExitActionNoArg();
		}

		public void SetEnterAction(Func<Task> action) =>
			EnterActionNoArg = action;

		public void SetEnterAction<T>(Func<T, Task> action) =>
			EnterAction = action;

		public void SetRunAction(Func<IStateController<TEvent>, Task> action) =>
			RunActionNoArg = action;

		public void SetRunAction<T>(Func<IStateController<TEvent>, T, Task> action) =>
			RunAction = action;

		public void SetExitAction(Func<Task> action) =>
			ExitActionNoArg = action;

		public void SetExitAction<T>(Func<T, Task> action) =>
			ExitAction = action;

		public IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState) =>
			Factory.CreateState(this, parentState);
	}
}