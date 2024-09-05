using System;
using System.Threading.Tasks;
using Binstate.Tests.Util;
using FakeItEasy;
using Xunit;

namespace Binstate.Tests;

public class ExitActionTest : StateMachineTestBase
{
	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_exit_action(RaiseWay raiseWay)
	{
		var onExit = A.Fake<Action>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).OnExit(onExit).AddTransition(GoToStateX, StateX);
		builder.DefineState(StateX);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		A.CallTo(() => onExit()).MustHaveHappenedOnceExactly();
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_exit_action_w_argument(RaiseWay raiseWay)
	{
		const string expected = "argument";
		var          onExit  = A.Fake<Action<string>>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).OnExit(onExit).AddTransition(GoToStateX, StateX);
		builder.DefineState(StateX);

		var target = await builder.Build(Initial, expected);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		A.CallTo(() => onExit(expected)).MustHaveHappenedOnceAndOnly();
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_exit_action_w_argument_from_prev_active_state(RaiseWay raiseWay)
	{
		const string expected = "argument";
		var          onExitInitial = A.Fake<Action<string>>();
		var          onExitX = A.Fake<Action<string>>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).OnExit(onExitInitial).AddTransition(GoToStateX, StateX);
		builder.DefineState(StateX).OnExit(onExitX).AddTransition(GoToStateY, StateY);
		builder.DefineState(StateY);

		var target = await builder.Build(Initial, expected);
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateY);

		// --assert
		A.CallTo(() => onExitX(expected)).MustHaveHappenedOnceAndOnly();
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_parent_exit_action_w_argument(RaiseWay raiseWay)
	{
		const string expected      = "argument";
		var          onExitParent = A.Fake<Action<string>>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Parent).OnExit(onExitParent);
		builder.DefineState(Initial).AsSubstateOf(Parent).AddTransition(GoToChild, Child);
		builder.DefineState(Child); // Child is w/o Exit and argument

		var target = await builder.Build(Initial, expected);

		// --act
		await target.RaiseAsync(raiseWay, GoToChild);

		// --assert
		A.CallTo(() => onExitParent(expected)).MustHaveHappenedOnceAndOnly();
	}
}