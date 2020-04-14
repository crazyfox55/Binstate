using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This class is used to configure and build a state machine.  
  /// </summary>
  public class Builder
  {
    private readonly List<Config.Enter> _states = new List<Config.Enter>();

    /// <summary>
    /// Defines the new state in the state machine
    /// </summary>
    /// <param name="stateId">Id of the state, is used to reference it from other elements of the state machine.</param>
    /// <remarks>Use returned syntax-sugar object to configure the new state.</remarks>
    public Config.Enter AddState([NotNull] object stateId)
    {
      if (stateId == null) throw new ArgumentNullException(nameof(stateId));

      var stateConfig = new Config.Enter(stateId);
      _states.Add(stateConfig);
      return stateConfig;
    }

    /// <summary>
    /// Validates consistency and builds the state machine using provided configuration. 
    /// </summary>
    /// <param name="initialState">The initial state of the state machine. The entering action of the initial state is not called by building the state machine.</param>
    /// <exception cref="InvalidOperationException">Throws if there are any inconsistencies in the provided configuration.</exception>
    public StateMachine Build([NotNull] object initialState)
    {
      if (initialState == null) throw new ArgumentNullException(nameof(initialState));

      var states = new Dictionary<object, State>();
      foreach (var stateConfig in _states)
      {
        var transitions = new Dictionary<object, Transition>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");
          transitions.Add(transition.Event, transition);
        }

        var state = new State(stateConfig.StateId, stateConfig.EnterAction, stateConfig.ExitAction, transitions);
        states.Add(stateConfig.StateId, state);
      }

      if (!states.ContainsKey(initialState))
        throw new ArgumentException($"No state '{initialState}' is defined");
      ValidateStateMachine(states);

      return new StateMachine(states[initialState], states);
    }

    private static void ValidateStateMachine(Dictionary<object, State> states)
    {
      foreach (var state in states.Values)
      {
        foreach (var transition in state.Transitions.Values)
        {
          if (!states.ContainsKey(transition.State))
            throw new InvalidOperationException($"Transition '{transition.Event}' from state '{state.Id}' references not defined state '{transition.State}'");
        }
      }
    }
  }
}