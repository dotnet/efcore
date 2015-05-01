// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    internal static class LoggingExtensions
    {
        public static void LogInformation(this ILogger logger, Func<string> formatter)
            => logger.LogInformation(0, default(object), _ => formatter());

        public static void LogInformation<TState>(this ILogger logger, TState state, Func<TState, string> formatter)
            => logger.LogInformation(0, state, formatter);

        public static void LogInformation(this ILogger logger, int eventId, Func<string> formatter)
            => logger.LogInformation(eventId, default(object), _ => formatter());

        public static void LogInformation<TState>(
            this ILogger logger, int eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log(LogLevel.Information, eventId, state, null, (s, _) => formatter((TState)s));
            }
        }

        public static void LogError<TState>(
            this ILogger logger, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.Log(LogLevel.Error, 0, null, exception, (s, e) => formatter((TState)s, e));
            }
        }

        public static void LogError<TState>(
            this ILogger logger, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.Log(LogLevel.Error, 0, state, exception, (s, e) => formatter((TState)s, e));
            }
        }

        public static void LogVerbose<TState>(this ILogger logger, int eventId, TState state)
            => logger.LogVerbose(eventId, state, s => s != null ? s.ToString() : null);

        public static void LogVerbose<TState>(
            this ILogger logger, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Verbose))
            {
                logger.Log(LogLevel.Verbose, 0, state, null, (s, _) => formatter((TState)s));
            }
        }

        public static void LogVerbose<TState>(
            this ILogger logger, int eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Verbose))
            {
                logger.Log(LogLevel.Verbose, eventId, state, null, (s, _) => formatter((TState)s));
            }
        }
    }
}
