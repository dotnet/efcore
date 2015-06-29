// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Internal;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    internal static class RelationalLoggerExtensions
    {
        public static void LogSql([NotNull] this ILogger logger, [NotNull] string sql)
            => logger.LogVerbose(RelationalLoggingEventIds.ExecutingSql, sql);

        public static void LogParameters([NotNull] this ILogger logger, [NotNull] DbParameterCollection parameters)
        {
            if (parameters.Count == 0)
            {
                return;
            }
            var paramList = new StringBuilder();

            paramList.AppendFormat("{0}: {1}", (parameters[0]).ParameterName, Convert.ToString((parameters[0]).Value, CultureInfo.InvariantCulture));
            for (var i = 1; i < parameters.Count; i++)
            {
                paramList.AppendLine();
                paramList.AppendFormat("{0}: {1}", (parameters[i]).ParameterName, Convert.ToString((parameters[i]).Value, CultureInfo.InvariantCulture));
            }
            logger.LogDebug(RelationalLoggingEventIds.ExecutingSql, paramList.ToString());
        }

        public static void LogCommand([NotNull] this ILogger logger, [NotNull] DbCommand command)
        {
            var scope = Guid.NewGuid();

            using (logger.BeginScopeImpl(scope))
            {
                logger.LogParameters(command.Parameters);
                logger.LogSql(command.CommandText);
            }
        }

        public static void CreatingDatabase([NotNull] this ILogger logger, [NotNull] string databaseName)
            => logger.LogInformation(
                RelationalLoggingEventIds.CreatingDatabase,
                databaseName,
                Strings.RelationalLoggerCreatingDatabase);

        public static void OpeningConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
            => logger.LogVerbose(
                RelationalLoggingEventIds.OpeningConnection,
                connectionString,
                Strings.RelationalLoggerOpeningConnection);

        public static void ClosingConnection([NotNull] this ILogger logger, [NotNull] string connectionString)
            => logger.LogVerbose(
                RelationalLoggingEventIds.ClosingConnection,
                connectionString,
                Strings.RelationalLoggerClosingConnection);

        public static void BeginningTransaction([NotNull] this ILogger logger, IsolationLevel isolationLevel)
            => logger.LogVerbose(
                RelationalLoggingEventIds.BeginningTransaction,
                isolationLevel,
                il => Strings.RelationalLoggerBeginningTransaction(il.ToString("G")));

        public static void CommittingTransaction([NotNull] this ILogger logger)
            => logger.LogVerbose(
                RelationalLoggingEventIds.CommittingTransaction,
                Strings.RelationalLoggerCommittingTransaction);

        public static void RollingbackTransaction([NotNull] this ILogger logger)
            => logger.LogVerbose(
                RelationalLoggingEventIds.RollingbackTransaction,
                Strings.RelationalLoggerRollingbackTransaction);
    }
}
