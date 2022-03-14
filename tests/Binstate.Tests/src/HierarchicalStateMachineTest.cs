using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class HierarchicalStateMachineTest : StateMachineTestBase
{
	private const string Branch1Level1 = nameof(Branch1Level1);
	private const string Branch1Level2 = nameof(Branch1Level2);
	private const string Branch1Level3 = nameof(Branch1Level3);
	private const string Branch2Level1 = nameof(Branch2Level1);
	private const string Branch2Level2 = nameof(Branch2Level2);
	private const string Free1         = nameof(Free1);
	private const string Exit          = nameof(Exit);

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_enter_all_parent_states(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(Branch1Level3, Branch1Level3);

		builder
		 .DefineState(Root)
		 .OnEnter(() => actual.Add(Root));

		builder
		 .DefineState(Branch1Level1)
		 .AsSubstateOf(Root)
		 .OnEnter(() => actual.Add(Branch1Level1));

		builder
		 .DefineState(Branch1Level2)
		 .AsSubstateOf(Branch1Level1)
		 .OnEnter(() => actual.Add(Branch1Level2));

		builder
		 .DefineState(Branch1Level3)
		 .AsSubstateOf(Branch1Level2)
		 .OnEnter(() => actual.Add(Branch1Level3));

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, Branch1Level3);

		// --assert
		actual.Should().Equal(Root, Branch1Level1, Branch1Level2, Branch1Level3);
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_exit_all_parent_states(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		async Task EnterAsync(IStateController<string> stateMachine, string state)
		{
			while(stateMachine.InMyState)
				await Task.Delay(1);

			actual.Add(state + Exit);
		}

		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(Branch1Level3, Branch1Level3);

		builder
		 .DefineState(Root)
		 .OnRun(_ => EnterAsync(_, Root))
		 .OnExit(() => actual.Add(Root));

		builder
		 .DefineState(Branch1Level1)
		 .AsSubstateOf(Root)
		 .OnRun(_ => EnterAsync(_, Branch1Level1))
		 .OnExit(() => actual.Add(Branch1Level1));

		builder
		 .DefineState(Branch1Level2)
		 .AsSubstateOf(Branch1Level1)
		 .OnRun(_ => EnterAsync(_, Branch1Level2))
		 .OnExit(() => actual.Add(Branch1Level2));

		builder
		 .DefineState(Branch1Level3)
		 .AsSubstateOf(Branch1Level2)
		 .OnRun(_ => EnterAsync(_, Branch1Level3))
		 .OnExit(() => actual.Add(Branch1Level3))
		 .AddTransition(Free1, Free1);

		builder
		 .DefineState(Free1)
		 .OnEnter(() => actual.Add(Free1));

		var target = await builder.Build(Initial);
		await target.RaiseAsync(raiseWay, Branch1Level3);

		// --act
		await target.RaiseAsync(raiseWay, Free1);

		// --assert
		actual.Should()
					.Equal(Branch1Level3 + Exit, Branch1Level3, Branch1Level2 + Exit, Branch1Level2, Branch1Level1 + Exit, Branch1Level1, Root + Exit, Root, Free1);
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_not_exit_parent_state(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		async Task EnterAsync(IStateController<string> stateMachine, string state)
		{
			while(stateMachine.InMyState)
				await Task.Delay(1);

			actual.Add(state + Exit);
		}

		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(Branch1Level2, Branch1Level2);

		builder
		 .DefineState(Root)
		 .OnRun(_ => EnterAsync(_, Root))
		 .OnExit(() => actual.Add(Root));

		builder
		 .DefineState(Branch1Level1)
		 .AsSubstateOf(Root)
		 .OnRun(_ => EnterAsync(_, Branch1Level1))
		 .OnExit(() => actual.Add(Branch1Level1));

		builder
		 .DefineState(Branch1Level2)
		 .AsSubstateOf(Branch1Level1)
		 .OnRun(_ => EnterAsync(_, Branch1Level2))
		 .OnExit(() => actual.Add(Branch1Level2))
		 .AddTransition(Branch1Level3, Branch1Level3);

		builder
		 .DefineState(Branch1Level3)
		 .AsSubstateOf(Branch1Level2)
		 .OnEnter(() => actual.Add(Branch1Level3));

		var target = await builder.Build(Initial);
		await target.RaiseAsync(raiseWay, Branch1Level2);

		// --act
		await target.RaiseAsync(raiseWay, Branch1Level3);

		// --assert
		actual.Should().Equal(Branch1Level3);
	}

	[TestCaseSource(nameof(RaiseWays))]
	public async Task should_not_exit_common_root(RaiseWay raiseWay)
	{
		var actual = new List<string>();

		async Task RootEnterAsync(IStateController<string> stateMachine)
		{
			actual.Add(Root);

			while(stateMachine.InMyState)
				await Task.Delay(1);

			actual.Add(Root + Exit);
		}

		// --arrange
		var builder = new Builder<string, string>(OnException);

		builder
		 .DefineState(Initial)
		 .AddTransition(Branch1Level2, Branch1Level2);

		builder
		 .DefineState(Root)
		 .OnRun(RootEnterAsync)
		 .OnExit(() => actual.Add(Root));

		builder
		 .DefineState(Branch1Level1)
		 .AsSubstateOf(Root)
		 .OnExit(() => actual.Add(Branch1Level1));

		builder
		 .DefineState(Branch1Level2)
		 .AsSubstateOf(Branch1Level1)
		 .OnExit(() => actual.Add(Branch1Level2))
		 .AddTransition(Branch2Level2, Branch2Level2);

		builder
		 .DefineState(Branch2Level1)
		 .AsSubstateOf(Root)
		 .OnEnter(() => actual.Add(Branch2Level1));

		builder
		 .DefineState(Branch2Level2)
		 .AsSubstateOf(Branch2Level1)
		 .OnEnter(() => actual.Add(Branch2Level2));

		var target = await builder.Build(Initial);
		await target.RaiseAsync(raiseWay, Branch1Level2);

		// --act
		await target.RaiseAsync(raiseWay, Branch2Level2);

		// --assert
		actual.Should().Equal(Root, Branch1Level2, Branch1Level1, Branch2Level1, Branch2Level2);
	}
}