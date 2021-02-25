// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class FakeLogger<T> : ILogger<T>
    {
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public IDisposable BeginScope<TState>(TState state)
            => throw new NotImplementedException();
    }
}
