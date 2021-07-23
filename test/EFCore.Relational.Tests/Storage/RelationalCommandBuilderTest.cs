﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalCommandBuilderTest
    {
        [ConditionalFact]
        public void Builds_simple_command()
        {
            var commandBuilder = CreateCommandBuilder();

            var command = commandBuilder.Build();

            Assert.Equal("", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [ConditionalFact]
        public void Build_command_with_parameter()
        {
            var commandBuilder = CreateCommandBuilder();

            commandBuilder.AddParameter(
                "InvariantName",
                "Name",
                new StringTypeMapping("nvarchar(100)"),
                nullable: true);

            var command = commandBuilder.Build();

            Assert.Equal("", command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal("InvariantName", command.Parameters[0].InvariantName);
        }

        private static RelationalCommandBuilder CreateCommandBuilder()
        {
            var dependencies = new RelationalCommandBuilderDependencies(
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

            var commandBuilder = new RelationalCommandBuilder(dependencies);
            return commandBuilder;
        }
    }
}
