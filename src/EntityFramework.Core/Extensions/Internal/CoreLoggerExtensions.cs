// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Extensions.Internal
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

        public static void LogVerbose(this ILogger logger, CoreLoggingEventId eventId, Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Verbose))
            {
                logger.Log(LogLevel.Verbose, (int)eventId, null, null, (_, __) => formatter());
            }
        }
    }
}
