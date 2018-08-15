// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public static class CosmosSqlLoggerExtensions
    {
        public static void ExecutingSqlQuery(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnosticsLogger,
            SqlQuerySpec sqlQuerySpec)
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
                FormatParameters(sqlQuerySpec.Parameters),
                Environment.NewLine,
                sqlQuerySpec.QueryText);
        }

        private static string FormatParameters(SqlParameterCollection parameters)
        {
            return parameters.Count == 0
                ? ""
                : string.Join(", ", parameters.Select(p => FormatParameter(p)));
        }

        private static string FormatParameter(SqlParameter parameter)
        {
            var builder = new StringBuilder();
            var clrType = parameter.Value?.GetType();
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

            if (parameterValue.GetType() == typeof(DateTime))
            {
                builder.Append(((DateTime)parameterValue).ToString("s"));
            }
            else if (parameterValue.GetType() == typeof(DateTimeOffset))
            {
                builder.Append(((DateTimeOffset)parameterValue).ToString("o"));
            }
            else if (parameterValue.GetType() == typeof(byte[]))
            {
                var buffer = (byte[])parameterValue;
                builder.Append("0x");

                for (var i = 0; i < buffer.Length; i++)
                {
                    if (i > 31)
                    {
                        builder.Append("...");
                        break;
                    }

                    builder.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
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
