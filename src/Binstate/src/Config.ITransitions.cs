using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	/// <summary>
	///   This interface is used to configure which transitions allowed from the currently configured state.
	/// </summary>
	public interface ITransitions
	{
		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref> when <paramref name="event"> event is raised </paramref>
		/// </summary>
		ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null);

		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref>
		///   when <paramref name="event"> event is raised </paramref>
		/// </summary>
		ITransitions<T> AddTransition<T>(TEvent @event, TState stateId, Action<T> action);

#pragma warning disable 1574, 1584, 1581, 1580
		/// <summary>
		///   Defines transition from the currently configured state to the state calculated dynamically depending on other application state.
		/// </summary>
		/// <param name="event"> </param>
		/// <param name="getState"> If getState returns false no transition performed. </param>
		/// <remarks>
		///   Use this overload if you use a value type (e.g. enum) as a <typeparamref name="TState" /> and the default value of the value type as a valid State id.
		/// </remarks>
#pragma warning restore 1574, 1584, 1581, 1580
		ITransitions AddTransition(TEvent @event, GetState<TState> getState);

		/// <summary>
		///   Defines transition from the state to itself when
		///   <param name="event"> is raised. Exit and enter actions are called in case of such transition. </param>
		/// </summary>
		void AllowReentrancy(TEvent @event);
	}

	/// <summary>
	///   This interface is used to configure which transitions allowed from the currently configured state.
	/// </summary>
	public interface ITransitions<out T>
	{
		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref> when <paramref name="event"> event is raised </paramref>
		/// </summary>
		ITransitions<T> AddTransition(TEvent @event, TState stateId, Action? action = null);

		/// <summary>
		///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref>
		///   when <paramref name="event"> event is raised </paramref>
		/// </summary>
		ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T> action);

#pragma warning disable 1574, 1584, 1581, 1580
		/// <summary>
		///   Defines transition from the currently configured state to the state calculated dynamically depending on other application state.
		/// </summary>
		/// <param name="event"> </param>
		/// <param name="getState"> If getState returns false no transition performed. </param>
		/// <remarks>
		///   Use this overload if you use a value type (e.g. enum) as a <typeparamref name="TState" /> and the default value of the value type as a valid State id.
		/// </remarks>
#pragma warning restore 1574, 1584, 1581, 1580
		ITransitions<T> AddTransition(TEvent @event, GetState<TState> getState);

		/// <summary>
		///   Defines transition from the state to itself when
		///   <param name="event"> is raised. Exit and enter actions are called in case of such transition. </param>
		/// </summary>
		void AllowReentrancy(TEvent @event);
	}
}