// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.FunctionalTests;
using Northwind;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class NorthwindQueryFixtureRelationalBase : NorthwindQueryFixtureBase
    {
        public Metadata.Model SetTableNames(Metadata.Model model)
        {
            model.GetEntityType(typeof(Customer)).SetTableName("Customers");
            model.GetEntityType(typeof(Employee)).SetTableName("Employees");
            model.GetEntityType(typeof(Product)).SetTableName("Products");
            model.GetEntityType(typeof(Order)).SetTableName("Orders");
            model.GetEntityType(typeof(OrderDetail)).SetTableName("OrderDetails");

            return model;
        }
    }
}
