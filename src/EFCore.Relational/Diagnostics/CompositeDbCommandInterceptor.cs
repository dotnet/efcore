// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    public class CompositeDbCommandInterceptor : IDbCommandInterceptor
    {
        private readonly List<IDbCommandInterceptor> _interceptors;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="CompositeDbCommandInterceptor" /> class,
        ///         creating a new <see cref="IDbCommandInterceptor" /> composed from other <see cref="IDbCommandInterceptor" />
        ///         instances.
        ///     </para>
        ///     <para>
        ///         The result from each interceptor method is passed as the 'result' parameter to the same method
        ///         on the next interceptor in the chain.
        ///     </para>
        /// </summary>
        /// <param name="interceptors"> The interceptors from which to create composite chain. </param>
        public CompositeDbCommandInterceptor([NotNull] params IDbCommandInterceptor[] interceptors)
            : this((IReadOnlyList<IDbCommandInterceptor>)interceptors)
        {
        }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="CompositeDbCommandInterceptor" /> class,
        ///         creating a new <see cref="IDbCommandInterceptor" /> composed from other <see cref="IDbCommandInterceptor" />
        ///         instances.
        ///     </para>
        ///     <para>
        ///         The result from each interceptor method is passed as the 'result' parameter to the same method
        ///         on the next interceptor in the chain.
        ///     </para>
        /// </summary>
        /// <param name="interceptors"> The interceptors from which to create composite chain. </param>
        public CompositeDbCommandInterceptor([NotNull] IEnumerable<IDbCommandInterceptor> interceptors)
        {
            Check.NotNull(interceptors, nameof(interceptors));

            _interceptors = interceptors.ToList();
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ReaderExecuting" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual InterceptionResult<DbDataReader>? ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader>? result)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = _interceptors[i].ReaderExecuting(command, eventData, result);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ScalarExecuting" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual InterceptionResult<object>? ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object>? result)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = _interceptors[i].ScalarExecuting(command, eventData, result);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.NonQueryExecuting" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual InterceptionResult<int>? NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int>? result)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = _interceptors[i].NonQueryExecuting(command, eventData, result);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ReaderExecutingAsync" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual async Task<InterceptionResult<DbDataReader>?> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader>? result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = await _interceptors[i].ReaderExecutingAsync(command, eventData, result, cancellationToken);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ScalarExecutingAsync" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual async Task<InterceptionResult<object>?> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object>? result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = await _interceptors[i].ScalarExecutingAsync(command, eventData, result, cancellationToken);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.NonQueryExecutingAsync" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual async Task<InterceptionResult<int>?> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int>? result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = await _interceptors[i].NonQueryExecutingAsync(command, eventData, result, cancellationToken);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ReaderExecuted" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = _interceptors[i].ReaderExecuted(command, eventData, result);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ScalarExecuted" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual object ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = _interceptors[i].ScalarExecuted(command, eventData, result);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.NonQueryExecuted" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = _interceptors[i].NonQueryExecuted(command, eventData, result);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ReaderExecutedAsync" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual async Task<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = await _interceptors[i].ReaderExecutedAsync(command, eventData, result, cancellationToken);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.ScalarExecutedAsync" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual async Task<object> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = await _interceptors[i].ScalarExecutedAsync(command, eventData, result, cancellationToken);
            }

            return result;
        }

        /// <summary>
        ///     Calls <see cref="IDbCommandInterceptor.NonQueryExecutedAsync" /> for all interceptors in the chain, passing
        ///     the result from one as the <paramref name="result" /> parameter for the next.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result"> The current result, or null if no result yet exists. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result returned from the last interceptor in the chain. </returns>
        public virtual async Task<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Count; i++)
            {
                result = await _interceptors[i].NonQueryExecutedAsync(command, eventData, result, cancellationToken);
            }

            return result;
        }
    }
}
