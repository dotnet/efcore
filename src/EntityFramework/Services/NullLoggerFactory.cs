// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Services
{
    public sealed class NullLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string _)
        {
            return NullLogger.Instance;
        }
    }

    public sealed class NullLogger : ILogger
    {
        public static readonly ILogger Instance = new NullLogger();

        private NullLogger()
        {
        }

        public bool WriteCore(
            TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            return false;
        }

        public IDisposable BeginScope(object state)
        {
            return NullScope.Instance;
        }
    }

    public sealed class NullScope : IDisposable
    {
        public static NullScope Instance = new NullScope();

        public void Dispose()
        {
            // intentionally does nothing
        }
    }
}
