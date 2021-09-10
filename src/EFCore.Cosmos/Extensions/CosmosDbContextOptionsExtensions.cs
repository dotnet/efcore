// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos-specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
    /// </remarks>
    public static class CosmosDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to an Azure Cosmos database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
        /// </remarks>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="accountEndpoint"> The account end-point to connect to. </param>
        /// <param name="accountKey"> The account key. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="cosmosOptionsAction"> An optional action to allow additional Cosmos-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseCosmos<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string accountEndpoint,
            string accountKey,
            string databaseName,
            Action<CosmosDbContextOptionsBuilder>? cosmosOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCosmos(
                (DbContextOptionsBuilder)optionsBuilder,
                accountEndpoint,
                accountKey,
                databaseName,
                cosmosOptionsAction);

        /// <summary>
        ///     Configures the context to connect to an Azure Cosmos database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
        /// </remarks>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="accountEndpoint"> The account end-point to connect to. </param>
        /// <param name="accountKey"> The account key. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="cosmosOptionsAction"> An optional action to allow additional Cosmos-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseCosmos(
            this DbContextOptionsBuilder optionsBuilder,
            string accountEndpoint,
            string accountKey,
            string databaseName,
            Action<CosmosDbContextOptionsBuilder>? cosmosOptionsAction = null)
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

        /// <summary>
        ///     Configures the context to connect to an Azure Cosmos database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
        /// </remarks>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="cosmosOptionsAction"> An optional action to allow additional Cosmos-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseCosmos<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string connectionString,
            string databaseName,
            Action<CosmosDbContextOptionsBuilder>? cosmosOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCosmos(
                (DbContextOptionsBuilder)optionsBuilder,
                connectionString,
                databaseName,
                cosmosOptionsAction);

        /// <summary>
        ///     Configures the context to connect to an Azure Cosmos database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
        /// </remarks>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="cosmosOptionsAction"> An optional action to allow additional Cosmos-specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseCosmos(
            this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            string databaseName,
            Action<CosmosDbContextOptionsBuilder>? cosmosOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<CosmosOptionsExtension>()
                ?? new CosmosOptionsExtension();

            extension = extension
                .WithConnectionString(connectionString)
                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            cosmosOptionsAction?.Invoke(new CosmosDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }
    }
}
