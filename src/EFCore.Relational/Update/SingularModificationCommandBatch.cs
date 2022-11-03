// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         An implementation of <see cref="AffectedCountModificationCommandBatch" /> that does not
///         support batching by limiting the number of commands in the batch to one.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class SingularModificationCommandBatch : AffectedCountModificationCommandBatch
{
    /// <summary>
    ///     Creates a new <see cref="SingularModificationCommandBatch" /> instance.
    /// </summary>
    /// <param name="dependencies">Service dependencies.</param>
    public SingularModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies)
        : base(dependencies, maxBatchSize: 1)
    {
    }
}
