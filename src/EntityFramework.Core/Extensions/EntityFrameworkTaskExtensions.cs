// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45
using System.Threading;
#endif
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace System.Threading.Tasks
{
    /// <summary>
    ///     Contains extension methods for the <see cref="Task" /> class.
    /// </summary>
    public static class EntityFrameworkTaskExtensions
    {
        /// <summary>
        ///     Configures an awaiter used to await this <see cref="Task{TResult}" /> to avoid
        ///     marshalling the continuation back to the original context, but preserve the
        ///     current culture and UI culture.
        /// </summary>
        /// <remarks> Calling this has no effect on platforms that don't use <see cref="SynchronizationContext" />. </remarks>
        /// <typeparam name="T">
        ///     The type of the result produced by the associated <see cref="Task{TResult}" />.
        /// </typeparam>
        /// <param name="task">The task to be awaited on.</param>
        /// <returns>An object used to await this task.</returns>
        public static CultureAwaiter<T> WithCurrentCulture<T>([NotNull] this Task<T> task)
        {
            Check.NotNull(task, nameof(task));

            return new CultureAwaiter<T>(task);
        }

        /// <summary>
        ///     Configures an awaiter used to await this <see cref="Task" /> to avoid
        ///     marshalling the continuation back to the original context, but preserve the
        ///     current culture and UI culture.
        /// </summary>
        /// <remarks> Calling this has no effect on platforms that don't use <see cref="SynchronizationContext" />. </remarks>
        /// <param name="task">The task to be awaited on.</param>
        /// <returns>An object used to await this task.</returns>
        public static CultureAwaiter WithCurrentCulture([NotNull] this Task task)
        {
            Check.NotNull(task, nameof(task));

            return new CultureAwaiter(task);
        }

        /// <summary>
        ///     Provides an awaitable object that allows for awaits on <see cref="Task{TResult}" /> that
        ///     preserve the culture.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the result produced by the associated <see cref="Task{TResult}" />.
        /// </typeparam>
        /// <remarks>This type is intended for compiler use only.</remarks>
        public struct CultureAwaiter<T> : ICriticalNotifyCompletion
        {
            private readonly Task<T> _task;

            /// <summary>
            ///     Constructs a new instance of the <see cref="CultureAwaiter{T}" /> class.
            /// </summary>
            /// <param name="task">The task to be awaited on.</param>
            public CultureAwaiter([NotNull] Task<T> task)
            {
                _task = task;
            }

            /// <summary>Gets an awaiter used to await this <see cref="Task{TResult}" />.</summary>
            /// <returns>An awaiter instance.</returns>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public CultureAwaiter<T> GetAwaiter() => this;

            /// <summary>
            ///     Gets whether this <see cref="Task">Task</see> has completed.
            /// </summary>
            /// <remarks>
            ///     <see cref="IsCompleted" /> will return true when the Task is in one of the three
            ///     final states: <see cref="TaskStatus.RanToCompletion">RanToCompletion</see>,
            ///     <see cref="TaskStatus.Faulted">Faulted</see>, or
            ///     <see cref="TaskStatus.Canceled">Canceled</see>.
            /// </remarks>
            public bool IsCompleted => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task{TResult}" />.</summary>
            /// <returns>The result of the completed <see cref="Task{TResult}" />.</returns>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
            public T GetResult() => _task.GetAwaiter().GetResult();

            /// <summary>This method is not implemented and should not be called.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            ///     Schedules the continuation onto the <see cref="Task{TResult}" /> associated with this
            ///     <see cref="TaskAwaiter{TResult}" />.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">
            ///     The <paramref name="continuation" /> argument is null
            ///     (Nothing in Visual Basic).
            /// </exception>
            /// <exception cref="System.InvalidOperationException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public void UnsafeOnCompleted(Action continuation)
            {
#if NET45
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                var currentUICulture = Thread.CurrentThread.CurrentUICulture;
                _task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().UnsafeOnCompleted(
                    () =>
                    {
                        var originalCulture = Thread.CurrentThread.CurrentCulture;
                        var originalUICulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentCulture = currentCulture;
                        Thread.CurrentThread.CurrentUICulture = currentUICulture;
                        try
                        {
                            continuation();
                        }
                        finally
                        {
                            Thread.CurrentThread.CurrentCulture = originalCulture;
                            Thread.CurrentThread.CurrentUICulture = originalUICulture;
                        }
                    });
#else
                _task.GetAwaiter().UnsafeOnCompleted(continuation);
#endif
            }
        }

        /// <summary>
        ///     Provides an awaitable object that allows for awaits on <see cref="Task" /> that
        ///     preserve the culture.
        /// </summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        public struct CultureAwaiter : ICriticalNotifyCompletion
        {
            private readonly Task _task;

            /// <summary>
            ///     Constructs a new instance of the <see cref="CultureAwaiter" /> class.
            /// </summary>
            /// <param name="task">The task to be awaited on.</param>
            public CultureAwaiter([NotNull] Task task)
            {
                _task = task;
            }

            /// <summary>Gets an awaiter used to await this <see cref="Task" />.</summary>
            /// <returns>An awaiter instance.</returns>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public CultureAwaiter GetAwaiter() => this;

            /// <summary>
            ///     Gets whether this <see cref="Task">Task</see> has completed.
            /// </summary>
            /// <remarks>
            ///     <see cref="IsCompleted" /> will return true when the Task is in one of the three
            ///     final states: <see cref="TaskStatus.RanToCompletion">RanToCompletion</see>,
            ///     <see cref="TaskStatus.Faulted">Faulted</see>, or
            ///     <see cref="TaskStatus.Canceled">Canceled</see>.
            /// </remarks>
            public bool IsCompleted => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task" />.</summary>
            /// <exception cref="System.NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="System.Exception">The task completed in a Faulted state.</exception>
            public void GetResult() => _task.GetAwaiter().GetResult();

            /// <summary>This method is not implemented and should not be called.</summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            ///     Schedules the continuation onto the <see cref="Task" /> associated with this
            ///     <see cref="TaskAwaiter" />.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="System.ArgumentNullException">
            ///     The <paramref name="continuation" /> argument is null
            ///     (Nothing in Visual Basic).
            /// </exception>
            /// <exception cref="System.InvalidOperationException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public void UnsafeOnCompleted(Action continuation)
            {
#if NET45
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                var currentUICulture = Thread.CurrentThread.CurrentUICulture;
                _task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().UnsafeOnCompleted(
                    () =>
                    {
                        var originalCulture = Thread.CurrentThread.CurrentCulture;
                        var originalUICulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentCulture = currentCulture;
                        Thread.CurrentThread.CurrentUICulture = currentUICulture;
                        try
                        {
                            continuation();
                        }
                        finally
                        {
                            Thread.CurrentThread.CurrentCulture = originalCulture;
                            Thread.CurrentThread.CurrentUICulture = originalUICulture;
                        }
                    });
#else
                _task.GetAwaiter().UnsafeOnCompleted(continuation);
#endif
            }
        }
    }
}
