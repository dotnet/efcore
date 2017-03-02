// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;

namespace Microsoft.EntityFrameworkCore.Tests.TestUtilities
{
    public class ListLoggerFactory : ILoggerFactory
    {
        private readonly Func<string, bool> _shouldCreateLogger;
        private readonly List<Tuple<LogLevel, string>> _log;

        public ListLoggerFactory(List<Tuple<LogLevel, string>> log)
            : this(log, null)

        {
        }

        public ListLoggerFactory(List<Tuple<LogLevel, string>> log, Func<string, bool> shouldCreateLogger)
        {
            _log = log;
            _shouldCreateLogger = shouldCreateLogger;
        }

        public ILogger CreateLogger(string name)
        {
            if ((_shouldCreateLogger != null)
                && !_shouldCreateLogger(name))
            {
                return Mock.Of<ILogger>();
            }

            return new ListLogger(_log);
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }
    }
}
