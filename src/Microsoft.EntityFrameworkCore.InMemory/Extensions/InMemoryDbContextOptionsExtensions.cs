// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class InMemoryDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder UseInMemoryDatabase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [CanBeNull] string databaseName,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var extension = optionsBuilder.Options.FindExtension<InMemoryOptionsExtension>();
            extension = extension != null
                ? new InMemoryOptionsExtension(extension)
                : new InMemoryOptionsExtension();

            if (databaseName != null)
            {
                extension.StoreName = databaseName;
            }
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            inMemoryOptionsAction?.Invoke(new InMemoryDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseInMemoryDatabase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [CanBeNull] string databaseName,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseInMemoryDatabase(
                (DbContextOptionsBuilder)optionsBuilder, databaseName, inMemoryOptionsAction);

        public static DbContextOptionsBuilder UseInMemoryDatabase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new InMemoryOptionsExtension());

            inMemoryOptionsAction?.Invoke(new InMemoryDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseInMemoryDatabase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [CanBeNull] Action<InMemoryDbContextOptionsBuilder> inMemoryOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseInMemoryDatabase(
                (DbContextOptionsBuilder)optionsBuilder, inMemoryOptionsAction);
    }
}
