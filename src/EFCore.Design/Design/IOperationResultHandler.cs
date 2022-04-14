// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Used with <see cref="OperationExecutor" /> to handle operation results.
/// </summary>
public interface IOperationResultHandler
{
    /// <summary>
    ///     Gets the contract version of this handler.
    /// </summary>
    /// <value> The contract version of this handler. </value>
    int Version { get; }

    /// <summary>
    ///     Invoked when a result is available.
    /// </summary>
    /// <param name="value">The result.</param>
    void OnResult(object? value);

    /// <summary>
    ///     Invoked when an error occurs.
    /// </summary>
    /// <param name="type">The exception type.</param>
    /// <param name="message">The error message.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <remarks>
    ///     When an <see cref="OperationException" /> is received, the stack trace should not be shown by default.
    /// </remarks>
    void OnError(string type, string message, string stackTrace);
}
