// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public IReadOnlyList<string> SqlStatements
            => ((TestSqlLogger)Logger).SqlStatements;

        public IReadOnlyList<string> Parameters
            => ((TestSqlLogger)Logger).Parameters;

        public string Sql
            => string.Join(_eol + _eol, SqlStatements);

        public void AssertBaseline(string[] expected, bool assertOrder = true)
        {
            if (_proceduralQueryGeneration)
            {
                return;
            }

            try
            {
                if (assertOrder)
                {
                    for (var i = 0; i < expected.Length; i++)
                    {
                        Assert.Equal(expected[i], SqlStatements[i], ignoreLineEndingDifferences: true);
                    }

                    Assert.Empty(SqlStatements.Skip(expected.Length));
                }
                else
                {
                    foreach (var expectedFragment in expected)
                    {
                        var normalizedExpectedFragment = NormalizeLineEndings(expectedFragment);
                        Assert.Contains(
                            normalizedExpectedFragment,
                            SqlStatements);
                    }
                }
            }
            catch
            {
                var methodCallLine = Environment.StackTrace.Split(
                    new[] { _eol },
                    StringSplitOptions.RemoveEmptyEntries)[3].Substring(6);

                var indexMethodEnding = methodCallLine.IndexOf(')') + 1;
                var testName = methodCallLine.Substring(0, indexMethodEnding);
                var parts = methodCallLine[indexMethodEnding..].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var fileName = parts[1][..^5];
                var lineNumber = int.Parse(parts[2]);

                var currentDirectory = Directory.GetCurrentDirectory();
                var logFile = currentDirectory.Substring(
                        0,
                        currentDirectory.LastIndexOf("\\artifacts\\", StringComparison.Ordinal) + 1)
                    + "QueryBaseline.txt";

                var testInfo = testName + " : " + lineNumber + FileNewLine;
                const string indent = FileNewLine + "                ";

                var newBaseLine = $@"            AssertSql(
                {string.Join("," + indent + "//" + indent, SqlStatements.Take(9).Select(sql => "@\"" + sql.Replace("\"", "\"\"") + "\""))});

";

                if (SqlStatements.Count > 9)
                {
                    newBaseLine += "Output truncated.";
                }

                Logger.TestOutputHelper?.WriteLine("---- New Baseline -------------------------------------------------------------------");
                Logger.TestOutputHelper?.WriteLine(newBaseLine);

                var contents = testInfo + newBaseLine + FileNewLine + "--------------------" + FileNewLine;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }

        protected class TestSqlLogger : ListLogger
        {
            private readonly bool _shouldLogCommands;

            public TestSqlLogger(bool shouldLogCommands)
                => _shouldLogCommands = shouldLogCommands;

            public List<string> SqlStatements { get; } = new();
            public List<string> Parameters { get; } = new();

            private StringBuilder _stringBuilder = new();

            protected override void UnsafeClear()
            {
                base.UnsafeClear();

                SqlStatements.Clear();
                Parameters.Clear();
            }

            protected override void UnsafeLog<TState>(
                LogLevel logLevel,
                EventId eventId,
                string message,
                TState state,
                Exception exception)
            {
                if ((eventId.Id == RelationalEventId.CommandExecuted.Id
                    || eventId.Id == RelationalEventId.CommandError.Id
                    || eventId.Id == RelationalEventId.CommandExecuting.Id))
                {
                    if (_shouldLogCommands)
                    {
                        base.UnsafeLog(logLevel, eventId, message, state, exception);
                    }

                    if (!IsRecordingSuspended
                        && message != null
                        && eventId.Id != RelationalEventId.CommandExecuting.Id)
                    {
                        var structure = (IReadOnlyList<KeyValuePair<string, object>>)state;

                        var parameters = structure.Where(i => i.Key == "parameters").Select(i => (string)i.Value).First();
                        var commandText = structure.Where(i => i.Key == "commandText").Select(i => (string)i.Value).First();

                        if (!string.IsNullOrWhiteSpace(parameters))
                        {
                            Parameters.Add(parameters);

                            _stringBuilder.Clear();

                            var inQuotes = false;
                            var inCurlies = false;
                            for (var i = 0; i < parameters.Length; i++)
                            {
                                var c = parameters[i];
                                switch (c)
                                {
                                    case '\'':
                                        inQuotes = !inQuotes;
                                        goto default;
                                    case '{':
                                        inCurlies = true;
                                        goto default;
                                    case '}':
                                        inCurlies = false;
                                        goto default;
                                    case ',' when parameters[i + 1] == ' ' && !inQuotes && !inCurlies:
                                        _stringBuilder.Append(_eol);
                                        i++;
                                        continue;
                                    default:
                                        _stringBuilder.Append(c);
                                        continue;
                                }
                            }

                            _stringBuilder.Append(_eol).Append(_eol);
                            parameters = _stringBuilder.ToString();
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
