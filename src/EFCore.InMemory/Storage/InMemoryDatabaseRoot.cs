// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Acts as a root for all in-memory databases such that they will be available
///     across context instances and service providers as long as the same instance
///     of this type is passed to
///     <see
///         cref="InMemoryDbContextOptionsExtensions.UseInMemoryDatabase{TContext}(DbContextOptionsBuilder{TContext},string,System.Action{InMemoryDbContextOptionsBuilder})" />
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
/// </remarks>
public sealed class InMemoryDatabaseRoot
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Entity Framework code will set this instance as needed. It should be considered opaque to
    ///     application code; the type of object may change at any time.
    /// </remarks>
    [EntityFrameworkInternal]
    public object? Instance;
}
