// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class UpdatesContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductWithBytes> ProductWithBytes { get; set; }

        public UpdatesContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(UpdatesContext context)
        {
            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            context.Add(new Category { Id = 78, PrincipalId = 778 });
            context.Add(new Product { Id = productId1, Name = "Apple Cider", Price = 1.49M, DependentId = 778 });
            context.Add(new Product { Id = productId2, Name = "Apple Cobler", Price = 2.49M, DependentId = 778 });
            context.Add(new ProductWithBytes { Id = productId1, Name = "MegaChips", Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } });

            context.SaveChanges();
        }
    }
}
