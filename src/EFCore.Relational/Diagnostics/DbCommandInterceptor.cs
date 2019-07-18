// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Abstract base class for <see cref="IDbCommandInterceptor" /> for use when implementing a subset
    ///         of the interface methods.
    ///     </para>
    /// </summary>
    public abstract class DbCommandInterceptor : IDbCommandInterceptor
    {
        /// <summary>
        ///     Called just before EF intends to call <see cref="DbCommand.ExecuteReader()" />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{DbDataReader}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbDataReader}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{DbDataReader}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{DbDataReader}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{DbDataReader}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            return result;
        }

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbCommand.ExecuteScalar()" />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{Object}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Object}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{Object}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{Object}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{Object}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result) => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbCommand.ExecuteNonQuery()" />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{Int32}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Int32}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{Int32}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{Int32}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{Int32}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
            => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbCommand.ExecuteReaderAsync()" />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{DbDataReader}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbDataReader}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{DbDataReader}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{DbDataReader}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{DbDataReader}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbCommand.ExecuteScalarAsync()" />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{Object}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Object}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{Object}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{Object}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{Object}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbCommand.ExecuteNonQueryAsync()" />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{Int32}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Int32}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{Int32}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{Int32}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{Int32}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbCommand.ExecuteReader()" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed execution of a command in <see cref="ReaderExecuting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="ReaderExecuting" />.
        ///     </para>
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbCommand.ExecuteReader()" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
            => result;

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbCommand.ExecuteScalar()" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed execution of a command in <see cref="ScalarExecuting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="ScalarExecuting" />.
        ///     </para>
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbCommand.ExecuteScalar()" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual object ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result)
            => result;

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbCommand.ExecuteNonQuery()" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed execution of a command in <see cref="NonQueryExecuting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="NonQueryExecuting" />.
        ///     </para>
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbCommand.ExecuteNonQuery()" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
            => result;

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbCommand.ExecuteReaderAsync()" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed execution of a command in <see cref="ReaderExecutingAsync" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="ReaderExecutingAsync" />.
        ///     </para>
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbCommand.ExecuteReaderAsync()" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A <see cref="Task" /> providing the result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbCommand.ExecuteScalarAsync()" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed execution of a command in <see cref="ScalarExecutingAsync" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="ScalarExecutingAsync" />.
        ///     </para>
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbCommand.ExecuteScalarAsync()" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A <see cref="Task" /> providing the result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<object> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbCommand.ExecuteNonQueryAsync()" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed execution of a command in <see cref="NonQueryExecutingAsync" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="NonQueryExecutingAsync" />.
        ///     </para>
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbCommand.ExecuteNonQueryAsync()" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A <see cref="Task" /> providing the result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called when execution of a command has failed with an exception. />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        public virtual void CommandFailed(
            DbCommand command,
            CommandErrorEventData eventData)
        {
        }

        /// <summary>
        ///     Called when execution of a command has failed with an exception. />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and execution. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        public virtual Task CommandFailedAsync(
            DbCommand command,
            CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Called when execution of a <see cref="DbDataReader"/> is about to be disposed. />.
        /// </summary>
        /// <param name="command"> The command. </param>
        /// <param name="eventData"> Contextual information about the command and reader. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult DataReaderDisposing(
            DbCommand command,
            DataReaderDisposingEventData eventData,
            InterceptionResult result)
            => result;
    }
}
