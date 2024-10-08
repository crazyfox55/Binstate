﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binstate;

internal partial class StateMachine<TState, TEvent>
{
	/// <summary>
	///   Performing transition is split into two parts, the first one is "read only", preparing and checking all the data, can throw an exception.
	/// </summary>
	/// <returns>
	///   Returns null if:
	///   no transition found by specified <paramref name="event" /> from the current state
	///   dynamic transition returns 'null'
	/// </returns>
	/// <exception cref="TransitionException"> Throws if passed argument doesn't match the 'enter' action of the target state. </exception>
	private TransitionData? PrepareTransition<TArgument>(TEvent @event, TArgument argument, bool argumentIsFallback, bool noWait = false)
	{
		try
		{
			if(noWait)
			{
				if(!_lock.WaitOne(0))
				{
					return null;
				}
			}
			else
			{
				_lock.WaitOne();
			}

			if(! _activeState.FindTransitionTransitive(@event, out var transition) // looks for a transition through all parent states
			|| ! transition!.GetTargetStateId(out var stateId))
			{
				// no transition by specified event is found or dynamic transition returns null as target state id
				_lock.Set();

				return null;
			}

			var targetState = GetStateById(stateId!);

			var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);
			var argumentsBag   = PrepareArgument(argument, argumentIsFallback, targetState, commonAncestor, _activeState);

			return new TransitionData(_activeState, transition, targetState, commonAncestor, argumentsBag);
		}
		catch(TransitionException)
		{
			_lock.Set();

			throw;
		}
		catch(Exception exception)
		{
			_onException(exception);
			_lock.Set();

			return null;
		}
	}

	private static Argument.Bag PrepareArgument<TArgument>(
		TArgument argument,
		bool      argumentIsFallback,
		IState    targetState,
		IState?   commonAncestor,
		IState    sourceState)
	{
		var argumentResolver = new Argument.Resolver();

		var state = targetState;
		while(state != commonAncestor)
		{
			if(state is null) throw new InvalidOperationException("It can't be null before it is equal to commonAncestor");

			argumentResolver.FindArgumentFor(state, argument, argumentIsFallback, sourceState);
			state = state.ParentState;
		}

		return argumentResolver.ArgumentsBag;
	}

	/// <summary>
	///   Performs changes in the state machine state. Doesn't throw any exceptions, exceptions from the user code, 'enter' and 'exit' actions are translated
	///   into the delegate passed to <see cref="Builder{TState,TEvent}(System.Action{System.Exception})" />
	/// </summary>
	private async Task<bool> PerformTransitionAsync(TransitionData transitionData)
	{
		var currentActiveState = transitionData.CurrentActiveState;
		var prevActiveState    = currentActiveState;
		var transition         = transitionData.Transition;
		var targetState        = transitionData.TargetState;
		var commonAncestor     = transitionData.CommonAncestor;
		var argumentsBag       = transitionData.ArgumentsBag;

		try
		{
			try
			{
				// exit all active states which are not parent for the new state
				while(currentActiveState != commonAncestor)
				{
					await currentActiveState!.ExitSafeAsync(_onException).ConfigureAwait(false); // currentActiveState can't become null earlier then be equal to commonAncestor
					currentActiveState = currentActiveState.ParentState;
				}

				// invoke action attached to the transition itself
				prevActiveState.CallTransitionActionSafe(transition, _onException);

				// and then activate new active states
				_activeState = targetState;

				var stack = new Stack<IState<TState, TEvent>>();

				while(targetState != commonAncestor)
				{
					stack.Push(targetState!);
					targetState = targetState!.ParentState;
				}

				while(stack.Count > 0)
				{
					var state = stack.Pop();
					await ActivateState(state, argumentsBag).ConfigureAwait(false);
				}
			}
			finally // no exception should be thrown here, but paranoia is my life
			{
				_lock.Set();
			}
		}
		catch(Exception exception)
		{
			_onException(exception);
		}

		return true; // just to reduce amount of code calling this method
	}

	private Task ActivateState(IState<TState, TEvent> state, Argument.Bag argumentsBag)
	{
		state.IsActive = true; // set is as active inside the lock, see implementation of State class for details
		var controller = new Controller(state, this);

		var setArgument = argumentsBag.GetValueSafe(state);
		setArgument?.Invoke();   // set the Argument property of the state if Argument is required

		// set Argument property before calling EnterSafe, due to it uses this property
		return state.EnterSafeAsync(controller, _onException);
	}

	private readonly struct TransitionData
	{
		public readonly IState                 CurrentActiveState;
		public readonly ITransition            Transition;
		public readonly IState<TState, TEvent> TargetState;
		public readonly IState?                CommonAncestor;
		public readonly Argument.Bag           ArgumentsBag;

		public TransitionData(
			IState                 currentActiveState,
			ITransition            transition,
			IState<TState, TEvent> targetState,
			IState?                commonAncestor,
			Argument.Bag           argumentsBag)
		{
			CurrentActiveState = currentActiveState;
			Transition         = transition;
			TargetState        = targetState;
			ArgumentsBag       = argumentsBag;
			CommonAncestor     = commonAncestor;
		}
	}
}