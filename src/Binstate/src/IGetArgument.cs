﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binstate;

internal interface IState : IArgumentProvider
{
	IState? ParentState { get; }
	int     DepthInTree { get; }

	bool IsActive { get; set; }

	/// <summary>
	///   <see cref="State{TState,TEvent,TArgument}.ExitSafeAsync" /> can be called earlier then <see cref="Config{TState,TEvent}.Enter" /> of the activated state,
	///   see <see cref="StateMachine{TState,TEvent}.PerformTransitionAsync" /> implementation for details.
	///   In this case it should wait till <see cref="Config{TState,TEvent}.Enter" /> will be called and exited, before call exit action
	/// </summary>
	Task ExitSafeAsync(Action<Exception> onException);

	void CallTransitionActionSafe(ITransition transition, Action<Exception> onException);
}

internal interface IState<TState, TEvent> : IState
{
	TState Id { get; }

	new IState<TState, TEvent>? ParentState { get; }

	Task EnterSafeAsync(IStateController<TEvent> stateController, Action<Exception> onException);

	Dictionary<TEvent, Transition<TState, TEvent>> Transitions { get; }

	bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent>? transition);
}