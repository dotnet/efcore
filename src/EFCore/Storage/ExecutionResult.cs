// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Represents the execution state of an operation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
///     for more information and examples.
/// </remarks>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class ExecutionResult<TResult>
{
    /// <summary>
    ///     Creates a new instance of <see cref="ExecutionResult{TResult}" />.
    /// </summary>
    /// <param name="successful"><see langword="true" /> if the operation succeeded.</param>
    /// <param name="result">The result of the operation if successful.</param>
    public ExecutionResult(bool successful, TResult result)
    {
        IsSuccessful = successful;
        Result = result;
    }

    /// <summary>
    ///     Indicates whether the operation succeeded.
    /// </summary>
    public virtual bool IsSuccessful { get; }

    /// <summary>
    ///     The result of the operation if successful.
    /// </summary>
    public virtual TResult Result { get; }
}
