// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

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
            modelBuilder.Entity<Order>().SplitToTable("OrderDetails", s =>
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

        [ConditionalFact]
        public virtual void Can_use_table_splitting_with_schema()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().ToTable("Order", "dbo")
                .SplitToTable("OrderDetails", "sch", s =>
                    s.ExcludeFromMigrations()
                    .Property(o => o.CustomerId).HasColumnName("id"));
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.False(entity.IsTableExcludedFromMigrations());
            Assert.False(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order", "dbo")));
            Assert.True(entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Same(entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.Table("OrderDetails", "sch")));
            Assert.Equal(RelationalStrings.TableNotMappedEntityType(nameof(Order), "Order"),
                Assert.Throws<InvalidOperationException>(() => entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.Table("Order"))).Message);

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
                .SplitToView("OrderDetails", s =>
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
                .SplitToView("OrderDetails", "sch", s =>
                    s.Property(o => o.CustomerId).HasColumnName("id"));
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;

            Assert.Same(entity.GetMappingFragments().Single(), entity.FindMappingFragment(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Equal(RelationalStrings.TableNotMappedEntityType(nameof(Order), "Order"),
                Assert.Throws<InvalidOperationException>(() => entity.IsTableExcludedFromMigrations(StoreObjectIdentifier.View("Order"))).Message);

            var customerId = entity.FindProperty(nameof(Order.CustomerId))!;
            Assert.Equal("CustomerId", customerId.GetColumnName());
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order", "dbo")));
            Assert.Equal("id", customerId.GetColumnName(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Same(customerId.GetOverrides().Single(), customerId.FindOverrides(StoreObjectIdentifier.View("OrderDetails", "sch")));
            Assert.Null(customerId.GetColumnName(StoreObjectIdentifier.View("Order")));
        }
    }

    public abstract class RelationalInheritanceTestBase : InheritanceTestBase
    {
        [ConditionalFact]
        public virtual void Can_use_table_splitting()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().SplitToTable("OrderDetails", s =>
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
        
        [ConditionalFact]
        public virtual void Sproc_overrides_update_when_renamed_in_TPH()
        {
            var modelBuilder = CreateModelBuilder();
            
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<Book>();

            modelBuilder.Entity<BookLabel>()
                .Ignore(s => s.SpecialBookLabel)
                .InsertUsingStoredProcedure(
                    s => s.SuppressTransactions().HasAnnotation("foo", "bar1")
                        .HasParameter(b => b.BookId)
                        .HasParameter("Discriminator")
                        .HasResultColumn(b => b.Id, p => p.HasName("InsertId")))
                .UpdateUsingStoredProcedure(
                    s => s.SuppressTransactions().HasAnnotation("foo", "bar2")
                        .HasParameter(b => b.Id, p => p.HasName("UpdateId"))
                        .HasParameter(b => b.BookId))
                .DeleteUsingStoredProcedure(
                    s => s.SuppressTransactions().HasAnnotation("foo", "bar3")
                        .HasParameter(b => b.Id, p => p.HasName("DeleteId")));

            modelBuilder.Entity<SpecialBookLabel>()
                .Ignore(s => s.BookLabel);
            modelBuilder.Entity<ExtraSpecialBookLabel>();

            modelBuilder.Entity<BookLabel>()
                .Ignore(s => s.SpecialBookLabel)
                .InsertUsingStoredProcedure("Insert", s => { })
                .UpdateUsingStoredProcedure("Update", "dbo", s => { })
                .DeleteUsingStoredProcedure("BookLabel_Delete", s => { });

            var model = modelBuilder.FinalizeModel();

            var bookLabel = model.FindEntityType(typeof(BookLabel))!;
            var insertSproc = bookLabel.GetInsertStoredProcedure()!;
            Assert.Equal("Insert", insertSproc.Name);
            Assert.Null(insertSproc.Schema);
            Assert.Equal(new[] { "BookId", "Discriminator" }, insertSproc.Parameters);
            Assert.Equal(new[] { "Id" }, insertSproc.ResultColumns);
            Assert.True(insertSproc.AreTransactionsSuppressed);
            Assert.Equal("bar1", insertSproc["foo"]);

            var updateSproc = bookLabel.GetUpdateStoredProcedure()!;
            Assert.Equal("Update", updateSproc.Name);
            Assert.Equal("dbo", updateSproc.Schema);
            Assert.Equal(new[] { "Id", "BookId" }, updateSproc.Parameters);
            Assert.Empty(updateSproc.ResultColumns);
            Assert.True(updateSproc.AreTransactionsSuppressed);
            Assert.Equal("bar2", updateSproc["foo"]);
            
            var deleteSproc = bookLabel.GetDeleteStoredProcedure()!;
            Assert.Equal("BookLabel_Delete", deleteSproc.Name);
            Assert.Null(deleteSproc.Schema);
            Assert.Equal(new[] { "Id" }, deleteSproc.Parameters);
            Assert.Empty(deleteSproc.ResultColumns);
            Assert.True(deleteSproc.AreTransactionsSuppressed);
            Assert.Equal("bar3", deleteSproc["foo"]);

            var id = bookLabel.FindProperty(nameof(BookLabel.Id))!;
            Assert.Equal(3, id.GetOverrides().Count());
            Assert.Equal("InsertId",
                id.GetColumnName(StoreObjectIdentifier.Create(bookLabel, StoreObjectType.InsertStoredProcedure)!.Value));
            Assert.Equal("UpdateId",
                id.GetColumnName(StoreObjectIdentifier.Create(bookLabel, StoreObjectType.UpdateStoredProcedure)!.Value));
            Assert.Equal("DeleteId",
                id.GetColumnName(StoreObjectIdentifier.Create(bookLabel, StoreObjectType.DeleteStoredProcedure)!.Value));

            var specialBookLabel = model.FindEntityType(typeof(SpecialBookLabel))!;
            Assert.Same(insertSproc, specialBookLabel.GetInsertStoredProcedure());
            Assert.Same(updateSproc, specialBookLabel.GetUpdateStoredProcedure());
            Assert.Same(deleteSproc, specialBookLabel.GetDeleteStoredProcedure());
            
            var extraSpecialBookLabel = model.FindEntityType(typeof(ExtraSpecialBookLabel))!;
            Assert.Same(insertSproc, extraSpecialBookLabel.GetInsertStoredProcedure());
            Assert.Same(updateSproc, extraSpecialBookLabel.GetUpdateStoredProcedure());
            Assert.Same(deleteSproc, extraSpecialBookLabel.GetDeleteStoredProcedure());
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
                    
                    lb.SplitToTable("BookLabelDetails", s =>
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
                    r.SplitToView("OrderDetails", s =>
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
    }

    public abstract class TestTableBuilder<TEntity>
        where TEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestTableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true);

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

        OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => TableBuilder;

        protected virtual TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyExpression));
    }

    public class NonGenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>, IInfrastructure<OwnedNavigationTableBuilder>
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

        OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>>.Instance
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
    }

    public class NonGenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>, IInfrastructure<OwnedNavigationSplitTableBuilder>
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

        protected virtual TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> Wrap(OwnedNavigationSplitTableBuilder tableBuilder)
            => new NonGenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
            => Wrap(TableBuilder.ExcludeFromMigrations(excluded));

        public override TestColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestColumnBuilder<TProperty>(TableBuilder.Property(propertyName));

        public override TestColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestColumnBuilder<TProperty>(TableBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestColumnBuilder<TProperty>
    {
        public abstract TestColumnBuilder<TProperty> HasColumnName(string? name);
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

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression);
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

        OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => ViewBuilder;

        protected virtual TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public class NonGenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>, IInfrastructure<OwnedNavigationViewBuilder>
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

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestSplitViewBuilder<TEntity>
        where TEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression);
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
    }

    public abstract class TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract string? Name { get; }

        public abstract string? Schema { get; }

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression);
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

        OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => ViewBuilder;

        protected virtual TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public class NonGenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> :
        TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>, IInfrastructure<OwnedNavigationSplitViewBuilder>
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

        protected virtual TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity> Wrap(OwnedNavigationSplitViewBuilder tableBuilder)
            => new NonGenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property(propertyName));

        public override TestViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestViewColumnBuilder<TProperty>(ViewBuilder.Property<TProperty>(propertyExpression.GetPropertyAccess().Name));
    }

    public abstract class TestViewColumnBuilder<TProperty>
    {
        public abstract TestViewColumnBuilder<TProperty> HasColumnName(string? name);
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

        public abstract TestStoredProcedureBuilder<TEntity> SuppressTransactions(bool suppress = true);

        //public abstract StoredProcedureBuilder<TEntity> HasRowsAffectedParameter(
        //    Action<TestStoredProcedureParameterBuilder<TEntity>> buildAction);

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
            string propertyName, Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter<TProperty>(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter<TProperty>(propertyExpression, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName, Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn<TProperty>(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn<TProperty>(propertyExpression, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> SuppressTransactions(bool suppress)
            => Wrap(StoredProcedureBuilder.SuppressTransactions(suppress));

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
            string propertyName, Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyName, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(
                propertyExpression.GetMemberAccess().Name, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasParameter(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureParameterBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasParameter(
                propertyExpression.GetMemberAccess().Name, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn(
            string propertyName, Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyName, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(
                propertyExpression.GetMemberAccess().Name, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
            => Wrap(StoredProcedureBuilder.HasResultColumn(propertyExpression.GetMemberAccess().Name));

        public override TestStoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
            Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
            Action<TestStoredProcedureResultColumnBuilder> buildAction)
            => Wrap(StoredProcedureBuilder.HasResultColumn(
                propertyExpression.GetMemberAccess().Name, s => buildAction(new(s))));

        public override TestStoredProcedureBuilder<TEntity> SuppressTransactions(bool suppress)
            => Wrap(StoredProcedureBuilder.SuppressTransactions(suppress));

        public override TestStoredProcedureBuilder<TEntity> HasAnnotation(string annotation, object? value)
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
            => new TestStoredProcedureParameterBuilder(storedProcedureParameterBuilder);

        public TestStoredProcedureParameterBuilder HasName(string? name)
            => Wrap(StoredProcedureParameterBuilder.HasName(name));

        //public TestStoredProcedureParameterBuilder<TProperty> IsOutput(bool isOutput = true);
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
            => new TestStoredProcedureResultColumnBuilder(storedProcedureResultColumnBuilder);

        public TestStoredProcedureResultColumnBuilder HasName(string? name)
            => Wrap(StoredProcedureResultColumnBuilder.HasName(name));
    }

    //public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToFunction<TEntity>(
    //    this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
    //    string name,
    //    Action<TestTableValuedFunctionBuilder<TEntity>> buildAction)
    //    where TEntity : class
    //{
    //    return builder;
    //}

    //public abstract class TestTableValuedFunctionBuilder<TEntity> : DbFunctionBuilderBase
    //    where TEntity : class
    //{
    //    protected TestTableValuedFunctionBuilder(IMutableDbFunction function) : base(function)
    //    {
    //    }

    //    public new abstract TestTableValuedFunctionBuilder<TEntity> HasName(string name);

    //    public new abstract TestTableValuedFunctionBuilder<TEntity> HasSchema(string? schema);

    //    public abstract TestTableValuedFunctionBuilder<TEntity> HasParameter<TProperty>(
    //        Expression<Func<TEntity, TProperty>> propertyExpression);

    //    public abstract TestTableValuedFunctionBuilder<TEntity> HasParameter<TProperty>(
    //        Expression<Func<TEntity, TProperty>> propertyExpression,
    //        Action<StoredProcedureParameterBuilder<TEntity>> buildAction);
    //}

    public abstract class TestTriggerBuilder
    {
        public abstract TestTriggerBuilder HasName(string name);
        public abstract TestTriggerBuilder HasAnnotation(string annotation, object? value);
    }

    public class NonGenericTestTriggerBuilder : TestTriggerBuilder, IInfrastructure<TriggerBuilder>
    {
        public NonGenericTestTriggerBuilder(TriggerBuilder triggerBuilder)
        {
            TriggerBuilder = triggerBuilder;
        }

        private TriggerBuilder TriggerBuilder { get; }

        TriggerBuilder IInfrastructure<TriggerBuilder>.Instance
            => TriggerBuilder;

        protected virtual TestTriggerBuilder Wrap(TriggerBuilder checkConstraintBuilder)
            => new NonGenericTestTriggerBuilder(checkConstraintBuilder);

        public override TestTriggerBuilder HasName(string name)
            => Wrap(TriggerBuilder.HasName(name));

        public override TestTriggerBuilder HasAnnotation(string annotation, object? value)
            => Wrap(TriggerBuilder.HasAnnotation(annotation, value));
    }

    public abstract class TestCheckConstraintBuilder
    {
        public abstract TestCheckConstraintBuilder HasName(string name);

        public abstract TestCheckConstraintBuilder HasAnnotation(string annotation, object? value);
    }

    public class NonGenericTestCheckConstraintBuilder : TestCheckConstraintBuilder, IInfrastructure<CheckConstraintBuilder>
    {
        public NonGenericTestCheckConstraintBuilder(CheckConstraintBuilder checkConstraintBuilder)
        {
            CheckConstraintBuilder = checkConstraintBuilder;
        }

        private CheckConstraintBuilder CheckConstraintBuilder { get; }

        CheckConstraintBuilder IInfrastructure<CheckConstraintBuilder>.Instance
            => CheckConstraintBuilder;

        protected virtual TestCheckConstraintBuilder Wrap(CheckConstraintBuilder checkConstraintBuilder)
            => new NonGenericTestCheckConstraintBuilder(checkConstraintBuilder);

        public override TestCheckConstraintBuilder HasName(string name)
            => Wrap(CheckConstraintBuilder.HasName(name));

        public override TestCheckConstraintBuilder HasAnnotation(string annotation, object? value)
            => Wrap(CheckConstraintBuilder.HasAnnotation(annotation, value));
    }
}
