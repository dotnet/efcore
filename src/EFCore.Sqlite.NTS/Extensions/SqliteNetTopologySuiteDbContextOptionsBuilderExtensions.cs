// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     NetTopologySuite specific extension methods for <see cref="SqliteDbContextOptionsBuilder" />.
    /// </summary>
    public static class SqliteNetTopologySuiteDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Use NetTopologySuite to access SpatiaLite data.
        /// </summary>
        /// <param name="optionsBuilder"> The build being used to configure SQLite. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static SqliteDbContextOptionsBuilder UseNetTopologySuite(
            [NotNull] this SqliteDbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;
            var infrastructure = (IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder;
            var sqliteExtension = coreOptionsBuilder.Options.FindExtension<SqliteOptionsExtension>()
                ?? new SqliteOptionsExtension();
            var ntsExtension = coreOptionsBuilder.Options.FindExtension<SqliteNetTopologySuiteOptionsExtension>()
                ?? new SqliteNetTopologySuiteOptionsExtension();

            infrastructure.AddOrUpdateExtension(sqliteExtension.WithLoadSpatialite(true));
            infrastructure.AddOrUpdateExtension(ntsExtension);

            return optionsBuilder;
        }
    }
}
