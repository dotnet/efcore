// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         A service for preparing a list of <see cref="ModificationCommandBatch" />s for the entities
///         represented by the given list of <see cref="IUpdateEntry" />s.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface ICommandBatchPreparer : IResettableService
{
    /// <summary>
    ///     Creates the command batches needed to insert/update/delete the entities represented by the given
    ///     list of <see cref="IUpdateEntry" />s.
    /// </summary>
    /// <param name="entries">The entries that represent the entities to be modified.</param>
    /// <param name="updateAdapter">The model data.</param>
    /// <returns>The list of batches to execute.</returns>
    IEnumerable<ModificationCommandBatch> BatchCommands(IList<IUpdateEntry> entries, IUpdateAdapter updateAdapter);

    /// <summary>
    ///     Given a set of modification commands, returns one more ready-to-execute batches for those commands, taking into account e.g.
    ///     maximum batch sizes and other batching constraints.
    /// </summary>
    /// <param name="commandSet">The set of commands to be organized in batches.</param>
    /// <param name="moreCommandSets">Whether more command sets are expected after this one within the same save operation.</param>
    /// <returns>The list of batches to execute.</returns>
    IEnumerable<ModificationCommandBatch> CreateCommandBatches(IEnumerable<IReadOnlyModificationCommand> commandSet, bool moreCommandSets);
}
