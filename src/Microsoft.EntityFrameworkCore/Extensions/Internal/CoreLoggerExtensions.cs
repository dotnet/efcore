// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    internal static class CoreLoggerExtensions
    {
        public static void LogError<TState>(
            this ILogger logger, CoreLoggingEventId eventId, Func<TState> state, Exception exception, Func<Exception, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.Log(LogLevel.Error, (int)eventId, state(), exception, (_, e) => formatter(e));
            }
        }

        public static void LogDebug(this ILogger logger, CoreLoggingEventId eventId, Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
            }
        }
    }
}
