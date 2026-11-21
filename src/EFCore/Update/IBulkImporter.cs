// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Service for performing efficient bulk import of entity instances directly into the database.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
/// </summary>
public interface IBulkImporter
{
    /// <summary>
    ///     Imports the specified entities into the database table for the given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entityType">The metadata entity type to import into.</param>
    /// <param name="entities">The entity instances to import.</param>
    /// <param name="options">Options to configure the bulk import.</param>
    void Import<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options = null)
        where TEntity : class;

    /// <summary>
    ///     Asynchronously imports the specified entities into the database table for the given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entityType">The metadata entity type to import into.</param>
    /// <param name="entities">The entity instances to import.</param>
    /// <param name="options">Options to configure the bulk import.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous bulk import operation.</returns>
    Task ImportAsync<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    ///     Asynchronously imports the specified entities stream into the database table for the given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entityType">The metadata entity type to import into.</param>
    /// <param name="entities">The entity instances stream to import.</param>
    /// <param name="options">Options to configure the bulk import.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous bulk import operation.</returns>
    Task ImportAsync<TEntity>(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> entities,
        BulkImportOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEntity : class;
}
