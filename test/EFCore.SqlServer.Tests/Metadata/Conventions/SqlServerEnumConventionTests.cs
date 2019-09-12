// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class SqlServerEnumConventionTests
    {
        [ConditionalFact]
        public void GenerateConstraintWithAllEnumNames()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            var entityType = modelBuilder.Entity<Order>().Metadata;
            var checkConstraint = entityType.FindCheckConstraint("CK_Order_CustomerType_Enum_Constraint");

            Assert.NotNull(checkConstraint);
            Assert.Equal(entityType, checkConstraint.EntityType);
            Assert.Equal("CK_Order_CustomerType_Enum_Constraint", checkConstraint.Name);
            Assert.Equal("CHECK (CustomerType IN('Standard', 'Premium'))", checkConstraint.Sql);

        }

        public class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public CustomerType CustomerType{ get; set; }
        }

        public enum CustomerType
        {
            Standard,
            Premium
        }
    }
}
