// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RawSqlCommandBuilderTest
    {
        [ConditionalFact]
        public virtual void Builds_RelationalCommand_without_optional_parameters()
        {
            var builder = CreateBuilder();

            var command = builder.Build("SQL COMMAND TEXT");

            Assert.Equal("SQL COMMAND TEXT", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        private static RawSqlCommandBuilder CreateBuilder()
        {
            return new RawSqlCommandBuilder(
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        new TestRelationalTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))),
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new ParameterNameGeneratorFactory(
                    new ParameterNameGeneratorDependencies()));
        }

        [ConditionalFact]
        public virtual void Builds_RelationalCommand_with_empty_parameter_list()
        {
            var builder = CreateBuilder();

            var rawSqlCommand = builder.Build("SQL COMMAND TEXT", Array.Empty<object>());

            Assert.Equal("SQL COMMAND TEXT", rawSqlCommand.RelationalCommand.CommandText);
            Assert.Equal(0, rawSqlCommand.RelationalCommand.Parameters.Count);
            Assert.Equal(0, rawSqlCommand.ParameterValues.Count);
        }

        [ConditionalFact]
        public virtual void Builds_RelationalCommand_with_parameters()
        {
            var builder = CreateBuilder();

            var rawSqlCommand = builder.Build("SQL COMMAND TEXT {0} {1} {2}", new object[] { 1, 2L, "three" });

            Assert.Equal("SQL COMMAND TEXT @p0 @p1 @p2", rawSqlCommand.RelationalCommand.CommandText);
            Assert.Equal(3, rawSqlCommand.RelationalCommand.Parameters.Count);
            Assert.Equal("p0", rawSqlCommand.RelationalCommand.Parameters[0].InvariantName);
            Assert.Equal("p1", rawSqlCommand.RelationalCommand.Parameters[1].InvariantName);
            Assert.Equal("p2", rawSqlCommand.RelationalCommand.Parameters[2].InvariantName);

            Assert.Equal(3, rawSqlCommand.ParameterValues.Count);
            Assert.Equal(1, rawSqlCommand.ParameterValues["p0"]);
            Assert.Equal(2L, rawSqlCommand.ParameterValues["p1"]);
            Assert.Equal("three", rawSqlCommand.ParameterValues["p2"]);
        }
    }
}
