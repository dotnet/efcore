// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

 // ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    internal static class LoggingExtensions
    {
        public const string CommandsLoggerName = "EntityFramework.Commands";

        public static ILogger CreateCommandsLogger(this ILoggerFactory loggerFactory)
            => loggerFactory.CreateLogger(CommandsLoggerName);

        public static ILogger CreateCommandsLogger(this ILoggerProvider loggerProvider)
            => loggerProvider.CreateLogger(CommandsLoggerName);
    }
}
