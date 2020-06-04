// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalModelTest
    {
        [ConditionalTheory]
        [InlineData(true, Mapping.TPH)]
        [InlineData(true, Mapping.TPT)]
        [InlineData(false, Mapping.TPH)]
        [InlineData(false, Mapping.TPT)]
        public void Can_use_relational_model_with_tables(bool useExplicitMapping, Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: useExplicitMapping, mapping: mapping);

            Assert.Equal(7, model.Model.GetEntityTypes().Count());
            Assert.Equal(mapping == Mapping.TPH || !useExplicitMapping ? 3 : 4, model.Tables.Count());
            Assert.Empty(model.Views);
            Assert.True(model.Model.GetEntityTypes().All(et => !et.GetViewMappings().Any()));

            AssertTables(model, useExplicitMapping ? mapping : Mapping.TPH);
        }

        [ConditionalTheory]
        [InlineData(Mapping.TPH)]
        [InlineData(Mapping.TPT)]
        public void Can_use_relational_model_with_views(Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: false, mapToViews: true, mapping);

            Assert.Equal(7, model.Model.GetEntityTypes().Count());
            Assert.Equal(mapping == Mapping.TPH ? 3 : 4, model.Views.Count());
            Assert.Empty(model.Tables);
            Assert.True(model.Model.GetEntityTypes().All(et => !et.GetTableMappings().Any()));

            AssertViews(model, mapping);
        }

        [ConditionalTheory]
        [InlineData(Mapping.TPH)]
        [InlineData(Mapping.TPT)]
        public void Can_use_relational_model_with_views_and_tables(Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: true, mapToViews: true, mapping);

            Assert.Equal(7, model.Model.GetEntityTypes().Count());
            Assert.Equal(mapping == Mapping.TPH ? 3 : 4, model.Tables.Count());
            Assert.Equal(mapping == Mapping.TPH ? 3 : 4, model.Views.Count());

            AssertTables(model, mapping);
            AssertViews(model, mapping);
        }

        private static void AssertViews(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetViewMappings().Single();
            Assert.Same(orderType.GetViewMappings(), orderType.GetViewOrTableMappings());
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate), nameof(Order.OrderId) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersView = orderMapping.View;
            Assert.Same(ordersView, model.FindView(ordersView.Name, ordersView.Schema));
            Assert.Equal(
                new[] { nameof(Order), "OrderDetails.BillingAddress#Address", "OrderDetails.ShippingAddress#Address", nameof(OrderDetails) },
                ordersView.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(new[] {
                    nameof(Order.AlternateId),
                    nameof(Order.CustomerId),
                    "Details_BillingAddress_City",
                    "Details_BillingAddress_Street",
                    "Details_ShippingAddress_City",
                    "Details_ShippingAddress_Street",
                    "OrderDateView",
                    nameof(Order.OrderId)
            },
                ordersView.Columns.Select(m => m.Name));
            Assert.Equal("OrderView", ordersView.Name);
            Assert.Equal("viewSchema", ordersView.Schema);
            Assert.Null(ordersView.ViewDefinitionSql);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            Assert.False(orderDate.IsViewColumnNullable());

            var orderDateMapping = orderDate.GetViewColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.ViewMapping);

            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersView, orderDetailsType.GetViewMappings().Single().View);
            Assert.Equal(ordersView.GetReferencingRowInternalForeignKeys(orderType), ordersView.GetRowInternalForeignKeys(orderDetailsType));

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
            Assert.True(orderDetailsDate.IsViewColumnNullable());

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersView.FindColumn("OrderDateView"));
            Assert.Same(orderDateColumn, orderDate.FindViewColumn(ordersView.Name, ordersView.Schema));
            Assert.Equal(new[] { orderDate, orderDetailsDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal("OrderDateView", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersView, orderDateColumn.Table);

            var customerType = model.Model.FindEntityType(typeof(Customer));
            var customerView = customerType.GetViewMappings().Single().View;
            Assert.Equal("CustomerView", customerView.Name);
            Assert.Equal("viewSchema", customerView.Schema);

            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));
            var customerPk = specialCustomerType.FindPrimaryKey();

            if (mapping == Mapping.TPT)
            {
                Assert.Equal(2, specialCustomerType.GetViewMappings().Count());
                Assert.True(specialCustomerType.GetViewMappings().First().IsMainTableMapping);
                Assert.False(specialCustomerType.GetViewMappings().Last().IsMainTableMapping);

                var specialCustomerView = specialCustomerType.GetViewMappings().Select(t => t.Table).First(t => t.Name == "SpecialCustomerView");
                Assert.Null(specialCustomerView.Schema);
                Assert.Equal(3, specialCustomerView.Columns.Count());

                Assert.True(specialCustomerView.EntityTypeMappings.Single().IsMainEntityTypeMapping);

                var specialityColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.False(specialityColumn.IsNullable);

                Assert.Null(customerType.GetDiscriminatorProperty());
                Assert.Null(customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.GetDiscriminatorProperty());
                Assert.Null(specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerViewMapping = specialCustomerType.GetViewMappings().Single();
                Assert.True(specialCustomerViewMapping.IsMainTableMapping);

                var specialCustomerView = specialCustomerViewMapping.View;
                Assert.Same(customerView, specialCustomerView);

                Assert.Equal(2, specialCustomerView.EntityTypeMappings.Count());
                Assert.True(specialCustomerView.EntityTypeMappings.First().IsMainEntityTypeMapping);
                Assert.False(specialCustomerView.EntityTypeMappings.Last().IsMainEntityTypeMapping);

                var specialityColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.True(specialityColumn.IsNullable);
            }
        }

        private static void AssertTables(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetTableMappings().Single();
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate), nameof(Order.OrderId) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersTable = orderMapping.Table;
            Assert.Same(ordersTable, model.FindTable(ordersTable.Name, ordersTable.Schema));
            Assert.Equal(
                new[] { nameof(Order), "OrderDetails.BillingAddress#Address", "OrderDetails.ShippingAddress#Address", nameof(OrderDetails) },
                ordersTable.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(new[] {
                    nameof(Order.AlternateId),
                    nameof(Order.CustomerId),
                    "Details_BillingAddress_City",
                    "Details_BillingAddress_Street",
                    "Details_ShippingAddress_City",
                    "Details_ShippingAddress_Street",
                    nameof(Order.OrderDate),
                    nameof(Order.OrderId)
            },
                ordersTable.Columns.Select(m => m.Name));
            Assert.Equal("Order", ordersTable.Name);
            Assert.Null(ordersTable.Schema);
            Assert.False(ordersTable.IsExcludedFromMigrations);
            Assert.True(ordersTable.IsShared);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            Assert.False(orderDate.IsColumnNullable());

            var orderDateMapping = orderDate.GetTableColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.TableMapping);

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersTable.FindColumn("OrderDate"));
            Assert.Same(orderDateColumn, orderDate.FindTableColumn(ordersTable.Name, ordersTable.Schema));
            Assert.Equal("OrderDate", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersTable, orderDateColumn.Table);

            var orderPk = orderType.FindPrimaryKey();
            var orderPkConstraint = orderPk.GetMappedConstraints().Single();

            Assert.Equal("PK_Order", orderPkConstraint.Name);
            Assert.Equal(nameof(Order.OrderId), orderPkConstraint.Columns.Single().Name);
            Assert.Same(ordersTable, orderPkConstraint.Table);
            Assert.True(orderPkConstraint.GetIsPrimaryKey());
            Assert.Same(orderPkConstraint, ordersTable.UniqueConstraints.Last());
            Assert.Same(orderPkConstraint, ordersTable.PrimaryKey);

            var orderAk = orderType.GetKeys().Single(k => k != orderPk);
            var orderAkConstraint = orderAk.GetMappedConstraints().Single();

            Assert.Equal("AK_AlternateId", orderAkConstraint.Name);
            Assert.Equal(nameof(Order.AlternateId), orderAkConstraint.Columns.Single().Name);
            Assert.Same(ordersTable, orderAkConstraint.Table);
            Assert.False(orderAkConstraint.GetIsPrimaryKey());
            Assert.Same(orderAkConstraint, ordersTable.UniqueConstraints.First());

            var orderDateIndex = orderType.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(Order.OrderDate)));
            var orderDateTableIndex = orderDateIndex.GetMappedTableIndexes().Single();

            Assert.Equal("IX_OrderDate", orderDateTableIndex.Name);
            Assert.Equal(nameof(Order.OrderDate), orderDateTableIndex.Columns.Single().Name);
            Assert.Same(ordersTable, orderDateTableIndex.Table);
            Assert.True(orderDateTableIndex.IsUnique);
            Assert.Null(orderDateTableIndex.Filter);
            Assert.Equal(orderDateTableIndex, ordersTable.Indexes.Last());

            var orderCustomerIndex = orderType.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(Order.CustomerId)));
            var orderTableIndex = orderCustomerIndex.GetMappedTableIndexes().Single();

            Assert.Equal("IX_Order_CustomerId", orderTableIndex.Name);
            Assert.Equal(nameof(Order.CustomerId), orderTableIndex.Columns.Single().Name);
            Assert.Same(ordersTable, orderTableIndex.Table);
            Assert.False(orderTableIndex.IsUnique);
            Assert.Null(orderTableIndex.Filter);
            Assert.Equal(orderCustomerIndex, orderTableIndex.MappedIndexes.Single());
            Assert.Same(orderTableIndex, ordersTable.Indexes.First());

            var orderDateFk = orderType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(DateDetails));
            var orderDateFkConstraint = orderDateFk.GetMappedConstraints().Single();

            Assert.Equal("FK_DateDetails", orderDateFkConstraint.Name);
            Assert.Equal(nameof(Order.OrderDate), orderDateFkConstraint.Columns.Single().Name);
            Assert.Equal(nameof(DateDetails.Date), orderDateFkConstraint.PrincipalColumns.Single().Name);
            Assert.Equal("DateDetails", orderDateFkConstraint.PrincipalTable.Name);

            var orderCustomerFk = orderType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));
            var orderCustomerFkConstraint = orderCustomerFk.GetMappedConstraints().Single();

            Assert.Equal("FK_Order_Customer_CustomerId", orderCustomerFkConstraint.Name);
            Assert.Equal(nameof(Order.CustomerId), orderCustomerFkConstraint.Columns.Single().Name);
            Assert.Equal(nameof(Customer.Id), orderCustomerFkConstraint.PrincipalColumns.Single().Name);
            Assert.Same(ordersTable, orderCustomerFkConstraint.Table);
            Assert.Equal("Customer", orderCustomerFkConstraint.PrincipalTable.Name);
            Assert.Equal(ReferentialAction.Cascade, orderCustomerFkConstraint.OnDeleteAction);
            Assert.Equal(orderCustomerFk, orderCustomerFkConstraint.MappedForeignKeys.Single());
            Assert.Equal(new[] { orderDateFkConstraint, orderCustomerFkConstraint }, ordersTable.ForeignKeyConstraints);

            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersTable, orderDetailsType.GetTableMappings().Single().Table);
            Assert.Equal(ordersTable.GetReferencingRowInternalForeignKeys(orderType), ordersTable.GetRowInternalForeignKeys(orderDetailsType));
            Assert.Empty(orderDetailsOwnership.GetMappedConstraints());
            Assert.Equal(2, orderDetailsType.GetForeignKeys().Count());

            var orderDetailsDateIndex = orderDetailsType.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(Order.OrderDate)));
            var orderDetailsDateTableIndex = orderDetailsDateIndex.GetMappedTableIndexes().Single();
            Assert.Same(orderDateTableIndex, orderDetailsDateTableIndex);
            Assert.Equal(new[] { orderDateIndex, orderDetailsDateIndex }, orderDateTableIndex.MappedIndexes);

            var orderDetailsPk = orderDetailsType.FindPrimaryKey();
            Assert.Same(orderPkConstraint, orderDetailsPk.GetMappedConstraints().Single());

            var billingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.BillingAddress)).ForeignKey;
            var billingAddressType = billingAddressOwnership.DeclaringEntityType;

            var shippingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.ShippingAddress)).ForeignKey;
            var shippingAddressType = shippingAddressOwnership.DeclaringEntityType;

            Assert.Equal(
                new[] { billingAddressType.FindPrimaryKey(), shippingAddressType.FindPrimaryKey(), orderPk, orderDetailsPk },
                orderPkConstraint.MappedKeys);

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
            Assert.True(orderDetailsDate.IsColumnNullable());
            Assert.Equal(new[] { orderDate, orderDetailsDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));

            var orderDetailsAk = orderDetailsType.GetKeys().Single(k => k != orderDetailsPk);
            var orderDetailsAkConstraint = orderDetailsAk.GetMappedConstraints().Single();
            Assert.Same(orderAkConstraint, orderDetailsAkConstraint);
            Assert.Equal(new[] { orderAk, orderDetailsAk }, orderAkConstraint.MappedKeys);

            var orderDetailsDateFk = orderDetailsType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(DateDetails));
            var orderDetailsDateFkConstraint = orderDateFk.GetMappedConstraints().Single();
            Assert.Same(orderDateFkConstraint, orderDetailsDateFkConstraint);
            Assert.Equal(new[] { orderDateFk, orderDetailsDateFk }, orderDateFkConstraint.MappedForeignKeys);

            Assert.Equal("FK_DateDetails", orderDateFkConstraint.Name);

            var customerType = model.Model.FindEntityType(typeof(Customer));
            var customerTable = customerType.GetTableMappings().Single().Table;
            Assert.Equal("Customer", customerTable.Name);

            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));
            var customerPk = specialCustomerType.FindPrimaryKey();

            if (mapping == Mapping.TPT)
            {
                Assert.Equal(2, specialCustomerType.GetTableMappings().Count());
                Assert.True(specialCustomerType.GetTableMappings().First().IsMainTableMapping);
                Assert.False(specialCustomerType.GetTableMappings().Last().IsMainTableMapping);

                var specialCustomerTable = specialCustomerType.GetTableMappings().Select(t => t.Table).First(t => t.Name == "SpecialCustomer");
                Assert.Equal("SpecialSchema", specialCustomerTable.Schema);
                Assert.Equal(3, specialCustomerTable.Columns.Count());

                Assert.True(specialCustomerTable.EntityTypeMappings.Single().IsMainEntityTypeMapping);

                var specialityColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.False(specialityColumn.IsNullable);

                Assert.Equal(2, customerPk.GetMappedConstraints().Count());
                var specialCustomerPkConstraint = specialCustomerTable.PrimaryKey;
                Assert.Equal("PK_SpecialCustomer", specialCustomerPkConstraint.Name);
                Assert.Same(specialCustomerPkConstraint.MappedKeys.Single(), customerPk);

                var idProperty = customerPk.Properties.Single();
                Assert.Equal(3, idProperty.GetTableColumnMappings().Count());

                Assert.Empty(customerTable.ForeignKeyConstraints);

                var specialCustomerUniqueConstraint = customerTable.UniqueConstraints.Single(c => !c.GetIsPrimaryKey());
                Assert.Equal("AK_Customer_SpecialityAk", specialCustomerUniqueConstraint.Name);
                Assert.NotNull(specialCustomerUniqueConstraint.MappedKeys.Single());

                var specialCustomerFkConstraint = specialCustomerTable.ForeignKeyConstraints.Single();
                Assert.Equal("FK_SpecialCustomer_Customer_RelatedCustomerSpeciality", specialCustomerFkConstraint.Name);
                Assert.Same(customerTable, specialCustomerFkConstraint.PrincipalTable);
                Assert.NotNull(specialCustomerFkConstraint.MappedForeignKeys.Single());

                var specialCustomerDbIndex = specialCustomerTable.Indexes.Single();
                Assert.Equal("IX_SpecialCustomer_RelatedCustomerSpeciality", specialCustomerDbIndex.Name);
                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());

                Assert.Null(customerType.GetDiscriminatorProperty());
                Assert.Null(customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.GetDiscriminatorProperty());
                Assert.Null(specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerTypeMapping = specialCustomerType.GetTableMappings().Single();
                Assert.True(specialCustomerTypeMapping.IsMainTableMapping);

                var specialCustomerTable = specialCustomerTypeMapping.Table;
                Assert.Same(customerTable, specialCustomerTable);

                Assert.Equal(2, specialCustomerTable.EntityTypeMappings.Count());
                Assert.True(specialCustomerTable.EntityTypeMappings.First().IsMainEntityTypeMapping);
                Assert.False(specialCustomerTable.EntityTypeMappings.Last().IsMainEntityTypeMapping);

                var specialityColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.True(specialityColumn.IsNullable);

                var specialCustomerPkConstraint = specialCustomerTable.PrimaryKey;
                Assert.Equal("PK_Customer", specialCustomerPkConstraint.Name);
                Assert.Same(specialCustomerPkConstraint.MappedKeys.Single(), customerPk);

                var idProperty = customerPk.Properties.Single();
                Assert.Equal(2, idProperty.GetTableColumnMappings().Count());

                var specialCustomerUniqueConstraint = specialCustomerTable.UniqueConstraints.Single(c => !c.GetIsPrimaryKey());
                Assert.Equal("AK_Customer_SpecialityAk", specialCustomerUniqueConstraint.Name);
                Assert.NotNull(specialCustomerUniqueConstraint.MappedKeys.Single());

                var specialCustomerFkConstraint = specialCustomerTable.ForeignKeyConstraints.Single();
                Assert.Equal("FK_Customer_Customer_RelatedCustomerSpeciality", specialCustomerFkConstraint.Name);
                Assert.NotNull(specialCustomerFkConstraint.MappedForeignKeys.Single());

                var specialCustomerDbIndex = specialCustomerTable.Indexes.Single();
                Assert.Equal("IX_Customer_RelatedCustomerSpeciality", specialCustomerDbIndex.Name);
                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());
            }
        }

        private IRelationalModel CreateTestModel(bool mapToTables = false, bool mapToViews = false, Mapping mapping = Mapping.TPH)
        {
            var modelBuilder = CreateConventionModelBuilder();
            modelBuilder.Entity<Customer>(cb =>
            {
                if (mapToViews)
                {
                    cb.ToView("CustomerView", "viewSchema");
                }

                if (mapToTables)
                {
                    cb.ToTable("Customer");
                }

                cb.Property<string>("SpecialityAk");
            });

            modelBuilder.Entity<SpecialCustomer>(cb =>
            {
                if (mapToViews
                    && mapping == Mapping.TPT)
                {
                    cb.ToView("SpecialCustomerView");
                }

                if (mapToTables
                    && mapping == Mapping.TPT)
                {
                    cb.ToTable("SpecialCustomer", "SpecialSchema");
                }

                cb.Property(s => s.Speciality).IsRequired();

                cb.HasOne(c => c.RelatedCustomer).WithOne()
                    .HasForeignKey<SpecialCustomer>(c => c.RelatedCustomerSpeciality)
                    .HasPrincipalKey<SpecialCustomer>("SpecialityAk"); // TODO: Use the derived one, #2611
            });

            modelBuilder.Entity<Order>(ob =>
            {
                ob.Property(o => o.OrderDate).HasColumnName("OrderDate").HasViewColumnName("OrderDateView");
                ob.Property(o => o.AlternateId).HasColumnName("AlternateId");

                ob.HasAlternateKey(o => o.AlternateId).HasName("AK_AlternateId");
                ob.HasOne(o => o.DateDetails).WithOne()
                    .HasForeignKey<Order>(o => o.OrderDate).HasPrincipalKey<DateDetails>(o => o.Date)
                    .HasConstraintName("FK_DateDetails");

                // Note: the below is resetting the name of the anonymous index
                // created in HasForeignKey() above, not creating a new index.
                ob.HasIndex(o => o.OrderDate).HasDatabaseName("IX_OrderDate");

                ob.OwnsOne(o => o.Details, odb =>
                {
                    odb.Property(od => od.OrderDate).HasColumnName("OrderDate").HasViewColumnName("OrderDateView");
                    var alternateId = odb.Property(o => o.AlternateId).HasColumnName("AlternateId").HasViewColumnName("AlternateId").Metadata;

                    odb.OwnedEntityType.AddKey(new[] { alternateId });
                    //odb.HasAlternateKey(o => o.AlternateId);
                    odb.HasOne(od => od.DateDetails).WithOne()
                        .HasForeignKey<OrderDetails>(o => o.OrderDate).HasPrincipalKey<DateDetails>(o => o.Date);

                    odb.OwnsOne(od => od.BillingAddress);
                    odb.OwnsOne(od => od.ShippingAddress);
                });

                if (mapToViews)
                {
                    ob.ToView("OrderView", "viewSchema");
                }

                if (mapToTables)
                {
                    ob.ToTable("Order");
                }
            });

            modelBuilder.Entity<DateDetails>(db =>
            {
                db.HasKey(d => d.Date);

                if (mapToViews)
                {
                    db.ToView("DateDetailsView", "viewSchema");
                }

                if (mapToTables)
                {
                    db.ToTable("DateDetails");
                }
            });

            return modelBuilder.FinalizeModel().GetRelationalModel();
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_keyless_TPH()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Entity<Customer>(cb =>
            {
                cb.Ignore(c => c.Orders);
                cb.ToView("CustomerView");
            });

            modelBuilder.Entity<SpecialCustomer>(cb =>
            {
                cb.Property(s => s.Speciality).IsRequired();
            });

            var model = modelBuilder.FinalizeModel().GetRelationalModel();

            Assert.Equal(2, model.Model.GetEntityTypes().Count());
            Assert.Empty(model.Tables);
            Assert.Single(model.Views);

            var customerType = model.Model.FindEntityType(typeof(Customer));
            Assert.NotNull(customerType.GetDiscriminatorProperty());

            var customerView = customerType.GetViewMappings().Single().View;
            Assert.Equal("CustomerView", customerView.Name);

            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));

            var specialCustomerTypeMapping = specialCustomerType.GetViewMappings().Single();
            Assert.True(specialCustomerTypeMapping.IsMainTableMapping);

            var specialCustomerView = specialCustomerTypeMapping.View;
            Assert.Same(customerView, specialCustomerView);

            Assert.Equal(2, specialCustomerView.EntityTypeMappings.Count());
            Assert.True(specialCustomerView.EntityTypeMappings.First().IsMainEntityTypeMapping);
            Assert.False(specialCustomerView.EntityTypeMappings.Last().IsMainEntityTypeMapping);

            var specialityColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
            Assert.True(specialityColumn.IsNullable);
        }

        protected virtual ModelBuilder CreateConventionModelBuilder() => RelationalTestHelpers.Instance.CreateConventionBuilder();

        public enum Mapping
        {
            TPH,
            TPT,
            TPC
        }

        private enum MyEnum : ulong
        {
            Sun,
            Mon,
            Tue
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public short SomeShort { get; set; }
            public MyEnum EnumValue { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
            public string Speciality { get; set; }
            public string RelatedCustomerSpeciality { get; set; }
            public SpecialCustomer RelatedCustomer { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public Guid AlternateId { get; set; }

            public DateTime OrderDate { get; set; }
            public DateDetails DateDetails { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int OrderId { get; set; }
            public Order Order { get; set; }
            public Guid AlternateId { get; set; }

            public DateTime OrderDate { get; set; }
            public DateDetails DateDetails { get; set; }

            public Address BillingAddress { get; set; }
            public Address ShippingAddress { get; set; }
        }

        private class DateDetails
        {
            public DateTime Date { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
        }
    }
}
