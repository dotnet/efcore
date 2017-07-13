// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class NorthwindRelationalContext : NorthwindContext
    {
        public NorthwindRelationalContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("Order Details");

            modelBuilder.Entity<CustomerOrderHistory>().HasKey(coh => coh.ProductName);
            modelBuilder.Entity<MostExpensiveProduct>().HasKey(mep => mep.TenMostExpensiveProducts);

            var methodInfo = typeof(NorthwindRelationalContext)
                .GetRuntimeMethod(nameof(MyCustomLength), new[] { typeof(string) });

            modelBuilder.HasDbFunction(methodInfo)
                .HasTranslation(args => new SqlFunctionExpression("len", methodInfo.ReturnType, args));

            modelBuilder.HasDbFunction(typeof(NorthwindRelationalContext)
                .GetRuntimeMethod(nameof(IsDate), new[] { typeof(string) }));
        }

        public enum ReportingPeriod
        {
            Winter = 0,
            Spring,
            Summer,
            Fall
        }

        public static int MyCustomLength(string s)
        {
            throw new Exception();
        }

        public static bool IsDate(string date)
        {
            throw new Exception();
        }

        [DbFunction(Schema = "dbo", FunctionName = "EmployeeOrderCount")]
        public static int EmployeeOrderCount(int employeeId)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo", FunctionName = "EmployeeOrderCount")]
        public static int EmployeeOrderCountWithClient(int employeeId)
        {
            switch (employeeId)
            {
                case 3:
                    return 127;
                default:
                    return 1;
            }
        }

        [DbFunction(Schema = "dbo")]
        public static bool IsTopEmployee(int employeeId)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public static int GetEmployeeWithMostOrdersAfterDate(DateTime? startDate)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public static DateTime? GetReportingPeriodStartDate(ReportingPeriod periodId)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public static string StarValue(int starCount, int value)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public static int AddValues(int a, int b)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public static DateTime GetBestYearEver()
        {
            throw new NotImplementedException();
        }
    }
}
