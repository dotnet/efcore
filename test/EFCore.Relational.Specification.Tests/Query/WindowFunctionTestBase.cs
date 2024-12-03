// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class WindowFunctionTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : SharedStoreFixtureBase<DbContext>, new()
{
    protected TFixture Fixture { get; }

    protected WindowFunctionTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected WindowFunctionContext CreateContext()
     => (WindowFunctionContext)Fixture.CreateContext();

    #region Model

    public class Employee
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public decimal Salary { get; set; }
        public int WorkExperience { get; set; }
    }

    public class NullTestEmployee
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public decimal? Salary { get; set; }
        public int WorkExperience { get; set; }
    }

    public class WindowFunctionContext : PoolableDbContext
    {
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<NullTestEmployee> NullTestEmployees  => Set<NullTestEmployee>();

        public WindowFunctionContext(DbContextOptions options)
          : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(e =>
            {
                e.Property(e => e.Salary).HasColumnType("decimal(10,2)");
                e.Property(e => e.Name).HasMaxLength(50);
                e.Property(e => e.DepartmentName).HasMaxLength(25);
            });

            modelBuilder.Entity<NullTestEmployee>(e =>
            {
                e.Property(e => e.Salary).HasColumnType("decimal(10,2)");
                e.Property(e => e.Name).HasMaxLength(50);
                e.Property(e => e.DepartmentName).HasMaxLength(25);
            });
        }
    }

    public abstract class WindowFunctionFixture : SharedStoreFixtureBase<DbContext>
    {
        protected override Type ContextType { get; } = typeof(WindowFunctionContext);

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        protected async override Task SeedAsync(DbContext context)
        {
            var ctx = context as WindowFunctionContext;

            context.Database.EnsureCreatedResiliently();

            var emp1 = new Employee
            {
                EmployeeId = 1,
                Name = "Luke Sykwalker",
                Salary = 100000.0m,
                WorkExperience = 10,
                DepartmentName = "IT"
            };

            var emp2 = new Employee
            {
                EmployeeId = 2,
                Name = "Darth Vader",
                Salary = 500000.0m,
                WorkExperience = 15,
                DepartmentName = "Security"
            };

            var emp3 = new Employee
            {
                EmployeeId = 3,
                Name = "Emperor Palpatine",
                Salary = 1000000.53m,
                WorkExperience = 20,
                DepartmentName = "Corporate"
            };

            var emp4 = new Employee
            {
                EmployeeId = 4,
                Name = "Leia Organa",
                Salary = 200000.0m,
                WorkExperience = 12,
                DepartmentName = "IT"
            };

            var emp5 = new Employee
            {
                EmployeeId = 5,
                Name = "Boba Fett",
                Salary = 50000.0m,
                WorkExperience = 4,
                DepartmentName = "Security"
            };

            var emp6 = new Employee
            {
                EmployeeId = 6,
                Name = "Han Solo",
                Salary = 350000.24m,
                WorkExperience = 8,
                DepartmentName = "Sales"
            };

            var emp7 = new Employee
            {
                EmployeeId = 7,
                Name = "Jabba the Hutt",
                Salary = 1750000.0m,
                WorkExperience = 18,
                DepartmentName = "Sales"
            };

            var emp8 = new Employee
            {
                EmployeeId = 5,
                Name = "Commander Cody",
                Salary = 25000.12m,
                WorkExperience = 4,
                DepartmentName = "Security"
            };

            ctx!.Employees.AddRange(emp1, emp2, emp3, emp4, emp5, emp6, emp7, emp8);

            var nullEmp1 = new NullTestEmployee
            {
                EmployeeId = 1,
                Name = "Battle Droid",
                Salary = null,
                WorkExperience = 1,
                DepartmentName = "Security"
            };

            var nullEmp2 = new NullTestEmployee
            {
                EmployeeId = 2,
                Name = "Super Battle Droid",
                Salary = null,
                WorkExperience = 2,
                DepartmentName = "Security"
            };

            ctx.NullTestEmployees.AddRange(nullEmp1, nullEmp2);

            await context.SaveChangesAsync();
        }
    }

    #endregion

    //todo - do tests need order by in the main query for comparing results?  Is the order right now dependent on what the db gives us?
    #region Tests

    #region Window Functions

    #region Max Tests

    [ConditionalFact]
    public virtual void Max_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1750000.0m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Max_Parition_Order_Rows()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.CurrentRow, 5).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Max_Null()
    {
        using var context = CreateContext();

        var results = context.NullTestEmployees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().Max(e.Salary)
        }).ToList();

        Assert.Equal(2, results.Count);
        Assert.Null(results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Max_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Max<decimal?>(e.Salary, () => e.Salary > 100000)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
        Assert.Equal(200000.00m, results[1].MaxSalary);
        Assert.Equal(200000.00m, results[2].MaxSalary);
        Assert.Equal(350000.24m, results[3].MaxSalary);
        Assert.Equal(1750000.00m, results[4].MaxSalary);
        Assert.Null(results[5].MaxSalary);
        Assert.Null(results[6].MaxSalary);
        Assert.Equal(500000.00m, results[7].MaxSalary);
    }

    #endregion

    #region Min Tests

    [ConditionalFact]
    public virtual void Min_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MinSalary = EF.Functions.Over().Min(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(25000.12m, results[0].MinSalary);
    }

    [ConditionalFact]
    public virtual void Min_Null()
    {
        using var context = CreateContext();

        var results = context.NullTestEmployees.Select(e => new
        {
            e.Id,
            e.Name,
            MinSalary = EF.Functions.Over().Min(e.Salary)
        }).ToList();

        Assert.Equal(2, results.Count);
        Assert.Null(results[0].MinSalary);
    }

    [ConditionalFact]
    public virtual void Min_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MinSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Min<decimal?>(e.Salary, () => e.Salary == 200000.00m)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Null(results[0].MinSalary);
        Assert.Equal(200000.00m, results[1].MinSalary);
        Assert.Equal(200000.00m, results[2].MinSalary);
        Assert.Null(results[3].MinSalary);
        Assert.Null(results[4].MinSalary);
        Assert.Null(results[5].MinSalary);
        Assert.Null(results[6].MinSalary);
        Assert.Null(results[7].MinSalary);
    }

    #endregion

    #region Count Tests

    [ConditionalFact]
    public virtual void Count_Star_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().Count()
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(8, results[0].Count);
    }

    [ConditionalFact]
    public virtual void Count_Col_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().Count(e.Id)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(8, results[0].Count);
    }

    [ConditionalFact]
    public virtual void Count_Star_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Count(() => e.Salary <= 1200000.00m)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1, results[0].Count);
    }

    [ConditionalFact]
    public virtual void Count_Col_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Count(e.Salary, () => e.Salary != 500000.00m)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1, results[0].Count);
    }

    #endregion

    #region Average Tests

    [ConditionalFact]
    public virtual void Avg_Decimal()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            AverageSalary = EF.Functions.Over().Average(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(496875.111250m, results[0].AverageSalary);
    }

    [ConditionalFact]
    public virtual void Avg_Int()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            AverageWork = EF.Functions.Over().Average(e.WorkExperience)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(11, results[0].AverageWork);
    }

    [ConditionalFact]
    public virtual void Avg_Decimal_Int_Cast_Decimal()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            AverageWork = EF.Functions.Over().Average<decimal>(e.WorkExperience)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(11.375m, results[0].AverageWork);
    }

    [ConditionalFact]
    public virtual void Avg_Null()
    {
        using var context = CreateContext();

        var results = context.NullTestEmployees.Select(e => new
        {
            e.Id,
            e.Name,
            AverageSalary = EF.Functions.Over().Average(e.Salary)
        }).ToList();

        Assert.Equal(2, results.Count);
        Assert.Null(results[0].AverageSalary);
    }

    [ConditionalFact]
    public virtual void Avg_Filter()
    {
        using var context = CreateContext();

        var ids = new int[] { 1, 2, 3 };

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Avg = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Average<decimal?>(e.Salary, () => ids.Contains(e.EmployeeId))
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].Avg);
    }

    #endregion

    #region Sum Tests

    [ConditionalFact]
    public virtual void Sum_Decimal()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            SumSalary = EF.Functions.Over().Sum(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(3975000.89m, results[0].SumSalary);
    }

    [ConditionalFact]
    public virtual void Sum_Int()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            SumWorkExperience = EF.Functions.Over().Sum(e.WorkExperience)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(91, results[0].SumWorkExperience);
    }

    [ConditionalFact]
    public virtual void Sum_Null()
    {
        using var context = CreateContext();

        var results = context.NullTestEmployees.Select(e => new
        {
            e.Id,
            e.Name,
            Sum = EF.Functions.Over().Sum(e.Salary)
        }).ToList();

        Assert.Equal(2, results.Count);
        Assert.Null(results[0].Sum);
    }

    [ConditionalFact]
    public virtual void Sum_Filter()
    {
        using var context = CreateContext();

        var ids = new int[] { 1, 2, 3 };

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Sum = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Sum<decimal?>(e.Salary, () => ids.Contains(e.EmployeeId))
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].Sum);
    }

    #endregion

    [ConditionalFact]
    public virtual void RowNumber_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            RowNumber = EF.Functions.Over().OrderBy(e.Name).RowNumber()
        }).ToList();

        Assert.Equal(8, results.Count);

        for (int i = 0; i < results.Count; i++)
        {
            Assert.Equal(i + 1, results[i].RowNumber);
        }
    }

    [ConditionalFact]
    public virtual void First_Value_OderByEnd_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            FirstValue = EF.Functions.Over().OrderBy(e.Salary).FirstValue(e.Name)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal("Commander Cody", results[0].FirstValue);
    }

    [ConditionalFact]
    public virtual void First_Value_FrameEnd_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            FirstValue = EF.Functions.Over().OrderBy(e.Salary).Rows(RowsPreceding.CurrentRow, RowsFollowing.UnboundedFollowing).FirstValue(e.Name)
        }).ToList();

        Assert.Equal(8, results.Count);

        foreach(var row in results)
            Assert.Equal(results[0].Name, results[0].FirstValue);
    }

    [ConditionalFact]
    public virtual void First_Value_Null()
    {
        using var context = CreateContext();

        var results = context.NullTestEmployees.Select(e => new
        {
            e.Id,
            e.Name,
            FirstValue = EF.Functions.Over().OrderBy(e.WorkExperience).FirstValue(e.Salary)
        }).ToList();

        Assert.Equal(2, results.Count);
        Assert.Null(results[0].FirstValue);
    }

    [ConditionalFact]
    public virtual void Last_Value_OderByEnd_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            LastValue = EF.Functions.Over().OrderBy(e.Salary).LastValue(e.Name)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal("Commander Cody", results[0].LastValue);
    }

    [ConditionalFact]
    public virtual void Last_Value_FrameEnd_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            LastValue = EF.Functions.Over().OrderBy(e.Salary).Rows(RowsPreceding.CurrentRow, RowsFollowing.UnboundedFollowing).LastValue(e.Name)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal("Jabba the Hutt", results[0].LastValue);
    }

    [ConditionalFact]
    public virtual void Last_Value_Null()
    {
        using var context = CreateContext();

        var results = context.NullTestEmployees.Select(e => new
        {
            e.Id,
            e.Name,
            LastValue = EF.Functions.Over().OrderBy(e.WorkExperience).LastValue(e.Salary)
        }).ToList();

        Assert.Equal(2, results.Count);
        Assert.Null(results[0].LastValue);
    }

    [ConditionalFact]
    public virtual void Rank_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Rank = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.WorkExperience).Rank()
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Equal(3, results[0].Id);
        Assert.Equal(1, results[0].Rank);

        Assert.Equal(1, results[1].Id);
        Assert.Equal(1, results[1].Rank);

        Assert.Equal(4, results[2].Id);
        Assert.Equal(2, results[2].Rank);

        Assert.Equal(6, results[3].Id);
        Assert.Equal(1, results[3].Rank);

        Assert.Equal(7, results[4].Id);
        Assert.Equal(2, results[4].Rank);

        Assert.True(results[5].Id == 5 || results[5].Id == 8);
        Assert.Equal(1, results[5].Rank);

        Assert.True(results[6].Id == 5 || results[6].Id == 8);
        Assert.Equal(1, results[6].Rank);

        Assert.Equal(2, results[7].Id);
        Assert.Equal(3, results[7].Rank);
    }

    [ConditionalFact]
    public virtual void Dense_Rank_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Rank = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.WorkExperience).DenseRank()
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Equal(3, results[0].Id);
        Assert.Equal(1, results[0].Rank);

        Assert.Equal(1, results[1].Id);
        Assert.Equal(1, results[1].Rank);

        Assert.Equal(4, results[2].Id);
        Assert.Equal(2, results[2].Rank);

        Assert.Equal(6, results[3].Id);
        Assert.Equal(1, results[3].Rank);

        Assert.Equal(7, results[4].Id);
        Assert.Equal(2, results[4].Rank);

        Assert.True(results[5].Id == 5 || results[5].Id == 8);
        Assert.Equal(1, results[5].Rank);

        Assert.True(results[6].Id == 5 || results[6].Id == 8);
        Assert.Equal(1, results[6].Rank);

        Assert.Equal(2, results[7].Id);
        Assert.Equal(2, results[7].Rank);
    }

    [ConditionalFact]
    public virtual void NTile_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Rank = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.WorkExperience).NTile(3)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Equal(3, results[0].Id);
        Assert.Equal(1, results[0].Rank);

        Assert.Equal(1, results[1].Id);
        Assert.Equal(1, results[1].Rank);

        Assert.Equal(4, results[2].Id);
        Assert.Equal(2, results[2].Rank);

        Assert.Equal(6, results[3].Id);
        Assert.Equal(1, results[3].Rank);

        Assert.Equal(7, results[4].Id);
        Assert.Equal(2, results[4].Rank);

        Assert.True(results[5].Id == 5 || results[5].Id == 8);
        Assert.Equal(1, results[5].Rank);

        Assert.True(results[6].Id == 5 || results[6].Id == 8);
        Assert.Equal(2, results[6].Rank);

        Assert.Equal(2, results[7].Id);
        Assert.Equal(3, results[7].Rank);
    }

    [ConditionalFact]
    public virtual void Percent_Rank_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            PercentRank = EF.Functions.Over().OrderBy(e.Salary).PercentRank()
        }).ToList();

        //todo - this test might need to be altered for sqlite due to precision

        Assert.Equal(8, results.Count);
        Assert.Equal(0, results[0].PercentRank);
        Assert.Equal(0.1428571, Math.Round(results[1].PercentRank, 7));
        Assert.Equal(0.2857143, Math.Round(results[2].PercentRank, 7));
        Assert.Equal(0.4285714, Math.Round(results[3].PercentRank, 7));
        Assert.Equal(0.5714286, Math.Round(results[4].PercentRank, 7));
        Assert.Equal(0.7142857, Math.Round(results[5].PercentRank, 7));
        Assert.Equal(0.8571429, Math.Round(results[6].PercentRank, 7));
        Assert.Equal(1, results[7].PercentRank);
    }


    [ConditionalFact]
    public virtual void Cume_Dist_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            CumeDist = EF.Functions.Over().OrderBy(e.Salary).CumeDist()
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(0.125, results[0].CumeDist);
        Assert.Equal(0.25, results[1].CumeDist);
        Assert.Equal(0.375, results[2].CumeDist);
        Assert.Equal(0.5, results[3].CumeDist);
        Assert.Equal(0.625, results[4].CumeDist);
        Assert.Equal(0.75, results[5].CumeDist);
        Assert.Equal(0.875, results[6].CumeDist);
        Assert.Equal(1, results[7].CumeDist);
    }

    [ConditionalFact]
    public virtual void Lag_Decimal_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            PreviousSalary = EF.Functions.Over().OrderBy(e.Salary).Lag(e.Salary, 1, 0.0m)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(0, results[0].PreviousSalary);
        Assert.Equal(25000.12m, results[1].PreviousSalary);
        Assert.Equal(50000.00m, results[2].PreviousSalary);
        Assert.Equal(100000.00m, results[3].PreviousSalary);
        Assert.Equal(200000.00m, results[4].PreviousSalary);
        Assert.Equal(350000.24m, results[5].PreviousSalary);
        Assert.Equal(500000.00m, results[6].PreviousSalary);
        Assert.Equal(1000000.53m, results[7].PreviousSalary);
    }

    [ConditionalFact]
    public virtual void Lag_Int_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            PreviousId = EF.Functions.Over().OrderBy(e.Id).Lag(e.Id, 1, 0)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(0, results[0].PreviousId);
        Assert.Equal(1, results[1].PreviousId);
        Assert.Equal(2, results[2].PreviousId);
        Assert.Equal(3, results[3].PreviousId);
        Assert.Equal(4, results[4].PreviousId);
        Assert.Equal(5, results[5].PreviousId);
        Assert.Equal(6, results[6].PreviousId);
        Assert.Equal(7, results[7].PreviousId);
    }

    [ConditionalFact]
    public virtual void Lag_String_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            PreviousName = EF.Functions.Over().OrderBy(e.Name).Lag(e.Name, 1, "test")
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal("test", results[0].PreviousName);
        Assert.Equal(results[0].Name, results[1].PreviousName);
        Assert.Equal(results[1].Name, results[2].PreviousName);
        Assert.Equal(results[2].Name, results[3].PreviousName);
        Assert.Equal(results[3].Name, results[4].PreviousName);
        Assert.Equal(results[4].Name, results[5].PreviousName);
        Assert.Equal(results[5].Name, results[6].PreviousName);
        Assert.Equal(results[6].Name, results[7].PreviousName);
    }

    [ConditionalFact]
    public virtual void Lead_Decimal_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            NextSalary = EF.Functions.Over().OrderBy(e.Salary).Lead(e.Salary, 1, 0.0m)
        }).ToList();

        Assert.Equal(8, results.Count);
      
        Assert.Equal(50000.00m, results[0].NextSalary);
        Assert.Equal(100000.00m, results[1].NextSalary);
        Assert.Equal(200000.00m, results[2].NextSalary);
        Assert.Equal(350000.24m, results[3].NextSalary);
        Assert.Equal(500000.00m, results[4].NextSalary);
        Assert.Equal(1000000.53m, results[5].NextSalary);
        Assert.Equal(1750000.00m, results[6].NextSalary);
        Assert.Equal(0, results[7].NextSalary);
    }

    [ConditionalFact]
    public virtual void Lead_Int_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            NextId = EF.Functions.Over().OrderBy(e.Id).Lead(e.Id, 1, 0)
        }).ToList();

        Assert.Equal(8, results.Count);
     
        Assert.Equal(2, results[0].NextId);
        Assert.Equal(3, results[1].NextId);
        Assert.Equal(4, results[2].NextId);
        Assert.Equal(5, results[3].NextId);
        Assert.Equal(6, results[4].NextId);
        Assert.Equal(7, results[5].NextId);
        Assert.Equal(8, results[6].NextId);
        Assert.Equal(0, results[7].NextId);
    }

    #endregion

    #region WindowOverExpression Equality tests

    [ConditionalFact]
    public virtual void Multiple_Aggregates_Basic_NoDup_Query()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().Max(e.Salary),
            MinSalary = EF.Functions.Over().Min(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1750000.0m, results[0].MaxSalary);
        Assert.Equal(25000.12m, results[0].MinSalary);
    }

    [ConditionalFact]
    public virtual void Multiple_Aggregates_Basic_Dup_Query()
    {

        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary1 = EF.Functions.Over().Max(e.Salary),
            MaxSalary2 = EF.Functions.Over().Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1750000.0m, results[0].MaxSalary1);
        Assert.Equal(1750000.0m, results[0].MaxSalary2);
    }

    #endregion

    #region Rows / Range Tests

    #region Rows(int preceding)

    [ConditionalFact]
    public virtual void Rows_Preceding_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Rows(RowsPreceding preceding)

    [ConditionalFact]
    public virtual void Rows_Preceding_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Rows_Preceding_UnboundedPreceding()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.UnboundedPreceding).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Rows(int preceding, int following)

    [ConditionalFact]
    public virtual void Rows_Preceding_X_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(1, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Rows(RowsPreceding preceding, int following)

    [ConditionalFact]
    public virtual void Rows_Preceding_CurrentRow_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.CurrentRow, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Rows_Preceding_UnboundedPreceding_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.UnboundedPreceding, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Rows(int preceding, RowsFollowing following)

    [ConditionalFact]
    public virtual void Rows_Preceding_X_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Rows_Preceding_X_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Rows(RowsPreceding preceding, RowsFollowing following)

    [ConditionalFact]
    public virtual void Rows_Preceding_CurrentRow_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.CurrentRow, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Rows_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.CurrentRow, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Rows_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.UnboundedPreceding, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Rows_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(RowsPreceding.UnboundedPreceding, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Range(RowsPreceding preceding)

    [ConditionalFact]
    public virtual void Range_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Range_UnboundedPreceding()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.UnboundedPreceding).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #region Range(RowsPreceding preceding, RowsFollowing following)

    [ConditionalFact]
    public virtual void Range_Preceding_CurrentRow_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.CurrentRow, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Range_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.CurrentRow, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Range_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.UnboundedPreceding, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Range_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.UnboundedPreceding, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    #endregion

    #endregion

    #region Partition / Order By

    [ConditionalFact]
    public virtual void Rows_No_Parition()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().OrderBy(e.Name).Rows(1, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(500000.00m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Range_No_Parition()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().OrderBy(e.Name).Range(RowsPreceding.CurrentRow, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).OrderBy(r => r.Name).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1750000.00m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void OrderBy_No_Parition()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().OrderBy(e.Name).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(50000.00m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void OrderBy_Desc_No_Parition()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().OrderByDescending(e.Name).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(100000.00m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void OrderBy_Desc_Rows()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().OrderByDescending(e.Name).Rows(1).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(100000.00m, results[0].MaxSalary);
    }
   
    [ConditionalFact]
    public virtual void Outer_Order_By_Sql()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Rank = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.WorkExperience).ThenBy(e.Name).Rank()
        }).OrderBy(r => r.Name).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(5, results[0].Id);
    }

    [ConditionalFact]
    public virtual void Partition_No_OrderBy_No_Frame()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Partition_MultipleColumns()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName, e.WorkExperience).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);
    }

    [ConditionalFact]
    public virtual void Partition_ColumnModified()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.WorkExperience / 10).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(350000.24m, results[0].MaxSalary);
    }

    #endregion

    #region Nullability

    [ConditionalFact]
    public virtual void NullTestBang()
    {
        using var context = CreateContext();

        var ids = new int[] { 1, 2, 3 };

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            SumIn = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Sum<decimal?>(e.Salary, () => ids.Contains(e.EmployeeId)),
            SumNotIn = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Sum<decimal?>(e.Salary, () => !ids.Contains(e.EmployeeId))
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Null(results[0].SumNotIn);
        Assert.Equal(1000000.53m, results[0].SumIn);
    }

    [ConditionalFact]
    public virtual void NullTestEquals()
    {
        using var context = CreateContext();

        var ids = new int[] { 1, 2, 3 };

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            SumIn = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Sum<decimal?>(e.Salary, () => ids.Contains(e.EmployeeId) == true),
            SumNotIn = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Sum<decimal?>(e.Salary, () => ids.Contains(e.EmployeeId) == false)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Null(results[0].SumNotIn);
        Assert.Equal(1000000.53m, results[0].SumIn);
    }

    #endregion

    //todo - have the results of one window function be used by a second.

    #region Where

    /*[ConditionalFact]
    public virtual void WindowFunctionInWhere()
    {
        using var context = CreateContext();

        var results = context.Employees
            .Where(e => EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).RowNumber() == 1)
            .Select(e => new
            {
                e.Id,
                e.Name,
            }).ToList();

        Assert.Equal(1, results.Count);
    }*/

    #endregion

    #region OrderBy

    [ConditionalFact]
    public virtual void WindowFunctionInOrderBy()
    {
        using var context = CreateContext();

        var results = context.Employees
            .OrderBy(e => EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).RowNumber())
            .Select(e => new
            {
                e.Id,
                e.Name,
            }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(3, results[0].Id);
        Assert.Equal(4, results[1].Id);
        Assert.Equal(6, results[2].Id);
    }

    #endregion

    #endregion
}

