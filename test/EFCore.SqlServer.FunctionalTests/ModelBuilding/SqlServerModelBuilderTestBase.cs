// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqlServerModelBuilderTestBase : RelationalModelBuilderTest
{
    public abstract class SqlServerNonRelationship(SqlServerModelBuilderFixture fixture) : RelationalNonRelationshipTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>
    {
        [ConditionalFact]
        public virtual void Index_has_a_filter_if_nonclustered_unique_with_nullable_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder
                .Entity<Customer>();
            var indexBuilder = entityTypeBuilder
                .HasIndex(ix => ix.Name)
                .IsUnique();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;
            var index = entityType.GetIndexes().Single();
            Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

            indexBuilder.IsUnique(false);

            Assert.Null(index.GetFilter());

            indexBuilder.IsUnique();

            Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

            indexBuilder.IsClustered();

            Assert.Null(index.GetFilter());

            indexBuilder.IsClustered(false);

            Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

            entityTypeBuilder.Property(e => e.Name).IsRequired();

            Assert.Null(index.GetFilter());

            entityTypeBuilder.Property(e => e.Name).IsRequired(false);

            Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

            entityTypeBuilder.Property(e => e.Name).HasColumnName("RelationalName");

            Assert.Equal("[RelationalName] IS NOT NULL", index.GetFilter());

            entityTypeBuilder.Property(e => e.Name).HasColumnName("SqlServerName");

            Assert.Equal("[SqlServerName] IS NOT NULL", index.GetFilter());

            entityTypeBuilder.Property(e => e.Name).HasColumnName(null);

            Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

            indexBuilder.HasFilter("Foo");

            Assert.Equal("Foo", index.GetFilter());

            indexBuilder.HasFilter(null);

            Assert.Null(index.GetFilter());
        }

        [ConditionalFact]
        public void Indexes_can_have_same_name_across_tables()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.Id, "Ix_Id")
                .IsUnique();
            modelBuilder.Entity<CustomerDetails>()
                .HasIndex(e => e.CustomerId, "Ix_Id")
                .IsUnique();

            var model = modelBuilder.FinalizeModel();

            var customerIndex = model.FindEntityType(typeof(Customer))!.GetIndexes().Single();
            Assert.Equal("Ix_Id", customerIndex.Name);
            Assert.Equal("Ix_Id", customerIndex.GetDatabaseName());
            Assert.Equal(
                "Ix_Id", customerIndex.GetDatabaseName(
                    StoreObjectIdentifier.Table("Customer")));

            var detailsIndex = model.FindEntityType(typeof(CustomerDetails))!.GetIndexes().Single();
            Assert.Equal("Ix_Id", detailsIndex.Name);
            Assert.Equal("Ix_Id", detailsIndex.GetDatabaseName());
            Assert.Equal(
                "Ix_Id", detailsIndex.GetDatabaseName(
                    StoreObjectIdentifier.Table("CustomerDetails")));
        }

        [ConditionalFact]
        public virtual void Can_set_store_type_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveColumnType("smallint");
                    c.Properties<string>().HaveColumnType("nchar(max)");
                    c.Properties(typeof(Nullable<>)).HavePrecision(2);
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int?>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal("smallint", entityType.FindProperty(Customer.IdProperty.Name)!.GetColumnType());
            Assert.Equal("smallint", entityType.FindProperty("Up")!.GetColumnType());
            Assert.Equal("nchar(max)", entityType.FindProperty("Down")!.GetColumnType());
            var charm = entityType.FindProperty("Charm")!;
            Assert.Equal("smallint", charm.GetColumnType());
            Assert.Null(charm.GetPrecision());
            Assert.Equal("nchar(max)", entityType.FindProperty("Strange")!.GetColumnType());
            var top = entityType.FindProperty("Top")!;
            Assert.Equal("smallint", top.GetColumnType());
            Assert.Equal(2, top.GetPrecision());
            Assert.Equal("nchar(max)", entityType.FindProperty("Bottom")!.GetColumnType());
        }

        [ConditionalFact]
        public virtual void Can_set_fixed_length_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().AreFixedLength(false);
                    c.Properties<string>().AreFixedLength();
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.False(entityType.FindProperty(Customer.IdProperty.Name)!.IsFixedLength());
            Assert.False(entityType.FindProperty("Up")!.IsFixedLength());
            Assert.True(entityType.FindProperty("Down")!.IsFixedLength());
            Assert.False(entityType.FindProperty("Charm")!.IsFixedLength());
            Assert.True(entityType.FindProperty("Strange")!.IsFixedLength());
            Assert.False(entityType.FindProperty("Top")!.IsFixedLength());
            Assert.True(entityType.FindProperty("Bottom")!.IsFixedLength());
        }

        [ConditionalFact]
        public virtual void Can_set_collation_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().UseCollation("Latin1_General_CS_AS_KS_WS");
                    c.Properties<string>().UseCollation("Latin1_General_BIN");
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty(Customer.IdProperty.Name)!.GetCollation());
            Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Up")!.GetCollation());
            Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Down")!.GetCollation());
            Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Charm")!.GetCollation());
            Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Strange")!.GetCollation());
            Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Top")!.GetCollation());
            Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Bottom")!.GetCollation());
        }

        [ConditionalFact]
        public virtual void Can_set_store_type_for_primitive_collection()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).HasColumnType("national character varying(255)");
                    b.PrimitiveCollection(e => e.Down).HasColumnType("nchar(10)");
                    b.PrimitiveCollection<int[]>("Charm").HasColumnType("nvarchar(25)");
                    b.PrimitiveCollection<string[]>("Strange").HasColumnType("text");
                    b.PrimitiveCollection<ObservableCollection<int>>("Top").HasColumnType("char(100)");
                    ;
                    b.PrimitiveCollection<ObservableCollection<string>?>("Bottom").HasColumnType("varchar(max)");
                    ;
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Equal("int", entityType.FindProperty(nameof(CollectionQuarks.Id))!.GetColumnType());
            Assert.Equal("national character varying(255)", entityType.FindProperty("Up")!.GetColumnType());
            Assert.Equal("nchar(10)", entityType.FindProperty("Down")!.GetColumnType());
            Assert.Equal("nvarchar(25)", entityType.FindProperty("Charm")!.GetColumnType());
            Assert.Equal("text", entityType.FindProperty("Strange")!.GetColumnType());
            Assert.Equal("char(100)", entityType.FindProperty("Top")!.GetColumnType());
            Assert.Equal("varchar(max)", entityType.FindProperty("Bottom")!.GetColumnType());
        }

        [ConditionalFact]
        public virtual void Can_set_fixed_length_for_primitive_collection()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).IsFixedLength(false);
                    b.PrimitiveCollection(e => e.Down).IsFixedLength();
                    b.PrimitiveCollection<int[]>("Charm").IsFixedLength();
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsFixedLength());
            Assert.True(entityType.FindProperty("Down")!.IsFixedLength());
            Assert.True(entityType.FindProperty("Charm")!.IsFixedLength());
        }

        [ConditionalFact]
        public virtual void Can_set_collation_for_primitive_collection()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).UseCollation("Latin1_General_CS_AS_KS_WS");
                    b.PrimitiveCollection(e => e.Down).UseCollation("Latin1_General_BIN");
                    b.PrimitiveCollection<int[]>("Charm").UseCollation("Latin1_General_CI_AI");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Up")!.GetCollation());
            Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Down")!.GetCollation());
            Assert.Equal("Latin1_General_CI_AI", entityType.FindProperty("Charm")!.GetCollation());
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Can_avoid_attributes_when_discovering_properties(bool useAttributes)
        {
            var modelBuilder = CreateModelBuilder(c => c.Conventions.Replace(
                s => new PropertyDiscoveryConvention(
                    s.GetService<ProviderConventionSetBuilderDependencies>()!, useAttributes)));
            modelBuilder.Entity<SqlVariantEntity>();

            if (useAttributes)
            {
                var model = modelBuilder.FinalizeModel();
                var entityType = model.FindEntityType(typeof(SqlVariantEntity))!;

                Assert.Equal([nameof(SqlVariantEntity.Id), nameof(SqlVariantEntity.Value),],
                    entityType.GetProperties().Select(p => p.Name));
            }
            else
            {
                Assert.Equal(CoreStrings.PropertyNotAdded(nameof(SqlVariantEntity), nameof(SqlVariantEntity.Value), "object"),
                    Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
            }
        }

        protected class SqlVariantEntity
        {
            public int Id { get; set; }
            [Column(TypeName = "sql_variant")]
            public object? Value { get; set; }
        }
    }

    public abstract class SqlServerComplexType(SqlServerModelBuilderFixture fixture) : RelationalComplexTypeTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>;

    public abstract class SqlServerInheritance(SqlServerModelBuilderFixture fixture) : RelationalInheritanceTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>
    {
        [ConditionalFact] // #7240
        public void Can_use_shadow_FK_that_collides_with_convention_shadow_FK_on_other_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Child>();
            modelBuilder.Entity<Parent>()
                .HasOne(p => p.A)
                .WithOne()
                .HasForeignKey<DisjointChildSubclass1>("ParentId");

            var model = modelBuilder.FinalizeModel();

            var property1 = model.FindEntityType(typeof(DisjointChildSubclass1))!.FindProperty("ParentId")!;
            Assert.True(property1.IsForeignKey());
            Assert.Equal("ParentId", property1.GetColumnName());
            var property2 = model.FindEntityType(typeof(DisjointChildSubclass2))!.FindProperty("ParentId")!;
            Assert.True(property2.IsForeignKey());
            Assert.Equal("ParentId", property2.GetColumnName());
            Assert.Equal("DisjointChildSubclass2_ParentId", property2.GetColumnName(StoreObjectIdentifier.Table(nameof(Child))));
        }

        [ConditionalFact]
        public void Inherited_clr_properties_are_mapped_to_the_same_column()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<ChildBase>();
            modelBuilder.Ignore<Child>();
            modelBuilder.Entity<DisjointChildSubclass1>();
            modelBuilder.Entity<DisjointChildSubclass2>();

            var model = modelBuilder.FinalizeModel();

            var property1 = model.FindEntityType(typeof(DisjointChildSubclass1))!.FindProperty(nameof(Child.Name))!;
            Assert.Equal(nameof(Child.Name), property1.GetColumnName());
            var property2 = model.FindEntityType(typeof(DisjointChildSubclass2))!.FindProperty(nameof(Child.Name))!;
            Assert.Equal(nameof(Child.Name), property2.GetColumnName());
        }

        [ConditionalFact] //Issue#10659
        public void Index_convention_run_for_fk_when_derived_type_discovered_before_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Order>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Entity<DetailsBase>();

            var index = modelBuilder.Model.FindEntityType(typeof(CustomerDetails))!.GetIndexes().Single();

            Assert.Equal("[CustomerId] IS NOT NULL", index.GetFilter());
        }

        [ConditionalFact]
        public void Index_convention_sets_filter_for_unique_index_when_base_type_changed()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Entity<CustomerDetails>()
                .HasIndex(e => e.CustomerId)
                .IsUnique();

            modelBuilder.Entity<DetailsBase>();

            var index = modelBuilder.Model.FindEntityType(typeof(CustomerDetails))!.GetIndexes().Single();

            Assert.Equal("[CustomerId] IS NOT NULL", index.GetFilter());

            modelBuilder.Ignore<DetailsBase>();

            Assert.Null(index.GetFilter());
        }

        [ConditionalFact]
        public virtual void Can_override_TPC_with_TPH()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<P>();
            modelBuilder.Entity<T>();
            modelBuilder.Entity<Q>();
            modelBuilder.Entity<PBase>()
                .UseTpcMappingStrategy()
                .UseTphMappingStrategy();

            var model = modelBuilder.FinalizeModel();

            Assert.Equal("Discriminator", model.FindEntityType(typeof(PBase))!.GetDiscriminatorPropertyName());
            Assert.Equal(nameof(PBase), model.FindEntityType(typeof(PBase))!.GetDiscriminatorValue());
            Assert.Equal(nameof(P), model.FindEntityType(typeof(P))!.GetDiscriminatorValue());
            Assert.Equal(nameof(Q), model.FindEntityType(typeof(Q))!.GetDiscriminatorValue());
        }

        [ConditionalFact]
        public virtual void TPT_identifying_FK_is_created_only_on_declaring_table()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<BigMak>()
                .Ignore(b => b.Bun)
                .Ignore(b => b.Pickles);
            modelBuilder.Entity<Ingredient>(
                b =>
                {
                    b.ToTable("Ingredients");
                    b.Ignore(i => i.BigMak);
                });
            modelBuilder.Entity<Bun>(
                b =>
                {
                    b.ToTable("Buns");
                    b.HasOne(i => i.BigMak).WithOne().HasForeignKey<Bun>(i => i.Id);
                });
            modelBuilder.Entity<SesameBun>(
                b =>
                {
                    b.ToTable("SesameBuns");
                });

            var model = modelBuilder.FinalizeModel();

            var principalType = model.FindEntityType(typeof(BigMak))!;
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Empty(principalType.GetIndexes());
            Assert.Null(principalType.FindDiscriminatorProperty());

            var ingredientType = model.FindEntityType(typeof(Ingredient))!;

            var bunType = model.FindEntityType(typeof(Bun))!;
            Assert.Empty(bunType.GetIndexes());
            Assert.Null(bunType.FindDiscriminatorProperty());
            var bunFk = bunType.GetDeclaredForeignKeys().Single(fk => !fk.IsBaseLinking());
            Assert.Equal("FK_Buns_BigMak_Id", bunFk.GetConstraintName());
            Assert.Equal(
                "FK_Buns_BigMak_Id", bunFk.GetConstraintName(
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table)!.Value,
                    StoreObjectIdentifier.Create(principalType, StoreObjectType.Table)!.Value));
            Assert.Single(bunFk.GetMappedConstraints());

            var bunLinkingFk = bunType.GetDeclaredForeignKeys().Single(fk => fk.IsBaseLinking());
            Assert.Equal("FK_Buns_Ingredients_Id", bunLinkingFk.GetConstraintName());
            Assert.Equal(
                "FK_Buns_Ingredients_Id", bunLinkingFk.GetConstraintName(
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table)!.Value,
                    StoreObjectIdentifier.Create(ingredientType, StoreObjectType.Table)!.Value));
            Assert.Single(bunLinkingFk.GetMappedConstraints());

            var sesameBunType = model.FindEntityType(typeof(SesameBun))!;
            Assert.Empty(sesameBunType.GetIndexes());
            var sesameBunFk = sesameBunType.GetDeclaredForeignKeys().Single();
            Assert.True(sesameBunFk.IsBaseLinking());
            Assert.Equal("FK_SesameBuns_Buns_Id", sesameBunFk.GetConstraintName());
            Assert.Equal(
                "FK_SesameBuns_Buns_Id", sesameBunFk.GetConstraintName(
                    StoreObjectIdentifier.Create(sesameBunType, StoreObjectType.Table)!.Value,
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table)!.Value));
            Assert.Single(sesameBunFk.GetMappedConstraints());
        }

        [ConditionalFact]
        public virtual void TPC_identifying_FKs_are_created_on_all_tables()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<BigMak>()
                .Ignore(b => b.Bun)
                .Ignore(b => b.Pickles);
            modelBuilder.Entity<Ingredient>(
                b =>
                {
                    b.ToTable("Ingredients");
                    b.Ignore(i => i.BigMak);
                    b.HasIndex(e => e.BurgerId);
                    b.UseTpcMappingStrategy();
                });
            modelBuilder.Entity<Bun>(
                b =>
                {
                    b.ToTable("Buns");
                    b.HasOne(i => i.BigMak).WithOne().HasForeignKey<Bun>(i => i.Id);
                    b.UseTpcMappingStrategy();
                });
            modelBuilder.Entity<SesameBun>(
                b =>
                {
                    b.ToTable("SesameBuns");
                });

            var model = modelBuilder.FinalizeModel();

            var principalType = model.FindEntityType(typeof(BigMak))!;
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Empty(principalType.GetIndexes());
            Assert.Null(principalType.FindDiscriminatorProperty());

            var bunType = model.FindEntityType(typeof(Bun))!;
            Assert.Empty(bunType.GetDeclaredIndexes());
            Assert.Null(bunType.FindDiscriminatorProperty());
            var bunFk = bunType.GetDeclaredForeignKeys().Single();
            Assert.Equal("FK_Buns_BigMak_Id", bunFk.GetConstraintName());
            Assert.Equal(
                "FK_Buns_BigMak_Id", bunFk.GetConstraintName(
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table)!.Value,
                    StoreObjectIdentifier.Create(principalType, StoreObjectType.Table)!.Value));
            Assert.Equal(2, bunFk.GetMappedConstraints().Count());

            Assert.Empty(bunType.GetDeclaredForeignKeys().Where(fk => fk.IsBaseLinking()));

            var sesameBunType = model.FindEntityType(typeof(SesameBun))!;
            Assert.Empty(sesameBunType.GetDeclaredIndexes());
            Assert.Empty(sesameBunType.GetDeclaredForeignKeys());
            Assert.Equal(
                "FK_SesameBuns_BigMak_Id", bunFk.GetConstraintName(
                    StoreObjectIdentifier.Create(sesameBunType, StoreObjectType.Table)!.Value,
                    StoreObjectIdentifier.Create(principalType, StoreObjectType.Table)!.Value));

            var ingredientType = model.FindEntityType(typeof(Ingredient))!;
            var ingredientIndex = ingredientType.GetDeclaredIndexes().Single();
            Assert.Equal("IX_Ingredients_BurgerId", ingredientIndex.GetDatabaseName());
            Assert.Equal(
                "IX_SesameBuns_BurgerId",
                ingredientIndex.GetDatabaseName(StoreObjectIdentifier.Create(sesameBunType, StoreObjectType.Table)!.Value));
            Assert.Equal(
                "IX_Buns_BurgerId",
                ingredientIndex.GetDatabaseName(StoreObjectIdentifier.Create(bunType, StoreObjectType.Table)!.Value));
        }

        [ConditionalFact]
        public virtual void TPT_index_can_use_inherited_properties()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<BigMak>()
                .Ignore(b => b.Bun)
                .Ignore(b => b.Pickles);
            modelBuilder.Entity<Ingredient>(
                b =>
                {
                    b.ToTable("Ingredients");
                    b.Property<int?>("NullableProp");
                    b.Ignore(i => i.BigMak);
                });
            modelBuilder.Entity<Bun>(
                b =>
                {
                    b.ToTable("Buns");
                    b.HasIndex(bun => bun.BurgerId);
                    b.HasIndex("NullableProp");
                    b.HasOne(i => i.BigMak).WithOne().HasForeignKey<Bun>(i => i.Id);
                });

            var model = modelBuilder.FinalizeModel();

            var bunType = model.FindEntityType(typeof(Bun))!;
            Assert.All(bunType.GetIndexes(), i => Assert.Null(i.GetFilter()));
        }

        [ConditionalFact]
        public void Can_add_check_constraints()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Child>()
                .HasBaseType(null)
                .ToTable(tb => tb.HasCheckConstraint("CK_ChildBase_LargeId", "Id > 1000").HasName("CK_LargeId"));
            modelBuilder.Entity<ChildBase>()
                .ToTable(
                    tb =>
                    {
                        tb.HasCheckConstraint("PositiveId", "Id > 0");
                        tb.HasCheckConstraint("CK_ChildBase_LargeId", "Id > 1000");
                    });
            modelBuilder.Entity<Child>()
                .HasBaseType<ChildBase>();
            modelBuilder.Entity<DisjointChildSubclass1>();

            var model = modelBuilder.FinalizeModel();

            var @base = model.FindEntityType(typeof(ChildBase))!;
            Assert.Equal(2, @base.GetCheckConstraints().Count());

            var firstCheckConstraint = @base.FindCheckConstraint("PositiveId")!;
            Assert.Equal("PositiveId", firstCheckConstraint.ModelName);
            Assert.Equal("Id > 0", firstCheckConstraint.Sql);
            Assert.Equal("PositiveId", firstCheckConstraint.Name);

            var secondCheckConstraint = @base.FindCheckConstraint("CK_ChildBase_LargeId")!;
            Assert.Equal("CK_ChildBase_LargeId", secondCheckConstraint.ModelName);
            Assert.Equal("Id > 1000", secondCheckConstraint.Sql);
            Assert.Equal("CK_LargeId", secondCheckConstraint.Name);

            var child = model.FindEntityType(typeof(Child))!;
            Assert.Equal(@base.GetCheckConstraints(), child.GetCheckConstraints());
            Assert.Empty(child.GetDeclaredCheckConstraints());
        }

        [ConditionalFact]
        public void Adding_conflicting_check_constraint_to_derived_type_throws()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<ChildBase>()
                .ToTable(tb => tb.HasCheckConstraint("LargeId", "Id > 100").HasName("CK_LargeId"));

            Assert.Equal(
                RelationalStrings.DuplicateCheckConstraint("LargeId", nameof(Child), nameof(ChildBase)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Child>().ToTable(tb => tb.HasCheckConstraint("LargeId", "Id > 1000"))).Message);
        }

        [ConditionalFact]
        public void Adding_conflicting_check_constraint_to_derived_type_before_base_throws()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Child>()
                .HasBaseType(null)
                .ToTable(tb => tb.HasCheckConstraint("LargeId", "Id > 1000"));
            modelBuilder.Entity<ChildBase>()
                .ToTable(tb => tb.HasCheckConstraint("LargeId", "Id > 100").HasName("CK_LargeId"));

            Assert.Equal(
                RelationalStrings.DuplicateCheckConstraint("LargeId", nameof(Child), nameof(ChildBase)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Child>().HasBaseType<ChildBase>()).Message);
        }

        protected class Parent
        {
            public int Id { get; set; }
            public DisjointChildSubclass1? A { get; set; }
            public IList<DisjointChildSubclass2>? B { get; set; }
        }

        protected abstract class ChildBase
        {
            public int Id { get; set; }
        }

        protected abstract class Child : ChildBase
        {
            public string? Name { get; set; }
        }

        protected class DisjointChildSubclass1 : Child;

        protected class DisjointChildSubclass2 : Child;
    }

    public abstract class SqlServerOneToMany(SqlServerModelBuilderFixture fixture) : RelationalOneToManyTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>
    {
        [ConditionalFact]
        public virtual void Shadow_foreign_keys_to_generic_types_have_terrible_names_that_should_not_change()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EventBase>().ToTable("Events");
            modelBuilder.Entity<Activity<Company>>().ToTable("CompanyActivities");
            modelBuilder.Entity<Activity<User>>().ToTable("UserActivities");

            var model = modelBuilder.FinalizeModel();

            var companyActivityEventType = model.FindEntityType(typeof(ActivityEvent<Company>))!;
            var eventTable = StoreObjectIdentifier.Create(companyActivityEventType, StoreObjectType.Table)!.Value;
            var companyActivityEventFk = companyActivityEventType.GetForeignKeys().Single();
            var companyActivityEventFkProperty = companyActivityEventFk.Properties.Single();
            Assert.Equal("Activity<Company>Id", companyActivityEventFkProperty.GetColumnName(eventTable));
            Assert.Equal("FK_Events_CompanyActivities_Activity<Company>Id", companyActivityEventFk.GetConstraintName());
            Assert.Equal(
                "FK_Events_CompanyActivities_Activity<Company>Id", companyActivityEventFk.GetConstraintName(
                    eventTable,
                    StoreObjectIdentifier.Create(companyActivityEventFk.PrincipalEntityType, StoreObjectType.Table)!.Value));

            var userActivityEventType = model.FindEntityType(typeof(ActivityEvent<User>))!;
            var userActivityEventFk = userActivityEventType.GetForeignKeys().Single();
            var userActivityEventFkProperty = userActivityEventFk.Properties.Single();
            Assert.Equal("Activity<User>Id", userActivityEventFkProperty.GetColumnName(eventTable));
            Assert.Equal("FK_Events_UserActivities_Activity<User>Id", userActivityEventFk.GetConstraintName());
            Assert.Equal(
                "FK_Events_UserActivities_Activity<User>Id", userActivityEventFk.GetConstraintName(
                    eventTable,
                    StoreObjectIdentifier.Create(userActivityEventFk.PrincipalEntityType, StoreObjectType.Table)!.Value));
        }

        protected abstract class EventBase
        {
            public string? Id { get; set; }
        }

        protected class Activity<T>
        {
            public string? Id { get; set; }
            public virtual List<ActivityEvent<T>> Events { get; } = null!;
        }

        protected class ActivityEvent<TTarget> : EventBase;

        protected class Company;

        protected class User;
    }

    public abstract class SqlServerManyToOne(SqlServerModelBuilderFixture fixture) : RelationalManyToOneTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>;

    public abstract class SqlServerOneToOne(SqlServerModelBuilderFixture fixture) : RelationalOneToOneTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>;

    public abstract class SqlServerManyToMany(SqlServerModelBuilderFixture fixture) : RelationalManyToManyTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>
    {
        [ConditionalFact]
        public virtual void Join_entity_type_uses_same_schema()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Category>().ToTable("Category", "mySchema").Ignore(c => c.ProductCategories);
            modelBuilder.Entity<Product>().ToTable("Product", "mySchema");
            modelBuilder.Entity<CategoryBase>();

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;
            var productCategoryType = categoriesFk.DeclaringEntityType;

            Assert.Equal(typeof(Dictionary<string, object>), productCategoryType.ClrType);
            Assert.Equal("mySchema", productCategoryType.GetSchema());
            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Join_entity_type_uses_default_schema_if_related_are_different()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Category>().ToTable("Category").Ignore(c => c.ProductCategories);
            modelBuilder.Entity<Product>().ToTable("Product", "dbo");
            modelBuilder.Entity<CategoryBase>();

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;
            var productCategoryType = categoriesFk.DeclaringEntityType;

            Assert.Equal(typeof(Dictionary<string, object>), productCategoryType.ClrType);
            Assert.Null(productCategoryType.GetSchema());
            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
        }
    }

    public abstract class SqlServerOwnedTypes(SqlServerModelBuilderFixture fixture) : RelationalOwnedTypesTestBase(fixture), IClassFixture<SqlServerModelBuilderFixture>
    {
        [ConditionalFact]
        public virtual void Owned_types_use_table_splitting_by_default()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.AlternateLabel,
                b =>
                {
                    b.Ignore(l => l.Book);
                    b.OwnsOne(
                        l => l.AnotherBookLabel,
                        ab =>
                        {
                            ab.Property(l => l.BookId).HasColumnName("BookId2");
                            ab.Ignore(l => l.Book);
                            ab.OwnsOne(
                                s => s.SpecialBookLabel,
                                s =>
                                {
                                    s.Property(l => l.BookId).HasColumnName("BookId2");
                                    s.Ignore(l => l.Book);
                                    s.Ignore(l => l.BookLabel);
                                });
                        });
                });

            modelBuilder.Entity<Book>().OwnsOne(b => b.Label)
                .Ignore(l => l.Book)
                .OwnsOne(l => l.SpecialBookLabel)
                .Ignore(l => l.Book)
                .OwnsOne(a => a.AnotherBookLabel)
                .Ignore(l => l.Book);

            modelBuilder.Entity<Book>().OwnsOne(b => b.Label)
                .OwnsOne(l => l.AnotherBookLabel)
                .Ignore(l => l.Book)
                .OwnsOne(a => a.SpecialBookLabel)
                .Ignore(l => l.Book)
                .Ignore(l => l.BookLabel);

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.AlternateLabel,
                b =>
                {
                    b.Ignore(l => l.Book);
                    b.OwnsOne(
                        l => l.SpecialBookLabel,
                        ab =>
                        {
                            ab.Property(l => l.BookId).HasColumnName("BookId2");
                            ab.Ignore(l => l.Book);
                            ab.OwnsOne(
                                s => s.AnotherBookLabel,
                                s =>
                                {
                                    s.Property(l => l.BookId).HasColumnName("BookId2");
                                    s.Ignore(l => l.Book);
                                });
                        });
                });

            var model = (IModel)modelBuilder.Model;
            var book = model.FindEntityType(typeof(Book))!;
            var bookOwnership1 = book.FindNavigation(nameof(Book.Label))!.ForeignKey;
            var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel))!.ForeignKey;
            var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))!.ForeignKey;
            var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!.ForeignKey;
            var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))!.ForeignKey;
            var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!.ForeignKey;

            Assert.Equal(book.GetTableName(), bookOwnership1.DeclaringEntityType.GetTableName());
            Assert.Equal(book.GetTableName(), bookOwnership2.DeclaringEntityType.GetTableName());
            Assert.Equal(book.GetTableName(), bookLabel1Ownership1.DeclaringEntityType.GetTableName());
            Assert.Equal(book.GetTableName(), bookLabel1Ownership2.DeclaringEntityType.GetTableName());
            Assert.Equal(book.GetTableName(), bookLabel2Ownership1.DeclaringEntityType.GetTableName());
            Assert.Equal(book.GetTableName(), bookLabel2Ownership2.DeclaringEntityType.GetTableName());

            Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
            Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());

            Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
            Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
            Assert.Single(bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys());

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
            Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));

            Assert.Null(
                bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))!
                    .GetColumnName(StoreObjectIdentifier.Table("Label")));
            Assert.Null(
                bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))!
                    .GetColumnName(StoreObjectIdentifier.Table("AlternateLabel")));

            modelBuilder.Entity<Book>().OwnsOne(b => b.Label).ToTable("Label");
            modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).ToTable("AlternateLabel");

            model = modelBuilder.FinalizeModel();

            Assert.Equal(
                nameof(BookLabel.Id),
                bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))!
                    .GetColumnName(StoreObjectIdentifier.Table("Label")));
            Assert.Equal(
                nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))!
                    .GetColumnName(StoreObjectIdentifier.Table("AlternateLabel")));

            var alternateTable = model.GetRelationalModel().FindTable("AlternateLabel", null)!;
            var bookId = alternateTable.FindColumn("BookId2")!;

            Assert.Equal(4, bookId.PropertyMappings.Count());
            Assert.All(bookId.PropertyMappings, m => Assert.Equal(ValueGenerated.OnUpdateSometimes, m.Property.ValueGenerated));
        }

        [ConditionalFact]
        public virtual void Owned_types_can_be_mapped_to_different_tables()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Book>(
                bb =>
                {
                    bb.ToTable(
                        "BT", "BS", t =>
                        {
                            t.ExcludeFromMigrations();

                            Assert.Equal("BT", t.Name);
                            Assert.Equal("BS", t.Schema);
                        });
                    bb.OwnsOne(
                        b => b.AlternateLabel, tb =>
                        {
                            tb.Ignore(l => l.Book);
                            tb.WithOwner()
                                .HasConstraintName("AlternateLabelFK");
                            tb.ToTable("TT", "TS", tb => tb.IsMemoryOptimized());
                            tb.OwnsOne(
                                l => l.AnotherBookLabel, ab =>
                                {
                                    ab.Ignore(l => l.Book);
                                    ab.ToTable(
                                        "AT1", "AS1", t =>
                                        {
                                            t.ExcludeFromMigrations(false);

                                            Assert.Equal("AT1", t.Name);
                                            Assert.Equal("AS1", t.Schema);
                                        });
                                    ab.OwnsOne(s => s.SpecialBookLabel)
                                        .ToTable("ST11", "SS11")
                                        .Ignore(l => l.Book)
                                        .Ignore(l => l.BookLabel);

                                    ab.OwnedEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!
                                        .AddAnnotation("Foo", "Bar");
                                });
                            tb.OwnsOne(
                                l => l.SpecialBookLabel, sb =>
                                {
                                    sb.Ignore(l => l.Book);
                                    sb.ToTable("ST2", "SS2");
                                    sb.OwnsOne(s => s.AnotherBookLabel)
                                        .ToTable("AT21", "AS21")
                                        .Ignore(l => l.Book);
                                });
                        });
                    bb.OwnsOne(
                        b => b.Label, lb =>
                        {
                            lb.Ignore(l => l.Book);
                            lb.ToTable("LT", "LS");
                            lb.OwnsOne(
                                l => l.SpecialBookLabel, sb =>
                                {
                                    sb.Ignore(l => l.Book);
                                    sb.ToTable("ST1", "SS1");
                                    sb.OwnsOne(a => a.AnotherBookLabel)
                                        .ToTable("AT11", "AS11")
                                        .Ignore(l => l.Book);
                                });
                            lb.OwnsOne(
                                l => l.AnotherBookLabel, ab =>
                                {
                                    ab.Ignore(l => l.Book);
                                    ab.ToTable("AT2", "AS2");
                                    ab.OwnsOne(a => a.SpecialBookLabel)
                                        .ToTable("ST21", "SS21")
                                        .Ignore(l => l.BookLabel)
                                        .Ignore(l => l.Book);
                                });
                        });
                });

            modelBuilder.FinalizeModel();

            var book = model.FindEntityType(typeof(Book))!;
            var bookOwnership1 = book.FindNavigation(nameof(Book.Label))!.ForeignKey;
            var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel))!.ForeignKey;
            var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))!.ForeignKey;
            var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!.ForeignKey;
            var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))!.ForeignKey;
            var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!.ForeignKey;
            var bookLabel1Ownership11 = bookLabel1Ownership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!
                .ForeignKey;
            var bookLabel1Ownership21 = bookLabel1Ownership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))!
                .ForeignKey;
            var bookLabel2Ownership11 = bookLabel2Ownership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))!
                .ForeignKey;
            var bookLabel2Ownership21 = bookLabel2Ownership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))!
                .ForeignKey;

            Assert.Equal("AlternateLabelFK", bookOwnership2.GetConstraintName());

            Assert.Equal("BS", book.GetSchema());
            Assert.Equal("BT", book.GetTableName());
            Assert.True(book.IsTableExcludedFromMigrations());
            Assert.Equal("LS", bookOwnership1.DeclaringEntityType.GetSchema());
            Assert.Equal("LT", bookOwnership1.DeclaringEntityType.GetTableName());
            Assert.False(bookOwnership1.DeclaringEntityType.IsMemoryOptimized());
            Assert.True(bookOwnership1.DeclaringEntityType.IsTableExcludedFromMigrations());
            Assert.Equal("TS", bookOwnership2.DeclaringEntityType.GetSchema());
            Assert.Equal("TT", bookOwnership2.DeclaringEntityType.GetTableName());
            Assert.True(bookOwnership2.DeclaringEntityType.IsMemoryOptimized());
            Assert.True(bookOwnership2.DeclaringEntityType.IsTableExcludedFromMigrations());
            Assert.Equal("AS2", bookLabel1Ownership1.DeclaringEntityType.GetSchema());
            Assert.Equal("AT2", bookLabel1Ownership1.DeclaringEntityType.GetTableName());
            Assert.Equal("SS1", bookLabel1Ownership2.DeclaringEntityType.GetSchema());
            Assert.Equal("ST1", bookLabel1Ownership2.DeclaringEntityType.GetTableName());
            Assert.Equal("AS1", bookLabel2Ownership1.DeclaringEntityType.GetSchema());
            Assert.Equal("AT1", bookLabel2Ownership1.DeclaringEntityType.GetTableName());
            Assert.False(bookLabel2Ownership1.DeclaringEntityType.IsTableExcludedFromMigrations());
            Assert.Equal("SS2", bookLabel2Ownership2.DeclaringEntityType.GetSchema());
            Assert.Equal("ST2", bookLabel2Ownership2.DeclaringEntityType.GetTableName());
            Assert.Equal("SS21", bookLabel1Ownership11.DeclaringEntityType.GetSchema());
            Assert.Equal("ST21", bookLabel1Ownership11.DeclaringEntityType.GetTableName());
            Assert.Equal("AS11", bookLabel1Ownership21.DeclaringEntityType.GetSchema());
            Assert.Equal("AT11", bookLabel1Ownership21.DeclaringEntityType.GetTableName());
            Assert.Equal("SS11", bookLabel2Ownership11.DeclaringEntityType.GetSchema());
            Assert.Equal("ST11", bookLabel2Ownership11.DeclaringEntityType.GetTableName());
            Assert.Equal("AS21", bookLabel2Ownership21.DeclaringEntityType.GetSchema());
            Assert.Equal("AT21", bookLabel2Ownership21.DeclaringEntityType.GetTableName());

            Assert.Equal("Bar", bookLabel2Ownership11.PrincipalToDependent?["Foo"]);

            Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
            Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookOwnership2.DeclaringEntityType.GetForeignKeys());

            Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
            Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
            Assert.Single(bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys());

            Assert.NotSame(bookLabel1Ownership11.DeclaringEntityType, bookLabel2Ownership11.DeclaringEntityType);
            Assert.NotSame(bookLabel1Ownership21.DeclaringEntityType, bookLabel2Ownership21.DeclaringEntityType);
            Assert.Single(bookLabel1Ownership11.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel1Ownership21.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership11.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership21.DeclaringEntityType.GetForeignKeys());

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
            Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));

            Assert.Equal(ValueGenerated.Never, bookOwnership1.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(ValueGenerated.Never, bookOwnership2.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);

            Assert.Equal(
                ValueGenerated.Never, bookLabel1Ownership1.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(
                ValueGenerated.Never, bookLabel1Ownership2.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(
                ValueGenerated.Never, bookLabel2Ownership1.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(
                ValueGenerated.Never, bookLabel2Ownership2.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);

            Assert.Equal(
                ValueGenerated.Never, bookLabel1Ownership11.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(
                ValueGenerated.Never, bookLabel1Ownership21.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(
                ValueGenerated.Never, bookLabel2Ownership11.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
            Assert.Equal(
                ValueGenerated.Never, bookLabel2Ownership21.DeclaringEntityType.FindPrimaryKey()!.Properties.Single().ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Owned_type_collections_can_be_mapped_to_different_tables()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.HasKey(o => o.OrderId);
                    r.ToTable(tb => tb.IsMemoryOptimized());
                    r.Ignore(o => o.OrderCombination);
                    r.Ignore(o => o.Details);
                });

            var ownership = model.FindEntityType(typeof(Customer))!.FindNavigation(nameof(Customer.Orders))!.ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal?.Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ownership.GetConstraintName());

            Assert.Single(owned.GetForeignKeys());
            Assert.Single(owned.GetIndexes());
            Assert.Equal(
                new[] { nameof(Order.OrderId), nameof(Order.AnotherCustomerId), nameof(Order.CustomerId) },
                owned.GetProperties().Select(p => p.GetColumnName()));
            Assert.Equal(nameof(Order), owned.GetTableName());
            Assert.Null(owned.GetSchema());
            Assert.True(owned.IsMemoryOptimized());

            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.WithOwner(o => o.Customer).HasConstraintName("Owned");
                    r.ToTable("bar", "foo");
                });

            Assert.Equal("bar", owned.GetTableName());
            Assert.Equal("foo", owned.GetSchema());
            Assert.Equal("Owned", ownership.GetConstraintName());

            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r => r.ToTable("blah"));

            modelBuilder.FinalizeModel();

            Assert.Equal("blah", owned.GetTableName());
            Assert.Null(owned.GetSchema());
        }

        [ConditionalFact]
        public virtual void Owned_type_collections_can_be_mapped_to_a_view()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.HasKey(o => o.OrderId);
                    r.Ignore(o => o.OrderCombination);
                    r.Ignore(o => o.Details);
                    r.ToView("bar", "foo");
                });

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(Customer))!;
            var ownership = owner.FindNavigation(nameof(Customer.Orders))!.ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal?.Name);
            Assert.Empty(ownership.GetMappedConstraints());

            Assert.Equal(nameof(Customer), owner.GetTableName());
            Assert.Null(owner.GetSchema());

            Assert.Null(owned.GetForeignKeys().Single().GetConstraintName());
            Assert.Single(owned.GetIndexes());
            Assert.Null(owned.FindPrimaryKey()!.GetName());
            Assert.Equal(
                new[] { nameof(Order.OrderId), nameof(Order.AnotherCustomerId), nameof(Order.CustomerId) },
                owned.GetProperties().Select(p => p.GetColumnName()));
            Assert.Null(owned.GetTableName());
            Assert.Null(owned.GetSchema());
            Assert.Equal("bar", owned.GetViewName());
            Assert.Equal("foo", owned.GetViewSchema());
        }

        [ConditionalFact]
        public virtual void Owner_can_be_mapped_to_a_view()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r =>
                    {
                        r.HasKey(o => o.OrderId);
                        r.Ignore(o => o.OrderCombination);
                        r.Ignore(o => o.Details);
                    })
                .ToView("bar", "foo");

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(Customer))!;
            var ownership = owner.FindNavigation(nameof(Customer.Orders))!.ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal?.Name);
            Assert.Empty(ownership.GetMappedConstraints());

            Assert.Null(owner.GetTableName());
            Assert.Null(owner.GetSchema());
            Assert.Equal("bar", owner.GetViewName());
            Assert.Equal("foo", owner.GetViewSchema());

            Assert.Null(owned.GetForeignKeys().Single().GetConstraintName());
            Assert.Equal("IX_Order_CustomerId", owned.GetIndexes().Single().GetDatabaseName());
            Assert.Equal("PK_Order", owned.FindPrimaryKey()!.GetName());
            Assert.Equal(
                new[] { nameof(Order.OrderId), nameof(Order.AnotherCustomerId), nameof(Order.CustomerId) },
                owned.GetProperties().Select(p => p.GetColumnName()));
            Assert.Equal(nameof(Order), owned.GetTableName());
            Assert.Null(owned.GetSchema());
        }

        [ConditionalFact]
        public virtual void Temporal_table_default_settings()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().ToTable(
                tb =>
                {
                    tb.IsTemporal();
                    Assert.Null(tb.Name);
                    Assert.Null(tb.Schema);
                });
            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.True(entity.IsTemporal());
            Assert.Equal("CustomerHistory", entity.GetHistoryTableName());
            Assert.Null(entity.GetHistoryTableSchema());

            var periodStart = entity.GetProperty(entity.GetPeriodStartPropertyName()!);
            var periodEnd = entity.GetProperty(entity.GetPeriodEndPropertyName()!);

            Assert.Equal("PeriodStart", periodStart.Name);
            Assert.True(periodStart.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodStart.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

            Assert.Equal("PeriodEnd", periodEnd.Name);
            Assert.True(periodEnd.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodEnd.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Temporal_table_with_history_table_configuration()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("HistoryTable", "historySchema");
                        ttb.HasPeriodStart("MyPeriodStart").HasColumnName("PeriodStartColumn");
                        ttb.HasPeriodEnd("MyPeriodEnd").HasColumnName("PeriodEndColumn");
                    }));

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.True(entity.IsTemporal());
            Assert.Equal(6, entity.GetProperties().Count());

            Assert.Equal("HistoryTable", entity.GetHistoryTableName());
            Assert.Equal("historySchema", entity.GetHistoryTableSchema());

            var periodStart = entity.GetProperty(entity.GetPeriodStartPropertyName()!);
            var periodEnd = entity.GetProperty(entity.GetPeriodEndPropertyName()!);

            Assert.Equal("MyPeriodStart", periodStart.Name);
            Assert.Equal("PeriodStartColumn", periodStart[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodStart.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodStart.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

            Assert.Equal("MyPeriodEnd", periodEnd.Name);
            Assert.Equal("PeriodEndColumn", periodEnd[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodEnd.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodEnd.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Temporal_table_with_changed_configuration()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("HistoryTable", "historySchema");
                        ttb.HasPeriodStart("MyPeriodStart");
                        ttb.HasPeriodEnd("MyPeriodEnd");
                    }));

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("ChangedHistoryTable", "changedHistorySchema");
                        ttb.HasPeriodStart("ChangedMyPeriodStart");
                        ttb.HasPeriodEnd("ChangedMyPeriodEnd");
                    }));

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.True(entity.IsTemporal());
            Assert.Equal(6, entity.GetProperties().Count());

            Assert.Equal("ChangedHistoryTable", entity.GetHistoryTableName());
            Assert.Equal("changedHistorySchema", entity.GetHistoryTableSchema());

            var periodStart = entity.GetProperty(entity.GetPeriodStartPropertyName()!);
            var periodEnd = entity.GetProperty(entity.GetPeriodEndPropertyName()!);

            Assert.Equal("ChangedMyPeriodStart", periodStart.Name);
            Assert.Equal("ChangedMyPeriodStart", periodStart[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodStart.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodStart.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

            Assert.Equal("ChangedMyPeriodEnd", periodEnd.Name);
            Assert.Equal("ChangedMyPeriodEnd", periodEnd[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodEnd.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodEnd.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Temporal_table_with_period_column_names_changed_configuration()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("HistoryTable", "historySchema");
                        ttb.HasPeriodStart("MyPeriodStart").HasColumnName("PeriodStartColumn");
                        ttb.HasPeriodEnd("MyPeriodEnd").HasColumnName("PeriodEndColumn");
                    }));

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("ChangedHistoryTable", "changedHistorySchema");
                        ttb.HasPeriodStart("MyPeriodStart").HasColumnName("ChangedPeriodStartColumn");
                        ttb.HasPeriodEnd("MyPeriodEnd").HasColumnName("ChangedPeriodEndColumn");
                    }));

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.True(entity.IsTemporal());
            Assert.Equal(6, entity.GetProperties().Count());

            Assert.Equal("ChangedHistoryTable", entity.GetHistoryTableName());
            Assert.Equal("changedHistorySchema", entity.GetHistoryTableSchema());

            var periodStart = entity.GetProperty(entity.GetPeriodStartPropertyName()!);
            var periodEnd = entity.GetProperty(entity.GetPeriodEndPropertyName()!);

            Assert.Equal("MyPeriodStart", periodStart.Name);
            Assert.Equal("ChangedPeriodStartColumn", periodStart[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodStart.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodStart.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

            Assert.Equal("MyPeriodEnd", periodEnd.Name);
            Assert.Equal("ChangedPeriodEndColumn", periodEnd[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodEnd.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodEnd.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Temporal_table_with_explicit_properties_mapped_to_the_period_columns()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("HistoryTable", schema: null);
                        ttb.HasPeriodStart("Start").HasColumnName("PeriodStartColumn");
                        ttb.HasPeriodEnd("End").HasColumnName("PeriodEndColumn");
                    }));

            modelBuilder.Entity<Customer>()
                .Property<DateTime>("Start")
                .HasColumnName("PeriodStartColumn")
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<Customer>()
                .Property<DateTime>("End")
                .HasColumnName("PeriodEndColumn")
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.True(entity.IsTemporal());
            Assert.Equal(6, entity.GetProperties().Count());

            Assert.Equal("HistoryTable", entity.GetHistoryTableName());

            var periodStart = entity.GetProperty(entity.GetPeriodStartPropertyName()!);
            var periodEnd = entity.GetProperty(entity.GetPeriodEndPropertyName()!);

            Assert.Equal("Start", periodStart.Name);
            Assert.Equal("PeriodStartColumn", periodStart[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodStart.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodStart.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

            Assert.Equal("End", periodEnd.Name);
            Assert.Equal("PeriodEndColumn", periodEnd[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodEnd.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodEnd.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void
            Temporal_table_with_explicit_properties_with_same_name_as_default_periods_but_different_periods_defined_explicity_as_well()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>()
                .Property<DateTime>("PeriodStart")
                .HasColumnName("PeriodStartColumn");

            modelBuilder.Entity<Customer>()
                .Property<DateTime>("PeriodEnd")
                .HasColumnName("PeriodEndColumn");

            modelBuilder.Entity<Customer>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("HistoryTable", schema: null);
                        ttb.HasPeriodStart("Start");
                        ttb.HasPeriodEnd("End");
                    }));

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.True(entity.IsTemporal());
            Assert.Equal(8, entity.GetProperties().Count());

            Assert.Equal("HistoryTable", entity.GetHistoryTableName());

            var periodStart = entity.GetProperty(entity.GetPeriodStartPropertyName()!);
            var periodEnd = entity.GetProperty(entity.GetPeriodEndPropertyName()!);

            Assert.Equal("Start", periodStart.Name);
            Assert.Equal("Start", periodStart[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodStart.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodStart.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

            Assert.Equal("End", periodEnd.Name);
            Assert.Equal("End", periodEnd[RelationalAnnotationNames.ColumnName]);
            Assert.True(periodEnd.IsShadowProperty());
            Assert.Equal(typeof(DateTime), periodEnd.ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);

            var propertyMappedToStart = entity.GetProperty("PeriodStart");
            Assert.Equal("PeriodStartColumn", propertyMappedToStart[RelationalAnnotationNames.ColumnName]);
            Assert.Equal(ValueGenerated.Never, propertyMappedToStart.ValueGenerated);

            var propertyMappedToEnd = entity.GetProperty("PeriodEnd");
            Assert.Equal("PeriodEndColumn", propertyMappedToEnd[RelationalAnnotationNames.ColumnName]);
            Assert.Equal(ValueGenerated.Never, propertyMappedToEnd.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Switching_from_temporal_to_non_temporal_default_settings()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(false));

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;
            Assert.False(entity.IsTemporal());
            Assert.Null(entity.GetPeriodStartPropertyName());
            Assert.Null(entity.GetPeriodEndPropertyName());
            Assert.Equal(4, entity.GetProperties().Count());
        }

        [ConditionalFact]
        public virtual void Implicit_many_to_many_converted_from_non_temporal_to_temporal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<ImplicitManyToManyA>();
            modelBuilder.Entity<ImplicitManyToManyB>();

            modelBuilder.Entity<ImplicitManyToManyA>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<ImplicitManyToManyB>().ToTable(tb => tb.IsTemporal());

            modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(ImplicitManyToManyA))!;
            var joinEntity = entity.GetSkipNavigations().Single().JoinEntityType!;

            Assert.True(joinEntity.IsTemporal());
        }

        [ConditionalFact]
        public virtual void Json_entity_and_normal_owned_can_exist_side_by_side_on_same_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<JsonEntity>(
                b =>
                {
                    b.OwnsOne(x => x.OwnedReference1);
                    b.OwnsOne(x => x.OwnedReference2, bb => bb.ToJson("reference"));
                    b.OwnsMany(x => x.OwnedCollection1);
                    b.OwnsMany(x => x.OwnedCollection2, bb => bb.ToJson("collection"));
                });

            var model = modelBuilder.FinalizeModel();
            var owner = model.FindEntityType(typeof(JsonEntity))!;
            Assert.False(owner.IsMappedToJson());
            Assert.True(owner.GetDeclaredProperties().All(x => x.GetJsonPropertyName() == null));

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(4, ownedEntities.Count());
            Assert.Equal(2, ownedEntities.Where(e => e.IsMappedToJson()).Count());
            Assert.Equal(2, ownedEntities.Where(e => e.IsOwned() && !e.IsMappedToJson()).Count());
            var reference = ownedEntities.Where(e => e.GetContainerColumnName() == "reference").Single();
            Assert.Equal("Date", reference.GetProperty("Date").GetJsonPropertyName());
            Assert.Equal("Fraction", reference.GetProperty("Fraction").GetJsonPropertyName());
            Assert.Equal("Enum", reference.GetProperty("Enum").GetJsonPropertyName());

            var collection = ownedEntities.Where(e => e.GetContainerColumnName() == "collection").Single();
            Assert.Equal("Date", collection.GetProperty("Date").GetJsonPropertyName());
            Assert.Equal("Fraction", collection.GetProperty("Fraction").GetJsonPropertyName());
            Assert.Equal("Enum", collection.GetProperty("Enum").GetJsonPropertyName());

            var nonJson = ownedEntities.Where(e => !e.IsMappedToJson()).ToList();
            Assert.True(nonJson.All(x => x.GetProperty("Date").GetJsonPropertyName() == null));
            Assert.True(nonJson.All(x => x.GetProperty("Fraction").GetJsonPropertyName() == null));
            Assert.True(nonJson.All(x => x.GetProperty("Enum").GetJsonPropertyName() == null));
        }

        [ConditionalFact]
        public virtual void Json_entity_with_tph_inheritance()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<JsonEntityInheritanceBase>(
                b =>
                {
                    b.OwnsOne(x => x.OwnedReferenceOnBase, bb => bb.ToJson("reference_on_base"));
                    b.OwnsMany(x => x.OwnedCollectionOnBase, bb => bb.ToJson("collection_on_base"));
                });

            modelBuilder.Entity<JsonEntityInheritanceDerived>(
                b =>
                {
                    b.HasBaseType<JsonEntityInheritanceBase>();
                    b.OwnsOne(x => x.OwnedReferenceOnDerived, bb => bb.ToJson("reference_on_derived"));
                    b.OwnsMany(x => x.OwnedCollectionOnDerived, bb => bb.ToJson("collection_on_derived"));
                });

            var model = modelBuilder.FinalizeModel();
            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity)).ToList();
            Assert.Equal(4, ownedEntities.Count());

            foreach (var ownedEntity in ownedEntities)
            {
                Assert.Equal("Date", ownedEntity.GetProperty("Date").GetJsonPropertyName());
                Assert.Equal("Fraction", ownedEntity.GetProperty("Fraction").GetJsonPropertyName());
                Assert.Equal("Enum", ownedEntity.GetProperty("Enum").GetJsonPropertyName());
            }

            var jsonColumnNames = ownedEntities.Select(x => x.GetContainerColumnName()).OrderBy(x => x).ToList();
            Assert.Equal("collection_on_base", jsonColumnNames[0]);
            Assert.Equal("collection_on_derived", jsonColumnNames[1]);
            Assert.Equal("reference_on_base", jsonColumnNames[2]);
            Assert.Equal("reference_on_derived", jsonColumnNames[3]);
        }

        [ConditionalFact]
        public virtual void Json_entity_with_nested_structure_same_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntityWithNesting>(
                b =>
                {
                    b.OwnsOne(
                        x => x.OwnedReference1, bb =>
                        {
                            bb.ToJson("ref1");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.OwnsOne(
                        x => x.OwnedReference2, bb =>
                        {
                            bb.ToJson("ref2");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.OwnsMany(
                        x => x.OwnedCollection1, bb =>
                        {
                            bb.ToJson("col1");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.OwnsMany(
                        x => x.OwnedCollection2, bb =>
                        {
                            bb.ToJson("col2");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });
                });

            var model = modelBuilder.FinalizeModel();
            var outerOwnedEntities = model.FindEntityTypes(typeof(OwnedEntityExtraLevel));
            Assert.Equal(4, outerOwnedEntities.Count());

            foreach (var outerOwnedEntity in outerOwnedEntities)
            {
                Assert.Equal("Date", outerOwnedEntity.GetProperty("Date").GetJsonPropertyName());
                Assert.Equal("Fraction", outerOwnedEntity.GetProperty("Fraction").GetJsonPropertyName());
                Assert.Equal("Enum", outerOwnedEntity.GetProperty("Enum").GetJsonPropertyName());
                Assert.Equal(
                    "Reference1",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Reference1").TargetEntityType.GetJsonPropertyName());
                Assert.Equal(
                    "Reference2",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Reference2").TargetEntityType.GetJsonPropertyName());
                Assert.Equal(
                    "Collection1",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Collection1").TargetEntityType.GetJsonPropertyName());
                Assert.Equal(
                    "Collection2",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Collection2").TargetEntityType.GetJsonPropertyName());
            }

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(16, ownedEntities.Count());

            foreach (var ownedEntity in ownedEntities)
            {
                Assert.Equal("Date", ownedEntity.GetProperty("Date").GetJsonPropertyName());
                Assert.Equal("Fraction", ownedEntity.GetProperty("Fraction").GetJsonPropertyName());
                Assert.Equal("Enum", ownedEntity.GetProperty("Enum").GetJsonPropertyName());
            }
        }

        [ConditionalFact]
        public virtual void Json_entity_nested_enums_have_conversions_to_int_by_default_ToJson_first()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntityWithNesting>(
                b =>
                {
                    b.OwnsOne(
                        x => x.OwnedReference1, bb =>
                        {
                            bb.ToJson();
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.Ignore(x => x.OwnedReference2);
                    b.OwnsMany(
                        x => x.OwnedCollection1, bb =>
                        {
                            bb.ToJson();
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.Ignore(x => x.OwnedCollection2);
                });

            var model = modelBuilder.FinalizeModel();
            var outerOwnedEntities = model.FindEntityTypes(typeof(OwnedEntityExtraLevel));
            Assert.Equal(2, outerOwnedEntities.Count());

            foreach (var outerOwnedEntity in outerOwnedEntities)
            {
                Assert.True(outerOwnedEntity.IsMappedToJson());
                var myEnum = outerOwnedEntity.GetDeclaredProperties().Where(p => p.ClrType.IsEnum).Single();
                var typeMapping = myEnum.FindRelationalTypeMapping()!;
                Assert.True(typeMapping.Converter is EnumToNumberConverter<MyJsonEnum, int>);
            }

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(8, ownedEntities.Count());

            foreach (var ownedEntity in ownedEntities)
            {
                Assert.True(ownedEntity.IsMappedToJson());
                var myEnum = ownedEntity.GetDeclaredProperties().Where(p => p.ClrType.IsEnum).Single();
                var typeMapping = myEnum.FindRelationalTypeMapping()!;
                Assert.True(typeMapping.Converter is EnumToNumberConverter<MyJsonEnum, int>);
            }
        }

        [ConditionalFact]
        public virtual void Json_entity_nested_enums_have_conversions_to_int_by_default_ToJson_last()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntityWithNesting>(
                b =>
                {
                    b.OwnsOne(
                        x => x.OwnedReference1, bb =>
                        {
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                            bb.ToJson();
                        });

                    b.Ignore(x => x.OwnedReference2);
                    b.OwnsMany(
                        x => x.OwnedCollection1, bb =>
                        {
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                            bb.ToJson();
                        });

                    b.Ignore(x => x.OwnedCollection2);
                });

            var model = modelBuilder.FinalizeModel();
            var outerOwnedEntities = model.FindEntityTypes(typeof(OwnedEntityExtraLevel));
            Assert.Equal(2, outerOwnedEntities.Count());

            foreach (var outerOwnedEntity in outerOwnedEntities)
            {
                Assert.True(outerOwnedEntity.IsMappedToJson());
                var myEnum = outerOwnedEntity.GetDeclaredProperties().Where(p => p.ClrType.IsEnum).Single();
                var typeMapping = myEnum.FindRelationalTypeMapping()!;
                Assert.True(typeMapping.Converter is EnumToNumberConverter<MyJsonEnum, int>);
            }

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(8, ownedEntities.Count());

            foreach (var ownedEntity in ownedEntities)
            {
                Assert.True(ownedEntity.IsMappedToJson());
                var myEnum = ownedEntity.GetDeclaredProperties().Where(p => p.ClrType.IsEnum).Single();
                var typeMapping = myEnum.FindRelationalTypeMapping()!;
                Assert.True(typeMapping.Converter is EnumToNumberConverter<MyJsonEnum, int>);
            }
        }

        [ConditionalFact]
        public virtual void Entity_mapped_to_json_and_unwound_afterwards_properly_cleans_up_its_state()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntityWithNesting>(
                b =>
                {
                    b.OwnsOne(
                        x => x.OwnedReference1, bb =>
                        {
                            bb.ToJson();
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                            bb.ToJson(null);
                        });

                    b.Ignore(x => x.OwnedReference2);
                    b.OwnsMany(
                        x => x.OwnedCollection1, bb =>
                        {
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                            bb.ToJson();
                            bb.ToJson(null);
                        });

                    b.Ignore(x => x.OwnedCollection2);
                });

            var model = modelBuilder.FinalizeModel();
            var outerOwnedEntities = model.FindEntityTypes(typeof(OwnedEntityExtraLevel));
            Assert.Equal(2, outerOwnedEntities.Count());

            foreach (var outerOwnedEntity in outerOwnedEntities)
            {
                Assert.False(outerOwnedEntity.IsMappedToJson());
#pragma warning disable CS0618
                Assert.Null(outerOwnedEntity.GetContainerColumnTypeMapping());
#pragma warning restore CS0618
                var myEnum = outerOwnedEntity.GetDeclaredProperties().Where(p => p.ClrType.IsEnum).Single();
                var typeMapping = myEnum.FindRelationalTypeMapping()!;

                Assert.True(typeMapping.Converter is EnumToNumberConverter<MyJsonEnum, int>);
            }

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(8, ownedEntities.Count());

            foreach (var ownedEntity in ownedEntities)
            {
                Assert.False(ownedEntity.IsMappedToJson());
#pragma warning disable CS0618
                Assert.Null(ownedEntity.GetContainerColumnTypeMapping());
#pragma warning restore CS0618
                var myEnum = ownedEntity.GetDeclaredProperties().Where(p => p.ClrType.IsEnum).Single();
                var typeMapping = myEnum.FindRelationalTypeMapping()!;
                Assert.True(typeMapping.Converter is EnumToNumberConverter<MyJsonEnum, int>);
            }
        }

        [ConditionalFact]
        public virtual void Json_entity_mapped_to_view()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntity>(
                b =>
                {
                    b.ToView("MyView");
                    b.OwnsOne(x => x.OwnedReference1, bb => bb.ToJson());
                    b.Ignore(x => x.OwnedReference2);
                    b.OwnsMany(x => x.OwnedCollection1, bb => bb.ToJson());
                    b.Ignore(x => x.OwnedCollection2);
                });

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(JsonEntity))!;
            Assert.Equal("MyView", owner.GetViewName());

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(2, ownedEntities.Count());
            Assert.Equal(2, ownedEntities.Where(e => e.IsMappedToJson()).Count());
            Assert.True(ownedEntities.All(x => x.GetViewName() == "MyView"));
        }

        [ConditionalFact]
        public virtual void Json_entity_with_custom_property_names()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntityWithNesting>(
                b =>
                {
                    b.OwnsOne(
                        x => x.OwnedReference1, bb =>
                        {
                            bb.ToJson();
                            bb.Property(x => x.Date).HasJsonPropertyName("OuterDate");
                            bb.Property(x => x.Fraction).HasJsonPropertyName("OuterFraction");
                            bb.Property(x => x.Enum).HasJsonPropertyName("OuterEnum");
                            bb.OwnsOne(
                                x => x.Reference1, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedReference1");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.OwnsOne(
                                x => x.Reference2, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedReference2");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.OwnsMany(
                                x => x.Collection1, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedCollection1");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.OwnsMany(
                                x => x.Collection2, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedCollection2");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                        });

                    b.OwnsMany(
                        x => x.OwnedCollection1, bb =>
                        {
                            bb.Property(x => x.Date).HasJsonPropertyName("OuterDate");
                            bb.Property(x => x.Fraction).HasJsonPropertyName("OuterFraction");
                            bb.Property(x => x.Enum).HasJsonPropertyName("OuterEnum");
                            bb.OwnsOne(
                                x => x.Reference1, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedReference1");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.OwnsOne(
                                x => x.Reference2, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedReference2");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.OwnsMany(
                                x => x.Collection1, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedCollection1");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.OwnsMany(
                                x => x.Collection2, bbb =>
                                {
                                    bbb.HasJsonPropertyName("RenamedCollection2");
                                    bbb.Property(x => x.Date).HasJsonPropertyName("InnerDate");
                                    bbb.Property(x => x.Fraction).HasJsonPropertyName("InnerFraction");
                                    bbb.Property(x => x.Enum).HasJsonPropertyName("InnerEnum");
                                });
                            bb.ToJson();
                        });

                    b.Ignore(x => x.OwnedReference2);
                    b.Ignore(x => x.OwnedCollection2);
                });

            var model = modelBuilder.FinalizeModel();
            var outerOwnedEntities = model.FindEntityTypes(typeof(OwnedEntityExtraLevel));
            Assert.Equal(2, outerOwnedEntities.Count());

            foreach (var outerOwnedEntity in outerOwnedEntities)
            {
                Assert.Equal("OuterDate", outerOwnedEntity.GetProperty("Date").GetJsonPropertyName());
                Assert.Equal("OuterFraction", outerOwnedEntity.GetProperty("Fraction").GetJsonPropertyName());
                Assert.Equal("OuterEnum", outerOwnedEntity.GetProperty("Enum").GetJsonPropertyName());
                Assert.Equal(
                    "RenamedReference1",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Reference1").TargetEntityType.GetJsonPropertyName());
                Assert.Equal(
                    "RenamedReference2",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Reference2").TargetEntityType.GetJsonPropertyName());
                Assert.Equal(
                    "RenamedCollection1",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Collection1").TargetEntityType.GetJsonPropertyName());
                Assert.Equal(
                    "RenamedCollection2",
                    outerOwnedEntity.GetNavigations().Single(n => n.Name == "Collection2").TargetEntityType.GetJsonPropertyName());
            }

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(8, ownedEntities.Count());

            foreach (var ownedEntity in ownedEntities)
            {
                Assert.Equal("InnerDate", ownedEntity.GetProperty("Date").GetJsonPropertyName());
                Assert.Equal("InnerFraction", ownedEntity.GetProperty("Fraction").GetJsonPropertyName());
                Assert.Equal("InnerEnum", ownedEntity.GetProperty("Enum").GetJsonPropertyName());
            }
        }

        [ConditionalFact]
        public virtual void Json_entity_and_normal_owned_can_exist_side_to_side_on_same_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<JsonEntity>(
                b =>
                {
                    b.OwnsOne(x => x.OwnedReference1);
                    b.OwnsOne(x => x.OwnedReference2, bb => bb.ToJson("reference"));
                    b.OwnsMany(x => x.OwnedCollection1);
                    b.OwnsMany(x => x.OwnedCollection2, bb => bb.ToJson("collection"));
                });

            var model = modelBuilder.FinalizeModel();

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(4, ownedEntities.Count());
            Assert.Equal(2, ownedEntities.Where(e => e.IsMappedToJson()).Count());
            Assert.Equal(2, ownedEntities.Where(e => e.IsOwned() && !e.IsMappedToJson()).Count());
        }

        [ConditionalFact]
        public virtual void Json_entity_with_nested_structure_same_property_names_()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<JsonEntityWithNesting>(
                b =>
                {
                    b.OwnsOne(
                        x => x.OwnedReference1, bb =>
                        {
                            bb.ToJson("ref1");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.OwnsOne(
                        x => x.OwnedReference2, bb =>
                        {
                            bb.ToJson("ref2");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.OwnsMany(
                        x => x.OwnedCollection1, bb =>
                        {
                            bb.ToJson("col1");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });

                    b.OwnsMany(
                        x => x.OwnedCollection2, bb =>
                        {
                            bb.ToJson("col2");
                            bb.OwnsOne(x => x.Reference1);
                            bb.OwnsOne(x => x.Reference2);
                            bb.OwnsMany(x => x.Collection1);
                            bb.OwnsMany(x => x.Collection2);
                        });
                });

            var model = modelBuilder.FinalizeModel();
            var outerOwnedEntities = model.FindEntityTypes(typeof(OwnedEntityExtraLevel));
            Assert.Equal(4, outerOwnedEntities.Count());

            var ownedEntities = model.FindEntityTypes(typeof(OwnedEntity));
            Assert.Equal(16, ownedEntities.Count());
        }
    }

    public class SqlServerModelBuilderFixture : RelationalModelBuilderFixture
    {
        public override TestHelpers TestHelpers => SqlServerTestHelpers.Instance;
    }

    public abstract class TestTemporalTableBuilder<TEntity>
        where TEntity : class
    {
        public abstract TestTemporalTableBuilder<TEntity> UseHistoryTable(string name, string? schema);
        public abstract TestTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName);
        public abstract TestTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName);
    }

    public class GenericTestTemporalTableBuilder<TEntity>(TemporalTableBuilder<TEntity> temporalTableBuilder) : TestTemporalTableBuilder<TEntity>,
        IInfrastructure<TemporalTableBuilder<TEntity>>
        where TEntity : class
    {
        private TemporalTableBuilder<TEntity> TemporalTableBuilder { get; } = temporalTableBuilder;

        TemporalTableBuilder<TEntity> IInfrastructure<TemporalTableBuilder<TEntity>>.Instance
            => TemporalTableBuilder;

        protected virtual TestTemporalTableBuilder<TEntity> Wrap(TemporalTableBuilder<TEntity> tableBuilder)
            => new GenericTestTemporalTableBuilder<TEntity>(tableBuilder);

        public override TestTemporalTableBuilder<TEntity> UseHistoryTable(string name, string? schema)
            => Wrap(TemporalTableBuilder.UseHistoryTable(name, schema));

        public override TestTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
            => new(TemporalTableBuilder.HasPeriodStart(propertyName));

        public override TestTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
            => new(TemporalTableBuilder.HasPeriodEnd(propertyName));
    }

    public class NonGenericTestTemporalTableBuilder<TEntity>(TemporalTableBuilder temporalTableBuilder) : TestTemporalTableBuilder<TEntity>, IInfrastructure<TemporalTableBuilder>
        where TEntity : class
    {
        private TemporalTableBuilder TemporalTableBuilder { get; } = temporalTableBuilder;

        TemporalTableBuilder IInfrastructure<TemporalTableBuilder>.Instance
            => TemporalTableBuilder;

        protected virtual TestTemporalTableBuilder<TEntity> Wrap(TemporalTableBuilder temporalTableBuilder)
            => new NonGenericTestTemporalTableBuilder<TEntity>(temporalTableBuilder);

        public override TestTemporalTableBuilder<TEntity> UseHistoryTable(string name, string? schema)
            => Wrap(TemporalTableBuilder.UseHistoryTable(name, schema));

        public override TestTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
            => new(TemporalTableBuilder.HasPeriodStart(propertyName));

        public override TestTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
            => new(TemporalTableBuilder.HasPeriodEnd(propertyName));
    }

    public abstract class TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        public abstract TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>
            UseHistoryTable(string name, string? schema);

        public abstract TestOwnedNavigationTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName);
        public abstract TestOwnedNavigationTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName);
    }

    public class GenericTestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(
        OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> temporalTableBuilder) :
        TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        private OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> TemporalTableBuilder { get; } = temporalTableBuilder;

        OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>
            IInfrastructure<OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>>.Instance
            => TemporalTableBuilder;

        protected virtual TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder)
            => new GenericTestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(tableBuilder);

        public override TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> UseHistoryTable(string name, string? schema)
            => Wrap(TemporalTableBuilder.UseHistoryTable(name, schema));

        public override TestOwnedNavigationTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
            => new(TemporalTableBuilder.HasPeriodStart(propertyName));

        public override TestOwnedNavigationTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
            => new(TemporalTableBuilder.HasPeriodEnd(propertyName));
    }

    public class NonGenericTestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(OwnedNavigationTemporalTableBuilder temporalTableBuilder) :
        TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>,
        IInfrastructure<OwnedNavigationTemporalTableBuilder>
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        private OwnedNavigationTemporalTableBuilder TemporalTableBuilder { get; } = temporalTableBuilder;

        OwnedNavigationTemporalTableBuilder IInfrastructure<OwnedNavigationTemporalTableBuilder>.Instance
            => TemporalTableBuilder;

        protected virtual TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> Wrap(
            OwnedNavigationTemporalTableBuilder temporalTableBuilder)
            => new NonGenericTestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(temporalTableBuilder);

        public override TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> UseHistoryTable(string name, string? schema)
            => Wrap(TemporalTableBuilder.UseHistoryTable(name, schema));

        public override TestOwnedNavigationTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
            => new(TemporalTableBuilder.HasPeriodStart(propertyName));

        public override TestOwnedNavigationTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
            => new(TemporalTableBuilder.HasPeriodEnd(propertyName));
    }

    public class TestTemporalPeriodPropertyBuilder(TemporalPeriodPropertyBuilder temporalPeriodPropertyBuilder)
    {
        protected TemporalPeriodPropertyBuilder TemporalPeriodPropertyBuilder { get; } = temporalPeriodPropertyBuilder;

        public TestTemporalPeriodPropertyBuilder HasColumnName(string name)
            => new(TemporalPeriodPropertyBuilder.HasColumnName(name));
    }

    public class TestOwnedNavigationTemporalPeriodPropertyBuilder(OwnedNavigationTemporalPeriodPropertyBuilder temporalPeriodPropertyBuilder)
    {
        protected OwnedNavigationTemporalPeriodPropertyBuilder TemporalPeriodPropertyBuilder { get; } = temporalPeriodPropertyBuilder;

        public TestOwnedNavigationTemporalPeriodPropertyBuilder HasColumnName(string name)
            => new(TemporalPeriodPropertyBuilder.HasColumnName(name));
    }
}
