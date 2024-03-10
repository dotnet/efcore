// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

#nullable disable

public abstract partial class ModelBuilderTest
{
    public abstract class OneToOneTestBase(ModelBuilderFixtureBase fixture) : ModelBuilderTestBase(fixture)
    {
        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<CustomerDetails>().HasOne(d => d.Customer).WithOne(c => c.Details)
                .HasForeignKey<CustomerDetails>(c => c.Id);
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));
            var fk = dependentType.GetForeignKeys().Single();

            var navToPrincipal = dependentType.FindNavigation(nameof(CustomerDetails.Customer));
            var navToDependent = principalType.FindNavigation(nameof(Customer.Details));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer);

            Assert.Single(dependentType.GetForeignKeys());
            Assert.Same(navToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(navToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk.PrincipalKey, principalType.GetNavigations().Single().ForeignKey.PrincipalKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK_with_fields()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<OneToOnePrincipalWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.Name);
                    e.HasKey(p => p.Id);
                });
            modelBuilder.Entity<DependentWithField>(
                e =>
                {
                    e.Property(d => d.DependentWithFieldId);
                    e.Property(d => d.OneToOnePrincipalId);
                    e.Ignore(d => d.ManyToManyPrincipals);
                    e.Ignore(d => d.OneToManyPrincipal);
                    e.HasKey(d => d.DependentWithFieldId);
                });

            modelBuilder.Entity<DependentWithField>()
                .HasOne(d => d.OneToOnePrincipal)
                .WithOne(p => p.Dependent)
                .HasForeignKey<DependentWithField>(d => d.DependentWithFieldId);

            var dependentType = model.FindEntityType(typeof(DependentWithField));
            var principalType = model.FindEntityType(typeof(OneToOnePrincipalWithField));
            var fk = dependentType.GetForeignKeys().Single();

            var navToPrincipal = dependentType.FindNavigation("OneToOnePrincipal");
            var navToDependent = principalType.FindNavigation("Dependent");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<DependentWithField>()
                .HasOne(d => d.OneToOnePrincipal)
                .WithOne(p => p.Dependent);

            modelBuilder.FinalizeModel();

            Assert.Single(dependentType.GetForeignKeys());
            Assert.Same(navToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(navToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk.PrincipalKey, principalType.GetNavigations().Single().ForeignKey.PrincipalKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_create_two_FKs_using_the_same_property()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<CustomerDetails>().HasOne(d => d.Customer).WithOne(c => c.Details)
                .HasForeignKey<CustomerDetails>(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().HasOne<Order>().WithOne()
                .HasPrincipalKey<Order>(e => e.OrderId)
                .HasForeignKey<CustomerDetails>(c => c.Id);

            var foreignKeys = model.FindEntityType(typeof(CustomerDetails)).GetForeignKeys()
                .Where(fk => fk.Properties.Single().Name == nameof(CustomerDetails.Id)).ToList();

            Assert.Equal(2, foreignKeys.Count);
            var customerFk = foreignKeys.Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));
            var orderFk = foreignKeys.Single(fk => fk.PrincipalEntityType.ClrType == typeof(Order));
            var dependentType = customerFk.DeclaringEntityType;
            var principalType = customerFk.PrincipalEntityType;
            Assert.Equal(nameof(CustomerDetails.Customer), customerFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(Customer.Details), customerFk.PrincipalToDependent.Name);
            Assert.Null(orderFk.DependentToPrincipal);
            Assert.Null(orderFk.PrincipalToDependent);
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Replaces_existing_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<CustomerDetails>().HasOne(c => c.Customer).WithOne();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigation_to_dependent_and_uses_associated_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>().HasOne(c => c.Details).WithOne()
                .HasForeignKey<CustomerDetails>(c => c.Id);
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_shadow_FK_if_existing_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>()
                .HasOne<CustomerDetails>()
                .WithOne()
                .HasForeignKey<CustomerDetails>(e => e.Id);
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder.Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details)
                .HasPrincipalKey<Customer>(e => e.Id);

            var existingFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.DependentToPrincipal == null);
            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);

            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Single(dependentType.GetIndexes());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_new_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(CustomerDetails.Customer), dependentType.GetNavigations().Single().Name);
            Assert.Equal(nameof(Customer.Details), principalType.GetNavigations().Single().Name);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_removes_existing_FK_when_not_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Details)
                .WithOne()
                .HasForeignKey<OrderDetails>(c => c.Id);
            modelBuilder.Ignore<Customer>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            modelBuilder.Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.True(fk.IsUnique);
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_creates_new_FK_when_not_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Contains("AnotherCustomerId", principalType.GetProperties().Select(p => p.Name));
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_new_FK_over_PK_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<OrderDetails>().Ignore(d => d.Id);
            modelBuilder.Entity<Order>().Ignore(o => o.Details);

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty(nameof(OrderDetails.OrderId));

            modelBuilder.Entity<OrderDetails>().HasKey(d => d.OrderId);
            var fkBuilder = modelBuilder.Entity<OrderDetails>().HasOne(d => d.Order).WithOne();

            var fk = dependentType.GetForeignKeys().Single();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            Assert.Equal(nameof(OrderDetails.Order), dependentType.GetNavigations().Single().Name);
            Assert.Empty(principalType.GetNavigations());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
            Assert.True(fk.IsRequired);

            fkBuilder.HasForeignKey<OrderDetails>("fk");

            modelBuilder.FinalizeModel();

            fk = dependentType.GetForeignKeys().Single();
            Assert.True(fk.IsRequired);
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_navigation_to_dependent_and_new_FK_from_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(Customer));
            var principalType = model.FindEntityType(typeof(CustomerDetails));

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne();

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.True(fk.IsUnique);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_navigation_to_dependent_and_new_FK_from_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details);

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.True(fk.IsUnique);
            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder.Entity<CustomerDetails>().HasOne<Customer>().WithOne();

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.PrincipalToDependent == null);

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Equal(principalType.GetForeignKeys().Count(), principalType.GetIndexes().Count());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_specified_FK_even_if_found_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_specified_FK_even_if_PK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer)
                .HasForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_existing_FK_not_found_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>().HasOne<BigMak>().WithOne()
                .HasForeignKey<Bun>(e => e.BurgerId);
            modelBuilder.Ignore<Pickle>();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .HasForeignKey<Bun>(e => e.BurgerId);

            var model = modelBuilder.FinalizeModel();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));
            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));
            Assert.True(fk.IsUnique);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            Assert.Same(fk, dependentType.GetForeignKeys().Single());
            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Bun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_specified_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .HasForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Bun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_specified_FK_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne()
                .HasForeignKey<Bun>(e => e.BurgerId);

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(BigMak.Bun), principalType.GetNavigations().Single().Name);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_specified_FK_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty(nameof(Bun.BurgerId));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasOne<Bun>().WithOne(e => e.BigMak)
                .HasForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal(nameof(Bun.BigMak), dependentType.GetNavigations().Single().Name);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.True(fk.IsUnique);
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_specified_FK_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>().Ignore(m => m.Bun);
            modelBuilder.Entity<Bun>().Ignore(b => b.BigMak);
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<BigMak>().HasOne<Bun>().WithOne()
                .HasForeignKey<Bun>(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_overrides_existing_FK_when_uniqueness_does_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>().HasOne<BigMak>().WithMany()
                .HasForeignKey(e => e.BurgerId);
            modelBuilder.Ignore<Pickle>();

            var dependentType = (IReadOnlyEntityType)model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));
            var fkProperty = dependentType.FindProperty(nameof(Bun.BurgerId));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .HasForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.True(fk.IsUnique);
            Assert.Equal(nameof(Bun.BigMak), dependentType.GetNavigations().Single().Name);
            Assert.Equal(nameof(BigMak.Bun), principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Removes_existing_FK_when_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<OrderDetails>().HasOne<Order>().WithOne()
                .HasForeignKey<OrderDetails>(c => c.Id);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));
            var existingFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "Id"));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<OrderDetails>(e => e.Id);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal(existingFk.Properties, fk.Properties);
            Assert.Equal(existingFk.PrincipalKey.Properties, fk.PrincipalKey.Properties);
            Assert.Equal("Order", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(OrderDetails));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<Order>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_principal_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(Customer));
            var principalType = model.FindEntityType(typeof(CustomerDetails));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne()
                .HasForeignKey<Customer>(e => e.Id);

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.True(fk.IsUnique);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Null(dependentType.FindIndex(fk.Properties));
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_dependent_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            var fk = modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne()
                .HasForeignKey<CustomerDetails>(e => e.Id).Metadata;

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count() - 1, dependentType.GetIndexes().Count());
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_principal_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(Customer));
            var principalType = model.FindEntityType(typeof(CustomerDetails));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details)
                .HasForeignKey<Customer>(e => e.Id);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Equal(nameof(Customer.Details), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Equal(principalType.GetForeignKeys().Count(), principalType.GetIndexes().Count());
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_dependent_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details)
                .HasForeignKey<CustomerDetails>(e => e.Id);

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Equal(nameof(Customer.Details), fk.PrincipalToDependent.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count() - 1, dependentType.GetIndexes().Count());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_dependent_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne()
                .HasForeignKey<CustomerDetails>(e => e.Id);

            var existingFk = dependentType.GetNavigations().Single().ForeignKey;
            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.Same(dependentType.FindProperty(nameof(Customer.Id)), fk.Properties.Single());

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count() - 1, dependentType.GetIndexes().Count());
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_specified_on_principal_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(Customer));
            var principalType = model.FindEntityType(typeof(CustomerDetails));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            var principalFk = principalType.GetForeignKeys().SingleOrDefault();
            var existingFk = dependentType.GetForeignKeys().SingleOrDefault();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne()
                .HasForeignKey<Customer>(e => e.Id);

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Same(principalFk, principalType.GetForeignKeys().SingleOrDefault());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Can_use_PK_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details)
                .HasForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void OneToOne_can_have_PK_explicitly_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer)
                .HasPrincipalKey<Customer>(e => e.Id);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_use_alternate_principal_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));
            var principalProperty = principalType.FindProperty("AlternateKey");
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer)
                .HasPrincipalKey<Customer>(e => e.AlternateKey);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_both_keys_specified_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var model = (Model)modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");
            var principalProperty = principalType.FindProperty("OrderId");

            var principalPropertyCount = principalType.GetProperties().Count();
            var dependentPropertyCount = dependentType.GetProperties().Count();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasForeignKey<OrderDetails>(e => e.OrderId)
                .HasPrincipalKey<Order>(e => e.OrderId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("Order", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.GetProperties().Count());
            Assert.Equal(dependentPropertyCount, dependentType.GetProperties().Count());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_both_keys_specified_explicitly_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = (Model)modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");
            var principalProperty = principalType.FindProperty("OrderId");

            var principalPropertyCount = principalType.GetProperties().Count();
            var dependentPropertyCount = dependentType.GetProperties().Count();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasPrincipalKey<Order>(e => e.OrderId)
                .HasForeignKey<OrderDetails>(e => e.OrderId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("Order", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.GetProperties().Count());
            Assert.Equal(dependentPropertyCount, dependentType.GetProperties().Count());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_both_alternate_keys_specified_explicitly()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");
            var principalProperty = principalType.FindProperty("AlternateKey");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .HasForeignKey<Bun>(e => e.BurgerId)
                .HasPrincipalKey<BigMak>(e => e.AlternateKey);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Bun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_both_alternate_keys_specified_explicitly_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.FindEntityType(typeof(Bun));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");
            var principalProperty = principalType.FindProperty("AlternateKey");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .HasPrincipalKey<BigMak>(e => e.AlternateKey)
                .HasForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Bun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_dependent_side_specified_explicitly_on_optional()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<CustomerDetails>().HasOne(d => d.Customer).WithOne(c => c.Details)
                .HasForeignKey<CustomerDetails>().IsRequired(false);
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));
            var fk = dependentType.GetForeignKeys().Single();

            var navToPrincipal = dependentType.FindNavigation(nameof(CustomerDetails.Customer));
            var navToDependent = principalType.FindNavigation(nameof(Customer.Details));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            Assert.False(fk.IsRequired);
            Assert.Single(dependentType.GetForeignKeys());
            Assert.Same(navToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(navToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk.PrincipalKey, principalType.GetNavigations().Single().ForeignKey.PrincipalKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void Does_not_use_existing_FK_when_principal_key_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne<Order>().WithOne()
                .HasForeignKey<OrderDetails>(e => e.Id);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));
            var existingFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "Id"));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.NotEqual(existingFk.Properties, fk.Properties);
            Assert.Equal("Order", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Details", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Single(dependentType.GetIndexes());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var keyProperty = principalType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(keyProperty, fk.PrincipalKey.Properties.Single());

            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(OrderDetails));

            var keyProperty = principalType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(keyProperty, fk.PrincipalKey.Properties.Single());

            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(fk.PrincipalKey, principalType.GetKeys().First(k => k != principalKey));
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_principal_and_foreign_key_specified_on_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<OrderDetails>(e => e.OrderId)
                .HasPrincipalKey<Order>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_principal_and_foreign_key_specified_on_dependent_in_reverse_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(OrderDetails));
            var principalType = model.FindEntityType(typeof(Order));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId)
                .HasForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_FK_when_principal_and_foreign_key_specified_on_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(OrderDetails));

            var fkProperty = dependentType.FindProperty("OrderId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<Order>(e => e.OrderId)
                .HasPrincipalKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            Assert.Equal(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_cannot_be_flipped_twice()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<OrderDetails>(e => e.Id);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            Assert.Equal(
                CoreStrings.RelationshipCannotBeInverted,
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                        .HasForeignKey<OrderDetails>(e => e.OrderId)
                        .HasPrincipalKey<OrderDetails>(e => e.OrderId)).Message);
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_twice_separately()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<OrderDetails>(e => e.Id);
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<OrderDetails>(e => e.OrderId);

            modelBuilder
                .Entity<OrderDetails>().Navigation(e => e.Order).IsRequired();
            modelBuilder
                .Entity<Order>().Navigation(e => e.Details).IsRequired();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<OrderDetails>(e => e.OrderId);

            var model = modelBuilder.FinalizeModel();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(OrderDetails));
            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(principalType.FindProperty(nameof(OrderDetails.OrderId)), fk.PrincipalKey.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(fk.PrincipalKey, principalType.GetKeys().First(k => !k.IsPrimaryKey()));
            Assert.Same(fk.PrincipalKey, principalType.GetDeclaredKeys().First(k => !k.IsPrimaryKey()));
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
            Assert.True(fk.IsRequiredDependent);
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_cannot_be_flipped_twice_in_reverse_order()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            Assert.Equal(
                CoreStrings.RelationshipCannotBeInverted,
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                        .HasPrincipalKey<OrderDetails>(e => e.OrderId)
                        .HasForeignKey<OrderDetails>(e => e.OrderId)).Message);
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_twice_in_reverse_order_separately()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId);

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .HasForeignKey<Order>(e => e.OrderId);

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(OrderDetails));
            var fk = dependentType.GetForeignKeys().Single();

            Assert.Same(dependentType.FindProperty(nameof(Order.OrderId)), fk.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.Same(fk.PrincipalToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void IsRequired_throws_principal_end_is_ambiguous()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Entity<Order>().Property<int>("OrderDetailsId");

            Assert.Equal(
                CoreStrings.AmbiguousEndRequiredDependentNavigation(
                    nameof(OrderDetails),
                    nameof(OrderDetails.Order),
                    "{'OrderId'}"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<OrderDetails>().Navigation(e => e.Order).IsRequired()).Message);

            Assert.Equal(
                CoreStrings.AmbiguousEndRequiredDependentNavigation(
                    nameof(Order),
                    nameof(Order.Details),
                    "{'OrderId'}"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<Order>().Navigation(e => e.Details).IsRequired()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_if_not_principal_or_dependent_specified()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .HasPrincipalKey<Order>(e => e.OrderId);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var relationship = modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details);

            Assert.Equal(
                CoreStrings.DependentEntityTypeNotInRelationship(
                    nameof(OrderDetails),
                    nameof(Order),
                    modelBuilder.GetDisplayName(typeof(OrderCombination))),
                Assert.Throws<InvalidOperationException>(() => relationship.HasForeignKey<OrderCombination>(e => e.OrderId)).Message);

            Assert.Equal(
                CoreStrings.PrincipalEntityTypeNotInRelationship(
                    nameof(OrderDetails),
                    nameof(Order),
                    modelBuilder.GetDisplayName(typeof(OrderCombination))),
                Assert.Throws<InvalidOperationException>(() => relationship.HasPrincipalKey<OrderCombination>(e => e.OrderId)).Message);
        }

        [ConditionalFact]
        public virtual void Configuring_principal_type_as_keyless_inverts_the_relationship()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .Ignore(e => e.Order)
                .HasOne<Order>().WithOne(e => e.Details);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var orderEntityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Order));
            Assert.False(orderEntityType.FindNavigation(nameof(Order.Details)).IsOnDependent);

            modelBuilder.Entity<Order>().HasNoKey();

            var model = modelBuilder.FinalizeModel();

            orderEntityType = model.FindEntityType(typeof(Order))!;
            Assert.True(orderEntityType.FindNavigation(nameof(Order.Details)).IsOnDependent);
        }

        [ConditionalFact]
        public virtual void Configuring_principal_type_as_keyless_throws_if_not_invertible()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .Ignore(e => e.Order)
                .HasOne<Order>().WithOne(e => e.Details)
                .HasPrincipalKey<Order>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var orderEntityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Order));
            Assert.False(orderEntityType.FindNavigation(nameof(Order.Details)).IsOnDependent);

            Assert.Equal(
                CoreStrings.PrincipalKeylessType(
                    nameof(Order),
                    nameof(Order) + "." + nameof(Order.Details),
                    nameof(OrderDetails)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Order>().HasNoKey()).Message);
        }

        [ConditionalFact]
        public virtual void Configuring_principal_type_as_keyless_throws_when_there_is_an_explicit_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var orderEntityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Order));
            Assert.False(orderEntityType.FindNavigation(nameof(Order.Details)).IsOnDependent);

            Assert.Equal(
                CoreStrings.NavigationToKeylessType(
                    nameof(OrderDetails.Order),
                    nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Order>().HasNoKey()).Message);
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_principal_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne()
                .HasPrincipalKey<Customer>(e => e.Id);

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(Customer.Details), fk.PrincipalToDependent.Name);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_dependent_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne()
                .HasPrincipalKey<Customer>(e => e.Id);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.True(fk.IsUnique);
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_principal_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasOne<CustomerDetails>().WithOne(e => e.Customer)
                .HasPrincipalKey<Customer>(e => e.Id);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.Same(fk.DependentToPrincipal, dependentType.GetNavigations().Single());
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Equal(principalType.GetForeignKeys().Count(), principalType.GetIndexes().Count());
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_dependent_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details)
                .HasPrincipalKey<Customer>(e => e.Id);

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_principal_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasForeignKey<CustomerDetails>(d => d.CustomerId);

            modelBuilder
                .Entity<Customer>().HasOne<CustomerDetails>().WithOne()
                .HasPrincipalKey<Customer>(e => e.Id);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.DependentToPrincipal == null);
            Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_principal_key_when_specified_on_dependent_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));

            var existingFk = dependentType.GetForeignKeys().SingleOrDefault();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne()
                .HasPrincipalKey<Customer>(e => e.Id);

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());
            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.GetProperties());
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_existing_composite_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            var dependentType = model.FindEntityType(typeof(ToastedBun));
            modelBuilder.Entity<ToastedBun>().HasOne<Whoopper>().WithOne()
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("ToastedBun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_creates_composite_FK_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("ToastedBun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_use_alternate_composite_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>(
                b => b.HasKey(
                    c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.FindProperty("AlternateKey1");
            var principalProperty2 = principalType.FindProperty("AlternateKey2");

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 })
                .HasPrincipalKey<Whoopper>(
                    e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
            Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("ToastedBun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_use_alternate_composite_key_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>(
                b => b.HasKey(
                    c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.FindProperty("AlternateKey1");
            var principalProperty2 = principalType.FindProperty("AlternateKey2");

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .HasPrincipalKey<Whoopper>(
                    e => new { e.AlternateKey1, e.AlternateKey2 })
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
            Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("ToastedBun", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Uses_composite_PK_for_FK_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Mustard>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.FindEntityType(typeof(Mustard));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("Id1");
            var fkProperty2 = dependentType.FindProperty("Id2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Mustard>().HasOne(e => e.Whoopper).WithOne(e => e.Mustard)
                .HasForeignKey<Mustard>();

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();

            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Mustard", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_and_composite_PK_is_still_used_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Mustard>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.FindEntityType(typeof(Mustard));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("Id1");
            var fkProperty2 = dependentType.FindProperty("Id2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.Mustard).WithOne(e => e.Whoopper)
                .HasForeignKey<Mustard>();

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Mustard", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_using_principal_and_composite_PK_is_still_used_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Mustard>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.FindEntityType(typeof(Mustard));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("Id1");
            var fkProperty2 = dependentType.FindProperty("Id2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Mustard>().HasOne(e => e.Whoopper).WithOne(e => e.Mustard)
                .HasPrincipalKey<Whoopper>(
                    e => new { e.Id1, e.Id2 })
                .IsRequired();

            var fk = dependentType.GetForeignKeys().Single();

            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Mustard", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Empty(dependentType.GetIndexes());
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_composite_FK_when_specified_on_principal_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty(nameof(ToastedBun.BurgerId1));
            var fkProperty2 = dependentType.FindProperty(nameof(ToastedBun.BurgerId2));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne()
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(Whoopper.ToastedBun), fk.PrincipalToDependent.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_composite_FK_when_specified_on_principal_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty(nameof(ToastedBun.BurgerId1));
            var fkProperty2 = dependentType.FindProperty(nameof(ToastedBun.BurgerId2));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne<ToastedBun>().WithOne(e => e.Whoopper)
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal(nameof(ToastedBun.Whoopper), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.True(fk.IsUnique);
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Creates_composite_FK_when_specified_on_principal_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<ToastedBun>().HasOne(b => b.Whoopper).WithOne(w => w.ToastedBun).HasPrincipalKey<ToastedBun>();
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne<ToastedBun>().WithOne()
                .HasForeignKey<ToastedBun>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == fk));
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.True(
                principalType.GetForeignKeys()
                    .All(foreignKey => principalType.FindIndex(foreignKey.Properties).IsUnique == foreignKey.IsUnique));
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_when_self_referencing()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef2);

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));
            var fk = entityType.GetForeignKeys().Single();

            var navigationToPrincipal = fk.DependentToPrincipal;
            var navigationToDependent = fk.PrincipalToDependent;

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef2);

            fk = entityType.GetForeignKeys().Single();
            Assert.Equal(navigationToDependent.Name, fk.PrincipalToDependent.Name);
            Assert.Equal(navigationToPrincipal.Name, fk.DependentToPrincipal.Name);
            Assert.True(fk.IsRequired);

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef2).WithOne(e => e.SelfRef1);

            fk = entityType.GetForeignKeys().Single();
            Assert.Equal(navigationToPrincipal.Name, fk.PrincipalToDependent.Name);
            Assert.Equal(navigationToDependent.Name, fk.DependentToPrincipal.Name);
            Assert.True(fk.IsRequired);

            Assert.Equal(fk.DeclaringEntityType.GetForeignKeys().Count(), fk.DeclaringEntityType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Creates_self_referencing_FK_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>(
                eb =>
                {
                    eb.HasKey(e => e.Id);
                    eb.Property(e => e.SelfRefId);
                });

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne();

            var fk = entityType.FindNavigation(nameof(SelfRef.SelfRef1)).ForeignKey;
            var conventionFk = entityType.FindNavigation(nameof(SelfRef.SelfRef2)).ForeignKey;

            Assert.NotEqual(fk, conventionFk);
            Assert.NotEqual(fk.Properties, entityType.FindPrimaryKey().Properties);
            Assert.Equal(fk.PrincipalKey, entityType.FindPrimaryKey());
            Assert.Null(fk.PrincipalToDependent);
            Assert.Equal(nameof(SelfRef.SelfRef1), fk.DependentToPrincipal?.Name);
            Assert.Equal(2, entityType.GetNavigations().Count());
            Assert.Equal(fk.DeclaringEntityType.GetForeignKeys().Count(), fk.DeclaringEntityType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Creates_self_referencing_FK_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>(
                eb =>
                {
                    eb.HasKey(e => e.Id);
                    eb.Property(e => e.SelfRefId);
                });

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));

            modelBuilder.Entity<SelfRef>().HasOne<SelfRef>().WithOne(e => e.SelfRef1);

            var fk = entityType.FindNavigation(nameof(SelfRef.SelfRef1)).ForeignKey;
            var conventionFk = entityType.FindNavigation(nameof(SelfRef.SelfRef2)).ForeignKey;

            Assert.NotEqual(fk, conventionFk);
            Assert.NotEqual(fk.Properties, entityType.FindPrimaryKey().Properties);
            Assert.Equal(fk.PrincipalKey, entityType.FindPrimaryKey());
            Assert.Equal(nameof(SelfRef.SelfRef1), fk.PrincipalToDependent?.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Equal(2, entityType.GetNavigations().Count());
            Assert.Equal(fk.DeclaringEntityType.GetForeignKeys().Count(), fk.DeclaringEntityType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Creates_self_referencing_FK_on_PK()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>(
                eb => eb.HasKey(e => e.Id));

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef2).HasForeignKey<SelfRef>(e => e.Id);

            modelBuilder.FinalizeModel();

            var fk = entityType.FindNavigation(nameof(SelfRef.SelfRef1)).ForeignKey;

            Assert.Equal(fk.Properties, entityType.FindPrimaryKey().Properties);
            Assert.Equal(fk.PrincipalKey, entityType.FindPrimaryKey());
            Assert.Equal(nameof(SelfRef.SelfRef1), fk.DependentToPrincipal?.Name);
            Assert.Equal(nameof(SelfRef.SelfRef2), fk.PrincipalToDependent?.Name);
            Assert.Equal(2, entityType.GetNavigations().Count());
            Assert.Empty(fk.DeclaringEntityType.GetIndexes());
            Assert.Single(fk.DeclaringEntityType.GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_when_self_referencing_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity<SelfRef>();
            entityBuilder.Ignore(nameof(SelfRef.SelfRef2));
            entityBuilder.HasOne(e => e.SelfRef1).WithOne();

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));
            var existingFk = entityType.GetForeignKeys().Single();

            var navigationToPrincipal = existingFk.DependentToPrincipal;
            var navigationToDependent = existingFk.PrincipalToDependent;

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne();

            Assert.Same(existingFk, entityType.GetForeignKeys().Single());
            Assert.Equal(navigationToDependent?.Name, existingFk.PrincipalToDependent?.Name);
            Assert.Equal(navigationToPrincipal.Name, existingFk.DependentToPrincipal.Name);
            Assert.True(((IReadOnlyForeignKey)existingFk).IsRequired);

            modelBuilder.Entity<SelfRef>().HasOne<SelfRef>().WithOne(e => e.SelfRef1);

            var fk = entityType.GetForeignKeys().Single();

            Assert.Equal(existingFk.Properties, fk.Properties);
            Assert.Equal(existingFk.PrincipalKey, fk.PrincipalKey);
            Assert.Equal(navigationToPrincipal.Name, fk.PrincipalToDependent.Name);
            Assert.Equal(navigationToDependent?.Name, fk.DependentToPrincipal?.Name);
            Assert.True(fk.IsRequired);
            Assert.False(fk.IsRequiredDependent);

            Assert.Equal(fk.DeclaringEntityType.GetForeignKeys().Count(), fk.DeclaringEntityType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Principal_and_dependent_can_be_flipped_when_self_referencing_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>().HasOne<SelfRef>().WithOne(e => e.SelfRef2);

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));
            var fk = entityType.FindNavigation(nameof(SelfRef.SelfRef2)).ForeignKey;

            var navigationToPrincipal = fk.DependentToPrincipal;
            var navigationToDependent = fk.PrincipalToDependent;

            modelBuilder.Entity<SelfRef>().HasOne<SelfRef>().WithOne(e => e.SelfRef2);

            Assert.Same(fk, entityType.FindNavigation(nameof(SelfRef.SelfRef2)).ForeignKey);
            Assert.Equal(navigationToDependent.Name, fk.PrincipalToDependent.Name);
            Assert.Equal(navigationToPrincipal?.Name, fk.DependentToPrincipal?.Name);
            Assert.True(fk.IsUnique);

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef2).WithOne();

            var newFk = entityType.FindNavigation(nameof(SelfRef.SelfRef2)).ForeignKey;

            Assert.Equal(fk.Properties, newFk.Properties);
            Assert.Equal(fk.PrincipalKey, newFk.PrincipalKey);
            Assert.Equal(navigationToPrincipal?.Name, newFk.PrincipalToDependent?.Name);
            Assert.Equal(navigationToDependent.Name, newFk.DependentToPrincipal.Name);
            Assert.True(newFk.IsUnique);

            Assert.Equal(fk.DeclaringEntityType.GetForeignKeys().Count(), fk.DeclaringEntityType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Throws_on_duplicate_navigation_when_self_referencing()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("SelfRef1", typeof(SelfRef).Name, typeof(SelfRef).Name),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef1)).Message);
        }

        [ConditionalFact]
        public virtual void Creates_self_referencing_FK_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SelfRef>().Ignore(s => s.SelfRefId);
            modelBuilder.Entity<SelfRef>().Property<int>("SelfRef1Id");

            modelBuilder.FinalizeModel();

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));
            var fk = entityType.GetForeignKeys().Single();
            Assert.Equal("SelfRef1Id", fk.Properties.Single().Name);
            Assert.True(fk.IsRequired);
            Assert.False(fk.IsRequiredDependent);
            Assert.Equal(fk.PrincipalKey, entityType.FindPrimaryKey());
            Assert.Equal(nameof(SelfRef.SelfRef2), fk.PrincipalToDependent?.Name);
            Assert.Equal(nameof(SelfRef.SelfRef1), fk.DependentToPrincipal?.Name);
            Assert.Equal(2, entityType.GetNavigations().Count());
            Assert.Single(fk.DeclaringEntityType.GetIndexes());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Creates_self_referencing_FK_by_convention_inverted()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SelfRef>().Property<int>("SelfRef2Id");
            modelBuilder.Entity<SelfRef>().Ignore(s => s.SelfRefId);

            modelBuilder.FinalizeModel();

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));
            var fk = entityType.GetForeignKeys().Single();
            Assert.Equal("SelfRef2Id", fk.Properties.Single().Name);
            Assert.Equal(fk.PrincipalKey, entityType.FindPrimaryKey());
            Assert.Equal(nameof(SelfRef.SelfRef1), fk.PrincipalToDependent?.Name);
            Assert.Equal(nameof(SelfRef.SelfRef2), fk.DependentToPrincipal?.Name);
            Assert.Equal(2, entityType.GetNavigations().Count());
            Assert.Single(fk.DeclaringEntityType.GetIndexes());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Throws_on_ambiguous_FK_when_self_referencing()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SelfRef>();

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship("SelfRef.SelfRef1", "SelfRef.SelfRef2"),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        public virtual void Throws_on_two_ambiguous_FK_when_self_referencing()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SelfRef>().Property<int>("SelfRef1Id");
            modelBuilder.Entity<SelfRef>().Property<int>("SelfRef2Id");
            modelBuilder.Entity<SelfRef>().Ignore(s => s.SelfRefId);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship("SelfRef.SelfRef1", "SelfRef.SelfRef2"),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_if_navigations_have_same_name()
        {
            var modelBuilder = CreateModelBuilder();

            var dependentEntityTypeBuilder = modelBuilder.Entity<BaseTypeWithKeyAnnotation>();
            var principalEntityTypeBuilder = modelBuilder.Entity<PrincipalTypeWithKeyAnnotation>();

            var foreignKey1 = dependentEntityTypeBuilder.HasOne(p => p.Navigation).WithOne(d => d.Navigation).Metadata;

            var foreignKey2 = principalEntityTypeBuilder.HasOne(p => p.Navigation).WithOne(d => d.Navigation).Metadata;

            modelBuilder.FinalizeModel();

            Assert.Same(foreignKey1, foreignKey2);
            Assert.Same(dependentEntityTypeBuilder.Metadata, foreignKey1.DeclaringEntityType);
            Assert.Equal(nameof(PrincipalTypeWithKeyAnnotation.Navigation), foreignKey1.DependentToPrincipal?.Name);
            Assert.Equal(nameof(BaseTypeWithKeyAnnotation.Navigation), foreignKey1.PrincipalToDependent?.Name);
        }

        [ConditionalFact]
        public virtual void Can_specify_shadow_fk_before_configuring_principal_PK()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Hob>()
                .HasMany(c => c.Nobs)
                .WithOne(d => d.Hob)
                .HasForeignKey("Fk1");

            modelBuilder.Entity<Hob>()
                .HasKey(
                    e => new { e.NobId1 });
            modelBuilder.Entity<Nob>()
                .HasKey(
                    e => new { e.HobId1 });

            modelBuilder.FinalizeModel();

            Assert.Equal(
                typeof(int?), modelBuilder.Model.FindEntityType(typeof(Nob)).GetForeignKeys().Single().Properties.Single().ClrType);
            Assert.Equal(
                typeof(string), modelBuilder.Model.FindEntityType(typeof(Hob)).GetForeignKeys().Single().Properties.Single().ClrType);
        }

        [ConditionalFact]
        public virtual void Can_specify_shadow_fk_before_reconfiguring_principal_PK()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Hob>()
                .HasKey(
                    e => new { e.Id1 });
            modelBuilder.Entity<Nob>()
                .HasKey(
                    e => new { e.Id1 });

            modelBuilder.Entity<Hob>()
                .HasMany(c => c.Nobs)
                .WithOne(d => d.Hob)
                .HasForeignKey("Fk1");

            modelBuilder.Entity<Hob>()
                .HasKey(
                    e => new { e.NobId1 });
            modelBuilder.Entity<Nob>()
                .HasKey(
                    e => new { e.HobId1 });

            modelBuilder.FinalizeModel();

            Assert.Equal(
                typeof(int?), modelBuilder.Model.FindEntityType(typeof(Nob)).GetForeignKeys().Single().Properties.Single().ClrType);
            Assert.Equal(
                typeof(string), modelBuilder.Model.FindEntityType(typeof(Hob)).GetForeignKeys().Single().Properties.Single().ClrType);
        }

        [ConditionalFact]
        public virtual void Can_specify_shadow_identifying_fk()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Nob>(
                eb =>
                {
                    eb.Ignore(e => e.Hobs);
                    eb.Ignore(e => e.Hob);

                    eb.Property<Guid>("Id");
                    eb.HasKey("Id");
                });

            modelBuilder.Entity<Hob>(
                eb =>
                {
                    eb.Ignore(e => e.Nobs);

                    eb.HasOne(c => c.Nob)
                        .WithOne()
                        .HasForeignKey<Hob>("NobId");

                    eb.HasKey("NobId");

                    eb.HasOne(c => c.Nob)
                        .WithOne()
                        .HasForeignKey<Hob>("NobId")
                        .IsRequired();
                });

            modelBuilder.FinalizeModel();

            var dependent = modelBuilder.Model.FindEntityType(typeof(Hob));
            var fk = dependent.GetForeignKeys().Single();
            Assert.Equal(typeof(Guid), fk.Properties.Single().ClrType);
        }

        [ConditionalFact]
        public virtual void Throws_if_specified_FK_types_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(
                CoreStrings.ForeignKeyTypeMismatch(
                    "{'GuidProperty' : Guid}", nameof(CustomerDetails), "{'Id' : int}", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                        .HasPrincipalKey<Customer>("Id")
                        .HasForeignKey<CustomerDetails>("GuidProperty")).Message);
        }

        [ConditionalFact]
        public virtual void Overrides_PK_if_specified_FK_types_do_not_match_separately()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            var guidProperty = modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty").Metadata;
            modelBuilder.Ignore<Order>();

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasPrincipalKey<Customer>(nameof(Customer.Id));

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasForeignKey<CustomerDetails>("GuidProperty");

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(guidProperty, fk.Properties.Single());
            Assert.Equal(typeof(Guid), fk.PrincipalKey.Properties.Single().ClrType);
        }

        [ConditionalFact]
        public virtual void Throws_if_specified_PK_types_do_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(
                CoreStrings.ForeignKeyTypeMismatch(
                    "{'GuidProperty' : Guid}", nameof(CustomerDetails), "{'Id' : int}", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder
                            .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                            .HasForeignKey<CustomerDetails>("GuidProperty")
                            .HasPrincipalKey<Customer>("Id")).Message);
        }

        [ConditionalFact]
        public virtual void Overrides_FK_if_specified_PK_types_do_not_match_separately()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasForeignKey<CustomerDetails>("GuidProperty");

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasPrincipalKey<Customer>(nameof(Customer.Id));

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var principalType = model.FindEntityType(typeof(Customer));
            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(principalType.FindProperty(nameof(Customer.Id)), fk.PrincipalKey.Properties.Single());
            Assert.False(fk.IsRequired);
            Assert.Equal(typeof(int?), fk.Properties.Single().ClrType);
        }

        [ConditionalFact]
        public virtual void Throws_if_specified_FK_count_does_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(
                CoreStrings.ForeignKeyCountMismatch("{'Id', 'GuidProperty'}", nameof(CustomerDetails), "{'Id'}", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder
                            .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                            .HasPrincipalKey<Customer>("Id")
                            .HasForeignKey<CustomerDetails>("Id", "GuidProperty")).Message);
        }

        [ConditionalFact]
        public virtual void Overrides_PK_if_specified_FK_count_does_not_match_separately()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            var guidProperty = modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty").Metadata;
            modelBuilder.Ignore<Order>();

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasPrincipalKey<Customer>(nameof(Customer.Id));

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasForeignKey<CustomerDetails>(nameof(CustomerDetails.Id), "GuidProperty");

            var dependentType = model.FindEntityType(typeof(CustomerDetails));
            var fk = dependentType.GetForeignKeys().Single();
            AssertEqual(new[] { dependentType.FindProperty(nameof(CustomerDetails.Id)), guidProperty }, fk.Properties);
            Assert.Equal(2, fk.PrincipalKey.Properties.Count);
        }

        [ConditionalFact]
        public virtual void Throws_if_specified_PK_count_does_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(
                CoreStrings.ForeignKeyCountMismatch("{'Id', 'GuidProperty'}", nameof(CustomerDetails), "{'Id'}", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder
                            .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                            .HasForeignKey<CustomerDetails>("Id", "GuidProperty")
                            .HasPrincipalKey<Customer>("Id")).Message);
        }

        [ConditionalFact]
        public virtual void Overrides_FK_if_specified_PK_count_does_not_match_separately()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            var principalType = model.FindEntityType(typeof(Customer));

            modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasForeignKey<CustomerDetails>(nameof(CustomerDetails.Id), "GuidProperty");

            var fk = modelBuilder
                .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                .HasPrincipalKey<Customer>(nameof(Customer.Id)).Metadata;

            Assert.Same(principalType.FindProperty(nameof(Customer.Id)), fk.PrincipalKey.Properties.Single());
            Assert.Single(fk.Properties);
        }

        [ConditionalFact]
        public virtual void Foreign_key_properties_are_not_discovered_for_one_to_one_relationship_with_ambiguous_principal_end()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);
            modelBuilder.Entity<Nob>().Ignore(e => e.Hobs);
            modelBuilder.Entity<Hob>().Ignore(e => e.Nobs);

            var dependentType = model.FindEntityType(typeof(Hob));
            var principalType = model.FindEntityType(typeof(Nob));
            var fk = dependentType.GetNavigations().First().ForeignKey;
            Assert.Same(fk, principalType.GetNavigations().First().ForeignKey);
            Assert.True(fk.Properties.All(p => p.IsShadowProperty()));

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(Nob).Name + "." + nameof(Nob.Hob),
                    typeof(Hob).Name + "." + nameof(Hob.Nob)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Creates_relationship_on_existing_FK_if_using_different_principal_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                .HasForeignKey(
                    c => new { c.BurgerId1, c.BurgerId2 });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne<Tomato>().WithOne()
                .HasForeignKey<Tomato>(
                    e => new { e.BurgerId1, e.BurgerId2 })
                .HasPrincipalKey<Whoopper>(
                    e => new { e.AlternateKey1, e.AlternateKey2 });

            var existingFk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(Tomato.Whoopper), existingFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(Whoopper.Tomatoes), existingFk.PrincipalToDependent.Name);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.NotSame(principalKey, fk.PrincipalKey);
            Assert.Empty(principalType.GetIndexes());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(existingFk.DeclaringEntityType.FindIndex(existingFk.Properties).IsUnique);
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_on_existing_FK_if_using_different_principal_key_different_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                .HasForeignKey(
                    c => new { c.BurgerId1, c.BurgerId2 });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne<Tomato>().WithOne()
                .HasPrincipalKey<Whoopper>(
                    e => new { e.AlternateKey1, e.AlternateKey2 })
                .HasForeignKey<Tomato>(
                    e => new { e.BurgerId1, e.BurgerId2 });

            var existingFk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(Tomato.Whoopper), existingFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(Whoopper.Tomatoes), existingFk.PrincipalToDependent.Name);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.NotSame(principalKey, fk.PrincipalKey);
            Assert.Empty(principalType.GetIndexes());
            Assert.Single(dependentType.GetIndexes());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_index_even_if_covered_by_an_alternate_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(
                c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>().HasAlternateKey(
                c => new { c.BurgerId1, c.BurgerId2 });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            modelBuilder
                .Entity<Whoopper>().HasOne<Tomato>().WithOne()
                .HasForeignKey<Tomato>(
                    e => new { e.BurgerId1 })
                .HasPrincipalKey<Whoopper>(
                    e => new { e.AlternateKey1 });

            var existingFk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Empty(principalType.GetForeignKeys());
            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.Empty(principalType.GetIndexes());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());

            modelBuilder.Entity<Tomato>().Ignore(t => t.BurgerId2);

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Throws_on_existing_many_to_one_relationship()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs);

            var dependentType = model.FindEntityType(typeof(Nob));
            var principalType = model.FindEntityType(typeof(Hob));

            Assert.Equal(
                CoreStrings.ConflictingRelationshipNavigation(
                    principalType.DisplayName() + "." + nameof(Hob.Nob),
                    dependentType.DisplayName() + "." + nameof(Nob.Hob),
                    dependentType.DisplayName() + "." + nameof(Nob.Hobs),
                    principalType.DisplayName() + "." + nameof(Hob.Nob)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob)).Message);
        }

        [ConditionalFact]
        public virtual void Removes_existing_unidirectional_many_to_one_relationship()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs);

            modelBuilder.Entity<Hob>().HasOne(e => e.Nob).WithMany();

            var dependentType = model.FindEntityType(typeof(Nob));
            var principalType = model.FindEntityType(typeof(Hob));
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

            var fk = dependentType.GetNavigations().Single(n => n.Name == nameof(Nob.Hob)).ForeignKey;
            Assert.Same(fk, principalType.GetNavigations().Single(n => n.Name == nameof(Hob.Nob)).ForeignKey);
            Assert.True(fk.IsUnique);

            Assert.Equal(0, dependentType.GetForeignKeys().Count(foreignKey => foreignKey != fk));
            Assert.Equal(0, principalType.GetForeignKeys().Count(foreignKey => foreignKey != fk));
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void Throws_on_existing_one_to_many_relationship()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob);

            var dependentType = model.FindEntityType(typeof(Nob));
            var principalType = model.FindEntityType(typeof(Hob));

            Assert.Equal(
                CoreStrings.ConflictingRelationshipNavigation(
                    principalType.DisplayName() + "." + nameof(Hob.Nob),
                    dependentType.DisplayName() + "." + nameof(Nob.Hob),
                    principalType.DisplayName() + "." + nameof(Hob.Nobs),
                    dependentType.DisplayName() + "." + nameof(Nob.Hob)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob)).Message);
        }

        [ConditionalFact]
        public virtual void Removes_existing_unidirectional_one_to_many_relationship()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob);

            modelBuilder.Entity<Hob>().HasMany<Nob>().WithOne(e => e.Hob);

            var dependentType = model.FindEntityType(typeof(Nob));
            var principalType = model.FindEntityType(typeof(Hob));
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

            var fk = dependentType.GetNavigations().Single(n => n.Name == nameof(Nob.Hob)).ForeignKey;
            Assert.Same(fk, principalType.GetNavigations().Single(n => n.Name == nameof(Hob.Nob)).ForeignKey);
            Assert.True(fk.IsUnique);

            // TODO: verify Hobs <-> Nobs

            Assert.Equal(0, dependentType.GetForeignKeys().Count(foreignKey => foreignKey != fk));
            Assert.Equal(0, principalType.GetForeignKeys().Count(foreignKey => foreignKey != fk));
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void Can_add_annotations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.FindEntityType(typeof(CustomerDetails));

            var builder = modelBuilder.Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details);
            builder = builder.HasAnnotation("Fus", "Ro");

            var fk = dependentType.FindNavigation(nameof(CustomerDetails.Customer)).ForeignKey;
            Assert.Same(fk, builder.Metadata);
            Assert.Equal("Ro", fk["Fus"]);
        }

        [ConditionalFact]
        public virtual void Nullable_FK_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .HasForeignKey<Nob>(
                    e => new { e.HobId1, e.HobId2 });

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));

            Assert.False(entityType.GetForeignKeys().Single().IsRequired);
            Assert.True(
                entityType.FindProperty(nameof(Nob.HobId1)).IsNullable
                || entityType.FindProperty(nameof(Nob.HobId2)).IsNullable);
        }

        [ConditionalFact]
        public virtual void Non_nullable_FK_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob)
                .HasForeignKey<Hob>(
                    e => new { e.NobId1, e.NobId2 });

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));

            Assert.False(entityType.FindProperty(nameof(Hob.NobId1)).IsNullable);
            Assert.False(entityType.FindProperty(nameof(Hob.NobId2)).IsNullable);
            Assert.True(entityType.GetForeignKeys().Single().IsRequired);
        }

        [ConditionalFact]
        public virtual void Nullable_FK_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>()
                .Ignore(e => e.Hobs)
                .Property(e => e.HobId1).IsRequired(false);

            modelBuilder
                .Entity<Nob>()
                .Property(e => e.HobId2).IsRequired(false);

            modelBuilder
                .Entity<Hob>()
                .Ignore(e => e.Nobs)
                .HasOne(e => e.Nob).WithOne(e => e.Hob)
                .HasForeignKey<Nob>(
                    e => new { e.HobId1, e.HobId2 })
                .IsRequired();

            modelBuilder.FinalizeModel();

            var principalType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();
            var fkProperty1 = dependentType.FindProperty(nameof(Nob.HobId1));
            var fkProperty2 = dependentType.FindProperty(nameof(Nob.HobId2));
            var fk = dependentType.GetForeignKeys().Single();

            Assert.True(fk.IsRequired);
            Assert.True(fkProperty1.IsNullable);
            Assert.True(fkProperty2.IsNullable);
            AssertEqual(new[] { fkProperty1, fkProperty2 }, fk.Properties);
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
        }

        [ConditionalFact]
        public virtual void Non_nullable_FK_can_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .Ignore(e => e.Nobs);

            modelBuilder
                .Entity<Nob>()
                .Ignore(e => e.Hobs)
                .HasOne(e => e.Hob).WithOne(e => e.Nob)
                .HasForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })
                .IsRequired(false);

            modelBuilder.FinalizeModel();

            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var fkProperty1 = dependentType.FindProperty(nameof(Hob.NobId1));
            var fkProperty2 = dependentType.FindProperty(nameof(Hob.NobId2));
            var fk = dependentType.GetForeignKeys().Single();

            Assert.False(fk.IsRequired);
            Assert.False(fkProperty1.IsNullable);
            Assert.False(fkProperty2.IsNullable);
            Assert.Contains(fkProperty1, fk.Properties);
            Assert.Contains(fkProperty2, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Non_nullable_FK_can_be_made_optional_separately()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .Ignore(e => e.Nobs);

            modelBuilder
                .Entity<Nob>()
                .Ignore(e => e.Hobs)
                .HasOne(e => e.Hob).WithOne(e => e.Nob)
                .HasForeignKey<Hob>(e => new { e.NobId1, e.NobId2 });

            modelBuilder
                .Entity<Nob>()
                .HasOne(e => e.Hob).WithOne(e => e.Nob)
                .IsRequired(false);

            modelBuilder.FinalizeModel();

            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var fkProperty1 = dependentType.FindProperty(nameof(Hob.NobId1));
            var fkProperty2 = dependentType.FindProperty(nameof(Hob.NobId2));
            var fk = dependentType.GetForeignKeys().Single();

            Assert.False(fk.IsRequired);
            Assert.False(fkProperty1.IsNullable);
            Assert.False(fkProperty2.IsNullable);
            Assert.Contains(fkProperty1, fk.Properties);
            Assert.Contains(fkProperty2, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Unspecified_FK_can_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();
            var principalType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .IsRequired(false)
                .HasPrincipalKey<Nob>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
        }

        [ConditionalFact]
        public virtual void Unspecified_FK_can_be_made_optional_in_any_order()
        {
            var modelBuilder = HobNobBuilder();
            var principalType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .HasPrincipalKey<Nob>(e => new { e.Id1, e.Id2 })
                .IsRequired(false);

            var fk = dependentType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
        }

        [ConditionalFact]
        public virtual void Unspecified_FK_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();
            var principalType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .HasPrincipalKey<Nob>(
                    e => new { e.Id1, e.Id2 })
                .IsRequired();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.True(fk.IsRequired);
            Assert.True(fk.Properties.All(p => !p.IsNullable));

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
        }

        [ConditionalFact]
        public virtual void Throws_if_ambiguous_FK_made_required()
        {
            var modelBuilder = HobNobBuilder();

            var foreignKeyBuilder = modelBuilder
                .Entity<Hob>()
                .HasOne(e => e.Nob).WithOne(e => e.Hob)
                .IsRequired();

            Assert.Contains(
                modelBuilder.ModelLoggerFactory.Log, l => l.Level == LogLevel.Warning
                    && l.Message
                    == CoreResources.LogAmbiguousEndRequired(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage("{'NobId11', 'NobId21'}", typeof(Hob).Name));

            Assert.Equal(
                CoreStrings.AmbiguousEndRequiredInverted("{'NobId11', 'NobId21'}", typeof(Hob).Name, typeof(Nob).Name),
                Assert.Throws<InvalidOperationException>(
                    () => foreignKeyBuilder.HasForeignKey<Nob>()).Message);
        }

        [ConditionalFact]
        public virtual void Can_be_defined_before_the_PK_from_principal()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Nob>();
            modelBuilder
                .Entity<Hob>(
                    eb =>
                    {
                        eb.HasOne(e => e.Nob).WithOne(e => e.Hob)
                            .HasForeignKey<Nob>(
                                e => new { e.HobId1, e.HobId2 })
                            .HasPrincipalKey<Hob>(
                                e => new { e.Id1, e.Id2 });
                        eb.HasKey(
                            e => new { e.Id1, e.Id2 });
                    });

            modelBuilder.Entity<Nob>().HasKey(
                e => new { e.Id1, e.Id2 });

            var dependentEntityType = modelBuilder.Model.FindEntityType(typeof(Nob));
            var fk = dependentEntityType.GetForeignKeys().Single();
            AssertEqual(
                new[] { dependentEntityType.FindProperty("HobId1"), dependentEntityType.FindProperty("HobId2") }, fk.Properties);
            Assert.False(fk.IsRequired);
            var principalEntityType = modelBuilder.Model.FindEntityType(typeof(Hob));
            AssertEqual(fk.PrincipalKey.Properties, principalEntityType.FindPrimaryKey().Properties);
        }

        [ConditionalFact]
        public virtual void Can_be_defined_before_the_PK_from_dependent()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Nob>();
            modelBuilder
                .Entity<Hob>(
                    eb =>
                    {
                        eb.HasOne(e => e.Nob).WithOne(e => e.Hob)
                            .HasForeignKey<Hob>(
                                e => new { e.NobId1, e.NobId2 })
                            .HasPrincipalKey<Nob>(
                                e => new { e.Id1, e.Id2 });
                        eb.HasKey(
                            e => new { e.Id1, e.Id2 });
                    });

            modelBuilder.Entity<Nob>().HasKey(
                e => new { e.Id1, e.Id2 });

            var dependentEntityType = modelBuilder.Model.FindEntityType(typeof(Hob));
            var fk = dependentEntityType.GetForeignKeys().Single();
            AssertEqual(
                new[] { dependentEntityType.FindProperty("NobId1"), dependentEntityType.FindProperty("NobId2") }, fk.Properties);
            Assert.True(fk.IsRequired);
            var principalEntityType = modelBuilder.Model.FindEntityType(typeof(Nob));
            AssertEqual(fk.PrincipalKey.Properties, principalEntityType.FindPrimaryKey().Properties);
        }

        [ConditionalFact]
        public virtual void Can_change_delete_behavior()
        {
            var modelBuilder = HobNobBuilder();
            var dependentType = modelBuilder.Model.FindEntityType(typeof(Hob));

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .OnDelete(DeleteBehavior.Cascade);

            Assert.Equal(DeleteBehavior.Cascade, dependentType.GetNavigations().Single().ForeignKey.DeleteBehavior);

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .OnDelete(DeleteBehavior.Restrict);

            Assert.Equal(DeleteBehavior.Restrict, dependentType.GetNavigations().Single().ForeignKey.DeleteBehavior);

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .OnDelete(DeleteBehavior.SetNull);

            Assert.Equal(DeleteBehavior.SetNull, dependentType.GetNavigations().Single().ForeignKey.DeleteBehavior);
        }

        [ConditionalFact]
        public virtual void Configuring_FK_properties_as_PK_sets_DeleteBehavior_Cascade()
        {
            var modelBuilder = HobNobBuilder();
            var dependentType = modelBuilder.Model.FindEntityType(typeof(Hob));

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .HasForeignKey<Nob>(e => e.HobId1);

            Assert.Equal(DeleteBehavior.ClientSetNull, dependentType.GetNavigations().Single().ForeignKey.DeleteBehavior);

            modelBuilder
                .Entity<Nob>().HasKey(e => e.HobId1);

            Assert.Equal(DeleteBehavior.Cascade, dependentType.GetNavigations().Single().ForeignKey.DeleteBehavior);
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_property_with_non_shadow_PK()
        {
            var modelBuilder = CreateModelBuilder();

            // For NonGenericStringTest
            modelBuilder.Entity<Alpha>();

            modelBuilder.Entity<Beta>(
                b =>
                {
                    b.HasOne(e => e.FirstNav)
                        .WithOne()
                        .HasForeignKey<Beta>("ShadowId");
                });

            Assert.Equal(
                "ShadowId",
                modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_property_with_shadow_PK()
        {
            var modelBuilder = CreateModelBuilder();

            // For NonGenericStringTest
            var entityA = modelBuilder.Entity<Alpha>();
            entityA.Property<int>("ShadowPK");
            entityA.HasKey("ShadowPK");

            var entityB = modelBuilder.Entity<Beta>();

            entityB.HasOne(e => e.FirstNav).WithOne().HasForeignKey<Beta>("ShadowId");

            Assert.Equal(
                "ShadowId",
                modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Creates_field_backed_FK_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Alpha>();

            modelBuilder.Entity<Quarks>(
                b => b.HasOne<Beta>().WithOne().HasForeignKey<Quarks>("_forUp").IsRequired());

            var fkProperty = modelBuilder.Model.FindEntityType(typeof(Quarks)).GetForeignKeys().Single().Properties.Single();
            Assert.Equal("_forUp", fkProperty.Name);
            Assert.Equal(typeof(int), fkProperty.ClrType);
            Assert.Equal("_forUp", fkProperty.FieldInfo.Name);
        }

        [ConditionalFact]
        public virtual void Handles_identity_correctly_while_removing_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Epsilon>();
            modelBuilder.Entity<Alpha>().HasOne(b => b.NavDelta).WithOne();

            var property = modelBuilder.Model.FindEntityType(typeof(Delta)).FindProperty("Id");
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Throws_if_multiple_navigations_are_pointing_to_same_foreign_key_property()
        {
            var modelBuilder = CreateModelBuilder();
            Assert.Equal(
                CoreStrings.MultipleNavigationsSameFk(typeof(Zeta).Name, "CommonFkProperty", "'AlphaOne', 'AlphaTwo'"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Zeta>().HasOne<Alpha>().WithOne()).Message);
        }

        [ConditionalFact]
        public virtual void Can_create_self_referencing_relationship_without_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>(
                b =>
                {
                    b.Ignore(e => e.SelfRef1);
                    b.Ignore(e => e.SelfRef2);
                });
            var relationship = modelBuilder.Entity<SelfRef>()
                .HasOne<SelfRef>()
                .WithOne()
                .Metadata;

            var entityType = modelBuilder.Model.FindEntityType(typeof(SelfRef));
            Assert.Empty(entityType.GetNavigations());
            Assert.Same(relationship, entityType.GetForeignKeys().Single());
            Assert.Null(relationship.PrincipalToDependent);
            Assert.Null(relationship.DependentToPrincipal);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_if_dependent_has_matching_property_with_navigation_name()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOneDependentEntityId);
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                });
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.OneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntity)).FindNavigation("NavOneToOneDependentEntity")
                .ForeignKey;

            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal("NavOneToOnePrincipalEntityId", fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_if_dependent_has_matching_property_with_entity_type_name()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOneDependentEntityId);
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                });
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.NavOneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntity)).FindNavigation("NavOneToOneDependentEntity")
                .ForeignKey;

            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal("OneToOnePrincipalEntityId", fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_invert_relationship_if_principal_has_matching_property_with_navigation_name()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.OneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntity)).FindNavigation("NavOneToOneDependentEntity")
                .ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal("NavOneToOneDependentEntityId", fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_invert_relationship_if_principal_has_matching_property_with_entity_type_name()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.NavOneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntity)).FindNavigation("NavOneToOneDependentEntity")
                .ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal("OneToOneDependentEntityId", fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Ignoring_properties_resolves_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity).IsRequired();

            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b => b.Ignore(e => e.NavOneToOneDependentEntityId));

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Ignoring_properties_on_principal_resolves_ambiguity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneToOneDependentEntity>(
                b => b.Ignore(e => e.OneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOneDependentEntityId);
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                });

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Throws_for_one_to_one_relationship_if_no_side_has_matching_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOneDependentEntityId);
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                });
            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(OneToOnePrincipalEntity).Name + "." + OneToOnePrincipalEntity.NavigationProperty.Name,
                    typeof(OneToOneDependentEntity).Name + "." + OneToOneDependentEntity.NavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_one_to_one_relationship_if_no_side_has_matching_property_anymore()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity).IsRequired();

            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.OneToOneDependentEntityId);
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                });

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(OneToOnePrincipalEntity).Name + "." + OneToOnePrincipalEntity.NavigationProperty.Name,
                    typeof(OneToOneDependentEntity).Name + "." + OneToOneDependentEntity.NavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_one_to_one_relationship_if_both_sides_have_matching_property_with_navigation_name()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.OneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.OneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(OneToOnePrincipalEntity).Name + "." + OneToOnePrincipalEntity.NavigationProperty.Name,
                    typeof(OneToOneDependentEntity).Name + "." + OneToOneDependentEntity.NavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_one_to_one_relationship_if_both_sides_have_matching_property_with_entity_type_name()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.NavOneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.NavOneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(OneToOnePrincipalEntity).Name + "." + OneToOnePrincipalEntity.NavigationProperty.Name,
                    typeof(OneToOneDependentEntity).Name + "." + OneToOneDependentEntity.NavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_one_to_one_relationship_if_both_sides_have_matching_property_mixed()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.NavOneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.OneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(OneToOnePrincipalEntity).Name + "." + OneToOnePrincipalEntity.NavigationProperty.Name,
                    typeof(OneToOneDependentEntity).Name + "." + OneToOneDependentEntity.NavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_one_to_one_relationship_if_both_sides_have_matching_PKs()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.Id);
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                    b.Ignore(e => e.OneToOneDependentEntityId);
                    b.Property<int>("Key");
                    b.HasKey("Key");
                });
            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.Id);
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                    b.Property<int>("Key");
                    b.HasKey("Key");
                });

            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    typeof(OneToOnePrincipalEntity).Name + "." + OneToOnePrincipalEntity.NavigationProperty.Name,
                    typeof(OneToOneDependentEntity).Name + "." + OneToOneDependentEntity.NavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_if_user_specify_foreign_key_property()
        {
            var modelBuilder = CreateModelBuilder();

            // For Non-Generic string test
            modelBuilder.Entity<OneToOneDependentEntity>();
            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity).HasForeignKey<OneToOneDependentEntity>(e => e.NavOneToOnePrincipalEntityId);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntity)).FindNavigation("NavOneToOneDependentEntity")
                .ForeignKey;

            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal("NavOneToOnePrincipalEntityId", fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_if_user_specifies_principal_key_property()
        {
            var modelBuilder = CreateModelBuilder();

            // For Non-Generic string test
            modelBuilder.Entity<OneToOneDependentEntity>();
            modelBuilder.Entity<OneToOnePrincipalEntity>().HasOne(e => e.NavOneToOneDependentEntity)
                .WithOne(e => e.NavOneToOnePrincipalEntity)
                .HasPrincipalKey<OneToOneDependentEntity>(e => e.NavOneToOnePrincipalEntityId);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntity))
                .FindNavigation(nameof(OneToOnePrincipalEntity.NavOneToOneDependentEntity)).ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal(nameof(OneToOneDependentEntity.NavOneToOnePrincipalEntityId), fk.PrincipalKey.Properties.First().Name);
            Assert.Equal(nameof(OneToOnePrincipalEntity.NavOneToOneDependentEntityId), fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_create_relationship_if_foreign_key_attribute_is_used()
        {
            var modelBuilder = CreateModelBuilder();

            // For Non-Generic string test
            modelBuilder.Entity<OneToOneDependentEntityWithAnnotation>();
            modelBuilder.Entity<OneToOnePrincipalEntityWithAnnotation>().HasOne(e => e.NavOneToOneDependentEntityWithAnnotation)
                .WithOne(e => e.NavOneToOnePrincipalEntityWithAnnotation);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOnePrincipalEntityWithAnnotation))
                .FindNavigation("NavOneToOneDependentEntityWithAnnotation").ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntityWithAnnotation), fk.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntityWithAnnotation), fk.PrincipalEntityType.ClrType);
            Assert.Equal("FkProperty", fk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_use_pk_as_fk_if_principal_end_is_specified()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Entity<BookDetails>().Ignore(b => b.AnotherBookId);

            modelBuilder.Entity<Book>().HasOne(e => e.Details).WithOne(e => e.AnotherBook).HasPrincipalKey<Book>(e => e.Id);

            Assert.Equal(
                "Id",
                modelBuilder.Model.FindEntityType(typeof(Book)).FindNavigation(Book.BookDetailsNavigation.Name).ForeignKey.Properties
                    .Single().Name);
        }

        [ConditionalFact]
        public virtual void Does_not_use_pk_as_fk_if_principal_end_is_not_specified()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<BookLabel>();
            modelBuilder.Entity<BookDetails>().Ignore(b => b.AnotherBookId);

            var fk = modelBuilder.Entity<Book>().HasOne(e => e.Details).WithOne(e => e.AnotherBook).Metadata;

            Assert.NotEqual("Id", fk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void
            Creates_one_to_one_relationship_with_single_ref_as_principal_to_dependent_if_matching_properties_are_on_the_other_side()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.NavOneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity))
                .FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

            Assert.Equal(typeof(OneToOneDependentEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.DeclaringEntityType.ClrType);
            Assert.True(fk.IsUnique);
            Assert.Null(fk.DependentToPrincipal);
            Assert.False(fk.Properties.Single().IsShadowProperty());
            Assert.Equal(OneToOnePrincipalEntity.EntityMatchingProperty.Name, fk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Multiple_self_referencing_navigations_throw_as_ambiguous()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<User>();

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship("User.CreatedBy", "User.UpdatedBy"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Navigation_properties_can_set_access_mode()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<NavDependent>()
                .HasOne(e => e.OneToOnePrincipal)
                .WithOne(e => e.Dependent);

            modelBuilder.Entity<NavDependent>()
                .Navigation(e => e.OneToOnePrincipal)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<OneToOneNavPrincipal>()
                .Navigation(e => e.Dependent)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            var principal = (IReadOnlyEntityType)model.FindEntityType(typeof(OneToOneNavPrincipal));
            var dependent = (IReadOnlyEntityType)model.FindEntityType(typeof(NavDependent));

            Assert.Equal(PropertyAccessMode.Field, principal.FindNavigation("Dependent").GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, dependent.FindNavigation("OneToOnePrincipal").GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Navigation_to_shared_type_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ReferenceNavigationToSharedType>();

            Assert.Equal(
                CoreStrings.NonConfiguredNavigationToSharedType("Navigation", nameof(ReferenceNavigationToSharedType)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Inverse_discovered_after_entity_unignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<ValueCategory>();
            modelBuilder.Ignore<Value>();
            modelBuilder.Entity<QueryResult>()
                .Property(x => x.Id)
                .HasConversion(x => x.Id, x => new CustomId { Id = x });
            modelBuilder.Entity<Value>();

            var model = modelBuilder.FinalizeModel();

            var queryResult = model.FindEntityType(typeof(QueryResult));
            Assert.NotNull(queryResult.FindNavigation(nameof(QueryResult.Value)));
            Assert.Null(queryResult.FindProperty("TempId"));
        }
    }
}
