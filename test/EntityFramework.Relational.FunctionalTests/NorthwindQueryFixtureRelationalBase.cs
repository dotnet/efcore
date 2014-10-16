// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Northwind;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class NorthwindQueryFixtureRelationalBase : NorthwindQueryFixtureBase
    {
        public Entity.Metadata.Model SetTableNames(Entity.Metadata.Model model)
        {
            model.GetEntityType(typeof(Customer)).Relational().Table = "Customers";
            model.GetEntityType(typeof(Employee)).Relational().Table = "Employees";
            model.GetEntityType(typeof(Product)).Relational().Table = "Products";
            model.GetEntityType(typeof(Order)).Relational().Table = "Orders";
            model.GetEntityType(typeof(OrderDetail)).Relational().Table = "Order Details";

            return model;
        }
    }
}
