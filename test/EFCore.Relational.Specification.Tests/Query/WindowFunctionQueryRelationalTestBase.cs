// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Tests for aggregates projected alongside the elements of a grouping, which are translated as window functions when
///     <c>UseWindowFunctionAggregates</c> is configured. See <see href="https://github.com/dotnet/efcore/issues/12747" />.
/// </summary>
public abstract class WindowFunctionQueryRelationalTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "WindowFunctionQueryTests";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    /// <summary>
    ///     Enables the window function translation on the given options, using the provider's own options builder.
    /// </summary>
    protected abstract DbContextOptionsBuilder UseWindowFunctionAggregates(DbContextOptionsBuilder optionsBuilder);

    private Task<ContextFactory<WindowFunctionContext>> InitializeAsync(bool useWindowFunctionAggregates)
        => InitializeNonSharedTest<WindowFunctionContext>(
            seed: c => c.SeedAsync(),
            onConfiguring: o =>
            {
                if (useWindowFunctionAggregates)
                {
                    UseWindowFunctionAggregates(o);
                }
            });

    [Fact]
    public virtual async Task Elements_and_aggregate()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { Employees = g.ToList(), AverageSalary = g.Average(e => e.Salary) })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal(["Alice", "Bob", "Cheng"], engineering.Employees.Select(e => e.Name).Order());
                Assert.Equal(200, engineering.AverageSalary);
            },
            sales =>
            {
                Assert.Equal(["Dara", "Emre"], sales.Employees.Select(e => e.Name).Order());
                Assert.Equal(75, sales.AverageSalary);
            });
    }

    [Fact]
    public virtual async Task Elements_and_multiple_aggregates()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(
                g => new
                {
                    g.Key,
                    Employees = g.ToList(),
                    Count = g.Count(),
                    Total = g.Sum(e => e.Salary),
                    Highest = g.Max(e => e.Salary)
                })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal("Engineering", engineering.Key);
                Assert.Equal(3, engineering.Employees.Count);
                Assert.Equal(3, engineering.Count);
                Assert.Equal(600, engineering.Total);
                Assert.Equal(300, engineering.Highest);
            },
            sales =>
            {
                Assert.Equal("Sales", sales.Key);
                Assert.Equal(2, sales.Employees.Count);
                Assert.Equal(2, sales.Count);
                Assert.Equal(150, sales.Total);
                Assert.Equal(100, sales.Highest);
            });
    }

    /// <summary>
    ///     LongCount, which providers may translate to a function of their own (COUNT_BIG on SQL Server) rather than to COUNT.
    /// </summary>
    [Fact]
    public virtual async Task Elements_and_long_count()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Names = g.Select(e => e.Name).ToList(), Count = g.LongCount() })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal("Engineering", engineering.Key);
                Assert.Equal(["Alice", "Bob", "Cheng"], engineering.Names.Order());
                Assert.Equal(3L, engineering.Count);
            },
            sales =>
            {
                Assert.Equal("Sales", sales.Key);
                Assert.Equal(["Dara", "Emre"], sales.Names.Order());
                Assert.Equal(2L, sales.Count);
            });
    }

    /// <summary>
    ///     Keys which differ only in case are one group under a case-insensitive collation, and two under a case-sensitive one. Either
    ///     way the groupings, and the window values they carry, have to agree with the database's grouping rather than with CLR string
    ///     equality.
    /// </summary>
    [Fact]
    public virtual async Task Elements_and_aggregate_with_keys_differing_in_case()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();
        context.Employees.Add(new Employee { Name = "Fen", DepartmentName = "engineering", Salary = 400 });
        await context.SaveChangesAsync();

        var databaseGroups = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => g.Count())
            .ToListAsync();

        ClearLog();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { Names = g.Select(e => e.Name).ToList(), Count = g.Count() })
            .ToListAsync();

        Assert.Equal(databaseGroups.Order(), results.Select(r => r.Names.Count).Order());
        Assert.All(results, r => Assert.Equal(r.Names.Count, r.Count));
    }

    /// <summary>
    ///     An aggregate whose translation wraps the function rather than being one - a float sum is a CAST around SUM on SQL Server - can't
    ///     carry an OVER clause, so it keeps the GROUP BY translation.
    /// </summary>
    [Fact]
    public virtual async Task Elements_and_float_sum()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Names = g.Select(e => e.Name).ToList(), Total = g.Sum(e => (float)e.Salary) })
            .ToListAsync();

        Assert.Collection(
            results.OrderBy(r => r.Key),
            engineering =>
            {
                Assert.Equal(["Alice", "Bob", "Cheng"], engineering.Names.Order());
                Assert.Equal(600f, engineering.Total);
            },
            sales =>
            {
                Assert.Equal(["Dara", "Emre"], sales.Names.Order());
                Assert.Equal(150f, sales.Total);
            });
    }

    [Fact]
    public virtual async Task Projected_elements_and_aggregate()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Names = g.Select(e => e.Name).ToList(), Total = g.Sum(e => e.Salary) })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal(["Alice", "Bob", "Cheng"], engineering.Names.Order());
                Assert.Equal(600, engineering.Total);
            },
            sales =>
            {
                Assert.Equal(["Dara", "Emre"], sales.Names.Order());
                Assert.Equal(150, sales.Total);
            });
    }

    /// <summary>
    ///     An aggregate is null for a partition in which every value is, exactly as it is for a group in which every value is - the
    ///     window neither adds rows to the partition nor takes any away.
    /// </summary>
    [Fact]
    public virtual async Task Nullable_aggregate_over_partition_without_values()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Employees = g.ToList(), AverageBonus = g.Average(e => e.Bonus) })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering => Assert.Null(engineering.AverageBonus),
            sales => Assert.Equal(10, sales.AverageBonus));
    }

    /// <summary>
    ///     A sum is zero for a partition in which every value is null, exactly as it is for a group in which every value is: a partition
    ///     always holding the current row doesn't keep SUM from returning null over it.
    /// </summary>
    [Fact]
    public virtual async Task Sum_over_partition_without_values()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Names = g.Select(e => e.Name).ToList(), TotalBonus = g.Sum(e => e.Bonus) })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering => Assert.Equal(0, engineering.TotalBonus),
            sales => Assert.Equal(10, sales.TotalBonus));
    }

    /// <summary>
    ///     Aggregates over decimals, which some providers translate to functions of their own rather than to the built-in aggregates.
    /// </summary>
    [Fact]
    public virtual async Task Decimal_aggregates()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(
                g => new
                {
                    g.Key,
                    Names = g.Select(e => e.Name).ToList(),
                    Total = g.Sum(e => (decimal)e.Salary),
                    Average = g.Average(e => (decimal)e.Salary),
                    Highest = g.Max(e => (decimal)e.Salary)
                })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal(600m, engineering.Total);
                Assert.Equal(200m, engineering.Average);
                Assert.Equal(300m, engineering.Highest);
            },
            sales =>
            {
                Assert.Equal(150m, sales.Total);
                Assert.Equal(75m, sales.Average);
                Assert.Equal(100m, sales.Highest);
            });
    }

    /// <summary>
    ///     An aggregate projected more than once is computed once.
    /// </summary>
    [Fact]
    public virtual async Task Repeated_aggregate()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(
                g => new
                {
                    g.Key,
                    Names = g.Select(e => e.Name).ToList(),
                    Total = g.Sum(e => e.Salary),
                    Summary = new { Total = g.Sum(e => e.Salary), Count = g.Count() }
                })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal(600, engineering.Total);
                Assert.Equal(600, engineering.Summary.Total);
                Assert.Equal(3, engineering.Summary.Count);
            },
            sales =>
            {
                Assert.Equal(150, sales.Total);
                Assert.Equal(150, sales.Summary.Total);
                Assert.Equal(2, sales.Summary.Count);
            });
    }

    /// <summary>
    ///     Grouping by a constant is the workaround for a window over the whole result set rather than over a partition of it.
    /// </summary>
    [Fact]
    public virtual async Task Constant_key()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => 1)
            .Select(g => new { Employees = g.ToList(), AverageSalary = g.Average(e => e.Salary) })
            .ToListAsync();

        var all = Assert.Single(results);
        Assert.Equal(5, all.Employees.Count);
        Assert.Equal(150, all.AverageSalary);
    }

    /// <summary>
    ///     A filter applied before the grouping needs no duplicating: the window is evaluated over the rows the query already left.
    /// </summary>
    [Fact]
    public virtual async Task Filter_before_grouping()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .Where(e => e.Salary > 50)
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Employees = g.ToList(), AverageSalary = g.Average(e => e.Salary) })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal(3, engineering.Employees.Count);
                Assert.Equal(200, engineering.AverageSalary);
            },
            sales =>
            {
                Assert.Equal(1, sales.Employees.Count);
                Assert.Equal(100, sales.AverageSalary);
            });
    }

    /// <summary>
    ///     Without the opt-in, the same query as <see cref="Elements_and_aggregate" /> keeps the GROUP BY translation it has today.
    /// </summary>
    [Fact]
    public virtual async Task Elements_and_aggregate_without_opt_in()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: false);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { Employees = g.ToList(), AverageSalary = g.Average(e => e.Salary) })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering =>
            {
                Assert.Equal(["Alice", "Bob", "Cheng"], engineering.Employees.Select(e => e.Name).Order());
                Assert.Equal(200, engineering.AverageSalary);
            },
            sales =>
            {
                Assert.Equal(["Dara", "Emre"], sales.Employees.Select(e => e.Name).Order());
                Assert.Equal(75, sales.AverageSalary);
            });
    }

    /// <summary>
    ///     An aggregate with no elements projected alongside it stays a plain GROUP BY: there are no rows to repeat the value on, and
    ///     collapsing them is exactly what the query asked for.
    /// </summary>
    [Fact]
    public virtual async Task Aggregate_without_elements()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, AverageSalary = g.Average(e => e.Salary) })
            .OrderBy(x => x.Key)
            .ToListAsync();

        Assert.Collection(
            results,
            engineering => Assert.Equal(200, engineering.AverageSalary),
            sales => Assert.Equal(75, sales.AverageSalary));
    }

    /// <summary>
    ///     A projection of the elements alone is lifted as it is today, with no window function involved.
    /// </summary>
    [Fact]
    public virtual async Task Elements_without_aggregate()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Employees = g.ToList() })
            .ToListAsync();

        Assert.Collection(
            results,
            engineering => Assert.Equal(3, engineering.Employees.Count),
            sales => Assert.Equal(2, sales.Employees.Count));
    }

    /// <summary>
    ///     COUNT(DISTINCT ...) OVER (...) isn't supported by SQL Server, so a distinct aggregate keeps the GROUP BY translation.
    /// </summary>
    [Fact]
    public virtual async Task Distinct_aggregate_is_not_lifted()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Employees = g.ToList(), Salaries = g.Select(e => e.Salary).Distinct().Count() })
            .ToListAsync();

        Assert.Collection(
            results.OrderBy(r => r.Key),
            engineering => Assert.Equal(3, engineering.Salaries),
            sales => Assert.Equal(2, sales.Salaries));
    }

    /// <summary>
    ///     A filtered aggregate composes over rows other than the partition's, so it too keeps the GROUP BY translation.
    /// </summary>
    [Fact]
    public virtual async Task Filtered_aggregate_is_not_lifted()
    {
        var contextFactory = await InitializeAsync(useWindowFunctionAggregates: true);
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new { g.Key, Employees = g.ToList(), HighEarners = g.Where(e => e.Salary > 100).Count() })
            .ToListAsync();

        Assert.Collection(
            results.OrderBy(r => r.Key),
            engineering => Assert.Equal(2, engineering.HighEarners),
            sales => Assert.Equal(0, sales.HighEarners));
    }

    protected class WindowFunctionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Employee> Employees
            => Set<Employee>();

        public Task SeedAsync()
        {
            AddRange(
                new Employee { Name = "Alice", DepartmentName = "Engineering", Salary = 100 },
                new Employee { Name = "Bob", DepartmentName = "Engineering", Salary = 200 },
                new Employee { Name = "Cheng", DepartmentName = "Engineering", Salary = 300 },
                new Employee { Name = "Dara", DepartmentName = "Sales", Salary = 50, Bonus = 10 },
                new Employee { Name = "Emre", DepartmentName = "Sales", Salary = 100 });

            return SaveChangesAsync();
        }
    }

    protected class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public int Salary { get; set; }
        public int? Bonus { get; set; }
    }
}
