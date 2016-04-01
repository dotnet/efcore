// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.UpdatesModel
{
    public class UpdatesContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public UpdatesContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasOne<Category>().WithOne()
                .HasForeignKey<Product>(e => e.DependentId)
                .HasPrincipalKey<Category>(e => e.PrincipalId);

            modelBuilder.Entity<Product>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Category>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }
}
