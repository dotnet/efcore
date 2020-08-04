// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class CosmosLoggerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ExecutingSqlQuery(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnosticsLogger,
            [NotNull] CosmosSqlQuery cosmosSqlQuery)
        {
            var definition = new EventDefinition<string, string, string>(
                diagnosticsLogger.Options,
                CosmosEventId.ExecutingSqlQuery,
                LogLevel.Debug,
                "CosmosEventId.ExecutingSqlQuery",
                level => LoggerMessage.Define<string, string, string>(
                    level,
                    CosmosEventId.ExecutingSqlQuery,
                    "Executing Sql Query [Parameters=[{parameters}]]{newLine}{commandText}"));

            definition.Log(
                diagnosticsLogger,
                FormatParameters(cosmosSqlQuery.Parameters, ShouldLogParameterValues(diagnosticsLogger, cosmosSqlQuery)),
                Environment.NewLine,
                cosmosSqlQuery.Query);
        }

        private static bool ShouldLogParameterValues(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            CosmosSqlQuery cosmosSqlQuery)
            => cosmosSqlQuery.Parameters.Count > 0
                && diagnostics.ShouldLogSensitiveData();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ExecutingReadItem(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnosticsLogger,
            [NotNull] string partitionKey,
            [NotNull] string resourceId)
        {
            var definition = new EventDefinition<string>(
                diagnosticsLogger.Options,
                CosmosEventId.ExecutingReadItem,
                LogLevel.Debug,
                "CosmosEventId.ExecutingReadItem",
                level => LoggerMessage.Define<string>(
                    level,
                    CosmosEventId.ExecutingReadItem,
                    "Executing Read Item [Partition Key, Resource Id=[{parameters}]]"));

            definition.Log(
                diagnosticsLogger,
                $"{partitionKey}, {resourceId}");
        }

        private static string FormatParameters(IReadOnlyList<SqlParameter> parameters, bool shouldLogParameterValues)
        {
            return parameters.Count == 0
                ? ""
                : string.Join(", ", parameters.Select(e => FormatParameter(e, shouldLogParameterValues)));
        }

        private static string FormatParameter(SqlParameter parameter, bool shouldLogParameterValue)
        {
            var builder = new StringBuilder();
            builder
                .Append(parameter.Name)
                .Append("=");

            if (shouldLogParameterValue)
            {
                FormatParameterValue(builder, parameter.Value);
            }
            else
            {
                builder.Append("?");
            }

            return builder.ToString();
        }

        private static void FormatParameterValue(StringBuilder builder, object parameterValue)
        {
            if (parameterValue == null)
            {
                builder.Append("null");
                return;
            }

            builder.Append('\'');

            switch (parameterValue)
            {
                case JToken jTokenValue:
                    builder.Append(jTokenValue.ToString(Formatting.None).Trim('"'));
                    break;
                case DateTime dateTimeValue:
                    builder.Append(dateTimeValue.ToString("s"));
                    break;
                case DateTimeOffset dateTimeOffsetValue:
                    builder.Append(dateTimeOffsetValue.ToString("o"));
                    break;
                case byte[] binaryValue:
                    builder.AppendBytes(binaryValue);
                    break;
                default:
                    builder.Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture));
                    break;
            }

            builder.Append('\'');
        }
    }
}
