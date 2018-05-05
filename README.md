## Install
    PM> Install-Package JohnKnoop.GracefulConsoleRunner

This is a .NET Standard 1.5 library.

## When to use?

Add graceful termination of your event-driven application without risking partially handled events.

### Typical use case:
Your app consumes messages off a message queue, and saves a new object to your database for each message it receives, and then acks the message so the queue knows it has been processed. The queue supports exactly-once-delivery using deduplication, so you haven't bothered making your app idempotent. However, if your application is shut down just after saving the object to the database, thus failing to ack the message, it will (depending on the queue) be consumed again, resulting in duplicate database objects. This scenario is applicable to message queues such as RabbitMQ, Azure Service Bus and Amazon SQS.

This package prevents this using two mechanisms:

#### 1. Prevent interruption of ongoing work
When shutdown is requested, you'll want a grace period before hard termination, allowing your ongoing work to complete.

#### 2. Best-effort cancellation
A CancellationToken enables you opt-out of starting new work once termination has begun.

The termination sequence looks like this:

```
+-----------------------+          Is work      Yes        +---------------------------+
| Termination requested |----->  being done?  -----------> | Wait for work to complete |
+-----------------------+                                  +---------------------------+
                                      |                                  |                     
                                      | No                               v                   
                                      |                            +----------+      +------+
                                      +--------------------------> | Clean up |----->| Exit |
                                                                   +----------+      +------+                 
```

To get started, simply paste this in your startup routine:

```csharp
GracefulConsoleRunner.Run(runContext => {
    // Your code here
});
```

The `runContext` parameter offers two properties:
1. A cancellation token to check before starting
2. A method called `BlockInterruption` that returns an `IDisposable`. Use this to wrap any code you want to protect from interruption.

## Example

```csharp
GracefulConsoleRunner.Run(
    run: context =>
    {
        myTimer.Elapsed += (sender, e) => {

            if (context.ApplicationTermination.IsCancellationRequested)
            {
                // Graceful termination has been requested.
                return;
            }

            // Once we've decided to proceed, we don't want to be interrupted until processing is complete
            using (context.BlockInterruption())
            {
                // Your code here
            }

        };
    },
    cleanup: () =>
    {
        // Any clean-up code you want to ensure being run before exit
    },
    gracePeriod: TimeSpan.FromSeconds(10));
```

Make sure to check if termination is requested before calling `BlockInterruption()`. Only work that has been started at the time of shutdown is allowed to execute until completion.
