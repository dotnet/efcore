// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Services
{
    using System;
    using System.Collections.Concurrent;
    using JetBrains.Annotations;
    using Microsoft.AspNet.Logging;
    using Microsoft.Data.Entity.Utilities;

    public class ConsoleLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers
            = new ConcurrentDictionary<string, ConsoleLogger>(StringComparer.OrdinalIgnoreCase);

        public virtual ILogger Create([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return _loggers.GetOrAdd(name, new ConsoleLogger(name));
        }

        private class ConsoleLogger : ILogger
        {
            private readonly string _name;

            public ConsoleLogger(string name)
            {
                _name = name;
            }

            public bool WriteCore(
                TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                DebugCheck.NotNull(formatter);

                Console.WriteLine("{0}: {1}: {2}", _name, eventType, formatter(state, exception));

                return true;
            }
        }
    }
}
