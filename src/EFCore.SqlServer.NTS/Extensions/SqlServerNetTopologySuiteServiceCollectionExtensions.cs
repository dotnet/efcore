// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetTopologySuite;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     EntityFrameworkCore.SqlServer.NetTopologySuite extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class SqlServerNetTopologySuiteServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required for NetTopologySuite support in the SQL Server provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddEntityFrameworkSqlServerNetTopologySuite(
            this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddSingleton(NtsGeometryServices.Instance);

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IRelationalTypeMappingSourcePlugin, SqlServerNetTopologySuiteTypeMappingSourcePlugin>()
                .TryAdd<IMethodCallTranslatorPlugin, SqlServerNetTopologySuiteMethodCallTranslatorPlugin>()
                .TryAdd<IMemberTranslatorPlugin, SqlServerNetTopologySuiteMemberTranslatorPlugin>();

            return serviceCollection;
        }
    }
}
