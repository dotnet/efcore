// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class UpdatesContext : PoolableDbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductWithBytes> ProductWithBytes { get; set; }
        public DbSet<AFewBytes> AFewBytes { get; set; }
        public DbSet<ProductViewTable> ProductView { get; set; }
        public DbSet<ProductTableWithView> ProductTable { get; set; }
        public DbSet<ProductTableView> ProductTableView { get; set; }

        public UpdatesContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(UpdatesContext context)
        {
            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            context.Add(
                new Category { Id = 78, PrincipalId = 778 });
            context.Add(
                new Product
                {
                    Id = productId1,
                    Name = "Apple Cider",
                    Price = 1.49M,
                    DependentId = 778
                });
            context.Add(
                new Product
                {
                    Id = productId2,
                    Name = "Apple Cobler",
                    Price = 2.49M,
                    DependentId = 778
                });

            context.SaveChanges();
        }
    }
}
