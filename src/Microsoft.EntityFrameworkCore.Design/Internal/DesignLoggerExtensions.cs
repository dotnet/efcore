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
        public static void ReportWarning(
               [NotNull] this ILogger logger,
               DesignEventId eventId,
               [NotNull] Func<string> formatter)
               => logger.LogReported<object>(LogLevel.Warning, (int)eventId, null, null, (_, __) => formatter());

        public static void ReportInformation(
               [NotNull] this ILogger logger,
               DesignEventId eventId,
               [NotNull] Func<string> formatter)
               => logger.LogReported<object>(LogLevel.Information, (int)eventId, null, null, (_, __) => formatter());

        public static void ReportDebug(
            [NotNull] this ILogger logger,
            DesignEventId eventId,
            [NotNull] Func<string> formatter)
            => logger.LogReported<object>(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
    }
}
