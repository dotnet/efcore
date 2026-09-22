// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

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
        => new(
            new RelationalCommandBuilderFactory(
                new RelationalCommandBuilderDependencies(
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                    new ExceptionDetector())),
            new RelationalSqlGenerationHelper(
                new RelationalSqlGenerationHelperDependencies()),
            new ParameterNameGeneratorFactory(
                new ParameterNameGeneratorDependencies()),
            new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

    [ConditionalFact]
    public virtual void Builds_RelationalCommand_with_empty_parameter_list()
    {
        var builder = CreateBuilder();

        var rawSqlCommand = builder.Build("SQL COMMAND TEXT", []);

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
