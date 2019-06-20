// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class CosmosDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseCosmos<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string accountEndpoint,
            [NotNull] string accountKey,
            [NotNull] string databaseName,
            [CanBeNull] Action<CosmosDbContextOptionsBuilder> cosmosOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCosmos(
                (DbContextOptionsBuilder)optionsBuilder,
                accountEndpoint,
                accountKey,
                databaseName,
                cosmosOptionsAction);

        public static DbContextOptionsBuilder UseCosmos(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string accountEndpoint,
            [NotNull] string accountKey,
            [NotNull] string databaseName,
            [CanBeNull] Action<CosmosDbContextOptionsBuilder> cosmosOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(accountEndpoint, nameof(accountEndpoint));
            Check.NotEmpty(accountKey, nameof(accountKey));
            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<CosmosOptionsExtension>()
                            ?? new CosmosOptionsExtension();

            extension = extension
                .WithAccountEndpoint(accountEndpoint)
                .WithAccountKey(accountKey)
                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            cosmosOptionsAction?.Invoke(new CosmosDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }
    }
}
