// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics.Tracing;

namespace Microsoft.Data.Entity.Internal
{
    internal static class RelationalTelemetry
    {
        private const string NamePrefix = "Microsoft.Data.Entity.";

        public const string BeforeExecuteCommand = NamePrefix + nameof(BeforeExecuteCommand);
        public const string AfterExecuteCommand = NamePrefix + nameof(AfterExecuteCommand);
        public const string CommandExecutionError = NamePrefix + nameof(CommandExecutionError);

        public static class ExecuteMethod
        {
            public const string ExecuteReader = nameof(ExecuteReader);
            public const string ExecuteScalar = nameof(ExecuteScalar);
            public const string ExecuteNonQuery = nameof(ExecuteNonQuery);
        }

        public static void WriteCommand(
            this TelemetrySource telemetrySource,
            string telemetryName,
            DbCommand command,
            string executeMethod,
            bool async)
        {
            if (telemetrySource.IsEnabled(telemetryName))
            {
                telemetrySource.WriteTelemetry(
                    telemetryName,
                    new
                    {
                        command = command,
                        executeMethod = executeMethod,
                        isAsync = async
                    });
            }
        }

        public static void WriteCommandError(
            this TelemetrySource telemetrySource,
            DbCommand command,
            string executeMethod,
            bool async,
            Exception exception)
        {
            if (telemetrySource.IsEnabled(CommandExecutionError))
            {
                telemetrySource.WriteTelemetry(
                    CommandExecutionError,
                    new
                    {
                        command = command,
                        executeMethod = executeMethod,
                        isAsync = async,
                        exception = exception
                    });
            }
        }
    }
}
