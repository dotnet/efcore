// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    internal static class RelationalLoggerExtensions
    {
        public static void WriteSql([NotNull] this ILogger logger, [NotNull] string sql)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(sql, nameof(sql));

            logger.WriteVerbose(RelationalLoggingEventIds.Sql, sql);
        }

        public static void CreatingDatabase([NotNull] this ILogger logger, [NotNull] string databaseName)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(databaseName, nameof(databaseName));

            logger.WriteInformation(
                RelationalLoggingEventIds.CreatingDatabase,
                databaseName,
                Strings.RelationalLoggerCreatingDatabase);
        }

        public static void OpeningConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(connectionString, nameof(connectionString));

            logger.WriteVerbose(
                RelationalLoggingEventIds.OpeningConnection,
                connectionString,
                Strings.RelationalLoggerOpeningConnection);
        }

        public static void ClosingConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(connectionString, nameof(connectionString));

            logger.WriteVerbose(
                RelationalLoggingEventIds.ClosingConnection,
                connectionString,
                Strings.RelationalLoggerClosingConnection);
        }

        public static void BeginningTransaction([NotNull] this ILogger logger, IsolationLevel isolationLevel)
        {
            Check.NotNull(logger, nameof(logger));

            logger.WriteVerbose(
                RelationalLoggingEventIds.BeginningTransaction,
                isolationLevel,
                il => Strings.RelationalLoggerBeginningTransaction(il.ToString("G")));
        }

        public static void CommittingTransaction([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            logger.WriteVerbose(
                RelationalLoggingEventIds.CommittingTransaction,
                Strings.RelationalLoggerCommittingTransaction);
        }

        public static void RollingbackTransaction([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            logger.WriteVerbose(
                RelationalLoggingEventIds.RollingbackTransaction,
                Strings.RelationalLoggerRollingbackTransaction);
        }
    }
}
