// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    internal static class InMemoryLoggerExtensions
    {
        public static void LogInformation<TState>(
            this ILogger logger, InMemoryEventId eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log(LogLevel.Information, (int)eventId, state, null, (s, _) => formatter(s));
            }
        }

        public static void LogWarning(this ILogger logger, InMemoryEventId eventId, Func<string> formatter)
        {
            // Always call Log for Warnings because Warnings as Errors should work even
            // if LogLevel.Warning is not enabled.
            logger.Log<object>(LogLevel.Warning, (int)eventId, eventId, null, (_, __) => formatter());
        }
    }
}
