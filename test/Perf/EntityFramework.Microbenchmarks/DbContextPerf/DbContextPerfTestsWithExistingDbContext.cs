// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.DbContextPerf.Model;

namespace EntityFramework.Microbenchmarks.DbContextPerf
{
    public class DbContextPerfTestsWithExistingDbContext : DbContextPerfTestsBase
    {
        public AdvWorksDbContext NewContext()
        {
            return new AdvWorksDbContext(ConnectionString, ServiceProvider, Options);
        }

        public void DbContextQueryOnExistingContextWithThreads(object state)
        {
            var advWorks = (AdvWorksDbContext)state;
            foreach (var product in advWorks.Products)
            {
                var id = product.ProductID;
                var subcategory = product.ProductSubcategoryID;
                var name = product.Name;
                var DaysToManufacture = product.DaysToManufacture;
            }
        }
    }
}
