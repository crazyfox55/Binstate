﻿using System;
using System.Threading.Tasks;
using Binstate.Tests.Util;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Binstate.Tests;

public class InitialStateTest : StateMachineTestBase
{
	[Fact]
	public async Task should_call_enter_of_initial_state()
	{
		var onEnter = A.Fake<Action>();

		// --arrange
		var target = new Builder<string, int>(OnException);

		target.DefineState(Initial).OnEnter(onEnter).AddTransition(GoToStateX, StateX);
		target.DefineState(StateX);

		// --act
		await target.Build(Initial, "arg");

		// --assert
		A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
	}

	[Fact]
	public async Task should_pass_argument_to_initial_state_enter_action()
	{
		const string expected = "expected";
		var          onEnter  = A.Fake<Action<string>>();

		// --arrange
		var target = new Builder<string, int>(OnException);

		target.DefineState(Initial).OnEnter(onEnter).AddTransition(GoToStateX, StateX);
		target.DefineState(StateX);

		// --act
		await target.Build(Initial, expected);

		// --assert
		A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceAndOnly();
	}

	[Theory, MemberData(nameof(EnterExit))]
	public async Task should_pass_argument_to_initial_and_its_parents_states(
		Action<Config<string, int>.IEnter, Action<string>> setupRoot,
		Action<Config<string, int>.IEnter, Action<string>> setupParent)
	{
		const string expected     = "expected";
		var          onEnter      = A.Fake<Action<string>>();
		var          parentAction = A.Fake<Action<string>>();
		var          rootAction   = A.Fake<Action<string>>();

		// --arrange
		var target = new Builder<string, int>(OnException);

		target.DefineState(Root).With(_ => setupRoot(_, rootAction));
		target.DefineState(Parent).AsSubstateOf(Root).With(_ => setupParent(_, parentAction));
		target.DefineState(Initial).AsSubstateOf(Parent).OnEnter(onEnter).AddTransition(GoToStateX, StateX);
		target.DefineState(StateX);

		// --act
		var sm = await target.Build(Initial, expected);
		await sm.RaiseAsync(GoToStateX); // exit initial state

		// --assert
		A.CallTo(() => rootAction(expected)).MustHaveHappenedOnceAndOnly();
		A.CallTo(() => parentAction(expected)).MustHaveHappenedOnceAndOnly();
		A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceAndOnly();
	}

	[Fact]
	public Task should_throw_exception_if_initial_state_is_not_defined()
	{
		const string wrongState = "Wrong";

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial);

		// --act
		Func<Task> target = () => builder.Build(wrongState);

		// --assert
		return target.Should()
			.ThrowExactlyAsync<ArgumentException>()
			.WithMessage($"No state '{wrongState}' is defined");
	}

	[Fact]
	public Task should_throw_exception_if_initial_state_has_no_transition()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial);

		// --act
		Func<Task> target = () => builder.Build(Initial);

		// --assert
		return target.Should()
			.ThrowExactlyAsync<ArgumentException>()
			.WithMessage("No transitions defined from the initial state*");
	}

	[Fact]
	public async Task should_use_parent_transition_if_transition_from_initial_state_is_not_set()
	{
		var onEnterX = A.Fake<Action>();

		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Parent).AddTransition(GoToStateX, StateX);
		builder.DefineState(Initial).AsSubstateOf(Parent);
		builder.DefineState(StateX).OnEnter(onEnterX);
		var sm = await builder.Build(Initial);

		// --act
		await sm.RaiseAsync(GoToStateX);

		// --assert
		A.CallTo(() => onEnterX()).MustHaveHappenedOnceExactly();
	}

	[Fact]
	public Task should_throw_exception_if_initial_state_requires_argument_but_no_argument_is_specified()
	{
		// --arrange
		var builder = new Builder<string, int>(_ => { });

		builder.DefineState(Initial).OnEnter<string>(_ => { }).AllowReentrancy(GoToStateX);

		// --act
		Func<Task> target = () => builder.Build(Initial);

		// --assert
		return target.Should()
			.ThrowExactlyAsync<TransitionException>()
			.WithMessage("The state*");
	}

	[Fact]
	public Task should_throw_exception_if_parent_of_initial_state_requires_argument_but_no_argument_is_specified()
	{
		// --arrange
		var builder = new Builder<string, int>(_ => { });

		builder.DefineState(Parent).OnEnter<string>(_ => { }).AllowReentrancy(GoToStateX);
		builder.DefineState(Initial).AsSubstateOf(Parent);

		// --act
		Func<Task> target = () => builder.Build(Initial);

		// --assert
		return target.Should()
			.ThrowExactlyAsync<TransitionException>()
			.WithMessage("The state*");
	}
}