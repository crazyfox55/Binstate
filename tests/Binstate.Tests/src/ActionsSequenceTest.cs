using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Binstate.Tests;

public class EnterExitActionsTest : StateMachineTestBase
{
	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_finish_enter_before_call_exit_and_call_next_enter(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		const string enter1 = nameof(enter1);
		const string exit1  = nameof(exit1);
		const string enter2 = nameof(enter2);

		// --arrange
		var builder = new Builder<string, int>(OnException);
		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

		builder.DefineState(StateX)
			.OnEnter(() =>
				{
					Thread.Sleep(299);
					actual.Add(enter1);
				}
			)
			.OnExit(() =>
				{
					Thread.Sleep(382);
					actual.Add(exit1);
				}
			)
			.AddTransition(GoToStateY, StateY);

		builder.DefineState(StateY)
			.OnEnter(() => actual.Add(enter2));

		var target = await builder.Build(Initial);
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateY);

		// --assert
		actual.Should().BeEquivalentTo(enter1, exit1, enter2);
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_exit_and_enter_on_reentering(RaiseWay raiseWay)
	{
		const string enter = nameof(enter);
		const string exit  = nameof(exit);

		var actual = new List<string>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

		builder
		 .DefineState(StateX)
		 .OnEnter(() => actual.Add(enter))
		 .OnExit(() => actual.Add(exit))
		 .AllowReentrancy(GoToStateX);

		var target = await builder.Build(Initial);
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().BeEquivalentTo(enter, exit, enter);
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_enter_exit_and_transition_in_order(RaiseWay raiseWay)
	{
		var onEnter      = A.Fake<Action>();
		var onExit       = A.Fake<Action>();
		var onTransition = A.Fake<Action>();

		// --arrange
		var builder = new Builder<string, int>(OnException);
		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
		builder.DefineState(StateY);
		builder.DefineState(StateX)
					 .OnEnter(onEnter)
					 .OnExit(onExit)
					 .AddTransition(GoToStateY, StateY, onTransition);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);
		await target.RaiseAsync(raiseWay, GoToStateY);

		// --assert
		A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly()
		 .Then(A.CallTo(() => onExit()).MustHaveHappenedOnceExactly())
		 .Then(A.CallTo(() => onTransition()).MustHaveHappenedOnceExactly());
	}
}