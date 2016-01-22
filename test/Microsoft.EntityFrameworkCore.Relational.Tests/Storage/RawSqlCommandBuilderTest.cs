// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RawSqlCommandBuilderTest
    {
        [Fact]
        public virtual void Builds_RelationalCommand_without_optional_parameters()
        {
            var builder = new RawSqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new FakeRelationalTypeMapper()),
                new RelationalSqlGenerationHelper(),
                new ParameterNameGeneratorFactory());

            var command = builder.Build("SQL COMMAND TEXT");

            Assert.Equal("SQL COMMAND TEXT", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public virtual void Builds_RelationalCommand_with_empty_parameter_list()
        {
            var builder = new RawSqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new FakeRelationalTypeMapper()),
                new RelationalSqlGenerationHelper(),
                new ParameterNameGeneratorFactory());

            var command = builder.Build("SQL COMMAND TEXT", new object[0]);

            Assert.Equal("SQL COMMAND TEXT", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public virtual void Builds_RelationalCommand_with_parameters()
        {
            var builder = new RawSqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new FakeRelationalTypeMapper()),
                new RelationalSqlGenerationHelper(),
                new ParameterNameGeneratorFactory());

            var command = builder.Build("SQL COMMAND TEXT {0} {1} {2}", new object[] { 1, 2L, "three" });

            Assert.Equal("SQL COMMAND TEXT @p0 @p1 @p2", command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
        }
    }
}
