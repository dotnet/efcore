// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class SqlExecutorTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<SqlExecutorModelCustomizer>, new()
{
    protected SqlExecutorTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Executes_stored_procedure(bool async)
    {
        using var context = CreateContext();

        Assert.Equal(
            -1,
            async
                ? await context.Database.ExecuteSqlRawAsync(TenMostExpensiveProductsSproc)
                : context.Database.ExecuteSqlRaw(TenMostExpensiveProductsSproc));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Executes_stored_procedure_with_parameter(bool async)
    {
        using var context = CreateContext();
        var parameter = CreateDbParameter("@CustomerID", "ALFKI");

        Assert.Equal(
            -1, async
                ? await context.Database.ExecuteSqlRawAsync(CustomerOrderHistorySproc, parameter)
                : context.Database.ExecuteSqlRaw(CustomerOrderHistorySproc, parameter));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Executes_stored_procedure_with_generated_parameter(bool async)
    {
        using var context = CreateContext();

        Assert.Equal(
            -1,
            async
                ? await context.Database.ExecuteSqlRawAsync(CustomerOrderHistoryWithGeneratedParameterSproc, "ALFKI")
                : context.Database.ExecuteSqlRaw(CustomerOrderHistoryWithGeneratedParameterSproc, "ALFKI"));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Throws_on_concurrent_command(bool async)
    {
        using var context = CreateContext();
        context.Database.EnsureCreatedResiliently();

        using var synchronizationEvent = new ManualResetEventSlim(false);
        using var blockingSemaphore = new SemaphoreSlim(0);
        var blockingTask = Task.Run(
            () =>
                context.Customers.Select(
                    c => Process(c, synchronizationEvent, blockingSemaphore)).ToList());

        if (async)
        {
            var throwingTask = Task.Run(
                async () =>
                {
                    synchronizationEvent.Wait();
                    Assert.Equal(
                        CoreStrings.ConcurrentMethodInvocation,
                        (await Assert.ThrowsAsync<InvalidOperationException>(
                            () => context.Database.ExecuteSqlRawAsync(@"SELECT * FROM ""Customers"""))).Message);
                });

            await throwingTask;
        }
        else
        {
            var throwingTask = Task.Run(
                () =>
                {
                    synchronizationEvent.Wait();
                    Assert.Equal(
                        CoreStrings.ConcurrentMethodInvocation,
                        Assert.Throws<InvalidOperationException>(
                            () => context.Database.ExecuteSqlRaw(@"SELECT * FROM ""Customers""")).Message);
                });

            throwingTask.Wait();
        }

        blockingSemaphore.Release(1);

        blockingTask.Wait();
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_parameters(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlRawAsync(
                @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = {1}", city, contactTitle)
            : context.Database.ExecuteSqlRaw(
                @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = {1}", city, contactTitle);

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_dbParameter_with_name(bool async)
    {
        var city = CreateDbParameter("@city", "London");

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlRawAsync(@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city", city)
            : context.Database.ExecuteSqlRaw(@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city", city);

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_positional_dbParameter_with_name(bool async)
    {
        var city = CreateDbParameter("@city", "London");

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlRawAsync(@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0}", city)
            : context.Database.ExecuteSqlRaw(@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0}", city);

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_positional_dbParameter_without_name(bool async)
    {
        var city = CreateDbParameter(name: null, value: "London");

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlRawAsync(@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0}", city)
            : context.Database.ExecuteSqlRaw(@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0}", city);

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_dbParameters_mixed(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        var cityParameter = CreateDbParameter("@city", city);
        var contactTitleParameter = CreateDbParameter("@contactTitle", contactTitle);

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlRawAsync(
                @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = @contactTitle", city,
                contactTitleParameter)
            : context.Database.ExecuteSqlRaw(
                @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = @contactTitle", city,
                contactTitleParameter);

        Assert.Equal(-1, actual);

        actual = async
            ? await context.Database.ExecuteSqlRawAsync(
                @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city AND ""ContactTitle"" = {1}", cityParameter, contactTitle)
            : context.Database.ExecuteSqlRaw(
                @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = @city AND ""ContactTitle"" = {1}", cityParameter, contactTitle);

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_parameters_interpolated(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlInterpolatedAsync(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}")
            : context.Database.ExecuteSqlInterpolated(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}");

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_DbParameters_interpolated(bool async)
    {
        var city = CreateDbParameter("city", "London");
        var contactTitle = CreateDbParameter("contactTitle", "Sales Representative");

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlInterpolatedAsync(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}")
            : context.Database.ExecuteSqlInterpolated(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}");

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_parameters_interpolated_2(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlAsync(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}")
            : context.Database.ExecuteSql(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}");

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_DbParameters_interpolated_2(bool async)
    {
        var city = CreateDbParameter("city", "London");
        var contactTitle = CreateDbParameter("contactTitle", "Sales Representative");

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlAsync(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}")
            : context.Database.ExecuteSql(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}");

        Assert.Equal(-1, actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_parameters_custom_converter(bool async)
    {
        var city = new City { Name = "London" };
        var contactTitle = "Sales Representative";

        using var context = CreateContext();

        var actual = async
            ? await context.Database.ExecuteSqlAsync(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}")
            : context.Database.ExecuteSql(
                $@"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {city} AND ""ContactTitle"" = {contactTitle}");

        Assert.Equal(-1, actual);
    }

    private static Customer Process(Customer c, ManualResetEventSlim e, SemaphoreSlim s)
    {
        e.Set();
        s.Wait();
        s.Release(1);
        return c;
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected abstract DbParameter CreateDbParameter(string name, object value);

    protected abstract string TenMostExpensiveProductsSproc { get; }

    protected abstract string CustomerOrderHistorySproc { get; }

    protected abstract string CustomerOrderHistoryWithGeneratedParameterSproc { get; }
}

public class SqlExecutorModelCustomizer : NoopModelCustomizer
{
    public override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.DefaultTypeMapping<City>().HasConversion<CityToStringConverter>();
    }

    private sealed class CityToStringConverter : ValueConverter<City, string>
    {
        public CityToStringConverter()
            : base(value => value.Name, value => new City { Name = value })
        {
        }
    }
}
