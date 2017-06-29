// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore
{
    public class NorthwindDbFunctionContext : NorthwindContext
    {
        public NorthwindDbFunctionContext(DbContextOptions options, QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(options, queryTrackingBehavior)
        {
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
                case 3: return 127;
                default: return 1;
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
