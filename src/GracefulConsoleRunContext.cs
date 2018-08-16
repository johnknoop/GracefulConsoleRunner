using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JohnKnoop.GracefulConsoleRunner
{
	public class GracefulConsoleRunContext
	{
		private readonly BlockingCollection<Task> _workInProcess = new BlockingCollection<Task>();

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
		public WorkWrapper BlockInterruption()
		{
			var wrapper = new WorkWrapper(true);

			if (!_workInProcess.TryAdd(wrapper.Work))
			{
				return new WorkWrapper(false);
			}

			return wrapper;
		}

		internal Task WhenWorkCompleted(TimeSpan? gracePeriod)
		{
			return Task.WhenAny(Task.WhenAll(_workInProcess), Task.Delay(gracePeriod ?? TimeSpan.FromMilliseconds(-1)));
		}

		internal void TerminationRequested() {
			_workInProcess.CompleteAdding();
		}
	}
}
