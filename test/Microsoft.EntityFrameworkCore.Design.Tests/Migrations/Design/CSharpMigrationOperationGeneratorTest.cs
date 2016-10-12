// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Migrations.Design
{
    public class CSharpMigrationOperationGeneratorTest
    {
        private static readonly string EOL = Environment.NewLine;

        [Fact]
        public void Generate_seperates_operations_by_a_blank_line()
        {
            var generator = new CSharpMigrationOperationGenerator(new CSharpHelper());
            var builder = new IndentedStringBuilder();

            generator.Generate(
                "mb",
                new[]
                {
                    new SqlOperation { Sql = "-- Don't stand so" },
                    new SqlOperation { Sql = "-- close to me" }
                },
                builder);

            Assert.Equal(
                "mb.Sql(\"-- Don't stand so\");" + EOL +
                EOL +
                "mb.Sql(\"-- close to me\");",
                builder.ToString());
        }
    }
}
