// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="DbContext.Database" />.
/// </summary>
public static class SqlServerDatabaseFacadeExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is the SQL Server provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method can only be used after the <see cref="DbContext" /> has been configured because
    ///         it is only then that the provider is known. This means that this method cannot be used
    ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    ///         provider to use as part of configuring the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns><see langword="true" /> if SQL Server is being used; <see langword="false" /> otherwise.</returns>
    public static bool IsSqlServer(this DatabaseFacade database)
        => database.ProviderName == typeof(SqlServerOptionsExtension).Assembly.GetName().Name;
}
