using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace StackExchange.Redis
{
    sealed partial class CompletionManager
    {
        private static readonly WaitCallback processAsyncCompletionQueue = ProcessAsyncCompletionQueue,
            anyOrderCompletionHandler = AnyOrderCompletionHandler;

        private readonly Queue<ICompletable> asyncCompletionQueue = new Queue<ICompletable>();

        private readonly ConnectionMultiplexer multiplexer;

        private readonly string name;

        int activeAsyncWorkerThread = 0;
        long completedSync, completedAsync, failedAsync;
        public CompletionManager(ConnectionMultiplexer multiplexer, string name)
        {
            this.multiplexer = multiplexer;
            this.name = name;
        }
        public void CompleteSyncOrAsync(ICompletable operation)
        {
            if (operation == null) return;
            if (operation.TryComplete(false))
            {
                multiplexer.Trace("Completed synchronously: " + operation, name);
                Interlocked.Increment(ref completedSync);
                return;
            }
            else
            {
                if (multiplexer.PreserveAsyncOrder)
                {
                    multiplexer.Trace("Queueing for asynchronous completion", name);
                    bool startNewWorker;
                    lock (asyncCompletionQueue)
                    {
                        asyncCompletionQueue.Enqueue(operation);
                        startNewWorker = asyncCompletionQueue.Count == 1;
                    }
                    if (startNewWorker)
                    {
                        multiplexer.Trace("Starting new async completion worker", name);
                        OnCompletedAsync();
                        ThreadPool.QueueUserWorkItem(processAsyncCompletionQueue, this);
                    }
                } else
                {
                    multiplexer.Trace("Using thread-pool for asynchronous completion", name);
                    ThreadPool.QueueUserWorkItem(anyOrderCompletionHandler, operation);
                    Interlocked.Increment(ref completedAsync); // k, *technically* we haven't actually completed this yet, but: close enough
                }
            }
        }
        internal void GetCounters(ConnectionCounters counters)
        {
            lock (asyncCompletionQueue)
            {
                counters.ResponsesAwaitingAsyncCompletion = asyncCompletionQueue.Count;
            }
            counters.CompletedSynchronously = Interlocked.Read(ref completedSync);
            counters.CompletedAsynchronously = Interlocked.Read(ref completedAsync);
            counters.FailedAsynchronously = Interlocked.Read(ref failedAsync);
        }

        internal int GetOutstandingCount()
        {
            lock(asyncCompletionQueue)
            {
                return asyncCompletionQueue.Count;
            }
        }

        internal void GetStormLog(StringBuilder sb)
        {
            lock(asyncCompletionQueue)
            {
                if (asyncCompletionQueue.Count == 0) return;
                sb.Append("Response awaiting completion: ").Append(asyncCompletionQueue.Count).AppendLine();
                int total = 0;
                foreach(var item in asyncCompletionQueue)
                {
                    if (++total >= 500) break;
                    item.AppendStormLog(sb);
                    sb.AppendLine();
                }
            }
        }

        private static void AnyOrderCompletionHandler(object state)
        {
            try
            {
                ConnectionMultiplexer.TraceWithoutContext("Completing async (any order): " + state);
                ((ICompletable)state).TryComplete(true);
            }
            catch (Exception ex)
            {
                ConnectionMultiplexer.TraceWithoutContext("Async completion error: " + ex.Message);
            }
        }

        private static void ProcessAsyncCompletionQueue(object state)
        {
            ((CompletionManager)state).ProcessAsyncCompletionQueueImpl();
        }

        partial void OnCompletedAsync();
        private void ProcessAsyncCompletionQueueImpl()
        {
#if NET40
            int currentThread = Thread.CurrentThread.ManagedThreadId;
#else
            int currentThread = Environment.CurrentManagedThreadId;
#endif

            try
            {
                while (Interlocked.CompareExchange(ref activeAsyncWorkerThread, currentThread, 0) != 0)
                {
                    // if we don't win the lock, check whether there is still work; if there is we
                    // need to retry to prevent a nasty race condition
                    lock(asyncCompletionQueue)
                    {
                        if (asyncCompletionQueue.Count == 0) return; // another thread drained it; can exit
                    }
                    Thread.Sleep(1);
                }
                int total = 0;
                do
                {
                    ICompletable next;
                    lock (asyncCompletionQueue)
                    {
                        next = asyncCompletionQueue.Count == 0 ? null
                            : asyncCompletionQueue.Dequeue();
                    }
                    if (next == null)
                    {
                        // give it a moment and try again, noting that we might lose the battle
                        // when we pause
                        Interlocked.CompareExchange(ref activeAsyncWorkerThread, 0, currentThread);
                        if (Thread.Yield() && Interlocked.CompareExchange(ref activeAsyncWorkerThread, currentThread, 0) == 0)
                        {
                            // we paused, and we got the lock back; anything else?
                            lock (asyncCompletionQueue)
                            {
                                next = asyncCompletionQueue.Count == 0 ? null
                                    : asyncCompletionQueue.Dequeue();
                            }
                        }
                    }
                    if (next == null) break; // nothing to do <===== exit point
                    try
                    {
                        multiplexer.Trace("Completing async (ordered): " + next, name);
                        next.TryComplete(true);
                        Interlocked.Increment(ref completedAsync);
                    }
                    catch (Exception ex)
                    {
                        multiplexer.Trace("Async completion error: " + ex.Message, name);
                        Interlocked.Increment(ref failedAsync);
                    }
                    total++;
                } while (true);
                multiplexer.Trace("Async completion worker processed " + total + " operations", name);
            }
            finally
            {
                Interlocked.CompareExchange(ref activeAsyncWorkerThread, 0, currentThread);
            }
        }
    }
}
