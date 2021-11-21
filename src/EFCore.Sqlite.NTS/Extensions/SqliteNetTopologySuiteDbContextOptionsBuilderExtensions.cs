// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     NetTopologySuite specific extension methods for <see cref="SqliteDbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteNetTopologySuiteDbContextOptionsBuilderExtensions
{
    /// <summary>
    ///     Use NetTopologySuite to access SpatiaLite data.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The build being used to configure SQLite.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static SqliteDbContextOptionsBuilder UseNetTopologySuite(
        this SqliteDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;
        var infrastructure = (IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder;
#pragma warning disable EF1001 // Internal EF Core API usage.
        // #20566
        var sqliteExtension = coreOptionsBuilder.Options.FindExtension<SqliteOptionsExtension>()
            ?? new SqliteOptionsExtension();
        var ntsExtension = coreOptionsBuilder.Options.FindExtension<SqliteNetTopologySuiteOptionsExtension>()
            ?? new SqliteNetTopologySuiteOptionsExtension();

        infrastructure.AddOrUpdateExtension(sqliteExtension.WithLoadSpatialite(true));
#pragma warning restore EF1001 // Internal EF Core API usage.
        infrastructure.AddOrUpdateExtension(ntsExtension);

        return optionsBuilder;
    }
}
