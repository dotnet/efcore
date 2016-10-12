// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    internal static class DesignLoggerExtensions
    {
        public static void LogWarning(
            [NotNull] this ILogger logger,
            DesignEventId eventId,
            [NotNull] Func<string> formatter)
            => logger.Log<object>(LogLevel.Warning, (int)eventId, null, null, (_, __) => formatter());

        public static void LogInformation(
            [NotNull] this ILogger logger,
            DesignEventId eventId,
            [NotNull] Func<string> formatter)
            => logger.Log<object>(LogLevel.Information, (int)eventId, null, null, (_, __) => formatter());

        public static void LogDebug(
            [NotNull] this ILogger logger,
            DesignEventId eventId,
            [NotNull] Func<string> formatter)
            => logger.Log<object>(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
    }
}
