// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     EntityFrameworkCore.SqlServer.HierarchyId extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class SqlServerHierarchyIdServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services required for HierarchyId support in the SQL Server provider for Entity Framework.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEntityFrameworkSqlServerHierarchyId(
        this IServiceCollection serviceCollection)
    {
        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IMethodCallTranslatorPlugin, SqlServerHierarchyIdMethodCallTranslatorPlugin>()
            .TryAdd<IRelationalTypeMappingSourcePlugin, SqlServerHierarchyIdTypeMappingSourcePlugin>();

        return serviceCollection;
    }
}
