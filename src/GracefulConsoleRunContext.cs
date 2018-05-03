using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JohnKnoop.GracefulConsoleRunner
{
	public class GracefulConsoleRunContext
	{
		private readonly ConcurrentBag<Task> _workInProcess = new ConcurrentBag<Task>();

		public GracefulConsoleRunContext(CancellationToken applicationTermination)
		{
			ApplicationTermination = applicationTermination;
		}

		/// <summary>
		/// Use this token to determine if the application has been requested to terminate
		/// </summary>
		public CancellationToken ApplicationTermination { get; private set; }

		/// <summary>
		/// Creates a disposable that will hold off termination until it is disposed
		/// </summary>
		public IDisposable BlockInterruption()
		{
			var wrapper = new WorkWrapper();
			_workInProcess.Add(wrapper.Work);
			return wrapper;
		}

		public void WaitForCompletion(TimeSpan gracePeriod)
		{
			Task.WaitAll(_workInProcess.ToArray(), gracePeriod);
		}
	}
}
