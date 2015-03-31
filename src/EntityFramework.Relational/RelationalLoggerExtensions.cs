// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    public static class RelationalLoggerExtensions
    {
        public static void LogSql([NotNull] this ILogger logger, [NotNull] string sql)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(sql, nameof(sql));

            logger.LogVerbose(RelationalLoggingEventIds.Sql, sql);
        }

        public static void LogParameters([NotNull] this ILogger logger, [NotNull] DbParameterCollection parameters)
        {
            if (parameters.Count == 0)
            {
                return;
            }
            StringBuilder paramList = new StringBuilder();

            paramList.AppendFormat("{0}: {1}", (parameters[0]).ParameterName, (parameters[0]).Value);
            for (int i = 1; i < parameters.Count; i++)
            {
                paramList.AppendLine();
                paramList.AppendFormat("{0}: {1}", (parameters[i]).ParameterName, (parameters[i]).Value);
            }
            logger.LogDebug(RelationalLoggingEventIds.Sql, paramList.ToString());
        }

        public static void LogCommand([NotNull] this ILogger logger, [NotNull] DbCommand command)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(command, nameof(command));

            var scope = Guid.NewGuid();

            using (logger.BeginScope(scope))
            {
                logger.LogParameters(command.Parameters);
                logger.LogSql(command.CommandText);
            }
        }

        public static void CreatingDatabase([NotNull] this ILogger logger, [NotNull] string databaseName)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(databaseName, nameof(databaseName));

            logger.LogInformation(
                RelationalLoggingEventIds.CreatingDatabase,
                databaseName,
                Strings.RelationalLoggerCreatingDatabase);
        }

        public static void OpeningConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(connectionString, nameof(connectionString));

            logger.LogVerbose(
                RelationalLoggingEventIds.OpeningConnection,
                connectionString,
                Strings.RelationalLoggerOpeningConnection);
        }

        public static void ClosingConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotEmpty(connectionString, nameof(connectionString));

            logger.LogVerbose(
                RelationalLoggingEventIds.ClosingConnection,
                connectionString,
                Strings.RelationalLoggerClosingConnection);
        }

        public static void BeginningTransaction([NotNull] this ILogger logger, IsolationLevel isolationLevel)
        {
            Check.NotNull(logger, nameof(logger));

            logger.LogVerbose(
                RelationalLoggingEventIds.BeginningTransaction,
                isolationLevel,
                il => Strings.RelationalLoggerBeginningTransaction(il.ToString("G")));
        }

        public static void CommittingTransaction([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            logger.LogVerbose(
                RelationalLoggingEventIds.CommittingTransaction,
                Strings.RelationalLoggerCommittingTransaction);
        }

        public static void RollingbackTransaction([NotNull] this ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            logger.LogVerbose(
                RelationalLoggingEventIds.RollingbackTransaction,
                Strings.RelationalLoggerRollingbackTransaction);
        }
    }
}
