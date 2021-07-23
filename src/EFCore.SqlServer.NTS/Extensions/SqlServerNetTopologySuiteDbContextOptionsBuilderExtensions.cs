// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     NetTopologySuite specific extension methods for <see cref="SqlServerDbContextOptionsBuilder" />.
    /// </summary>
    public static class SqlServerNetTopologySuiteDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Use NetTopologySuite to access SQL Server spatial data.
        /// </summary>
        /// <param name="optionsBuilder"> The build being used to configure SQL Server. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static SqlServerDbContextOptionsBuilder UseNetTopologySuite(
            this SqlServerDbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = coreOptionsBuilder.Options.FindExtension<SqlServerNetTopologySuiteOptionsExtension>()
                ?? new SqlServerNetTopologySuiteOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
