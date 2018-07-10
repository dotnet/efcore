// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
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

        private static string FormatParameters(SqlParameterCollection parameters) => "";
    }
}
