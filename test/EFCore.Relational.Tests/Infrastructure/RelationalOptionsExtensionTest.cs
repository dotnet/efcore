// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class RelationalOptionsExtensionTest
{
    private const string ConnectionString = "Fraggle=Rock";

    [ConditionalFact]
    public void Can_set_Connection()
    {
        var optionsExtension = new FakeRelationalOptionsExtension();

        Assert.Null(optionsExtension.Connection);

        var connection = new FakeDbConnection("A=B");
        optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnection(connection);

        Assert.Same(connection, optionsExtension.Connection);
        Assert.False(optionsExtension.IsConnectionOwned);
    }

    [ConditionalFact]
    public void Can_set_owned_Connection()
    {
        var optionsExtension = new FakeRelationalOptionsExtension();

        Assert.Null(optionsExtension.Connection);

        var connection = new FakeDbConnection("A=B");
        optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnection(connection, owned: true);

        Assert.Same(connection, optionsExtension.Connection);
        Assert.True(optionsExtension.IsConnectionOwned);
    }

    [ConditionalFact]
    public void Can_set_ConnectionString()
    {
        var optionsExtension = new FakeRelationalOptionsExtension();

        Assert.Null(optionsExtension.ConnectionString);

        optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnectionString(ConnectionString);

        Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
    }

    [ConditionalFact]
    public void Can_set_CommandTimeout()
    {
        var optionsExtension = new FakeRelationalOptionsExtension();

        Assert.Null(optionsExtension.CommandTimeout);

        optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithCommandTimeout(1);

        Assert.Equal(1, optionsExtension.CommandTimeout);
    }

    [ConditionalFact]
    public void Throws_if_CommandTimeout_out_of_range()
        => Assert.Equal(
            RelationalStrings.InvalidCommandTimeout(-1),
            Assert.Throws<InvalidOperationException>(
                () => new FakeRelationalOptionsExtension().WithCommandTimeout(-1)).Message);

    [ConditionalFact]
    public void Can_set_MaxBatchSize()
    {
        var optionsExtension = new FakeRelationalOptionsExtension();

        Assert.Null(optionsExtension.MaxBatchSize);

        optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithMaxBatchSize(1);

        Assert.Equal(1, optionsExtension.MaxBatchSize);
    }

    [ConditionalFact]
    public void Throws_if_MaxBatchSize_out_of_range()
        => Assert.Equal(
            RelationalStrings.InvalidMaxBatchSize(-1),
            Assert.Throws<InvalidOperationException>(
                () => new FakeRelationalOptionsExtension().WithMaxBatchSize(-1)).Message);

    [ConditionalFact]
    public void Throws_if_MinBatchSize_out_of_range()
        => Assert.Equal(
            RelationalStrings.InvalidMinBatchSize(-1),
            Assert.Throws<InvalidOperationException>(
                () => new FakeRelationalOptionsExtension().WithMinBatchSize(-1)).Message);
}
