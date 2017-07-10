// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ListLoggerFactory : ILoggerFactory
    {
        private readonly Func<string, bool> _shouldCreateLogger;
        private readonly List<(LogLevel, EventId, string)> _log;

        public ListLoggerFactory(List<(LogLevel, EventId, string)> log)
            : this(log, null)

        {
        }

        public ListLoggerFactory(List<(LogLevel, EventId, string)> log, Func<string, bool> shouldCreateLogger)
        {
            _log = log;
            _shouldCreateLogger = shouldCreateLogger;
        }

        public ILogger CreateLogger(string name)
            => _shouldCreateLogger != null && !_shouldCreateLogger(name)
                ? (ILogger)NullLogger.Instance
                : new ListLogger(_log);

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }
    }
}
