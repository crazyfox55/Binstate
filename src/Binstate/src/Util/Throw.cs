﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate;

internal static class Throw
{
	[DoesNotReturn]
	public static void NoArgument(IState state) =>
		throw new TransitionException(
			$"The state '{state}' requires argument of type '{state.GetArgumentType()}' but no argument of compatible type has passed to "
			+ $"the Raise(Async) method and no compatible argument is found in the currently active states");

	[DoesNotReturn]
	public static void ImpossibleException(IState targetState) =>
		throw new InvalidOperationException("This exception should never be thrown, because all verifications should be performed in the caller part. "
																				 + $"Target state = {targetState}");

	[DoesNotReturn]
	public static void ImpossibleException() =>
		throw new InvalidOperationException("This exception should never be thrown, because all verifications should be performed in the caller part. ");
}