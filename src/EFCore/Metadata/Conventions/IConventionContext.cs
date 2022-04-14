// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Contextual information associated with each convention call.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionContext
{
    /// <summary>
    ///     Calling this will prevent further processing of the associated event by other conventions.
    /// </summary>
    /// <remarks>
    ///     The common use case is when the metadata object was removed by the convention.
    /// </remarks>
    void StopProcessing();

    /// <summary>
    ///     Prevents conventions from being executed immediately when a metadata aspect is modified. All the delayed conventions
    ///     will be executed after the returned object is disposed.
    /// </summary>
    /// <remarks>
    ///     This is useful when performing multiple operations that depend on each other.
    /// </remarks>
    /// <returns>An object that should be disposed to execute the delayed conventions.</returns>
    IConventionBatch DelayConventions();
}
