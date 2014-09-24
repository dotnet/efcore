// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    internal static class RelationalLoggerExtensions
    {
        public static void WriteSql([NotNull] this ILogger logger, [NotNull] string sql)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(sql, "sql");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.Sql, sql, null, (o, _) => (string)o);
        }

        public static void CreatingDatabase([NotNull] this ILogger logger, [NotNull] string databaseName)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(databaseName, "databaseName");

            logger.WriteCore(TraceType.Information, RelationalLoggingEventIds.CreatingDatabase, databaseName, null,
                (o, _) => Strings.FormatRelationalLoggerCreatingDatabase(databaseName));
        }

        public static void OpeningConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(connectionString, "connectionString");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.OpeningConnection, connectionString, null,
                (o, _) => Strings.FormatRelationalLoggerOpeningConnection(o));
        }

        public static void ClosingConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(connectionString, "connectionString");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.ClosingConnection, connectionString, null,
                (o, _) => Strings.FormatRelationalLoggerClosingConnection(o));
        }

        public static void BeginningTransaction([NotNull] this ILogger logger, IsolationLevel isolationLevel)
        {
            Check.NotNull(logger, "logger");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.BeginningTransaction, isolationLevel.ToString("G"), null,
                (o, _) => Strings.FormatRelationalLoggerBeginningTransaction(o));
        }

        public static void CommittingTransaction([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, "logger");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.CommittingTransaction, null, null,
                (_, __) => Strings.RelationalLoggerCommittingTransaction);
        }

        public static void RollingbackTransaction([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, "logger");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.RollingbackTransaction, null, null,
                (_, __) => Strings.RelationalLoggerRollingbackTransaction);
        }
    }
}
