// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class QueryLoggingCosmosTestBase
{
    protected QueryLoggingCosmosTestBase(NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected NorthwindQueryCosmosFixture<NoopModelCustomizer> Fixture { get; }

    protected virtual bool ExpectSensitiveData
        => true;

    [ConditionalFact]
    public virtual async Task Queryable_simple()
    {
        using var context = CreateContext();
        var customers = await context.Set<Customer>().ToListAsync();

        Assert.NotNull(customers);

        Assert.StartsWith(
            "Compiling query expression: ",
            Fixture.TestSqlLoggerFactory.Log[0].Message);

        Assert.StartsWith(
            "Generated query execution expression: " + Environment.NewLine + "'queryContext => new QueryingEnumerable<Customer>(",
            Fixture.TestSqlLoggerFactory.Log[1].Message);

        if (ExpectSensitiveData)
        {
            Assert.Equal(
                CosmosResources.LogExecutingSqlQuery(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(
                    "NorthwindContext", "(null)", "", Environment.NewLine,
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
"""),
                Fixture.TestSqlLoggerFactory.Log[2].Message);
        }
        else
        {
            Assert.Equal(
                CosmosResources.LogExecutingSqlQuery(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(
                    "NorthwindContext", "?", "", Environment.NewLine,
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
"""),
                Fixture.TestSqlLoggerFactory.Log[2].Message);
        }
    }

    [ConditionalFact]
    public virtual async Task Queryable_with_parameter_outputs_parameter_value_logging_warning()
    {
        using var context = CreateContext();
        context.GetInfrastructure().GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Query>>()
            .Options.IsSensitiveDataLoggingWarned = false;
        // ReSharper disable once ConvertToConstant.Local
        var city = "Redmond";

        var customers = await context.Customers.Where(c => c.City == city).ToListAsync();

        Assert.NotNull(customers);

        if (ExpectSensitiveData)
        {
            Assert.Contains(
                CoreResources.LogSensitiveDataLoggingEnabled(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        if (ExpectSensitiveData)
        {
            Assert.Equal(
                CosmosResources.LogExecutingSqlQuery(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(
                    "NorthwindContext", "(null)", "@__city_0='Redmond'", Environment.NewLine,
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_0))
"""),
                Fixture.TestSqlLoggerFactory.Log[3].Message);
        }
        else
        {
            Assert.Equal(
                CosmosResources.LogExecutingSqlQuery(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(
                    "NorthwindContext", "?", "@__city_0=?", Environment.NewLine,
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_0))
"""),
                Fixture.TestSqlLoggerFactory.Log[2].Message);
        }
    }

    [ConditionalFact]
    public virtual async Task Skip_without_order_by()
    {
        using var context = CreateContext();
        var customers = await context.Set<Customer>().Skip(85).Take(5).ToListAsync();

        Assert.NotNull(customers);

        Assert.Equal(
            CoreResources.LogRowLimitingOperationWithoutOrderBy(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
            Fixture.TestSqlLoggerFactory.Log[1].Message);
    }

    [ConditionalFact]
    public virtual async Task Take_without_order_by()
    {
        using var context = CreateContext();
        var customers = await context.Set<Customer>().Take(5).ToListAsync();

        Assert.NotNull(customers);

        Assert.Equal(
            CoreResources.LogRowLimitingOperationWithoutOrderBy(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
            Fixture.TestSqlLoggerFactory.Log[1].Message);
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
