// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<(SendOrPostCallback callback, object state)> _tasks = new();

    public Thread Thread { get; }

    public SingleThreadSynchronizationContext()
    {
        Thread = new Thread(WorkLoop);
        Thread.IsBackground = true;
        Thread.Start();
    }

    public override void Post(SendOrPostCallback callback, object state)
        => _tasks.Add((callback, state));

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
