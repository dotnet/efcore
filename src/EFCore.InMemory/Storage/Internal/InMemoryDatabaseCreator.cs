// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryDatabaseCreator : IDatabaseCreator
{
    private readonly IDatabase _database;
    private readonly ICurrentDbContext _currentContext;
    private readonly IDbContextOptions _contextOptions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryDatabaseCreator(
        IDatabase database,
        ICurrentDbContext currentContext,
        IDbContextOptions contextOptions)
    {
        _database = database;
        _currentContext = currentContext;
        _contextOptions = contextOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IInMemoryDatabase Database
        => (IInMemoryDatabase)_database;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureDeleted()
        => Database.Store.Clear();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(EnsureDeleted());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureCreated()
    {
        var created = Database.EnsureDatabaseCreated();

        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        var seed = coreOptionsExtension.Seeder;
        if (seed != null)
        {
            seed(_currentContext.Context, created);
        }
        else if (coreOptionsExtension.AsyncSeeder != null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }

        return created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var created = Database.EnsureDatabaseCreated();

        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        var seedAsync = coreOptionsExtension.AsyncSeeder;
        if (seedAsync != null)
        {
            await seedAsync(_currentContext.Context, created, cancellationToken).ConfigureAwait(false);
        }
        else if (coreOptionsExtension.Seeder != null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }

        return created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanConnect()
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
