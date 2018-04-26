using System;
using System.Threading.Tasks;

namespace JohnKnoop.GracefulConsoleRunner
{
	public class WorkWrapper : IDisposable
	{
		private readonly TaskCompletionSource<object> _taskCompletionSource;

		internal WorkWrapper()
		{
			_taskCompletionSource = new TaskCompletionSource<object>();
			Work = _taskCompletionSource.Task;
		}

		public Task Work { get; }

		public void Dispose()
		{
			_taskCompletionSource.SetResult(null);
		}
	}
}