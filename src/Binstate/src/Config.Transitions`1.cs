using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	internal class Transitions<T> : ITransitions<T>
	{
		public readonly StateConfig StateConfig;
		private readonly Transitions _transition;

		public Transitions(Transitions transitions)
		{
			StateConfig = transitions.StateConfig;
			_transition = transitions;
		}

		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref> when <paramref name="event"> event is raised </paramref>
		/// </summary>
		public ITransitions<T> AddTransition(TEvent @event, TState stateId, Action? action = null)
		{
			_transition.AddTransition(@event, stateId, action);
			return this;
		}

		/// <inheritdoc />
		public ITransitions<T> AddTransition(TEvent @event, GetState<TState> getState)
		{
			_transition.AddTransition(@event, getState);
			return this;
		}

		public void AllowReentrancy(TEvent @event) =>
			_transition.AllowReentrancy(@event);

		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref>
		///   when <paramref name="event"> event is raised </paramref>
		/// </summary>
		public ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T> action)
		{
			if(@event is null) throw new ArgumentNullException(nameof(@event));
			if(stateId is null) throw new ArgumentNullException(nameof(stateId));
			if(action == null) throw new ArgumentNullException(nameof(action));

			AddTransitionToList(@event, Transitions.StaticGetState(stateId), true, action);
			return this;
		}

		protected void AddTransitionToList(TEvent @event, GetState<TState> getState, bool isStatic, object? action) =>
			StateConfig.TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, action));
	}
}