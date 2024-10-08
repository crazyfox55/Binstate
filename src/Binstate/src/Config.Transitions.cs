﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Transitions : ITransitions
	{
		public readonly StateConfig StateConfig;

		protected Transitions(StateConfig stateConfig) =>
			StateConfig = stateConfig;

		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref> when <paramref name="event"> event is raised </paramref>
		/// </summary>
		public ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null)
		{
			if(@event is null) throw new ArgumentNullException(nameof(@event));
			if(stateId is null) throw new ArgumentNullException(nameof(stateId));

			AddTransitionToList(@event, StaticGetState(stateId), true, action);
			return this;
		}

		/// <inheritdoc />
		public ITransitions AddTransition(TEvent @event, GetState<TState> getState)
		{
			if(@event is null) throw new ArgumentNullException(nameof(@event));
			if(getState is null) throw new ArgumentNullException(nameof(getState));

			AddTransitionToList(@event, getState, false, null);
			return this;
		}

		public void AllowReentrancy(TEvent @event) =>
			AddTransition(@event, StateConfig.StateId);

		public ITransitions<T> AddTransition<T>(TEvent @event, TState stateId, Action<T> action)
		{
			StateConfig.Factory = new StateFactory<T>();

			var transitions = new Transitions<T>(this);
			return transitions.AddTransition(@event, stateId, action); // delegate call
		}

		public static GetState<TState> StaticGetState(TState stateId)
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
			=> (out TState? state) =>
			{
				state = stateId;
				return true;
			};
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

		protected void AddTransitionToList(TEvent @event, GetState<TState> getState, bool isStatic, object? action) =>
			StateConfig.TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, action));

		public static Func<Task> WrapEnterExitAction(Action action)
			=> () =>
			{
				action();
				return Task.CompletedTask;
			};

		public static Func<TArgument, Task> WrapEnterExitAction<TArgument>(Action<TArgument> action)
			=> (argument) =>
			{
				action(argument);
				return Task.CompletedTask;
			};

		public static Func<IStateController<TEvent>, Task> WrapRunAction(Action<IStateController<TEvent>> runAction) =>
			controller =>
			{
				runAction(controller);
				return Task.CompletedTask;
			};

		public static Func<IStateController<TEvent>, TArgument, Task> WrapRunAction<TArgument>(Action<IStateController<TEvent>, TArgument> runAction) =>
			(controller, argument) =>
			{
				runAction(controller, argument);
				return Task.CompletedTask;
			};
	}
}