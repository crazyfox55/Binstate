using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;


namespace Binstate.Tests;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class ArgumentPassingTest : StateMachineTestBase
{
	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_pass_argument_to_enter(RaiseWay raiseWay)
	{
		const string expected = "expected";
		var          actual   = expected + "bad";

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(GoToStateX, StateX);

		builder
		 .DefineState(StateX)
		 .OnEnter<string>((param) => actual = param);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX, expected);

		// --assert
		actual.Should().Be(expected);
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_pass_argument_if_argument_is_differ_but_assignable_to_enter_action_argument(RaiseWay raiseWay)
	{
		var expected = new MemoryStream();
		var onEnter  = A.Fake<Action<IDisposable>>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
		builder.DefineState(StateX).OnEnter(onEnter);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX, expected);

		// --assert
		A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceExactly();
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_pass_argument_if_parent_and_child_argument_are_differ_but_assignable_and_enter_with_no_argument_on_the_pass(RaiseWay raiseWay)
	{
		var onEnterRoot  = A.Fake<Action<IDisposable>>();
		var onEnterChild = A.Fake<Action<Stream>>();
		var expected     = new MemoryStream();

		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder.DefineState(Initial).AddTransition(Child, Child);
		builder.DefineState(Root).OnEnter(onEnterRoot);
		builder.DefineState(Parent).AsSubstateOf(Root);
		builder.DefineState(Child).AsSubstateOf(Parent).OnEnter(onEnterChild);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, Child, expected);

		// --assert
		A.CallTo(() => onEnterRoot(expected)).MustHaveHappenedOnceExactly();
		A.CallTo(() => onEnterChild(expected)).MustHaveHappenedOnceExactly();
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_throw_exception_if_argument_is_not_assignable_to_enter_action(RaiseWay raiseWay)
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(GoToStateX, StateX);

		builder
		 .DefineState(StateX)
		 .OnEnter<string>((value) => { });

		var stateMachine = await builder.Build(Initial);

		// --act
		Func<Task> target = () => stateMachine.RaiseAsync(raiseWay, GoToStateX, 983);

		// --assert
		await target.Should()
			.ThrowExactlyAsync<TransitionException>()
			.WithMessage($"The state '{StateX}' requires argument of type '{typeof(string)}' but no argument*");
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_throw_exception_if_no_argument_specified_for_enter_action_with_argument(RaiseWay raiseWay)
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(GoToStateX, StateX);

		builder
		 .DefineState(StateX)
		 .OnEnter<int>(value => { });

		var stateMachine = await builder.Build(Initial);

		// --act
		Func<Task> target = () => stateMachine.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		await target.Should()
			.ThrowExactlyAsync<TransitionException>()
			.WithMessage($"The state '{StateX}' requires argument of type '{typeof(int)}' but no argument*");
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_throw_exception_if_parent_and_child_state_has_not_assignable_arguments_enable_free_mode_and_argument_is_passed(RaiseWay way)
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).AddTransition(GoToStateX, Child);

		builder.DefineState(Parent)
					 .OnEnter<int>(value => { });

		builder.DefineState(Child)
					 .AsSubstateOf(Parent)
					 .OnEnter<string>(value => { });

		// --act
		var sm = await builder.Build(Initial, ArgumentTransferMode.Free);

		Func<Task> target = () => sm.RaiseAsync(GoToStateX, "stringArgument");

		// --assert
		await target.Should()
			.ThrowAsync<TransitionException>()
			.WithMessage($"The state '{Parent}' requires argument of type '{typeof(int)}' but no argument*");
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_pass_the_same_argument_to_enter_exit_and_transition(RaiseWay raiseWay)
	{
		var expected     = new MemoryStream();
		var onEnter      = A.Fake<Action<IDisposable>>();
		var onExit       = A.Fake<Action<IDisposable>>();
		var onTransition = A.Fake<Action<IDisposable>>();

		// --arrange
		var builder = new Builder<string, int>(OnException);
		builder.DefineState(Initial)
			.AddTransition(GoToStateX, StateX);
		builder.DefineState(StateY);
		builder.DefineState(StateX)
			.OnEnter(onEnter)
			.OnExit(onExit)
			.AddTransition(GoToStateY, StateY, onTransition);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(GoToStateX, expected);
		await target.RaiseAsync(GoToStateY);

		// --assert
		A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceExactly()
			.Then(A.CallTo(() => onExit(expected)).MustHaveHappenedOnceExactly())
			.Then(A.CallTo(() => onTransition(expected)).MustHaveHappenedOnceExactly());
	}
}