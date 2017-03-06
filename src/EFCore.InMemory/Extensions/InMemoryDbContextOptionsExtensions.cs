// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     In-memory specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    public static class InMemoryDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to an in-memory database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context being configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context.
        /// </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseInMemoryDatabase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string databaseName,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseInMemoryDatabase(
                (DbContextOptionsBuilder)optionsBuilder, databaseName, inMemoryOptionsAction);

        /// <summary>
        ///     Configures the context to connect to an in-memory database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context.
        /// </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseInMemoryDatabase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string databaseName,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<InMemoryOptionsExtension>()
                ?? new InMemoryOptionsExtension();

            if (databaseName != null)
            {
                extension = extension.WithStoreName(databaseName);
            }

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            inMemoryOptionsAction?.Invoke(new InMemoryDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to an in-memory database. The database will only exist for the scope
        ///     of the <see cref="DbContext"/> instance that uses it. Use <see cref="UseInMemoryDatabase{TContext}"/> with
        ///     a name to create a longer-lived database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context being configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseTransientInMemoryDatabase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseTransientInMemoryDatabase(
                (DbContextOptionsBuilder)optionsBuilder, inMemoryOptionsAction);

        /// <summary>
        ///     Configures the context to connect to an in-memory database. The database will only exist for the scope
        ///     of the <see cref="DbContext"/> instance that uses it. Use <see cref="UseInMemoryDatabase{TContext}"/> with
        ///     a name to create a longer-lived database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseTransientInMemoryDatabase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new InMemoryOptionsExtension());

            inMemoryOptionsAction?.Invoke(new InMemoryDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            // Set warnings defaults
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                  ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    InMemoryEventId.TransactionIgnoredWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
