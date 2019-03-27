// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Storage;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public static class CosmosLoggerExtensions
    {
        public static void ExecutingSqlQuery(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnosticsLogger,
            CosmosSqlQuery cosmosSqlQuery)
        {
            var definition = new EventDefinition<string, string, string>(
                CoreEventId.ProviderBaseId,
                LogLevel.Debug,
                LoggerMessage.Define<string, string, string>(
                    LogLevel.Debug,
                    CoreEventId.ProviderBaseId,
                    "Executing Sql Query [Parameters=[{parameters}]]{newLine}{commandText}"));

            var warningBehavior = definition.GetLogBehavior(diagnosticsLogger);

            definition.Log(
                diagnosticsLogger,
                warningBehavior,
                FormatParameters(cosmosSqlQuery.Parameters),
                Environment.NewLine,
                cosmosSqlQuery.Query);
        }

        private static string FormatParameters(IReadOnlyList<SqlParameter> parameters)
        {
            return parameters.Count == 0
                ? ""
                : string.Join(", ", parameters.Select(FormatParameter));
        }

        private static string FormatParameter(SqlParameter parameter)
        {
            var builder = new StringBuilder();
            builder
                .Append(parameter.Name)
                .Append("=");

            FormatParameterValue(builder, parameter.Value);

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

            if (parameterValue is DateTime dateTimeValue)
            {
                builder.Append(dateTimeValue.ToString("s"));
            }
            else if (parameterValue is DateTimeOffset dateTimeOffsetValue)
            {
                builder.Append(dateTimeOffsetValue.ToString("o"));
            }
            else if (parameterValue is byte[] binaryValue)
            {
                builder.Append("0x");

                for (var i = 0; i < binaryValue.Length; i++)
                {
                    if (i > 31)
                    {
                        builder.Append("...");
                        break;
                    }

                    builder.Append(binaryValue[i].ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            else
            {
                builder.Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture));
            }

            builder.Append('\'');
        }
    }
}
