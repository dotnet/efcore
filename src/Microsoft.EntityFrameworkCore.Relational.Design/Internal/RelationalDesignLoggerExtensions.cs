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
    }
}
