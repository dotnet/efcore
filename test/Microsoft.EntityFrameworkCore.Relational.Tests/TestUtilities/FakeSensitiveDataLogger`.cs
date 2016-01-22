// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.TestUtilities
{
    public class FakeSensitiveDataLogger<T> : ISensitiveDataLogger<T>

    {
        public bool LogSensitiveData { get; }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope(object state)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScopeImpl(object state) => null;
    }
}
