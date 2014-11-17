// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace EntityFramework.Microbenchmarks.DbContextPerf.Model
{
    public class AdvWorksDbContext : DbContext
    {
        private readonly string _connectionString;

        public AdvWorksDbContext(string connectionString, IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
            _connectionString = connectionString;
        }

        public DbSet<DbProduct> Products { get; set; }
        public DbSet<DbProductModel> ProductModels { get; set; }
        public DbSet<DbWorkOrder> WorkOrders { get; set; }
        public DbSet<DbProductSubcategory> ProductSubcategories { get; set; }

        protected override void OnConfiguring(DbContextOptions builder)
        {
            builder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DbProduct>(b =>
                {
                    b.Key(e => e.ProductID);
                    b.Property(e => e.ProductID).ForSqlServer().UseSequence();
                    b.ForRelational().Table("Product", "dbo");
                });
            builder.Entity<DbProductModel>(b =>
                {
                    b.Key(e => e.ProductModelID);
                    b.Property(e => e.ProductModelID).ForSqlServer().UseSequence();
                    b.ForRelational().Table("ProductModel", "dbo");
                });
            builder.Entity<DbWorkOrder>(b =>
                {
                    b.Key(e => e.WorkOrderID);
                    b.Property(e => e.WorkOrderID).ForSqlServer().UseSequence();
                    b.ForRelational().Table("WorkOrder", "dbo");
                });
            builder.Entity<DbProductSubcategory>(b =>
                {
                    b.Key(e => e.ProductSubcategoryID);
                    b.Property(e => e.ProductSubcategoryID).ForSqlServer().UseSequence();
                    b.ForRelational().Table("ProductSubcategory", "dbo");
                });

            //Foreign keys
            builder.Entity<DbProduct>().ForeignKey<DbProductSubcategory>(e => e.ProductSubcategoryID);
            builder.Entity<DbProduct>().ForeignKey<DbProductModel>(e => e.ProductModelID);

            var model = builder.Model;

            // TODO: Key should get by-convention value generation even if key is not discovered by convention
            var productId = model.GetEntityType(typeof(DbProduct)).GetProperty("ProductID");
            productId.GenerateValueOnAdd = true;

            var productModelId = model.GetEntityType(typeof(DbProductModel)).GetProperty("ProductModelID");
            productModelId.GenerateValueOnAdd = true;

            var workOrderId = model.GetEntityType(typeof(DbWorkOrder)).GetProperty("WorkOrderID");
            workOrderId.GenerateValueOnAdd = true;

            var productSubcategoryId = model.GetEntityType(typeof(DbProductSubcategory)).GetProperty("ProductSubcategoryID");
            productSubcategoryId.GenerateValueOnAdd = true;

            AddNavigationToPrincipal(model, typeof(DbProduct), "ProductSubcategoryID", "ProductSubcategory");
            AddNavigationToPrincipal(model, typeof(DbProduct), "ProductModelID", "Model");
        }

        private static void AddNavigationToPrincipal(Microsoft.Data.Entity.Metadata.Model model, Type type, string fk, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    navigation,
                    model.GetEntityType(type).ForeignKeys.Single(
                        f => f.Properties.Count == 1 && f.Properties.Single().Name == fk),
                    pointsToPrincipal: true);
        }

        private static void AddNavigationToDependent(Microsoft.Data.Entity.Metadata.Model model, Type type, string fk, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    navigation,
                    model.GetEntityType(type).ForeignKeys.Single(
                        f => f.Properties.Count == 1 && f.Properties.Single().Name == fk),
                    pointsToPrincipal: false);
        }

        private static void AddNavigationToDependent(Microsoft.Data.Entity.Metadata.Model model, Type type, Type dependentType, string fk, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    navigation,
                    model.GetEntityType(dependentType).ForeignKeys.Single(
                        f => f.Properties.Count == 1 && f.Properties.Single().Name == fk),
                    pointsToPrincipal: false);
        }

        private static void AddNavigationToDependent(Microsoft.Data.Entity.Metadata.Model model, Type type, Type dependentType, string fk1, string fk2, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    navigation,
                    model.GetEntityType(dependentType).ForeignKeys.Single(
                        f => f.Properties.Count == 2
                             && f.Properties.Any(p => p.Name == fk1)
                             && f.Properties.Any(p => p.Name == fk2)),
                    pointsToPrincipal: false);
        }

        private static void AddNavigationToPrincipal(Microsoft.Data.Entity.Metadata.Model model, Type type, string fk1, string fk2, string navigation)
        {
            model.GetEntityType(type)
                .AddNavigation(
                    navigation,
                    model.GetEntityType(type).ForeignKeys.Single(
                        f => f.Properties.Count == 2
                             && f.Properties.Any(p => p.Name == fk1)
                             && f.Properties.Any(p => p.Name == fk2)),
                    pointsToPrincipal: true);
        }
    }
}
