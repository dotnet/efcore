// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class CosmosSqlDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseCosmosSql<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] Uri serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<CosmosSqlDbContextOptionsBuilder> cosmosSqlOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCosmosSql(
                (DbContextOptionsBuilder)optionsBuilder,
                serviceEndPoint,
                authKeyOrResourceToken,
                databaseName,
                cosmosSqlOptionsAction);

        public static DbContextOptionsBuilder UseCosmosSql(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] Uri serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<CosmosSqlDbContextOptionsBuilder> cosmosSqlOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(serviceEndPoint, nameof(serviceEndPoint));
            Check.NotEmpty(authKeyOrResourceToken, nameof(authKeyOrResourceToken));
            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<CosmosSqlDbOptionsExtension>()
                            ?? new CosmosSqlDbOptionsExtension();

            extension = extension
                .WithServiceEndPoint(serviceEndPoint)
                .WithAuthKeyOrResourceToken(authKeyOrResourceToken)
                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            cosmosSqlOptionsAction?.Invoke(new CosmosSqlDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseCosmosSql<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<CosmosSqlDbContextOptionsBuilder> documentDbOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCosmosSql(
                (DbContextOptionsBuilder)optionsBuilder,
                serviceEndPoint,
                authKeyOrResourceToken,
                databaseName,
                documentDbOptionsAction);

        public static DbContextOptionsBuilder UseCosmosSql(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<CosmosSqlDbContextOptionsBuilder> documentDbOptionsAction = null)
            => UseCosmosSql(
                optionsBuilder,
                new Uri(serviceEndPoint),
                authKeyOrResourceToken,
                databaseName,
                documentDbOptionsAction);
    }
}
