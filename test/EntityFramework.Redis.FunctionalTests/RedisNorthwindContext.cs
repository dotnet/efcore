using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity;

namespace EntityFramework.Redis.FunctionalTests
{
    public class RedisNorthwindContext :NorthwindContext
    {
        public RedisNorthwindContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>().Key("OrderID", "ProductID");
            base.OnModelCreating(modelBuilder);
        }
    }
}