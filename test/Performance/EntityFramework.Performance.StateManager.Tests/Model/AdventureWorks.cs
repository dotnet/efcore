// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace StateManager.Model
{
    using System;
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Metadata;

    public class AdventureWorks : DbContext
    {
        private readonly string _connectionString;

        public AdventureWorks(string connectionString, IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
            _connectionString = connectionString;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductModel> ProductModels { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSubCategory> ProductSubCategories { get; set; }

        protected override void OnConfiguring(DbContextOptions builder)
        {
            builder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(b =>
            {
                b.ForRelational().Table("Product", "dbo");
                b.Key(e => e.ProductId);
            });
            modelBuilder.Entity<ProductModel>(b =>
            {
                b.ForRelational().Table("ProductModel", "dbo");
                b.Key(e => e.ProductModelId);
            });
            modelBuilder.Entity<ProductCategory>(b =>
            {
                b.ForRelational().Table("ProductCategory", "dbo");
                b.Key(e => e.ProductCategoryId);
            });
            modelBuilder.Entity<ProductSubCategory>(b =>
            {
                b.ForRelational().Table("ProductSubCategory", "dbo");
                b.Key(e => e.ProductSubcategoryId);
            });

            modelBuilder.Entity<ProductModel>(
                b => b.OneToMany(e => e.Products, e => e.Model));

            modelBuilder.Entity<ProductSubCategory>(
                b => b.OneToMany(e => e.Products, e => e.SubCategory));

            modelBuilder.Entity<ProductCategory>(
                b => b.OneToMany(e => e.SubCategories, e => e.Category));
        }
    }
}
