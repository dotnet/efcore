// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class InheritanceTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Can_set_and_remove_base_type()
            {
                var modelBuilder = CreateModelBuilder();

                var pickleBuilder = modelBuilder.Entity<Pickle>();
                pickleBuilder.HasOne(e => e.BigMak).WithMany(e => e.Pickles);
                var pickle = pickleBuilder.Metadata;
                // TODO: Remove this line
                // Issue #2837
                modelBuilder.Entity<BigMak>().Ignore(b => b.Bun);

                Assert.Null(pickle.BaseType);
                var pickleClone = modelBuilder.Model.Clone().FindEntityType(pickle.Name);
                var initialProperties = pickleClone.GetProperties();
                var initialKeys = pickleClone.GetKeys();
                var initialIndexes = pickleClone.GetIndexes();
                var initialForeignKeys = pickleClone.GetForeignKeys();
                var initialReferencingForeignKeys = pickleClone.FindReferencingForeignKeys();

                pickleBuilder.HasBaseType<Ingredient>();
                var ingredientBuilder = modelBuilder.Entity<Ingredient>();
                var ingredient = ingredientBuilder.Metadata;

                Assert.Same(typeof(Ingredient), pickle.BaseType.ClrType);
                AssertEqual(initialProperties, pickle.GetProperties(), new PropertyComparer(compareAnnotations: false));
                AssertEqual(initialKeys, pickle.GetKeys());
                AssertEqual(initialIndexes, pickle.GetIndexes());
                AssertEqual(initialForeignKeys, pickle.GetForeignKeys());
                AssertEqual(initialReferencingForeignKeys, pickle.FindReferencingForeignKeys());

                pickleBuilder.HasBaseType(null);

                Assert.Null(pickle.BaseType);
                AssertEqual(initialProperties, pickle.GetProperties(), new PropertyComparer(compareAnnotations: false));
                AssertEqual(initialKeys, pickle.GetKeys());
                AssertEqual(initialIndexes, pickle.GetIndexes());
                AssertEqual(initialForeignKeys, pickle.GetForeignKeys());
                AssertEqual(initialReferencingForeignKeys, pickle.FindReferencingForeignKeys());

                AssertEqual(initialProperties, ingredient.GetProperties(), new PropertyComparer(compareAnnotations: false));
                AssertEqual(initialKeys, ingredient.GetKeys());
                AssertEqual(initialIndexes, ingredient.GetIndexes());
                Assert.Equal(initialForeignKeys.Count(), ingredient.GetForeignKeys().Count());
                Assert.Equal(initialReferencingForeignKeys.Count(), ingredient.FindReferencingForeignKeys().Count());
            }
            
            [Fact]
            public virtual void Setting_base_type_to_null_fixes_relationships()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<OrderDetails>();
                var principalEntityBuilder = modelBuilder.Entity<Customer>();
                principalEntityBuilder.Ignore(nameof(Customer.Orders));
                var derivedPrincipalEntityBuilder = modelBuilder.Entity<SpecialCustomer>();
                var dependentEntityBuilder = modelBuilder.Entity<Order>();
                var derivedDependentEntityBuilder = modelBuilder.Entity<SpecialOrder>();

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
                var newDerivedFk = derivedDependentEntityBuilder.Metadata.GetDeclaredNavigations().Single().ForeignKey;
                Assert.Equal(nameof(Order.Customer), newDerivedFk.DependentToPrincipal.Name);
                Assert.Equal(nameof(SpecialCustomer.SpecialOrders), newDerivedFk.PrincipalToDependent.Name);
                Assert.Same(derivedPrincipalEntityBuilder.Metadata, newDerivedFk.PrincipalEntityType);
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
                var otherDerivedDependentEntityBuilder = modelBuilder.Entity<BackOrder>();

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
        }
    }
}
