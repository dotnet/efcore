// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Metadata.Conventions
{
    public class SqlServerCascadeDeleteConventionTest
    {
        [Fact]
        public void Foreign_keys_are_not_set_to_cascade_delete_for_memory_optimized_tables()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<Order>();

            Assert.True(
                modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().All(fk => fk.DeleteBehavior == DeleteBehavior.Cascade));

            modelBuilder.Entity<Order>().ForSqlServerIsMemoryOptimized();
            modelBuilder.Entity<Order>().HasOne<Customer>().WithMany().HasPrincipalKey(c => c.Id).IsRequired();

            Assert.True(
                modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().All(fk => fk.DeleteBehavior == DeleteBehavior.Restrict));

            modelBuilder.Entity<Order>().ForSqlServerIsMemoryOptimized(false);

            Assert.True(
                modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().All(fk => fk.DeleteBehavior == DeleteBehavior.Cascade));
        }

        private class Customer
        {
            public int Id { get; set; }

            public ICollection<Order> Orders { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }

            public Customer Customer { get; set; }
        }
    }
}
