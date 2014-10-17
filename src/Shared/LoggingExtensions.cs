// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.Logging
{
    internal static class LoggingExtensions
    {
        public static void WriteInformation(this ILogger logger, Func<string> formatter)
        {
            logger.WriteInformation(0, default(object), _ => formatter());
        }

        public static void WriteInformation<TState>(this ILogger logger, TState state, Func<TState, string> formatter)
        {
            logger.WriteInformation(0, state, formatter);
        }

        public static void WriteInformation(this ILogger logger, int eventId, Func<string> formatter)
        {
            logger.WriteInformation(eventId, default(object), _ => formatter());
        }

        public static void WriteInformation<TState>(
            this ILogger logger, int eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(TraceType.Information))
            {
                logger.Write(TraceType.Information, eventId, state, null, (s, _) => formatter((TState)s));
            }
        }

        public static void WriteError<TState>(
            this ILogger logger, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logger.IsEnabled(TraceType.Error))
            {
                logger.Write(TraceType.Error, 0, null, exception, (s, e) => formatter((TState)s, e));
            }
        }

        public static void WriteError<TState>(
            this ILogger logger, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logger.IsEnabled(TraceType.Error))
            {
                logger.Write(TraceType.Error, 0, state, exception, (s, e) => formatter((TState)s, e));
            }
        }

        public static void WriteVerbose<TState>(this ILogger logger, int eventId, TState state)
        {
            logger.WriteVerbose(eventId, state, s => s != null ? s.ToString() : null);
        }

        public static void WriteVerbose<TState>(
            this ILogger logger, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(TraceType.Verbose))
            {
                logger.Write(TraceType.Verbose, 0, state, null, (s, _) => formatter((TState)s));
            }
        }

        public static void WriteVerbose<TState>(
            this ILogger logger, int eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(TraceType.Verbose))
            {
                logger.Write(TraceType.Verbose, eventId, state, null, (s, _) => formatter((TState)s));
            }
        }
    }
}
