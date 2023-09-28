// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Allows interception of commands sent to a relational database.
/// </summary>
/// <remarks>
///     <para>
///         Command interceptors can be used to view, change, or suppress execution of the <see cref="DbCommand" />, and
///         to modify the result before it is returned to EF.
///     </para>
///     <para>
///         Consider inheriting from <see cref="DbCommandInterceptor" /> if not implementing all methods.
///     </para>
///     <para>
///         Use <see cref="DbContextOptionsBuilder.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[])" />
///         to register application interceptors.
///     </para>
///     <para>
///         Extensions can also register interceptors in the internal service provider.
///         If both injected and application interceptors are found, then the injected interceptors are run in the
///         order that they are resolved from the service provider, and then the application interceptors are run last.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDbCommandInterceptor : IInterceptor
{
    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.CreateCommand" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{DbCommand}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbCommand}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{DbCommand}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{DbCommand}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{DbCommand}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbConnection.CreateCommand" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed creation of a command in <see cref="CommandCreating" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="CommandCreating" />.
    /// </remarks>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbConnection.CreateCommand" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
        => result;

    /// <summary>
    ///     Called after EF has initialized <see cref="DbCommand.CommandText" /> and other command configuration.
    /// </summary>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The command. This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbCommand.ExecuteReader()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{DbDataReader}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbDataReader}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{DbDataReader}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{DbDataReader}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{DbDataReader}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbCommand.ExecuteScalar()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{Object}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Object}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{Object}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{Object}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{Object}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbCommand.ExecuteNonQuery()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{Int32}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Int32}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{Int32}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbCommand.ExecuteReaderAsync()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{DbDataReader}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbDataReader}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult{DbDataReader}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{DbDataReader}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{DbDataReader}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbCommand.ExecuteScalarAsync()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{Object}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Object}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult{Object}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{Object}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{Object}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbCommand.ExecuteNonQueryAsync()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{Int32}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Int32}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{Int32}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbCommand.ExecuteReader()" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed execution of a command in <see cref="ReaderExecuting" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="ReaderExecuting" />.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbCommand.ExecuteReader()" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbCommand.ExecuteScalar()" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed execution of a command in <see cref="ScalarExecuting" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="ScalarExecuting" />.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbCommand.ExecuteScalar()" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbCommand.ExecuteNonQuery()" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed execution of a command in <see cref="NonQueryExecuting" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="NonQueryExecuting" />.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbCommand.ExecuteNonQuery()" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbCommand.ExecuteReaderAsync()" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed execution of a command in <see cref="ReaderExecutingAsync" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="ReaderExecutingAsync" />.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbCommand.ExecuteReaderAsync()" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task" /> providing the result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbCommand.ExecuteScalarAsync()" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed execution of a command in <see cref="ScalarExecutingAsync" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="ScalarExecutingAsync" />.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbCommand.ExecuteScalarAsync()" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task" /> providing the result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbCommand.ExecuteNonQueryAsync()" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed execution of a command in <see cref="NonQueryExecutingAsync" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="NonQueryExecutingAsync" />.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbCommand.ExecuteNonQueryAsync()" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task" /> providing the result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called when a command was canceled.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    void CommandCanceled(DbCommand command, CommandEndEventData eventData)
    {
    }

    /// <summary>
    ///     Called when a command was canceled.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CommandCanceledAsync(DbCommand command, CommandEndEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called when execution of a command has failed with an exception.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
    }

    /// <summary>
    ///     Called when execution of a command has failed with an exception.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbDataReader.Close()" />.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult DataReaderClosing(DbCommand command, DataReaderClosingEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbDataReader.CloseAsync()" /> in an async context.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    ValueTask<InterceptionResult> DataReaderClosingAsync(DbCommand command, DataReaderClosingEventData eventData, InterceptionResult result)
        => new(result);

    /// <summary>
    ///     Called when execution of a <see cref="DbDataReader" /> is about to be disposed.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">Contextual information about the command and reader.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult DataReaderDisposing(DbCommand command, DataReaderDisposingEventData eventData, InterceptionResult result)
        => result;
}
