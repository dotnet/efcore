// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ListLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory _factory;

        public ListLoggerProvider(List<(LogLevel, EventId, string)> log)
            => _factory = new ListLoggerFactory(log);

        public ILogger CreateLogger(string categoryName)
            => _factory.CreateLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
