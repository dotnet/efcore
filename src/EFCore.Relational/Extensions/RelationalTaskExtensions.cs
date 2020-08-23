// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace

namespace System.Threading.Tasks
{
    internal static class RelationalTaskExtensions
    {
        public static Task<TDerived> Cast<T, TDerived>(this Task<T> task)
            where TDerived : T
        {
            var taskCompletionSource = new TaskCompletionSource<TDerived>();

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        taskCompletionSource.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        taskCompletionSource.TrySetResult((TDerived)t.Result);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);

            return taskCompletionSource.Task;
        }

        public static Task<T?> CastToNullable<T>(this Task<T> task)
            where T : struct
        {
            var taskCompletionSource = new TaskCompletionSource<T?>();

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        taskCompletionSource.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        taskCompletionSource.TrySetResult(t.Result);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);

            return taskCompletionSource.Task;
        }
    }
}
