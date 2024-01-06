// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
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
            Assert.Equal(
                mapping switch
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
            var model = CreateTestModel(mapToTables: false, mapToViews: true, mapping: mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());
            Assert.Equal(
                mapping switch
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
        [InlineData(true, Mapping.TPH)]
        [InlineData(true, Mapping.TPT)]
        [InlineData(true, Mapping.TPC)]
        [InlineData(false, Mapping.TPH)]
        [InlineData(false, Mapping.TPT)]
        [InlineData(false, Mapping.TPC)]
        public void Can_use_relational_model_with_sprocs(bool mapToTables, Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: mapToTables, mapToSprocs: true, mapping: mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());
            Assert.Equal(
                mapping switch
                {
                    Mapping.TPC => 5,
                    Mapping.TPH => 3,
                    _ => 6
                }, model.Tables.Count());

            Assert.Equal(
                mapping switch
                {
                    Mapping.TPC => 24,
                    Mapping.TPH => 18,
                    _ => 27
                }, model.StoredProcedures.Count());

            Assert.Empty(model.Views);
            Assert.True(model.Model.GetEntityTypes().All(et => !et.GetViewMappings().Any()));

            AssertDefaultMappings(model, mapping);
            AssertTables(model, mapping);
            AssertSprocs(model, mapping, mappedToTables: true);
        }

        [ConditionalTheory(Skip = "#28703")]
        [InlineData(Mapping.TPH)]
        [InlineData(Mapping.TPT)]
        [InlineData(Mapping.TPC)]
        public void Can_use_relational_model_with_sprocs_and_views(Mapping mapping)
        {
            var model = CreateTestModel(mapToViews: true, mapToSprocs: true, mapping: mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());

            Assert.Equal(
                mapping switch
                {
                    Mapping.TPC => 5,
                    Mapping.TPH => 3,
                    _ => 6
                }, model.Views.Count());

            Assert.Equal(
                mapping switch
                {
                    Mapping.TPC => 24,
                    Mapping.TPH => 18,
                    _ => 27
                }, model.StoredProcedures.Count());

            AssertDefaultMappings(model, mapping);
            AssertViews(model, mapping);
            AssertSprocs(model, mapping);
        }

        [ConditionalTheory]
        [InlineData(Mapping.TPH)]
        [InlineData(Mapping.TPT)]
        [InlineData(Mapping.TPC)]
        public void Can_use_relational_model_with_tables_and_views(Mapping mapping)
        {
            var model = CreateTestModel(mapToTables: true, mapToViews: true, mapping: mapping);

            Assert.Equal(11, model.Model.GetEntityTypes().Count());
            Assert.Equal(
                mapping switch
                {
                    Mapping.TPC => 5,
                    Mapping.TPH => 3,
                    _ => 6
                }, model.Tables.Count());

            Assert.Equal(
                mapping switch
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
            Assert.Null(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.Id), nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersTable = orderMapping.Table;
            Assert.Equal(new[] { nameof(Order) }, ordersTable.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));
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
                Assert.Null(specialCustomerType.GetDefaultMappings().First().IsSplitEntityTypePrincipal);
                Assert.False(specialCustomerType.GetDefaultMappings().First().IncludesDerivedTypes);
                Assert.Null(specialCustomerType.GetDefaultMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetDefaultMappings().Last().IncludesDerivedTypes);

                var specialCustomerTable = specialCustomerType.GetDefaultMappings().Last().Table;
                Assert.Null(specialCustomerTable.Schema);
                Assert.Equal(4, specialCustomerTable.Columns.Count());

                Assert.Null(
                    specialCustomerTable.EntityTypeMappings.Single(m => m.TypeBase == specialCustomerType).IsSharedTablePrincipal);

                var specialtyColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));
                Assert.False(specialtyColumn.IsNullable);

                Assert.Null(customerType.FindDiscriminatorProperty());
                Assert.Equal("Customer", customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.FindDiscriminatorProperty());
                Assert.Equal("SpecialCustomer", specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerTableMapping = specialCustomerType.GetDefaultMappings().Single();
                Assert.Null(specialCustomerTableMapping.IsSplitEntityTypePrincipal);
                var specialCustomerTable = specialCustomerTableMapping.Table;
                var specialtyColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));
                if (mapping == Mapping.TPH)
                {
                    var baseTable = abstractBaseType.GetDefaultMappings().Single().Table;
                    Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.RelationalModelTest+AbstractBase", baseTable.Name);
                    Assert.Equal(baseTable.Name, customerTable.Name);
                    Assert.Equal(baseTable.Schema, customerTable.Schema);
                    Assert.True(specialCustomerTableMapping.IncludesDerivedTypes);
                    Assert.Same(customerTable, specialCustomerTable);

                    Assert.Equal(5, specialCustomerTable.EntityTypeMappings.Count());
                    Assert.All(specialCustomerTable.EntityTypeMappings, t => Assert.Null(t.IsSharedTablePrincipal));

                    Assert.Equal(10, specialCustomerTable.Columns.Count());

                    Assert.True(specialtyColumn.IsNullable);
                }
                else
                {
                    Assert.False(specialCustomerTableMapping.IncludesDerivedTypes);
                    Assert.NotSame(customerTable, specialCustomerTable);

                    Assert.Null(customerTable.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Equal(5, customerTable.Columns.Count());

                    Assert.Null(specialCustomerTable.EntityTypeMappings.Single().IsSharedTablePrincipal);

                    Assert.Equal(9, specialCustomerTable.Columns.Count());

                    Assert.False(specialtyColumn.IsNullable);
                }
            }
        }

        private static void AssertViews(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order))!;
            var orderMapping = orderType.GetViewMappings().Single();
            Assert.Equal(orderType.GetViewMappings(), orderType.GetViewOrTableMappings());
            Assert.Null(orderMapping.IncludesDerivedTypes);
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
                ordersView.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));
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
            var customerType = model.Model.FindEntityType(typeof(Customer))!;
            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer))!;
            var extraSpecialCustomerType = model.Model.FindEntityType(typeof(ExtraSpecialCustomer))!;
            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;
            Assert.Same(ordersView, orderDetailsType.GetViewMappings().Single().View);
            Assert.Equal(
                ordersView.GetReferencingRowInternalForeignKeys(orderType), ordersView.GetRowInternalForeignKeys(orderDetailsType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersView.SchemaQualifiedName),
                Assert.Throws<InvalidOperationException>(
                    () => ordersView.GetReferencingRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersView.SchemaQualifiedName),
                Assert.Throws<InvalidOperationException>(
                    () => ordersView.GetRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.False(ordersView.IsOptional(orderType));
            Assert.True(ordersView.IsOptional(orderDetailsType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersView.SchemaQualifiedName),
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
                    RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerView.SchemaQualifiedName),
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
            Assert.Equal(
                mappedToTable && mapping != Mapping.TPC
                    ? "FK_Order_" + baseTableName + "_CustomerId"
                    : null, ordersCustomerForeignKey.GetConstraintName());
            Assert.Null(
                ordersCustomerForeignKey.GetConstraintName(
                    StoreObjectIdentifier.View(ordersView.Name, ordersView.Schema),
                    StoreObjectIdentifier.View(customerView.Name, customerView.Schema)));
            Assert.Equal(
                mappedToTable && mapping != Mapping.TPC
                    ? "FK_Order_" + baseTableName + "_CustomerId"
                    : null, ordersCustomerForeignKey.GetDefaultName());
            Assert.Null(
                ordersCustomerForeignKey.GetDefaultName(
                    StoreObjectIdentifier.View(ordersView.Name, ordersView.Schema),
                    StoreObjectIdentifier.View(customerView.Name, customerView.Schema)));

            var ordersCustomerIndex = orderType.FindIndex(ordersCustomerForeignKey.Properties);
            Assert.Equal(
                mappedToTable
                    ? "IX_Order_CustomerId"
                    : null, ordersCustomerIndex.GetDatabaseName());
            Assert.Null(
                ordersCustomerIndex.GetDatabaseName(
                    StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
            Assert.Equal(
                mappedToTable
                    ? "IX_Order_CustomerId"
                    : null, ordersCustomerIndex.GetDefaultDatabaseName());
            Assert.Null(
                ordersCustomerIndex.GetDefaultDatabaseName(
                    StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));

            if (mappedToTable)
            {
                var specialtyCK = specialCustomerType.GetCheckConstraints().Single();
                Assert.Equal("Specialty", specialtyCK.Name);
                Assert.Null(
                    specialtyCK.GetName(
                        StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
                Assert.Equal("Specialty", specialtyCK.GetDefaultName());
                Assert.Equal(
                    "Specialty", specialtyCK.GetDefaultName(
                        StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
            }
            else
            {
                Assert.Empty(specialCustomerType.GetCheckConstraints());
            }

            Assert.Equal(
                mappedToTable
                    ? "PK_Order"
                    : null, orderPk.GetName());
            Assert.Null(
                orderPk.GetName(
                    StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));
            Assert.Equal(
                mappedToTable
                    ? "PK_Order"
                    : null, orderPk.GetDefaultName());
            Assert.Equal(
                "PK_OrderView", orderPk.GetDefaultName(
                    StoreObjectIdentifier.Table(ordersView.Name, ordersView.Schema)));

            if (mapping == Mapping.TPT)
            {
                Assert.Equal("CustomerView", customerView.Name);
                Assert.Equal("viewSchema", customerView.Schema);
                Assert.Equal(3, specialCustomerType.GetViewMappings().Count());
                Assert.Null(specialCustomerType.GetViewMappings().First().IsSplitEntityTypePrincipal);
                Assert.False(specialCustomerType.GetViewMappings().First().IncludesDerivedTypes);
                Assert.Null(specialCustomerType.GetViewMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetViewMappings().Last().IncludesDerivedTypes);

                var specialCustomerView = specialCustomerType.GetViewMappings().Select(t => t.Table)
                    .First(t => t.Name == "SpecialCustomerView");
                Assert.Null(specialCustomerView.Schema);
                Assert.Equal(7, specialCustomerView.Columns.Count());

                Assert.True(specialCustomerView.EntityTypeMappings.Single(m => m.TypeBase == specialCustomerType).IsSharedTablePrincipal);

                var specialtyColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));
                Assert.False(specialtyColumn.IsNullable);

                Assert.Null(customerType.FindDiscriminatorProperty());
                Assert.Equal("Customer", customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.FindDiscriminatorProperty());
                Assert.Equal("SpecialCustomer", specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerViewMapping = specialCustomerType.GetViewMappings().Single();
                Assert.Null(specialCustomerViewMapping.IsSplitEntityTypePrincipal);
                var specialCustomerView = specialCustomerViewMapping.View;
                var specialtyColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));

                var extraSpecialCustomerViewMapping = extraSpecialCustomerType.GetViewMappings().Single();
                Assert.Null(extraSpecialCustomerViewMapping.IsSplitEntityTypePrincipal);
                var extraSpecialCustomerView = extraSpecialCustomerViewMapping.View;
                if (mapping == Mapping.TPH)
                {
                    var baseView = abstractBaseType.GetViewMappings().Single().Table;
                    Assert.Equal("BaseView", baseView.Name);
                    Assert.Equal(baseView.Name, abstractBaseType.GetViewName());
                    Assert.Equal(baseView.Name, customerView.Name);
                    Assert.Equal(baseView.Schema, customerView.Schema);
                    Assert.True(specialCustomerViewMapping.IncludesDerivedTypes);
                    Assert.Same(customerView, specialCustomerView);

                    Assert.Equal(12, specialCustomerView.Columns.Count());

                    Assert.Equal(6, specialCustomerView.EntityTypeMappings.Count());
                    Assert.True(specialCustomerView.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.Null(specialCustomerView.EntityTypeMappings.First().IsSplitEntityTypePrincipal);
                    Assert.False(specialCustomerView.EntityTypeMappings.Last().IsSharedTablePrincipal);
                    Assert.Null(specialCustomerView.EntityTypeMappings.Last().IsSplitEntityTypePrincipal);

                    Assert.True(specialtyColumn.IsNullable);
                }
                else
                {
                    Assert.False(specialCustomerViewMapping.IncludesDerivedTypes);
                    Assert.NotSame(customerView, specialCustomerView);

                    Assert.Null(customerView.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Null(customerView.EntityTypeMappings.Single().IsSplitEntityTypePrincipal);
                    Assert.Equal(5, customerView.Columns.Count());

                    Assert.Single(specialCustomerView.EntityTypeMappings);

                    Assert.Equal(9, specialCustomerView.Columns.Count());

                    Assert.False(specialtyColumn.IsNullable);

                    Assert.Equal(2, extraSpecialCustomerView.EntityTypeMappings.Count());
                    Assert.True(extraSpecialCustomerView.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.Null(extraSpecialCustomerView.EntityTypeMappings.First().IsSplitEntityTypePrincipal);
                    Assert.False(extraSpecialCustomerView.EntityTypeMappings.Last().IsSharedTablePrincipal);
                    Assert.Null(extraSpecialCustomerView.EntityTypeMappings.Last().IsSplitEntityTypePrincipal);

                    Assert.Equal(11, extraSpecialCustomerView.Columns.Count());
                }
            }
        }

        private static void AssertTables(IRelationalModel model, Mapping mapping)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderMapping = orderType.GetTableMappings().Single();
            Assert.Null(orderMapping.IncludesDerivedTypes);
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
                ordersTable.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));
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
            Assert.Equal("OrderId", orderDetailsPkProperty.GetColumnName());

            var billingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.BillingAddress)).ForeignKey;
            Assert.True(billingAddressOwnership.IsRequiredDependent);

            var billingAddressType = billingAddressOwnership.DeclaringEntityType;

            var shippingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.ShippingAddress)).ForeignKey;
            Assert.True(shippingAddressOwnership.IsRequiredDependent);

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
            Assert.Equal(
                "IX_Order_CustomerId", ordersCustomerIndex.GetDatabaseName(
                    StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));
            Assert.Equal("IX_Order_CustomerId", ordersCustomerIndex.GetDefaultDatabaseName());
            Assert.Equal(
                "IX_Order_CustomerId", ordersCustomerIndex.GetDefaultDatabaseName(
                    StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));

            Assert.Equal("PK_Order", orderPk.GetName());
            Assert.Equal(
                "PK_Order", orderPk.GetName(
                    StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));
            Assert.Equal("PK_Order", orderPk.GetDefaultName());
            Assert.Equal(
                "PK_Order", orderPk.GetDefaultName(
                    StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema)));

            var specialCustomerTable =
                specialCustomerType.GetTableMappings().Select(t => t.Table).Last();
            var specialtyCk = specialCustomerType.GetCheckConstraints().Single();
            Assert.Equal("Specialty", specialtyCk.Name);
            Assert.Equal(
                "Specialty", specialtyCk.GetName(
                    StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema)));
            Assert.Equal("Specialty", specialtyCk.GetDefaultName());
            Assert.Equal(
                "Specialty", specialtyCk.GetDefaultName(
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

            var orderTrigger = Assert.Single(orderType.GetDeclaredTriggers());
            Assert.Equal("Order_Trigger", orderTrigger.GetDatabaseName());
            Assert.Equal("Order", orderTrigger.GetTableName());
            Assert.Null(orderTrigger.GetTableSchema());

            var customerPk = specialCustomerType.FindPrimaryKey();

            if (mapping == Mapping.TPT)
            {
                var baseTable = abstractBaseType.GetTableMappings().Single().Table;
                Assert.Equal("AbstractBase", baseTable.Name);
                Assert.Equal(nameof(Customer), customerTable.Name);
                Assert.Null(abstractCustomerType.GetTableName());
                Assert.Equal(nameof(SpecialCustomer), specialCustomerType.GetTableName());
                Assert.Equal(3, specialCustomerType.GetTableMappings().Count());
                Assert.Null(specialCustomerType.GetTableMappings().First().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetTableMappings().First().IncludesDerivedTypes);
                Assert.Null(specialCustomerType.GetTableMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetTableMappings().Last().IncludesDerivedTypes);

                Assert.Equal("SpecialCustomer", specialCustomerTable.Name);
                Assert.Equal(7, specialCustomerTable.Columns.Count());

                Assert.True(
                    specialCustomerTable.EntityTypeMappings.Single(m => m.TypeBase == specialCustomerType).IsSharedTablePrincipal);

                var specialtyColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));
                Assert.False(specialtyColumn.IsNullable);

                var addressColumn = specialCustomerTable.Columns.Single(
                    c =>
                        c.Name == nameof(SpecialCustomer.Details) + "_" + nameof(CustomerDetails.Address));
                Assert.False(addressColumn.IsNullable);
                var specialtyProperty = specialtyColumn.PropertyMappings.First().Property;

                Assert.Equal(
                    RelationalStrings.PropertyNotMappedToTable(
                        nameof(SpecialCustomer.Specialty), nameof(SpecialCustomer), "Customer"),
                    Assert.Throws<InvalidOperationException>(
                            () =>
                                specialtyProperty.IsColumnNullable(StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)))
                        .Message);

                var abstractStringColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                Assert.False(specialtyColumn.IsNullable);
                Assert.Equal(2, specialtyColumn.PropertyMappings.Count);

                var abstractStringProperty = abstractStringColumn.PropertyMappings.First().Property;
                Assert.Equal(2, abstractStringProperty.GetTableColumnMappings().Count());
                Assert.Equal(
                    new[] { StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema) },
                    abstractStringProperty.GetMappedStoreObjects(StoreObjectType.Table));

                var extraSpecialCustomerTable =
                    extraSpecialCustomerType.GetTableMappings().Select(t => t.Table).First(t => t.Name == "ExtraSpecialCustomer");

                Assert.Empty(customerTable.CheckConstraints);
                Assert.Same(specialtyCk, specialCustomerTable.CheckConstraints.Single());
                Assert.Empty(extraSpecialCustomerTable.CheckConstraints);

                Assert.Equal(4, customerPk.GetMappedConstraints().Count());
                var specialCustomerPkConstraint = specialCustomerTable.PrimaryKey;
                Assert.Equal("PK_SpecialCustomer", specialCustomerPkConstraint.Name);
                Assert.Same(specialCustomerPkConstraint.MappedKeys.First(), customerPk);

                var idProperty = customerPk.Properties.Single();
                Assert.Equal(10, idProperty.GetTableColumnMappings().Count());
                Assert.Equal(
                    new[]
                    {
                        StoreObjectIdentifier.Table(baseTable.Name, baseTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema),
                        StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema),
                        StoreObjectIdentifier.Table(extraSpecialCustomerTable.Name, extraSpecialCustomerTable.Schema)
                    },
                    idProperty.GetMappedStoreObjects(StoreObjectType.Table));

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
                Assert.Equal(
                    orderCustomerFkConstraint.Name, orderCustomerFk.GetConstraintName(
                        StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));
                Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName());
                Assert.Equal(
                    orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName(
                        StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                        StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));

                var specialCustomerUniqueConstraint = baseTable.UniqueConstraints.Single(c => !c.GetIsPrimaryKey());
                Assert.Equal("AK_AbstractBase_SpecialtyAk", specialCustomerUniqueConstraint.Name);
                Assert.NotNull(specialCustomerUniqueConstraint.MappedKeys.Single());

                var foreignKeys = specialCustomerTable.ForeignKeyConstraints.ToArray();

                Assert.Equal(3, foreignKeys.Length);

                var specialCustomerFkConstraint = foreignKeys[0];
                Assert.Equal("FK_SpecialCustomer_AbstractBase_RelatedCustomerSpecialty", specialCustomerFkConstraint.Name);
                Assert.NotNull(specialCustomerFkConstraint.MappedForeignKeys.Single());
                Assert.Same(baseTable, specialCustomerFkConstraint.PrincipalTable);

                var specialCustomerTptFkConstraint = foreignKeys[1];
                Assert.Equal("FK_SpecialCustomer_Customer_Id", specialCustomerTptFkConstraint.Name);
                Assert.NotNull(specialCustomerTptFkConstraint.MappedForeignKeys.Single());
                Assert.Same(customerTable, specialCustomerTptFkConstraint.PrincipalTable);
                Assert.Equal(ReferentialAction.Cascade, specialCustomerTptFkConstraint.OnDeleteAction);

                var anotherSpecialCustomerFkConstraint = foreignKeys[2];
                Assert.Equal("FK_SpecialCustomer_SpecialCustomer_AnotherRelatedCustomerId", anotherSpecialCustomerFkConstraint.Name);
                Assert.NotNull(anotherSpecialCustomerFkConstraint.MappedForeignKeys.Single());
                Assert.Same(specialCustomerTable, anotherSpecialCustomerFkConstraint.PrincipalTable);
                Assert.Equal(ReferentialAction.Cascade, specialCustomerTptFkConstraint.OnDeleteAction);

                Assert.Equal(
                    new[] { orderCustomerFkConstraint, specialCustomerTptFkConstraint }, customerTable.ReferencingForeignKeyConstraints);

                var specialCustomerDbIndex = specialCustomerTable.Indexes.Last();
                Assert.Equal("IX_SpecialCustomer_RelatedCustomerSpecialty", specialCustomerDbIndex.Name);
                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());

                var anotherSpecialCustomerDbIndex = specialCustomerTable.Indexes.First();
                Assert.Equal("IX_SpecialCustomer_AnotherRelatedCustomerId", anotherSpecialCustomerDbIndex.Name);
                Assert.NotNull(anotherSpecialCustomerDbIndex.MappedIndexes.Single());

                Assert.Null(customerType.FindDiscriminatorProperty());
                Assert.Equal("Customer", customerType.GetDiscriminatorValue());
                Assert.Null(specialCustomerType.FindDiscriminatorProperty());
                Assert.Equal("SpecialCustomer", specialCustomerType.GetDiscriminatorValue());
            }
            else
            {
                var specialCustomerTypeMapping = specialCustomerType.GetTableMappings().Single();
                Assert.Null(specialCustomerTypeMapping.IsSplitEntityTypePrincipal);

                var specialtyColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));

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

                    Assert.Equal(12, specialCustomerTable.Columns.Count());

                    var addressColumn = specialCustomerTable.Columns.Single(
                        c =>
                            c.Name == nameof(SpecialCustomer.Details) + "_" + nameof(CustomerDetails.Address));

                    Assert.True(specialtyColumn.IsNullable);
                    Assert.True(addressColumn.IsNullable);

                    var abstractStringColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                    Assert.True(abstractStringColumn.IsNullable);
                    Assert.Equal(3, abstractStringColumn.PropertyMappings.Count);

                    var abstractStringProperty = abstractStringColumn.PropertyMappings.First().Property;
                    Assert.Equal(3, abstractStringProperty.GetTableColumnMappings().Count());
                    Assert.Equal(
                        new[] { StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema) },
                        abstractStringProperty.GetMappedStoreObjects(StoreObjectType.Table));

                    Assert.Equal(5, idProperty.GetTableColumnMappings().Count());
                    Assert.Equal(
                        new[] { StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema) },
                        idProperty.GetMappedStoreObjects(StoreObjectType.Table));

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
                    Assert.Equal(
                        orderCustomerFkConstraint.Name, orderCustomerFk.GetConstraintName(
                            StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                            StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));
                    Assert.Equal(orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName());
                    Assert.Equal(
                        orderCustomerFkConstraint.Name, orderCustomerFk.GetDefaultName(
                            StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                            StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));

                    Assert.Equal("PK_" + baseTable.Name, specialCustomerPkConstraint.Name);
                    Assert.Equal("AK_AbstractBase_SpecialtyAk", specialCustomerUniqueConstraint.Name);

                    var specialCustomerFkConstraint = specialCustomerTable.ForeignKeyConstraints.Last();
                    Assert.Equal("FK_AbstractBase_AbstractBase_RelatedCustomerSpecialty", specialCustomerFkConstraint.Name);
                    Assert.NotNull(specialCustomerFkConstraint.MappedForeignKeys.Single());

                    var anotherSpecialCustomerFkConstraint = specialCustomerTable.ForeignKeyConstraints.First();
                    Assert.Equal("FK_AbstractBase_AbstractBase_AnotherRelatedCustomerId", anotherSpecialCustomerFkConstraint.Name);
                    Assert.NotNull(anotherSpecialCustomerFkConstraint.MappedForeignKeys.Single());

                    Assert.Equal(
                        new[] { anotherSpecialCustomerFkConstraint, specialCustomerFkConstraint, orderCustomerFkConstraint },
                        customerTable.ReferencingForeignKeyConstraints);

                    Assert.Equal("IX_AbstractBase_RelatedCustomerSpecialty", specialCustomerDbIndex.Name);
                    Assert.Equal("IX_AbstractBase_AnotherRelatedCustomerId", anotherSpecialCustomerDbIndex.Name);
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

                    Assert.Null(customerTable.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Equal(5, customerTable.Columns.Count());

                    Assert.Single(specialCustomerTable.EntityTypeMappings);

                    var abstractStringColumn = specialCustomerTable.Columns.Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                    Assert.False(specialtyColumn.IsNullable);

                    var extraSpecialCustomerTable =
                        extraSpecialCustomerType.GetTableMappings().Select(t => t.Table).First(t => t.Name == "ExtraSpecialCustomer");

                    Assert.Equal(2, extraSpecialCustomerTable.EntityTypeMappings.Count());

                    var addressColumn = extraSpecialCustomerTable.Columns.Single(
                        c =>
                            c.Name == nameof(SpecialCustomer.Details) + "_" + nameof(CustomerDetails.Address));
                    Assert.False(addressColumn.IsNullable);

                    var abstractStringProperty = abstractStringColumn.PropertyMappings.Single().Property;
                    Assert.Equal(2, abstractStringProperty.GetTableColumnMappings().Count());
                    Assert.Equal(
                        new[]
                        {
                            StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema),
                            StoreObjectIdentifier.Table(extraSpecialCustomerTable.Name, extraSpecialCustomerTable.Schema)
                        },
                        abstractStringProperty.GetMappedStoreObjects(StoreObjectType.Table));

                    Assert.Equal(3, idProperty.GetTableColumnMappings().Count());
                    Assert.Equal(
                        new[]
                        {
                            StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema),
                            StoreObjectIdentifier.Table(specialCustomerTable.Name, specialCustomerTable.Schema),
                            StoreObjectIdentifier.Table(extraSpecialCustomerTable.Name, extraSpecialCustomerTable.Schema)
                        },
                        idProperty.GetMappedStoreObjects(StoreObjectType.Table));

                    // Derived principal entity types are mapped to different tables, so the constraint is not enforceable
                    Assert.Empty(orderCustomerFk.GetMappedConstraints());

                    Assert.Null(orderCustomerFk.GetConstraintName());
                    Assert.Null(
                        orderCustomerFk.GetConstraintName(
                            StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                            StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));
                    Assert.Null(orderCustomerFk.GetDefaultName());
                    Assert.Null(
                        orderCustomerFk.GetDefaultName(
                            StoreObjectIdentifier.Table(ordersTable.Name, ordersTable.Schema),
                            StoreObjectIdentifier.Table(customerTable.Name, customerTable.Schema)));

                    Assert.Equal("PK_SpecialCustomer", specialCustomerPkConstraint.Name);
                    Assert.Equal("AK_SpecialCustomer_SpecialtyAk", specialCustomerUniqueConstraint.Name);

                    Assert.Empty(specialCustomerTable.ForeignKeyConstraints);

                    Assert.Equal("IX_SpecialCustomer_RelatedCustomerSpecialty", specialCustomerDbIndex.Name);
                    Assert.Equal("IX_SpecialCustomer_AnotherRelatedCustomerId", anotherSpecialCustomerDbIndex.Name);
                }

                Assert.Same(specialCustomerPkConstraint.MappedKeys.First(), customerPk);

                Assert.NotNull(specialCustomerUniqueConstraint.MappedKeys.Single());

                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());
                Assert.NotNull(specialCustomerDbIndex.MappedIndexes.Single());
            }
        }

        private static void AssertSprocs(IRelationalModel model, Mapping mapping, bool mappedToTables = false)
        {
            var orderType = model.Model.FindEntityType(typeof(Order));
            var orderInsertMapping = orderType.GetInsertStoredProcedureMappings().Single();
            Assert.Null(orderInsertMapping.IncludesDerivedTypes);
            Assert.Same(orderType.GetInsertStoredProcedure(), orderInsertMapping.StoredProcedure);

            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate) },
                orderInsertMapping.ParameterMappings.Select(m => m.Property.Name));

            Assert.Equal(
                new[] { nameof(Order.Id) },
                orderInsertMapping.ResultColumnMappings.Select(m => m.Property.Name));
            Assert.Equal(orderInsertMapping.ResultColumnMappings, orderInsertMapping.ColumnMappings);

            var ordersInsertSproc = orderInsertMapping.StoreStoredProcedure;
            Assert.Same(ordersInsertSproc, orderInsertMapping.Table);
            Assert.Equal("Order_Insert", ordersInsertSproc.Name);
            Assert.Null(ordersInsertSproc.Schema);
            Assert.False(ordersInsertSproc.IsShared);
            Assert.Same(ordersInsertSproc, model.FindStoredProcedure(ordersInsertSproc.Name, ordersInsertSproc.Schema));
            Assert.Equal(
                new[] { nameof(Order) },
                ordersInsertSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.OrderDate) },
                ordersInsertSproc.Parameters.Select(m => m.Name));

            Assert.Equal(
                new[] { 0, 1, 2 },
                ordersInsertSproc.Parameters.Select(m => m.Position));

            Assert.Equal(
                new[] { nameof(Order.Id) },
                ordersInsertSproc.ResultColumns.Select(m => m.Name));
            Assert.Equal(ordersInsertSproc.ResultColumns, ordersInsertSproc.Columns);

            var orderDate = orderType.FindProperty(nameof(Order.OrderDate));

            var orderDateInsertMapping = orderDate.GetInsertStoredProcedureParameterMappings().Single();
            Assert.NotNull(orderDateInsertMapping.TypeMapping);
            Assert.Equal("default_datetime_mapping", orderDateInsertMapping.TypeMapping.StoreType);
            Assert.Same(orderInsertMapping, orderDateInsertMapping.TableMapping);

            var orderDateParameter = orderDateInsertMapping.StoreParameter;
            Assert.Same(orderDateInsertMapping.StoreParameter, orderDateInsertMapping.Column);
            Assert.Same(orderDateParameter, ordersInsertSproc.FindParameter("OrderDate"));
            Assert.Same(orderDateParameter, ordersInsertSproc.FindParameter(orderDate));
            Assert.Equal("OrderDate", orderDateParameter.Name);
            Assert.Equal("default_datetime_mapping", orderDateParameter.StoreType);
            Assert.False(orderDateParameter.IsNullable);
            Assert.Equal(ParameterDirection.Input, orderDateParameter.Direction);
            Assert.Same(ordersInsertSproc, orderDateParameter.StoredProcedure);
            Assert.Same(orderDateParameter.StoredProcedure, orderDateParameter.Table);
            Assert.Same(orderDateInsertMapping, orderDateParameter.FindParameterMapping(orderType));

            var abstractBaseType = model.Model.FindEntityType(typeof(AbstractBase));
            var abstractCustomerType = model.Model.FindEntityType(typeof(AbstractCustomer));
            var customerType = model.Model.FindEntityType(typeof(Customer));
            var specialCustomerType = model.Model.FindEntityType(typeof(SpecialCustomer));
            var extraSpecialCustomerType = model.Model.FindEntityType(typeof(ExtraSpecialCustomer));
            var orderDetailsOwnership = orderType.FindNavigation(nameof(Order.Details)).ForeignKey;
            var orderDetailsType = orderDetailsOwnership.DeclaringEntityType;

            Assert.Empty(ordersInsertSproc.GetReferencingRowInternalForeignKeys(orderType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersInsertSproc.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersInsertSproc.GetReferencingRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersInsertSproc.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersInsertSproc.GetRowInternalForeignKeys(specialCustomerType)).Message);
            Assert.False(ordersInsertSproc.IsOptional(orderType));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(OrderDetails), ordersInsertSproc.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersInsertSproc.IsOptional(orderDetailsType)).Message);
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), ordersInsertSproc.Name),
                Assert.Throws<InvalidOperationException>(
                    () => ordersInsertSproc.IsOptional(specialCustomerType)).Message);

            var tableMapping = orderInsertMapping.TableMapping;
            if (mappedToTables)
            {
                Assert.Equal("Order", tableMapping.Table.Name);
                Assert.Same(orderInsertMapping, tableMapping.InsertStoredProcedureMapping);
            }
            else
            {
                Assert.Null(tableMapping);
            }

            var billingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.BillingAddress)).ForeignKey;
            Assert.True(billingAddressOwnership.IsRequiredDependent);

            var billingAddressType = billingAddressOwnership.DeclaringEntityType;

            var shippingAddressOwnership = orderDetailsType.FindNavigation(nameof(OrderDetails.ShippingAddress)).ForeignKey;
            Assert.True(shippingAddressOwnership.IsRequiredDependent);

            var billingAddressInsertMapping = billingAddressType.GetInsertStoredProcedureMappings().Single();
            Assert.Same(billingAddressType.GetInsertStoredProcedure(), billingAddressInsertMapping.StoredProcedure);
            Assert.Same(billingAddressType, billingAddressInsertMapping.StoredProcedure.EntityType);

            var billingAddressInsertSproc = billingAddressInsertMapping.StoreStoredProcedure;
            Assert.Equal("BillingAddress_Insert", billingAddressInsertSproc.Name);
            Assert.Null(billingAddressInsertSproc.Schema);
            Assert.Equal(
                new[] { "OrderDetails.BillingAddress#Address" },
                billingAddressInsertSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

            Assert.Equal(
                new[] { nameof(Address.City), nameof(Address.Street), "OrderDetailsOrderId" },
                billingAddressInsertSproc.Parameters.Select(m => m.Name));

            Assert.Empty(billingAddressInsertSproc.ResultColumns.Select(m => m.Name));

            var billingAddressUpdateMapping = billingAddressType.GetUpdateStoredProcedureMappings().Single();
            Assert.Same(billingAddressType.GetUpdateStoredProcedure(), billingAddressUpdateMapping.StoredProcedure);
            Assert.Same(billingAddressType, billingAddressUpdateMapping.StoredProcedure.EntityType);

            var billingAddressUpdateSproc = billingAddressUpdateMapping.StoreStoredProcedure;
            Assert.Equal("BillingAddress_Update", billingAddressUpdateSproc.Name);
            Assert.Null(billingAddressUpdateSproc.Schema);
            Assert.Equal(
                new[] { "OrderDetails.BillingAddress#Address" },
                billingAddressUpdateSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

            Assert.Equal(
                new[] { nameof(Address.City), nameof(Address.Street), "OrderDetailsOrderId_Original" },
                billingAddressUpdateSproc.Parameters.Select(m => m.Name));

            Assert.Empty(billingAddressUpdateSproc.ResultColumns.Select(m => m.Name));

            var billingAddressDeleteMapping = billingAddressType.GetDeleteStoredProcedureMappings().Single();
            Assert.Same(billingAddressType.GetDeleteStoredProcedure(), billingAddressDeleteMapping.StoredProcedure);
            Assert.Same(billingAddressType, billingAddressDeleteMapping.StoredProcedure.EntityType);

            var billingAddressDeleteSproc = billingAddressDeleteMapping.StoreStoredProcedure;
            Assert.Equal("BillingAddress_Delete", billingAddressDeleteSproc.Name);
            Assert.Null(billingAddressDeleteSproc.Schema);
            Assert.Equal(
                new[] { "OrderDetails.BillingAddress#Address" },
                billingAddressDeleteSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

            Assert.Equal(
                new[] { "OrderDetailsOrderId_Original" },
                billingAddressDeleteSproc.Parameters.Select(m => m.Name));

            Assert.Empty(billingAddressDeleteSproc.ResultColumns.Select(m => m.Name));

            Assert.Equal(new[] { orderDate }, orderDateParameter.PropertyMappings.Select(m => m.Property));

            var specialCustomerInsertSproc =
                specialCustomerType.GetInsertStoredProcedureMappings().Last().StoreStoredProcedure;
            var specialCustomerUpdateSproc =
                specialCustomerType.GetUpdateStoredProcedureMappings().Last().StoreStoredProcedure;
            var specialCustomerDeleteSproc =
                specialCustomerType.GetDeleteStoredProcedureMappings().Last().StoreStoredProcedure;

            var customerInsertSproc = customerType.GetInsertStoredProcedureMappings().Last().StoreStoredProcedure;
            Assert.False(customerInsertSproc.IsOptional(customerType));
            if (mapping == Mapping.TPC)
            {
                Assert.Equal(
                    RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerInsertSproc.Name),
                    Assert.Throws<InvalidOperationException>(
                        () => customerInsertSproc.IsOptional(specialCustomerType)).Message);
            }
            else
            {
                Assert.False(customerInsertSproc.IsOptional(specialCustomerType));
                Assert.False(customerInsertSproc.IsOptional(extraSpecialCustomerType));
            }

            var customerUpdateSproc = customerType.GetUpdateStoredProcedureMappings().Last().StoreStoredProcedure;
            Assert.False(customerUpdateSproc.IsOptional(customerType));
            if (mapping == Mapping.TPC)
            {
                Assert.Equal(
                    RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerUpdateSproc.Name),
                    Assert.Throws<InvalidOperationException>(
                        () => customerUpdateSproc.IsOptional(specialCustomerType)).Message);
            }
            else
            {
                Assert.False(customerUpdateSproc.IsOptional(specialCustomerType));
                Assert.False(customerUpdateSproc.IsOptional(extraSpecialCustomerType));
            }

            var customerDeleteSproc = customerType.GetDeleteStoredProcedureMappings().Last().StoreStoredProcedure;
            Assert.False(customerDeleteSproc.IsOptional(customerType));
            if (mapping == Mapping.TPC)
            {
                Assert.Equal(
                    RelationalStrings.TableNotMappedEntityType(nameof(SpecialCustomer), customerDeleteSproc.Name),
                    Assert.Throws<InvalidOperationException>(
                        () => customerDeleteSproc.IsOptional(specialCustomerType)).Message);
            }
            else
            {
                Assert.False(customerDeleteSproc.IsOptional(specialCustomerType));
                Assert.False(customerDeleteSproc.IsOptional(extraSpecialCustomerType));
            }

            var customerPk = specialCustomerType.FindPrimaryKey();
            var idProperty = customerPk.Properties.Single();

            if (mapping == Mapping.TPT)
            {
                var baseInsertMapping = abstractBaseType.GetInsertStoredProcedureMappings().Single();
                Assert.True(baseInsertMapping.IncludesDerivedTypes);
                Assert.Same(abstractBaseType.GetInsertStoredProcedure(), baseInsertMapping.StoredProcedure);

                Assert.Equal(
                    new[] { nameof(AbstractBase.Id), "SpecialtyAk" },
                    baseInsertMapping.ParameterMappings.Select(m => m.Property.Name));

                Assert.Empty(baseInsertMapping.ResultColumnMappings.Select(m => m.Property.Name));
                Assert.Equal(baseInsertMapping.ResultColumnMappings, baseInsertMapping.ColumnMappings);

                var baseInsertSproc = baseInsertMapping.StoreStoredProcedure;
                Assert.Equal("AbstractBase_Insert", baseInsertSproc.Name);
                Assert.Equal("Customer_Insert", customerInsertSproc.Name);
                Assert.Empty(abstractCustomerType.GetInsertStoredProcedureMappings().Where(m => m.IncludesDerivedTypes != false));
                Assert.Equal(
                    "SpecialCustomer_Insert",
                    specialCustomerType.GetInsertStoredProcedureMappings().Single(m => m.IncludesDerivedTypes == true).StoreStoredProcedure.Name);
                Assert.Null(baseInsertSproc.Schema);
                Assert.Equal(
                    new[]
                    {
                        nameof(AbstractBase),
                        nameof(AbstractCustomer),
                        nameof(Customer),
                        nameof(SpecialCustomer),
                        nameof(ExtraSpecialCustomer)
                    },
                    baseInsertSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

                Assert.Equal(
                    new[] { "InsertId", "SpecialtyAk" },
                    baseInsertSproc.Parameters.Select(m => m.Name));

                Assert.Empty(baseInsertSproc.ResultColumns.Select(m => m.Name));
                Assert.Equal(baseInsertSproc.ResultColumns, baseInsertSproc.Columns);

                var baseUpdateMapping = abstractBaseType.GetUpdateStoredProcedureMappings().Single();
                Assert.True(baseUpdateMapping.IncludesDerivedTypes);
                Assert.Same(abstractBaseType.GetUpdateStoredProcedure(), baseUpdateMapping.StoredProcedure);

                Assert.Equal(
                    new[] { nameof(AbstractBase.Id), "SpecialtyAk" },
                    baseUpdateMapping.ParameterMappings.Select(m => m.Property.Name));

                Assert.Empty(
                    baseUpdateMapping.ResultColumnMappings.Select(m => m.Property.Name));
                Assert.Equal(baseUpdateMapping.ResultColumnMappings, baseUpdateMapping.ColumnMappings);

                var baseUpdateSproc = baseUpdateMapping.StoreStoredProcedure;
                Assert.Equal("AbstractBase_Update", baseUpdateSproc.Name);
                Assert.Equal("Customer_Update", customerUpdateSproc.Name);
                Assert.Empty(abstractCustomerType.GetUpdateStoredProcedureMappings().Where(m => m.IncludesDerivedTypes != false));
                Assert.Equal(
                    "SpecialCustomer_Update",
                    specialCustomerType.GetUpdateStoredProcedureMappings().Single(m => m.IncludesDerivedTypes == true).StoreStoredProcedure.Name);

                Assert.Null(baseUpdateSproc.Schema);
                Assert.Equal(
                    new[]
                    {
                        nameof(AbstractBase),
                        nameof(AbstractCustomer),
                        nameof(Customer),
                        nameof(SpecialCustomer),
                        nameof(ExtraSpecialCustomer)
                    },
                    baseUpdateSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

                Assert.Equal(
                    new[] { "UpdateId", "SpecialtyAk_Original" },
                    baseUpdateSproc.Parameters.Select(m => m.Name));

                Assert.Empty(baseUpdateSproc.ResultColumns.Select(m => m.Name));
                Assert.Equal(baseUpdateSproc.ResultColumns, baseUpdateSproc.Columns);

                var baseDeleteMapping = abstractBaseType.GetDeleteStoredProcedureMappings().Single();
                Assert.True(baseDeleteMapping.IncludesDerivedTypes);
                Assert.Same(abstractBaseType.GetDeleteStoredProcedure(), baseDeleteMapping.StoredProcedure);

                Assert.Equal(
                    new[] { nameof(AbstractBase.Id) },
                    baseDeleteMapping.ParameterMappings.Select(m => m.Property.Name));

                Assert.Empty(
                    baseDeleteMapping.ResultColumnMappings.Select(m => m.Property.Name));
                Assert.Equal(baseDeleteMapping.ResultColumnMappings, baseDeleteMapping.ColumnMappings);

                var baseDeleteSproc = baseDeleteMapping.StoreStoredProcedure;
                Assert.Equal("AbstractBase_Delete", baseDeleteSproc.Name);
                Assert.Equal("Customer_Delete", customerDeleteSproc.Name);
                Assert.Empty(abstractCustomerType.GetDeleteStoredProcedureMappings().Where(m => m.IncludesDerivedTypes != false));
                Assert.Equal(
                    "SpecialCustomer_Delete",
                    specialCustomerType.GetDeleteStoredProcedureMappings().Single(m => m.IncludesDerivedTypes == true).StoreStoredProcedure.Name);

                Assert.Null(baseDeleteSproc.Schema);
                Assert.Equal(
                    new[]
                    {
                        nameof(AbstractBase),
                        nameof(AbstractCustomer),
                        nameof(Customer),
                        nameof(SpecialCustomer),
                        nameof(ExtraSpecialCustomer)
                    },
                    baseDeleteSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

                Assert.Equal(
                    new[] { "DeleteId" },
                    baseDeleteSproc.Parameters.Select(m => m.Name));

                Assert.Empty(baseDeleteSproc.ResultColumns.Select(m => m.Name));
                Assert.Equal(baseDeleteSproc.ResultColumns, baseDeleteSproc.Columns);

                Assert.Equal(3, specialCustomerType.GetInsertStoredProcedureMappings().Count());
                Assert.Null(specialCustomerType.GetInsertStoredProcedureMappings().First().IsSplitEntityTypePrincipal);
                Assert.False(specialCustomerType.GetInsertStoredProcedureMappings().First().IncludesDerivedTypes);
                Assert.Null(specialCustomerType.GetInsertStoredProcedureMappings().Last().IsSplitEntityTypePrincipal);
                Assert.True(specialCustomerType.GetInsertStoredProcedureMappings().Last().IncludesDerivedTypes);

                Assert.Equal("SpecialCustomer_Insert", specialCustomerInsertSproc.Name);
                Assert.Single(specialCustomerInsertSproc.ResultColumns);
                Assert.Equal(4, specialCustomerInsertSproc.Parameters.Count());

                Assert.Null(
                    specialCustomerInsertSproc.EntityTypeMappings.Single(m => m.TypeBase == specialCustomerType).IsSharedTablePrincipal);

                var specialtyInsertParameter =
                    specialCustomerInsertSproc.Parameters.Single(c => c.Name == nameof(SpecialCustomer.Specialty));

                Assert.False(specialtyInsertParameter.IsNullable);

                var specialtyProperty = specialtyInsertParameter.PropertyMappings.First().Property;

                Assert.Equal(
                    RelationalStrings.PropertyNotMappedToTable(
                        nameof(SpecialCustomer.Specialty), nameof(SpecialCustomer), "Customer_Insert"),
                    Assert.Throws<InvalidOperationException>(
                            () =>
                                specialtyProperty.IsColumnNullable(
                                    StoreObjectIdentifier.InsertStoredProcedure(customerInsertSproc.Name, customerInsertSproc.Schema)))
                        .Message);

                var abstractStringParameter =
                    specialCustomerInsertSproc.Parameters.Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                Assert.False(abstractStringParameter.IsNullable);
                Assert.Equal(2, abstractStringParameter.PropertyMappings.Count);

                var abstractStringProperty = abstractStringParameter.PropertyMappings.First().Property;
                Assert.Equal(2, abstractStringProperty.GetInsertStoredProcedureParameterMappings().Count());
                Assert.Equal(
                    new[]
                    {
                        StoreObjectIdentifier.InsertStoredProcedure(specialCustomerInsertSproc.Name, specialCustomerInsertSproc.Schema)
                    },
                    abstractStringProperty.GetMappedStoreObjects(StoreObjectType.InsertStoredProcedure));

                var extraSpecialCustomerInsertSproc =
                    extraSpecialCustomerType.GetInsertStoredProcedureMappings().Select(t => t.StoreStoredProcedure)
                        .First(t => t.Name == "ExtraSpecialCustomer_Insert");

                var idPropertyInsertParameter = baseInsertSproc.FindParameter(idProperty)!;
                var idPropertyInsertParameterMapping = idProperty.GetInsertStoredProcedureParameterMappings().First();
                Assert.Same(idPropertyInsertParameter, baseInsertSproc.FindParameter("InsertId"));
                Assert.Same(idPropertyInsertParameter, idPropertyInsertParameterMapping.Column);
                Assert.Equal("InsertId", idPropertyInsertParameter.Name);
                Assert.Equal("default_int_mapping", idPropertyInsertParameter.StoreType);
                Assert.False(idPropertyInsertParameter.IsNullable);
                Assert.Same(baseInsertSproc, idPropertyInsertParameter.StoredProcedure);
                Assert.Same(idPropertyInsertParameter.StoredProcedure, idPropertyInsertParameter.Table);
                Assert.Same(idPropertyInsertParameterMapping, idPropertyInsertParameter.FindParameterMapping(abstractBaseType));

                Assert.Equal(2, idProperty.GetInsertStoredProcedureResultColumnMappings().Count());
                Assert.Equal(10, idProperty.GetInsertStoredProcedureParameterMappings().Count());
                Assert.Equal(
                    new[]
                    {
                        StoreObjectIdentifier.InsertStoredProcedure(baseInsertSproc.Name, baseInsertSproc.Schema),
                        StoreObjectIdentifier.InsertStoredProcedure(customerInsertSproc.Name, customerInsertSproc.Schema),
                        StoreObjectIdentifier.InsertStoredProcedure(specialCustomerInsertSproc.Name, specialCustomerInsertSproc.Schema),
                        StoreObjectIdentifier.InsertStoredProcedure(
                            extraSpecialCustomerInsertSproc.Name, extraSpecialCustomerInsertSproc.Schema)
                    },
                    idProperty.GetMappedStoreObjects(StoreObjectType.InsertStoredProcedure));

                var extraSpecialCustomerUpdateSproc =
                    extraSpecialCustomerType.GetUpdateStoredProcedureMappings().Select(t => t.StoreStoredProcedure)
                        .First(t => t.Name == "ExtraSpecialCustomer_Update");

                var idPropertyUpdateParameter = baseUpdateSproc.FindParameter(idProperty)!;
                var idPropertyUpdateParameterMapping = idProperty.GetUpdateStoredProcedureParameterMappings().First();
                Assert.Same(idPropertyUpdateParameter, baseUpdateSproc.FindParameter("UpdateId"));
                Assert.Same(idPropertyUpdateParameter, idPropertyUpdateParameterMapping.StoreParameter);
                Assert.Equal("UpdateId", idPropertyUpdateParameter.Name);
                Assert.Equal("default_int_mapping", idPropertyUpdateParameter.StoreType);
                Assert.Equal(ParameterDirection.Input, idPropertyUpdateParameter.Direction);
                Assert.False(idPropertyUpdateParameter.IsNullable);
                Assert.Same(baseUpdateSproc, idPropertyUpdateParameter.StoredProcedure);
                Assert.Same(idPropertyUpdateParameter.StoredProcedure, idPropertyUpdateParameter.Table);
                Assert.Same(idPropertyUpdateParameterMapping, idPropertyUpdateParameter.FindParameterMapping(abstractBaseType));

                Assert.Empty(idProperty.GetUpdateStoredProcedureResultColumnMappings());
                Assert.Equal(12, idProperty.GetUpdateStoredProcedureParameterMappings().Count());
                Assert.Equal(
                    new[]
                    {
                        StoreObjectIdentifier.UpdateStoredProcedure(baseUpdateSproc.Name, baseUpdateSproc.Schema),
                        StoreObjectIdentifier.UpdateStoredProcedure(customerUpdateSproc.Name, customerUpdateSproc.Schema),
                        StoreObjectIdentifier.UpdateStoredProcedure(specialCustomerUpdateSproc.Name, specialCustomerUpdateSproc.Schema),
                        StoreObjectIdentifier.UpdateStoredProcedure(
                            extraSpecialCustomerUpdateSproc.Name, extraSpecialCustomerUpdateSproc.Schema)
                    },
                    idProperty.GetMappedStoreObjects(StoreObjectType.UpdateStoredProcedure));

                var extraSpecialCustomerDeleteSproc =
                    extraSpecialCustomerType.GetDeleteStoredProcedureMappings().Select(t => t.StoreStoredProcedure)
                        .First(t => t.Name == "ExtraSpecialCustomer_Delete");

                var idPropertyDeleteParameter = baseDeleteSproc.FindParameter(idProperty)!;
                var idPropertyDeleteParameterMapping = idProperty.GetDeleteStoredProcedureParameterMappings().First();
                Assert.Same(idPropertyDeleteParameter, baseDeleteSproc.FindParameter("DeleteId"));
                Assert.Same(idPropertyDeleteParameter, idPropertyDeleteParameterMapping.StoreParameter);
                Assert.Equal("DeleteId", idPropertyDeleteParameter.Name);
                Assert.Equal("default_int_mapping", idPropertyDeleteParameter.StoreType);
                Assert.Equal(ParameterDirection.Input, idPropertyDeleteParameter.Direction);
                Assert.False(idPropertyDeleteParameter.IsNullable);
                Assert.Same(baseDeleteSproc, idPropertyDeleteParameter.StoredProcedure);
                Assert.Same(idPropertyDeleteParameter.StoredProcedure, idPropertyDeleteParameter.Table);
                Assert.Same(idPropertyDeleteParameterMapping, idPropertyDeleteParameter.FindParameterMapping(abstractBaseType));

                Assert.Equal(12, idProperty.GetDeleteStoredProcedureParameterMappings().Count());
                Assert.Equal(
                    new[]
                    {
                        StoreObjectIdentifier.DeleteStoredProcedure(baseDeleteSproc.Name, baseDeleteSproc.Schema),
                        StoreObjectIdentifier.DeleteStoredProcedure(customerDeleteSproc.Name, customerDeleteSproc.Schema),
                        StoreObjectIdentifier.DeleteStoredProcedure(specialCustomerDeleteSproc.Name, specialCustomerDeleteSproc.Schema),
                        StoreObjectIdentifier.DeleteStoredProcedure(
                            extraSpecialCustomerDeleteSproc.Name, extraSpecialCustomerDeleteSproc.Schema)
                    },
                    idProperty.GetMappedStoreObjects(StoreObjectType.DeleteStoredProcedure));
            }
            else // Non-TPT
            {
                var specialCustomerInsertMapping = specialCustomerType.GetInsertStoredProcedureMappings().Single();
                Assert.Null(specialCustomerInsertMapping.IsSplitEntityTypePrincipal);

                var specialtyParameter = specialCustomerInsertSproc.Parameters.Single(c => c.Name == nameof(SpecialCustomer.Specialty));

                if (mapping == Mapping.TPH)
                {
                    var baseInsertMapping = abstractBaseType.GetInsertStoredProcedureMappings().Single();
                    Assert.True(baseInsertMapping.IncludesDerivedTypes);
                    Assert.Same(abstractBaseType.GetInsertStoredProcedure(), baseInsertMapping.StoredProcedure);

                    Assert.Equal(
                        new[] { "Discriminator", "SpecialtyAk" },
                        baseInsertMapping.ParameterMappings.Select(m => m.Property.Name));

                    Assert.Equal(
                        new[] { nameof(AbstractBase.Id) },
                        baseInsertMapping.ResultColumnMappings.Select(m => m.Property.Name));
                    Assert.Equal(baseInsertMapping.ResultColumnMappings, baseInsertMapping.ColumnMappings);

                    var baseInsertSproc = baseInsertMapping.StoreStoredProcedure;
                    Assert.Equal("AbstractBase_Insert", baseInsertSproc.Name);
                    Assert.Same(baseInsertSproc, customerInsertSproc);
                    Assert.Same(baseInsertSproc, abstractBaseType.GetInsertStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseInsertSproc, abstractCustomerType.GetInsertStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseInsertSproc, specialCustomerType.GetInsertStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseInsertSproc, baseInsertMapping.Table);
                    Assert.Null(baseInsertSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            nameof(AbstractBase),
                            nameof(AbstractCustomer),
                            nameof(Customer),
                            nameof(SpecialCustomer),
                            nameof(ExtraSpecialCustomer)
                        },
                        baseInsertSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

                    Assert.Equal(
                        new[]
                        {
                            "Discriminator",
                            nameof(SpecialCustomer.Specialty),
                            nameof(SpecialCustomer.RelatedCustomerSpecialty),
                            "SpecialtyAk",
                            "AnotherRelatedCustomerId",
                            nameof(Customer.EnumValue),
                            nameof(Customer.Name),
                            nameof(Customer.SomeShort),
                            nameof(AbstractCustomer.AbstractString),
                        },
                        baseInsertSproc.Parameters.Select(m => m.Name));

                    Assert.Equal(new[] { "InsertId" }, baseInsertSproc.ResultColumns.Select(m => m.Name));
                    Assert.Equal(baseInsertSproc.ResultColumns, baseInsertSproc.Columns);

                    Assert.True(specialCustomerInsertMapping.IncludesDerivedTypes);
                    Assert.Same(customerUpdateSproc, specialCustomerUpdateSproc);

                    Assert.Equal(5, specialCustomerInsertSproc.EntityTypeMappings.Count());
                    Assert.Null(specialCustomerInsertSproc.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.Null(specialCustomerInsertSproc.EntityTypeMappings.Last().IsSharedTablePrincipal);

                    Assert.Single(specialCustomerInsertSproc.Columns);
                    Assert.Equal(9, specialCustomerInsertSproc.Parameters.Count());

                    var baseUpdateMapping = abstractBaseType.GetUpdateStoredProcedureMappings().Single();
                    Assert.True(baseUpdateMapping.IncludesDerivedTypes);
                    Assert.Same(abstractBaseType.GetUpdateStoredProcedure(), baseUpdateMapping.StoredProcedure);

                    Assert.Equal(
                        new[] { nameof(AbstractBase.Id) },
                        baseUpdateMapping.ParameterMappings.Select(m => m.Property.Name));

                    Assert.Empty(
                        baseUpdateMapping.ResultColumnMappings.Select(m => m.Property.Name));
                    Assert.Equal(baseUpdateMapping.ResultColumnMappings, baseUpdateMapping.ColumnMappings);

                    var baseUpdateSproc = baseUpdateMapping.StoreStoredProcedure;
                    Assert.Equal("AbstractBase_Update", baseUpdateSproc.Name);
                    Assert.Same(baseUpdateSproc, customerUpdateSproc);
                    Assert.Same(baseUpdateSproc, abstractBaseType.GetUpdateStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseUpdateSproc, abstractCustomerType.GetUpdateStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseUpdateSproc, specialCustomerType.GetUpdateStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseUpdateSproc, baseUpdateMapping.Table);
                    Assert.Null(baseUpdateSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            nameof(AbstractBase),
                            nameof(AbstractCustomer),
                            nameof(Customer),
                            nameof(SpecialCustomer),
                            nameof(ExtraSpecialCustomer)
                        },
                        baseUpdateSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

                    Assert.Equal(
                        new[]
                        {
                            "UpdateId",
                            nameof(SpecialCustomer.Specialty),
                            nameof(SpecialCustomer.RelatedCustomerSpecialty),
                            "AnotherRelatedCustomerId",
                            nameof(Customer.EnumValue),
                            nameof(Customer.Name),
                            nameof(Customer.SomeShort),
                            nameof(AbstractCustomer.AbstractString),
                        },
                        baseUpdateSproc.Parameters.Select(m => m.Name));

                    Assert.Empty(baseUpdateSproc.ResultColumns.Select(m => m.Name));
                    Assert.Equal(baseUpdateSproc.ResultColumns, baseUpdateSproc.Columns);

                    var baseDeleteMapping = abstractBaseType.GetDeleteStoredProcedureMappings().Single();
                    Assert.True(baseDeleteMapping.IncludesDerivedTypes);
                    Assert.Same(abstractBaseType.GetDeleteStoredProcedure(), baseDeleteMapping.StoredProcedure);

                    Assert.Equal(
                        new[] { nameof(AbstractBase.Id) },
                        baseDeleteMapping.ParameterMappings.Select(m => m.Property.Name));

                    Assert.Empty(
                        baseDeleteMapping.ResultColumnMappings.Select(m => m.Property.Name));
                    Assert.Equal(baseDeleteMapping.ResultColumnMappings, baseDeleteMapping.ColumnMappings);

                    var baseDeleteSproc = baseDeleteMapping.StoreStoredProcedure;
                    Assert.Equal("AbstractBase_Delete", baseDeleteSproc.Name);
                    Assert.Same(baseDeleteSproc, customerDeleteSproc);
                    Assert.Same(baseDeleteSproc, abstractBaseType.GetDeleteStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseDeleteSproc, abstractCustomerType.GetDeleteStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseDeleteSproc, specialCustomerType.GetDeleteStoredProcedureMappings().Single().StoreStoredProcedure);
                    Assert.Same(baseDeleteSproc, baseDeleteMapping.Table);
                    Assert.Null(baseDeleteSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            nameof(AbstractBase),
                            nameof(AbstractCustomer),
                            nameof(Customer),
                            nameof(SpecialCustomer),
                            nameof(ExtraSpecialCustomer)
                        },
                        baseDeleteSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));

                    Assert.Equal(
                        new[] { "DeleteId" },
                        baseDeleteSproc.Parameters.Select(m => m.Name));

                    Assert.Empty(baseDeleteSproc.ResultColumns.Select(m => m.Name));
                    Assert.Equal(baseDeleteSproc.ResultColumns, baseDeleteSproc.Columns);

                    Assert.True(specialCustomerInsertMapping.IncludesDerivedTypes);
                    Assert.Same(customerInsertSproc, specialCustomerInsertSproc);

                    Assert.Equal(5, specialCustomerInsertSproc.EntityTypeMappings.Count());
                    Assert.Null(specialCustomerInsertSproc.EntityTypeMappings.First().IsSharedTablePrincipal);
                    Assert.Null(specialCustomerInsertSproc.EntityTypeMappings.Last().IsSharedTablePrincipal);

                    Assert.Single(specialCustomerInsertSproc.Columns);
                    Assert.Equal(9, specialCustomerInsertSproc.Parameters.Count());

                    Assert.True(specialtyParameter.IsNullable);

                    var abstractStringColumn =
                        specialCustomerInsertSproc.Parameters.Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                    Assert.True(specialtyParameter.IsNullable);
                    Assert.Equal(2, specialtyParameter.PropertyMappings.Count);

                    var abstractStringProperty = abstractStringColumn.PropertyMappings.First().Property;
                    Assert.Equal(3, abstractStringProperty.GetInsertStoredProcedureParameterMappings().Count());
                    Assert.Equal(
                        new[] { StoreObjectIdentifier.InsertStoredProcedure(customerInsertSproc.Name, customerInsertSproc.Schema) },
                        abstractStringProperty.GetMappedStoreObjects(StoreObjectType.InsertStoredProcedure));

                    var idPropertyInsertColumn = baseInsertSproc.FindResultColumn(idProperty)!;
                    var idPropertyInsertColumnMapping = idProperty.GetInsertStoredProcedureResultColumnMappings().First();
                    Assert.Same(idPropertyInsertColumn, baseInsertSproc.FindResultColumn("InsertId"));
                    Assert.Same(idPropertyInsertColumn, idPropertyInsertColumnMapping.Column);
                    Assert.Equal("InsertId", idPropertyInsertColumn.Name);
                    Assert.Equal("default_int_mapping", idPropertyInsertColumn.StoreType);
                    Assert.False(idPropertyInsertColumn.IsNullable);
                    Assert.Same(baseInsertSproc, idPropertyInsertColumn.StoredProcedure);
                    Assert.Same(idPropertyInsertColumn.StoredProcedure, idPropertyInsertColumn.Table);
                    Assert.Same(idPropertyInsertColumnMapping, idPropertyInsertColumn.FindColumnMapping(abstractBaseType));

                    Assert.Empty(idProperty.GetInsertStoredProcedureParameterMappings());
                    Assert.Equal(5, idProperty.GetInsertStoredProcedureResultColumnMappings().Count());
                    Assert.Equal(
                        new[] { StoreObjectIdentifier.InsertStoredProcedure(customerInsertSproc.Name, customerInsertSproc.Schema) },
                        idProperty.GetMappedStoreObjects(StoreObjectType.InsertStoredProcedure));

                    var idPropertyUpdateParameter = baseUpdateSproc.FindParameter(idProperty)!;
                    var idPropertyUpdateParameterMapping = idProperty.GetUpdateStoredProcedureParameterMappings().First();
                    Assert.Same(idPropertyUpdateParameter, baseUpdateSproc.FindParameter("UpdateId"));
                    Assert.Same(idPropertyUpdateParameter, idPropertyUpdateParameterMapping.StoreParameter);
                    Assert.Equal("UpdateId", idPropertyUpdateParameter.Name);
                    Assert.Equal("default_int_mapping", idPropertyUpdateParameter.StoreType);
                    Assert.Equal(ParameterDirection.Input, idPropertyUpdateParameter.Direction);
                    Assert.False(idPropertyUpdateParameter.IsNullable);
                    Assert.Same(baseUpdateSproc, idPropertyUpdateParameter.StoredProcedure);
                    Assert.Same(idPropertyUpdateParameter.StoredProcedure, idPropertyUpdateParameter.Table);
                    Assert.Same(idPropertyUpdateParameterMapping, idPropertyUpdateParameter.FindParameterMapping(abstractBaseType));

                    Assert.Empty(idProperty.GetUpdateStoredProcedureResultColumnMappings());
                    Assert.Equal(5, idProperty.GetUpdateStoredProcedureParameterMappings().Count());
                    Assert.Equal(
                        new[] { StoreObjectIdentifier.UpdateStoredProcedure(customerUpdateSproc.Name, customerUpdateSproc.Schema) },
                        idProperty.GetMappedStoreObjects(StoreObjectType.UpdateStoredProcedure));

                    var idPropertyDeleteParameter = baseDeleteSproc.FindParameter(idProperty)!;
                    var idPropertyDeleteParameterMapping = idProperty.GetDeleteStoredProcedureParameterMappings().First();
                    Assert.Same(idPropertyDeleteParameter, baseDeleteSproc.FindParameter("DeleteId"));
                    Assert.Same(idPropertyDeleteParameter, idPropertyDeleteParameterMapping.StoreParameter);
                    Assert.Equal("DeleteId", idPropertyDeleteParameter.Name);
                    Assert.Equal("default_int_mapping", idPropertyDeleteParameter.StoreType);
                    Assert.Equal(ParameterDirection.Input, idPropertyDeleteParameter.Direction);
                    Assert.False(idPropertyDeleteParameter.IsNullable);
                    Assert.Same(baseDeleteSproc, idPropertyDeleteParameter.StoredProcedure);
                    Assert.Same(idPropertyDeleteParameter.StoredProcedure, idPropertyDeleteParameter.Table);
                    Assert.Same(idPropertyDeleteParameterMapping, idPropertyDeleteParameter.FindParameterMapping(abstractBaseType));

                    Assert.Equal(5, idProperty.GetDeleteStoredProcedureParameterMappings().Count());
                    Assert.Equal(
                        new[] { StoreObjectIdentifier.DeleteStoredProcedure(customerDeleteSproc.Name, customerDeleteSproc.Schema) },
                        idProperty.GetMappedStoreObjects(StoreObjectType.DeleteStoredProcedure));
                }
                else // TPC
                {
                    Assert.Null(abstractBaseType.GetInsertStoredProcedure());
                    Assert.Null(abstractBaseType.GetUpdateStoredProcedure());
                    Assert.Null(abstractBaseType.GetDeleteStoredProcedure());

                    Assert.Equal("Customer_Insert", customerInsertSproc.Name);
                    Assert.Null(abstractCustomerType.GetInsertStoredProcedure());
                    Assert.Equal("SpecialCustomer_Insert", specialCustomerType.GetInsertStoredProcedure().Name);

                    Assert.False(specialCustomerInsertMapping.IncludesDerivedTypes);
                    Assert.NotSame(customerInsertSproc, specialCustomerInsertSproc);

                    Assert.Equal("Customer_Insert", customerInsertSproc.Name);
                    Assert.Empty(abstractCustomerType.GetInsertStoredProcedureMappings());
                    Assert.Equal(
                        "SpecialCustomer_Insert",
                        specialCustomerType.GetInsertStoredProcedureMappings().Single().StoreStoredProcedure
                            .Name);

                    Assert.Null(customerInsertSproc.Schema);
                    Assert.Equal(
                        new[] { nameof(Customer) },
                        customerInsertSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));
                    Assert.Null(customerInsertSproc.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Null(customerInsertSproc.EntityTypeMappings.Single().IsSplitEntityTypePrincipal);

                    Assert.Equal(
                        new[] { "InsertId", nameof(Customer.EnumValue), nameof(Customer.Name), nameof(Customer.SomeShort), "SpecialtyAk" },
                        customerInsertSproc.Parameters.Select(m => m.Name));

                    Assert.Empty(customerInsertSproc.ResultColumns.Select(m => m.Name));

                    Assert.Equal("Customer_Update", customerUpdateSproc.Name);
                    Assert.Empty(abstractCustomerType.GetUpdateStoredProcedureMappings());
                    Assert.Equal(
                        "SpecialCustomer_Update",
                        specialCustomerType.GetUpdateStoredProcedureMappings().Single().StoreStoredProcedure
                            .Name);
                    Assert.Null(customerUpdateSproc.Schema);
                    Assert.Equal(
                        new[] { nameof(Customer) },
                        customerUpdateSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));
                    Assert.Null(customerUpdateSproc.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Null(customerUpdateSproc.EntityTypeMappings.Single().IsSplitEntityTypePrincipal);

                    Assert.Equal(
                        new[] { "UpdateId", nameof(Customer.EnumValue), nameof(Customer.Name), nameof(Customer.SomeShort) },
                        customerUpdateSproc.Parameters.Select(m => m.Name));

                    Assert.Empty(customerUpdateSproc.ResultColumns.Select(m => m.Name));

                    Assert.Equal("Customer_Delete", customerDeleteSproc.Name);
                    Assert.Empty(abstractCustomerType.GetDeleteStoredProcedureMappings());
                    Assert.Equal(
                        "SpecialCustomer_Delete",
                        specialCustomerType.GetDeleteStoredProcedureMappings().Single().StoreStoredProcedure
                            .Name);
                    Assert.Null(customerDeleteSproc.Schema);
                    Assert.Equal(
                        new[] { nameof(Customer) },
                        customerDeleteSproc.EntityTypeMappings.Select(m => m.TypeBase.DisplayName()));
                    Assert.Null(customerDeleteSproc.EntityTypeMappings.Single().IsSharedTablePrincipal);
                    Assert.Null(customerDeleteSproc.EntityTypeMappings.Single().IsSplitEntityTypePrincipal);

                    Assert.Equal(
                        new[] { "DeleteId" },
                        customerDeleteSproc.Parameters.Select(m => m.Name));

                    Assert.Empty(customerDeleteSproc.ResultColumns.Select(m => m.Name));

                    Assert.Single(specialCustomerInsertSproc.EntityTypeMappings);

                    var abstractStringInsertParameter = specialCustomerInsertSproc.Parameters
                        .Single(c => c.Name == nameof(AbstractCustomer.AbstractString));
                    Assert.False(specialtyParameter.IsNullable);

                    var extraSpecialCustomerInsertSproc =
                        extraSpecialCustomerType.GetInsertStoredProcedureMappings().Select(t => t.StoreStoredProcedure)
                            .First(t => t.Name == "ExtraSpecialCustomer_Insert");

                    Assert.Single(extraSpecialCustomerInsertSproc.EntityTypeMappings);

                    var idPropertyInsertColumn = customerInsertSproc.FindParameter(idProperty)!;
                    var idPropertyInsertColumnMapping = idProperty.GetInsertStoredProcedureParameterMappings().First();
                    Assert.Same(idPropertyInsertColumn, customerInsertSproc.FindParameter("InsertId"));
                    Assert.Same(idPropertyInsertColumn, idPropertyInsertColumnMapping.Column);
                    Assert.Equal("InsertId", idPropertyInsertColumn.Name);
                    Assert.Equal("default_int_mapping", idPropertyInsertColumn.StoreType);
                    Assert.False(idPropertyInsertColumn.IsNullable);
                    Assert.Same(customerInsertSproc, idPropertyInsertColumn.StoredProcedure);
                    Assert.Same(idPropertyInsertColumn.StoredProcedure, idPropertyInsertColumn.Table);
                    Assert.Same(idPropertyInsertColumnMapping, idPropertyInsertColumn.FindColumnMapping(abstractBaseType));

                    Assert.Empty(idProperty.GetInsertStoredProcedureResultColumnMappings());
                    Assert.Equal(3, idProperty.GetInsertStoredProcedureParameterMappings().Count());
                    Assert.Equal(
                        new[]
                        {
                            StoreObjectIdentifier.InsertStoredProcedure(customerInsertSproc.Name, customerInsertSproc.Schema),
                            StoreObjectIdentifier.InsertStoredProcedure(
                                specialCustomerInsertSproc.Name, specialCustomerInsertSproc.Schema),
                            StoreObjectIdentifier.InsertStoredProcedure(
                                extraSpecialCustomerInsertSproc.Name, extraSpecialCustomerInsertSproc.Schema)
                        },
                        idProperty.GetMappedStoreObjects(StoreObjectType.InsertStoredProcedure));

                    var extraSpecialCustomerUpdateSproc =
                        extraSpecialCustomerType.GetUpdateStoredProcedureMappings().Select(t => t.StoreStoredProcedure)
                            .First(t => t.Name == "ExtraSpecialCustomer_Update");

                    Assert.Single(extraSpecialCustomerUpdateSproc.EntityTypeMappings);

                    var idPropertyUpdateParameter = customerUpdateSproc.FindParameter(idProperty)!;
                    var idPropertyUpdateParameterMapping = idProperty.GetUpdateStoredProcedureParameterMappings().First();
                    Assert.Same(idPropertyUpdateParameter, customerUpdateSproc.FindParameter("UpdateId"));
                    Assert.Same(idPropertyUpdateParameter, idPropertyUpdateParameterMapping.StoreParameter);
                    Assert.Equal("UpdateId", idPropertyUpdateParameter.Name);
                    Assert.Equal("default_int_mapping", idPropertyUpdateParameter.StoreType);
                    Assert.Equal(ParameterDirection.Input, idPropertyUpdateParameter.Direction);
                    Assert.False(idPropertyUpdateParameter.IsNullable);
                    Assert.Same(customerUpdateSproc, idPropertyUpdateParameter.StoredProcedure);
                    Assert.Same(idPropertyUpdateParameter.StoredProcedure, idPropertyUpdateParameter.Table);
                    Assert.Same(idPropertyUpdateParameterMapping, idPropertyUpdateParameter.FindParameterMapping(abstractBaseType));

                    Assert.Empty(idProperty.GetUpdateStoredProcedureResultColumnMappings());
                    Assert.Equal(3, idProperty.GetUpdateStoredProcedureParameterMappings().Count());
                    Assert.Equal(
                        new[]
                        {
                            StoreObjectIdentifier.UpdateStoredProcedure(customerUpdateSproc.Name, customerUpdateSproc.Schema),
                            StoreObjectIdentifier.UpdateStoredProcedure(
                                specialCustomerUpdateSproc.Name, specialCustomerUpdateSproc.Schema),
                            StoreObjectIdentifier.UpdateStoredProcedure(
                                extraSpecialCustomerUpdateSproc.Name, extraSpecialCustomerUpdateSproc.Schema)
                        },
                        idProperty.GetMappedStoreObjects(StoreObjectType.UpdateStoredProcedure));

                    var extraSpecialCustomerDeleteSproc =
                        extraSpecialCustomerType.GetDeleteStoredProcedureMappings().Select(t => t.StoreStoredProcedure)
                            .First(t => t.Name == "ExtraSpecialCustomer_Delete");

                    Assert.Single(extraSpecialCustomerDeleteSproc.EntityTypeMappings);

                    var idPropertyDeleteParameter = customerDeleteSproc.FindParameter(idProperty)!;
                    var idPropertyDeleteParameterMapping = idProperty.GetDeleteStoredProcedureParameterMappings().First();
                    Assert.Same(idPropertyDeleteParameter, customerDeleteSproc.FindParameter("DeleteId"));
                    Assert.Same(idPropertyDeleteParameter, idPropertyDeleteParameterMapping.StoreParameter);
                    Assert.Equal("DeleteId", idPropertyDeleteParameter.Name);
                    Assert.Equal("default_int_mapping", idPropertyDeleteParameter.StoreType);
                    Assert.Equal(ParameterDirection.Input, idPropertyDeleteParameter.Direction);
                    Assert.False(idPropertyDeleteParameter.IsNullable);
                    Assert.Same(customerDeleteSproc, idPropertyDeleteParameter.StoredProcedure);
                    Assert.Same(idPropertyDeleteParameter.StoredProcedure, idPropertyDeleteParameter.Table);
                    Assert.Same(idPropertyDeleteParameterMapping, idPropertyDeleteParameter.FindParameterMapping(abstractBaseType));

                    Assert.Equal(3, idProperty.GetDeleteStoredProcedureParameterMappings().Count());
                    Assert.Equal(
                        new[]
                        {
                            StoreObjectIdentifier.DeleteStoredProcedure(customerDeleteSproc.Name, customerDeleteSproc.Schema),
                            StoreObjectIdentifier.DeleteStoredProcedure(
                                specialCustomerDeleteSproc.Name, specialCustomerDeleteSproc.Schema),
                            StoreObjectIdentifier.DeleteStoredProcedure(
                                extraSpecialCustomerDeleteSproc.Name, extraSpecialCustomerDeleteSproc.Schema)
                        },
                        idProperty.GetMappedStoreObjects(StoreObjectType.DeleteStoredProcedure));
                }
            }
        }

        private IRelationalModel CreateTestModel(
            bool mapToTables = false,
            bool mapToViews = false,
            bool mapToSprocs = false,
            Mapping mapping = Mapping.TPH)
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
                            cb.ToTable(_ => { });
                        }

                        if (mapToSprocs)
                        {
                            if (mapping == Mapping.TPH)
                            {
                                cb
                                    .InsertUsingStoredProcedure(
                                        s => s
                                            .HasResultColumn(b => b.Id, p => p.HasName("InsertId"))
                                            .HasParameter("Discriminator")
                                            .HasParameter((SpecialCustomer c) => c.Specialty)
                                            .HasParameter((SpecialCustomer c) => c.RelatedCustomerSpecialty)
                                            .HasParameter("SpecialtyAk")
                                            .HasParameter("AnotherRelatedCustomerId")
                                            .HasParameter((Customer c) => c.EnumValue)
                                            .HasParameter((Customer c) => c.Name)
                                            .HasParameter((Customer c) => c.SomeShort)
                                            .HasParameter((AbstractCustomer c) => c.AbstractString))
                                    .UpdateUsingStoredProcedure(
                                        s => s
                                            .HasOriginalValueParameter(b => b.Id, p => p.HasName("UpdateId"))
                                            .HasParameter((SpecialCustomer c) => c.Specialty)
                                            .HasParameter((SpecialCustomer c) => c.RelatedCustomerSpecialty)
                                            .HasParameter("AnotherRelatedCustomerId")
                                            .HasParameter((Customer c) => c.EnumValue)
                                            .HasParameter((Customer c) => c.Name)
                                            .HasParameter((Customer c) => c.SomeShort)
                                            .HasParameter((AbstractCustomer c) => c.AbstractString))
                                    .DeleteUsingStoredProcedure(
                                        s => s.HasOriginalValueParameter(b => b.Id, p => p.HasName("DeleteId")));
                            }
                            else
                            {
                                cb
                                    .InsertUsingStoredProcedure(
                                        s => s
                                            .HasParameter(b => b.Id, p => p.IsOutput().HasName("InsertId"))
                                            .HasParameter("SpecialtyAk"))
                                    .UpdateUsingStoredProcedure(
                                        s => s
                                            .HasOriginalValueParameter(b => b.Id, p => p.HasName("UpdateId"))
                                            .HasOriginalValueParameter("SpecialtyAk"))
                                    .DeleteUsingStoredProcedure(
                                        s => s.HasOriginalValueParameter(b => b.Id, p => p.HasName("DeleteId")));
                            }
                        }
                    }

                    if (mapping == Mapping.TPC)
                    {
                        cb.UseTpcMappingStrategy();
                    }
                    else if (mapping == Mapping.TPT
                             && !mapToTables
                             && !mapToViews)
                    {
                        cb.UseTptMappingStrategy();
                    }

                    // TODO: Don't map it on the base #19811
                    cb.Property<string>("SpecialtyAk");
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

                        if (mapToSprocs)
                        {
                            cb
                                .InsertUsingStoredProcedure(
                                    s => s
                                        .HasParameter(c => c.Id, p => p.HasName("InsertId"))
                                        .HasParameter(c => c.EnumValue)
                                        .HasParameter(c => c.Name)
                                        .HasParameter(c => c.SomeShort))
                                .UpdateUsingStoredProcedure(
                                    s => s
                                        .HasOriginalValueParameter(b => b.Id, p => p.HasName("UpdateId"))
                                        .HasParameter(c => c.EnumValue)
                                        .HasParameter(c => c.Name)
                                        .HasParameter(c => c.SomeShort))
                                .DeleteUsingStoredProcedure(
                                    s => s.HasOriginalValueParameter(b => b.Id, p => p.HasName("DeleteId")));

                            if (mapping == Mapping.TPC)
                            {
                                cb.InsertUsingStoredProcedure(s => s.HasParameter("SpecialtyAk"));
                            }
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

                    if (mapToSprocs)
                    {
                        if (mapping == Mapping.TPC)
                        {
                            cb
                                .InsertUsingStoredProcedure(
                                    s => s
                                        .HasParameter(b => b.Id, p => p.HasName("InsertId"))
                                        .HasParameter(c => c.Specialty)
                                        .HasParameter(c => c.RelatedCustomerSpecialty)
                                        .HasParameter("AnotherRelatedCustomerId")
                                        .HasParameter("SpecialtyAk")
                                        .HasParameter(c => c.EnumValue)
                                        .HasParameter(c => c.Name)
                                        .HasParameter(c => c.SomeShort)
                                        .HasParameter(c => c.AbstractString))
                                .UpdateUsingStoredProcedure(
                                    s => s
                                        .HasOriginalValueParameter(b => b.Id, p => p.HasName("UpdateId"))
                                        .HasParameter(c => c.Specialty)
                                        .HasParameter(c => c.RelatedCustomerSpecialty)
                                        .HasParameter("AnotherRelatedCustomerId")
                                        .HasParameter(c => c.EnumValue)
                                        .HasParameter(c => c.Name)
                                        .HasParameter(c => c.SomeShort)
                                        .HasParameter(c => c.AbstractString))
                                .DeleteUsingStoredProcedure(
                                    s => s.HasOriginalValueParameter(b => b.Id, p => p.HasName("DeleteId")));
                        }
                        else if (mapping == Mapping.TPT)
                        {
                            cb
                                .InsertUsingStoredProcedure(
                                    s => s
                                        .HasResultColumn(b => b.Id, p => p.HasName("InsertId"))
                                        .HasParameter(c => c.Specialty)
                                        .HasParameter(c => c.RelatedCustomerSpecialty)
                                        .HasParameter("AnotherRelatedCustomerId")
                                        .HasParameter(c => c.AbstractString))
                                .UpdateUsingStoredProcedure(
                                    s => s
                                        .HasOriginalValueParameter(b => b.Id, p => p.HasName("UpdateId"))
                                        .HasParameter(c => c.Specialty)
                                        .HasParameter(c => c.RelatedCustomerSpecialty)
                                        .HasParameter("AnotherRelatedCustomerId")
                                        .HasParameter(c => c.AbstractString))
                                .DeleteUsingStoredProcedure(
                                    s => s.HasOriginalValueParameter(b => b.Id, p => p.HasName("DeleteId")));
                        }
                    }

                    cb.Property(s => s.Specialty).IsRequired();

                    if (cb.Metadata.GetTableName() != null)
                    {
                        cb.ToTable(tb => tb.HasCheckConstraint("Specialty", "[Specialty] IN ('Specialist', 'Generalist')"));
                    }

                    cb.HasOne(c => c.RelatedCustomer).WithOne()
                        .HasForeignKey<SpecialCustomer>(c => c.RelatedCustomerSpecialty)
                        .HasPrincipalKey<SpecialCustomer>("SpecialtyAk"); // TODO: Use the derived one, #2611

                    cb.HasOne<SpecialCustomer>().WithOne()
                        .HasForeignKey<SpecialCustomer>("AnotherRelatedCustomerId");

                    if (mapping == Mapping.TPC)
                    {
                        cb.Ignore(c => c.Details);
                    }
                    else
                    {
                        cb.OwnsOne(c => c.Details).Property(d => d.Address).IsRequired();
                        cb.Navigation(c => c.Details).IsRequired();

                        if (mapping == Mapping.TPT)
                        {
                            if (mapToViews)
                            {
                                cb.OwnsOne(c => c.Details, cdb => cdb.ToView("SpecialCustomerView"));
                            }

                            if (mapToTables)
                            {
                                cb.OwnsOne(c => c.Details, cdb => cdb.ToTable("SpecialCustomer", "SpecialSchema"));
                            }
                        }

                        if (mapToSprocs)
                        {
                            cb.OwnsOne(
                                c => c.Details, cdb => cdb
                                    .InsertUsingStoredProcedure(
                                        "CustomerDetailsInsert", s => s
                                            .HasParameter("SpecialCustomerId")
                                            .HasParameter(b => b.BirthDay)
                                            .HasParameter(b => b.Address))
                                    .UpdateUsingStoredProcedure(
                                        "CustomerDetailsUpdate", s => s
                                            .HasOriginalValueParameter("SpecialCustomerId")
                                            .HasParameter(b => b.BirthDay)
                                            .HasParameter(b => b.Address))
                                    .DeleteUsingStoredProcedure(
                                        "CustomerDetailsDelete", s => s
                                            .HasOriginalValueParameter("SpecialCustomerId")));
                        }
                    }
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

                        if (mapToSprocs)
                        {
                            if (mapping == Mapping.TPC)
                            {
                                cb
                                    .InsertUsingStoredProcedure(
                                        s => s
                                            .HasParameter(b => b.Id, p => p.HasName("InsertId"))
                                            .HasParameter(c => c.Specialty)
                                            .HasParameter(c => c.RelatedCustomerSpecialty)
                                            .HasParameter("AnotherRelatedCustomerId")
                                            .HasParameter("SpecialtyAk")
                                            .HasParameter(c => c.EnumValue)
                                            .HasParameter(c => c.Name)
                                            .HasParameter(c => c.SomeShort)
                                            .HasParameter(c => c.AbstractString))
                                    .UpdateUsingStoredProcedure(
                                        s => s
                                            .HasOriginalValueParameter(b => b.Id, p => p.HasName("UpdateId"))
                                            .HasParameter(c => c.Specialty)
                                            .HasParameter(c => c.RelatedCustomerSpecialty)
                                            .HasParameter("AnotherRelatedCustomerId")
                                            .HasParameter(c => c.EnumValue)
                                            .HasParameter(c => c.Name)
                                            .HasParameter(c => c.SomeShort)
                                            .HasParameter(c => c.AbstractString))
                                    .DeleteUsingStoredProcedure(
                                        s => s.HasOriginalValueParameter(b => b.Id, p => p.HasName("DeleteId")));
                            }
                            else if (mapping == Mapping.TPT)
                            {
                                cb
                                    .InsertUsingStoredProcedure(s => s.HasParameter(b => b.Id))
                                    .UpdateUsingStoredProcedure(s => s.HasOriginalValueParameter(b => b.Id))
                                    .DeleteUsingStoredProcedure(s => s.HasOriginalValueParameter(b => b.Id));
                            }
                        }
                    }

                    if (mapping == Mapping.TPC)
                    {
                        cb.OwnsOne(c => c.Details).Property(d => d.Address).IsRequired();
                        cb.Navigation(c => c.Details).IsRequired();

                        if (mapToViews)
                        {
                            cb.OwnsOne(c => c.Details, cdb => cdb.ToView("ExtraSpecialCustomerView"));
                        }

                        if (mapToTables)
                        {
                            cb.OwnsOne(c => c.Details, cdb => cdb.ToTable("ExtraSpecialCustomer", "ExtraSpecialSchema"));
                        }

                        if (mapToSprocs)
                        {
                            cb.OwnsOne(
                                c => c.Details, cdb => cdb
                                    .InsertUsingStoredProcedure(
                                        "CustomerDetailsInsert", s => s
                                            .HasParameter("ExtraSpecialCustomerId")
                                            .HasParameter(b => b.BirthDay)
                                            .HasParameter(b => b.Address))
                                    .UpdateUsingStoredProcedure(
                                        "CustomerDetailsUpdate", s => s
                                            .HasOriginalValueParameter("ExtraSpecialCustomerId")
                                            .HasParameter(b => b.BirthDay)
                                            .HasParameter(b => b.Address))
                                    .DeleteUsingStoredProcedure(
                                        "CustomerDetailsDelete", s => s
                                            .HasOriginalValueParameter("ExtraSpecialCustomerId")));
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

                    ob.HasIndex(o => o.OrderDate).HasDatabaseName("IX_OrderDate");

                    if (mapToSprocs)
                    {
                        ob
                            .InsertUsingStoredProcedure(
                                s => s
                                    .HasResultColumn(c => c.Id)
                                    .HasParameter(c => c.AlternateId)
                                    .HasParameter(c => c.CustomerId)
                                    .HasParameter(c => c.OrderDate))
                            .UpdateUsingStoredProcedure(
                                s => s
                                    .HasOriginalValueParameter(c => c.Id)
                                    .HasParameter(c => c.CustomerId)
                                    .HasParameter(c => c.OrderDate))
                            .DeleteUsingStoredProcedure(s => s.HasOriginalValueParameter(b => b.Id));
                    }

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

                            if (mapToSprocs)
                            {
                                odb
                                    .InsertUsingStoredProcedure(
                                        "OrderDetails_Insert", s => s
                                            .HasParameter(c => c.OrderId)
                                            .HasParameter(c => c.AlternateId)
                                            .HasParameter(c => c.Active)
                                            .HasParameter(c => c.OrderDate))
                                    .UpdateUsingStoredProcedure(
                                        "OrderDetails_Update", s => s
                                            .HasOriginalValueParameter(c => c.OrderId)
                                            .HasParameter(c => c.Active)
                                            .HasParameter(c => c.OrderDate))
                                    .DeleteUsingStoredProcedure(
                                        "OrderDetails_Delete", s => s
                                            .HasOriginalValueParameter(b => b.OrderId));

                                odb.OwnsOne(
                                    od => od.BillingAddress, bab => bab
                                        .InsertUsingStoredProcedure(
                                            "BillingAddress_Insert", s => s
                                                .HasParameter(c => c.City)
                                                .HasParameter(c => c.Street)
                                                .HasParameter("OrderDetailsOrderId"))
                                        .UpdateUsingStoredProcedure(
                                            "BillingAddress_Update", s => s
                                                .HasParameter(c => c.City)
                                                .HasParameter(c => c.Street)
                                                .HasOriginalValueParameter("OrderDetailsOrderId"))
                                        .DeleteUsingStoredProcedure(
                                            "BillingAddress_Delete", s => s
                                                .HasOriginalValueParameter("OrderDetailsOrderId")));

                                odb.OwnsOne(
                                    od => od.ShippingAddress, sab => sab
                                        .InsertUsingStoredProcedure(
                                            "ShippingAddress_Insert", s => s
                                                .HasParameter("OrderDetailsOrderId")
                                                .HasParameter(c => c.City)
                                                .HasParameter(c => c.Street))
                                        .UpdateUsingStoredProcedure(
                                            "ShippingAddress_Update", s => s
                                                .HasOriginalValueParameter("OrderDetailsOrderId")
                                                .HasParameter(c => c.City)
                                                .HasParameter(c => c.Street))
                                        .DeleteUsingStoredProcedure(
                                            "ShippingAddress_Delete", s => s
                                                .HasOriginalValueParameter("OrderDetailsOrderId")));
                            }
                            else
                            {
                                odb.OwnsOne(od => od.BillingAddress);
                                odb.OwnsOne(od => od.ShippingAddress);
                            }

                            odb.Navigation(od => od.BillingAddress).IsRequired();
                            odb.Navigation(od => od.ShippingAddress).IsRequired();
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

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_use_relational_model_with_entity_splitting_and_table_splitting_on_both_fragments(bool mapToViews)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Ignore<AbstractCustomer>();
            modelBuilder.Ignore<Customer>();

            modelBuilder.Entity<SpecialCustomer>(
                cb =>
                {
                    cb.Ignore(c => c.Orders);
                    cb.Ignore(c => c.RelatedCustomer);

                    if (mapToViews)
                    {
                        cb.ToView(
                            "CustomerView", tb =>
                            {
                                tb.Property(c => c.AbstractString);
                            });

                        cb.SplitToView(
                            "CustomerDetailsView", tb =>
                            {
                                tb.Property(c => c.AbstractString);
                                tb.Property(c => c.Specialty);
                                tb.Property(c => c.RelatedCustomerSpecialty);
                            });
                    }
                    else
                    {
                        cb.ToTable(
                            "Customer", tb =>
                            {
                                tb.Property(c => c.AbstractString);
                            });

                        cb.SplitToTable(
                            "CustomerDetails", tb =>
                            {
                                tb.Property(c => c.AbstractString);
                                tb.Property(c => c.Specialty);
                                tb.Property(c => c.RelatedCustomerSpecialty);
                            });
                    }

                    cb.OwnsOne(
                        c => c.Details, db =>
                        {
                            if (mapToViews)
                            {
                                db.ToView("CustomerView");

                                db.SplitToView(
                                    "CustomerDetailsView", tb =>
                                    {
                                        tb.Property(d => d.BirthDay);
                                    });
                            }
                            else
                            {
                                db.SplitToTable(
                                    "CustomerDetails", tb =>
                                    {
                                        tb.Property(d => d.BirthDay);
                                    });
                            }

                            db.Property("SpecialCustomerId").HasColumnName("Id");
                        });
                    cb.Navigation(c => c.Details).IsRequired();
                });

            var model = Finalize(modelBuilder);
            var customerType = model.Model.FindEntityType(typeof(SpecialCustomer));

            var detailsNavigation = customerType.FindNavigation(nameof(SpecialCustomer.Details));
            var detailsType = detailsNavigation.TargetEntityType;

            Assert.Equal(2, model.Model.GetEntityTypes().Count());
            if (mapToViews)
            {
                Assert.Empty(model.Tables);
                Assert.Equal(2, model.Views.Count());

                var customerView = model.Views.Single(t => t.Name == "CustomerView");

                Assert.Equal(2, customerView.EntityTypeMappings.Count());

                var customerMapping = customerView.EntityTypeMappings.First();
                Assert.True(customerMapping.IsSharedTablePrincipal);
                Assert.True(customerMapping.IsSplitEntityTypePrincipal);
                var detailsMapping = customerView.EntityTypeMappings.Last();
                Assert.False(detailsMapping.IsSharedTablePrincipal);
                Assert.True(detailsMapping.IsSplitEntityTypePrincipal);

                var customerDetailsView = model.Views.Single(t => t.Name == "CustomerDetailsView");

                Assert.Equal(
                    new[] { customerView, customerDetailsView },
                    customerType.GetViewMappings().Select(m => m.View));

                Assert.Equal(2, customerDetailsView.EntityTypeMappings.Count());

                var customerSplitMapping = customerDetailsView.EntityTypeMappings.First();
                Assert.True(customerSplitMapping.IsSharedTablePrincipal);
                Assert.False(customerSplitMapping.IsSplitEntityTypePrincipal);
                var detailsSplitMapping = customerDetailsView.EntityTypeMappings.Last();
                Assert.False(detailsSplitMapping.IsSharedTablePrincipal);
                Assert.False(detailsSplitMapping.IsSplitEntityTypePrincipal);

                Assert.Equal(
                    new[] { customerView, customerDetailsView },
                    detailsType.GetViewMappings().Select(m => m.View));

                Assert.Equal(
                    new[] { "AbstractString", "Details_Address", "EnumValue", "Id", "Name", "SomeShort" },
                    customerView.Columns.Select(t => t.Name));

                Assert.Equal(
                    new[] { "AbstractString", "Details_BirthDay", "Id", "RelatedCustomerSpecialty", "Specialty" },
                    customerDetailsView.Columns.Select(t => t.Name));
            }
            else
            {
                Assert.Empty(model.Views);
                Assert.Equal(2, model.Tables.Count());

                var customerTable = model.Tables.Single(t => t.Name == "Customer");

                Assert.Equal(2, customerTable.EntityTypeMappings.Count());

                var customerMapping = customerTable.EntityTypeMappings.First();
                Assert.True(customerMapping.IsSharedTablePrincipal);
                Assert.True(customerMapping.IsSplitEntityTypePrincipal);
                var detailsMapping = customerTable.EntityTypeMappings.Last();
                Assert.False(detailsMapping.IsSharedTablePrincipal);
                Assert.True(detailsMapping.IsSplitEntityTypePrincipal);

                var customerDetailsTable = model.Tables.Single(t => t.Name == "CustomerDetails");

                Assert.Equal(
                    new[] { customerTable, customerDetailsTable },
                    customerType.GetTableMappings().Select(m => m.Table));

                Assert.Equal(2, customerDetailsTable.EntityTypeMappings.Count());

                var customerSplitMapping = customerDetailsTable.EntityTypeMappings.First();
                Assert.True(customerSplitMapping.IsSharedTablePrincipal);
                Assert.False(customerSplitMapping.IsSplitEntityTypePrincipal);
                var detailsSplitMapping = customerDetailsTable.EntityTypeMappings.Last();
                Assert.False(detailsSplitMapping.IsSharedTablePrincipal);
                Assert.False(detailsSplitMapping.IsSplitEntityTypePrincipal);

                Assert.Equal(
                    new[] { customerTable, customerDetailsTable },
                    detailsType.GetTableMappings().Select(m => m.Table));

                Assert.Single(customerTable.UniqueConstraints);
                Assert.Empty(customerTable.ForeignKeyConstraints);
                Assert.Empty(customerTable.Indexes);
                Assert.Empty(customerTable.GetRowInternalForeignKeys(customerType));
                Assert.Single(customerTable.GetRowInternalForeignKeys(detailsType));
                Assert.Equal(
                    new[] { "Id", "AbstractString", "Details_Address", "EnumValue", "Name", "SomeShort" },
                    customerTable.Columns.Select(t => t.Name));

                Assert.Single(customerDetailsTable.UniqueConstraints);
                var fkConstraint = customerDetailsTable.ForeignKeyConstraints.Single();
                Assert.Empty(customerDetailsTable.Indexes);
                Assert.Empty(customerDetailsTable.GetRowInternalForeignKeys(customerType));
                Assert.Single(customerDetailsTable.GetRowInternalForeignKeys(detailsType));
                Assert.Equal(
                    new[] { "Id", "AbstractString", "Details_BirthDay", "RelatedCustomerSpecialty", "Specialty" },
                    customerDetailsTable.Columns.Select(t => t.Name));

                Assert.Equal(2, fkConstraint.MappedForeignKeys.Count());
                Assert.All(
                    fkConstraint.MappedForeignKeys,
                    fk =>
                    {
                        Assert.True(fk.IsUnique);
                        Assert.True(fk.IsRequired);
                        Assert.True(fk.IsRequiredDependent);
                    });
            }
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_entity_splitting_and_table_splitting_on_main_fragments()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Ignore<AbstractCustomer>();
            modelBuilder.Ignore<Customer>();

            modelBuilder.Entity<SpecialCustomer>(
                cb =>
                {
                    cb.Ignore(c => c.Orders);
                    cb.Ignore(c => c.RelatedCustomer);

                    cb.ToTable(
                        "Customer", tb =>
                        {
                            tb.Property(c => c.AbstractString);
                        });

                    cb.SplitToTable(
                        "CustomerSpecialty", tb =>
                        {
                            tb.Property(c => c.AbstractString);
                            tb.Property(c => c.Specialty);
                            tb.Property(c => c.RelatedCustomerSpecialty);
                        });

                    cb.OwnsOne(
                        c => c.Details, db =>
                        {
                            db.SplitToTable(
                                "CustomerDetails", tb =>
                                {
                                    tb.Property(d => d.BirthDay);
                                });
                            db.Property("SpecialCustomerId").HasColumnName("Id");
                        });
                    cb.Navigation(c => c.Details).IsRequired();
                });

            var model = Finalize(modelBuilder);
            var customerType = model.Model.FindEntityType(typeof(SpecialCustomer));

            var detailsNavigation = customerType.FindNavigation(nameof(SpecialCustomer.Details));
            var detailsType = detailsNavigation.TargetEntityType;

            Assert.Equal(2, model.Model.GetEntityTypes().Count());
            Assert.Empty(model.Views);
            Assert.Equal(3, model.Tables.Count());

            var customerTable = model.Tables.Single(t => t.Name == "Customer");

            Assert.Equal(2, customerTable.EntityTypeMappings.Count());

            var customerMapping = customerTable.EntityTypeMappings.First();
            Assert.True(customerMapping.IsSharedTablePrincipal);
            Assert.True(customerMapping.IsSplitEntityTypePrincipal);
            var detailsMapping = customerTable.EntityTypeMappings.Last();
            Assert.False(detailsMapping.IsSharedTablePrincipal);
            Assert.True(detailsMapping.IsSplitEntityTypePrincipal);

            var customerDetailsTable = model.Tables.Single(t => t.Name == "CustomerDetails");

            Assert.Equal(
                new[] { customerTable, customerDetailsTable },
                detailsType.GetTableMappings().Select(m => m.Table));

            var detailsSplitMapping = customerDetailsTable.EntityTypeMappings.Single();
            Assert.Null(detailsSplitMapping.IsSharedTablePrincipal);
            Assert.False(detailsSplitMapping.IsSplitEntityTypePrincipal);

            var customerSpecialtyTable = model.Tables.Single(t => t.Name == "CustomerSpecialty");

            Assert.Equal(
                new[] { customerTable, customerSpecialtyTable },
                customerType.GetTableMappings().Select(m => m.Table));

            var customerSplitMapping = customerSpecialtyTable.EntityTypeMappings.Single();
            Assert.Null(customerSplitMapping.IsSharedTablePrincipal);
            Assert.False(customerSplitMapping.IsSplitEntityTypePrincipal);

            Assert.Single(customerTable.UniqueConstraints);
            Assert.Empty(customerTable.ForeignKeyConstraints);
            Assert.Empty(customerTable.Indexes);
            Assert.Empty(customerTable.GetRowInternalForeignKeys(customerType));
            Assert.Single(customerTable.GetRowInternalForeignKeys(detailsType));
            Assert.Equal(
                new[] { "Id", "AbstractString", "Details_Address", "EnumValue", "Name", "SomeShort" },
                customerTable.Columns.Select(t => t.Name));

            Assert.Single(customerDetailsTable.UniqueConstraints);
            var detailsFkConstraint = customerDetailsTable.ForeignKeyConstraints.Single();
            Assert.Empty(customerDetailsTable.Indexes);
            Assert.Empty(customerDetailsTable.GetRowInternalForeignKeys(detailsType));
            Assert.Equal(
                new[] { "Id", "BirthDay" },
                customerDetailsTable.Columns.Select(t => t.Name));

            var detailsFk = detailsFkConstraint.MappedForeignKeys.Single();

            Assert.True(detailsFk.IsUnique);
            Assert.True(detailsFk.IsRequired);
            Assert.True(detailsFk.IsRequiredDependent);

            Assert.Single(customerSpecialtyTable.UniqueConstraints);
            var specialtyFkConstraint = customerSpecialtyTable.ForeignKeyConstraints.Single();
            Assert.Empty(customerSpecialtyTable.Indexes);
            Assert.Empty(customerSpecialtyTable.GetRowInternalForeignKeys(customerType));
            Assert.Equal(
                new[] { "Id", "AbstractString", "RelatedCustomerSpecialty", "Specialty" },
                customerSpecialtyTable.Columns.Select(t => t.Name));

            var specialtyFk = specialtyFkConstraint.MappedForeignKeys.Single();

            Assert.True(specialtyFk.IsUnique);
            Assert.True(specialtyFk.IsRequired);
            Assert.True(specialtyFk.IsRequiredDependent);
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_entity_splitting_and_table_splitting_on_leaf_and_main_fragments()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Ignore<AbstractCustomer>();
            modelBuilder.Ignore<Customer>();

            modelBuilder.Entity<SpecialCustomer>(
                cb =>
                {
                    cb.Ignore(c => c.Orders);
                    cb.Ignore(c => c.RelatedCustomer);

                    cb.ToTable(
                        "Customer", tb =>
                        {
                            tb.Property(c => c.AbstractString);
                        });

                    cb.SplitToTable(
                        "CustomerDetails", tb =>
                        {
                            tb.Property(c => c.AbstractString);
                            tb.Property(c => c.Specialty);
                            tb.Property(c => c.RelatedCustomerSpecialty);
                        });

                    cb.OwnsOne(
                        c => c.Details, db =>
                        {
                            db.ToTable("CustomerDetails");

                            db.SplitToTable(
                                "Details", tb =>
                                {
                                    tb.Property(d => d.BirthDay);
                                });
                            db.Property("SpecialCustomerId").HasColumnName("Id");
                        });
                    cb.Navigation(c => c.Details).IsRequired();
                });

            var model = Finalize(modelBuilder);
            var customerType = model.Model.FindEntityType(typeof(SpecialCustomer));

            var detailsNavigation = customerType.FindNavigation(nameof(SpecialCustomer.Details));
            var detailsType = detailsNavigation.TargetEntityType;

            Assert.Equal(2, model.Model.GetEntityTypes().Count());
            Assert.Empty(model.Views);
            Assert.Equal(3, model.Tables.Count());

            var customerTable = model.Tables.Single(t => t.Name == "Customer");

            var customerMapping = customerTable.EntityTypeMappings.Single();
            Assert.Null(customerMapping.IsSharedTablePrincipal);
            Assert.True(customerMapping.IsSplitEntityTypePrincipal);

            var customerDetailsTable = model.Tables.Single(t => t.Name == "CustomerDetails");

            Assert.Equal(
                new[] { customerTable, customerDetailsTable },
                customerType.GetTableMappings().Select(m => m.Table));

            Assert.Equal(2, customerDetailsTable.EntityTypeMappings.Count());

            var customerSplitMapping = customerDetailsTable.EntityTypeMappings.First();
            Assert.True(customerSplitMapping.IsSharedTablePrincipal);
            Assert.False(customerSplitMapping.IsSplitEntityTypePrincipal);
            var detailsMapping = customerDetailsTable.EntityTypeMappings.Last();
            Assert.False(detailsMapping.IsSharedTablePrincipal);
            Assert.True(detailsMapping.IsSplitEntityTypePrincipal);

            var detailsTable = model.Tables.Single(t => t.Name == "Details");

            Assert.Equal(
                new[] { customerDetailsTable, detailsTable },
                detailsType.GetTableMappings().Select(m => m.Table));

            var detailsSplitMapping = detailsTable.EntityTypeMappings.Single();
            Assert.Null(detailsSplitMapping.IsSharedTablePrincipal);
            Assert.False(detailsSplitMapping.IsSplitEntityTypePrincipal);

            Assert.Single(customerTable.UniqueConstraints);
            Assert.Empty(customerTable.ForeignKeyConstraints);
            Assert.Empty(customerTable.Indexes);
            Assert.Empty(customerTable.GetRowInternalForeignKeys(customerType));
            Assert.Equal(
                new[] { "Id", "AbstractString", "EnumValue", "Name", "SomeShort" },
                customerTable.Columns.Select(t => t.Name));

            Assert.Single(customerDetailsTable.UniqueConstraints);
            var customerDetailsFkConstraint = customerDetailsTable.ForeignKeyConstraints.Single();
            Assert.Empty(customerDetailsTable.Indexes);
            Assert.Empty(customerDetailsTable.GetRowInternalForeignKeys(customerType));
            Assert.Single(customerDetailsTable.GetRowInternalForeignKeys(detailsType));
            Assert.Equal(
                new[] { "Id", "AbstractString", "Details_Address", "RelatedCustomerSpecialty", "Specialty" },
                customerDetailsTable.Columns.Select(t => t.Name));

            Assert.Equal(2, customerDetailsFkConstraint.MappedForeignKeys.Count());

            var customerFk = customerDetailsFkConstraint.MappedForeignKeys.First();

            Assert.True(customerFk.IsUnique);
            Assert.True(customerFk.IsRequired);
            Assert.True(customerFk.IsRequiredDependent);
            Assert.Same(customerType, customerFk.DeclaringEntityType);

            var customerDetailsFk = customerDetailsFkConstraint.MappedForeignKeys.Last();

            Assert.True(customerDetailsFk.IsUnique);
            Assert.True(customerDetailsFk.IsRequired);
            Assert.True(customerDetailsFk.IsRequiredDependent);
            Assert.Same(detailsType, customerDetailsFk.DeclaringEntityType);

            Assert.Single(detailsTable.UniqueConstraints);
            var detailsFkConstraint = detailsTable.ForeignKeyConstraints.Single();
            Assert.Empty(detailsTable.Indexes);
            Assert.Empty(detailsTable.GetRowInternalForeignKeys(detailsType));
            Assert.Equal(
                new[] { "Id", "BirthDay" },
                detailsTable.Columns.Select(t => t.Name));

            var detailsFk = detailsFkConstraint.MappedForeignKeys.Last();

            Assert.True(detailsFk.IsUnique);
            Assert.True(detailsFk.IsRequired);
            Assert.True(detailsFk.IsRequiredDependent);
            Assert.Same(detailsType, detailsFk.DeclaringEntityType);
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
                    cb.Property(s => s.Specialty).IsRequired();
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
            Assert.Null(specialCustomerTypeMapping.IsSplitEntityTypePrincipal);

            var specialCustomerView = specialCustomerTypeMapping.View;
            Assert.Same(customerView, specialCustomerView);

            Assert.Equal(2, specialCustomerView.EntityTypeMappings.Count());
            Assert.True(specialCustomerView.EntityTypeMappings.First().IsSharedTablePrincipal);
            Assert.False(specialCustomerView.EntityTypeMappings.Last().IsSharedTablePrincipal);

            var specialtyColumn = specialCustomerView.Columns.Single(c => c.Name == nameof(SpecialCustomer.Specialty));
            Assert.True(specialtyColumn.IsNullable);
        }

        [ConditionalFact]
        public void Can_use_relational_model_with_tables_in_different_schemas()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Ignore<Order>();
            modelBuilder.Entity<OrderDetails>(
                cb =>
                {
                    cb.HasKey(b => b.OrderId);
                    cb.OwnsOne(d => d.BillingAddress, a => a.ToTable("Details", "Billing"));
                    cb.OwnsOne(d => d.ShippingAddress, a => a.ToTable("Details", "Shipping"));
                    cb.OwnsOne(d => d.DateDetails, a => a.ToTable("Details", "Date"));
                });

            var model = Finalize(modelBuilder);

            Assert.Equal(4, model.Model.GetEntityTypes().Count());
            Assert.Empty(model.Views);

            var orderDetails = model.Model.FindEntityType(typeof(OrderDetails));
            var orderDetailsTable = orderDetails.GetTableMappings().Single().Table;
            Assert.Equal(3, orderDetailsTable.ReferencingForeignKeyConstraints.Count());
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
            Assert.Null(orderMapping.IsSharedTablePrincipal);
            Assert.Null(orderMapping.IsSplitEntityTypePrincipal);

            Assert.Null(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.Id), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersQuery = orderMapping.SqlQuery;
            Assert.Equal(
                new[] { orderType },
                ordersQuery.EntityTypeMappings.Select(m => m.TypeBase));
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
            Assert.Null(orderMapping.IsSharedTablePrincipal);
            Assert.Null(orderMapping.IsSplitEntityTypePrincipal);
            Assert.True(orderMapping.IsDefaultFunctionMapping);

            var tvfMapping = orderType.GetFunctionMappings().Last();
            Assert.Null(tvfMapping.IsSharedTablePrincipal);
            Assert.Null(tvfMapping.IsSplitEntityTypePrincipal);
            Assert.False(tvfMapping.IsDefaultFunctionMapping);

            Assert.Null(orderMapping.IncludesDerivedTypes);
            Assert.Equal(
                new[] { nameof(Order.AlternateId), nameof(Order.CustomerId), nameof(Order.Id), nameof(Order.OrderDate) },
                orderMapping.ColumnMappings.Select(m => m.Property.Name));

            var ordersFunction = orderMapping.StoreFunction;
            Assert.Same(ordersFunction, model.FindFunction(ordersFunction.Name, ordersFunction.Schema, []));
            Assert.Equal(
                new[] { orderType },
                ordersFunction.EntityTypeMappings.Select(m => m.TypeBase));
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
            => FakeRelationalTestHelpers.Instance.CreateConventionBuilder(
                configureContext: b =>
                    b.ConfigureWarnings(
                        w => w.Default(WarningBehavior.Throw)
                            .Ignore(RelationalEventId.ForeignKeyTpcPrincipalWarning)
                            .Ignore(RelationalEventId.AllIndexPropertiesNotToMappedToAnyTable)));

        public static void AssertEqual(IRelationalModel expectedModel, IRelationalModel actualModel)
            => RelationalModelAsserter.Instance.AssertEqual(expectedModel, actualModel);

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
            public string Specialty { get; set; }
            public string RelatedCustomerSpecialty { get; set; }
            public SpecialCustomer RelatedCustomer { get; set; }
            public CustomerDetails Details { get; set; }
        }

        private class CustomerDetails
        {
            public string Address { get; set; }
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DateTime BirthDay { get; set; }
        }

        private class ExtraSpecialCustomer : SpecialCustomer;

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
