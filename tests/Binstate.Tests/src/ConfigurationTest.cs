﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
public class ConfigurationTest : StateMachineTestBase
{
	[Fact]
	public void should_throw_exception_if_pass_null_to_as_substate_of()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		// --act
		Action target = () => builder.DefineState(Initial).AsSubstateOf(null!);

		// --assert
		target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*parentStateId*");
	}

	[Fact]
	public void should_throw_exception_if_pass_null_to_define_state()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		// --act
		Action target = () => builder.DefineState(null!);

		// --assert
		target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*stateId*");
	}

	[Fact]
	public void should_throw_exception_if_pass_null_to_get_or_define_state()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		// --act
		Action target = () => builder.GetOrDefineState(null!);

		// --assert
		target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*stateId*");
	}

	[Fact]
	public Task should_throw_exception_if_pass_null_initial_state_id()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);
		builder.DefineState(Initial).AllowReentrancy(GoToStateX);

		// --act
		Func<Task> target = () => builder.Build(null!);

		// --assert
		return target.Should()
			.ThrowExactlyAsync<ArgumentNullException>()
			.WithMessage("*initialStateId*");
	}

	[Fact]
	public void on_enter_should_check_arguments_for_null()
	{
		// --arrange
		var builder = new Builder<string, string>(OnException);
		var config  = builder.DefineState(Initial);

#pragma warning disable 8625

		// --act
		Action target01 = () => config.OnEnter((Action)null!);
		Action target02 = () => config.OnEnter((Func<Task>)null!);

		Action target03 = () => config.OnEnter((Action<object>)null!);
		Action target04 = () => config.OnEnter((Func<object, Task>)null!);

		Action target05 = () => config.OnEnter((Action<object, object>)null!);
		Action target06 = () => config.OnEnter((Func<object, object, Task>)null!);
#pragma warning restore 8625

		// --assert
		target01.Should().ThrowExactly<ArgumentNullException>();
		target02.Should().ThrowExactly<ArgumentNullException>();
		target03.Should().ThrowExactly<ArgumentNullException>();
		target04.Should().ThrowExactly<ArgumentNullException>();
		target05.Should().ThrowExactly<ArgumentNullException>();
		target06.Should().ThrowExactly<ArgumentNullException>();
	}

	[Fact]
	public void on_exit_should_check_arguments_for_null()
	{
		// --arrange
		var builder = new Builder<string, string>(OnException);
		var config  = builder.DefineState(Initial);

#pragma warning disable 8625

		// --act
		Action target01 = () => config.OnExit(null!);
		Action target03 = () => config.OnExit((Action<object>)null!);
#pragma warning restore 8625

		// --assert
		target01.Should().ThrowExactly<ArgumentNullException>();
		target03.Should().ThrowExactly<ArgumentNullException>();
	}

	[Fact]
	public void on_enter_should_not_accept_async_void_method()
	{
		// --arrange
		var builder = new Builder<string, string>(OnException);
		var config  = builder.DefineState(Initial);

#pragma warning disable 1998
		async void AsyncMethod1()                                   { }
		async void AsyncMethod2(object                _)            { }
		async void AsyncMethod3(object                _, object __) { }
#pragma warning restore 1998

		// --act
		Action target1 = () => config.OnEnter(AsyncMethod1);
		Action target2 = () => config.OnEnter<object>(AsyncMethod2);
		Action target3 = () => config.OnEnter<object, object>(AsyncMethod3);

		// --assert
		target1.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
		target2.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
		target3.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
	}

	[Fact]
	public void add_transition_should_check_arguments_for_null()
	{
		static bool GetState(out string? _)
		{
			_ = null;
			return false;
		}

		// --arrange
		var builder = new Builder<string, string>(OnException);
		var config  = builder.DefineState(Initial);

		// --act
#pragma warning disable 8625
		Action target1 = () => config.AddTransition(null,    Initial);
		Action target2 = () => config.AddTransition(Initial, null, null!);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
		Action target5 = () => config.AddTransition(null,    (out string? s) => GetState(out s));
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
		Action target6 = () => config.AddTransition(Initial, (GetState<string>)null!);
#pragma warning restore 8625

		// --assert
		target1.Should().ThrowExactly<ArgumentNullException>();
		target2.Should().ThrowExactly<ArgumentNullException>();
		target5.Should().ThrowExactly<ArgumentNullException>();
		target6.Should().ThrowExactly<ArgumentNullException>();
	}

	[Fact]
	public void define_state_should_throw_exception_on_define_already_defined_state()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.DefineState(Initial);

		// --act
		Action target = () => builder.DefineState(Initial);

		// --assert
		target.Should().ThrowExactly<ArgumentException>();
	}

	[Fact]
	public void get_or_define_state_should_return_existent_state_if_already_defined()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		var expected = builder.DefineState(Initial);

		// --act
		var actual = builder.GetOrDefineState(Initial);

		// --assert
		actual.Should().BeSameAs(expected);
	}

	[Fact]
	public void get_or_define_state_should_define_state_if_still_no()
	{
		// --arrange
		var builder = new Builder<string, int>(OnException);

		builder.GetOrDefineState(Initial).AllowReentrancy(GoToStateX);

		// --act
		var actual = builder.Build(Initial);

		// --assert
		actual.Should().NotBeNull();
	}
}