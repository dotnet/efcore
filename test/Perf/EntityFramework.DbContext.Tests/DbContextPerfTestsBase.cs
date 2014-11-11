// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Performance.DbContextTests.Model;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Performance.DbContextTests
{
    public class DbContextPerfTestsBase
    {
        protected const int OuterLoop = 10;
        protected const int InnerLoop = 100;
        protected const int InnerLoopLarge = 1000;
        protected const string DefaultServer = @".";

        protected const string ConnectionStringBase =
            @"Data Source={0};Initial Catalog=EF7_DbContextPerfTests;Integrated Security=True; MultipleActiveResultSets={1}";

        protected const bool EnableMultipleActiveResultSets = false;

        protected string ConnectionString;
        protected IServiceProvider ServiceProvider;
        protected DbContextOptions Options;

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer();
            return services.BuildServiceProvider();
        }

        public virtual void Setup()
        {
            var configuration = new Configuration();

            try
            {
                configuration.AddJsonFile(@"LocalConfig.json");
                ConnectionString = configuration.Get("Data:DefaultConnection:Connectionstring");
            }
            catch (Exception e)
            {
                Console.WriteLine("error reading config: " + e.Message);
            }

            ConnectionString = ConnectionString ?? string.Format(ConnectionStringBase, DefaultServer, EnableMultipleActiveResultSets);
            ServiceProvider = CreateServiceProvider();
            Options = new DbContextOptions();
            SetupDatabase(
                () => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options),
                productModelCount: 100,
                productSubcategoryCount: 100,
                productCount: 2000);
        }

        private void SetupDatabase(Func<AdvWorksDbContext> getContext, int productModelCount, int productSubcategoryCount, int productCount)
        {
            using (var advWorks = getContext())
            {
                advWorks.Database.EnsureCreated();
                if (!advWorks.ProductModels.Any())
                {
                    for (var insertId = 1; insertId < productModelCount; ++insertId)
                    {
                        var newProductModel = new DbProductModel
                            {
                                Name = "TEST_INSERTS",
                                ProductModelID = insertId
                            };
                        advWorks.ProductModels.Add(newProductModel);
                    }
                    advWorks.SaveChanges();
                }

                if (!advWorks.Products.Any())
                {
                    var productModel = advWorks.ProductModels.First(p => p.ProductModelID == 15);
                    var productSet = advWorks.Set<DbProduct>();

                    for (var insertId = 1; insertId < productCount; insertId++)
                    {
                        var product = new DbProduct
                            {
                                ProductID = insertId++,
                                Name = "TEST_INSERTS",
                                DaysToManufacture = 5
                            };
                        productModel.Products.Add(product);

                        productSet.Add(product);
                    }

                    advWorks.SaveChanges();
                }
            }
        }

        public virtual void Cleanup()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                new SqlCommand("DELETE FROM dbo.Product WHERE ProductID >= 100", conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM dbo.ProductSubcategory WHERE ProductSubcategoryID >= 100", conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM dbo.ProductModel WHERE ProductModelID >= 2000", conn).ExecuteNonQuery();
                conn.Close();
            }
        }

        protected List<DbProduct> Populate(DbContext advWorks, int numRows)
        {
            var list = new List<DbProduct>(numRows);

            var productModel = advWorks.Set<DbProductModel>().First(p => p.ProductModelID == 15);
            var productSet = advWorks.Set<DbProduct>();

            var insertId = 2000;

            for (var i = 0; i < numRows; i++)
            {
                var product = new DbProduct
                    {
                        ProductID = insertId++,
                        Name = "TEST_INSERTS",
                        DaysToManufacture = 5
                    };
                productModel.Products.Add(product);

                productSet.Add(product);

                list.Add(product);
            }

            advWorks.SaveChanges();

            return list;
        }

        protected void DbContextQuery(Func<AdvWorksDbContext> getContext)
        {
            using (var advWorks = getContext())
            {
                for (var r = 0; r < OuterLoop; r++)
                {
                    for (var i = 0; i < InnerLoop; i++)
                    {
                        foreach (var product in advWorks.Products)
                        {
                            var val1 = product.Name;
                        }
                    }
                }
            }
        }

        protected void DbContextQueryNoTracking(Func<AdvWorksDbContext> getContext)
        {
            for (var r = 0; r < OuterLoop; r++)
            {
                for (var i = 0; i < InnerLoop; i++)
                {
                    using (var context = getContext())
                    {
                        //TODO: var products = context.Products.AsNoTracking(); when it becomes available
                        var products = context.Products;

                        foreach (var product in products)
                        {
                            var val1 = product.Name;
                        }
                    }
                }
            }
        }

        protected void DbContextInsert(Func<AdvWorksDbContext> getContext)
        {
            using (var advWorks = getContext())
            {
                var insertId = 2000;
                var productModel = advWorks.ProductModels.First(p => p.ProductModelID == 15);

                for (var i = 0; i < OuterLoop; i++)
                {
                    for (var j = 0; j < InnerLoopLarge; j++)
                    {
                        var product = new DbProduct
                            {
                                ProductID = insertId++,
                                Name = "TEST_INSERTS",
                                DaysToManufacture = 5
                            };
                        productModel.Products.Add(product);
                    }

                    advWorks.SaveChanges();
                }
            }
        }

        private List<DbProduct> _dbContextUpdateProducts;

        protected void DbContextUpdateSetup(Func<AdvWorksDbContext> getContext)
        {
            Setup();
            using (var advWorks = getContext())
            {
                _dbContextUpdateProducts = Populate(advWorks, 2000);
            }
        }

        protected void DbContextUpdate(Func<AdvWorksDbContext> getContext)
        {
            Debug.Assert(_dbContextUpdateProducts != null && _dbContextUpdateProducts.Count > 0);

            using (var advWorks = getContext())
            {
                var x = 0;

                for (var i = 0; i < 100; i++)
                {
                    const int updFreq = 10;
                    const int saveFreq = 100;

                    var j = 1;
                    foreach (var product in _dbContextUpdateProducts)
                    {
                        if (j % updFreq == 0)
                        {
                            product.Name = "TEST_UPDATES" + x++;
                            product.DaysToManufacture = i;
                        }

                        if (j % saveFreq == 0)
                        {
                            advWorks.SaveChanges();
                        }

                        j++;
                    }
                }
            }
            _dbContextUpdateProducts = null;
        }

        private List<DbProduct> _dbContextDeleteProducts;

        protected void DbContextDeleteSetup(Func<AdvWorksDbContext> getContext)
        {
            Setup();
            using (var advWorks = getContext())
            {
                _dbContextDeleteProducts = Populate(advWorks, 14000);
            }
        }

        protected void DbContextDelete(Func<AdvWorksDbContext> getContext)
        {
            Debug.Assert(_dbContextDeleteProducts != null && _dbContextDeleteProducts.Count > 0);

            using (var advWorks = getContext())
            {
                for (var pass = 0; pass < 10; pass++)
                {
                    const int delFreq = 10;
                    const int saveFreq = 100;

                    var i = 1;
                    foreach (var product in _dbContextDeleteProducts)
                    {
                        if (i % delFreq == pass)
                        {
                            advWorks.Products.Remove(product);
                        }

                        if (i % saveFreq == 0)
                        {
                            advWorks.SaveChanges();
                        }

                        i++;
                    }
                }
            }
            _dbContextDeleteProducts = null;
        }

        protected void DbContextQueryWithThreadsNoTracking(Func<AdvWorksDbContext> getContext)
        {
            using (var advWorks = getContext())
            {
                //TODO: var products = context.Products.AsNoTracking(); when it becomes available
                var products = advWorks.Products;

                foreach (var product in products)
                {
                    var id = product.ProductID;
                    var subcategory = product.ProductSubcategoryID;
                    var name = product.Name;
                    var daysToManufacture = product.DaysToManufacture;
                }
            }
        }
    }
}
