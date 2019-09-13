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
            var model = BuildModel();

            var checkConstraint = model.FindEntityType(typeof(Order))
                                        .GetCheckConstraints()
                                        .FirstOrDefault<ICheckConstraint>(constraint =>
                                            constraint.Name == "CK_Order_CustomerType_Enum_Constraint");

            Assert.Null(checkConstraint);
            //Assert.Null(checkConstraint1);
            // Assert.Equal(entityType, checkConstraint.EntityType);
            // Assert.Equal("CK_Order_CustomerType_Enum_Constraint", checkConstraint.Name);
            // Assert.Equal("CHECK (CustomerType IN('Standard', 'Premium'))", checkConstraint.Sql);

        }

        private static IModel BuildModel(bool generateValues = true)
        {
            var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<Order>();

            return builder.FinalizeModel();
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public CustomerType CustomerType{ get; set; }
        }

        private enum CustomerType
        {
            Standard,
            Premium
        }
    }
}
