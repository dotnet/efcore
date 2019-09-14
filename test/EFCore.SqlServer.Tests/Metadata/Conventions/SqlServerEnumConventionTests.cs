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
        public void GenerateCheckConstraintWithAllEnumNames()
        {

            var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<Order>()
                        .Property(o => o.OrderStatus).HasConversion<string>();

            var model = builder.FinalizeModel();

            var checkConstraint = model.FindEntityType(typeof(Order))
                                        .GetCheckConstraints()
                                        .FirstOrDefault<ICheckConstraint>(constraint =>
                                            constraint.Name == "CK_Order_OrderStatus_Enum_Constraint");

            Assert.NotNull(checkConstraint);
            Assert.Equal("CK_Order_OrderStatus_Enum_Constraint", checkConstraint.Name);
            Assert.Equal("CHECK (OrderStatus IN('Active', 'Completed'))", checkConstraint.Sql);

        }

        [ConditionalFact]
        public void GenerateCheckConstraintWithAllEnumValues()
        {

            var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<Customer>();

            var model = builder.FinalizeModel();

            var checkConstraint = model.FindEntityType(typeof(Customer))
                                        .GetCheckConstraints()
                                        .FirstOrDefault<ICheckConstraint>(constraint =>
                                            constraint.Name == "CK_Customer_CustomerType_Enum_Constraint");

            Assert.NotNull(checkConstraint);
            Assert.Equal("CK_Customer_CustomerType_Enum_Constraint", checkConstraint.Name);
            Assert.Equal("CHECK (CustomerType IN(0, 1))", checkConstraint.Sql);

        }

        [ConditionalFact]
        public void ShouldNotGenerateCheckConstraintForEmptyEnum()
        {

            var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<Seller>()
                        .Property(p => p.SellerStatusString)
                        .HasConversion<string>();

            var model = builder.FinalizeModel();

            var checkConstraintString = model.FindEntityType(typeof(Seller))
                                        .GetCheckConstraints()
                                        .FirstOrDefault<ICheckConstraint>(constraint =>
                                            constraint.Name == "CK_Seller_SellerStatusString_Enum_Constraint");

            var checkConstraintInt = model.FindEntityType(typeof(Seller))
                                        .GetCheckConstraints()
                                        .FirstOrDefault<ICheckConstraint>(constraint =>
                                            constraint.Name == "CK_Seller_checkConstraintInt_Enum_Constraint");

            Assert.Null(checkConstraintString);
            Assert.Null(checkConstraintInt);
        }

        private class Seller
        {
            public int Id{ get; set; }
            public string SellerName { get; set; }
            public SellerStatus SellerStatusString { get; set; }
            public SellerStatus SellerStatusInt { get; set; }
        }

        private enum SellerStatus
        {
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public OrderStatus OrderStatus{ get; set; }
        }

        private enum OrderStatus
        {
            Active,
            Completed
        }

        private class Customer
        {
            public int Id { get; set; }
            public int CustomerName { get; set; }
            public CustomerType CustomerType{ get; set; }
        }

        private enum CustomerType
        {
            Standard = 0,
            Premium = 1
        }
    }
}
