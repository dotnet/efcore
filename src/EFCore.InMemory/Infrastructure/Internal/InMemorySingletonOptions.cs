// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemorySingletonOptions : IInMemorySingletonOptions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Initialize(IDbContextOptions options)
    {
        var inMemoryOptions = options.FindExtension<InMemoryOptionsExtension>();

        if (inMemoryOptions != null)
        {
            DatabaseRoot = inMemoryOptions.DatabaseRoot;
            IsNullabilityCheckEnabled = inMemoryOptions.IsNullabilityCheckEnabled;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var inMemoryOptions = options.FindExtension<InMemoryOptionsExtension>();

        if (inMemoryOptions != null
            && DatabaseRoot != inMemoryOptions.DatabaseRoot)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(InMemoryDbContextOptionsExtensions.UseInMemoryDatabase),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (inMemoryOptions != null
            && IsNullabilityCheckEnabled != inMemoryOptions.IsNullabilityCheckEnabled)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(InMemoryDbContextOptionsBuilder.EnableNullChecks),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryDatabaseRoot? DatabaseRoot { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsNullabilityCheckEnabled { get; private set; }
}
