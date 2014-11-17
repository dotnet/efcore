// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using EntityFramework.Microbenchmarks.StateManagerPerf.Model;
using Microsoft.Data.Entity;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace EntityFramework.Microbenchmarks.StateManagerPerf
{
    public class StateManagerTestBase
    {
        public const string DefaultConnectionString =
            @"Data Source={0};Initial Catalog=EF7_StateManager;Integrated Security=True; MultipleActiveResultSets=true";

        public string ConnectionString { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public DbContextOptions Options { get; set; }

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer();
            return services.BuildServiceProvider();
        }

        public virtual void Setup()
        {
            ConnectionString = string.Format(DefaultConnectionString, TestConfig.Instance.DataSource);

            ServiceProvider = CreateServiceProvider();
            Options = new DbContextOptions();
            SetupDatabase(() => new AdventureWorks(ConnectionString, ServiceProvider, Options));
        }

        protected virtual void SetupDatabase(Func<AdventureWorks> getContext)
        {
            using (var context = getContext())
            {
                context.Database.EnsureCreated();
                if (!context.Products.Any())
                {
                    InsertTestingData(context);
                }
                //clear the SQL Server plan cache
                //TODO: context.ExecuteStoreCommand("DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
            }
        }

        private void InsertTestingData(AdventureWorks context)
        {
            var runGuid = Guid.NewGuid();
            const int productCount = 1000;
            for (var productIterator = 0; productIterator < productCount; ++productIterator)
            {
                var productModel = new ProductModel
                    {
                        CatalogDescription = "Product model " + productIterator,
                        Instructions = "Enjoy",
                        ModifiedDate = DateTime.Now,
                        Name = "Some model",
                        RowGuid = runGuid
                    };
                context.ProductModels.Add(productModel);
                var productCategory = new ProductCategory
                    {
                        Name = "pc",
                        RowGuid = runGuid,
                        ModifiedDate = DateTime.Now
                    };
                context.ProductCategories.Add(productCategory);
                var productSubCategory = new ProductSubCategory
                    {
                        Name = "psc",
                        RowGuid = runGuid,
                        ModifiedDate = DateTime.Now,
                        Category = productCategory
                    };
                context.ProductSubCategories.Add(productSubCategory);
                var product = new Product
                    {
                        Name = "Test product " + productIterator,
                        ProductNumber = productIterator.ToString(CultureInfo.InvariantCulture),
                        MakeFlag = false,
                        FinishedGoodsFlag = true,
                        Color = "Berengene",
                        SafetyStockLevel = 5,
                        ReorderPoint = 3,
                        StandardCost = 100,
                        ListPrice = (decimal)244.99,
                        Size = "XXXL",
                        SizeUnitMeasureCode = "US",
                        Weight = null,
                        DaysToManufacture = 13,
                        ProductLine = "1",
                        Class = "Test product",
                        Style = "A lot of it",
                        SellStartDate = DateTime.Now,
                        SellEndDate = null,
                        DiscontinuedDate = null,
                        RowGuid = runGuid,
                        ModifiedDate = DateTime.Now,
                        Model = productModel,
                        SubCategory = productSubCategory
                    };
                context.Products.Add(product);
                context.SaveChanges();
            }
            Console.WriteLine("Database setup complete");
        }
    }
}
