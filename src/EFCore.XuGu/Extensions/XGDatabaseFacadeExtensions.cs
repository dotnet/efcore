// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     MySQL specific extension methods for <see cref="DbContext.Database" />.
    /// </summary>
    public static class XGDatabaseFacadeExtensions
    {
        /// <summary>
        ///     <para>
        ///         Returns true if the database provider currently in use is the MySQL provider.
        ///     </para>
        ///     <para>
        ///         This method can only be used after the <see cref="DbContext" /> has been configured because
        ///         it is only then that the provider is known. This means that this method cannot be used
        ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
        ///         provider to use as part of configuring the context.
        ///     </para>
        /// </summary>
        /// <param name="database"> The facade from <see cref="DbContext.Database" />. </param>
        /// <returns> True if MySQL is being used; false otherwise. </returns>
        public static bool IsXG([NotNull] this DatabaseFacade database)
            => database.ProviderName.Equals(
                typeof(XGOptionsExtension).GetTypeInfo().Assembly.GetName().Name,
                StringComparison.Ordinal);

        /// <summary>
        ///     Uses a <see cref="DbDataSource" /> for this <see cref="DbContext" /> as the underlying database provider.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         It may not be possible to change the data source if an existing connection is open.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
        ///     </para>
        /// </remarks>
        /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
        /// <param name="dataSource">The data source.</param>
        public static void SetDbDataSource(this DatabaseFacade databaseFacade, DbDataSource dataSource)
            => ((XGRelationalConnection)GetFacadeDependencies(databaseFacade).RelationalConnection).DbDataSource = dataSource;

        private static IRelationalDatabaseFacadeDependencies GetFacadeDependencies(DatabaseFacade databaseFacade)
        {
            var dependencies = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies;

            if (dependencies is IRelationalDatabaseFacadeDependencies relationalDependencies)
            {
                return relationalDependencies;
            }

            throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
        }
        public static string GetTimeZone(this IReadOnlyModel model)
            => (model is RuntimeModel)
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : (string)model["TimeZone"];
    }
}
