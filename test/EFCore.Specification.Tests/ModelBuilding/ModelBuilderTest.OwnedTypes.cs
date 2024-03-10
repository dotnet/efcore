// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

#nullable disable

public abstract partial class ModelBuilderTest
{
    public abstract class OwnedTypesTestBase(ModelBuilderFixtureBase fixture) : ModelBuilderTestBase(fixture)
    {
        [ConditionalFact]
        public virtual void Can_configure_owned_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>()
                .OwnsOne(
                    c => c.Details, db =>
                    {
                        db.WithOwner(d => d.Customer)
                            .HasPrincipalKey(c => c.AlternateKey);
                        db.Property(d => d.CustomerId);
                        db.HasIndex(d => d.CustomerId);
                    });

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(Customer));
            Assert.Equal(typeof(Customer).FullName, owner.Name);
            var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
            Assert.Equal(nameof(CustomerDetails.Customer), ownership.DependentToPrincipal.Name);
            Assert.Equal("CustomerAlternateKey", ownership.Properties.Single().Name);
            Assert.Equal(nameof(Customer.AlternateKey), ownership.PrincipalKey.Properties.Single().Name);
            var owned = ownership.DeclaringEntityType;
            Assert.Single(owned.GetForeignKeys());
            Assert.Equal(nameof(CustomerDetails.CustomerId), owned.GetIndexes().Single().Properties.Single().Name);
            Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_using_nested_closure()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsOne(
                c => c.Details,
                r => r.HasAnnotation("foo", "bar")
                    .WithOwner(d => d.Customer)
                    .HasAnnotation("bar", "foo"));

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal("bar", owned.FindAnnotation("foo").Value);
            Assert.Single(owned.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_configure_one_to_one_owned_type_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Owned<OneToOneOwnedWithField>();
            modelBuilder.Entity<OneToOneOwnerWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.AlternateKey);
                    e.Property(p => p.Description);
                    e.HasKey(p => p.Id);
                });

            modelBuilder.Entity<OneToOneOwnerWithField>()
                .OwnsOne(
                    owner => owner.OwnedDependent,
                    db =>
                    {
                        db.WithOwner(d => d.OneToOneOwner);
                        db.Property(d => d.OneToOneOwnerId);
                        db.HasIndex(d => d.OneToOneOwnerId);
                        db.Navigation(owned => owned.OneToOneOwner);
                    });

            modelBuilder.Entity<OneToOneOwnerWithField>()
                .Navigation(owner => owner.OwnedDependent);

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(OneToOneOwnerWithField));
            Assert.Equal(typeof(OneToOneOwnerWithField).FullName, owner.Name);
            var ownership = owner.FindNavigation(nameof(OneToOneOwnerWithField.OwnedDependent)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(OneToOneOwnerWithField.OwnedDependent), ownership.PrincipalToDependent.Name);
            Assert.Equal(nameof(OneToOneOwnedWithField.OneToOneOwner), ownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(OneToOneOwnedWithField.OneToOneOwnerId), ownership.Properties.Single().Name);
            Assert.Equal(nameof(OneToOneOwnerWithField.Id), ownership.PrincipalKey.Properties.Single().Name);
            var owned = ownership.DeclaringEntityType;
            Assert.Single(owned.GetForeignKeys());
            Assert.Equal(nameof(OneToOneOwnedWithField.OneToOneOwnerId), owned.GetIndexes().Single().Properties.Single().Name);
            Assert.NotNull(model.FindEntityType(typeof(OneToOneOwnedWithField)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(OneToOneOwnedWithField)));
        }

        [ConditionalFact]
        public virtual void Can_configure_one_to_many_owned_type_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Owned<OneToManyOwnedWithField>();
            modelBuilder.Entity<OneToManyOwnerWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.AlternateKey);
                    e.Property(p => p.Description);
                    e.HasKey(p => p.Id);
                });

            modelBuilder.Entity<OneToManyOwnerWithField>()
                .OwnsMany(
                    owner => owner.OwnedDependents,
                    db =>
                    {
                        db.WithOwner(d => d.OneToManyOwner);
                        db.Property(d => d.OneToManyOwnerId);
                        db.HasIndex(d => d.OneToManyOwnerId);
                        db.Navigation(owned => owned.OneToManyOwner);
                    });

            modelBuilder.Entity<OneToManyOwnerWithField>()
                .Navigation(owner => owner.OwnedDependents);

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(OneToManyOwnerWithField));
            Assert.Equal(typeof(OneToManyOwnerWithField).FullName, owner.Name);
            var ownership = owner.FindNavigation(nameof(OneToManyOwnerWithField.OwnedDependents)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(OneToManyOwnerWithField.OwnedDependents), ownership.PrincipalToDependent.Name);
            Assert.Equal(nameof(OneToManyOwnedWithField.OneToManyOwner), ownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(OneToManyOwnedWithField.OneToManyOwnerId), ownership.Properties.Single().Name);
            Assert.Equal(nameof(OneToManyOwnerWithField.Id), ownership.PrincipalKey.Properties.Single().Name);
            var owned = ownership.DeclaringEntityType;
            Assert.Single(owned.GetForeignKeys());
            Assert.Equal(nameof(OneToManyOwnedWithField.OneToManyOwnerId), owned.GetIndexes().Single().Properties.Single().Name);
            Assert.NotNull(model.FindEntityType(typeof(OneToManyOwnedWithField)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(OneToManyOwnedWithField)));
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_inverse()
        {
            var modelBuilder = CreateModelBuilder();
            IReadOnlyModel model = modelBuilder.Model;

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);

            var owner = model.FindEntityType(typeof(Customer));
            var ownee = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
            Assert.Equal(nameof(CustomerDetails.CustomerId), ownee.FindPrimaryKey().Properties.Single().Name);

            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                .HasOne(d => d.Customer);

            model = modelBuilder.FinalizeModel();

            var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(CustomerDetails.Customer), ownership.DependentToPrincipal.Name);
            Assert.Same(ownee, ownership.DeclaringEntityType);
            Assert.Equal(nameof(CustomerDetails.CustomerId), ownee.FindPrimaryKey().Properties.Single().Name);
            Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)
                .Ignore(d => d.Id)
                .Property<int>("foo");

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(Customer));
            var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
            Assert.Null(owner.FindProperty("foo"));
            Assert.Contains("foo", owned.GetProperties().Select(p => p.Name));
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, owned.GetPropertyAccessMode());
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, owned.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                .HasKey(c => c.Id);

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(Customer));
            var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
            Assert.Equal(nameof(CustomerDetails.Id), owned.FindPrimaryKey().Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Can_configure_ownership_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>()
                .OwnsOne(c => c.Details)
                .WithOwner(d => d.Customer)
                .HasForeignKey(c => c.Id);

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            Assert.Equal(nameof(CustomerDetails.Id), ownership.Properties.Single().Name);
            Assert.Equal(nameof(CustomerDetails.Id), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
            Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_configure_another_relationship_to_owner()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsOne(
                c => c.Details,
                r =>
                {
                    r.WithOwner();
                    r.HasOne(d => d.Customer)
                        .WithMany();
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
            Assert.Null(ownership.DependentToPrincipal);
            Assert.Equal("CustomerId", ownership.Properties.Single().Name);

            var otherFk = owned.GetForeignKeys().Single(fk => fk != ownership);
            Assert.Null(otherFk.PrincipalToDependent);
            Assert.Equal(nameof(CustomerDetails.Customer), otherFk.DependentToPrincipal.Name);
            Assert.Equal("CustomerId1", otherFk.Properties.Single().Name);
            Assert.False(otherFk.IsOwnership);
            Assert.False(otherFk.IsUnique);

            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
        }

        [ConditionalFact]
        public virtual void Changing_ownership_uniqueness_throws()
        {
            var modelBuilder = CreateModelBuilder();
            var customerBuilder = modelBuilder.Entity<Customer>();

            Assert.Equal(
                CoreStrings.UnableToSetIsUnique(
                    false,
                    nameof(Customer.Details),
                    nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () => customerBuilder.OwnsOne(
                        c => c.Details,
                        r =>
                        {
                            r.HasOne(d => d.Customer)
                                .WithMany();
                        })).Message);
        }

        [ConditionalFact]
        public virtual void Can_configure_on_derived_type_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details);
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            Assert.Equal(typeof(CustomerDetails), ownership.DeclaringEntityType.ClrType);
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_configure_on_derived_types_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details);
            modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            Assert.Equal(typeof(CustomerDetails), ownership.DeclaringEntityType.ClrType);
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_configure_multiple_ownerships()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details);
            modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

            var model = modelBuilder.FinalizeModel();

            var ownership1 = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            var ownership2 = model.FindEntityType(typeof(SpecialCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            Assert.Equal(typeof(CustomerDetails), ownership1.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(CustomerDetails), ownership2.DeclaringEntityType.ClrType);
            Assert.NotSame(ownership1.DeclaringEntityType, ownership2.DeclaringEntityType);
            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            Assert.Single(ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(ownership2.DeclaringEntityType.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_configure_one_to_one_relationship_from_an_owned_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<SpecialCustomer>();
            modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details)
                .HasOne<SpecialCustomer>()
                .WithOne()
                .HasPrincipalKey<SpecialCustomer>();

            modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
            var foreignKey = model.FindEntityType(typeof(SpecialCustomer)).GetReferencingForeignKeys()
                .Single(
                    fk => fk.DeclaringEntityType.ClrType == typeof(CustomerDetails)
                        && fk.PrincipalToDependent == null);
            Assert.Same(ownership.DeclaringEntityType, foreignKey.DeclaringEntityType);
            Assert.NotEqual(ownership.Properties.Single().Name, foreignKey.Properties.Single().Name);
            Assert.Equal(2, model.FindEntityTypes(typeof(CustomerDetails)).Count());
            Assert.Equal(2, ownership.DeclaringEntityType.GetForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection_from_an_owned_type()
        {
            var modelBuilder = CreateModelBuilder();
            IReadOnlyModel model = modelBuilder.Model;

            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Product>();
            var entityBuilder = modelBuilder.Entity<CustomerDetails>().OwnsOne(o => o.Customer)
                .OwnsMany(c => c.Orders);

            var ownership = model.FindEntityType(typeof(CustomerDetails)).FindNavigation(nameof(CustomerDetails.Customer)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            var chainedOwnership = owned.FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var chainedOwned = chainedOwnership.DeclaringEntityType;
            Assert.Equal(
                new[] { nameof(Order.CustomerId), nameof(Order.OrderId) },
                chainedOwned.FindPrimaryKey().Properties.Select(p => p.Name));

            entityBuilder.HasKey(o => o.OrderId);

            model = modelBuilder.FinalizeModel();

            Assert.True(ownership.IsOwnership);
            Assert.True(ownership.IsUnique);
            Assert.Equal(nameof(Customer.Details), ownership.DependentToPrincipal.Name);
            Assert.Equal("DetailsId", ownership.Properties.Single().Name);
            Assert.Equal("DetailsId", owned.FindPrimaryKey().Properties.Single().Name);
            Assert.Empty(owned.GetIndexes());
            Assert.True(chainedOwnership.IsOwnership);
            Assert.False(chainedOwnership.IsUnique);
            Assert.Equal(nameof(Order.Customer), chainedOwnership.DependentToPrincipal.Name);
            Assert.Equal(nameof(Order.CustomerId), chainedOwnership.Properties.Single().Name);
            Assert.Equal(nameof(Order.OrderId), chainedOwned.FindPrimaryKey().Properties.Single().Name);
            Assert.Single(chainedOwned.GetForeignKeys());
            Assert.Equal(nameof(Order.CustomerId), chainedOwned.GetIndexes().Single().Properties.Single().Name);
            Assert.Same(entityBuilder.OwnedEntityType, chainedOwned);

            Assert.Equal(3, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            var entityBuilder = modelBuilder.Entity<Customer>().OwnsMany(c => c.Orders)
                .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)
                .Ignore(nameof(Order.OrderId))
                .Ignore(o => o.OrderCombination)
                .Ignore(o => o.Details);
            entityBuilder.Property<int>("foo");
            entityBuilder.HasIndex("foo");
            entityBuilder.HasKey(o => o.AnotherCustomerId);
            entityBuilder.WithOwner(o => o.Customer)
                .HasPrincipalKey(c => c.AlternateKey);

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(Customer));
            var ownership = owner.FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal("CustomerAlternateKey", ownership.Properties.Single().Name);
            Assert.Equal(nameof(Customer.AlternateKey), ownership.PrincipalKey.Properties.Single().Name);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);

            Assert.Null(owner.FindProperty("foo"));
            Assert.Equal(nameof(Order.AnotherCustomerId), owned.FindPrimaryKey().Properties.Single().Name);
            Assert.Equal(2, owned.GetIndexes().Count());
            Assert.Equal("CustomerAlternateKey", owned.GetIndexes().First().Properties.Single().Name);
            Assert.Equal("foo", owned.GetIndexes().Last().Properties.Single().Name);
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, owned.GetPropertyAccessMode());
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, owned.GetChangeTrackingStrategy());

            Assert.NotNull(model.FindEntityType(typeof(Order)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(Order)));
            Assert.Single(owned.GetForeignKeys());
            Assert.Single(owned.GetNavigations());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection_using_nested_closure()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.HasAnnotation("foo", "bar");
                    r.Property<uint>("Id");
                    r.HasKey("Id");
                    r.HasIndex(o => o.AnotherCustomerId);
                    r.Property(o => o.AnotherCustomerId).IsRequired();
                    r.Ignore(o => o.OrderCombination);
                    r.Ignore(o => o.Details);
                    r.WithOwner(o => o.Customer)
                        .HasAnnotation("bar", "foo")
                        .HasForeignKey("DifferentCustomerId");
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal("DifferentCustomerId", ownership.Properties.Single().Name);
            Assert.Equal("bar", owned.FindAnnotation("foo").Value);
            Assert.Single(owned.GetForeignKeys());
            Assert.Equal("Id", owned.FindPrimaryKey().Properties.Single().Name);
            Assert.Equal(2, owned.GetIndexes().Count());
            Assert.Equal(nameof(Order.AnotherCustomerId), owned.GetIndexes().First().Properties.Single().Name);
            Assert.Equal("DifferentCustomerId", owned.GetIndexes().Last().Properties.Single().Name);
            Assert.False(owned.FindProperty(nameof(Order.AnotherCustomerId)).IsNullable);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
        }

        [ConditionalFact]
        public virtual void Can_configure_one_to_one_relationship_from_an_owned_type_collection()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<OtherCustomer>().OwnsMany(
                c => c.Orders, ob =>
                {
                    ob.HasKey(o => o.OrderId);
                    ob.HasOne<SpecialCustomer>()
                        .WithOne()
                        .HasPrincipalKey<SpecialCustomer>();
                });

            modelBuilder.Entity<SpecialCustomer>().OwnsMany(c => c.Orders)
                .HasKey(o => o.OrderId);

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(Order)));
            var ownership1 = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var ownership2 = model.FindEntityType(typeof(SpecialCustomer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            Assert.Equal(typeof(Order), ownership1.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(Order), ownership2.DeclaringEntityType.ClrType);
            Assert.NotSame(ownership1.DeclaringEntityType, ownership2.DeclaringEntityType);
            Assert.Equal(nameof(Order.Customer), ownership1.DependentToPrincipal.Name);
            Assert.Equal(nameof(Order.Customer), ownership2.DependentToPrincipal.Name);
            Assert.Equal("CustomerId", ownership1.Properties.Single().Name);
            Assert.Equal("CustomerId", ownership2.Properties.Single().Name);

            var foreignKey = model.FindEntityType(typeof(SpecialCustomer)).GetReferencingForeignKeys()
                .Single(
                    fk => fk.DeclaringEntityType.ClrType == typeof(Order)
                        && fk.PrincipalToDependent == null);
            Assert.Same(ownership1.DeclaringEntityType, foreignKey.DeclaringEntityType);
            Assert.Null(foreignKey.PrincipalToDependent);
            Assert.NotEqual(ownership1.Properties.Single().Name, foreignKey.Properties.Single().Name);
            Assert.Equal(2, model.FindEntityTypes(typeof(Order)).Count());
            Assert.Equal(2, ownership1.DeclaringEntityType.GetForeignKeys().Count());

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(Order)));
            Assert.Equal(2, ownership1.DeclaringEntityType.GetForeignKeys().Count());
            Assert.Single(ownership2.DeclaringEntityType.GetForeignKeys());
            Assert.Null(model.FindEntityType(typeof(SpecialOrder)));
        }

        [ConditionalFact]
        public virtual void Can_call_Owner_fluent_api_after_calling_Entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OwnerOfOwnees>();
            modelBuilder.Owned<Ownee1>();
            modelBuilder.Owned<Ownee2>();
            modelBuilder.Owned<Ownee3>();
        }

        [Flags]
        public enum HasDataOverload
        {
            Array = 0,
            Enumerable = 1,
            Generic = 2,
            Params = 4
        }

        [ConditionalTheory]
        [InlineData(HasDataOverload.Array)]
        [InlineData(HasDataOverload.Array | HasDataOverload.Params)]
        [InlineData(HasDataOverload.Array | HasDataOverload.Generic)]
        [InlineData(HasDataOverload.Array | HasDataOverload.Params | HasDataOverload.Generic)]
        [InlineData(HasDataOverload.Enumerable)]
        [InlineData(HasDataOverload.Enumerable | HasDataOverload.Generic)]
        public virtual void Can_configure_owned_type_from_an_owned_type_collection(HasDataOverload hasDataOverload)
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders, ob =>
                {
                    ob.HasKey(o => o.OrderId);
                    var ownedNavigationBuilder = ob.OwnsOne(o => o.Details);

                    switch (hasDataOverload)
                    {
                        case HasDataOverload.Array:
                            ownedNavigationBuilder.HasData(new object[] { new OrderDetails { OrderId = -1 } });
                            break;
                        case HasDataOverload.Array | HasDataOverload.Params:
                            ownedNavigationBuilder.HasData((object)new OrderDetails { OrderId = -1 });
                            break;
                        case HasDataOverload.Array | HasDataOverload.Generic:
                            // ReSharper disable once RedundantExplicitParamsArrayCreation
                            ownedNavigationBuilder.HasData([new OrderDetails { OrderId = -1 }]);
                            break;
                        case HasDataOverload.Array | HasDataOverload.Params | HasDataOverload.Generic:
                            ownedNavigationBuilder.HasData(new OrderDetails { OrderId = -1 });
                            break;
                        case HasDataOverload.Enumerable:
                            ownedNavigationBuilder.HasData(new List<object> { new OrderDetails { OrderId = -1 } });
                            break;
                        case HasDataOverload.Enumerable | HasDataOverload.Generic:
                            ownedNavigationBuilder.HasData(new List<OrderDetails> { new() { OrderId = -1 } });
                            break;
                        default:
                            Assert.Fail($"Unexpected HasData overload specification {hasDataOverload}");
                            break;
                    }
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.Equal(nameof(Order.CustomerId), ownership.Properties.Single().Name);
            Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
            var chainedOwnership = owned.FindNavigation(nameof(Order.Details)).ForeignKey;
            var chainedOwned = chainedOwnership.DeclaringEntityType;
            Assert.True(chainedOwnership.IsOwnership);
            Assert.True(chainedOwnership.IsUnique);
            Assert.Equal(nameof(OrderDetails.OrderId), chainedOwned.FindPrimaryKey().Properties.Single().Name);
            Assert.Empty(chainedOwned.GetIndexes());
            Assert.Equal(-1, chainedOwned.GetSeedData().Single()[nameof(OrderDetails.OrderId)]);
            Assert.Equal(nameof(OrderDetails.OrderId), chainedOwnership.Properties.Single().Name);
            Assert.Equal(nameof(OrderDetails.Order), chainedOwnership.DependentToPrincipal.Name);

            Assert.Equal(4, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Can_chain_owned_type_collection_configurations()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders, ob =>
                {
                    ob.HasKey(o => o.OrderId);
                    ob.HasData(
                        new Order { OrderId = -2, CustomerId = -1 });
                    ob.OwnsMany(
                        o => o.Products, pb =>
                        {
                            pb.WithOwner(p => p.Order);
                            pb.Ignore(p => p.Categories);
                            pb.HasKey(p => p.Id);
                        });
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
            var seedData = owned.GetSeedData().Single();
            Assert.Equal(-2, seedData[nameof(Order.OrderId)]);
            Assert.Equal(-1, seedData[nameof(Order.CustomerId)]);
            var chainedOwnership = owned.FindNavigation(nameof(Order.Products)).ForeignKey;
            var chainedOwned = chainedOwnership.DeclaringEntityType;
            Assert.True(chainedOwnership.IsOwnership);
            Assert.False(chainedOwnership.IsUnique);
            Assert.Equal("OrderId", chainedOwnership.Properties.Single().Name);
            Assert.Equal(nameof(Product.Id), chainedOwned.FindPrimaryKey().Properties.Single().Name);
            Assert.Equal(
                "OrderId",
                chainedOwned.GetIndexes().Single().Properties.Single().Name);
            Assert.Equal(nameof(Product.Order), chainedOwnership.DependentToPrincipal.Name);

            Assert.Equal(4, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection_without_explicit_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.Ignore(o => o.OrderCombination);
                    r.Ignore(o => o.Details);
                    r.OwnsMany(
                        o => o.Products, pb =>
                        {
                            pb.WithOwner(p => p.Order);
                            pb.Ignore(p => p.Categories);
                        });
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(Order.CustomerId), ownership.Properties.Single().Name);
            Assert.Single(owned.GetForeignKeys());
            var pk = owned.FindPrimaryKey();
            Assert.Equal(new[] { nameof(Order.CustomerId), nameof(Order.OrderId) }, pk.Properties.Select(p => p.Name));
            Assert.Empty(owned.GetIndexes());

            var chainedOwnership = owned.FindNavigation(nameof(Order.Products)).ForeignKey;
            var chainedOwned = chainedOwnership.DeclaringEntityType;
            Assert.True(chainedOwnership.IsOwnership);
            Assert.False(chainedOwnership.IsUnique);
            Assert.Equal(nameof(Product.Order), chainedOwnership.DependentToPrincipal.Name);
            Assert.Equal(new[] { "OrderCustomerId", "OrderId" }, chainedOwnership.Properties.Select(p => p.Name));
            var chainedPk = chainedOwned.FindPrimaryKey();
            Assert.Equal(new[] { "OrderCustomerId", "OrderId", nameof(Product.Id) }, chainedPk.Properties.Select(p => p.Name));
            Assert.Empty(chainedOwned.GetIndexes());

            Assert.Equal(4, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection_without_explicit_key_or_candidate()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().OwnsMany(
                c => c.Orders,
                r =>
                {
                    r.Ignore(o => o.OrderCombination);
                    r.Ignore(o => o.Details);
                    r.Ignore(o => o.OrderId);
                    r.OwnsMany(
                        o => o.Products, pb =>
                        {
                            pb.WithOwner(p => p.Order);
                            pb.Ignore(p => p.Categories);
                            pb.Ignore(p => p.Id);
                        });
                });

            var model = modelBuilder.FinalizeModel();

            var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
            var owned = ownership.DeclaringEntityType;
            Assert.True(ownership.IsOwnership);
            Assert.False(ownership.IsUnique);
            Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(Order.CustomerId), ownership.Properties.Single().Name);
            Assert.Single(owned.GetForeignKeys());
            var pk = owned.FindPrimaryKey();
            Assert.Equal(new[] { nameof(Order.CustomerId), "Id" }, pk.Properties.Select(p => p.Name));
            Assert.Empty(owned.GetIndexes());

            var chainedOwnership = owned.FindNavigation(nameof(Order.Products)).ForeignKey;
            var chainedOwned = chainedOwnership.DeclaringEntityType;
            Assert.True(chainedOwnership.IsOwnership);
            Assert.False(chainedOwnership.IsUnique);
            Assert.Equal(nameof(Product.Order), chainedOwnership.DependentToPrincipal.Name);
            Assert.Equal(new[] { "OrderCustomerId", "OrderId" }, chainedOwnership.Properties.Select(p => p.Name));
            var chainedPk = chainedOwned.FindPrimaryKey();
            Assert.Equal(new[] { "OrderCustomerId", "OrderId", "Id1" }, chainedPk.Properties.Select(p => p.Name));
            Assert.Empty(chainedOwned.GetIndexes());

            Assert.Equal(4, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Ambiguous_relationship_between_owned_types_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Owned<BookLabel>();
            modelBuilder.Owned<Book>();
            modelBuilder.Entity<BookDetails>();

            Assert.Equal(
                CoreStrings.AmbiguousOwnedNavigation(
                    "Book.AlternateLabel#BookLabel.Book",
                    nameof(Book)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection_with_one_call()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Owned<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Entity<Customer>()
                .OwnsMany(c => c.Orders)
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<SpecialCustomer>()
                .OwnsMany(
                    c => c.SpecialOrders, so =>
                    {
                        so.HasKey(o => o.SpecialOrderId);
                        so.Ignore(o => o.Customer);
                        so.OwnsOne(o => o.BackOrder);
                    });

            var model = modelBuilder.FinalizeModel();

            var customer = model.FindEntityType(typeof(Customer));
            var specialCustomer = model.FindEntityType(typeof(SpecialCustomer));

            var ownership = customer.FindNavigation(nameof(Customer.Orders)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.False(ownership.IsUnique);
            Assert.Equal(nameof(Order.OrderId), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
            Assert.Same(
                ownership.DeclaringEntityType,
                model.FindEntityType(typeof(Order), nameof(Customer.Orders), customer));
            Assert.True(model.IsShared(typeof(Order)));

            var specialOwnership = specialCustomer.FindNavigation(nameof(SpecialCustomer.SpecialOrders)).ForeignKey;
            Assert.True(specialOwnership.IsOwnership);
            Assert.False(specialOwnership.IsUnique);
            Assert.Equal(
                nameof(SpecialOrder.SpecialOrderId), specialOwnership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
            Assert.Same(
                specialOwnership.DeclaringEntityType,
                model.FindEntityType(typeof(SpecialOrder)));

            Assert.Equal(9, modelBuilder.Model.GetEntityTypes().Count());
            Assert.Equal(2, modelBuilder.Model.FindEntityTypes(typeof(Order)).Count());
            Assert.Equal(7, modelBuilder.Model.GetEntityTypes().Count(e => !e.HasSharedClrType));

            var conventionModel = (IConventionModel)modelBuilder.Model;
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Order)));
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialOrder)));
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Customer)));
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialCustomer)));
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_type_collection_with_one_call_afterwards()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Owned<SpecialOrder>();

            modelBuilder.Entity<SpecialCustomer>();
            var specialCustomer = modelBuilder.Entity<SpecialCustomer>().OwnsMany(
                c => c.SpecialOrders, so =>
                {
                    so.HasKey(o => o.SpecialOrderId);
                    so.Ignore(o => o.Customer);
                    so.OwnsOne(o => o.BackOrder);
                }).Metadata;

            modelBuilder.Owned<Order>();

            modelBuilder.Entity<Customer>()
                .OwnsMany(c => c.Orders)
                .HasKey(o => o.OrderId);

            var model = modelBuilder.FinalizeModel();

            var customer = model.FindEntityType(typeof(Customer));

            var ownership = customer.FindNavigation(nameof(Customer.Orders)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.False(ownership.IsUnique);
            Assert.Equal(nameof(Order.OrderId), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
            var specialOwnership = specialCustomer.FindNavigation(nameof(SpecialCustomer.SpecialOrders)).ForeignKey;
            Assert.True(specialOwnership.IsOwnership);
            Assert.False(specialOwnership.IsUnique);
            Assert.Equal(
                nameof(SpecialOrder.SpecialOrderId), specialOwnership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);

            Assert.Equal(2, modelBuilder.Model.FindEntityTypes(typeof(Order)).Count());

            var conventionModel = (IConventionModel)modelBuilder.Model;
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Order)));
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialOrder)));
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Customer)));
            Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialCustomer)));
        }

        [ConditionalFact]
        public virtual void Can_configure_single_owned_type_using_attribute()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OrderCombination>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<SpecialOrder>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<SpecialCustomer>();

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(SpecialOrder));
            var ownership = owner.FindNavigation(nameof(SpecialOrder.ShippingAddress)).ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.True(ownership.IsRequired);
            Assert.True(ownership.IsRequiredDependent);
            Assert.NotNull(ownership.DeclaringEntityType.FindProperty(nameof(StreetAddress.Street)));
        }

        [ConditionalFact]
        public virtual void Can_configure_fk_on_multiple_ownerships()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<BookDetails>();

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, lb =>
                {
                    lb.WithOwner()
                        .HasForeignKey("BookLabelId")
                        .HasAnnotation("Foo", "Bar");
                    lb.Ignore(l => l.Book);
                });
            modelBuilder.Entity<Book>().OwnsOne(
                b => b.AlternateLabel, lb =>
                {
                    lb.WithOwner()
                        .HasForeignKey("BookLabelId");

                    lb.Ignore(l => l.Book);
                });

            var model = modelBuilder.FinalizeModel();

            var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
            var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
            Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
            Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
            Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
            Assert.Equal("Bar", bookOwnership1["Foo"]);

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(3, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Can_map_base_of_owned_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Order>();
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
            modelBuilder.Entity<DetailsBase>();
            modelBuilder.Ignore<SpecialBookLabel>();

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(BookDetails)));
            Assert.Null(model.FindEntityType(typeof(BookDetailsBase)));
            var baseType = model.FindEntityType(typeof(DetailsBase));
            var owner = model.FindEntityType(typeof(Customer));
            var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
            Assert.Null(owned.BaseType);
            Assert.Null(owned.GetDiscriminatorPropertyName());
            Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            Assert.Single(owned.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_map_base_of_owned_type_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Order>();
            modelBuilder.Entity<DetailsBase>();
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
            modelBuilder.Ignore<SpecialBookLabel>();

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(BookDetails)));
            Assert.Null(model.FindEntityType(typeof(BookDetailsBase)));
            var baseType = model.FindEntityType(typeof(DetailsBase));
            var owner = model.FindEntityType(typeof(Customer));
            var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
            Assert.Null(owned.BaseType);
            Assert.Null(owned.GetDiscriminatorPropertyName());
            Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            Assert.Single(owned.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Can_map_derived_of_owned_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Order>();
            modelBuilder.Ignore<SpecialOrder>();
            modelBuilder.Entity<OrderCombination>().OwnsOne(c => c.Details);
            modelBuilder.Entity<Customer>();

            IReadOnlyModel model = modelBuilder.Model;

            var owner = model.FindEntityType(typeof(OrderCombination));
            var owned = owner.FindNavigation(nameof(OrderCombination.Details)).ForeignKey.DeclaringEntityType;
            Assert.Empty(owned.GetDirectlyDerivedTypes());
            Assert.Null(owned.GetDiscriminatorPropertyName());
            var navToCustomerDetails = model.GetEntityTypes().SelectMany(e => e.GetDeclaredNavigations()).Where(
                n =>
                {
                    var targetType = n.TargetEntityType.ClrType;
                    return targetType != typeof(DetailsBase) && typeof(DetailsBase).IsAssignableFrom(targetType);
                }).Single();
            Assert.Single(owned.GetForeignKeys());
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(DetailsBase)));
            var derivedType = model.FindEntityType(typeof(CustomerDetails));
            Assert.Same(derivedType, navToCustomerDetails.TargetEntityType);
            Assert.Null(derivedType.BaseType);
            Assert.Null(derivedType.GetDiscriminatorPropertyName());

            modelBuilder.Entity<Customer>().Ignore(c => c.Details);
            modelBuilder.Entity<Order>().Ignore(c => c.Details);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Can_map_derived_of_owned_type_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Order>();
            modelBuilder.Ignore<SpecialOrder>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<OrderCombination>().OwnsOne(c => c.Details);

            IReadOnlyModel model = modelBuilder.Model;

            var owner = model.FindEntityType(typeof(OrderCombination));
            var owned = owner.FindNavigation(nameof(OrderCombination.Details)).ForeignKey.DeclaringEntityType;
            Assert.Empty(owned.GetDirectlyDerivedTypes());
            Assert.Null(owned.GetDiscriminatorPropertyName());
            var navToCustomerDetails = model.GetEntityTypes().SelectMany(e => e.GetDeclaredNavigations()).Where(
                n =>
                {
                    var targetType = n.TargetEntityType.ClrType;
                    return targetType != typeof(DetailsBase) && typeof(DetailsBase).IsAssignableFrom(targetType);
                }).Single();
            Assert.Single(owned.GetForeignKeys());
            Assert.Single(model.FindEntityTypes(typeof(DetailsBase)));
            var derivedType = model.FindEntityType(typeof(CustomerDetails));
            Assert.Null(derivedType.BaseType);
            Assert.Null(derivedType.GetDiscriminatorPropertyName());

            modelBuilder.Entity<Customer>().Ignore(c => c.Details);
            modelBuilder.Entity<Order>().Ignore(c => c.Details);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Can_configure_relationship_with_PK_ValueConverter()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<QueryResult>().Property(x => x.Id)
                .HasConversion(x => x.Id, x => new CustomId { Id = x });

            modelBuilder.Entity<ValueCategory>()
                .Property(x => x.Id)
                .HasConversion(x => x.Id, x => new CustomId { Id = x });

            modelBuilder.Entity<QueryResult>()
                .OwnsOne(q => q.Value)
                .Property(x => x.CategoryId)
                .HasConversion(x => x.Id, x => new CustomId { Id = x });

            var model = modelBuilder.FinalizeModel();

            var result = model.FindEntityType(typeof(QueryResult));
            Assert.Null(result.FindProperty("TempId"));

            var owned = result.GetDeclaredNavigations().Single().TargetEntityType;
            Assert.Null(owned.FindProperty("TempId"));

            var ownedPkProperty = owned.FindPrimaryKey().Properties.Single();
            Assert.NotNull(ownedPkProperty.GetValueConverter());

            var category = model.FindEntityType(typeof(ValueCategory));
            Assert.Null(category.FindProperty("TempId"));

            var barNavigation = owned.GetDeclaredNavigations().Single(n => !n.ForeignKey.IsOwnership);
            Assert.Same(category, barNavigation.TargetEntityType);
            var fkProperty = barNavigation.ForeignKey.Properties.Single();
            Assert.Equal("CategoryId", fkProperty.Name);

            Assert.Equal(3, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Throws_on_FK_matching_two_relationships()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<BookDetails>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Entity<Book>();
            modelBuilder.Entity<BookLabel>();

            Assert.Equal(
                CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                    nameof(BookLabel) + "." + nameof(BookLabel.Book),
                    nameof(Book) + "." + nameof(Book.Label),
                    nameof(BookLabel),
                    nameof(Book) + "." + nameof(Book.AlternateLabel),
                    "{'" + nameof(BookLabel.BookId) + "'}"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Can_configure_chained_ownerships()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, bb =>
                {
                    bb.Ignore(l => l.Book);
                    bb.OwnsOne(
                        l => l.AnotherBookLabel, ab =>
                        {
                            ab.Ignore(l => l.Book);
                            ab.OwnsOne(l => l.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .Ignore(s => s.BookLabel)
                                .HasIndex(l => l.BookId);
                        });
                    bb.OwnsOne(
                        l => l.SpecialBookLabel, sb =>
                        {
                            sb.Ignore(l => l.Book);
                            sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                        });
                });

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.AlternateLabel, bb =>
                {
                    bb.Ignore(l => l.Book);
                    bb.OwnsOne(
                        l => l.SpecialBookLabel, sb =>
                        {
                            sb.Ignore(l => l.Book);
                            sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                        });
                    bb.OwnsOne(
                        l => l.AnotherBookLabel, ab =>
                        {
                            ab.Ignore(l => l.Book);
                            ab.OwnsOne(l => l.SpecialBookLabel).Ignore(l => l.Book).Ignore(s => s.BookLabel);
                        });
                });

            modelBuilder.Entity<BookDetails>();

            var model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            VerifyOwnedBookLabelModel(model);
        }

        [ConditionalFact]
        public virtual void Can_configure_chained_ownerships_different_order()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, bb =>
                {
                    bb.OwnsOne(
                        l => l.AnotherBookLabel, ab =>
                        {
                            ab.OwnsOne(l => l.SpecialBookLabel).Ignore(s => s.BookLabel).Ignore(l => l.Book);
                            ab.Ignore(l => l.Book);
                        });
                    bb.Ignore(l => l.Book);
                });

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(
                        l => l.AnotherBookLabel, ab =>
                        {
                            ab.OwnsOne(l => l.SpecialBookLabel).Ignore(s => s.BookLabel).Ignore(l => l.Book);
                            ab.Ignore(l => l.Book);
                        });
                    bb.Ignore(l => l.Book);
                });

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.Label, bb =>
                {
                    bb.OwnsOne(
                        l => l.SpecialBookLabel, sb =>
                        {
                            sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                            sb.Ignore(l => l.Book);
                        });
                });

            modelBuilder.Entity<Book>().OwnsOne(
                b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(
                        l => l.SpecialBookLabel, sb =>
                        {
                            sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                            sb.Ignore(l => l.Book);
                        });
                });

            modelBuilder.Entity<BookDetails>();

            var model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            VerifyOwnedBookLabelModel(model);
        }

        [ConditionalFact]
        public virtual void Can_configure_hierarchy_with_reference_navigations_as_owned()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Owned<BookLabel>();
            modelBuilder.Entity<Book>();

            // SpecialBookLabel has an inverse to BookLabel making it ambiguous
            modelBuilder.Entity<Book>()
                .OwnsOne(
                    b => b.Label, lb =>
                    {
                        lb.Ignore(l => l.Book);

                        lb.OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .Ignore(l => l.BookLabel);
                        lb.OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book);
                    });

            modelBuilder.Entity<Book>()
                .OwnsOne(
                    b => b.AlternateLabel, al =>
                    {
                        al.Ignore(l => l.Book);

                        al.OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .Ignore(l => l.BookLabel);
                        al.OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book);
                    });

            modelBuilder.Entity<BookDetails>();

            var model = modelBuilder.FinalizeModel();

            VerifyOwnedBookLabelModel(model);
        }

        [ConditionalFact]
        public virtual void Can_configure_hierarchy_with_reference_navigations_as_owned_afterwards()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Book>();
            modelBuilder.Owned<BookLabel>();

            modelBuilder.Entity<Book>()
                .OwnsOne(
                    b => b.Label, lb =>
                    {
                        lb.Ignore(l => l.Book);

                        lb.OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .Ignore(l => l.BookLabel);
                        lb.OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book);
                    });

            modelBuilder.Entity<Book>()
                .OwnsOne(
                    b => b.AlternateLabel, al =>
                    {
                        al.Ignore(l => l.Book);

                        al.OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .Ignore(l => l.BookLabel);
                        al.OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.Book)
                            .OwnsOne(b => b.AnotherBookLabel)
                            .Ignore(l => l.Book);
                    });

            modelBuilder.Entity<BookDetails>();

            var model = modelBuilder.FinalizeModel();

            VerifyOwnedBookLabelModel(model);
        }

        protected virtual void VerifyOwnedBookLabelModel(IReadOnlyModel model)
        {
            var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
            var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;

            Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
            Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
            Assert.Null(bookOwnership1.DependentToPrincipal);
            Assert.Null(bookOwnership2.DependentToPrincipal);

            var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.AnotherBookLabel)).ForeignKey;
            var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.SpecialBookLabel)).ForeignKey;
            var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.AnotherBookLabel)).ForeignKey;
            var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.SpecialBookLabel)).ForeignKey;

            Assert.Null(bookLabel1Ownership1.DependentToPrincipal);
            Assert.Equal(nameof(SpecialBookLabel.BookLabel), bookLabel1Ownership2.DependentToPrincipal.Name);
            Assert.Null(bookLabel2Ownership1.DependentToPrincipal);
            Assert.Equal(nameof(SpecialBookLabel.BookLabel), bookLabel2Ownership2.DependentToPrincipal.Name);

            var bookLabel1Ownership1Subownership = bookLabel1Ownership1.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.SpecialBookLabel)).ForeignKey;
            var bookLabel1Ownership2Subownership = bookLabel1Ownership2.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.AnotherBookLabel)).ForeignKey;
            var bookLabel2Ownership1Subownership = bookLabel2Ownership1.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.SpecialBookLabel)).ForeignKey;
            var bookLabel2Ownership2Subownership = bookLabel2Ownership2.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.AnotherBookLabel)).ForeignKey;

            Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
            Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
            Assert.Single(bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel1Ownership1Subownership.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel1Ownership2Subownership.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership1Subownership.DeclaringEntityType.GetForeignKeys());
            Assert.Single(bookLabel2Ownership2Subownership.DeclaringEntityType.GetForeignKeys());
            Assert.Equal(nameof(SpecialBookLabel.AnotherBookLabel), bookLabel1Ownership1Subownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(AnotherBookLabel.SpecialBookLabel), bookLabel1Ownership2Subownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(SpecialBookLabel.AnotherBookLabel), bookLabel2Ownership1Subownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(AnotherBookLabel.SpecialBookLabel), bookLabel2Ownership2Subownership.DependentToPrincipal.Name);

            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
            Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));
        }

        [ConditionalFact]
        public virtual void Removing_ambiguous_inverse_allows_navigations_to_be_discovered()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Owned<BookLabel>();
            modelBuilder.Entity<Book>();

            modelBuilder.Entity<Book>()
                .OwnsOne(
                    b => b.AlternateLabel, al =>
                    {
                        al.OwnsOne(b => b.AnotherBookLabel)
                            .OwnsOne(b => b.SpecialBookLabel)
                            .Ignore(l => l.BookLabel);

                        al.OwnsOne(b => b.SpecialBookLabel)
                            .OwnsOne(b => b.AnotherBookLabel);
                    });

            modelBuilder.Entity<Book>().Ignore(b => b.Label);

            modelBuilder.Entity<BookDetails>();

            var model = modelBuilder.FinalizeModel();

            var bookOwnership = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
            Assert.Equal(nameof(BookLabel.Book), bookOwnership.DependentToPrincipal.Name);

            var bookLabelOwnership1 = bookOwnership.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.AnotherBookLabel)).ForeignKey;
            var bookLabelOwnership2 = bookOwnership.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.SpecialBookLabel)).ForeignKey;

            Assert.Null(bookLabelOwnership1.DependentToPrincipal);
            Assert.Equal(nameof(SpecialBookLabel.BookLabel), bookLabelOwnership2.DependentToPrincipal.Name);

            var bookLabel2Ownership1Subownership = bookLabelOwnership1.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.SpecialBookLabel)).ForeignKey;
            var bookLabel2Ownership2Subownership = bookLabelOwnership2.DeclaringEntityType.FindNavigation(
                nameof(BookLabel.AnotherBookLabel)).ForeignKey;

            Assert.NotNull(bookLabelOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.Book)));
            Assert.NotNull(bookLabelOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.Book)));
            Assert.NotNull(bookLabel2Ownership1Subownership.DeclaringEntityType.FindNavigation(nameof(BookLabel.Book)));
            Assert.NotNull(bookLabel2Ownership2Subownership.DeclaringEntityType.FindNavigation(nameof(BookLabel.Book)));
            Assert.Equal(nameof(SpecialBookLabel.AnotherBookLabel), bookLabel2Ownership1Subownership.DependentToPrincipal.Name);
            Assert.Equal(nameof(AnotherBookLabel.SpecialBookLabel), bookLabel2Ownership2Subownership.DependentToPrincipal.Name);

            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));
        }

        [ConditionalFact]
        public virtual void Can_configure_self_ownership()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Book>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Entity<BookLabel>().OwnsOne(l => l.AnotherBookLabel, ab => ab.OwnsOne(l => l.AnotherBookLabel));

            var model = modelBuilder.FinalizeModel();

            var bookLabelOwnership = model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(BookLabel.AnotherBookLabel))
                .ForeignKey;
            var selfOwnership = bookLabelOwnership.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
            Assert.NotSame(selfOwnership.PrincipalEntityType, selfOwnership.DeclaringEntityType);
            Assert.Equal(selfOwnership.PrincipalEntityType.ClrType, selfOwnership.DeclaringEntityType.ClrType);
            Assert.True(selfOwnership.IsOwnership);
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
            Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
        }

        [ConditionalTheory] // Issue #28091
        [InlineData(16, 2, 16, 4, 16, 4, 16, 4, 16, 4)]
        [InlineData(16, 2, 17, 4, 17, 4, 17, 4, 17, 4)]
        [InlineData(null, null, 16, 4, 16, 4, 16, 4, 16, 4)]
        [InlineData(null, null, 16, 4, 15, 3, 14, 2, 13, 1)]
        [InlineData(null, null, 16, null, 15, null, 14, null, 13, null)]
        [InlineData(17, null, 16, null, 15, null, 14, null, 13, null)]
        [InlineData(17, 5, 16, 4, 15, 3, 14, 2, 13, 1)]
        [InlineData(17, 5, null, null, null, null, null, null, null, null)]
        public virtual void Precision_and_scale_for_property_type_used_in_owned_types_can_be_overwritten(
            int? defaultPrecision,
            int? defaultScale,
            int? mainPrecision,
            int? mainScale,
            int? otherPrecision,
            int? otherScale,
            int? onePrecision,
            int? oneScale,
            int? manyPrecision,
            int? manyScale)
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    if (defaultPrecision.HasValue)
                    {
                        if (defaultScale.HasValue)
                        {
                            c.Properties<decimal>().HavePrecision(defaultPrecision.Value, defaultScale.Value);
                        }
                        else
                        {
                            c.Properties<decimal>().HavePrecision(defaultPrecision.Value);
                        }
                    }
                });

            modelBuilder.Entity<MainOtter>(
                b =>
                {
                    HasPrecision(b.Property(x => x.Number), mainPrecision, mainScale);
                    b.OwnsOne(
                        b => b.OwnedEntity, b =>
                        {
                            HasPrecision(b.Property(x => x.Number), onePrecision, oneScale);
                        });
                });

            modelBuilder.Entity<OtherOtter>(
                b =>
                {
                    HasPrecision(b.Property(x => x.Number), otherPrecision, otherScale);
                    b.OwnsMany(
                        b => b.OwnedEntities, b =>
                        {
                            HasPrecision(b.Property(x => x.Number), manyPrecision, manyScale);
                        });
                });

            var model = modelBuilder.FinalizeModel();

            var mainType = model.FindEntityType(typeof(MainOtter))!;
            var otherType = model.FindEntityType(typeof(OtherOtter))!;
            var oneType = model.FindEntityType(typeof(OwnedOtter), nameof(MainOtter.OwnedEntity), mainType)!;
            var manyType = model.FindEntityType(typeof(OwnedOtter), nameof(OtherOtter.OwnedEntities), otherType)!;

            Assert.Equal(mainPrecision ?? defaultPrecision, mainType.FindProperty(nameof(MainOtter.Number))!.GetPrecision());
            Assert.Equal(mainScale ?? defaultScale, mainType.FindProperty(nameof(MainOtter.Number))!.GetScale());

            Assert.Equal(otherPrecision ?? defaultPrecision, otherType.FindProperty(nameof(OtherOtter.Number))!.GetPrecision());
            Assert.Equal(otherScale ?? defaultScale, otherType.FindProperty(nameof(OtherOtter.Number))!.GetScale());

            Assert.Equal(onePrecision ?? defaultPrecision, oneType.FindProperty(nameof(OwnedOtter.Number))!.GetPrecision());
            Assert.Equal(oneScale ?? defaultScale, oneType.FindProperty(nameof(OwnedOtter.Number))!.GetScale());

            Assert.Equal(manyPrecision ?? defaultPrecision, manyType.FindProperty(nameof(OwnedOtter.Number))!.GetPrecision());
            Assert.Equal(manyScale ?? defaultScale, manyType.FindProperty(nameof(OwnedOtter.Number))!.GetScale());

            void HasPrecision(TestPropertyBuilder<decimal> testPropertyBuilder, int? precision, int? scale)
            {
                if (precision.HasValue)
                {
                    if (scale.HasValue)
                    {
                        testPropertyBuilder.HasPrecision(precision.Value, scale.Value);
                    }
                    else
                    {
                        testPropertyBuilder.HasPrecision(precision.Value);
                    }
                }
            }
        }

        protected class Department
        {
            public DepartmentId Id { get; protected set; }
            public List<DepartmentId> DepartmentIds { get; set; }
        }

        protected class DepartmentId
        {
            public DepartmentId() { }

            public DepartmentId(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        protected class Office
        {
            public int Id { get; protected set; }
            public List<DepartmentId> DepartmentIds { get; set; }
        }

        [ConditionalFact]
        public virtual void Can_configure_property_and_owned_entity_of_same_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Department>(b =>
            {
                b.Property(d => d.Id)
                    .HasConversion(
                        id => id.Value,
                        value => new DepartmentId(value));

                b.OwnsMany(d => d.DepartmentIds);
            });

            modelBuilder.Entity<Office>()
                .OwnsMany(o => o.DepartmentIds);

            var model = modelBuilder.FinalizeModel();

            var departmentType = model.FindEntityType(typeof(Department))!;
            var departmentNestedType = model.FindEntityType(typeof(DepartmentId), nameof(Department.DepartmentIds), departmentType)!;
            var officeType = model.FindEntityType(typeof(Office))!;
            var officeNestedType = model.FindEntityType(typeof(DepartmentId), nameof(Office.DepartmentIds), officeType)!;

            var departmentIdProperty = departmentType.FindProperty(nameof(Department.Id));
            Assert.NotNull(departmentIdProperty);
            Assert.NotNull(departmentNestedType);
            Assert.NotNull(officeNestedType);

            var departmentIdFkProperty = departmentNestedType.GetForeignKeys().Single().Properties[0];
            Assert.Same(departmentIdProperty.GetValueConverter(), departmentIdFkProperty.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Can_configure_owned_entity_and_property_of_same_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Office>()
                .OwnsMany(o => o.DepartmentIds);

            modelBuilder.Entity<Department>(b =>
            {
                b.Property(d => d.Id)
                    .HasConversion(
                        id => id.Value,
                        value => new DepartmentId(value));

                b.OwnsMany(d => d.DepartmentIds);
            });

            var model = modelBuilder.FinalizeModel();

            var departmentType = model.FindEntityType(typeof(Department))!;
            var departmentNestedType = model.FindEntityType(typeof(DepartmentId), nameof(Department.DepartmentIds), departmentType)!;
            var officeType = model.FindEntityType(typeof(Office))!;
            var officeNestedType = model.FindEntityType(typeof(DepartmentId), nameof(Office.DepartmentIds), officeType)!;

            var departmentIdProperty = departmentType.FindProperty(nameof(Department.Id));
            Assert.NotNull(departmentIdProperty);
            Assert.NotNull(departmentIdProperty.GetValueConverter());
            Assert.NotNull(departmentNestedType);
            Assert.NotNull(officeNestedType);

            var departmentIdFkProperty = departmentNestedType.GetForeignKeys().Single().Properties[0];
            Assert.Same(departmentIdProperty.GetValueConverter(), departmentIdFkProperty.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Reconfiguring_entity_type_as_owned_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            Assert.Equal(
                CoreStrings.ClashingNonOwnedEntityType(nameof(CustomerDetails)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details)).Message);
        }

        [ConditionalFact]
        public virtual void Reconfiguring_owned_type_as_non_owned_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Customer>();
            modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

            Assert.Equal(
                CoreStrings.ClashingOwnedEntityType(nameof(CustomerDetails)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<SpecialCustomer>().HasOne(c => c.Details)).Message);
        }

        [ConditionalFact]
        public virtual void Deriving_from_owned_type_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Book>()
                .Ignore(b => b.AlternateLabel)
                .Ignore(b => b.Details)
                .OwnsOne(
                    b => b.Label, lb =>
                    {
                        lb.Ignore(l => l.AnotherBookLabel);
                        lb.Ignore(l => l.SpecialBookLabel);
                    });

            Assert.Equal(
                modelBuilder.Model.IsShared(typeof(BookLabel))
                    ? CoreStrings.ClashingSharedType(nameof(BookLabel))
                    : CoreStrings.ClashingOwnedEntityType(nameof(BookLabel)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<AnotherBookLabel>().HasBaseType<BookLabel>()).Message);
        }

        [ConditionalFact]
        public virtual void Configuring_base_type_as_owned_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<AnotherBookLabel>();

            modelBuilder.Entity<Book>()
                .Ignore(b => b.AlternateLabel)
                .Ignore(b => b.Details);

            Assert.Equal(
                CoreStrings.ClashingNonOwnedDerivedEntityType(nameof(BookLabel), nameof(AnotherBookLabel)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Book>().OwnsOne(c => c.Label)).Message);
        }

        [ConditionalFact]
        public virtual void CLR_base_type_can_be_owned_when_not_in_hierarchy()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<AnotherBookLabel>()
                .HasBaseType(null)
                .Ignore(l => l.Book)
                .Ignore(l => l.SpecialBookLabel)
                .Ignore(l => l.AnotherBookLabel);

            modelBuilder.Entity<Book>()
                .Ignore(b => b.AlternateLabel)
                .Ignore(b => b.Details)
                .OwnsOne(
                    c => c.Label, lb =>
                    {
                        lb.Ignore(l => l.AnotherBookLabel);
                        lb.Ignore(l => l.SpecialBookLabel);
                    });

            var model = modelBuilder.FinalizeModel();

            var bookLabelOwnership = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label))
                .ForeignKey;

            Assert.True(bookLabelOwnership.IsOwnership);
            Assert.Equal(nameof(BookLabel.Book), bookLabelOwnership.DependentToPrincipal.Name);

            Assert.Null(model.FindEntityType(typeof(AnotherBookLabel)).BaseType);
        }

        [ConditionalFact]
        public virtual void OwnedType_can_derive_from_Collection()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<PrincipalEntity>().OwnsOne(o => o.InverseNav);

            var model = modelBuilder.FinalizeModel();

            Assert.Single(model.FindEntityTypes(typeof(List<DependentEntity>)));
        }

        [ConditionalFact]
        public virtual void Shared_type_entity_types_with_FK_to_another_entity_works()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Country>();
            var ownerEntityTypeBuilder = modelBuilder.Entity<BillingOwner>();
            ownerEntityTypeBuilder.OwnsOne(
                e => e.Bill1,
                o =>
                {
                    o.HasOne<Country>().WithMany().HasPrincipalKey(c => c.Name).HasForeignKey(d => d.Country);
                    o.HasIndex(c => c.Country);
                });

            ownerEntityTypeBuilder.OwnsOne(
                e => e.Bill2,
                o => o.HasOne<Country>().WithMany().HasPrincipalKey(c => c.Name).HasForeignKey(d => d.Country));

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(4, model.GetEntityTypes().Count());

            var owner = model.FindEntityType(typeof(BillingOwner));

            var bill1 = owner.FindNavigation(nameof(BillingOwner.Bill1)).TargetEntityType;
            Assert.Equal(2, bill1.GetForeignKeys().Count());
            Assert.Single(bill1.GetIndexes());

            var bill2 = owner.FindNavigation(nameof(BillingOwner.Bill2)).TargetEntityType;
            Assert.Equal(2, bill2.GetForeignKeys().Count());
            Assert.Single(bill2.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Inheritance_where_base_has_multiple_owned_types_works()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<BaseOwner>();
            modelBuilder.Entity<DerivedOwner>();

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(4, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Navigations_on_owned_type_can_set_access_mode_using_expressions()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<OneToOneNavPrincipalOwner>()
                .OwnsOne(
                    e => e.OwnedDependent,
                    a =>
                    {
                        a.WithOwner(owned => owned.OneToOneOwner);
                        a.Navigation(owned => owned.OneToOneOwner)
                            .UsePropertyAccessMode(PropertyAccessMode.Property);
                    });

            modelBuilder.Entity<OneToOneNavPrincipalOwner>()
                .Navigation(e => e.OwnedDependent)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            var principal = (IReadOnlyEntityType)model.FindEntityType(typeof(OneToOneNavPrincipalOwner));
            var dependent = (IReadOnlyEntityType)model.FindEntityType(typeof(OwnedNavDependent));

            Assert.Equal(PropertyAccessMode.Field, principal.FindNavigation("OwnedDependent").GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, dependent.FindNavigation("OneToOneOwner").GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Navigations_on_owned_type_collection_can_set_access_mode()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<OneToManyNavPrincipalOwner>()
                .OwnsMany(
                    e => e.OwnedDependents,
                    a =>
                    {
                        a.WithOwner(owned => owned.OneToManyOwner);
                        a.Navigation(owned => owned.OneToManyOwner)
                            .UsePropertyAccessMode(PropertyAccessMode.Property);
                    });

            modelBuilder.Entity<OneToManyNavPrincipalOwner>()
                .Navigation(e => e.OwnedDependents)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            var principal = (IReadOnlyEntityType)model.FindEntityType(typeof(OneToManyNavPrincipalOwner));
            var dependent = (IReadOnlyEntityType)model.FindEntityType(typeof(OwnedOneToManyNavDependent));

            Assert.Equal(PropertyAccessMode.Field, principal.FindNavigation("OwnedDependents").GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, dependent.FindNavigation("OneToManyOwner").GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Attempt_to_create_OwnsMany_on_a_reference_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.UnableToSetIsUnique(
                    false,
                    "OwnedDependent",
                    typeof(OneToOneNavPrincipalOwner).Name),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<OneToOneNavPrincipalOwner>()
                        .OwnsMany<OwnedNavDependent>("OwnedDependent")).Message
            );
        }

        [ConditionalFact]
        public virtual void Attempt_to_create_OwnsOne_on_a_collection_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.UnableToSetIsUnique(
                    true,
                    "OwnedDependents",
                    typeof(OneToManyNavPrincipalOwner).Name),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<OneToManyNavPrincipalOwner>()
                        .OwnsOne<OwnedOneToManyNavDependent>("OwnedDependents")).Message);
        }

        [ConditionalFact]
        public virtual void Shared_type_can_be_used_as_owned_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OwnerOfSharedType>(
                b =>
                {
                    b.OwnsOne(
                        "Shared1", e => e.Reference, sb =>
                        {
                            sb.IndexerProperty<int>("Value");
                        });
                    b.OwnsMany("Shared2", e => e.Collection).IndexerProperty<bool>("IsDeleted");
                    b.OwnsOne(
                        e => e.OwnedNavigation,
                        o =>
                        {
                            o.OwnsOne(
                                "Shared3", e => e.Reference, sb =>
                                {
                                    sb.IndexerProperty<int>("NestedValue");
                                });
                            o.OwnsMany("Shared4", e => e.Collection).IndexerProperty<long>("NestedLong");
                        });
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Collection(
                model.GetEntityTypes().OrderBy(e => e.Name),
                t => { Assert.Equal(typeof(NestedOwnerOfSharedType), t.ClrType); },
                t => { Assert.Equal(typeof(OwnerOfSharedType), t.ClrType); },
                t =>
                {
                    Assert.Equal("Shared1", t.Name);
                    Assert.NotNull(t.FindProperty("Value"));
                },
                t =>
                {
                    Assert.Equal("Shared2", t.Name);
                    Assert.NotNull(t.FindProperty("IsDeleted"));
                },
                t =>
                {
                    Assert.Equal("Shared3", t.Name);
                    Assert.NotNull(t.FindProperty("NestedValue"));
                },
                t =>
                {
                    Assert.Equal("Shared4", t.Name);
                    Assert.NotNull(t.FindProperty("NestedLong"));
                });
        }

        [ConditionalFact]
        public virtual void Shared_type_used_as_owned_type_throws_for_same_name()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OwnerOfSharedType>(
                b =>
                {
                    b.OwnsOne("Shared1", e => e.Reference);
                    b.OwnsOne("Shared1", e => e.Reference);

                    Assert.Equal(
                        CoreStrings.ClashingNamedOwnedType(
                            "Shared1", nameof(OwnerOfSharedType), nameof(OwnerOfSharedType.Collection)),
                        Assert.Throws<InvalidOperationException>(
                            () => b.OwnsMany("Shared1", e => e.Collection)).Message);
                });
        }

        [ConditionalFact]
        public virtual void PrimitiveCollectionBuilder_methods_can_be_chained()
            => CreateModelBuilder()
                .Entity<ComplexProperties>()
                .OwnsOne(e => e.CollectionQuarks)
                .PrimitiveCollection(e => e.Up)
                .ElementType(t => t
                    .HasAnnotation("B", "C")
                    .HasConversion(typeof(long))
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(typeof(long), typeof(CustomValueComparer<int>))
                    .HasConversion(typeof(long), new CustomValueComparer<int>())
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(new CastingConverter<int, long>(), new CustomValueComparer<int>())
                    .HasConversion<long>()
                    .HasConversion<long>(new CustomValueComparer<int>())
                    .HasConversion<long, CustomValueComparer<int>>()
                    .HasMaxLength(2)
                    .HasPrecision(1)
                    .HasPrecision(1, 2)
                    .IsRequired()
                    .IsUnicode())
                .IsRequired()
                .HasAnnotation("A", "V")
                .IsConcurrencyToken()
                .ValueGeneratedNever()
                .ValueGeneratedOnAdd()
                .ValueGeneratedOnAddOrUpdate()
                .ValueGeneratedOnUpdate()
                .IsUnicode()
                .HasMaxLength(100)
                .HasSentinel(null)
                .HasValueGenerator<CustomValueGenerator>()
                .HasValueGenerator(typeof(CustomValueGenerator))
                .HasValueGeneratorFactory<CustomValueGeneratorFactory>()
                .HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory))
                .IsRequired();

        [ConditionalFact]
        public virtual void PrimitiveCollectionBuilder_methods_can_be_chained_on_collection()
            => CreateModelBuilder()
                .Entity<Customer>()
                .OwnsMany(e => e.Orders)
                .PrimitiveCollection<List<int>>("List")
                .ElementType(t => t
                    .HasAnnotation("B", "C")
                    .HasConversion(typeof(long))
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(typeof(long), typeof(CustomValueComparer<int>))
                    .HasConversion(typeof(long), new CustomValueComparer<int>())
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(new CastingConverter<int, long>(), new CustomValueComparer<int>())
                    .HasConversion<long>()
                    .HasConversion<long>(new CustomValueComparer<int>())
                    .HasConversion<long, CustomValueComparer<int>>()
                    .HasMaxLength(2)
                    .HasPrecision(1)
                    .HasPrecision(1, 2)
                    .IsRequired()
                    .IsUnicode())
                .IsRequired()
                .HasAnnotation("A", "V")
                .IsConcurrencyToken()
                .ValueGeneratedNever()
                .ValueGeneratedOnAdd()
                .ValueGeneratedOnAddOrUpdate()
                .ValueGeneratedOnUpdate()
                .IsUnicode()
                .HasMaxLength(100)
                .HasSentinel(null)
                .HasValueGenerator<CustomValueGenerator>()
                .HasValueGenerator(typeof(CustomValueGenerator))
                .HasValueGeneratorFactory<CustomValueGeneratorFactory>()
                .HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory))
                .IsRequired();

        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry)
                => throw new NotImplementedException();

            public override bool GeneratesTemporaryValues
                => false;
        }

        private class CustomValueGeneratorFactory : ValueGeneratorFactory
        {
            public override ValueGenerator Create(IProperty property, ITypeBase entityType)
                => new CustomValueGenerator();
        }
        private class CustomValueComparer<T> : ValueComparer<T>
        {
            public CustomValueComparer()
                : base(false)
            {
            }
        }
    }
}
