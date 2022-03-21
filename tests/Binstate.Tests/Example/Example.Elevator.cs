using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Binstate.Tests;

public partial class Example
{
	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class Elevator
	{
		private readonly IStateMachine<Events> _elevator;

		public int FloorNumber { get; private set; } = 0;
		public bool DoorsOpen { get; private set; } = false;

		public Elevator()
		{
			var builder = new Builder<States, Events>(Console.WriteLine);

			builder.DefineState(States.Healthy)
				.AddTransition(Events.Error, States.Error);

			builder.DefineState(States.Error)
				.AddTransition(Events.Reset, (out States state) =>
				{
					if(DoorsOpen)
					{
						if (FloorNumber == 0)
						{
							state = States.Level1Open;
						}
						else
						{
							state = States.Level2Open;
						}
					}
					else
					{
						if(FloorNumber == 0)
						{
							state = States.Level1Closed;
						}
						else
						{
							state = States.Level2Closed;
						}
					}
					return true;
				})
				.AllowReentrancy(Events.Error);

			builder.DefineState(States.Open)
				.AsSubstateOf(States.Healthy)
				.OnEnter(() => DoorsOpen = true);
			builder.DefineState(States.Closed)
				.AsSubstateOf(States.Healthy)
				.OnEnter(() => DoorsOpen = false);

			builder.DefineState(States.Level1Open)
				.AsSubstateOf(States.Open)
				.OnEnter(() => AnnounceFloor(0))
				.OnExit(() => Beep(2))
				.AddTransition(Events.GoUp, States.Level1Closed);

			builder.DefineState(States.Level2Open)
				.AsSubstateOf(States.Open)
				.OnEnter(() => AnnounceFloor(1))
				.OnExit(() => Beep(2))
				.AddTransition(Events.GoDown, States.Level2Closed);

			builder.DefineState(States.Level1Closed)
				.AsSubstateOf(States.Closed)
				.OnEnter(() => AnnounceFloor(0))
				.OnExit(() => Beep(2))
				.AddTransition(Events.Stop, States.Level1Open)
				.AddTransition(Events.GoUp, States.Level2Closed);

			builder.DefineState(States.Level2Closed)
				.AsSubstateOf(States.Closed)
				.OnEnter(() => AnnounceFloor(1))
				.OnExit(() => Beep(2))
				.AddTransition(Events.Stop, States.Level2Open)
				.AddTransition(Events.GoDown, States.Level1Closed);

			_elevator = builder.Build(States.Level1Closed).Result;

			// ready to work
		}

		public async Task GoTopFloor()
		{
			await _elevator.RaiseAsync(Events.GoUp);
			await _elevator.RaiseAsync(Events.GoUp);
			await _elevator.RaiseAsync(Events.Stop);
		}

		public async Task GoBottomFloor()
		{
			await _elevator.RaiseAsync(Events.GoDown);
			await _elevator.RaiseAsync(Events.GoDown);
			await _elevator.RaiseAsync(Events.Stop);
		}

		public Task Error() => _elevator.RaiseAsync(Events.Error);

		public Task Stop() => _elevator.RaiseAsync(Events.Stop);

		public Task Reset() => _elevator.RaiseAsync(Events.Reset);

		private void AnnounceFloor(int floor)
		{
			/* announce floor number */
			FloorNumber = floor;
		}

		private void AnnounceOverload()
		{
			/* announce overload */
		}

		private void Beep(int times)
		{
			/* beep */
		}

		private async Task CheckOverload(IStateController<Events> stateController)
		{
			while(stateController.InMyState)
			{
				if(IsOverloaded())
				{
					AnnounceOverload();
					await stateController.RaiseAsync(Events.Stop);
				}
				else
				{
					await Task.Delay(300);
				}
			}
		}

		private bool IsOverloaded() => false;

		private enum States { None, Healthy, Open, Closed, Level1Open, Level1Closed, Level2Open, Level2Closed, Error, }

		private enum Events { GoUp, GoDown, Stop, Error, Reset, }
	}
}