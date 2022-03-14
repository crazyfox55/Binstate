using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
	/// <summary>
	/// 
	/// </summary>
	public interface IRun : IExit
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit OnRun(Action runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit OnRun(Action<IStateController<TEvent>> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit OnRun(Func<Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit OnRun(Func<IStateController<TEvent>, Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<TArgument> OnRun<TArgument>(Action<TArgument> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<TArgument> OnRun<TArgument>(Action<IStateController<TEvent>, TArgument> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<TArgument> OnRun<TArgument>(Func<TArgument, Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<TArgument> OnRun<TArgument>(Func<IStateController<TEvent>, TArgument, Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <typeparam name="TRelay"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Action<TArgument, TRelay> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <typeparam name="TRelay"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Action<IStateController<TEvent>, TArgument, TRelay> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <typeparam name="TRelay"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Func<TArgument, TRelay, Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TArgument"></typeparam>
		/// <typeparam name="TRelay"></typeparam>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<ITuple<TArgument, TRelay>> OnRun<TArgument, TRelay>(Func<IStateController<TEvent>, TArgument, TRelay, Task> runAction);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRun<out T> : IExit<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Action runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Action<IStateController<TEvent>> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Func<Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Func<IStateController<TEvent>, Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Action<T> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Action<IStateController<TEvent>, T> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Func<T, Task> runAction);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="runAction"></param>
		/// <returns></returns>
		IExit<T> OnRun(Func<IStateController<TEvent>, T, Task> runAction);
	}
}
