using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Binstate.Tests;

public class MultiThreadingTest : StateMachineTestBase
{
	[Fact]
	public async Task multiple_threads_should_execute_without_overlapping()
	{
		//arrange
		var actual = new ConcurrentStack<string>();
		var syncronize = new ManualResetEvent(false);
		var threadOneDone = new ManualResetEvent(false);
		var threadTwoDone = new ManualResetEvent(false);

		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

		builder.DefineState(StateX)
			.OnEnter(() => actual.Push(StateX))
			.AddTransition(GoToStateY, StateY, () => actual.Push("X to Y"));

		builder.DefineState(StateY)
			.OnEnter(() => actual.Push(StateY))
			.AddTransition(GoToStateX, StateX);

		var target = await builder.Build(Initial);

		await target.RaiseAsync(GoToStateX);

		var threads = new List<Thread>()
		{
			new Thread(async () => {
				syncronize.WaitOne();
				// try to go to state y 100 times
				for (var i = 0; i < 100; i++)
				{
					if (!await target.RaiseAsync(GoToStateY))
					{
						i--;
					}
				}
				threadOneDone.Set();
			}),
			new Thread(async () => {
				syncronize.WaitOne();
				// try to go to state x 100 times
				for (var i = 0; i < 100; i++)
				{
					if (!await target.RaiseAsync(GoToStateX))
					{
						i--;
					}
				}
				threadTwoDone.Set();
			})
		};

		foreach(var thread in threads)
		{
			thread.Start();
		}

		syncronize.Set();

		threadOneDone.WaitOne();
		threadTwoDone.WaitOne();

		var stateXCount = 0;
		var stateYCount = 0;
		var stateXToStateYCount = 0;
		foreach(var item in actual)
		{
			if (item == StateX)
			{
				stateXCount++;
			}
			else if (item == StateY)
			{
				stateYCount++;
			}
			else if (item == "X to Y")
			{
				stateXToStateYCount++;
			}
		}
		actual.TryPeek(out var top);

		Assert.Equal(StateX, top);

		Assert.Equal(101, stateXCount);
		Assert.Equal(100, stateYCount);
		Assert.Equal(100, stateXToStateYCount);
	}

	[Fact]
	public async Task eternal_async_run_should_be_stopped_by_changing_state()
	{
		const string enter = nameof(enter);
		const string exit  = nameof(exit);

		// --arrange
		var actual  = new List<string>();
		var entered = new ManualResetEvent(false);

		void BlockingRun(IStateController<int> machine)
		{
			entered.Set();
			while(machine.InMyState) Thread.Sleep(100);
			actual.Add(enter);
		}

		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

		builder.DefineState(StateX)
			.OnRun(BlockingRun)
			.OnExit(() => actual.Add(exit))
			.AddTransition(GoToStateY, StateY);

		builder.DefineState(StateY)
			.OnEnter(() => actual.Add(StateY));

		var target = await builder.Build(Initial);

		await target.RaiseAsync(GoToStateX); // raise async to not to block test execution
		entered.WaitOne(1000);         // wait till OnEnter will block execution

		// --act
		await target.RaiseAsync(GoToStateY);

		// -- assert
		actual.Should().Equal(enter, exit, StateY);
	}

	[Theory(Timeout=5000), MemberData(nameof(RaiseWays))]
	public async Task async_run_should_not_block(RaiseWay raiseWay)
	{
		// --arrange
		var entered = new ManualResetEvent(false);

		async Task AsyncRun(IStateController<int> stateMachine)
		{
			entered.Set();
			while(stateMachine.InMyState) await Task.Delay(546);
		}

		var builder = new Builder<string, int>(OnException);
		builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

		builder
		 .DefineState(StateX)
		 .OnRun(AsyncRun)
		 .AddTransition(GoToStateY, StateY);

		builder.DefineState(StateY);

		var target = await builder.Build(Initial);

		// --act
		await target.RaiseAsync(raiseWay, GoToStateX);

		// --assert
		entered.WaitOne(TimeSpan.FromSeconds(4)).Should().BeTrue();

		// --cleanup
		await target.RaiseAsync(GoToStateY); // exit async method
	}
}