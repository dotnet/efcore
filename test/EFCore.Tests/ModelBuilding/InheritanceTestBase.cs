// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class InheritanceTestBase : ModelBuilderTestBase
        {
            [Fact]
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

                modelBuilder.Validate();

                var model = modelBuilder.Model;
                Assert.Equal(0, model.FindEntityType(typeof(ExtraSpecialBookLabel)).GetDeclaredProperties().Count());
                Assert.Equal(0, model.FindEntityType(typeof(SpecialBookLabel)).GetDeclaredProperties().Count());
                Assert.NotNull(model.FindEntityType(typeof(SpecialBookLabel)).FindProperty(nameof(BookLabel.BookId)));
            }

            [Fact]
            public virtual void Base_types_are_mapped_correctly_if_discovered_last()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<AnotherBookLabel>();
                modelBuilder.Ignore<Book>();
                modelBuilder.Entity<ExtraSpecialBookLabel>();
                modelBuilder.Entity<SpecialBookLabel>().Ignore(b => b.BookLabel);

                modelBuilder.Validate();

                var model = modelBuilder.Model;
                var moreDerived = model.FindEntityType(typeof(ExtraSpecialBookLabel));
                var derived = model.FindEntityType(typeof(SpecialBookLabel));
                var baseType = model.FindEntityType(typeof(BookLabel));

                Assert.Same(baseType, derived.BaseType);
                Assert.Same(derived, moreDerived.BaseType);
            }

            [Fact]
            public virtual void Can_map_derived_self_ref_many_to_one()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<SelfRefManyToOneDerived>();
                modelBuilder.Entity<SelfRefManyToOne>();

                modelBuilder.Validate();

                var model = modelBuilder.Model;
                Assert.Equal(0, model.FindEntityType(typeof(SelfRefManyToOneDerived)).GetDeclaredProperties().Count());
                Assert.NotNull(model.FindEntityType(typeof(SelfRefManyToOne)).FindNavigation(nameof(SelfRefManyToOne.SelfRef1)));
                Assert.NotNull(model.FindEntityType(typeof(SelfRefManyToOne)).FindNavigation(nameof(SelfRefManyToOne.SelfRef2)));
            }

            [Fact]
            public virtual void Can_set_and_remove_base_type()
            {
                var modelBuilder = CreateModelBuilder();

                var pickleBuilder = modelBuilder.Entity<Pickle>();
                pickleBuilder.HasOne(e => e.BigMak).WithMany(e => e.Pickles);
                var pickle = pickleBuilder.Metadata;
                modelBuilder.Entity<BigMak>().Ignore(b => b.Bun);

                Assert.Null(pickle.BaseType);
                var pickleClone = modelBuilder.Model.Clone().FindEntityType(pickle.Name);
                var initialProperties = pickleClone.GetProperties().ToList();
                var initialKeys = pickleClone.GetKeys().ToList();
                var initialIndexes = pickleClone.GetIndexes().ToList();
                var initialForeignKeys = pickleClone.GetForeignKeys().ToList();
                var initialReferencingForeignKeys = pickleClone.GetReferencingForeignKeys().ToList();

                pickleBuilder.HasBaseType<Ingredient>();
                var ingredientBuilder = modelBuilder.Entity<Ingredient>();
                var ingredient = ingredientBuilder.Metadata;

                Assert.Same(typeof(Ingredient), pickle.BaseType.ClrType);
                AssertEqual(initialProperties, pickle.GetProperties().Where(p => p.Name != "Discriminator"), new PropertyComparer(compareAnnotations: false));
                AssertEqual(initialKeys, pickle.GetKeys());
                AssertEqual(initialIndexes, pickle.GetIndexes());
                AssertEqual(initialForeignKeys, pickle.GetForeignKeys());
                AssertEqual(initialReferencingForeignKeys, pickle.GetReferencingForeignKeys());

                pickleBuilder.HasBaseType(null);

                Assert.Null(pickle.BaseType);
                AssertEqual(initialProperties, pickle.GetProperties(), new PropertyComparer(compareAnnotations: false));
                AssertEqual(initialKeys, pickle.GetKeys());
                AssertEqual(initialIndexes, pickle.GetIndexes());
                AssertEqual(initialForeignKeys, pickle.GetForeignKeys());
                AssertEqual(initialReferencingForeignKeys, pickle.GetReferencingForeignKeys());

                AssertEqual(initialProperties, ingredient.GetProperties().Where(p => p.Name != "Discriminator"), new PropertyComparer(compareAnnotations: false));
                AssertEqual(initialKeys, ingredient.GetKeys());
                AssertEqual(initialIndexes, ingredient.GetIndexes());
                Assert.Equal(initialForeignKeys.Count(), ingredient.GetForeignKeys().Count());
                Assert.Equal(initialReferencingForeignKeys.Count(), ingredient.GetReferencingForeignKeys().Count());
            }

            [Fact]
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

            [Fact]
            public virtual void Pulling_relationship_to_a_derived_type_creates_relationships_on_other_derived_types()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<OrderDetails>();

                var principalEntityBuilder = modelBuilder.Entity<Customer>();
                principalEntityBuilder.Ignore(nameof(Customer.Orders));
                var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
                var dependentEntityBuilder = modelBuilder.Entity<Order>();
                var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
                derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
                derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
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

            [Fact]
            public virtual void Pulling_relationship_to_a_derived_type_reverted_creates_relationships_on_other_derived_types()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<OrderDetails>();

                var principalEntityBuilder = modelBuilder.Entity<Customer>();
                principalEntityBuilder.Ignore(nameof(Customer.Orders));
                var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
                var dependentEntityBuilder = modelBuilder.Entity<Order>();
                var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();
                derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.BackOrder));
                derivedDependentEntityBuilder.Ignore(nameof(SpecialOrder.SpecialCustomer));
                var otherDerivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();
                otherDerivedDependentEntityBuilder.Ignore(nameof(BackOrder.SpecialOrder));

                derivedDependentEntityBuilder
                    .HasOne(e => (SpecialCustomer)e.Customer)
                    .WithMany(e => e.SpecialOrders);

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

            [Fact]
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

            [Fact]
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

                principalEntityBuilder
                    .HasOne(e => (SpecialOrder)e.Order)
                    .WithOne(e => e.SpecialOrderCombination)
                    .HasPrincipalKey<OrderCombination>(e => e.Id);

                Assert.Null(dependentEntityBuilder.Metadata.GetNavigations().Single().FindInverse());
                var newFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
                Assert.Equal(nameof(SpecialOrder.SpecialOrderCombination), newFk.DependentToPrincipal.Name);
                Assert.Equal(nameof(OrderCombination.Order), newFk.PrincipalToDependent.Name);
                Assert.Same(derivedDependentEntityBuilder.Metadata, newFk.DeclaringEntityType);
            }

            [Fact]
            public virtual void Pulling_relationship_to_a_derived_type_one_to_one_with_fk_creates_relationship_on_base()
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

                principalEntityBuilder
                    .HasOne(e => (SpecialOrder)e.Order)
                    .WithOne()
                    .HasForeignKey<SpecialOrder>(e => e.SpecialCustomerId);

                Assert.Null(dependentEntityBuilder.Metadata.GetNavigations().Single().FindInverse());
                var newFk = principalEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
                Assert.Null(newFk.DependentToPrincipal);
                Assert.Equal(nameof(OrderCombination.Order), newFk.PrincipalToDependent.Name);
                Assert.Same(derivedDependentEntityBuilder.Metadata, newFk.DeclaringEntityType);
            }

            [Fact]
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

            [Fact]
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
                derivedDependentEntityBuilder.Ignore(e => e.SpecialCustomerId);

                dependentEntityBuilder
                    .HasOne<SpecialCustomer>()
                    .WithMany()
                    .HasForeignKey(nameof(SpecialOrder.SpecialCustomerId));

                var newFk = dependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single();
                Assert.NotEqual(newFk, derivedDependentEntityBuilder.Metadata.GetDeclaredForeignKeys().Single());
                Assert.Null(newFk.DependentToPrincipal);
                Assert.Null(newFk.PrincipalToDependent);
                Assert.Equal(nameof(SpecialOrder.SpecialCustomerId), newFk.Properties.Single().Name);
            }

            [Fact]
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

            [Fact]
            public virtual void Index_removed_when_covered_by_an_inherited_foreign_key()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<BackOrder>();
                modelBuilder.Ignore<SpecialCustomer>();
                modelBuilder.Ignore<SpecialOrder>();

                var principalEntityBuilder = modelBuilder.Entity<Customer>();
                var derivedPrincipalEntityBuilder = modelBuilder.Entity<OtherCustomer>();
                var dependentEntityBuilder = modelBuilder.Entity<Order>();
                var derivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();

                principalEntityBuilder.HasMany(c => c.Orders).WithOne(o => o.Customer)
                    .HasForeignKey(o => new { o.CustomerId, o.AnotherCustomerId })
                    .HasPrincipalKey(c => new { c.Id, c.AlternateKey });

                derivedPrincipalEntityBuilder.HasMany<BackOrder>().WithOne()
                    .HasForeignKey(o => new { o.CustomerId })
                    .HasPrincipalKey(c => new { c.Id });

                var dependentEntityType = dependentEntityBuilder.Metadata;
                var derivedDependentEntityType = derivedDependentEntityBuilder.Metadata;
                var fk = dependentEntityType.GetForeignKeys().Single();
                Assert.Equal(1, dependentEntityType.GetIndexes().Count());
                Assert.False(dependentEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
                Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

                var backOrderClone = modelBuilder.Model.Clone().FindEntityType(derivedDependentEntityType.Name);
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
                Assert.Equal(1, dependentEntityType.GetIndexes().Count());
                Assert.False(dependentEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
                Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

                AssertEqual(initialProperties, derivedDependentEntityType.GetProperties());
                AssertEqual(initialKeys, derivedDependentEntityType.GetKeys());
                AssertEqual(initialIndexes, derivedDependentEntityType.GetIndexes());
                AssertEqual(initialForeignKeys, derivedDependentEntityType.GetForeignKeys());

                Assert.Equal(1, modelBuilder.Log.Count);
                Assert.Equal(LogLevel.Information, modelBuilder.Log[0].Level);
                Assert.Equal(CoreStrings.LogRedundantIndexRemoved.GenerateMessage("{'CustomerId'}", "{'CustomerId', 'AnotherCustomerId'}"), modelBuilder.Log[0].Message);

                principalEntityBuilder.HasOne<Order>().WithOne()
                    .HasPrincipalKey<Customer>(c => new { c.Id })
                    .HasForeignKey<Order>(o => new { o.CustomerId });

                fk = dependentEntityType.GetForeignKeys().Single(foreignKey => foreignKey.DependentToPrincipal == null);
                Assert.Equal(2, dependentEntityType.GetIndexes().Count());
                Assert.True(dependentEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());
            }

            [Fact]
            public virtual void Index_removed_when_covered_by_an_inherited_index()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<BackOrder>();
                modelBuilder.Ignore<SpecialCustomer>();
                modelBuilder.Ignore<SpecialOrder>();

                var principalEntityBuilder = modelBuilder.Entity<Customer>();
                var derivedPrincipalEntityBuilder = modelBuilder.Entity<OtherCustomer>();
                var dependentEntityBuilder = modelBuilder.Entity<Order>();
                var derivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();

                dependentEntityBuilder.HasIndex(o => new { o.CustomerId, o.AnotherCustomerId })
                    .IsUnique();

                derivedPrincipalEntityBuilder.HasMany<BackOrder>().WithOne()
                    .HasPrincipalKey(c => new { c.Id })
                    .HasForeignKey(o => new { o.CustomerId });

                modelBuilder.Validate();

                var dependentEntityType = dependentEntityBuilder.Metadata;
                var derivedDependentEntityType = derivedDependentEntityBuilder.Metadata;
                var index = dependentEntityType.FindIndex(dependentEntityType.GetForeignKeys().Single().Properties);
                Assert.False(index.IsUnique);
                Assert.True(dependentEntityType.GetIndexes().Single(i => i != index).IsUnique);
                Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
                Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

                var backOrderClone = modelBuilder.Model.Clone().FindEntityType(derivedDependentEntityType.Name);
                var initialProperties = backOrderClone.GetProperties().ToList();
                var initialKeys = backOrderClone.GetKeys().ToList();
                var initialIndexes = backOrderClone.GetIndexes().ToList();
                var initialForeignKeys = backOrderClone.GetForeignKeys().ToList();

                var indexRemoveMessage =
                    CoreStrings.LogRedundantIndexRemoved.GenerateMessage("{'CustomerId'}", "{'CustomerId', 'AnotherCustomerId'}");
                Assert.Equal(1, modelBuilder.Log.Count(l => l.Message == indexRemoveMessage));

                derivedDependentEntityBuilder.HasBaseType(null);

                var derivedFk = derivedDependentEntityType.GetForeignKeys()
                    .Single(foreignKey => foreignKey.PrincipalEntityType == derivedPrincipalEntityBuilder.Metadata);
                Assert.Equal(2, derivedDependentEntityType.GetIndexes().Count());
                Assert.False(derivedDependentEntityType.FindIndex(derivedFk.Properties).IsUnique);

                derivedDependentEntityBuilder.HasBaseType<Order>();
                modelBuilder.Validate();

                var baseFK = dependentEntityType.GetForeignKeys().Single();
                var baseIndex = dependentEntityType.FindIndex(baseFK.Properties);
                Assert.False(baseIndex.IsUnique);
                Assert.True(dependentEntityType.GetIndexes().Single(i => i != baseIndex).IsUnique);
                Assert.False(derivedDependentEntityType.GetDeclaredForeignKeys().Single().IsUnique);
                Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());

                AssertEqual(initialProperties, derivedDependentEntityType.GetProperties());
                AssertEqual(initialKeys, derivedDependentEntityType.GetKeys());
                AssertEqual(initialIndexes, derivedDependentEntityType.GetIndexes());
                AssertEqual(initialForeignKeys, derivedDependentEntityType.GetForeignKeys());

                Assert.Equal(2, modelBuilder.Log.Count(l => l.Message == indexRemoveMessage));

                dependentEntityBuilder.HasIndex(o => new { o.CustomerId, o.AnotherCustomerId })
                    .IsUnique(false);

                Assert.True(dependentEntityType.GetIndexes().All(i => !i.IsUnique));
                Assert.Empty(derivedDependentEntityType.GetDeclaredIndexes());
            }

            [Fact]
            public virtual void Setting_base_type_handles_require_value_generator_properly()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Entity<SpecialOrder>();

                var fkProperty = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).FindProperty(OrderDetails.OrderIdProperty);
                Assert.Equal(ValueGenerated.Never, fkProperty.ValueGenerated);
            }

            [Fact]
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

            [Fact]
            public virtual void Removing_derived_type_make_sure_that_entity_type_is_removed_from_directly_derived_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Book>();
                modelBuilder.Ignore<SpecialBookLabel>();
                modelBuilder.Ignore<AnotherBookLabel>();

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(BookLabel).FullName).GetDirectlyDerivedTypes());
            }

            [Fact]
            public virtual void Can_ignore_base_entity_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<SpecialBookLabel>();
                modelBuilder.Entity<AnotherBookLabel>();
                modelBuilder.Ignore<BookLabel>();

                Assert.Null(modelBuilder.Model.FindEntityType(typeof(BookLabel).FullName));
            }

            [Fact]
            public virtual void Relationships_are_discovered_on_the_base_entity_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<SpecialBookLabel>();
                modelBuilder.Entity<AnotherBookLabel>();

                var bookLabel = modelBuilder.Model.FindEntityType(typeof(BookLabel));
                var specialNavigation = bookLabel.GetDeclaredNavigations().Single(n => n.Name == nameof(BookLabel.SpecialBookLabel));
                Assert.Equal(typeof(SpecialBookLabel), specialNavigation.GetTargetType().ClrType);
                Assert.Equal(nameof(SpecialBookLabel.BookLabel), specialNavigation.FindInverse().Name);
                var anotherNavigation = bookLabel.GetDeclaredNavigations().Single(n => n.Name == nameof(BookLabel.AnotherBookLabel));
                Assert.Equal(typeof(AnotherBookLabel), anotherNavigation.GetTargetType().ClrType);
                Assert.Null(anotherNavigation.FindInverse());
            }

            [Fact]
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
                    .WithOne(e => (SpecialBookLabel)e.BookLabel);

                var fk = bookLabelEntityBuilder.Metadata.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                Assert.Equal(nameof(SpecialBookLabel.BookLabel), fk.DependentToPrincipal.Name);
                Assert.Equal(new[] { fk }, extraSpecialBookLabelEntityBuilder.Metadata.GetForeignKeys());
            }

            [Fact]
            public virtual void Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<PersonBaseViewModel>();

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(PersonBaseViewModel)).GetForeignKeys());

                var citizen = modelBuilder.Model.FindEntityType(typeof(CitizenViewModel));
                var citizenNavigation = citizen.GetDeclaredNavigations().Single(n => n.Name == nameof(CitizenViewModel.CityVM));
                Assert.Equal(nameof(CityViewModel.People), citizenNavigation.FindInverse().Name);

                var doctor = modelBuilder.Model.FindEntityType(typeof(DoctorViewModel));
                var doctorNavigation = doctor.GetDeclaredNavigations().Single(n => n.Name == nameof(CitizenViewModel.CityVM));
                Assert.Equal(nameof(CityViewModel.Medics), doctorNavigation.FindInverse().Name);

                var police = modelBuilder.Model.FindEntityType(typeof(PoliceViewModel));
                var policeNavigation = police.GetDeclaredNavigations().Single(n => n.Name == nameof(CitizenViewModel.CityVM));
                Assert.Equal(nameof(CityViewModel.Police), policeNavigation.FindInverse().Name);

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(CityViewModel)).GetForeignKeys());

                modelBuilder.Entity<CityViewModel>();
                modelBuilder.Validate();
            }

            [Fact]
            public virtual void Can_remove_objects_in_derived_type_which_was_set_using_data_annotation_while_setting_base_type_by_convention()
            {
                var modelBuilder = CreateModelBuilder();

                var derivedEntityType = modelBuilder.Entity<DerivedTypeWithKeyAnnotation>().Metadata;
                var baseEntityType = (EntityType)modelBuilder.Entity<BaseTypeWithKeyAnnotation>().Metadata;

                Assert.Equal(baseEntityType, derivedEntityType.BaseType);
                Assert.Equal(ConfigurationSource.DataAnnotation, baseEntityType.GetPrimaryKeyConfigurationSource());
                Assert.Equal(ConfigurationSource.DataAnnotation, baseEntityType.FindNavigation(nameof(BaseTypeWithKeyAnnotation.Navigation)).ForeignKey.GetConfigurationSource());
                Assert.Equal(ConfigurationSource.Convention, baseEntityType.GetBaseTypeConfigurationSource());
            }

            [Fact]
            public virtual void Cannot_remove_objects_in_derived_type_which_was_set_using_explicit_while_setting_base_type_by_convention()
            {
                var modelBuilder = CreateModelBuilder();

                var derivedEntityTypeBuilder = modelBuilder.Entity<DerivedTypeWithKeyAnnotation>();
                derivedEntityTypeBuilder.HasKey(e => e.MyPrimaryKey);
                derivedEntityTypeBuilder.HasOne(e => e.Navigation).WithOne().HasForeignKey<DerivedTypeWithKeyAnnotation>(e => e.MyPrimaryKey);
                var derivedEntityType = (EntityType)derivedEntityTypeBuilder.Metadata;
                var baseEntityType = (EntityType)modelBuilder.Entity<BaseTypeWithKeyAnnotation>().Metadata;

                Assert.Null(derivedEntityType.BaseType);
                Assert.Equal(ConfigurationSource.DataAnnotation, baseEntityType.GetPrimaryKeyConfigurationSource());
                Assert.Equal(ConfigurationSource.DataAnnotation, baseEntityType.FindNavigation(nameof(BaseTypeWithKeyAnnotation.Navigation)).ForeignKey.GetConfigurationSource());
                Assert.Equal(ConfigurationSource.Explicit, derivedEntityType.FindNavigation(nameof(DerivedTypeWithKeyAnnotation.Navigation)).ForeignKey.GetConfigurationSource());
                Assert.Equal(ConfigurationSource.Explicit, derivedEntityType.GetPrimaryKeyConfigurationSource());
            }

            [Fact]
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

            [Fact] // #7049
            public void Base_type_can_be_discovered_after_creating_foreign_keys_on_derived()
            {
                var mb = CreateModelBuilder();
                mb.Entity<AL>();
                mb.Entity<L>();

                Assert.Equal(ValueGenerated.OnAdd, mb.Model.FindEntityType(typeof(Q)).FindProperty(nameof(Q.ID)).ValueGenerated);
            }

            public class L
            {
                public int Id { get; set; }
                public IList<T> Ts { get; set; }
            }

            public class T : P
            {
                public Q D { get; set; }
                public P P { get; set; }
                public Q F { get; set; }
            }

            public class P : PBase
            {
            }

            public class Q : PBase
            {
            }

            public abstract class PBase
            {
                public int ID { get; set; }
                public string Stuff { get; set; }
            }

            public class AL
            {
                public int Id { get; set; }
                public PBase L { get; set; }
            }
        }
    }
}
