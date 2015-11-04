// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Diagnostics;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.TestUtilities;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Xunit;

namespace Microsoft.Data.Entity.Storage
{
    public class SqlCommandBuilderTest
    {
        [Fact]
        public virtual void Builds_RelationalCommand_without_optional_parameters()
        {
            var builder = new SqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new FakeRelationalTypeMapper()),
                new RelationalSqlGenerator(),
                new ParameterNameGeneratorFactory());

            var command = builder.Build("SQL COMMAND TEXT");

            Assert.Equal("SQL COMMAND TEXT", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public virtual void Builds_RelationalCommand_with_empty_parameter_list()
        {
            var builder = new SqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new FakeRelationalTypeMapper()),
                new RelationalSqlGenerator(),
                new ParameterNameGeneratorFactory());

            var command = builder.Build("SQL COMMAND TEXT", new object[0]);

            Assert.Equal("SQL COMMAND TEXT", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public virtual void Builds_RelationalCommand_with_parameters()
        {
            var builder = new SqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new FakeRelationalTypeMapper()),
                new RelationalSqlGenerator(),
                new ParameterNameGeneratorFactory());

            var command = builder.Build("SQL COMMAND TEXT {0} {1} {2}", new object[] { 1, 2L, "three" });

            Assert.Equal("SQL COMMAND TEXT @p0 @p1 @p2", command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
        }
    }
}
