// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Allows interception of the <see cref="O:DbContext.SaveChanges" /> and <see cref="O:DbContext.SaveChangesAync" /> methods.
/// </summary>
/// <remarks>
///     <para>
///         SaveChanges interceptors can be used to view, change, or suppress execution of the SaveChanges call and
///         modify the result before it is returned to EF.
///     </para>
///     <para>
///         Consider inheriting from <see cref="SaveChangesInterceptor" /> if not implementing all methods.
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
public interface ISaveChangesInterceptor : IInterceptor
{
    /// <summary>
    ///     Called at the start of <see cref="O:DbContext.SaveChanges" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the <see cref="DbContext" /> being used.</param>
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
    InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        => result;

    /// <summary>
    ///     Called at the end of <see cref="O:DbContext.SaveChanges" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed creation of a command in <see cref="SavingChanges" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="SavingChanges" />.
    /// </remarks>
    /// <param name="eventData">Contextual information about the <see cref="DbContext" /> being used.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="O:DbContext.SaveChanges" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        => result;

    /// <summary>
    ///     Called when an exception has been thrown in <see cref="O:DbContext.SaveChanges" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the failure.</param>
    void SaveChangesFailed(DbContextErrorEventData eventData)
    {
    }

    /// <summary>
    ///     Called at the start of <see cref="O:DbContext.SaveChangesAsync" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the <see cref="DbContext" /> being used.</param>
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
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called at the end of <see cref="O:DbContext.SaveChangesAsync" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed creation of a command in <see cref="SavingChangesAsync" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="SavingChangesAsync" />.
    /// </remarks>
    /// <param name="eventData">Contextual information about the <see cref="DbContext" /> being used.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="O:DbContext.SaveChangesAsync" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called when an exception has been thrown in <see cref="O:DbContext.SaveChangesAsync" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the failure.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called when <see cref="O:DbContext.SaveChanges" /> was canceled.
    /// </summary>
    /// <param name="eventData">Contextual information about the failure.</param>
    void SaveChangesCanceled(DbContextEventData eventData)
    {
    }

    /// <summary>
    ///     Called when <see cref="O:DbContext.SaveChangesAsync" /> was canceled.
    /// </summary>
    /// <param name="eventData">Contextual information about the failure.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called immediately before EF is going to throw a <see cref="DbUpdateConcurrencyException" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the concurrency conflict.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will throw the exception.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will not throw the exception.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     setting property values must return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately before EF is going to throw a <see cref="DbUpdateConcurrencyException" />.
    /// </summary>
    /// <param name="eventData">Contextual information about the concurrency conflict.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will throw the exception.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will not throw the exception.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     setting property values must return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
        ConcurrencyExceptionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);
}
