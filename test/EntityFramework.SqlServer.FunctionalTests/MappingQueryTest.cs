// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MappingQueryTest : MappingQueryTestBase, IClassFixture<MappingQueryFixture>
    {
        public override void All_customers()
        {
            base.All_customers();
            
            Assert.Equal(
                @"SELECT [c].[CompanyName], [c].[CustomerID]
FROM [dbo].[Customers] AS [c]",
                _fixture.Sql);
        }
        
        public override void All_employees()
        {
            base.All_employees();
            
            Assert.Equal(
                @"SELECT [e].[City], [e].[EmployeeID]
FROM [dbo].[Employees] AS [e]",
                _fixture.Sql);
        }

        public override void All_orders()
        {
            base.All_orders();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ShipVia]
FROM [dbo].[Orders] AS [o]",
                _fixture.Sql);
        }

        public override void Project_nullable_enum()
        {
            base.Project_nullable_enum();

            Assert.Equal(
                @"SELECT [o].[ShipVia]
FROM [dbo].[Orders] AS [o]",
                _fixture.Sql);
        }

        private readonly MappingQueryFixture _fixture;

        public MappingQueryTest(MappingQueryFixture fixture)
        {
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
