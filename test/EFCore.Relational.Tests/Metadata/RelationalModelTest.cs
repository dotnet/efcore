// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NameSpace1;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalModelTest
    {
        [ConditionalFact]
        public void GetRelationalModel_throws_if_convention_has_not_run()
        {
            var modelBuilder = CreateConventionModelBuilder();

            Assert.Equal(
                CoreStrings.ModelNotFinalized("GetRelationalModel"),
                Assert.Throws<InvalidOperationException>(
                    () => ((IModel)modelBuilder.Model).GetRelationalModel()).Message);
        }

        [ConditionalTheory]
        [InlineData(true, Mapping.TPH)]
        [InlineData(true, Mapping.TPT)]
        [InlineData(true, Mapping.TPC)]
        [InlineData(false, Mapping.TPH)]
        [InlineData(false, Mapping.TPT)]
        [InlineData(false, Mapping.TPC)]
        public void Can_use_relational_model_with_tables(bool useExplicitMapping, Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: useExplicitMapping, mapping: mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());
            Assert.Equal(mapping switch
            {
                Mapping.TPC => 5,
                Mapping.TPH => 3,
                _ => 6
            }, model.Tables.Count());
            Assert.Empty(model.Views);
            Assert.True(model.Model.GetEntityTypes().All(et => !et.GetViewMappings().Any()));

            AssertDefaultMappings(model, mapping);
            AssertTables(model, mapping);
        }

        [ConditionalTheory]
        [InlineData(Mapping.TPH)]
        [InlineData(Mapping.TPT)]
        [InlineData(Mapping.TPC)]
        public void Can_use_relational_model_with_views(Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: false, mapToViews: true, mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());
            Assert.Equal(mapping switch
            {
                Mapping.TPC => 5,
                Mapping.TPH => 3,
                _ => 6
            }, model.Views.Count());
            Assert.Empty(model.Tables);
            Assert.True(model.Model.GetEntityTypes().All(et => !et.GetTableMappings().Any()));

            AssertDefaultMappings(model, mapping);
            AssertViews(model, mapping);
        }

        [ConditionalTheory]
        [InlineData(Mapping.TPH)]
        [InlineData(Mapping.TPT)]
        [InlineData(Mapping.TPC)]
        public void Can_use_relational_model_with_views_and_tables(Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: true, mapToViews: true, mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());
            Assert.Equal(mapping switch
            {
                Mapping.TPC => 5,
                Mapping.TPH => 3,
                _ => 6
            }, model.Tables.Count());
            Assert.Equal(mapping switch
            {
                Mapping.TPC => 5,
                Mapping.TPH => 3,
                _ => 6
            }, model.Views.Count());

            AssertDefaultMappings(model, mapping);
            AssertTables(model, mapping);
            AssertViews(model, mapping);
        }

        private static void AssertDefaultMappings(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetDefaultMappings().Single();
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.Id), nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersTable = orderMapping.Table;
            Assert.Equal(new[] { nameof(Order) }, ordersTable.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.Id), "OrderDate" },
                ordersTable.Columns.Select(m => m.Name));
            Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.RelationalModelTest+Order", ordersTable.Name);
            Assert.Null(ordersTable.Schema);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));

            var orderDateMapping = orderDate.GetDefaultColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.TableMapping);

            var abstractBaseType = model.Model.FindEntityType(typeof(AbstractBase));
            var abstractCustomerType = model.Model.FindEntityType(typeof(AbstractCustomer));
            var customerType = model.Model.FindEntityType(typeof(Customer));
            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));
            var extraSpecialCustomerType = model.Model.FindEntityType(typeof(ExtraSpecialCustomer));
            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            var orderDetailsTable = orderDetailsType.GetDefaultMappings().Single().Table;
            Assert.NotEqual(ordersTable, orderDetailsTable);
            Assert.Empty(ordersTable.GetReferencingRowInternalForeignKeys(orderType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersTable.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersTable.IsOptional(specialCustomerType)).Message);

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersTable.FindColumn("OrderDate"));
            Assert.Same(orderDateColumn, ordersTable.FindColumn(orderDate));
            Assert.Equal(new[] { orderDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal("OrderDate", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersTable, orderDateColumn.Table);

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
            Assert.Equal(new[] { orderDetailsDate }, orderDetailsTable.FindColumn("OrderDate").PropertyMappings.Select(m => m.Property));

            var customerTable = customerType.GetDefaultMappings().Last().Table;
            Assert.False(customerTable.IsOptional(customerType));
            if (mapping == Mapping.TPC)
            {
                Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerTable.Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerTable.IsOptional(specialCustomerType)).Message);
            }
            else
            {
                Assert.False(customerTable.IsOptional(specialCustomerType));
                Assert.False(customerTable.IsOptional(extraSpecialCustomerType));
            }

            if (mapping == Mapping.TPT)
            {
                Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.RelationalModelTest+Customer", customerTable.Name);
                Assert.Null(customerTable.Schema);
                Assert.Equal(4, specialCustomerType.GetDefaultMappings().Count());
                Assert.True(specialCustomerType.GetDefaultMappings().First().IsSplitEntityTypePrincipal);
                Assert.False(specialCustomerType.GetDefaultMappings().First().IncludesDerivedTypes);
                Assert.True(specialCustomerType.GetDefaultMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetDefaultMappings().Last().IncludesDerivedTypes);

                var specialCustomerTable = specialCustomerType.GetDefaultMappings().Last().Table;
                Assert.Null(specialCustomerTable.Schema);
                Assert.Equal(4, specialCustomerTable.Columns.Count());

                Assert.True(specialCustomerTable.EntityTypeMappings.Single(m => m.EntityType == specialCustomerType).IsSharedTablePrincipal);

                var specialityColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.False(specialityColumn.IsNullable);

                Assert.Null(customerType.FindDiscriminatorProperty());
                Assert.Null(customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.FindDiscriminatorProperty());
                Assert.Null(specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerTableMapping = specialCustomerType.GetDefaultMappings().Single();
                Assert.True(specialCustomerTableMapping.IsSplitEntityTypePrincipal);
                var specialCustomerTable = specialCustomerTableMapping.Table;
                var specialityColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                if (mapping == Mapping.TPH)
                {
                    var baseTable = abstractBaseType.GetDefaultMappings().Single().Table;
                    Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.RelationalModelTest+AbstractBase", baseTable.Name);
                    Assert.Equal(baseTable.Name, customerTable.Name);
                    Assert.Equal(baseTable.Schema, customerTable.Schema);
                    Assert.True(specialCustomerTableMapping.IncludesDerivedTypes);
                    Assert.Same(customerTable, specialCustomerTable);

                    Assert.Equal(5, specialCustomerTable.EntityTypeMappings.Count());
                    Assert.True(specialCustomerTable.EntityTypeMappings.All(t => t.IsSharedTablePrincipal));

                    Assert.Equal(10, specialCustomerTable.Columns.Count());

                    Assert.True(specialityColumn.IsNullable);
                }
                else
                {
                    Assert.False(specialCustomerTableMapping.IncludesDerivedTypes);
                    Assert.NotSame(customerTable, specialCustomerTable);

                    Assert.True(customerTable.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Equal(5, customerTable.Columns.Count());

                    Assert.True(specialCustomerTable.EntityTypeMappings.Single().IsSharedTablePrincipal);

                    Assert.Equal(9, specialCustomerTable.Columns.Count());

                    Assert.False(specialityColumn.IsNullable);
                }
            }
        }

        private static void AssertViews(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetViewMappings().Single();
            Assert.Equal(orderType.GetViewMappings(), orderType.GetViewOrTableMappings());
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.Id), nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersView = orderMapping.View;
            Assert.Same(ordersView, model.FindView(ordersView.Name, ordersView.Schema));
            Assert.Equal(
                new[]
                {
                    nameof(Order), nameof(OrderDetails), "OrderDetails.BillingAddress#Address", "OrderDetails.ShippingAddress#Address"
                },
                ordersView.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(
                new[]
                {
                    nameof(Order.AlternateId),
                    nameof(Order.CustomerId),
                    "Details_Active",
                    "Details_BillingAddress_City",
                    "Details_BillingAddress_Street",
                    "Details_ShippingAddress_City",
                    "Details_ShippingAddress_Street",
                    nameof(Order.Id),
                    "OrderDate"
                },
                ordersView.Columns.Select(m => m.Name));
            Assert.Equal("OrderView", ordersView.Name);
            Assert.Equal("viewSchema", ordersView.Schema);
            Assert.Null(ordersView.ViewDefinitionSql);

            var orderPk = orderType.FindPrimaryKey();

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            var orderDateMapping = orderDate.GetViewColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.ViewMapping);

            var abstractBaseType = model.Model.FindEntityType(typeof(AbstractBase));
            var abstractCustomerType = model.Model.FindEntityType(typeof(AbstractCustomer));
            var customerType = model.Model.FindEntityType(typeof(Customer));
            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));
            var extraSpecialCustomerType = model.Model.FindEntityType(typeof(ExtraSpecialCustomer));
            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersView, orderDetailsType.GetViewMappings().Single().View);
            Assert.Equal(
                ordersView.GetReferencingRowInternalForeignKeys(orderType), ordersView.GetRowInternalForeignKeys(orderDetailsType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersView.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersView.GetReferencingRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersView.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersView.GetRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.False(ordersView.IsOptional(orderType));
            Assert.True(ordersView.IsOptional(orderDetailsType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersView.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersView.IsOptional(specialCustomerType)).Message);

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersView.FindColumn("OrderDate"));
            Assert.Same(orderDateColumn, orderDate.FindColumn(StoreObjectIdentifier.View(ordersView.Name, ordersView.Schema)));
            Assert.Same(orderDateColumn, ordersView.FindColumn(orderDate));

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
            Assert.Equal(new[] { orderDate, orderDetailsDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal("OrderDate", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersView, orderDateColumn.Table);

            var customerView = customerType.GetViewMappings().Last().View;
            Assert.False(customerView.IsOptional(customerType));
            if (mapping == Mapping.TPC)
            {
                Assert.Equal(
                    RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerView.Name),
                    Assert.Throws<InvalidOperationException>(
                        () => customerView.IsOptional(specialCustomerType)).Message);
            }
            else
            {
                Assert.False(customerView.IsOptional(specialCustomerType));
                Assert.False(customerView.IsOptional(extraSpecialCustomerType));
            }

            var baseTableName = mapping == Mapping.TPH
                ? abstractBaseType.GetTableName()
                : customerType.GetTableName();
            var mappedToTable = baseTableName != null;
            var ordersCustomerForeignKey = orderType.FindNavigation(nameof(Order.Customer)).ForeignKey;
            Assert.Equal(mappedToTable && mapping != Mapping.TPC
                ? "FK_Order_" + baseTableName + "_CustomerId"
                : null, ordersCustomerForeignKey.GetConstraintName());
            Assert.Null(ordersCustomerForeignKey.GetConstraintName(
                StoreObjectIdentifier.View(ordersView.Name, ordersView.Schema),
                StoreObjectIdentifier.View(customerView.Name, customerView.Schema)));
            Assert.Equal(mappedToTable && mapping != Mapping.TPC
                ? "FK_Order_" + baseTableName + "_CustomerId"
                : null, ordersCustomerForeignKey.GetDefaultName());
            Assert.Null(ordersCustomerForeignKey.GetDefaultName(
                StoreObjectIdentifier.View(ordersView.Name, ordersView.Schema),
                StoreObjectIdentifier.View(customerView.Name, customerView.Schema)));

            var ordersCustomerIndex = orderType.FindIndex(ordersCustomerForeignKey.Properties);
            Assert.Equal(mappedToTable
                ? "IX_Order_CustomerId"
                : null, ordersCustomerIndex.GetDatabaseName());
            Assert.Null(ordersCustomerIndex.GetDatabaseName(
                StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
            Assert.Equal(mappedToTable
                ? "IX_Order_CustomerId"
                : null, ordersCustomerIndex.GetDefaultDatabaseName());
            Assert.Null(ordersCustomerIndex.GetDefaultDatabaseName(
                StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));

            var specialityCK = specialCustomerType.GetCheckConstraints().Single();
            Assert.Equal(mappedToTable
                ? "Speciality"
                : null, specialityCK.Name);
            Assert.Null(specialityCK.GetName(
                StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
            Assert.Equal(mappedToTable
                ? "Speciality"
                : null, specialityCK.GetDefaultName());
            Assert.Equal("Speciality", specialityCK.GetDefaultName(
                StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));

            Assert.Equal(mappedToTable
                ? "PK_Order"
                : null, orderPk.GetName());
            Assert.Null(orderPk.GetName(
                StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
            Assert.Equal(mappedToTable
                ? "PK_Order"
                : null, orderPk.GetDefaultName());
            Assert.Equal("PK_OrderView", orderPk.GetDefaultName(
                StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));

            if (mapping == Mapping.TPT)
            {
                Assert.Equal("CustomerView", customerView.Name);
                Assert.Equal("viewSchema", customerView.Schema);
                Assert.Equal(3, specialCustomerType.GetViewMappings().Count());
                Assert.True(specialCustomerType.GetViewMappings().First().IsSplitEntityTypePrincipal);
                Assert.False(specialCustomerType.GetViewMappings().First().IncludesDerivedTypes);
                Assert.True(specialCustomerType.GetViewMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetViewMappings().Last().IncludesDerivedTypes);

                var specialCustomerView = specialCustomerType.GetViewMappings().Select(t => t.Table)
                    .First(t => t.Name == "SpecialCustomerView");
                Assert.Null(specialCustomerView.Schema);
                Assert.Equal(6, specialCustomerView.Columns.Count());

                Assert.True(specialCustomerView.EntityTypeMappings.Single(m => m.EntityType == specialCustomerType).IsSharedTablePrincipal);

                var specialityColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.False(specialityColumn.IsNullable);

                Assert.Null(customerType.FindDiscriminatorProperty());
                Assert.Null(customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.FindDiscriminatorProperty());
                Assert.Null(specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerViewMapping = specialCustomerType.GetViewMappings().Single();
                Assert.True(specialCustomerViewMapping.IsSplitEntityTypePrincipal);
                var specialCustomerView = specialCustomerViewMapping.View;
                var specialityColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                if (mapping == Mapping.TPH)
                {
                    var baseView = abstractBaseType.GetViewMappings().Single().Table;
                    Assert.Equal("BaseView", baseView.Name);
                    Assert.Equal(baseView.Name, abstractBaseType.GetViewName());
                    Assert.Equal(baseView.Name, customerView.Name);
                    Assert.Equal(baseView.Schema, customerView.Schema);
                    Assert.True(specialCustomerViewMapping.IncludesDerivedTypes);
                    Assert.Same(customerView, specialCustomerView);

                    Assert.Equal(6, specialCustomerView.EntityTypeMappings.Count());
                    Assert.True(specialCustomerView.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.False(specialCustomerView.EntityTypeMappings.Last().IsSharedTablePrincipal);

                    Assert.True(specialityColumn.IsNullable);
                }
                else
                {
                    Assert.False(specialCustomerViewMapping.IncludesDerivedTypes);
                    Assert.NotSame(customerView, specialCustomerView);

                    Assert.True(customerView.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Equal(5, customerView.Columns.Count());

                    Assert.Equal(2, specialCustomerView.EntityTypeMappings.Count());
                    Assert.True(specialCustomerView.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.False(specialCustomerView.EntityTypeMappings.Last().IsSharedTablePrincipal);

                    Assert.Equal(10, specialCustomerView.Columns.Count());

                    Assert.False(specialityColumn.IsNullable);
                }
            }
        }

        private static void AssertTables(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetTableMappings().Single();
            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.Id), nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersTable = orderMapping.Table;
            Assert.Same(ordersTable, model.FindTable(ordersTable.Name, ordersTable.Schema));
            Assert.Equal(
                new[]
                {
                    nameof(Order), nameof(OrderDetails), "OrderDetails.BillingAddress#Address", "OrderDetails.ShippingAddress#Address"
                },
                ordersTable.EntityTypeMappings.Select(m => m.EntityType.DisplayName()));
            Assert.Equal(
                new[]
                {
                    nameof(Order.Id),
                    nameof(Order.AlternateId),
                    nameof(Order.CustomerId),
                    "Details_Active",
                    "Details_BillingAddress_City",
                    "Details_BillingAddress_Street",
                    "Details_ShippingAddress_City",
                    "Details_ShippingAddress_Street",
                    nameof(Order.OrderDate)
                },
                ordersTable.Columns.Select(m => m.Name));
            Assert.Equal("Order", ordersTable.Name);
            Assert.Null(ordersTable.Schema);
            Assert.False(ordersTable.IsExcludedFromMigrations);
            Assert.True(ordersTable.IsShared);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));

            var orderDateMapping = orderDate.GetTableColumnMappings().Single();
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.TableMapping);

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersTable.FindColumn("OrderDate"));
            Assert.Same(orderDateColumn, orderDate.FindColumn(StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));
            Assert.Same(orderDateColumn, ordersTable.FindColumn(orderDate));
            Assert.Equal("OrderDate", orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersTable, orderDateColumn.Table);
            Assert.Same(orderDateMapping, orderDateColumn.FindColumnMapping(orderType));

            var orderPk = orderType.FindPrimaryKey();
            var orderPkConstraint = orderPk.GetMappedConstraints().Single();

            Assert.Equal("PK_Order", orderPkConstraint.Name);
            Assert.Equal(nameof(Order.Id), orderPkConstraint.Columns.Single().Name);
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
            Assert.Equal("PK_DateDetails", orderDateFkConstraint.PrincipalUniqueConstraint.Name);
            Assert.Equal("DateDetails", orderDateFkConstraint.PrincipalTable.Name);

            var orderCustomerFk = orderType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            var abstractBaseType = model.Model.FindEntityType(typeof(AbstractBase));
            var abstractCustomerType = model.Model.FindEntityType(typeof(AbstractCustomer));
            var customerType = model.Model.FindEntityType(typeof(Customer));
            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));
            var extraSpecialCustomerType = model.Model.FindEntityType(typeof(ExtraSpecialCustomer));
            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersTable, orderDetailsType.GetTableMappings().Single().Table);
            Assert.Equal(
                ordersTable.GetReferencingRowInternalForeignKeys(orderType), ordersTable.GetRowInternalForeignKeys(orderDetailsType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersTable.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersTable.GetReferencingRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersTable.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersTable.GetRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.False(ordersTable.IsOptional(orderType));
            Assert.True(ordersTable.IsOptional(orderDetailsType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersTable.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersTable.IsOptional(specialCustomerType)).Message);
            Assert.Empty(orderDetailsOwnership.GetMappedConstraints());
            Assert.Equal(2, orderDetailsType.GetForeignKeys().Count());

            var orderDetailsDateIndex = orderDetailsType.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(Order.OrderDate)));
            var orderDetailsDateTableIndex = orderDetailsDateIndex.GetMappedTableIndexes().Single();
            Assert.Same(orderDateTableIndex, orderDetailsDateTableIndex);
            Assert.Equal(new[] { orderDateIndex, orderDetailsDateIndex }, orderDateTableIndex.MappedIndexes);

            var orderDetailsPk = orderDetailsType.FindPrimaryKey();
            Assert.Same(orderPkConstraint, orderDetailsPk.GetMappedConstraints().Single());

            var orderDetailsPkProperty = orderDetailsPk.Properties.Single();
            Assert.Equal("OrderId", orderDetailsPkProperty.GetColumnBaseName());

            var billingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.BillingAddress)).ForeignKey;
            var billingAddressType = billingAddressOwnership.DeclaringEntityType;

            var shippingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.ShippingAddress)).ForeignKey;
            var shippingAddressType = shippingAddressOwnership.DeclaringEntityType;

            Assert.Equal(
                new[] { orderPk, billingAddressType.FindPrimaryKey(), shippingAddressType.FindPrimaryKey(), orderDetailsPk },
                orderPkConstraint.MappedKeys);

            var orderDetailsDate = orderDetailsType.FindProperty(nameof(OrderDetails.OrderDate));
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

            var ordersCustomerIndex = orderType.FindIndex(orderCustomerFk.Properties);
            Assert.Equal("IX_Order_CustomerId", ordersCustomerIndex.GetDatabaseName());
            Assert.Equal("IX_Order_CustomerId", ordersCustomerIndex.GetDatabaseName(
                StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));
            Assert.Equal("IX_Order_CustomerId", ordersCustomerIndex.GetDefaultDatabaseName());
            Assert.Equal("IX_Order_CustomerId", ordersCustomerIndex.GetDefaultDatabaseName(
                StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));
            
            Assert.Equal("PK_Order", orderPk.GetName());
            Assert.Equal("PK_Order", orderPk.GetName(
                StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));
            Assert.Equal("PK_Order", orderPk.GetDefaultName());
            Assert.Equal("PK_Order", orderPk.GetDefaultName(
                StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));

            var specialCustomerTable =
                specialCustomerType.GetTableMappings().Select(t => t.Table).Last();
            var specialityCK = specialCustomerType.GetCheckConstraints().Single();
            Assert.Equal("Speciality", specialityCK.Name);
            Assert.Equal("Speciality", specialityCK.GetName(
                StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema)));
            Assert.Equal("Speciality", specialityCK.GetDefaultName());
            Assert.Equal("Speciality", specialityCK.GetDefaultName(
                StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema)));

            var customerTable = customerType.GetTableMappings().Last().Table;
            Assert.False(customerTable.IsOptional(customerType));
            if (mapping == Mapping.TPC)
            {
                Assert.Equal(
                    RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerTable.Name),
                    Assert.Throws<InvalidOperationException>(
                        () => customerTable.IsOptional(specialCustomerType)).Message);
            }
            else
            {
                Assert.False(customerTable.IsOptional(specialCustomerType));
                Assert.False(customerTable.IsOptional(extraSpecialCustomerType));
            }

            var orderTrigger = Assert.Single(orderType.GetTriggers());
            Assert.Equal("Order_Trigger", orderTrigger.Name);
            Assert.Equal("Order", orderTrigger.TableName);
            Assert.Null(orderTrigger.TableSchema);

            var customerPk = specialCustomerType.FindPrimaryKey();

            if (mapping == Mapping.TPT)
            {
                var baseTable = abstractBaseType.GetTableMappings().Single().Table;
                Assert.Equal("AbstractBase", baseTable.Name);
                Assert.Equal(nameof(Customer), customerTable.Name);
                Assert.Null(abstractCustomerType.GetTableName());
                Assert.Equal(nameof(SpecialCustomer), specialCustomerType.GetTableName());
                Assert.Equal(3, specialCustomerType.GetTableMappings().Count());
                Assert.True(specialCustomerType.GetTableMappings().First().IsSplitEntityTypePrincipal);
                Assert.False(specialCustomerType.GetTableMappings().First().IncludesDerivedTypes);
                Assert.True(specialCustomerType.GetTableMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetTableMappings().Last().IncludesDerivedTypes);

                Assert.Equal("SpecialCustomer", specialCustomerTable.Name);
                Assert.Equal(6, specialCustomerTable.Columns.Count());

                Assert.True(
                    specialCustomerTable.EntityTypeMappings.Single(m => m.EntityType == specialCustomerType).IsSharedTablePrincipal);

                var specialityColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                Assert.False(specialityColumn.IsNullable);

                var addressColumn = specialCustomerTable.Columns.Single(c =>
                    c.Name == nameof(SpecialCustomer.Details) + "_" + nameof(CustomerDetails.Address));
                Assert.False(addressColumn.IsNullable);
                var specialityProperty = specialityColumn.PropertyMappings.First().Property;

                Assert.Equal(
                    RelationalStrings.PropertyNotMappedToTable(
                        nameof(SpecialCustomer.Speciality), nameof(SpecialCustomer), "Customer"),
                    Assert.Throws<InvalidOperationException>(() =>
                        specialityProperty.IsColumnNullable(StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)))
                        .Message);

                var abstractStringColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                Assert.False(specialityColumn.IsNullable);
                Assert.Equal(2, specialityColumn.PropertyMappings.Count());

                var extraSpecialCustomerTable =
                    extraSpecialCustomerType.GetTableMappings().Select(t => t.Table).First(t => t.Name == "ExtraSpecialCustomer");

                Assert.Empty(customerTable.CheckConstraints);
                Assert.Same(specialityCK, specialCustomerTable.CheckConstraints.Single());
                Assert.Empty(extraSpecialCustomerTable.CheckConstraints);

                Assert.Equal(4, customerPk.GetMappedConstraints().Count());
                var specialCustomerPkConstraint = specialCustomerTable.PrimaryKey;
                Assert.Equal("PK_SpecialCustomer", specialCustomerPkConstraint.Name);
                Assert.Same(specialCustomerPkConstraint.MappedKeys.First(), customerPk);

                var idProperty = customerPk.Properties.Single();
                Assert.Equal(10, idProperty.GetTableColumnMappings().Count());

                var customerFk = customerTable.ForeignKeyConstraints.Single();
                Assert.Equal("FK_Customer_AbstractBase_Id", customerFk.Name);
                Assert.NotNull(customerFk.MappedForeignKeys.Single());
                Assert.Same(baseTable, customerFk.PrincipalTable);

                var orderCustomerFkConstraint = orderCustomerFk.GetMappedConstraints().Single();

                Assert.Equal("FK_Order_Customer_CustomerId", orderCustomerFkConstraint.Name);
                Assert.Equal(nameof(Order.CustomerId), orderCustomerFkConstraint.Columns.Single().Name);
                Assert.Equal(nameof(Customer.Id), orderCustomerFkConstraint.PrincipalColumns.Single().Name);
                Assert.Same(ordersTable, orderCustomerFkConstraint.Table);
                Assert.Equal("Customer", orderCustomerFkConstraint.PrincipalTable.Name);
                Assert.Equal(ReferentialAction.Cascade, orderCustomerFkConstraint.OnDeleteAction);
                Assert.Equal(orderCustomerFk, orderCustomerFkConstraint.MappedForeignKeys.Single());
                Assert.Equal(new[] { orderDateFkConstraint, orderCustomerFkConstraint }, ordersTable.ForeignKeyConstraints);
                Assert.Empty(ordersTable.ReferencingForeignKeyConstraints);

                Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetConstraintName());
                Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetConstraintName(
                    StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                    StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));
                Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName());
                Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName(
                    StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                    StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));

                var specialCustomerUniqueConstraint = baseTable.UniqueConstraints.Single(c => !c.GetIsPrimaryKey());
                Assert.Equal("AK_AbstractBase_SpecialityAk", specialCustomerUniqueConstraint.Name);
                Assert.NotNull(specialCustomerUniqueConstraint.MappedKeys.Single());

                var foreignKeys = specialCustomerTable.ForeignKeyConstraints.ToArray();

                Assert.Equal(3, foreignKeys.Length);

                var specialCustomerFkConstraint = foreignKeys[0];
                Assert.Equal("FK_SpecialCustomer_AbstractBase_RelatedCustomerSpeciality", specialCustomerFkConstraint.Name);
                Assert.NotNull(specialCustomerFkConstraint.MappedForeignKeys.Single());
                Assert.Same(baseTable, specialCustomerFkConstraint.PrincipalTable);

                var specialCustomerTptFkConstraint = foreignKeys[1];
                Assert.Equal("FK_SpecialCustomer_Customer_Id", specialCustomerTptFkConstraint.Name);
                Assert.NotNull(specialCustomerTptFkConstraint.MappedForeignKeys.Single());
                Assert.Same(customerTable, specialCustomerTptFkConstraint.PrincipalTable);

                var anotherSpecialCustomerFkConstraint = foreignKeys[2];
                Assert.Equal("FK_SpecialCustomer_SpecialCustomer_AnotherRelatedCustomerId", anotherSpecialCustomerFkConstraint.Name);
                Assert.NotNull(anotherSpecialCustomerFkConstraint.MappedForeignKeys.Single());
                Assert.Same(specialCustomerTable, anotherSpecialCustomerFkConstraint.PrincipalTable);

                Assert.Equal(new[] { orderCustomerFkConstraint, specialCustomerTptFkConstraint }, customerTable.ReferencingForeignKeyConstraints);

                var specialCustomerDbIndex = specialCustomerTable.Indexes.Last();
                Assert.Equal("IX_SpecialCustomer_RelatedCustomerSpeciality", specialCustomerDbIndex.Name);
                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());

                var anotherSpecialCustomerDbIndex = specialCustomerTable.Indexes.First();
                Assert.Equal("IX_SpecialCustomer_AnotherRelatedCustomerId", anotherSpecialCustomerDbIndex.Name);
                Assert.NotNull(anotherSpecialCustomerDbIndex.MappedIndexes.Single());

                Assert.Null(customerType.FindDiscriminatorProperty());
                Assert.Null(customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.FindDiscriminatorProperty());
                Assert.Null(specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerTypeMapping = specialCustomerType.GetTableMappings().Single();
                Assert.True(specialCustomerTypeMapping.IsSplitEntityTypePrincipal);

                var specialityColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
                var addressColumn = specialCustomerTable.Columns.Single(c =>
                    c.Name == nameof(SpecialCustomer.Details) + "_" + nameof(CustomerDetails.Address));

                var specialCustomerPkConstraint = specialCustomerTable.PrimaryKey;
                var specialCustomerUniqueConstraint = specialCustomerTable.UniqueConstraints.Single(c => !c.GetIsPrimaryKey());
                var specialCustomerDbIndex = specialCustomerTable.Indexes.Last();
                var anotherSpecialCustomerDbIndex = specialCustomerTable.Indexes.First();

                var idProperty = customerPk.Properties.Single();

                if (mapping == Mapping.TPH)
                {
                    var baseTable = abstractBaseType.GetTableMappings().Single().Table;
                    Assert.Equal("AbstractBase", baseTable.Name);
                    Assert.Equal(baseTable.Name, abstractBaseType.GetTableName());
                    Assert.Equal(baseTable.Name, customerTable.Name);
                    Assert.Equal(baseTable.Name, abstractCustomerType.GetTableName());
                    Assert.Equal(baseTable.Name, specialCustomerType.GetTableName());

                    Assert.True(specialCustomerTypeMapping.IncludesDerivedTypes);
                    Assert.Same(customerTable, specialCustomerTable);

                    Assert.Equal(6, specialCustomerTable.EntityTypeMappings.Count());
                    Assert.True(specialCustomerTable.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.False(specialCustomerTable.EntityTypeMappings.Last().IsSharedTablePrincipal);

                    Assert.Equal(11, specialCustomerTable.Columns.Count());

                    Assert.True(specialityColumn.IsNullable);
                    Assert.True(addressColumn.IsNullable);

                    var orderCustomerFkConstraint = orderCustomerFk.GetMappedConstraints().Single();

                    Assert.Equal("FK_Order_" + baseTable.Name + "_CustomerId", orderCustomerFkConstraint.Name);
                    Assert.Equal(nameof(Order.CustomerId), orderCustomerFkConstraint.Columns.Single().Name);
                    Assert.Equal(nameof(Customer.Id), orderCustomerFkConstraint.PrincipalColumns.Single().Name);
                    Assert.Same(ordersTable, orderCustomerFkConstraint.Table);
                    Assert.Equal(baseTable.Name, orderCustomerFkConstraint.PrincipalTable.Name);
                    Assert.Equal(ReferentialAction.Cascade, orderCustomerFkConstraint.OnDeleteAction);
                    Assert.Equal(orderCustomerFk, orderCustomerFkConstraint.MappedForeignKeys.Single());
                    Assert.Equal(new[] { orderDateFkConstraint, orderCustomerFkConstraint }, ordersTable.ForeignKeyConstraints);
                    Assert.Empty(ordersTable.ReferencingForeignKeyConstraints);

                    Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetConstraintName());
                    Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetConstraintName(
                        StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));
                    Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName());
                    Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName(
                        StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));

                    Assert.Equal("PK_" + baseTable.Name, specialCustomerPkConstraint.Name);
                    Assert.Equal("AK_AbstractBase_SpecialityAk", specialCustomerUniqueConstraint.Name);

                    var specialCustomerFkConstraint = specialCustomerTable.ForeignKeyConstraints.Last();
                    Assert.Equal("FK_AbstractBase_AbstractBase_RelatedCustomerSpeciality", specialCustomerFkConstraint.Name);
                    Assert.NotNull(specialCustomerFkConstraint.MappedForeignKeys.Single());

                    var anotherSpecialCustomerFkConstraint = specialCustomerTable.ForeignKeyConstraints.First();
                    Assert.Equal("FK_AbstractBase_AbstractBase_AnotherRelatedCustomerId", anotherSpecialCustomerFkConstraint.Name);
                    Assert.NotNull(anotherSpecialCustomerFkConstraint.MappedForeignKeys.Single());

                    Assert.Equal(new[] { anotherSpecialCustomerFkConstraint, specialCustomerFkConstraint, orderCustomerFkConstraint },
                        customerTable.ReferencingForeignKeyConstraints);

                    Assert.Equal("IX_AbstractBase_RelatedCustomerSpeciality", specialCustomerDbIndex.Name);
                    Assert.Equal("IX_AbstractBase_AnotherRelatedCustomerId", anotherSpecialCustomerDbIndex.Name);

                    Assert.Equal(5, idProperty.GetTableColumnMappings().Count());
                }
                else
                {
                    Assert.Null(abstractBaseType.GetTableName());
                    Assert.Equal(nameof(Customer), customerTable.Name);
                    Assert.Null(abstractCustomerType.GetTableName());
                    Assert.Equal(nameof(SpecialCustomer), specialCustomerType.GetTableName());

                    Assert.False(specialCustomerTypeMapping.IncludesDerivedTypes);
                    Assert.NotSame(customerTable, specialCustomerTable);

                    Assert.Empty(ordersTable.ReferencingForeignKeyConstraints);
                    Assert.Empty(customerTable.ReferencingForeignKeyConstraints);

                    Assert.True(customerTable.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Equal(5, customerTable.Columns.Count());

                    Assert.Equal(2, specialCustomerTable.EntityTypeMappings.Count());
                    Assert.True(specialCustomerTable.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.False(specialCustomerTable.EntityTypeMappings.Last().IsSharedTablePrincipal);

                    Assert.Equal(10, specialCustomerTable.Columns.Count());

                    Assert.False(specialityColumn.IsNullable);
                    Assert.False(addressColumn.IsNullable);

                    // Derived principal entity types are mapped to different tables, so the constraint is not enforceable
                    Assert.Empty(orderCustomerFk.GetMappedConstraints());

                    Assert.Null(orderCustomerFk.GetConstraintName());
                    Assert.Null(orderCustomerFk.GetConstraintName(
                        StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));
                    Assert.Null(orderCustomerFk.GetDefaultName());
                    Assert.Null(orderCustomerFk.GetDefaultName(
                        StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));

                    Assert.Equal("PK_SpecialCustomer", specialCustomerPkConstraint.Name);
                    Assert.Equal("AK_SpecialCustomer_SpecialityAk", specialCustomerUniqueConstraint.Name);

                    Assert.Empty(specialCustomerTable.ForeignKeyConstraints);

                    Assert.Equal("IX_SpecialCustomer_RelatedCustomerSpeciality", specialCustomerDbIndex.Name);
                    Assert.Equal("IX_SpecialCustomer_AnotherRelatedCustomerId", anotherSpecialCustomerDbIndex.Name);

                    Assert.Equal(3, idProperty.GetTableColumnMappings().Count());
                }

                Assert.Same(specialCustomerPkConstraint.MappedKeys.First(), customerPk);

                Assert.NotNull(specialCustomerUniqueConstraint.MappedKeys.Single());

                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());
                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());
            }
        }

        private IRelationalModel CreateTestModel(bool mapToTables = false, bool mapToViews = false, Mapping mapping = Mapping.TPH)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Entity<AbstractBase>(
                cb =>
                {
                    if (mapping != Mapping.TPC)
                    {
                        if (mapToViews)
                        {
                            cb.ToView("BaseView", "viewSchema");
                        }

                        if (mapToTables)
                        {
                            cb.ToTable(t => { });
                        }
                    }

                    if (mapping == Mapping.TPC)
                    {
                        cb.UseTpcMappingStrategy();
                    }
                    else if (mapping == Mapping.TPT
                        && (!mapToTables && !mapToViews))
                    {
                        cb.UseTptMappingStrategy();
                    }

                    // TODO: Don't map it on the base #19811
                    cb.Property<string>("SpecialityAk");
                });

            modelBuilder.Entity<Customer>(
                cb =>
                {
                    if (mapping != Mapping.TPH)
                    {
                        if (mapToViews)
                        {
                            cb.ToView("CustomerView", "viewSchema");
                        }

                        if (mapToTables)
                        {
                            cb.ToTable("Customer");
                        }
                    }
                });

            modelBuilder.Entity<AbstractCustomer>(
                cb =>
                {
                    if (mapping == Mapping.TPT)
                    {
                        cb.ToView(null);
                        cb.ToTable((string)null);
                    }
                });

            modelBuilder.Entity<SpecialCustomer>(
                cb =>
                {
                    if (mapping != Mapping.TPH)
                    {
                        if (mapToViews)
                        {
                            cb.ToView("SpecialCustomerView");
                        }

                        if (mapToTables)
                        {
                            cb.ToTable("SpecialCustomer", "SpecialSchema");
                        }
                    }
                    cb.HasCheckConstraint($"Speciality", $"[Speciality] IN ('Specialist', 'Generalist')");

                    cb.Property(s => s.Speciality).IsRequired();

                    cb.HasOne(c => c.RelatedCustomer).WithOne()
                        .HasForeignKey<SpecialCustomer>(c => c.RelatedCustomerSpeciality)
                        .HasPrincipalKey<SpecialCustomer>("SpecialityAk"); // TODO: Use the derived one, #2611

                    cb.HasOne<SpecialCustomer>().WithOne()
                        .HasForeignKey<SpecialCustomer>("AnotherRelatedCustomerId");

                    cb.OwnsOne(c => c.Details).Property(d => d.Address).IsRequired();
                    cb.Navigation(c => c.Details).IsRequired();
                });

            modelBuilder.Entity<ExtraSpecialCustomer>(
                cb =>
                {
                    if (mapping != Mapping.TPH)
                    {
                        if (mapToViews)
                        {
                            cb.ToView("ExtraSpecialCustomerView");
                        }

                        if (mapToTables)
                        {
                            cb.ToTable("ExtraSpecialCustomer", "ExtraSpecialSchema");
                        }
                    }
                });

            modelBuilder.Entity<Order>(
                ob =>
                {
                    ob.Property(o => o.OrderDate).HasColumnName("OrderDate");
                    ob.Property(o => o.AlternateId).HasColumnName("AlternateId");

                    ob.HasAlternateKey(o => o.AlternateId).HasName("AK_AlternateId");
                    ob.HasOne(o => o.DateDetails).WithOne()
                        .HasForeignKey<Order>(o => o.OrderDate).HasPrincipalKey<DateDetails>(o => o.Date)
                        .HasConstraintName("FK_DateDetails");

                    // Note: the below is resetting the name of the anonymous index
                    // created in HasForeignKey() above, not creating a new index.
                    ob.HasIndex(o => o.OrderDate).HasDatabaseName("IX_OrderDate");

                    ob.OwnsOne(
                        o => o.Details, odb =>
                        {
                            odb.Property(od => od.OrderDate).HasColumnName("OrderDate");
                            var alternateId = odb.Property(o => o.AlternateId).HasColumnName("AlternateId").Metadata;

                            odb.OwnedEntityType.AddKey(new[] { alternateId });
                            // Issue #20948
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

                    if (mapToTables || !mapToViews)
                    {
                        ob.ToTable(o => o.HasTrigger("Order_Trigger"));
                    }
                });

            modelBuilder.Entity<DateDetails>(
                db =>
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

            return Finalize(modelBuilder);
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_keyless_TPH()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.Ignore(c => c.Orders);
                    cb.ToView("CustomerView");
                });

            modelBuilder.Entity<SpecialCustomer>(
                cb =>
                {
                    cb.Ignore(c => c.Details);
                    cb.Property(s => s.Speciality).IsRequired();
                });

            var model = Finalize(modelBuilder);

            Assert.Equal(2, model.Model.GetEntityTypes().Count());
            Assert.Empty(model.Tables);
            Assert.Single(model.Views);

            var customerType = model.Model.FindEntityType(typeof(Customer));
            Assert.NotNull(customerType.FindDiscriminatorProperty());

            var customerView = customerType.GetViewMappings().Single().View;
            Assert.Equal("CustomerView", customerView.Name);

            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));

            var specialCustomerTypeMapping = specialCustomerType.GetViewMappings().Single();
            Assert.True(specialCustomerTypeMapping.IsSplitEntityTypePrincipal);

            var specialCustomerView = specialCustomerTypeMapping.View;
            Assert.Same(customerView, specialCustomerView);

            Assert.Equal(2, specialCustomerView.EntityTypeMappings.Count());
            Assert.True(specialCustomerView.EntityTypeMappings.First().IsSharedTablePrincipal);
            Assert.False(specialCustomerView.EntityTypeMappings.Last().IsSharedTablePrincipal);

            var specialityColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Speciality));
            Assert.True(specialityColumn.IsNullable);
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_SQL_queries()
        {
            var modelBuilder = CreateConventionModelBuilder();
            modelBuilder.Entity<Order>(
                cb =>
                {
                    cb.ToSqlQuery("GetOrders()");
                    cb.Ignore(c => c.Customer);
                    cb.Ignore(c => c.Details);
                    cb.Ignore(c => c.DateDetails);

                    cb.Property(c => c.AlternateId).HasColumnName("SomeName");
                    cb.HasNoKey();
                });

            var model = Finalize(modelBuilder);

            Assert.Single(model.Model.GetEntityTypes());
            Assert.Single(model.Queries);
            Assert.Empty(model.Views);
            Assert.Empty(model.Tables);
            Assert.Empty(model.Functions);

            var orderType = model.Model.FindEntityType(typeof(Order));
            Assert.Null(orderType.FindPrimaryKey());

            var orderMapping = orderType.GetSqlQueryMappings().Single();
            Assert.True(orderMapping.IsSharedTablePrincipal);
            Assert.True(orderMapping.IsSplitEntityTypePrincipal);

            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.Id), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersQuery = orderMapping.SqlQuery;
            Assert.Equal(
                new[] { orderType },
                ordersQuery.EntityTypeMappings.Select(m => m.EntityType));
            Assert.Equal(
                new[] { nameof(Order.CustomerId), nameof(Order.Id), nameof(Order.OrderDate), "SomeName" },
                ordersQuery.Columns.Select(m => m.Name));
            Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.RelationalModelTest+Order.MappedSqlQuery", ordersQuery.Name);
            Assert.Null(ordersQuery.Schema);
            Assert.Equal("GetOrders()", ordersQuery.Sql);
            Assert.False(ordersQuery.IsShared);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            Assert.Single(orderDate.GetSqlQueryColumnMappings());
            var orderDateMapping = orderMapping.ColumnMappings.Single(m => m.Property == orderDate);
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.SqlQueryMapping);

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersQuery.FindColumn(nameof(Order.OrderDate)));
            Assert.Same(orderDateColumn, orderDate.FindColumn(StoreObjectIdentifier.SqlQuery(orderType)));
            Assert.Same(orderDateColumn, ordersQuery.FindColumn(orderDate));
            Assert.Equal(new[] { orderDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal(nameof(Order.OrderDate), orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersQuery, orderDateColumn.SqlQuery);

            Assert.Same(orderMapping, ordersQuery.EntityTypeMappings.Single());
        }

        private static IQueryable<Order> GetOrdersForCustomer(int id)
            => throw new NotImplementedException();

        [ConditionalFact]
        public void Can_use_relational_model_with_functions()
        {
            var modelBuilder = CreateConventionModelBuilder();
            modelBuilder.Entity<Order>(
                cb =>
                {
                    cb.ToFunction("GetOrders");
                    cb.Ignore(c => c.Customer);
                    cb.Ignore(c => c.Details);
                    cb.Ignore(c => c.DateDetails);

                    cb.Property(c => c.AlternateId).HasColumnName("SomeName");
                    cb.HasNoKey();
                });

            modelBuilder.HasDbFunction(
                typeof(RelationalModelTest).GetMethod(
                    nameof(GetOrdersForCustomer), BindingFlags.NonPublic | BindingFlags.Static));

            var model = Finalize(modelBuilder);

            Assert.Single(model.Model.GetEntityTypes());
            Assert.Equal(2, model.Functions.Count());
            Assert.Empty(model.Views);
            Assert.Empty(model.Tables);

            var orderType = model.Model.FindEntityType(typeof(Order));
            Assert.Null(orderType.FindPrimaryKey());

            Assert.Equal(2, orderType.GetFunctionMappings().Count());
            var orderMapping = orderType.GetFunctionMappings().First();
            Assert.True(orderMapping.IsSharedTablePrincipal);
            Assert.True(orderMapping.IsSplitEntityTypePrincipal);
            Assert.True(orderMapping.IsDefaultFunctionMapping);

            var tvfMapping = orderType.GetFunctionMappings().Last();
            Assert.True(tvfMapping.IsSharedTablePrincipal);
            Assert.True(tvfMapping.IsSplitEntityTypePrincipal);
            Assert.False(tvfMapping.IsDefaultFunctionMapping);

            Assert.True(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.Id), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersFunction = orderMapping.StoreFunction;
            Assert.Same(ordersFunction, model.FindFunction(ordersFunction.Name, ordersFunction.Schema, new string[0]));
            Assert.Equal(
                new[] { orderType },
                ordersFunction.EntityTypeMappings.Select(m => m.EntityType));
            Assert.Equal(
                new[] { nameof(Order.CustomerId), nameof(Order.Id), nameof(Order.OrderDate), "SomeName" },
                ordersFunction.Columns.Select(m => m.Name));
            Assert.Equal("GetOrders", ordersFunction.Name);
            Assert.Null(ordersFunction.Schema);
            Assert.False(ordersFunction.IsBuiltIn);
            Assert.False(ordersFunction.IsShared);
            Assert.Null(ordersFunction.ReturnType);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));
            Assert.Equal(2, orderDate.GetFunctionColumnMappings().Count());
            var orderDateMapping = orderMapping.ColumnMappings.Single(m => m.Property == orderDate);
            Assert.NotNull(orderDateMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateMapping.TypeMapping.StoreType);
            Assert.Same(orderMapping, orderDateMapping.FunctionMapping);

            var orderDateColumn = orderDateMapping.Column;
            Assert.Same(orderDateColumn, ordersFunction.FindColumn(nameof(Order.OrderDate)));
            Assert.Same(orderDateColumn, orderDate.FindColumn(StoreObjectIdentifier.DbFunction(ordersFunction.Name)));
            Assert.Same(orderDateColumn, ordersFunction.FindColumn(orderDate));
            Assert.Equal(new[] { orderDate }, orderDateColumn.PropertyMappings.Select(m => m.Property));
            Assert.Equal(nameof(Order.OrderDate), orderDateColumn.Name);
            Assert.Equal("default_datetime_mapping", orderDateColumn.StoreType);
            Assert.False(orderDateColumn.IsNullable);
            Assert.Same(ordersFunction, orderDateColumn.Function);

            Assert.Same(orderMapping, ordersFunction.EntityTypeMappings.Single());

            var tvfFunction = tvfMapping.StoreFunction;
            Assert.Same(tvfMapping, tvfFunction.EntityTypeMappings.Single());
            Assert.Same(tvfFunction, model.FindFunction(tvfFunction.Name, tvfFunction.Schema, new[] { "default_int_mapping" }));
            Assert.Equal(nameof(GetOrdersForCustomer), tvfFunction.Name);
            Assert.Null(tvfFunction.Schema);
            Assert.False(tvfFunction.IsBuiltIn);
            Assert.False(tvfFunction.IsShared);
            Assert.Null(tvfFunction.ReturnType);

            var tvfDbFunction = tvfFunction.DbFunctions.Single();
            Assert.Same(tvfFunction, tvfDbFunction.StoreFunction);
            Assert.Same(model.Model.GetDbFunctions().Single(f => f.Parameters.Count() == 1), tvfDbFunction);
            Assert.Same(tvfFunction.Parameters.Single(), tvfDbFunction.Parameters.Single().StoreFunctionParameter);
            Assert.Equal(tvfDbFunction.Parameters.Single().Name, tvfFunction.Parameters.Single().DbFunctionParameters.Single().Name);
        }

        [ConditionalFact]
        public void Default_mappings_does_not_share_tableBase()
        {
            var modelBuilder = CreateConventionModelBuilder();
            modelBuilder.Entity<SameEntityType>().HasNoKey().ToTable((string)null);
            modelBuilder.Entity<NameSpace2.SameEntityType>().HasNoKey().ToTable((string)null);

            var model = Finalize(modelBuilder);

            Assert.Equal(2, model.Model.GetEntityTypes().Count());
            Assert.Empty(model.Tables);
            Assert.Empty(model.Views);
            Assert.Empty(model.Functions);
            Assert.Empty(model.Queries);

            var entityType1 = model.Model.FindEntityType(typeof(SameEntityType));
            var entityType2 = model.Model.FindEntityType(typeof(NameSpace2.SameEntityType));

            var defaultMapping1 = Assert.Single(entityType1.GetDefaultMappings());
            var defaultMapping2 = Assert.Single(entityType2.GetDefaultMappings());

            Assert.NotSame(defaultMapping1, defaultMapping2);

            Assert.True(defaultMapping1.Table.Columns.Single().IsNullable);
            Assert.False(defaultMapping2.Table.Columns.Single().IsNullable);
        }

        private static IRelationalModel Finalize(TestHelpers.TestModelBuilder modelBuilder)
            => modelBuilder.FinalizeModel(designTime: true).GetRelationalModel();

        protected virtual TestHelpers.TestModelBuilder CreateConventionModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        public enum Mapping
        {
#pragma warning disable SA1602 // Enumeration items should be documented
            TPH,
            TPT,
            TPC
#pragma warning restore SA1602 // Enumeration items should be documented
        }

        private enum MyEnum : ulong
        {
            Sun,
            Mon,
            Tue
        }

        private abstract class AbstractBase
        {
            public int Id { get; set; }
        }

        private class Customer : AbstractBase
        {
            public string Name { get; set; }
            public short SomeShort { get; set; }
            public MyEnum EnumValue { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

#nullable enable
        private abstract class AbstractCustomer : Customer
        {
            public string AbstractString { get; set; } = null!;
        }
#nullable disable

        private class SpecialCustomer : AbstractCustomer
        {
            public string Speciality { get; set; }
            public string RelatedCustomerSpeciality { get; set; }
            public SpecialCustomer RelatedCustomer { get; set; }
            public CustomerDetails Details { get; set; }
        }

        private class CustomerDetails
        {
            public string Address { get; set; }
        }

        private class ExtraSpecialCustomer : SpecialCustomer
        {
        }

        private class Order
        {
            public int Id { get; set; }
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
            public bool Active { get; set; }

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

namespace NameSpace1
{
    public class SameEntityType
    {
        public int? MyValue { get; set; }
    }
}

namespace NameSpace2
{
    public class SameEntityType
    {
        public int MyValue { get; set; }
    }
}
