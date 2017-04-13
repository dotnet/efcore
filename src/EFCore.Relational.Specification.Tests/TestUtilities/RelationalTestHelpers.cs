// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public abstract class RelationalTestHelpers : TestHelpers
    {
        private const string FileLineEnding = @"
";

        public static void AssertBaseline(ITestOutputHelper testOutputHelper, bool assertOrder, params string[] expected)
        {
            var sqlStatements = TestSqlLoggerFactory.SqlStatements
                .Select(sql => sql.Replace(Environment.NewLine, FileLineEnding))
                .ToList();

            try
            {
                if (assertOrder)
                {
                    for (var i = 0; i < expected.Length; i++)
                    {
                        Assert.Equal(expected[i], sqlStatements[i]);
                    }
                }
                else
                {
                    foreach (var expectedFragment in expected)
                    {
                        Assert.Contains(expectedFragment, sqlStatements);
                    }
                }
            }
            catch
            {
                var methodCallLine = Environment.StackTrace.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries)[4]
                    .Substring(6);
                var testName = methodCallLine.Substring(0, methodCallLine.IndexOf(')') + 1);
                var lineIndex = methodCallLine.LastIndexOf("line", StringComparison.Ordinal);
                var lineNumber = lineIndex> 0 ? methodCallLine.Substring(lineIndex) : "";
                var indent = FileLineEnding + "                ";

                var currentDirectory = Directory.GetCurrentDirectory();
                var logFile = currentDirectory.Substring(
                                  0,
                                  currentDirectory.LastIndexOf("\\test\\", StringComparison.Ordinal) + 1)
                              + "QueryBaseline.cs";

                var testInfo = $"{testName + " : " + lineNumber}" + FileLineEnding;

                var newBaseLine = $@"            AssertSql(
                {string.Join("," + indent + "//" + indent, sqlStatements.Take(9).Select(sql => "@\"" + sql.Replace("\"","\"\"") + "\""))});

";

                if (sqlStatements.Count > 9)
                {
                    newBaseLine += "Output truncated.";
                }

                testOutputHelper.WriteLine(newBaseLine);

                var contents = testInfo + newBaseLine + FileLineEnding + FileLineEnding;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }
    }
}