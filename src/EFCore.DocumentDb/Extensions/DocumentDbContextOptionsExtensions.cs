// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class DocumentDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseDocumentDb<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] Uri serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<DocumentDbContextOptionsBuilder> documentDbOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseDocumentDb(
                (DbContextOptionsBuilder)optionsBuilder,
                serviceEndPoint,
                authKeyOrResourceToken,
                databaseName,
                documentDbOptionsAction);

        public static DbContextOptionsBuilder UseDocumentDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] Uri serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<DocumentDbContextOptionsBuilder> documentDbOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<DocumentDbOptionsExtension>()
                            ?? new DocumentDbOptionsExtension();

            extension = extension
                .WithServiceEndPoint(serviceEndPoint)
                .WithAuthKeyOrResourceToken(authKeyOrResourceToken)
                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            documentDbOptionsAction?.Invoke(new DocumentDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseDocumentDb<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<DocumentDbContextOptionsBuilder> documentDbOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseDocumentDb(
                (DbContextOptionsBuilder)optionsBuilder,
                serviceEndPoint,
                authKeyOrResourceToken,
                databaseName,
                documentDbOptionsAction);

        public static DbContextOptionsBuilder UseDocumentDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string serviceEndPoint,
            [NotNull] string authKeyOrResourceToken,
            [NotNull] string databaseName,
            [CanBeNull] Action<DocumentDbContextOptionsBuilder> documentDbOptionsAction = null)
            => UseDocumentDb(
                optionsBuilder,
                new Uri(serviceEndPoint),
                authKeyOrResourceToken,
                databaseName,
                documentDbOptionsAction);
    }
}
