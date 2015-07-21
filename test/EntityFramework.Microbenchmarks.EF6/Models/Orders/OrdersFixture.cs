// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;

namespace EntityFramework.Microbenchmarks.EF6.Models.Orders
{
    public class OrdersFixture
    {
        private readonly string _connectionString;

        public OrdersFixture(string databaseName, int productCount, int customerCount, int ordersPerCustomer, int linesPerOrder)
        {
            _connectionString = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database={databaseName};Integrated Security=True;MultipleActiveResultSets=true;";

            new OrdersSeedData().EnsureCreated(_connectionString, productCount, customerCount, ordersPerCustomer, linesPerOrder);
        }

        public OrdersContext CreateContext()
        {
            return new OrdersContext(_connectionString);
        }
    }
}
