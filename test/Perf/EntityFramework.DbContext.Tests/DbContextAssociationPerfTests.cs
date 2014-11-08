// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Performance.DbContextTests.Model;

namespace Microsoft.Data.Entity.Performance.DbContextTests
{
    public class DbContextAssociationPerfTests : DbContextPerfTestsBase
    {
        public void DbContextRelationshipFixup()
        {
            using (var context = new AdvWorksDbContext(ConnectionString, ServiceProvider, Options))
            {
                var x1 = context.ProductModels.ToList();
                var x2 = context.ProductSubcategories.ToList();

                //Materialize all dependents
                var x3 = context.Products.ToList();
            }
        }
    }
}
