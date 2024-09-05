using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Binstate.Tests;

public abstract class StateMachineTestBase : IAssemblyFixture<TestFixture>
{
	protected const string Initial    = nameof(Initial);
	protected const string StateX     = nameof(StateX);
	protected const string StateY     = nameof(StateY);
	protected const string Final      = nameof(Final);
	protected const string Root       = nameof(Root);
	protected const string Parent     = nameof(Parent);
	protected const string Child      = nameof(Child);
	protected const int    GoToStateX = 1;
	protected const int    GoToStateY = 2;
	protected const int    GoToParent  = 9;
	protected const int    GoToChild  = 3;

	protected static void OnException(Exception exception)
	{
		Assert.Fail(exception.Message);
		Assert.Fail(exception.GetType().Name);
	}

	public static IEnumerable<object[]> RaiseWays() => [ [ RaiseWay.Raise ] ];//, RaiseWay.RaiseAsync, };

	public static void OnEnter<T>(Config<string, int>.IEnter state, Action<T> action) => state.OnEnter(action);
	public static void OnExit<T>(Config<string, int>.IEnter  state, Action<T> action) => state.OnExit(action);

	public static IEnumerable<object[]> EnterExit()
	{
		yield return [ new Action<Config<string, int>.IEnter, Action<string>>(OnEnter), new Action<Config<string, int>.IEnter, Action<string>>(OnEnter)];
		yield return [ new Action<Config<string, int>.IEnter, Action<string>>(OnExit), new Action<Config<string, int>.IEnter, Action<string>>(OnExit)];
	}
}

public enum RaiseWay { Raise } //, RaiseAsync, }

public static class Extension
{
	public static async Task<bool> RaiseAsync<TEvent>(this IStateMachine<TEvent> stateMachine, RaiseWay way, TEvent @event)
		=> await stateMachine.RaiseAsync(@event);

	public static async Task<bool> RaiseAsync<TEvent, TA>(this IStateMachine<TEvent> stateMachine, RaiseWay way, TEvent @event, TA arg, bool argumentIsFallback = false)
		=> await stateMachine.RaiseAsync(@event, arg, argumentIsFallback);

	//private static bool Call(RaiseWay way, Func<bool> syncAction, Func<bool> asyncAction)
	//	=> way switch
	//	{
	//		RaiseWay.RaiseAsync      => syncAction(),
	//		RaiseWay.RaiseAsync => asyncAction(),
	//		_                   => throw new InvalidOperationException(),
	//	};
}