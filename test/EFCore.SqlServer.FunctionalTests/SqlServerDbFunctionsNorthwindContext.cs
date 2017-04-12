// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels
{
    public class SqlServerDbFunctionsNorthwindContext : SqlServerNorthwindContext
    {
        public enum ReportingPeriod
        {
            Winter = 0,
            Spring,
            Summer,
            Fall
        }

        public SqlServerDbFunctionsNorthwindContext(DbContextOptions options,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(options, queryTrackingBehavior)
        {
        }

        [DbFunction("dbo", "EmployeeOrderCount")]
        public int EmployeeOrderCount(int employeeId)
        {
            throw new NotImplementedException();
        }

        [DbFunction("dbo", "EmployeeOrderCount")]
        public int EmployeeOrderCountWithClient(int employeeId)
        {
            switch (employeeId)
            {
                case 3: return 127;
                default: return 1;
            }
        }

        [DbFunction(Schema = "dbo")]
        public bool IsTopEmployee(int employeeId)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public int GetEmployeeWithMostOrdersAfterDate(DateTime? startDate)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public DateTime? GetReportingPeriodStartDate(ReportingPeriod periodId)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public string StarValue(int starCount, int value)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public int AddValues(int a, int b)
        {
            throw new NotImplementedException();
        }

        [DbFunction(Schema = "dbo")]
        public DateTime GetBestYearEver()
        {
            throw new NotImplementedException();
        }
    }
}
