// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestSqlLoggerFactory : ListLoggerFactory
    {
        private readonly bool _proceduralQueryGeneration = false;

        private const string FileNewLine = @"
";

        private static readonly string _eol = Environment.NewLine;

        public TestSqlLoggerFactory()
            : this(_ => true)
        {
        }

        public TestSqlLoggerFactory(Func<string, bool> shouldLogCategory)
            : base(c => shouldLogCategory(c) || c == DbLoggerCategory.Database.Command.Name)
        {
            Logger = new TestSqlLogger(shouldLogCategory(DbLoggerCategory.Database.Command.Name));
        }

        public IReadOnlyList<string> SqlStatements => ((TestSqlLogger)Logger).SqlStatements;
        public IReadOnlyList<string> Parameters => ((TestSqlLogger)Logger).Parameters;
        public string Sql => string.Join(_eol + _eol, SqlStatements);

        public void AssertBaseline(string[] expected)
        {
            if (_proceduralQueryGeneration)
            {
                return;
            }

            try
            {
                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i], SqlStatements[i], ignoreLineEndingDifferences: true);
                }
            }
            catch
            {
                var methodCallLine = Environment.StackTrace.Split(
                    new[] { _eol },
                    StringSplitOptions.RemoveEmptyEntries)[3].Substring(6);

                var testName = methodCallLine.Substring(0, methodCallLine.IndexOf(')') + 1);
                var lineIndex = methodCallLine.LastIndexOf("line", StringComparison.Ordinal);
                var lineNumber = lineIndex > 0 ? methodCallLine.Substring(lineIndex) : "";

                const string indent = FileNewLine + "                ";

                var currentDirectory = Directory.GetCurrentDirectory();
                var logFile = currentDirectory.Substring(
                        0,
                        currentDirectory.LastIndexOf("\\artifacts\\", StringComparison.Ordinal) + 1)
                    + "QueryBaseline.txt";

                var testInfo = testName + " : " + lineNumber + FileNewLine;

                var newBaseLine = $@"            AssertSql(
                {string.Join("," + indent + "//" + indent, SqlStatements.Take(9).Select(sql => "@\"" + sql.Replace("\"", "\"\"") + "\""))});

";

                if (SqlStatements.Count > 9)
                {
                    newBaseLine += "Output truncated.";
                }

                Logger.TestOutputHelper?.WriteLine("---- New Baseline -------------------------------------------------------------------");
                Logger.TestOutputHelper?.WriteLine(newBaseLine);

                var contents = testInfo + newBaseLine + FileNewLine + FileNewLine;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }

        protected class TestSqlLogger : ListLogger
        {
            private readonly bool _shouldLogCommands;

            public TestSqlLogger(bool shouldLogCommands) => _shouldLogCommands = shouldLogCommands;

            public List<string> SqlStatements { get; } = new List<string>();
            public List<string> Parameters { get; } = new List<string>();

            protected override void UnsafeClear()
            {
                base.UnsafeClear();

                SqlStatements.Clear();
                Parameters.Clear();
            }

            protected override void UnsafeLog<TState>(
                LogLevel logLevel, EventId eventId, string message, TState state, Exception exception)
            {
                if ((eventId.Id == RelationalEventId.CommandExecuted.Id
                    || eventId.Id == RelationalEventId.CommandError.Id
                    || eventId.Id == RelationalEventId.CommandExecuting.Id))
                {
                    if (_shouldLogCommands)
                    {
                        base.UnsafeLog(logLevel, eventId, message, state, exception);
                    }

                    if (message != null
                        && eventId.Id != RelationalEventId.CommandExecuting.Id)
                    {
                        var structure = (IReadOnlyList<KeyValuePair<string, object>>)state;

                        var parameters = structure.Where(i => i.Key == "parameters").Select(i => (string)i.Value).First();
                        var commandText = structure.Where(i => i.Key == "commandText").Select(i => (string)i.Value).First();

                        if (!string.IsNullOrWhiteSpace(parameters))
                        {
                            Parameters.Add(parameters);
                            parameters = parameters.Replace(", ", _eol) + _eol + _eol;
                        }

                        SqlStatements.Add(parameters + commandText);
                    }
                }
                else
                {
                    base.UnsafeLog(logLevel, eventId, message, state, exception);
                }
            }
        }
    }
}
