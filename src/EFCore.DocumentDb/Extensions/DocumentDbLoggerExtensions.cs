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

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class DocumentDbLoggerExtensions
    {
        public static void ExecutingQuery(
               [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
               SqlQuerySpec querySpec)
        {
            var definition = new EventDefinition<string, string, string>(
                CoreEventId.ProviderBaseId,
                LogLevel.Debug,
                LoggerMessage.Define<string, string, string>(
                    LogLevel.Debug,
                    CoreEventId.ProviderBaseId,
                    "Executing DocumentQuery [Parameters=[{parameters}]]{newLine}{commandText}"));

            var warningBehavior = definition.GetLogBehavior(diagnostics);

            definition.Log(
                diagnostics,
                warningBehavior,
                FormatParameters(querySpec.Parameters),
                Environment.NewLine,
                querySpec.QueryText);
        }

        private static string FormatParameters(SqlParameterCollection parameters)
        {
            if (parameters.Count == 0)
            {
                return "";
            }

            return string.Join(", ", parameters.Select(p => FormatParameter(p)));
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
            builder.Append('\'');

            if (parameterValue == null)
            {
                builder.Append('\'');
                return;
            }
            else if (parameterValue.GetType() == typeof(DateTime))
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
