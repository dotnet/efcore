// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
            [NotNull] this SqlServerDbContextOptionsBuilder optionsBuilder)
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
