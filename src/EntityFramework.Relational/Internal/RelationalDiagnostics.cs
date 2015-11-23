// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.Data.Entity.Internal
{
    internal static class RelationalDiagnostics
    {
        private const string NamePrefix = "Microsoft.Data.Entity.";

        public const string BeforeExecuteCommand = NamePrefix + nameof(BeforeExecuteCommand);
        public const string AfterExecuteCommand = NamePrefix + nameof(AfterExecuteCommand);
        public const string CommandExecutionError = NamePrefix + nameof(CommandExecutionError);

        public static void WriteCommand(
            this DiagnosticSource diagnosticSource,
            string diagnosticName,
            DbCommand command,
            string executeMethod,
            bool async)
        {
            if (diagnosticSource.IsEnabled(diagnosticName))
            {
                diagnosticSource.Write(
                    diagnosticName,
                    new
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        IsAsync = async
                    });
            }
        }

        public static void WriteCommandError(
            this DiagnosticSource diagnosticSource,
            DbCommand command,
            string executeMethod,
            bool async,
            Exception exception)
        {
            if (diagnosticSource.IsEnabled(CommandExecutionError))
            {
                diagnosticSource.Write(
                    CommandExecutionError,
                    new
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        IsAsync = async,
                        Exception = exception
                    });
            }
        }
    }
}
