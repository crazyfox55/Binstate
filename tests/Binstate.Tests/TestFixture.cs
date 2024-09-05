using System.Diagnostics;

namespace Binstate.Tests;

public class TestFixture
{
	public TestFixture()
	{
		Trace.Listeners.Clear();
		Trace.Listeners.Add(new ConsoleTraceListener());
	}
}