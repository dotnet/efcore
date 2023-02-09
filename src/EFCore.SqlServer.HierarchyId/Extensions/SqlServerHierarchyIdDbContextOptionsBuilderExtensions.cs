// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     HierarchyId specific extension methods for <see cref="SqlServerDbContextOptionsBuilder" />.
/// </summary>
public static class SqlServerHierarchyIdDbContextOptionsBuilderExtensions
{
    /// <summary>
    ///     Enable HierarchyId mappings.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure SQL Server.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static SqlServerDbContextOptionsBuilder UseHierarchyId(
        this SqlServerDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<SqlServerHierarchyIdOptionsExtension>()
            ?? new SqlServerHierarchyIdOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
