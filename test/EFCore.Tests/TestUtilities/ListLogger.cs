// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Tests.TestUtilities
{
    public class ListLogger : ILogger
    {
        public ListLogger(List<Tuple<LogLevel, string>> logMessages)
        {
            LogMessages = logMessages;
        }

        public List<Tuple<LogLevel, string>> LogMessages { get; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = new StringBuilder();
            if (formatter != null)
            {
                message.Append(formatter(state, exception));
            }
            else if (state != null)
            {
                message.Append(state);

                if (exception != null)
                {
                    message.Append(Environment.NewLine);
                    message.Append(exception);
                }
            }

            LogMessages.Add(new Tuple<LogLevel, string>(logLevel, message.ToString()));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope(object state)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
