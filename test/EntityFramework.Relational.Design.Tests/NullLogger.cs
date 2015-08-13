// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class NullLogger : ILogger
    {
        public virtual IDisposable BeginScopeImpl(object state) => null;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            // do nothing
        }
    }
}
