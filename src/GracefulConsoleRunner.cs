using System;
using System.Threading;
using System.Threading.Tasks;

namespace JohnKnoop.GracefulConsoleRunner
{
	public static class GracefulConsoleRunner
	{
		public static void Run(Action<GracefulConsoleRunContext> run, Action cleanup, int gracePeriodSeconds = 10)
		{
			Func<Task> cleanupJob = async () =>
			{
				cleanup();
				await Task.CompletedTask;
			};

			Run(run, cleanupJob, gracePeriodSeconds);;
		}

		public static void Run(Action<GracefulConsoleRunContext> run, Func<Task> cleanup, int gracePeriodSeconds = 10)
		{
			var gracefulShutdown = new CancellationTokenSource();
			var runContext = new GracefulConsoleRunContext(gracefulShutdown.Token);

			Console.CancelKeyPress += (sender, eventArgs) =>
			{
				if (gracefulShutdown.IsCancellationRequested)
				{
					// Forceful termination
					Environment.Exit(0);
					return;
				}

				eventArgs.Cancel = true;
				gracefulShutdown.Cancel();

				runContext.WaitForCompletion();

				cleanup().GetAwaiter().GetResult();

				Environment.Exit(0);
			};

			Console.WriteLine("Press CTRL+C to exit");

			try
			{
				run(runContext);
			}
			catch
			{
				cleanup().GetAwaiter().GetResult();
				Environment.Exit(1);
			}

			while (true) Console.Read();
		}
	}
}