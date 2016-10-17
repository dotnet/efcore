// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class RelationalDesignLoggerExtensions
    {
        public static void LogWarning(
            [NotNull] this ILogger logger,
            RelationalDesignEventId eventId,
            [NotNull] Func<string> formatter)
            => logger.Log<object>(LogLevel.Warning, (int)eventId, null, null, (_, __) => formatter());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogDebug(
            [NotNull] this ILogger logger,
            RelationalDesignEventId eventId,
            [NotNull] Func<string> formatter)
            => logger.Log<object>(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
    }
}
