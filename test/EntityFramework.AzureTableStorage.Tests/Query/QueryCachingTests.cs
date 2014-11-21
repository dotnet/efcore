// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class QueryCachingTests
    {
        [Fact]
        public void Multiple_identical_queries()
        {
            var connection = new Mock<AtsConnection>();
            connection
                .Setup(s => s.ExecuteRequest(It.IsAny<TableRequest<bool>>(), It.IsAny<ILogger>()))
                .Returns(false); // keep all requests in memory

            var services = new ServiceCollection();
            services.AddInstance(connection.Object);

            var context = TestHelpers.CreateContext(services, CreateModel());
            (from c in context.Set<Customer>()
                orderby context.Set<Customer>().Any(c2 => c2.CustomerID == c.CustomerID)
                select c).AsNoTracking().ToList();

            connection.Verify(s => s.ExecuteRequest(
                It.IsAny<QueryTableRequest<Customer>>(),
                It.IsAny<ILogger>()),
                Times.Once());
        }

        private IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<Customer>().Property(s => s.CustomerID);

            return model;
        }

        private class Customer
        {
            public string CustomerID { get; set; }
        }
    }
}
