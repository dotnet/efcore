// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.StateManagerPerf.Model;

namespace EntityFramework.Microbenchmarks.StateManagerPerf
{
    public class FixupTests : StateManagerTestBase
    {
        public void RelationshipFixup()
        {
            using (var context = new AdventureWorks(ConnectionString, ServiceProvider, Options))
            {
                //Bring principals into the context
                context.ProductModels.ToList();
                context.ProductSubCategories.ToList();

                //Materialize all dependents
                context.Products.ToList();
            }
        }

        /*public void RelationshipFixupMultithreaded(object state)
        {
            using (var context = new AdventureWorks(ConnectionString, ServiceProvider, Options))
            {
                //Bring principals into the context
                context.ProductModels.ToList();
                context.ProductSubCategories.ToList();

                //Materialize all dependents
                context.Products.ToList();
            }
        }

        internal object NewContextAndLoadDependants()
        {
            return null;
        }*/
    }
}
