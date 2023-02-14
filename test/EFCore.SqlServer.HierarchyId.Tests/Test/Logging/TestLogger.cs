using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.Test.Logging
{
    public class TestLogger : ILogger
    {
        public string Sql { get; set; }

        public IDisposable BeginScope<TState>(TState state)
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (eventId != RelationalEventId.CommandExecuting)
                return;

            var structure = (IReadOnlyList<KeyValuePair<string, object>>)state;
            var commandText = (string)structure.FirstOrDefault(i => i.Key == "commandText").Value;

            Sql = commandText;
        }
    }
}
