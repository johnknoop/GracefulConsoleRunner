Add (truly) graceful termination to your .NET console app!

## Install
    PM> Install-Package JohnKnoop.GracefulConsoleRunner

This is a .NET Standard 1.5 library.

## What is it?

When your application logic is invoked by external events, such as a message bus or a scheduler, you'll want to control termination in two levels:

### 1. Best-effort cancellation
Passing a CancellationToken down the call hierarchy enables you opt-out of starting new work.

### 2. A hold-off mechanism to prevent interruption of important work
When shutdown is requested, you'll want a grace period between soft and hard termination, allowing your ongoing work to complete. The termination sequence looks like this:

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

Paste this in your startup routine:

```csharp
GracefulConsoleRunner.Run(runContext => {
    // Your code here
});
```

The `runContext` parameter offers two properties:
1. A cancellation token for best-effort cancellation
2. A method called `BlockInterruption` that returns an `IDisposable`. Use this to wrap any code you want to protect from interruption.


## Example

```csharp
GracefulConsoleRunner.Run(
    run: runContext =>
    {
        myTimer.Elapsed += (sender, e) => {

            if (context.ApplicationTermination.IsCancellationRequested)
            {
                // Graceful termination has been requested.
                return;
            }

            // Once we've decided to proceed, we don't want to be interrupted until processing is complete
            using (runContext.BlockInterruption())
            {
                // Your code here
            }

        };
    },
    cleanup: () =>
    {
        /* 
        * Any clean-up code you want to ensure being run before exit
        */
    },
    gracePeriodSeconds: 30);
```

In a message queue scenario, the pattern would be the same, where you first check for cancellation, and then wrap the message processing in a `BlockInterruption()` to make sure the message is processed and acknowledged without interruption.
