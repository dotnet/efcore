// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalCommandBuilderTest
    {
        [Fact]
        public void Builds_simple_command()
        {
            var commandBuilder = new RelationalCommandBuilder(
                new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

            var command = commandBuilder.Build();

            Assert.Equal("", command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public void Build_command_with_parameter()
        {
            var commandBuilder = new RelationalCommandBuilder(
                new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

            commandBuilder.ParameterBuilder.AddParameter(
                "InvariantName",
                "Name",
                new StringTypeMapping("nvarchar(100)"),
                nullable: true);

            var command = commandBuilder.Build();

            Assert.Equal("", command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal("InvariantName", command.Parameters[0].InvariantName);
        }
    }
}
