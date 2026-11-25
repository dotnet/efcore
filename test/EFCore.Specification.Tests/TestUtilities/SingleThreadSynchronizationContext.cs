// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _tasks = [];

    public Thread Thread { get; }

    public SingleThreadSynchronizationContext()
    {
        Thread = new Thread(WorkLoop) { IsBackground = true };
        Thread.Start();
    }

    public override void Post(SendOrPostCallback d, object? state)
        => _tasks.Add((d, state));

    public void Dispose()
        => _tasks.CompleteAdding();

    private void WorkLoop()
    {
        SetSynchronizationContext(this);

        try
        {
            while (true)
            {
                var (callback, state) = _tasks.Take();
                callback(state);
            }
        }
        catch (InvalidOperationException)
        {
            _tasks.Dispose();
        }
    }
}
