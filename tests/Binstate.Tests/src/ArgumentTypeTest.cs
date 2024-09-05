using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Binstate.Tests.Util;
using FakeItEasy;
using Xunit;

namespace Binstate.Tests;

public class ArgumentTypeTest : StateMachineTestBase
{
	[Fact]
	[Description("If argument type is set by OnEnter action, no parameters, Exit and Transition should work")]
	public async Task set_type_in_enter()
	{
		const string expected = "arg";

		var onEnter      = A.Fake<Action<string>>();
		var onExit       = A.Fake<Action>();
		var onTransition = A.Fake<Action>();

		// --arrange
		var target = new Builder<string, int>(OnException);

		target.DefineState(Initial)
					.OnEnter(onEnter)
					.OnExit(onExit)
					.AddTransition(GoToStateX, StateX, onTransition);

		target.DefineState(StateX);

		// --act
		var sm = await target.Build(Initial, expected);

		await sm.RaiseAsync(GoToStateX);

		// --assert
		A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceExactly();
		A.CallTo(() => onExit()).MustHaveHappenedOnceAndOnly();
		A.CallTo(() => onTransition()).MustHaveHappenedOnceExactly();
	}

	[Fact]
	[Description("If argument type is set by OnExit action, no parameters, Enter and Transition should work")]
	public async Task set_type_in_exit()
	{
		const string expected = "arg";

		var onEnter      = A.Fake<Action>();
		var onExit       = A.Fake<Action<string>>();
		var onTransition = A.Fake<Action>();

		// --arrange
		var target = new Builder<string, int>(OnException);

		target.DefineState(Initial)
					.OnEnter(onEnter)
					.OnExit(onExit)
					.AddTransition(GoToStateX, StateX, onTransition);

		target.DefineState(StateX);

		// --act
		var sm = await target.Build(Initial, expected);

		await sm.RaiseAsync(GoToStateX);

		// --assert
		A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
		A.CallTo(() => onExit(expected)).MustHaveHappenedOnceAndOnly();
		A.CallTo(() => onTransition()).MustHaveHappenedOnceExactly();
	}

	[Fact]
	[Description("If argument type is set by OnTransition action, no parameters, Enter and Exit should work")]
	public async Task set_type_in_transition()
	{
		const string expected = "arg";

		var onEnter      = A.Fake<Action>();
		var onExit       = A.Fake<Action>();
		var onTransition = A.Fake<Action<string>>();

		// --arrange
		var target = new Builder<string, int>(OnException);

		target.DefineState(Initial)
					.OnEnter(onEnter)
					.OnExit(onExit)
					.AddTransition(GoToStateX, StateX, onTransition);

		target.DefineState(StateX);

		// --act
		var sm = await target.Build(Initial, expected);
		await sm.RaiseAsync(GoToStateX);

		// --assert
		A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
		A.CallTo(() => onExit()).MustHaveHappenedOnceExactly();
		A.CallTo(() => onTransition(expected)).MustHaveHappenedOnceAndOnly();
	}
}