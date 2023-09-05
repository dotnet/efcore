// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     NetTopologySuite specific extension methods for <see cref="SqlServerDbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerNetTopologySuiteDbContextOptionsBuilderExtensions
{
    /// <summary>
    ///     Use NetTopologySuite to access SQL Server spatial data.
    /// </summary>
    /// <param name="optionsBuilder">The build being used to configure SQL Server.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static SqlServerDbContextOptionsBuilder UseNetTopologySuite(
        this SqlServerDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<SqlServerNetTopologySuiteOptionsExtension>()
            ?? new SqlServerNetTopologySuiteOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
