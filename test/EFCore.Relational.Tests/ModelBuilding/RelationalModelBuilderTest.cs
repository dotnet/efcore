// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Data;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class RelationalModelBuilderTest : ModelBuilderTest
{
    public abstract class RelationalNonRelationshipTestBase : NonRelationshipTestBase
    {
        [ConditionalFact]
        public virtual void Can_use_table_splitting()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Order>().SplitToTable(
                "OrderDetails", s =>
                {
                    s.ExcludeFromMigrations();
                    var propertyBuilder = s.Property(o => o.CustomerId);
                    var columnBuilder = propertyBuilder.HasColumnName("id");
                    if (columnBuilder is IInfrastructure<ColumnBuilder<int?>> genericBuilder)
                    {
                        Assert.IsType<PropertyBuilder<int?>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<int?>>());
                        Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                    }
                    else
                    {
                        var nonGenericBuilder = (IInfrastructure<ColumnBuilder>)columnBuilder;
                        Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                        Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                    }
                });
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.False(entity.IsTableExcludedFromMigrations());
            Assert.False(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.True(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("OrderDetails", "dbo")));
            Assert.Same(
                entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.Table("OrderDetails", "dbo")));

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.Table("OrderDetails", "dbo")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.Table("OrderDetails", "dbo")));
        }

        [ConditionalFact]
        public virtual void Can_use_table_splitting_with_schema()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToTable("Order", "dbo")
                .SplitToTable(
                    "OrderDetails", "sch", s =>
                        s.ExcludeFromMigrations()
                            .Property(o => o.CustomerId).HasColumnName("id"));
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.False(entity.IsTableExcludedFromMigrations());
            Assert.False(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.True(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Same(
                entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(Order), "Order"),
                Assert.Throws<InvalidOperationException>(() => entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order")))
                    .Message);

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order")));
        }

        [ConditionalFact]
        public virtual void Can_use_view_splitting()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToView("Order")
                .SplitToView(
                    "OrderDetails", s =>
                    {
                        var propertyBuilder = s.Property(o => o.CustomerId);
                        var columnBuilder = propertyBuilder.HasColumnName("id");
                        if (columnBuilder is IInfrastructure<ViewColumnBuilder<int?>> genericBuilder)
                        {
                            Assert.IsType<PropertyBuilder<int?>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<int?>>());
                            Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                        }
                        else
                        {
                            var nonGenericBuilder = (IInfrastructure<ViewColumnBuilder>)columnBuilder;
                            Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                            Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                        }
                    });
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.Same(entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.View("OrderDetails")));

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.View("OrderDetails")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.View("OrderDetails")));
        }

        [ConditionalFact]
        public virtual void Can_use_view_splitting_with_schema()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToView("Order", "dbo")
                .SplitToView(
                    "OrderDetails", "sch", s =>
                        s.Property(o => o.CustomerId).HasColumnName("id"));
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.Same(
                entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(Order), "Order"),
                Assert.Throws<InvalidOperationException>(() => entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.View("Order")))
                    .Message);

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order")));
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_return_and_parameter_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureRowsAffectedReturnConflictingParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedParameter()
                                    .HasRowsAffectedReturnValue()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_return_and_result_column_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureRowsAffectedReturnConflictingParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedResultColumn()
                                    .HasRowsAffectedReturnValue()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_parameter_and_return_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedReturnValue()
                                    .HasRowsAffectedParameter()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_result_column_and_return_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedResultColumn("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedReturnValue()
                                    .HasRowsAffectedResultColumn()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_result_column_and_parameter_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedResultColumn("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedParameter()
                                    .HasRowsAffectedResultColumn()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_rows_affected_result_column_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .UpdateUsingStoredProcedure(
                    s => s.HasRowsAffectedResultColumn()).Metadata.GetUpdateStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedResultColumn("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddRowsAffectedResultColumn())
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_parameter_and_result_column_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedResultColumn()
                                    .HasRowsAffectedParameter()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_rows_affected_parameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .UpdateUsingStoredProcedure(
                    s => s.HasRowsAffectedParameter()).Metadata.GetUpdateStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddRowsAffectedParameter())
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_parameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasParameter(b => b.Id)).Metadata.GetInsertStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateParameter("Id", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddParameter("Id"))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_original_value_parameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasOriginalValueParameter(b => b.Id)).Metadata.GetInsertStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateOriginalValueParameter("Id", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddOriginalValueParameter("Id"))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_result_column_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasResultColumn(b => b.Id)).Metadata.GetInsertStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateResultColumn("Id", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddResultColumn("Id"))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Configuring_direction_on_RowsAffectedParameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var param = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasRowsAffectedParameter()).Metadata.GetInsertStoredProcedure()!.Parameters.Single();

            Assert.Equal(
                RelationalStrings.StoredProcedureParameterInvalidConfiguration("Direction", "RowsAffected", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => param.Direction = ParameterDirection.Input)
                    .Message);
        }
    }

    public abstract class RelationalComplexTypeTestBase : ComplexTypeTestBase
    {
        [ConditionalFact]
        public virtual void Can_use_table_splitting()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Order>().SplitToTable(
                "OrderDetails", s =>
                {
                    s.ExcludeFromMigrations();
                    var propertyBuilder = s.Property(o => o.CustomerId);
                    var columnBuilder = propertyBuilder.HasColumnName("id");
                    if (columnBuilder is IInfrastructure<ColumnBuilder<int?>> genericBuilder)
                    {
                        Assert.IsType<PropertyBuilder<int?>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<int?>>());
                        Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                    }
                    else
                    {
                        var nonGenericBuilder = (IInfrastructure<ColumnBuilder>)columnBuilder;
                        Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                        Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                    }
                });
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.False(entity.IsTableExcludedFromMigrations());
            Assert.False(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.True(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("OrderDetails", "dbo")));
            Assert.Same(
                entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.Table("OrderDetails", "dbo")));

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.Table("OrderDetails", "dbo")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.Table("OrderDetails", "dbo")));
        }

        [ConditionalFact]
        public virtual void Can_use_table_splitting_with_schema()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToTable("Order", "dbo")
                .SplitToTable(
                    "OrderDetails", "sch", s =>
                        s.ExcludeFromMigrations()
                            .Property(o => o.CustomerId).HasColumnName("id"));
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.False(entity.IsTableExcludedFromMigrations());
            Assert.False(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.True(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Same(
                entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(Order), "Order"),
                Assert.Throws<InvalidOperationException>(() => entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order")))
                    .Message);

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order")));
        }

        [ConditionalFact]
        public virtual void Can_use_view_splitting()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToView("Order")
                .SplitToView(
                    "OrderDetails", s =>
                    {
                        var propertyBuilder = s.Property(o => o.CustomerId);
                        var columnBuilder = propertyBuilder.HasColumnName("id");
                        if (columnBuilder is IInfrastructure<ViewColumnBuilder<int?>> genericBuilder)
                        {
                            Assert.IsType<PropertyBuilder<int?>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<int?>>());
                            Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                        }
                        else
                        {
                            var nonGenericBuilder = (IInfrastructure<ViewColumnBuilder>)columnBuilder;
                            Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                            Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                        }
                    });
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.Same(entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.View("OrderDetails")));

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.View("OrderDetails")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.View("OrderDetails")));
        }

        [ConditionalFact]
        public virtual void Can_use_view_splitting_with_schema()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToView("Order", "dbo")
                .SplitToView(
                    "OrderDetails", "sch", s =>
                        s.Property(o => o.CustomerId).HasColumnName("id"));
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.Same(
                entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Equal(
                RelationalStrings.TableNotMappedEntityType(nameof(Order), "Order"),
                Assert.Throws<InvalidOperationException>(() => entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.View("Order")))
                    .Message);

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order")));
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_return_and_parameter_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureRowsAffectedReturnConflictingParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedParameter()
                                    .HasRowsAffectedReturnValue()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_return_and_result_column_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureRowsAffectedReturnConflictingParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedResultColumn()
                                    .HasRowsAffectedReturnValue()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_rows_affected_result_column_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .UpdateUsingStoredProcedure(
                    s => s.HasRowsAffectedResultColumn()).Metadata.GetUpdateStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedResultColumn("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddRowsAffectedResultColumn())
                    .Message);
        }

        [ConditionalFact]
        public virtual void Conflicting_sproc_rows_affected_parameter_and_result_column_throw()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<BookLabel>()
                            .UpdateUsingStoredProcedure(
                                s => s.HasRowsAffectedResultColumn()
                                    .HasRowsAffectedParameter()))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_rows_affected_parameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .UpdateUsingStoredProcedure(
                    s => s.HasRowsAffectedParameter()).Metadata.GetUpdateStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedParameter("BookLabel_Update"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddRowsAffectedParameter())
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_parameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasParameter(b => b.Id)).Metadata.GetInsertStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateParameter("Id", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddParameter("Id"))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_original_value_parameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasOriginalValueParameter(b => b.Id)).Metadata.GetInsertStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateOriginalValueParameter("Id", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddOriginalValueParameter("Id"))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Duplicate_sproc_result_column_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var sproc = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasResultColumn(b => b.Id)).Metadata.GetInsertStoredProcedure()!;

            Assert.Equal(
                RelationalStrings.StoredProcedureDuplicateResultColumn("Id", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => sproc.AddResultColumn("Id"))
                    .Message);
        }

        [ConditionalFact]
        public virtual void Configuring_direction_on_RowsAffectedParameter_throws()
        {
            var modelBuilder = CreateModelBuilder();

            var param = modelBuilder.Entity<BookLabel>()
                .InsertUsingStoredProcedure(
                    s => s.HasRowsAffectedParameter()).Metadata.GetInsertStoredProcedure()!.Parameters.Single();

            Assert.Equal(
                RelationalStrings.StoredProcedureParameterInvalidConfiguration("Direction", "RowsAffected", "BookLabel_Insert"),
                Assert.Throws<InvalidOperationException>(() => param.Direction = ParameterDirection.Input)
                    .Message);
        }
    }

    public abstract class RelationalInheritanceTestBase : InheritanceTestBase
    {
        [ConditionalFact]
        public virtual void Can_use_table_splitting()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().SplitToTable(
                "OrderDetails", s =>
                {
                    s.ExcludeFromMigrations();
                    var propertyBuilder = s.Property(o => o.CustomerId);
                    var columnBuilder = propertyBuilder.HasColumnName("id");
                    if (columnBuilder is IInfrastructure<ColumnBuilder<int?>> genericBuilder)
                    {
                        Assert.IsType<PropertyBuilder<int?>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<int?>>());
                        Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                    }
                    else
                    {
                        var nonGenericBuilder = (IInfrastructure<ColumnBuilder>)columnBuilder;
                        Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                        Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                    }
                });
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.False(entity.IsTableExcludedFromMigrations());
            Assert.False(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order")));
            Assert.True(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("OrderDetails")));
            Assert.Same(entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.Table("OrderDetails")));

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.Table("Order")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.Table("OrderDetails")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.Table("OrderDetails")));
        }
    }

    public abstract class RelationalOneToManyTestBase : OneToManyTestBase
    {
    }

    public abstract class RelationalManyToOneTestBase : ManyToOneTestBase
    {
    }

    public abstract class RelationalOneToOneTestBase : OneToOneTestBase
    {
    }

    public abstract class RelationalManyToManyTestBase : ManyToManyTestBase
    {
    }

    public abstract class RelationalOwnedTypesTestBase : OwnedTypesTestBase
    {
        [ConditionalFact]
        public virtual void Can_use_table_splitting_with_owned_reference()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<BookDetails>();

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, lb =>
                {
                    lb.Ignore(l => l.Book);
                    lb.Property<string>("ShadowProp");

                    lb.SplitToTable(
                        "BookLabelDetails", s =>
                        {
                            var propertyBuilder = s.Property(o => o.Id);
                            var columnBuilder = propertyBuilder.HasColumnName("bid");
                            if (columnBuilder is IInfrastructure<ColumnBuilder<int>> genericBuilder)
                            {
                                Assert.IsType<PropertyBuilder<int>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<int>>());
                                Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                            }
                            else
                            {
                                var nonGenericBuilder = (IInfrastructure<ColumnBuilder>)columnBuilder;
                                Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                                Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                            }
                        });
                });
            modelBuilder.Entity<Book>()
                .OwnsOne(b => b.AlternateLabel);

            var model = modelBuilder.Model;

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(3, model.GetEntityTypes().Count());

            var book = model.FindEntityType(typeof(Book))!;
            var bookOwnership1 = book.FindNavigation(nameof(Book.Label))!.ForeignKey;
            var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel))!.ForeignKey;
            Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);

            var splitTable = StoreObjectIdentifier.Table("BookLabelDetails");
            var fragment = bookOwnership1.DeclaringEntityType.GetMappingFragments().Single();
            Assert.Same(fragment, bookOwnership1.DeclaringEntityType.FindMappingFragment(splitTable));
            Assert.Same(fragment, bookOwnership1.DeclaringEntityType.GetMappingFragments(StoreObjectType.Table).Single());

            Assert.True(((IConventionEntityTypeMappingFragment)fragment).IsInModel);
            Assert.Same(bookOwnership1.DeclaringEntityType, fragment.EntityType);

            var bookId = bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))!;
            Assert.Equal("Id", bookId.GetColumnName());
            Assert.Null(bookId.GetColumnName(StoreObjectIdentifier.Table("Book")));
            Assert.Equal("bid", bookId.GetColumnName(splitTable));

            var overrides = bookId.GetOverrides().Single();
            Assert.Same(overrides, bookId.FindOverrides(splitTable));
            Assert.True(((IConventionRelationalPropertyOverrides)overrides).IsInModel);
            Assert.Same(bookId, overrides.Property);

            var readOnlyModel = modelBuilder.FinalizeModel();

            Assert.Equal(2, readOnlyModel.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(3, readOnlyModel.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Can_use_view_splitting_with_owned_collection()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.Ignore(o => o.OrderCombination);
                    r.Ignore(o => o.Details);
                    r.Property<string>("ShadowProp");

                    r.ToView("Order");
                    r.SplitToView(
                        "OrderDetails", s =>
                        {
                            var propertyBuilder = s.Property(o => o.AnotherCustomerId);
                            var columnBuilder = propertyBuilder.HasColumnName("cid");
                            if (columnBuilder is IInfrastructure<ViewColumnBuilder<Guid>> genericBuilder)
                            {
                                Assert.IsType<PropertyBuilder<Guid>>(genericBuilder.Instance.GetInfrastructure<PropertyBuilder<Guid>>());
                                Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(genericBuilder.GetInfrastructure().Overrides);
                            }
                            else
                            {
                                var nonGenericBuilder = (IInfrastructure<ViewColumnBuilder>)columnBuilder;
                                Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                                Assert.IsAssignableFrom<IMutableRelationalPropertyOverrides>(nonGenericBuilder.Instance.Overrides);
                            }
                        });
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer))!.FindNavigation(nameof(Customer.Orders))!.ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);

            var splitView = StoreObjectIdentifier.View("OrderDetails");
            var fragment = owned.GetMappingFragments().Single();
            Assert.Same(fragment, owned.FindMappingFragment(splitView));
            Assert.Same(fragment, owned.GetMappingFragments(StoreObjectType.View).Single());

            Assert.True(((IConventionEntityTypeMappingFragment)fragment).IsInModel);
            Assert.Same(owned, fragment.EntityType);

            var anotherCustomerId = owned.FindProperty(nameof(Order.AnotherCustomerId))!;
            Assert.Equal("AnotherCustomerId", anotherCustomerId.GetColumnName());
            Assert.Null(anotherCustomerId.GetColumnName(StoreObjectIdentifier.View("Order")));
            Assert.Equal("cid", anotherCustomerId.GetColumnName(splitView));

            var overrides = anotherCustomerId.GetOverrides().Single();
            Assert.Same(overrides, anotherCustomerId.FindOverrides(splitView));
            Assert.True(((IConventionRelationalPropertyOverrides)overrides).IsInModel);
            Assert.Same(anotherCustomerId, overrides.Property);
        }

        [ConditionalFact]
        public virtual void Can_use_sproc_mapping_with_owned_reference()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<BookDetails>();

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, lb =>
                {
                    lb.Ignore(l => l.Book);
                    lb.Ignore(s => s.SpecialBookLabel);

                    lb.Property(l => l.Id).ValueGeneratedOnUpdate();

                    lb.InsertUsingStoredProcedure(
                            s => s
                                .HasAnnotation("foo", "bar1")
                                .HasParameter(b => b.Id)
                                .HasParameter(b => b.BookId, p => p.HasName("InsertId")))
                        .UpdateUsingStoredProcedure(
                            s => s
                                .HasAnnotation("foo", "bar2")
                                .HasOriginalValueParameter(b => b.Id)
                                .HasOriginalValueParameter(
                                    b => b.BookId, p =>
                                    {
                                        var parameterBuilder = p.HasName("UpdateId");
                                        var nonGenericBuilder = (IInfrastructure<StoredProcedureParameterBuilder>)parameterBuilder;
                                        Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                                    })
                                .HasResultColumn(
                                    b => b.Id, p =>
                                    {
                                        var nonGenericBuilder = (IInfrastructure<StoredProcedureResultColumnBuilder>)p;
                                        Assert.IsAssignableFrom<PropertyBuilder>(nonGenericBuilder.Instance.GetInfrastructure());
                                    }))
                        .DeleteUsingStoredProcedure(
                            s => s
                                .HasAnnotation("foo", "bar3")
                                .HasOriginalValueParameter(b => b.BookId, p => p.HasName("DeleteId")));
                });
            modelBuilder.Entity<Book>()
                .OwnsOne(b => b.AlternateLabel);

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, lb =>
                {
                    lb.InsertUsingStoredProcedure("Insert", s => { });
                    lb.UpdateUsingStoredProcedure("Update", "dbo", s => { });
                    lb.DeleteUsingStoredProcedure("BookLabel_Delete", s => { });
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(3, model.GetEntityTypes().Count());

            var book = model.FindEntityType(typeof(Book))!;
            var bookOwnership1 = book.FindNavigation(nameof(Book.Label))!.ForeignKey;
            var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel))!.ForeignKey;
            Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);

            var insertSproc = bookOwnership1.DeclaringEntityType.GetInsertStoredProcedure()!;
            Assert.Equal("Insert", insertSproc.Name);
            Assert.Null(insertSproc.Schema);
            Assert.Equal(new[] { "Id", "BookId" }, insertSproc.Parameters.Select(p => p.PropertyName));
            Assert.Empty(insertSproc.ResultColumns);
            Assert.Equal("bar1", insertSproc["foo"]);
            Assert.Same(bookOwnership1.DeclaringEntityType, insertSproc.EntityType);

            var updateSproc = bookOwnership1.DeclaringEntityType.GetUpdateStoredProcedure()!;
            Assert.Equal("Update", updateSproc.Name);
            Assert.Equal("dbo", updateSproc.Schema);
            Assert.Equal(new[] { "Id", "BookId" }, updateSproc.Parameters.Select(p => p.PropertyName));
            Assert.Equal(new[] { "Id" }, updateSproc.ResultColumns.Select(p => p.Name));
            Assert.Equal("bar2", updateSproc["foo"]);
            Assert.Same(bookOwnership1.DeclaringEntityType, updateSproc.EntityType);

            var deleteSproc = bookOwnership1.DeclaringEntityType.GetDeleteStoredProcedure()!;
            Assert.Equal("BookLabel_Delete", deleteSproc.Name);
            Assert.Null(deleteSproc.Schema);
            Assert.Equal(new[] { "BookId" }, deleteSproc.Parameters.Select(p => p.PropertyName));
            Assert.Empty(deleteSproc.ResultColumns);
            Assert.Equal("bar3", deleteSproc["foo"]);
            Assert.Same(bookOwnership1.DeclaringEntityType, deleteSproc.EntityType);

            var bookId = bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.BookId))!;
            Assert.Empty(bookId.GetOverrides());
            Assert.Equal(
                "BookId",
                bookId.GetColumnName(
                    StoreObjectIdentifier.Create(bookOwnership1.DeclaringEntityType, StoreObjectType.InsertStoredProcedure)!.Value));
            Assert.Equal(
                "BookId",
                bookId.GetColumnName(
                    StoreObjectIdentifier.Create(bookOwnership1.DeclaringEntityType, StoreObjectType.UpdateStoredProcedure)!.Value));
            Assert.Equal(
                "BookId",
                bookId.GetColumnName(
                    StoreObjectIdentifier.Create(bookOwnership1.DeclaringEntityType, StoreObjectType.DeleteStoredProcedure)!.Value));

            Assert.Null(bookOwnership2.DeclaringEntityType.GetInsertStoredProcedure());
            Assert.Null(bookOwnership2.DeclaringEntityType.GetUpdateStoredProcedure());
            Assert.Null(bookOwnership2.DeclaringEntityType.GetDeleteStoredProcedure());
        }
#nullable disable
        protected class JsonEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public OwnedEntity OwnedReference1 { get; set; }
            public OwnedEntity OwnedReference2 { get; set; }

            public List<OwnedEntity> OwnedCollection1 { get; set; }
            public List<OwnedEntity> OwnedCollection2 { get; set; }
        }

        protected class OwnedEntity
        {
            public DateTime Date { get; set; }
            public double Fraction { get; set; }
            public MyJsonEnum Enum { get; set; }
        }

        protected enum MyJsonEnum
        {
            One,
            Two,
            Three,
        }

        protected class JsonEntityInheritanceBase
        {
            public int Id { get; set; }
            public OwnedEntity OwnedReferenceOnBase { get; set; }
            public List<OwnedEntity> OwnedCollectionOnBase { get; set; }
        }

        protected class JsonEntityInheritanceDerived : JsonEntityInheritanceBase
        {
            public string Name { get; set; }
            public OwnedEntity OwnedReferenceOnDerived { get; set; }
            public List<OwnedEntity> OwnedCollectionOnDerived { get; set; }
        }

        protected class OwnedEntityExtraLevel
        {
            public DateTime Date { get; set; }
            public double Fraction { get; set; }
            public MyJsonEnum Enum { get; set; }

            public OwnedEntity Reference1 { get; set; }
            public OwnedEntity Reference2 { get; set; }
            public List<OwnedEntity> Collection1 { get; set; }
            public List<OwnedEntity> Collection2 { get; set; }
        }

        protected class JsonEntityWithNesting
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public OwnedEntityExtraLevel OwnedReference1 { get; set; }
            public OwnedEntityExtraLevel OwnedReference2 { get; set; }
            public List<OwnedEntityExtraLevel> OwnedCollection1 { get; set; }
            public List<OwnedEntityExtraLevel> OwnedCollection2 { get; set; }
        }
#nullable enable
    }

    public abstract class TestTableBuilder<TEntity>
        where TEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true);

        public abstract TestCheckConstraintBuilder HasCheckConstraint(
            string name,
            string? sql);

        public abstract TestTriggerBuilder HasTrigger(string name);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression);
    }

    public class GenericTestTableBuilder<TEntity> : TestTableBuilder<TEntity>, IInfrastructure<TableBuilder<TEntity>>
        where TEntity : class
    {
        public GenericTestTableBuilder(TableBuilder<TEntity> tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private TableBuilder<TEntity> TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        TableBuilder<TEntity> IInfrastructure<TableBuilder<TEntity>>.Instance
            => TableBuilder;

        protected virtual TestTableBuilder<TEntity> Wrap(TableBuilder<TEntity> tableBuilder)
            => new GenericTestTableBuilder<TEntity>(tableBuilder);

        public override TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestCheckConstraintBuilder HasCheckConstraint(string name, string? sql)
            => new(TableBuilder.HasCheckConstraint(name, sql));

        public override TestTriggerBuilder HasTrigger(string name)
            => new NonGenericTestTriggerBuilder(TableBuilder.HasTrigger(name));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyExpression));
    }

    public class NonGenericTestTableBuilder<TEntity> : TestTableBuilder<TEntity>, IInfrastructure<TableBuilder>
        where TEntity : class
    {
        public NonGenericTestTableBuilder(TableBuilder tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private TableBuilder TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        TableBuilder IInfrastructure<TableBuilder>.Instance
            => TableBuilder;

        protected virtual TestTableBuilder<TEntity> Wrap(TableBuilder tableBuilder)
            => new NonGenericTestTableBuilder<TEntity>(tableBuilder);

        public override TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestCheckConstraintBuilder HasCheckConstraint(string name, string? sql)
            => new(TableBuilder.HasCheckConstraint(name, sql));

        public override TestTriggerBuilder HasTrigger(string name)
            => new NonGenericTestTriggerBuilder(TableBuilder.HasTrigger(name));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true);

        public abstract TestCheckConstraintBuilder HasCheckConstraint(
            string name,
            string? sql);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression);
    }

    public class GenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public GenericTestOwnedNavigationTableBuilder(OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>
            IInfrastructure<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => TableBuilder;

        protected virtual TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestCheckConstraintBuilder HasCheckConstraint(string name, string? sql)
            => new(TableBuilder.HasCheckConstraint(name, sql));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyExpression));
    }

    public class NonGenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationTableBuilder>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public NonGenericTestOwnedNavigationTableBuilder(OwnedNavigationTableBuilder tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private OwnedNavigationTableBuilder TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        OwnedNavigationTableBuilder IInfrastructure<OwnedNavigationTableBuilder>.Instance
            => TableBuilder;

        protected virtual TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> Wrap(OwnedNavigationTableBuilder tableBuilder)
            => new NonGenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestCheckConstraintBuilder HasCheckConstraint(string name, string? sql)
            => new(TableBuilder.HasCheckConstraint(name, sql));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestSplitTableBuilder<TEntity>
        where TEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestSplitTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true);

        public abstract TestTriggerBuilder HasTrigger(string name);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestSplitTableBuilder<TEntity> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestSplitTableBuilder<TEntity> : TestSplitTableBuilder<TEntity>, IInfrastructure<SplitTableBuilder<TEntity>>
        where TEntity : class
    {
        public GenericTestSplitTableBuilder(SplitTableBuilder<TEntity> tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private SplitTableBuilder<TEntity> TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        SplitTableBuilder<TEntity> IInfrastructure<SplitTableBuilder<TEntity>>.Instance
            => TableBuilder;

        protected virtual TestSplitTableBuilder<TEntity> Wrap(SplitTableBuilder<TEntity> tableBuilder)
            => new GenericTestSplitTableBuilder<TEntity>(tableBuilder);

        public override TestSplitTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestTriggerBuilder HasTrigger(string name)
            => new NonGenericTestTriggerBuilder(TableBuilder.HasTrigger(name));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyExpression));

        public override TestSplitTableBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(TableBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestSplitTableBuilder<TEntity> : TestSplitTableBuilder<TEntity>, IInfrastructure<SplitTableBuilder>
        where TEntity : class
    {
        public NonGenericTestSplitTableBuilder(SplitTableBuilder tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private SplitTableBuilder TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        SplitTableBuilder IInfrastructure<SplitTableBuilder>.Instance
            => TableBuilder;

        protected virtual TestSplitTableBuilder<TEntity> Wrap(SplitTableBuilder tableBuilder)
            => new NonGenericTestSplitTableBuilder<TEntity>(tableBuilder);

        public override TestSplitTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestTriggerBuilder HasTrigger(string name)
            => new NonGenericTestTriggerBuilder(TableBuilder.HasTrigger(name));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));

        public override TestSplitTableBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(TableBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public GenericTestOwnedNavigationSplitTableBuilder(OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>
            IInfrastructure<OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => TableBuilder;

        protected virtual TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyExpression));

        public override TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(string annotation, object? value)
            => Wrap(TableBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationSplitTableBuilder>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public NonGenericTestOwnedNavigationSplitTableBuilder(OwnedNavigationSplitTableBuilder tableBuilder)
        {
            TableBuilder = tableBuilder;
        }

        private OwnedNavigationSplitTableBuilder TableBuilder { get; }

        public override string? Name
            => TableBuilder.Name;

        public override string? Schema
            => TableBuilder.Schema;

        OwnedNavigationSplitTableBuilder IInfrastructure<OwnedNavigationSplitTableBuilder>.Instance
            => TableBuilder;

        protected virtual TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationSplitTableBuilder tableBuilder)
            => new NonGenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));

        public override TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(string annotation, object? value)
            => Wrap(TableBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestColumnBuilder<TProperty>
    {
        public abstract TestColumnBuilder<TProperty> HasColumnName(string? name);

        public abstract TestColumnBuilder<TProperty> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestColumnBuilder<TProperty> : TestColumnBuilder<TProperty>, IInfrastructure<ColumnBuilder<TProperty>>
    {
        public GenericTestColumnBuilder(ColumnBuilder<TProperty> columnBuilder)
        {
            ColumnBuilder = columnBuilder;
        }

        private ColumnBuilder<TProperty> ColumnBuilder { get; }

        ColumnBuilder<TProperty> IInfrastructure<ColumnBuilder<TProperty>>.Instance
            => ColumnBuilder;

        protected virtual TestColumnBuilder<TProperty> Wrap(ColumnBuilder<TProperty> columnBuilder)
            => new GenericTestColumnBuilder<TProperty>(columnBuilder);

        public override TestColumnBuilder<TProperty> HasColumnName(string? name)
            => Wrap(ColumnBuilder.HasColumnName(name));

        public override TestColumnBuilder<TProperty> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(ColumnBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestColumnBuilder<TProperty> : TestColumnBuilder<TProperty>, IInfrastructure<ColumnBuilder>
    {
        public NonGenericTestColumnBuilder(ColumnBuilder tableBuilder)
        {
            ColumnBuilder = tableBuilder;
        }

        private ColumnBuilder ColumnBuilder { get; }

        ColumnBuilder IInfrastructure<ColumnBuilder>.Instance
            => ColumnBuilder;

        protected virtual TestColumnBuilder<TProperty> Wrap(ColumnBuilder tableBuilder)
            => new NonGenericTestColumnBuilder<TProperty>(tableBuilder);

        public override TestColumnBuilder<TProperty> HasColumnName(string? name)
            => Wrap(ColumnBuilder.HasColumnName(name));

        public override TestColumnBuilder<TProperty> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(ColumnBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestViewBuilder<TEntity>
        where TEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression);
    }

    public class GenericTestViewBuilder<TEntity> : TestViewBuilder<TEntity>, IInfrastructure<ViewBuilder<TEntity>>
        where TEntity : class
    {
        public GenericTestViewBuilder(ViewBuilder<TEntity> tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private ViewBuilder<TEntity> ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        ViewBuilder<TEntity> IInfrastructure<ViewBuilder<TEntity>>.Instance
            => ViewBuilder;

        protected virtual TestViewBuilder<TEntity> Wrap(ViewBuilder<TEntity> tableBuilder)
            => new GenericTestViewBuilder<TEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyExpression));
    }

    public class NonGenericTestViewBuilder<TEntity> : TestViewBuilder<TEntity>, IInfrastructure<ViewBuilder>
        where TEntity : class
    {
        public NonGenericTestViewBuilder(ViewBuilder tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private ViewBuilder ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        ViewBuilder IInfrastructure<ViewBuilder>.Instance
            => ViewBuilder;

        protected virtual TestViewBuilder<TEntity> Wrap(ViewBuilder tableBuilder)
            => new NonGenericTestViewBuilder<TEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);
    }

    public class GenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public GenericTestOwnedNavigationViewBuilder(OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>
            IInfrastructure<OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => ViewBuilder;

        protected virtual TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public class NonGenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationViewBuilder>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public NonGenericTestOwnedNavigationViewBuilder(OwnedNavigationViewBuilder tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private OwnedNavigationViewBuilder ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        OwnedNavigationViewBuilder IInfrastructure<OwnedNavigationViewBuilder>.Instance
            => ViewBuilder;

        protected virtual TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> Wrap(OwnedNavigationViewBuilder tableBuilder)
            => new NonGenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestSplitViewBuilder<TEntity>
        where TEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestSplitViewBuilder<TEntity> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestSplitViewBuilder<TEntity> : TestSplitViewBuilder<TEntity>, IInfrastructure<SplitViewBuilder<TEntity>>
        where TEntity : class
    {
        public GenericTestSplitViewBuilder(SplitViewBuilder<TEntity> tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private SplitViewBuilder<TEntity> ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        SplitViewBuilder<TEntity> IInfrastructure<SplitViewBuilder<TEntity>>.Instance
            => ViewBuilder;

        protected virtual TestSplitViewBuilder<TEntity> Wrap(SplitViewBuilder<TEntity> tableBuilder)
            => new GenericTestSplitViewBuilder<TEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyExpression));

        public override TestSplitViewBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ViewBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestSplitViewBuilder<TEntity> : TestSplitViewBuilder<TEntity>, IInfrastructure<SplitViewBuilder>
        where TEntity : class
    {
        public NonGenericTestSplitViewBuilder(SplitViewBuilder tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private SplitViewBuilder ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        SplitViewBuilder IInfrastructure<SplitViewBuilder>.Instance
            => ViewBuilder;

        protected virtual TestSplitViewBuilder<TEntity> Wrap(SplitViewBuilder tableBuilder)
            => new NonGenericTestSplitViewBuilder<TEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));

        public override TestSplitViewBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ViewBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public GenericTestOwnedNavigationSplitViewBuilder(OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>
            IInfrastructure<OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => ViewBuilder;

        protected virtual TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));

        public override TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ViewBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationSplitViewBuilder>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public NonGenericTestOwnedNavigationSplitViewBuilder(OwnedNavigationSplitViewBuilder tableBuilder)
        {
            ViewBuilder = tableBuilder;
        }

        private OwnedNavigationSplitViewBuilder ViewBuilder { get; }

        public override string? Name
            => ViewBuilder.Name;

        public override string? Schema
            => ViewBuilder.Schema;

        OwnedNavigationSplitViewBuilder IInfrastructure<OwnedNavigationSplitViewBuilder>.Instance
            => ViewBuilder;

        protected virtual TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationSplitViewBuilder tableBuilder)
            => new NonGenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));

        public override TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ViewBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestViewColumnBuilder<TProperty>
    {
        public abstract TestViewColumnBuilder<TProperty> HasColumnName(string? name);

        public abstract TestViewColumnBuilder<TProperty> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestViewColumnBuilder<TProperty> : TestViewColumnBuilder<TProperty>, IInfrastructure<ViewColumnBuilder<TProperty>>
    {
        public GenericTestViewColumnBuilder(ViewColumnBuilder<TProperty> columnBuilder)
        {
            ViewColumnBuilder = columnBuilder;
        }

        private ViewColumnBuilder<TProperty> ViewColumnBuilder { get; }

        ViewColumnBuilder<TProperty> IInfrastructure<ViewColumnBuilder<TProperty>>.Instance
            => ViewColumnBuilder;

        protected virtual TestViewColumnBuilder<TProperty> Wrap(ViewColumnBuilder<TProperty> columnBuilder)
            => new GenericTestViewColumnBuilder<TProperty>(columnBuilder);

        public override TestViewColumnBuilder<TProperty> HasColumnName(string? name)
            => Wrap(ViewColumnBuilder.HasColumnName(name));

        public override TestViewColumnBuilder<TProperty> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(ViewColumnBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestViewColumnBuilder<TProperty>
        : TestViewColumnBuilder<TProperty>, IInfrastructure<ViewColumnBuilder>
    {
        public NonGenericTestViewColumnBuilder(ViewColumnBuilder tableBuilder)
        {
            ViewColumnBuilder = tableBuilder;
        }

        private ViewColumnBuilder ViewColumnBuilder { get; }

        ViewColumnBuilder IInfrastructure<ViewColumnBuilder>.Instance
            => ViewColumnBuilder;

        protected virtual TestViewColumnBuilder<TProperty> Wrap(ViewColumnBuilder viewColumnBuilder)
            => new NonGenericTestViewColumnBuilder<TProperty>(viewColumnBuilder);

        public override TestViewColumnBuilder<TProperty> HasColumnName(string? name)
            => Wrap(ViewColumnBuilder.HasColumnName(name));

        public override TestViewColumnBuilder<TProperty> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(ViewColumnBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestStoredProcedureBuilder<TEntity>
        where TEntity : class
    {
        public abstract TestStoredProcedureBuilder<TEntity> HasParameter(
            string propertyName);

        public abstract TestStoredProcedureBuilder<TEntity> HasParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            where TDerivedEntity : class, TEntity;

        public abstract TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            where TDerivedEntity : class, TEntity;

        public abstract TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter(
            string propertyName);

        public abstract TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            where TDerivedEntity : class, TEntity;

        public abstract TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            where TDerivedEntity : class, TEntity;

        public abstract TestStoredProcedureBuilder<TEntity> HasRowsAffectedParameter();

        public abstract TestStoredProcedureBuilder<TEntity> HasRowsAffectedParameter(
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName);

        public abstract TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName,
            Action<TestStoredProcedureResultColumnBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            where TDerivedEntity : class, TEntity;

        public abstract TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            where TDerivedEntity : class, TEntity;

        public abstract TestStoredProcedureBuilder<TEntity> HasRowsAffectedResultColumn();

        public abstract TestStoredProcedureBuilder<TEntity> HasRowsAffectedResultColumn(
            Action<TestStoredProcedureResultColumnBuilder> buildAction);

        public abstract TestStoredProcedureBuilder<TEntity> HasRowsAffectedReturnValue(bool rowsAffectedReturned = true);

        public abstract TestStoredProcedureBuilder<TEntity> HasAnnotation(string annotation, object? value);
    }

    public class GenericTestStoredProcedureBuilder<TEntity>
        : TestStoredProcedureBuilder<TEntity>, IInfrastructure<StoredProcedureBuilder<TEntity>>
        where TEntity : class
    {
        public GenericTestStoredProcedureBuilder(StoredProcedureBuilder<TEntity> storedProcedureBuilder)
        {
            StoredProcedureBuilder = storedProcedureBuilder;
        }

        private StoredProcedureBuilder<TEntity> StoredProcedureBuilder { get; }

        StoredProcedureBuilder<TEntity> IInfrastructure<StoredProcedureBuilder<TEntity>>.Instance
            => StoredProcedureBuilder;

        protected virtual TestStoredProcedureBuilder<TEntity> Wrap(StoredProcedureBuilder<TEntity> storedProcedureBuilder)
            => new GenericTestStoredProcedureBuilder<TEntity>(storedProcedureBuilder);

        public override TestStoredProcedureBuilder<TEntity> HasParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter<TProperty>(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasParameter<TProperty>(
                    propertyExpression, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter<TProperty>(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter<TProperty>(
                    propertyExpression, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyExpression, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedParameter()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter());

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedParameter(
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter(s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn<TProperty>(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasResultColumn<TProperty>(
                    propertyExpression, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasResultColumn(
                    propertyExpression, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedResultColumn()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn());

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedResultColumn(
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn(s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedReturnValue(bool rowsAffectedReturned)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedReturnValue(rowsAffectedReturned));

        public override TestStoredProcedureBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(StoredProcedureBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestStoredProcedureBuilder<TEntity>
        : TestStoredProcedureBuilder<TEntity>, IInfrastructure<StoredProcedureBuilder>
        where TEntity : class
    {
        public NonGenericTestStoredProcedureBuilder(StoredProcedureBuilder storedProcedureBuilder)
        {
            StoredProcedureBuilder = storedProcedureBuilder;
        }

        private StoredProcedureBuilder StoredProcedureBuilder { get; }

        StoredProcedureBuilder IInfrastructure<StoredProcedureBuilder>.Instance
            => StoredProcedureBuilder;

        protected virtual TestStoredProcedureBuilder<TEntity> Wrap(StoredProcedureBuilder storedProcedureBuilder)
            => new NonGenericTestStoredProcedureBuilder<TEntity>(storedProcedureBuilder);

        public override TestStoredProcedureBuilder<TEntity> HasParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasParameter(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasParameter(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasOriginalValueParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedParameter()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter());

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedParameter(
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter(s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasResultColumn(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasResultColumn(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedResultColumn()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn());

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedResultColumn(
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn(s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestStoredProcedureBuilder<TEntity> HasRowsAffectedReturnValue(bool rowsAffectedReturned)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedReturnValue(rowsAffectedReturned));

        public override TestStoredProcedureBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(StoredProcedureBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
            string propertyName);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
            string propertyName);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter();

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter(
            Action<TestStoredProcedureParameterBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
            string propertyName);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
            string propertyName,
            Action<TestStoredProcedureResultColumnBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn();

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn(
            Action<TestStoredProcedureResultColumnBuilder> buildAction);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>
            HasRowsAffectedReturnValue(bool rowsAffectedReturned = true);

        public abstract TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value);
    }

    public class GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>
        : TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>,
            IInfrastructure<OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public GenericTestOwnedNavigationStoredProcedureBuilder(
            OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> storedProcedureBuilder)
        {
            StoredProcedureBuilder = storedProcedureBuilder;
        }

        private OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> StoredProcedureBuilder { get; }

        OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>
            IInfrastructure<OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => StoredProcedureBuilder;

        protected virtual TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> storedProcedureBuilder)
            => new GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(storedProcedureBuilder);

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyName));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyExpression));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyExpression, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter());

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter(
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter(s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
            string propertyName,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasResultColumn(
                    propertyExpression, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn());

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn(
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn(s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedReturnValue(
            bool rowsAffectedReturned)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedReturnValue(rowsAffectedReturned));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(StoredProcedureBuilder.HasAnnotation(annotation, value));
    }

    public class NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>
        : TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>,
            IInfrastructure<OwnedNavigationStoredProcedureBuilder>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public NonGenericTestOwnedNavigationStoredProcedureBuilder(OwnedNavigationStoredProcedureBuilder storedProcedureBuilder)
        {
            StoredProcedureBuilder = storedProcedureBuilder;
        }

        private OwnedNavigationStoredProcedureBuilder StoredProcedureBuilder { get; }

        OwnedNavigationStoredProcedureBuilder IInfrastructure<OwnedNavigationStoredProcedureBuilder>.Instance
            => StoredProcedureBuilder;

        protected virtual TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationStoredProcedureBuilder storedProcedureBuilder)
            => new NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(storedProcedureBuilder);

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression.GetMemberAccess().Name));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasParameter(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyName));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
            string propertyName,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyName, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasOriginalValueParameter(propertyExpression.GetMemberAccess().Name));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasOriginalValueParameter(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter());

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter(
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedParameter(s => buildAction(new TestStoredProcedureParameterBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
            string propertyName,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression.GetMemberAccess().Name));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(
                StoredProcedureBuilder.HasResultColumn(
                    propertyExpression.GetMemberAccess().Name, s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn()
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn());

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn(
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedResultColumn(s => buildAction(new TestStoredProcedureResultColumnBuilder(s))));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedReturnValue(
            bool rowsAffectedReturned)
            => Wrap(StoredProcedureBuilder.HasRowsAffectedReturnValue(rowsAffectedReturned));

        public override TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(StoredProcedureBuilder.HasAnnotation(annotation, value));
    }

    public class TestStoredProcedureParameterBuilder : IInfrastructure<StoredProcedureParameterBuilder>
    {
        public TestStoredProcedureParameterBuilder(StoredProcedureParameterBuilder storedProcedureParameterBuilder)
        {
            StoredProcedureParameterBuilder = storedProcedureParameterBuilder;
        }

        private StoredProcedureParameterBuilder StoredProcedureParameterBuilder { get; }

        StoredProcedureParameterBuilder IInfrastructure<StoredProcedureParameterBuilder>.Instance
            => StoredProcedureParameterBuilder;

        protected virtual TestStoredProcedureParameterBuilder Wrap(StoredProcedureParameterBuilder storedProcedureParameterBuilder)
            => new(storedProcedureParameterBuilder);

        public virtual TestStoredProcedureParameterBuilder HasName(string name)
            => Wrap(StoredProcedureParameterBuilder.HasName(name));

        public virtual TestStoredProcedureParameterBuilder IsOutput()
            => Wrap(StoredProcedureParameterBuilder.IsOutput());

        public virtual TestStoredProcedureParameterBuilder IsInputOutput()
            => Wrap(StoredProcedureParameterBuilder.IsInputOutput());

        public virtual TestStoredProcedureParameterBuilder HasAnnotation(
            string annotation,
            object? value)
            => Wrap(StoredProcedureParameterBuilder.HasAnnotation(annotation, value));
    }

    public class TestStoredProcedureResultColumnBuilder : IInfrastructure<StoredProcedureResultColumnBuilder>
    {
        public TestStoredProcedureResultColumnBuilder(StoredProcedureResultColumnBuilder storedProcedureResultColumnBuilder)
        {
            StoredProcedureResultColumnBuilder = storedProcedureResultColumnBuilder;
        }

        private StoredProcedureResultColumnBuilder StoredProcedureResultColumnBuilder { get; }

        StoredProcedureResultColumnBuilder IInfrastructure<StoredProcedureResultColumnBuilder>.Instance
            => StoredProcedureResultColumnBuilder;

        protected virtual TestStoredProcedureResultColumnBuilder Wrap(StoredProcedureResultColumnBuilder storedProcedureResultColumnBuilder)
            => new(storedProcedureResultColumnBuilder);

        public virtual TestStoredProcedureResultColumnBuilder HasName(string name)
            => Wrap(StoredProcedureResultColumnBuilder.HasName(name));

        public virtual TestStoredProcedureResultColumnBuilder HasAnnotation(
            string annotation,
            object? value)
            => Wrap(StoredProcedureResultColumnBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestTableValuedFunctionBuilder<TEntity> : DbFunctionBuilderBase
        where TEntity : class
    {
        protected TestTableValuedFunctionBuilder(IMutableDbFunction function)
            : base(function)
        {
        }

        public new abstract TestTableValuedFunctionBuilder<TEntity> HasName(string name);

        public new abstract TestTableValuedFunctionBuilder<TEntity> HasSchema(string? schema);

        public abstract TestTableValuedFunctionBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestTableValuedFunctionBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<DbFunctionParameterBuilder> buildAction);
    }

    public abstract class TestTriggerBuilder
    {
        public abstract TestTriggerBuilder HasDatabaseName(string name);
        public abstract TestTriggerBuilder HasAnnotation(string annotation, object? value);
    }

    public class NonGenericTestTriggerBuilder : TestTriggerBuilder, IInfrastructure<TableTriggerBuilder>
    {
        public NonGenericTestTriggerBuilder(TableTriggerBuilder triggerBuilder)
        {
            TriggerBuilder = triggerBuilder;
        }

        private TableTriggerBuilder TriggerBuilder { get; }

        TableTriggerBuilder IInfrastructure<TableTriggerBuilder>.Instance
            => TriggerBuilder;

        protected virtual TestTriggerBuilder Wrap(TableTriggerBuilder checkConstraintBuilder)
            => new NonGenericTestTriggerBuilder(checkConstraintBuilder);

        public override TestTriggerBuilder HasDatabaseName(string name)
            => Wrap(TriggerBuilder.HasDatabaseName(name));

        public override TestTriggerBuilder HasAnnotation(string annotation, object? value)
            => Wrap(TriggerBuilder.HasAnnotation(annotation, value));
    }

    public class TestCheckConstraintBuilder : IInfrastructure<CheckConstraintBuilder>
    {
        public TestCheckConstraintBuilder(CheckConstraintBuilder checkConstraintBuilder)
        {
            CheckConstraintBuilder = checkConstraintBuilder;
        }

        private CheckConstraintBuilder CheckConstraintBuilder { get; }

        CheckConstraintBuilder IInfrastructure<CheckConstraintBuilder>.Instance
            => CheckConstraintBuilder;

        protected virtual TestCheckConstraintBuilder Wrap(CheckConstraintBuilder checkConstraintBuilder)
            => new(checkConstraintBuilder);

        public virtual TestCheckConstraintBuilder HasName(string name)
            => Wrap(CheckConstraintBuilder.HasName(name));

        public virtual TestCheckConstraintBuilder HasAnnotation(string annotation, object? value)
            => Wrap(CheckConstraintBuilder.HasAnnotation(annotation, value));
    }
}
