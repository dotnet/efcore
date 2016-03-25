// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.EntityFrameworkCore.Internal
{
    internal static class RelationalDiagnostics
    {
        private const string NamePrefix = "Microsoft.EntityFrameworkCore.";

        public const string BeforeExecuteCommand = NamePrefix + nameof(BeforeExecuteCommand);
        public const string AfterExecuteCommand = NamePrefix + nameof(AfterExecuteCommand);
        public const string CommandExecutionError = NamePrefix + nameof(CommandExecutionError);

        public static void WriteCommandBefore(
            this DiagnosticSource diagnosticSource,
            DbCommand command,
            string executeMethod,
            bool async,
            Guid instanceId,
            long startTimestamp)
        {
            if (diagnosticSource.IsEnabled(BeforeExecuteCommand))
            {
                diagnosticSource.Write(
                    BeforeExecuteCommand,
                    new RelationalDiagnosticSourceBeforeMessage
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        IsAsync = async,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp
                    });
            }
        }

        public static void WriteCommandAfter(
            this DiagnosticSource diagnosticSource,
            DbCommand command,
            string executeMethod,
            bool async,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp)
        {
            if (diagnosticSource.IsEnabled(AfterExecuteCommand))
            {
                diagnosticSource.Write(
                    AfterExecuteCommand,
                    new RelationalDiagnosticSourceAfterMessage
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        IsAsync = async,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        public static void WriteCommandError(
            this DiagnosticSource diagnosticSource,
            DbCommand command,
            string executeMethod,
            bool async,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            Exception exception)
        {
            if (diagnosticSource.IsEnabled(CommandExecutionError))
            {
                diagnosticSource.Write(
                    CommandExecutionError,
                    new RelationalDiagnosticSourceAfterMessage
                    {
                        Command = command,
                        ExecuteMethod = executeMethod,
                        IsAsync = async,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        Exception = exception
                    });
            }
        }
    }
}
