// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class SqlServerMemoryOptimizedTablesConventionTest
    {
        [ConditionalFact]
        public void Keys_and_indexes_are_nonclustered_for_memory_optimized_tables()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<Order>();

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.IsClustered() == null));
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.IsClustered() == null));

            modelBuilder.Entity<Order>().IsMemoryOptimized();

            modelBuilder.Entity<Order>().HasKey(
                o => new { o.Id, o.CustomerId });
            modelBuilder.Entity<Order>().HasIndex(o => o.CustomerId);

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.IsClustered() == false));
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.IsClustered() == false));

            modelBuilder.Entity<Order>().IsMemoryOptimized(false);

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.IsClustered() == null));
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.IsClustered() == null));
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
