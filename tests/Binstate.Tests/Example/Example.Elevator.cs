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
		public bool MovingUp { get; private set; } = false;
		public bool MovingDown { get; private set; } = false;

		public Elevator()
		{
			var builder = new Builder<States, Events>(Console.WriteLine);

			builder
			 .DefineState(States.Healthy)
			 .AddTransition(Events.Error, States.Error);

			builder
			 .DefineState(States.Error)
			 .AddTransition(Events.Reset, States.Healthy)
			 .AllowReentrancy(Events.Error);

			builder
			 .DefineState(States.OnFloor)
			 .AsSubstateOf(States.Healthy)
			 .OnEnter(AnnounceFloor)
			 .OnExit(() => Beep(2))
			 .AddTransition(Events.CloseDoor, States.DoorClosed)
			 .AddTransition(Events.OpenDoor, States.DoorOpen);

			builder
			 .DefineState(States.Moving)
			 .AsSubstateOf(States.Healthy)
			 .OnRun(CheckOverload)
			 .AddTransition(Events.Stop, States.OnFloor);

			builder.DefineState(States.MovingUp).AsSubstateOf(States.Moving)
				.OnEnter(() => MovingUp = true)
				.OnRun<int>(async (_, floor) =>
				{
					while(floor != FloorNumber)
					{
						FloorNumber++;
						await Task.Delay(100);
					}
				})
				.OnExit(() => MovingUp = false);
			builder.DefineState(States.MovingDown).AsSubstateOf(States.Moving)
				.OnEnter(() => MovingDown = true)
				.OnRun<int>(async (sc, floor) =>
				{
					while(floor != FloorNumber)
					{
						FloorNumber--;
						await Task.Delay(100);
					}

					await sc.RaiseAsync(Events.Stop);
				})
				.OnExit(() => MovingDown = false);

			builder.DefineState(States.DoorClosed).AsSubstateOf(States.OnFloor)
				.AddTransition(Events.GoUp, States.MovingUp)
				.AddTransition(Events.GoDown, States.MovingDown);

			builder.DefineState(States.DoorOpen).AsSubstateOf(States.OnFloor);

			_elevator = builder.Build(States.OnFloor).Result;

			// ready to work
		}

		public async Task CallFloor(int floorNumber)
		{
			await _elevator.RaiseAsync(Events.Stop);
			await _elevator.RaiseAsync(Events.CloseDoor);
			if(floorNumber > FloorNumber)
			{
				await _elevator.RaiseAsync(Events.GoUp, floorNumber);
			}
			else
			{
				await _elevator.RaiseAsync(Events.GoDown, floorNumber);
			}
			await _elevator.RaiseAsync(Events.Stop);
			await _elevator.RaiseAsync(Events.OpenDoor);
		}

		public Task Error() => _elevator.RaiseAsync(Events.Error);

		public Task Stop() => _elevator.RaiseAsync(Events.Stop);

		public Task Reset() => _elevator.RaiseAsync(Events.Reset);

		private void AnnounceFloor()
		{
			/* announce floor number */
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

		private enum States { None, Healthy, OnFloor, Moving, MovingUp, MovingDown, DoorOpen, DoorClosed, Error, }

		private enum Events { GoUp, GoDown, OpenDoor, CloseDoor, Stop, Error, Reset, }
	}
}