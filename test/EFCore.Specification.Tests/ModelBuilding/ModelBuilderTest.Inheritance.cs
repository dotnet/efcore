// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

#nullable disable

public abstract partial class ModelBuilderTest
{
    public abstract class InheritanceTestBase(ModelBuilderFixtureBase fixture) : ModelBuilderTestBase(fixture)
    {
        [ConditionalFact]
        public virtual void Can_map_derived_types_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<Book>();

            modelBuilder.Entity<ExtraSpecialBookLabel>()
                .HasBaseType(null)
                .Property(b => b.BookId);

            modelBuilder.Entity<SpecialBookLabel>()
                .HasBaseType(null)
                .Ignore(b => b.BookLabel)
                .Ignore(b => b.BookId);

            modelBuilder.Entity<BookLabel>();

            modelBuilder.Entity<ExtraSpecialBookLabel>().HasBaseType<SpecialBookLabel>();

            modelBuilder.Entity<SpecialBookLabel>().HasBaseType<BookLabel>();

            modelBuilder.Entity<SpecialBookLabel>().Property(b => b.BookId);

            var model = modelBuilder.FinalizeModel();

            Assert.Empty(model.FindEntityType(typeof(ExtraSpecialBookLabel)).GetDeclaredProperties());
            Assert.Empty(model.FindEntityType(typeof(SpecialBookLabel)).GetDeclaredProperties());
            Assert.NotNull(model.FindEntityType(typeof(SpecialBookLabel)).FindProperty(nameof(BookLabel.BookId)));
        }

        [ConditionalFact]
        public virtual void Base_types_are_mapped_correctly_if_discovered_last()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<AnotherBookLabel>();
            modelBuilder.Ignore<Book>();
            modelBuilder.Entity<ExtraSpecialBookLabel>();
            modelBuilder.Entity<SpecialBookLabel>().Ignore(b => b.BookLabel);

            var model = modelBuilder.FinalizeModel();

            var moreDerived = model.FindEntityType(typeof(ExtraSpecialBookLabel));
            var derived = model.FindEntityType(typeof(SpecialBookLabel));
            var baseType = model.FindEntityType(typeof(BookLabel));

            Assert.Same(baseType, derived.BaseType);
            Assert.Same(derived, moreDerived.BaseType);
        }

        [ConditionalFact]
        public virtual void Can_specify_discriminator_without_derived_types()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Q>()
                .HasDiscriminator<string>("Discriminator");

            var model = modelBuilder.FinalizeModel();

            Assert.Equal("Q", model.FindEntityType(typeof(Q)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public virtual void Can_specify_discriminator_values_first()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<P>();

            modelBuilder.Entity<PBase>()
                .HasDiscriminator<int>("TypeDiscriminator")
                .HasValue<T>(1)
                .HasValue<Q>(2);

            modelBuilder.Entity<P>();

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(PBase)).GetDiscriminatorValue());
            Assert.Null(model.FindEntityType(typeof(P)).GetDiscriminatorValue());
            Assert.Equal(1, model.FindEntityType(typeof(T)).GetDiscriminatorValue());
            Assert.Equal(2, model.FindEntityType(typeof(Q)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public virtual void Can_map_derived_self_ref_many_to_one()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SelfRefManyToOneDerived>().HasData(
                new SelfRefManyToOneDerived { Id = 1, SelfRef1Id = 1 });
            modelBuilder.Entity<SelfRefManyToOne>();

            modelBuilder.FinalizeModel();

            var model = modelBuilder.Model;
            Assert.Empty(model.FindEntityType(typeof(SelfRefManyToOneDerived)).GetDeclaredProperties());
            var fk = model.FindEntityType(typeof(SelfRefManyToOne)).FindNavigation(nameof(SelfRefManyToOne.SelfRef1)).ForeignKey;
            Assert.Equal(nameof(SelfRefManyToOne.SelfRef2), fk.PrincipalToDependent.Name);
            Assert.Equal(nameof(SelfRefManyToOne.SelfRef1Id), fk.Properties.Single().Name);
            Assert.True(fk.IsRequired);
        }

        [ConditionalFact]
        public virtual void Can_set_and_remove_base_type()
        {
            var modelBuilder = CreateModelBuilder();

            var pickleBuilder = modelBuilder.Entity<Pickle>();
            pickleBuilder.HasOne(e => e.BigMak).WithMany(e => e.Pickles);
            var pickle = pickleBuilder.Metadata;
            modelBuilder.Entity<BigMak>().Ignore(b => b.Bun);

            Assert.Null(pickle.BaseType);
            var pickleClone = Clone(modelBuilder.Model).FindEntityType(pickle.Name);
            var initialProperties = pickleClone.GetProperties().ToList();
            var initialKeys = pickleClone.GetKeys().ToList();
            var initialIndexes = pickleClone.GetIndexes().ToList();
            var initialForeignKeys = pickleClone.GetForeignKeys().ToList();
            var initialReferencingForeignKeys = pickleClone.GetReferencingForeignKeys().ToList();

            pickleBuilder.HasBaseType<Ingredient>();
            var ingredientBuilder = modelBuilder.Entity<Ingredient>();
            var ingredient = ingredientBuilder.Metadata;

            Assert.Same(typeof(Ingredient), pickle.BaseType.ClrType);

            var actualProperties = pickle.GetProperties();
            Fixture.TestHelpers.ModelAsserter.AssertEqual(
                initialProperties.Where(p => p.Name != "Discriminator"),
                actualProperties.Where(p => p.Name != "Discriminator"));
            Assert.Equal(initialKeys, pickle.GetKeys(),
                (expected, actual) =>
                {
                    Fixture.TestHelpers.ModelAsserter.AssertEqual(
                        expected.Properties,
                        actual.Properties);

                    return true;
                });
            Fixture.TestHelpers.ModelAsserter.AssertEqual(
                initialIndexes.Single().Properties,
                pickle.GetIndexes().Single().Properties);
            Fixture.TestHelpers.ModelAsserter.AssertEqual(
                initialForeignKeys.Single().Properties,
                pickle.GetForeignKeys().Single().Properties);
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialReferencingForeignKeys, pickle.GetReferencingForeignKeys());

            pickleBuilder.HasBaseType(null);

            Assert.Null(pickle.BaseType);
            actualProperties = pickle.GetProperties();
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialProperties, actualProperties);
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialKeys, pickle.GetKeys());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialIndexes, pickle.GetIndexes());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialForeignKeys, pickle.GetForeignKeys());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialReferencingForeignKeys, pickle.GetReferencingForeignKeys());

            actualProperties = ingredient.GetProperties();
            Fixture.TestHelpers.ModelAsserter.AssertEqual(
                initialProperties.Where(p => p.Name != "Discriminator"),
                actualProperties.Where(p => p.Name != "Discriminator"));
            Assert.Equal(initialKeys, ingredient.GetKeys(),
                (expected, actual) =>
                {
                    Fixture.TestHelpers.ModelAsserter.AssertEqual(
                        expected.Properties,
                        actual.Properties);

                    return true;
                });
            Fixture.TestHelpers.ModelAsserter.AssertEqual(
                initialIndexes.Single().Properties,
                ingredient.GetIndexes().Single().Properties);
            Fixture.TestHelpers.ModelAsserter.AssertEqual(
                initialForeignKeys.Single().Properties,
                ingredient.GetForeignKeys().Single().Properties);
            Assert.Equal(initialReferencingForeignKeys.Count(), ingredient.GetReferencingForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Setting_base_type_to_null_fixes_relationships()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<BackOrder>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            principalEntityBuilder.Ignore(nameof(Customer.Orders));
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));

            Assert.Same(principalEntityBuilder.Metadata, derivedPrincipalEntityBuilder.Metadata.BaseType);
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);

            var fk = dependentEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.Same(principalEntityBuilder.Metadata, fk.PrincipalEntityType);
            var derivedFk = derivedPrincipalEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Null(derivedFk.DependentToPrincipal);
            Assert.Equal(nameof(SpecialCustomer.SpecialOrders), derivedFk.PrincipalToDependent.Name);
            Assert.Empty(derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations());
            Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());

            derivedDependentEntityBuilder.HasBaseType(null);

            modelBuilder.FinalizeModel();

            fk = dependentEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.Same(principalEntityBuilder.Metadata, fk.PrincipalEntityType);
            derivedFk = derivedPrincipalEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            var anotherDerivedFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.NotSame(derivedFk, anotherDerivedFk);
            Assert.Null(derivedFk.DependentToPrincipal);
            Assert.Equal(nameof(SpecialCustomer.SpecialOrders), derivedFk.PrincipalToDependent.Name);
            Assert.Equal(nameof(Order.Customer), anotherDerivedFk.DependentToPrincipal.Name);
            Assert.Null(anotherDerivedFk.PrincipalToDependent);
            Assert.Same(principalEntityBuilder.Metadata, anotherDerivedFk.PrincipalEntityType);
            Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());
        }

        [ConditionalFact]
        public virtual void Pulling_relationship_to_a_derived_type_creates_relationships_on_other_derived_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Product>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            principalEntityBuilder.Ignore(nameof(Customer.Orders));
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));
            var otherDerivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();
            otherDerivedDependentEntityBuilder.Ignore(nameof(BackOrder.SpecialOrder));

            Assert.Same(principalEntityBuilder.Metadata, derivedPrincipalEntityBuilder.Metadata.BaseType);
            Assert.Same(dependentEntityBuilder.Metadata, derivedDependentEntityBuilder.Metadata.BaseType);
            Assert.Same(dependentEntityBuilder.Metadata, otherDerivedDependentEntityBuilder.Metadata.BaseType);

            var fk = dependentEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.Same(principalEntityBuilder.Metadata, fk.PrincipalEntityType);
            var derivedFk = derivedPrincipalEntityBuilder.Metadata.GetNavigations().Single().ForeignKey;
            Assert.Null(derivedFk.DependentToPrincipal);
            Assert.Equal(nameof(SpecialCustomer.SpecialOrders), derivedFk.PrincipalToDependent.Name);
            Assert.Empty(derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations());
            Assert.Empty(otherDerivedDependentEntityBuilder.Metadata.GetDeclaredNavigations());
            Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());

            derivedPrincipalEntityBuilder
                .HasMany(e => e.SpecialOrders)
                .WithOne(e => (SpecialCustomer)e.Customer);

            var model = modelBuilder.FinalizeModel();

            var dependentEntityType = model.FindEntityType(dependentEntityBuilder.Metadata.Name);
            var derivedDependentEntityType = model.FindEntityType(derivedDependentEntityBuilder.Metadata.Name);
            var otherDerivedDependentEntityType = model.FindEntityType(otherDerivedDependentEntityBuilder.Metadata.Name);
            var derivedPrincipalEntityType = model.FindEntityType(derivedPrincipalEntityBuilder.Metadata.Name);

            Assert.Empty(dependentEntityType.GetForeignKeys());
            Assert.Empty(dependentEntityType.GetNavigations());
            var newFk = derivedDependentEntityType.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(SpecialCustomer.SpecialOrders), newFk.PrincipalToDependent.Name);
            Assert.Same(derivedPrincipalEntityType, newFk.PrincipalEntityType);
            Assert.Same(
                newFk.DependentToPrincipal,
                dependentEntityType.GetDerivedNavigations().Single(fk => fk.DeclaringEntityType == derivedDependentEntityType));
            Assert.Same(
                newFk, dependentEntityType.GetDerivedForeignKeys().Single(fk => fk.DeclaringEntityType == derivedDependentEntityType));
            Assert.Same(newFk, derivedDependentEntityType.GetDeclaredForeignKeys().Single());
            Assert.Same(
                newFk, derivedDependentEntityType.FindForeignKey(newFk.Properties, newFk.PrincipalKey, newFk.PrincipalEntityType));
            Assert.Same(
                newFk,
                derivedPrincipalEntityType.GetReferencingForeignKeys()
                    .Single(fk => fk.DeclaringEntityType == derivedDependentEntityType));
            Assert.Equal(
                derivedPrincipalEntityType.GetDeclaredReferencingForeignKeys(),
                derivedPrincipalEntityType.GetReferencingForeignKeys()
                    .Where(fk => fk.DeclaringEntityType == derivedDependentEntityType));
            var otherDerivedFk = otherDerivedDependentEntityType.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), otherDerivedFk.DependentToPrincipal.Name);
            Assert.Null(otherDerivedFk.PrincipalToDependent);
            Assert.Equal(nameof(Order.CustomerId), otherDerivedFk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Pulling_relationship_to_a_derived_type_reverted_creates_relationships_on_other_derived_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<Product>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            principalEntityBuilder.Ignore(nameof(Customer.Orders));
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));
            var otherDerivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();
            otherDerivedDependentEntityBuilder.Ignore(nameof(BackOrder.SpecialOrder));

            derivedDependentEntityBuilder
                .HasOne(e => (SpecialCustomer)e.Customer)
                .WithMany(e => e.SpecialOrders);

            modelBuilder.FinalizeModel();

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(dependentEntityBuilder.Metadata.GetNavigations());
            var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(SpecialCustomer.SpecialOrders), newFk.PrincipalToDependent.Name);
            Assert.Same(derivedPrincipalEntityBuilder.Metadata, newFk.PrincipalEntityType);
            var otherDerivedFk = otherDerivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), otherDerivedFk.DependentToPrincipal.Name);
            Assert.Null(otherDerivedFk.PrincipalToDependent);
            Assert.Equal(nameof(Order.CustomerId), otherDerivedFk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Can_match_navigation_to_derived_type_with_inverse_on_base()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            principalEntityBuilder.Ignore(nameof(Customer.Orders));
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));

            derivedDependentEntityBuilder
                .HasOne(e => e.Customer)
                .WithMany(e => e.SomeOrders);

            modelBuilder.FinalizeModel();

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(dependentEntityBuilder.Metadata.GetNavigations());
            var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(Customer.SomeOrders), newFk.PrincipalToDependent.Name);
            Assert.Same(principalEntityBuilder.Metadata, newFk.PrincipalEntityType);
        }

        [ConditionalFact]
        public virtual void Pulling_relationship_to_a_derived_type_many_to_one_creates_relationships_on_other_derived_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            dependentEntityBuilder.Ignore(nameof(Order.Customer));
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));
            var otherDerivedPrincipalEntityBuilder = modelBuilder.Entity<OtherCustomer>();

            derivedPrincipalEntityBuilder
                .HasMany(e => (IEnumerable<SpecialOrder>)e.Orders)
                .WithOne(e => e.SpecialCustomer);

            Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());
            var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(SpecialOrder.SpecialCustomer), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(SpecialCustomer.Orders), newFk.PrincipalToDependent.Name);
            Assert.Same(derivedPrincipalEntityBuilder.Metadata, newFk.PrincipalEntityType);
            var otherDerivedFk = otherDerivedPrincipalEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Null(otherDerivedFk.DependentToPrincipal);
            Assert.Equal(nameof(OtherCustomer.Orders), otherDerivedFk.PrincipalToDependent.Name);
        }

        [ConditionalFact]
        public virtual void Pulling_relationship_to_a_derived_type_one_to_one_creates_relationship_on_base()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<OrderDetails>();

            var principalEntityBuilder = modelBuilder.Entity<OrderCombination>();
            principalEntityBuilder.Ignore(nameof(OrderCombination.SpecialOrder));
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));

            principalEntityBuilder
                .HasOne(e => (SpecialOrder)e.Order)
                .WithOne(e => e.SpecialOrderCombination)
                .HasPrincipalKey<OrderCombination>(e => e.Id);

            Assert.Null(dependentEntityBuilder.Metadata.GetNavigations().Single().Inverse);
            var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(SpecialOrder.SpecialOrderCombination), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(OrderCombination.Order), newFk.PrincipalToDependent.Name);
            Assert.Same(derivedDependentEntityBuilder.Metadata, newFk.DeclaringEntityType);
        }

        [ConditionalFact]
        public virtual void Pulling_relationship_to_a_derived_type_one_to_one_with_fk_creates_relationship_on_base()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<OrderDetails>();

            var principalEntityBuilder = modelBuilder.Entity<OrderCombination>();
            principalEntityBuilder.Ignore(nameof(OrderCombination.SpecialOrder));
            principalEntityBuilder.Ignore(nameof(OrderCombination.Details));
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));

            principalEntityBuilder
                .HasOne(e => (SpecialOrder)e.Order)
                .WithOne()
                .HasForeignKey<SpecialOrder>(e => e.SpecialCustomerId);

            Assert.Null(dependentEntityBuilder.Metadata.GetNavigations().Single().Inverse);
            var newFk = principalEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Null(newFk.DependentToPrincipal);
            Assert.Equal(nameof(OrderCombination.Order), newFk.PrincipalToDependent.Name);
            Assert.Same(derivedDependentEntityBuilder.Metadata, newFk.DeclaringEntityType);
        }

        [ConditionalFact]
        public virtual void Pulling_relationship_to_a_derived_type_with_fk_creates_relationships_on_other_derived_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            principalEntityBuilder.Ignore(nameof(Customer.Orders));
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));
            var otherDerivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();
            otherDerivedDependentEntityBuilder.Ignore(nameof(BackOrder.SpecialOrder));

            derivedDependentEntityBuilder
                .HasOne(e => (SpecialCustomer)e.Customer)
                .WithMany()
                .HasForeignKey(e => e.SpecialCustomerId);

            Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(dependentEntityBuilder.Metadata.GetNavigations());
            var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
            Assert.Null(newFk.PrincipalToDependent);
            var otherDerivedFk = otherDerivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), otherDerivedFk.DependentToPrincipal.Name);
            Assert.Null(otherDerivedFk.PrincipalToDependent);
            Assert.Equal(nameof(Order.CustomerId), otherDerivedFk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Can_promote_shadow_fk_to_the_base_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<BackOrder>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            principalEntityBuilder.Ignore(nameof(Customer.Orders));
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            dependentEntityBuilder.Ignore(e => e.Customer);
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(e => e.ShippingAddress);

            dependentEntityBuilder
                .HasOne<SpecialCustomer>()
                .WithMany()
                .HasForeignKey(nameof(SpecialOrder.SpecialCustomerId));

            modelBuilder.FinalizeModel();

            var newFk = dependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
            Assert.NotEqual(newFk, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single());
            Assert.Null(newFk.DependentToPrincipal);
            Assert.Null(newFk.PrincipalToDependent);
            Assert.Equal(nameof(SpecialOrder.SpecialCustomerId), newFk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Removing_a_key_triggers_fk_discovery_on_derived_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<BackOrder>();
            modelBuilder.Ignore<OtherCustomer>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            dependentEntityBuilder.Ignore(nameof(Order.Customer));
            var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
            derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.ShippingAddress));
            dependentEntityBuilder.Property<int?>("SpecialCustomerId");

            derivedPrincipalEntityBuilder
                .HasMany(e => (IEnumerable<SpecialOrder>)e.Orders)
                .WithOne(e => e.SpecialCustomer);

            dependentEntityBuilder.HasKey("SpecialCustomerId");
            dependentEntityBuilder.HasKey(o => o.OrderId);
            dependentEntityBuilder.Ignore("SpecialCustomerId");
            derivedDependentEntityBuilder.Property(e => e.SpecialCustomerId);

            Assert.Null(dependentEntityBuilder.Metadata.FindProperty("SpecialCustomerId"));
            Assert.NotNull(derivedDependentEntityBuilder.Metadata.FindProperty("SpecialCustomerId"));
            Assert.Empty(principalEntityBuilder.Metadata.GetNavigations());
            var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
            Assert.Equal(nameof(SpecialOrder.SpecialCustomer), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(SpecialCustomer.Orders), newFk.PrincipalToDependent.Name);
            Assert.Same(derivedPrincipalEntityBuilder.Metadata, newFk.PrincipalEntityType);
        }

        [ConditionalFact]
        public virtual void Index_removed_when_covered_by_an_inherited_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<BackOrder>();
            modelBuilder.Ignore<SpecialCustomer>();
            modelBuilder.Ignore<Product>();

            var principalEntityBuilder = modelBuilder.Entity<Customer>();
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<OtherCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();

            var dependentEntityType = dependentEntityBuilder.Metadata;
            var derivedDependentEntityType = derivedDependentEntityBuilder.Metadata;

            Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

            principalEntityBuilder.HasMany(c => c.Orders).WithOne(o => o.Customer)
                .HasForeignKey(
                    o => new { o.CustomerId, o.AnotherCustomerId })
                .HasPrincipalKey(
                    c => new { c.Id, c.AlternateKey });

            Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

            derivedPrincipalEntityBuilder.HasMany<BackOrder>().WithOne()
                .HasForeignKey(
                    o => new { o.CustomerId })
                .HasPrincipalKey(
                    c => new { c.Id });

            var fk = dependentEntityType.GetForeignKeys().Single();
            Assert.Single(dependentEntityType.GetIndexes());
            Assert.False(dependentEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
            Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

            var backOrderClone = Clone(modelBuilder.Model).FindEntityType(derivedDependentEntityType.Name);
            var initialProperties = backOrderClone.GetProperties().ToList();
            var initialKeys = backOrderClone.GetKeys().ToList();
            var initialIndexes = backOrderClone.GetIndexes().ToList();
            var initialForeignKeys = backOrderClone.GetForeignKeys().ToList();

            derivedDependentEntityBuilder.HasBaseType(null);

            var derivedFk = derivedDependentEntityType.GetForeignKeys()
                .Single(foreignKey => foreignKey.PrincipalEntityType == derivedPrincipalEntityBuilder.Metadata);
            Assert.Equal(2, derivedDependentEntityType.GetIndexes().Count());
            Assert.False(derivedDependentEntityType.FindIndex(derivedFk.Properties).IsUnique);

            derivedDependentEntityBuilder.HasBaseType<Order>();

            fk = dependentEntityType.GetForeignKeys().Single();
            Assert.Single(dependentEntityType.GetIndexes());
            Assert.False(dependentEntityType.FindIndex(fk.Properties).IsUnique);

            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialProperties, derivedDependentEntityType.GetProperties());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialKeys, derivedDependentEntityType.GetKeys());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialIndexes, derivedDependentEntityType.GetIndexes());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialForeignKeys, derivedDependentEntityType.GetForeignKeys());

            principalEntityBuilder.HasOne<Order>().WithOne()
                .HasPrincipalKey<Customer>(
                    c => new { c.Id })
                .HasForeignKey<Order>(
                    o => new { o.CustomerId });

            modelBuilder.FinalizeModel();

            var (Level, _, Message, _, _) = modelBuilder.ModelLoggerFactory.Log.Single(e => e.Id == CoreEventId.RedundantIndexRemoved);
            Assert.Equal(LogLevel.Debug, Level);
            Assert.Equal(
                CoreResources.LogRedundantIndexRemoved(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    "{'CustomerId'}", nameof(Order), "{'CustomerId', 'AnotherCustomerId'}"), Message);

            fk = dependentEntityType.GetForeignKeys().Single(foreignKey => foreignKey.DependentToPrincipal == null);
            Assert.Equal(2, dependentEntityType.GetIndexes().Count());
            Assert.True(dependentEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());
        }

        [ConditionalFact]
        public virtual void Index_removed_when_covered_by_an_inherited_index()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<BackOrder>();
            modelBuilder.Ignore<SpecialCustomer>();
            modelBuilder.Ignore<Product>();

            modelBuilder.Entity<Customer>();
            var derivedPrincipalEntityBuilder = modelBuilder.Entity<OtherCustomer>();
            var dependentEntityBuilder = modelBuilder.Entity<Order>();
            var derivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();

            dependentEntityBuilder.HasIndex(
                    o => new { o.CustomerId, o.AnotherCustomerId })
                .IsUnique();

            derivedPrincipalEntityBuilder.HasMany<BackOrder>().WithOne()
                .HasPrincipalKey(
                    c => new { c.Id })
                .HasForeignKey(
                    o => new { o.CustomerId });

            var dependentEntityType = dependentEntityBuilder.Metadata;
            var derivedDependentEntityType = derivedDependentEntityBuilder.Metadata;
            var index = dependentEntityType.FindIndex(dependentEntityType.GetForeignKeys().Single().Properties);
            Assert.False(index.IsUnique);
            Assert.True(dependentEntityType.GetIndexes().Single(i => i != index).IsUnique);
            Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
            Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

            var backOrderClone = Clone(modelBuilder.Model).FindEntityType(derivedDependentEntityType.Name);
            var initialProperties = backOrderClone.GetProperties().ToList();
            var initialKeys = backOrderClone.GetKeys().ToList();
            var initialIndexes = backOrderClone.GetIndexes().ToList();
            var initialForeignKeys = backOrderClone.GetForeignKeys().ToList();

            derivedDependentEntityBuilder.HasBaseType(null);

            var derivedFk = derivedDependentEntityType.GetForeignKeys()
                .Single(foreignKey => foreignKey.PrincipalEntityType == derivedPrincipalEntityBuilder.Metadata);
            Assert.Equal(2, derivedDependentEntityType.GetIndexes().Count());
            Assert.False(derivedDependentEntityType.FindIndex(derivedFk.Properties).IsUnique);

            derivedDependentEntityBuilder.HasBaseType<Order>();

            var baseFK = dependentEntityType.GetForeignKeys().Single();
            var baseIndex = dependentEntityType.FindIndex(baseFK.Properties);
            Assert.False(baseIndex.IsUnique);
            Assert.True(dependentEntityType.GetIndexes().Single(i => i != baseIndex).IsUnique);
            Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
            Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialProperties, derivedDependentEntityType.GetProperties());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialKeys, derivedDependentEntityType.GetKeys());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialIndexes, derivedDependentEntityType.GetIndexes());
            Fixture.TestHelpers.ModelAsserter.AssertEqual(initialForeignKeys, derivedDependentEntityType.GetForeignKeys());

            modelBuilder.FinalizeModel();

            var indexRemoveMessage =
                CoreResources.LogRedundantIndexRemoved(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    "{'CustomerId'}", nameof(Order), "{'CustomerId', 'AnotherCustomerId'}");
            Assert.Equal(1, modelBuilder.ModelLoggerFactory.Log.Count(l => l.Message == indexRemoveMessage));
        }

        [ConditionalFact]
        public virtual void Setting_base_type_handles_require_value_generator_properly()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Entity<SpecialOrder>();

            var fkProperty = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).FindProperty(OrderDetails.OrderIdProperty);
            Assert.Equal(ValueGenerated.Never, fkProperty.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_between_base_type_and_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            var relationshipBuilder = modelBuilder.Entity<BookLabel>()
                .HasOne(e => e.SpecialBookLabel)
                .WithOne(e => e.BookLabel)
                .HasPrincipalKey<SpecialBookLabel>(e => e.Id);

            Assert.NotNull(relationshipBuilder);
            Assert.Equal(typeof(BookLabel), relationshipBuilder.Metadata.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(SpecialBookLabel), relationshipBuilder.Metadata.PrincipalEntityType.ClrType);
            Assert.Equal(nameof(BookLabel.SpecialBookLabel), relationshipBuilder.Metadata.DependentToPrincipal.Name);
            Assert.Equal(nameof(SpecialBookLabel.BookLabel), relationshipBuilder.Metadata.PrincipalToDependent.Name);
            Assert.Equal(nameof(SpecialBookLabel.Id), relationshipBuilder.Metadata.PrincipalKey.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Removing_derived_removes_it_from_directly_derived_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Book>();
            modelBuilder.Entity<BookLabel>();
            modelBuilder.Ignore<SpecialBookLabel>();
            modelBuilder.Ignore<AnotherBookLabel>();

            Assert.Empty(modelBuilder.Model.FindEntityType(typeof(BookLabel).FullName).GetDirectlyDerivedTypes());
        }

        [ConditionalFact]
        public virtual void Can_ignore_base_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SpecialBookLabel>();
            modelBuilder.Entity<AnotherBookLabel>();
            modelBuilder.Entity<Book>().HasOne<SpecialBookLabel>().WithOne().HasForeignKey<Book>();
            modelBuilder.Ignore<BookLabel>();

            var model = modelBuilder.Model;
            Assert.Null(model.FindEntityType(typeof(BookLabel).FullName));
            foreach (var entityType in model.GetEntityTypes())
            {
                Assert.Empty(
                    entityType.GetForeignKeys()
                        .Where(fk => fk.PrincipalEntityType.ClrType == typeof(BookLabel)));
                Assert.Empty(
                    entityType.GetForeignKeys()
                        .Where(fk => fk.PrincipalKey.DeclaringEntityType.ClrType == typeof(BookLabel)));
            }
        }

        [ConditionalFact]
        public virtual void Relationships_are_discovered_on_the_base_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SpecialBookLabel>();
            modelBuilder.Entity<AnotherBookLabel>();

            var bookLabel = modelBuilder.Model.FindEntityType(typeof(BookLabel));
            var specialNavigation = bookLabel.GetDeclaredNavigations().Single(n => n.Name == nameof(BookLabel.SpecialBookLabel));
            Assert.Equal(typeof(SpecialBookLabel), specialNavigation.TargetEntityType.ClrType);
            Assert.Equal(nameof(SpecialBookLabel.BookLabel), specialNavigation.Inverse.Name);
            var anotherNavigation = bookLabel.GetDeclaredNavigations().Single(n => n.Name == nameof(BookLabel.AnotherBookLabel));
            Assert.Equal(typeof(AnotherBookLabel), anotherNavigation.TargetEntityType.ClrType);
            Assert.Null(anotherNavigation.Inverse);
        }

        [ConditionalFact]
        public virtual void Can_reconfigure_inherited_intraHierarchical_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Book>();
            var bookLabelEntityBuilder = modelBuilder.Entity<BookLabel>();
            bookLabelEntityBuilder.Ignore(e => e.AnotherBookLabel);
            bookLabelEntityBuilder.HasOne(e => e.SpecialBookLabel)
                .WithOne(e => e.BookLabel)
                .HasPrincipalKey<BookLabel>(e => e.Id);

            var extraSpecialBookLabelEntityBuilder = modelBuilder.Entity<ExtraSpecialBookLabel>();
            modelBuilder.Entity<SpecialBookLabel>()
                .HasOne(e => (ExtraSpecialBookLabel)e.SpecialBookLabel)
                .WithOne(e => (SpecialBookLabel)e.BookLabel)
                .HasForeignKey<ExtraSpecialBookLabel>();

            var fk = bookLabelEntityBuilder.Metadata.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
            Assert.Equal(new[] { fk }, extraSpecialBookLabelEntityBuilder.Metadata.GetForeignKeys());
            Assert.Equal(nameof(SpecialBookLabel.BookLabel), fk.DependentToPrincipal.Name);
            Assert.Equal(new[] { fk }, extraSpecialBookLabelEntityBuilder.Metadata.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<PersonBaseViewModel>();

            Assert.Empty(modelBuilder.Model.FindEntityType(typeof(PersonBaseViewModel)).GetForeignKeys());

            var citizen = modelBuilder.Model.FindEntityType(typeof(CitizenViewModel));
            var citizenNavigation = citizen.GetDeclaredNavigations().Single(n => n.Name == nameof(CitizenViewModel.CityVM));
            Assert.Equal(nameof(CityViewModel.People), citizenNavigation.Inverse.Name);

            var doctor = modelBuilder.Model.FindEntityType(typeof(DoctorViewModel));
            var doctorNavigation = doctor.GetDeclaredNavigations().Single(n => n.Name == nameof(CitizenViewModel.CityVM));
            Assert.Equal(nameof(CityViewModel.Medics), doctorNavigation.Inverse.Name);

            var police = modelBuilder.Model.FindEntityType(typeof(PoliceViewModel));
            var policeNavigation = police.GetDeclaredNavigations().Single(n => n.Name == nameof(CitizenViewModel.CityVM));
            Assert.Equal(nameof(CityViewModel.Police), policeNavigation.Inverse.Name);

            Assert.Empty(modelBuilder.Model.FindEntityType(typeof(CityViewModel)).GetForeignKeys());

            modelBuilder.Entity<CityViewModel>(
                c =>
                {
                    c.Ignore(c => c.CustomValues);
                });

            Assert.Null(modelBuilder.Model.FindEntityType(typeof(Dictionary<string, string>)));

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void
            Can_remove_objects_in_derived_type_which_was_set_using_data_annotation_while_setting_base_type_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            var derivedEntityType = (EntityType)modelBuilder.Entity<DerivedTypeWithKeyAnnotation>().Metadata;
            var baseEntityType = (EntityType)modelBuilder.Entity<BaseTypeWithKeyAnnotation>().Metadata;

            Assert.Equal(baseEntityType, derivedEntityType.BaseType);
            Assert.Equal(ConfigurationSource.DataAnnotation, baseEntityType.GetPrimaryKeyConfigurationSource());
            Assert.Equal(
                ConfigurationSource.DataAnnotation,
                baseEntityType.FindNavigation(nameof(BaseTypeWithKeyAnnotation.Navigation)).ForeignKey.GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, derivedEntityType.GetBaseTypeConfigurationSource());
        }

        [ConditionalFact]
        public virtual void Cannot_remove_objects_in_derived_type_which_was_set_using_explicit_while_setting_base_type_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            var derivedEntityTypeBuilder = modelBuilder.Entity<DerivedTypeWithKeyAnnotation>();
            derivedEntityTypeBuilder.HasKey(e => e.MyPrimaryKey);
            derivedEntityTypeBuilder.HasOne(e => e.Navigation).WithOne()
                .HasForeignKey<DerivedTypeWithKeyAnnotation>(e => e.MyPrimaryKey);
            var derivedEntityType = (EntityType)derivedEntityTypeBuilder.Metadata;
            var baseEntityType = (EntityType)modelBuilder.Entity<BaseTypeWithKeyAnnotation>().Metadata;

            Assert.Null(derivedEntityType.BaseType);
            Assert.Equal(ConfigurationSource.DataAnnotation, baseEntityType.GetPrimaryKeyConfigurationSource());
            Assert.Equal(
                ConfigurationSource.DataAnnotation,
                baseEntityType.FindNavigation(nameof(BaseTypeWithKeyAnnotation.Navigation)).ForeignKey.GetConfigurationSource());
            Assert.Equal(
                ConfigurationSource.Explicit,
                derivedEntityType.FindNavigation(nameof(DerivedTypeWithKeyAnnotation.Navigation)).ForeignKey.GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Explicit, derivedEntityType.GetPrimaryKeyConfigurationSource());
        }

        [ConditionalFact]
        public virtual void Ordering_of_entityType_discovery_does_not_affect_key_convention()
        {
            var modelBuilder = CreateModelBuilder();

            var baseEntity = modelBuilder.Entity<StringIdBase>().Metadata;
            var derivedEntity = modelBuilder.Entity<StringIdDerived>().Metadata;

            Assert.Equal(baseEntity, derivedEntity.BaseType);
            Assert.NotNull(baseEntity.FindPrimaryKey());

            var modelBuilder2 = CreateModelBuilder();
            var derivedEntity2 = modelBuilder2.Entity<StringIdDerived>().Metadata;
            var baseEntity2 = modelBuilder2.Entity<StringIdBase>().Metadata;

            Assert.Equal(baseEntity2, derivedEntity2.BaseType);
            Assert.NotNull(baseEntity2.FindPrimaryKey());
        }

        [ConditionalFact] // #7049
        public virtual void Base_type_can_be_discovered_after_creating_foreign_keys_on_derived()
        {
            var mb = CreateModelBuilder();
            mb.Entity<AL>();
            mb.Entity<L>();

            Assert.Equal(ValueGenerated.OnAdd, mb.Model.FindEntityType(typeof(Q)).FindProperty(nameof(Q.ID)).ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Can_get_set_discriminator_mapping_is_complete()
        {
            var mb = CreateModelBuilder();
            var baseTypeBuilder = mb.Entity<PBase>();
            var derivedTypeBuilder = mb.Entity<Q>();

            Assert.True(baseTypeBuilder.Metadata.GetIsDiscriminatorMappingComplete());

            baseTypeBuilder.HasDiscriminator<string>("Discriminator").IsComplete(false);
            Assert.False(baseTypeBuilder.Metadata.GetIsDiscriminatorMappingComplete());
            Assert.True(derivedTypeBuilder.Metadata.GetIsDiscriminatorMappingComplete());

            derivedTypeBuilder.HasDiscriminator<string>("Discriminator").IsComplete(true);
            Assert.False(baseTypeBuilder.Metadata.GetIsDiscriminatorMappingComplete());
            Assert.True(derivedTypeBuilder.Metadata.GetIsDiscriminatorMappingComplete());
        }

        protected class L
        {
            public int Id { get; set; }
            public IList<T> Ts { get; set; }
        }

        protected class T : P
        {
            public Q D { get; set; }
            public P P { get; set; }
            public Q F { get; set; }
        }

        protected abstract class P : PBase;

        protected class Q : PBase;

        protected abstract class PBase
        {
            public int ID { get; set; }
            public string Stuff { get; set; }
        }

        protected class AL
        {
            public int Id { get; set; }
            public PBase L { get; set; }
        }
    }
}
