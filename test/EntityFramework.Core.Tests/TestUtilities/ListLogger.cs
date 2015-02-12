// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Tests.TestUtilities
{
    public class ListLogger : ILogger
    {
        public ListLogger(List<Tuple<LogLevel, string>> log)
        {
            Log = log;
        }

        public List<Tuple<LogLevel, string>> Log { get; }

        public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
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

            Log.Add(new Tuple<LogLevel, string>(logLevel, message.ToString()));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope(object state) => null;
    }
}
