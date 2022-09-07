// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

// ReSharper disable MethodHasAsyncOverload

namespace Microsoft.EntityFrameworkCore.Storage;

public class RelationalDataReaderTest
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Does_not_hold_reference_to_DbDataReader_after_dispose(bool async)
    {
        var fakeConnection = CreateConnection();
        var relationalCommand = CreateRelationalCommand(commandText: "CommandText");

        var reader = relationalCommand.ExecuteReader(new(
            fakeConnection,
            new Dictionary<string, object>(),
            readerColumns: null,
            context: null,
            logger: null));

        Assert.NotNull(reader.DbDataReader);

        if (async)
        {
            await reader.DisposeAsync();
        }
        else
        {
            reader.Dispose();
        }

        Assert.Null(reader.DbDataReader);
    }

    private const string ConnectionString = "Fake Connection String";

    private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
        => new(options ?? CreateOptions());

    private static IDbContextOptions CreateOptions(
        RelationalOptionsExtension optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                optionsExtension
                ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }

    private IRelationalCommand CreateRelationalCommand(
        string commandText = "Command Text",
        IReadOnlyList<IRelationalParameter> parameters = null)
        => new RelationalCommand(
            new RelationalCommandBuilderDependencies(
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())),
            commandText,
            parameters ?? Array.Empty<IRelationalParameter>());

    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };
}
