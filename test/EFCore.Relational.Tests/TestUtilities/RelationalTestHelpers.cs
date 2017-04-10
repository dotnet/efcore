// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
{
    public class RelationalTestHelpers : TestHelpers
    {
        protected RelationalTestHelpers()
        {
        }

        public static RelationalTestHelpers Instance { get; } = new RelationalTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => FakeRelationalOptionsExtension.AddEntityFrameworkRelationalDatabase(services);

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            var extension = optionsBuilder.Options.FindExtension<FakeRelationalOptionsExtension>()
                            ?? new FakeRelationalOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
                extension.WithConnection(new FakeDbConnection("Database=Fake")));
        }

        private const string FileLineEnding = @"
";

        public static void AssertBaseline(params string[] expected)
        {
            var sqlStatements = TestSqlLoggerFactory.SqlStatements
                .Select(sql => sql.Replace(Environment.NewLine, FileLineEnding))
                .ToList();

            try
            {
                // Contains match because of ordering differences between .NET framework & Core
                foreach (var expectedFragment in expected)
                {
                    Assert.Contains(expectedFragment, sqlStatements);
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

                var contents = $@"{testName + " : " + lineNumber}
            AssertSql(
                {string.Join("," + indent + "//" + indent, sqlStatements.Take(9).Select(sql => "@\"" + sql.Replace("\"","\"\"") + "\""))});

";

                if (sqlStatements.Count > 9)
                {
                    contents += "Output truncated.";
                }

                contents += FileLineEnding + FileLineEnding;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }
    }
}