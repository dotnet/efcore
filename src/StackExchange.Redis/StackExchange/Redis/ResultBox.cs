using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    abstract partial class ResultBox
    {
        protected Exception exception;

        public void SetException(Exception exception)
        {
            this.exception = exception;
            //try
            //{
            //    throw exception;
            //}
            //catch (Exception caught)
            //{ // stacktrace etc
            //    this.exception = caught;
            //}
        }
        public abstract bool TryComplete(bool isAsync);

        [Conditional("DEBUG")]
        protected static void IncrementAllocationCount()
        {
            OnAllocated();
        }

        static partial void OnAllocated();
    }
    sealed class ResultBox<T> : ResultBox
    {
        private static readonly ResultBox<T>[] store = new ResultBox<T>[64];

        private object stateOrCompletionSource;

        private T value;

        public ResultBox(object stateOrCompletionSource)
        {
            this.stateOrCompletionSource = stateOrCompletionSource;
        }
        public object AsyncState
        {
            get
            {
                return stateOrCompletionSource is TaskCompletionSource<T>
                    ? ((TaskCompletionSource<T>)stateOrCompletionSource).Task.AsyncState
                    : stateOrCompletionSource;
            }
        }

        public static ResultBox<T> Get(object stateOrCompletionSource)
        {
            ResultBox<T> found;
            for (int i = 0; i < store.Length; i++)
            {
                if ((found = Interlocked.Exchange(ref store[i], null)) != null)
                {
                    found.Reset(stateOrCompletionSource);
                    return found;
                }
            }
            IncrementAllocationCount();
            
            return new ResultBox<T>(stateOrCompletionSource);
        }

        public static void UnwrapAndRecycle(ResultBox<T> box, out T value, out Exception exception)
        {
            if (box == null)
            {
                value = default(T);
                exception = null;
            }
            else
            {
                value = box.value;
                exception = box.exception;
                box.value = default(T);
                box.exception = null;
                for (int i = 0; i < store.Length; i++)
                {
                    if (Interlocked.CompareExchange(ref store[i], box, null) == null) return;
                }
            }
        }

        public void SetResult(T value)
        {
            this.value = value;
        }

        public override bool TryComplete(bool isAsync)
        {
            if (stateOrCompletionSource is TaskCompletionSource<T>)
            {
                var tcs = (TaskCompletionSource<T>)stateOrCompletionSource;
                if (isAsync || TaskSource.IsSyncSafe(tcs.Task))
                {
                    T val;
                    Exception ex;
                    UnwrapAndRecycle(this, out val, out ex);

                    if (ex == null) tcs.TrySetResult(val);
                    else
                    {
                        if (ex is TaskCanceledException) tcs.TrySetCanceled();
                        else tcs.TrySetException(ex);
                        // mark it as observed
                        GC.KeepAlive(tcs.Task.Exception);
                        GC.SuppressFinalize(tcs.Task);
                    }
                    return true;
                }
                else
                { // looks like continuations; push to async to preserve the reader thread
                    return false;
                }
            }
            else
            {
                lock (this)
                { // tell the waiting thread that we're done
                    Monitor.PulseAll(this);
                }
                ConnectionMultiplexer.TraceWithoutContext("Pulsed", "Result");
                return true;
            }
        }

        private void Reset(object stateOrCompletionSource)
        {
            value = default(T);
            exception = null;
            this.stateOrCompletionSource = stateOrCompletionSource;
        }
    }

}
