// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryLoggingCosmosTest : IClassFixture<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public QueryLoggingCosmosTest(NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected NorthwindQueryCosmosFixture<NoopModelCustomizer> Fixture { get; }

        [ConditionalFact]
        public virtual void Queryable_simple()
        {
            using var context = CreateContext();
            var customers
                = context.Set<Customer>()
                    .ToList();

            Assert.NotNull(customers);

            Assert.StartsWith(
                "Compiling query expression: ",
                Fixture.TestSqlLoggerFactory.Log[0].Message);
            Assert.StartsWith(
                "Generated query execution expression: " + Environment.NewLine + "'queryContext => new QueryingEnumerable<Customer>(",
                Fixture.TestSqlLoggerFactory.Log[1].Message);
        }

        [ConditionalFact]
        public virtual void Queryable_with_parameter_outputs_parameter_value_logging_warning()
        {
            using var context = CreateContext();
            context.GetInfrastructure().GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Query>>()
                .Options.IsSensitiveDataLoggingWarned = false;
            // ReSharper disable once ConvertToConstant.Local
            var city = "Redmond";

            var customers
                = context.Customers
                    .Where(c => c.City == city)
                    .ToList();

            Assert.NotNull(customers);
            Assert.Contains(
                CoreResources.LogSensitiveDataLoggingEnabled(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        [ConditionalFact]
        public virtual void Skip_without_order_by()
        {
            using var context = CreateContext();
            var customers = context.Set<Customer>().Skip(85).Take(5).ToList();

            Assert.NotNull(customers);

            Assert.Equal(
                CoreResources.LogRowLimitingOperationWithoutOrderBy(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
                Fixture.TestSqlLoggerFactory.Log[1].Message);
        }

        [ConditionalFact]
        public virtual void Take_without_order_by()
        {
            using var context = CreateContext();
            var customers = context.Set<Customer>().Take(5).ToList();

            Assert.NotNull(customers);

            Assert.Equal(
                CoreResources.LogRowLimitingOperationWithoutOrderBy(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
                Fixture.TestSqlLoggerFactory.Log[1].Message);
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
