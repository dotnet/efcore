// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Design
{
    public class SqliteDmlParserTest
    {
        public static object[][] Statements =
        {
            new object[] { "CREATE TABLE t (a,b,d);", new[] { "a", "b", "d" } },
            new object[] { "CREATE TABLE t ( \"col,name\" text, col2);", new[] { "\"col,name\" text", "col2" } },
            new object[] { "CREATE TABLE t ( \", \"\"col,name\" text, col2);", new[] { "\", \"\"col,name\" text", "col2" } },
            new object[] { "CREATE TABLE t (Int decimal(10,3), col2);", new[] { "Int decimal(10,3)", "col2" } },
            new object[] { "CREATE TABLE t (a string, foreign key (a) references b (\"a)\") );", new[] { "a string", "foreign key (a) references b (\"a)\")" } },
            new object[]
            {
                "CREATE TABLE t ( Int decimal(10,3) default (IF(1 + (3) = 3, 10.2, NULL)) , col2);",
                new[] { "Int decimal(10,3) default (IF(1 + (3) = 3, 10.2, NULL))", "col2" }
            },
            new object[] { "CREATE TABLE '(' ( A,B)", new object[] { "A", "B" } }
        };

        [Theory]
        [MemberData(nameof(Statements))]
        public void It_extracts_statements(string sql, string[] statements)
        {
            Assert.Equal(statements, SqliteDmlParser.ParseStatements(sql).ToArray());
        }

        [Theory]
        [InlineData("'Escaped''single'", "Escaped'single")]
        [InlineData("\"Escaped\"\"single\"", "Escaped\"single")]
        [InlineData("\"No ending quote", "\"No ending quote")]
        [InlineData("No open quote'", "No open quote'")]
        [InlineData("'''quoted'''", "'quoted'")]
        public void It_unescapes_strings(string input, string result)
        {
            Assert.Equal(result, SqliteDmlParser.UnescapeString(input));
        }

        [Theory]
        [InlineData(',', "a,b,c", new[] { "a", "b", "c" })]
        [InlineData(',', "a',' ,b,c", new[] { "a',' ", "b", "c" })]
        [InlineData(',', "(a')' ,b),c", new[] { "(a')' ,b)", "c" })]
        [InlineData(',', "\",,,\",\",,,\"", new[] { "\",,,\"", "\",,,\"" })]
        [InlineData(',', "\",',\",\",',\"", new[] { "\",',\"", "\",',\"" })]
        [InlineData(',', @"`~!@#$%^&*()+=-[];'',.<>/?|\", new[] { @"`~!@#$%^&*()+=-[];''", @".<>/?|\" })]
        [InlineData(' ', @"CREATE TABLE '`~!@#$%^&*()+=-[];''"",.<>/?|\ ' ", new[] { "CREATE", "TABLE", @"'`~!@#$%^&*()+=-[];''"",.<>/?|\ '" })]
        public void It_safely_splits(char sep, string input, string[] results)
        {
            Assert.Equal(results, SqliteDmlParser.SafeSplit(input, sep));
        }
    }
}
