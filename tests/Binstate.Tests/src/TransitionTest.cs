using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Binstate.Tests;

public class TransitionTest : StateMachineTestBase
{
	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_action_on_transition_between_exit_and_enter(RaiseWay raiseWay)
	{
		var onExitInitial = A.Fake<Action>();
		var onTransit = A.Fake<Action>();
		var onEnterState1 = A.Fake<Action>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial)
					 .OnExit(onExitInitial)
					 .AddTransition(GoToStateX, StateX, onTransit);

		builder.DefineState(StateX)
					 .OnEnter(onEnterState1);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		A.CallTo(() => onExitInitial()).MustHaveHappenedOnceExactly()
		 .Then(A.CallTo(() => onTransit()).MustHaveHappenedOnceExactly())
		 .Then(A.CallTo(() => onEnterState1()).MustHaveHappenedOnceExactly());
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_call_action_w_argument_on_transition_between_exit_and_enter(RaiseWay raiseWay)
	{
		var expected = new MemoryStream();

		var onExitInitial = A.Fake<Action>();
		var onTransit = A.Fake<Action<IDisposable>>();
		var onEnterState1 = A.Fake<Action>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial)
					 .OnEnter<IDisposable>(_ => { })
					 .OnExit(onExitInitial)
					 .AddTransition(GoToStateX, StateX, onTransit);

		builder.DefineState(StateX)
					 .OnEnter(onEnterState1);

		var target = await builder.Build(Initial, expected);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		A.CallTo(() => onExitInitial())
		 .MustHaveHappenedOnceExactly()
		 .Then(A.CallTo(() => onTransit(expected)).MustHaveHappenedOnceExactly())
		 .Then(A.CallTo(() => onEnterState1()).MustHaveHappenedOnceExactly());
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task raise_should_return_false_if_no_transition_found(RaiseWay raiseWay)
	{
		// --arrange
		var builder = new Builder<string, string>(OnException);
		builder.DefineState(Initial).AddTransition(StateX, StateX);
		builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

		var target = await builder.Build(Initial);

		// --act
		var actual = await target.RaiseAsync(raiseWay, "WrongEvent");

		// --assert
		actual.Should().BeFalse();
	}

	[Fact]
	public async Task controller_should_return_false_if_no_transition_found()
	{
		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder.DefineState(Initial)
			.OnRun(OnEnterInitialState)
			.AddTransition(StateX, StateX);

		builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

		await builder.Build(Initial);

		static async Task OnEnterInitialState(IStateController<string> stateMachine)
		{
			// --act
			var actual = await stateMachine.RaiseAsync("WrongEvent");

			// --assert
			actual.Should().BeFalse();
		}
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_transit_using_dynamic_transition(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		var first = true;

		builder
		 .DefineState(Initial)
		 .AddTransition(
				GoToStateX,
				getState: (out string? state) =>
				{
					state = first ? StateX : StateY;
					first = false;

					return true;
				}
			);

		builder
		 .DefineState(StateX)
		 .AsSubstateOf(Initial)
		 .OnEnter(() => actual.Add(StateX));

		builder
		 .DefineState(StateY)
		 .OnEnter(() => actual.Add(StateY));

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().BeEquivalentTo(StateX, StateY);
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_transit_using_dynamic_transition_using_value_type_default(RaiseWay raiseWay)
	{
		const int initialStateId = 1;
		const int stateId1 = 0; // default value
		const int stateId2 = 38;

		var first = true;

		bool DynamicTransition(out int state)
		{
			state = first ? stateId1 : stateId2;
			first = false;

			return true;
		}

		var actual = new List<int>();

		// --arrange
		var builder = new Builder<int, int>(OnException);

		builder
		 .DefineState(initialStateId)
		 .AddTransition(GoToStateX, DynamicTransition);

		builder
		 .DefineState(stateId1)
		 .AsSubstateOf(initialStateId)
		 .OnEnter(() => actual.Add(stateId1));

		builder
		 .DefineState(stateId2)
		 .OnEnter(() => actual.Add(stateId2));

		var target = await builder.Build(initialStateId);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().Equal(stateId1, stateId2);
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task raise_should_return_false_if_dynamic_transition_returns_false_value_type(RaiseWay raiseWay)
	{
		const int initialStateId = 1;
		const int stateId = 2;

		static bool DynamicTransition(out int state)
		{
			state = stateId;
			return false;
		}

		// --arrange
		var builder = new Builder<int, int>(OnException);

		builder.DefineState(initialStateId)
					 .AddTransition(GoToStateX, DynamicTransition);

		builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

		var target = await builder.Build(initialStateId);

		// --act
		var actual = await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().BeFalse();
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task raise_should_return_false_if_dynamic_transition_returns_false_reference_type(RaiseWay raiseWay)
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		static bool DynamicTransition(out string stateId)
		{
			stateId = StateX;

			return false;
		}

		builder.DefineState(Initial)
					 .AddTransition(GoToStateX, DynamicTransition);

		builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

		var target = await builder.Build(Initial);

		// --act
		var actual = await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().BeFalse();
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task raise_should_return_false_if_dynamic_transition_returns_null(RaiseWay raiseWay)
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial)
			.AddTransition(GoToStateX, (out string? state) =>
			{
				state = null;
				return false;
			});

		builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

		var target = await builder.Build(Initial);

		// --act
		var actual = await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().BeFalse();
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task raise_should_return_false_if_dynamic_transition_returns_default(RaiseWay raiseWay)
	{
		const int initialStateId = 1;
		const int stateId = 2;

		// --arrange
		var builder = new Builder<int, int>(OnException);

		builder.DefineState(initialStateId)
			.AddTransition(GoToStateX, getState: (out int state) =>
			{
				state = default;
				return false;
			});

		builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

		var target = await builder.Build(initialStateId);

		// --act
		var actual = await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		actual.Should().BeFalse();
	}

	[Fact]
	public async Task controller_should_return_false_if_dynamic_transition_returns_null()
	{
		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder.DefineState(Initial)
			.OnRun(OnEnterInitialState)
			.AddTransition(StateX, (out string? state) =>
			{
				state = null;
				return false;
			});

		builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

		await builder.Build(Initial);

		static async Task OnEnterInitialState(IStateController<string> stateMachine)
		{
			// --act
			var actual = await stateMachine.RaiseAsync(StateX);

			// --assert
			actual.Should().BeFalse();
		}
	}

	[Fact]
	public void controller_should_return_false_if_dynamic_transition_returns_default()
	{
		const int initialStateId = 1;
		const int stateId = 2;

		// --arrange
		var builder = new Builder<int, int>(OnException);

		builder.DefineState(initialStateId)
			.OnRun(OnEnterInitialState)
			.AddTransition(GoToStateX, (out int state) =>
			{
				state = default;
				return false;
			});

		builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

		builder.Build(initialStateId);

		static async Task OnEnterInitialState(IStateController<int> stateMachine)
		{
			// --act
			var actual = await stateMachine.RaiseAsync(GoToStateX);

			// --assert
			actual.Should().BeFalse();
		}
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_use_parent_transition(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder.DefineState(Initial).AddTransition(Child, Child);

		builder.DefineState(Parent)
			.AddTransition(StateX, StateX, () => actual.Add(Parent));

		builder.DefineState(Child).AsSubstateOf(Parent);

		builder.DefineState(StateX)
			.OnEnter(() => actual.Add(StateX));

		var target = await builder.Build(Initial);
		await target.RaiseAsync(raiseWay, Child);

		// --act
		await target.RaiseAsync(raiseWay, StateX);

		// --assert
		actual.Should().Equal(Parent, StateX);
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_catch_user_action_exception_and_report(RaiseWay raiseWay)
	{
		var onException = A.Fake<Action<Exception>>();

		// --arrange
		var builder = new Builder<string, string>(onException);

		builder.DefineState(Initial)
					 .AddTransition(StateX, StateX, () => throw new TestException());

		builder.DefineState(StateX);

		var target = await builder.Build(Initial);

		// --act
		var actual = await target.RaiseAsync(raiseWay, StateX);

		// --assert
		actual.Should().BeTrue();
		A.CallTo(() => onException(An<Exception>.That.Matches(exc => exc is TestException))).MustHaveHappenedOnceExactly();
	}

	[Fact]
	public void should_throw_exception_if_transitions_to_different_states_by_one_event()
	{
		var builder = new Builder<string, int>(OnException);

		var config = builder
								.DefineState(Initial)
								.AddTransition(GoToStateX, StateX);

		// --act
		Action target = () => config.AddTransition(GoToStateX, StateY);

		// --assert
		target.Should().ThrowExactly<ArgumentException>().WithMessage("An item with the same key has already been added*");
	}

	[Theory, MemberData(nameof(RaiseWays))]
	public async Task should_not_perform_transition_if_dynamic_transition_throws_exception(RaiseWay raiseWay)
	{
		var onException = A.Fake<Action<Exception>>();

		// --arrange
		var builder = new Builder<string, int>(onException);

		builder
		 .DefineState(Initial)
		 .AddTransition(GoToStateX, (out string? state) => throw new TestException());

		var target = await builder.Build(Initial);

		// --act
		var result = await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		result.Should().BeFalse();
		A.CallTo(() => onException(An<Exception>.That.Matches(exc => exc is TestException))).MustHaveHappenedOnceExactly();
	}

	[Theory(Timeout=1000)]
	[InlineData(true)]
	[InlineData(false)]
	public async Task should_perform_internal_or_external_transition(bool internalTransition)
	{
		// --arrange
		var entered = new ManualResetEvent(false);

		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial)
			.AddTransition(GoToStateX, StateX);

		builder.DefineState(StateX)
			.OnRun(async sc =>
			{
				await Task.Delay(100);
				var internalHappened = await sc.RaiseAsync(GoToStateY);
				internalHappened.Should().Be(internalTransition);
			})
			.AddTransition(GoToStateY, StateY);

		builder.DefineState(StateY)
			.OnEnter(() => {
					entered.Set();
				});

		var target = await builder.Build(Initial);

		await target.RaiseAsync(GoToStateX);

		if(internalTransition)
		{
			await Task.Delay(200);
		}

		var externalHappened = await target.RaiseAsync(GoToStateY);
		externalHappened.Should().NotBe(internalTransition);
		
		Assert.True(entered.WaitOne());
	}
}