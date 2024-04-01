// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

#nullable disable

public abstract partial class ModelBuilderTest
{
    public abstract class OneToManyTestBase(ModelBuilderFixtureBase fixture) : ModelBuilderTestBase(fixture)
    {
        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer).WithMany(c => c.Orders)
                .HasForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var fk = dependentType.GetForeignKeys().Single();

            var navToPrincipal = dependentType.FindNavigation("Customer");
            var navToDependent = principalType.FindNavigation("Orders");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            modelBuilder.FinalizeModel();

            Assert.Same(fk, dependentType.GetForeignKeys().Single());
            Assert.Equal(navToPrincipal.Name, dependentType.GetNavigations().Single().Name);
            Assert.Same(navToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK_with_fields()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<OneToManyPrincipalWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.AlternateKey);
                    e.Property(p => p.Name);
                    e.HasKey(p => p.Id);
                });
            modelBuilder.Entity<DependentWithField>(
                e =>
                {
                    e.Property(d => d.DependentWithFieldId);
                    e.Property(d => d.OneToManyPrincipalId);
                    e.Property(d => d.AnotherOneToManyPrincipalId);
                    e.Ignore(d => d.ManyToManyPrincipals);
                    e.Ignore(d => d.OneToOnePrincipal);
                    e.HasKey(d => d.DependentWithFieldId);
                });

            modelBuilder.Entity<DependentWithField>()
                .HasOne(d => d.OneToManyPrincipal)
                .WithMany(p => p.Dependents)
                .HasForeignKey(d => d.OneToManyPrincipalId);

            var dependentType = model.FindEntityType(typeof(DependentWithField));
            var principalType = model.FindEntityType(typeof(OneToManyPrincipalWithField));
            var fk = dependentType.GetForeignKeys().Single();

            var navToPrincipal = dependentType.FindNavigation("OneToManyPrincipal");
            var navToDependent = principalType.FindNavigation("Dependents");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<DependentWithField>()
                .HasOne(d => d.OneToManyPrincipal)
                .WithMany(p => p.Dependents);

            modelBuilder.FinalizeModel();

            Assert.Same(fk, dependentType.GetForeignKeys().Single());
            Assert.Equal(navToPrincipal.Name, dependentType.GetNavigations().Single().Name);
            Assert.Same(navToDependent, principalType.GetNavigations().Single());
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigation_to_principal_and_uses_associated_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>().HasOne(c => c.Customer).WithMany()
                .HasForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigation_to_dependent_and_uses_associated_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne()
                .HasForeignKey(e => e.CustomerId);
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var fk = principalType.GetNavigations().Single().ForeignKey;

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            modelBuilder.FinalizeModel();

            var newFk = principalType.GetNavigations().Single().ForeignKey;
            AssertEqual(fk.Properties, newFk.Properties);
            Assert.Same(newFk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(Customer.Orders), newFk.PrincipalToDependent.Name);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_existing_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var fk = dependentType.GetForeignKeys().Single();

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            modelBuilder.FinalizeModel();

            Assert.Same(fk, dependentType.GetForeignKeys().Single());
            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_both_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty("CustomerId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty(nameof(Order.CustomerId));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne();

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Customer.Orders), fk.PrincipalToDependent.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.NotNull(dependentType.FindForeignKeys(fkProperty).SingleOrDefault());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty(nameof(Order.CustomerId));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany<Order>().WithOne(e => e.Customer);

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.NotNull(dependentType.FindForeignKeys(fkProperty).SingleOrDefault());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_no_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty("CustomerId");
            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Customer>().HasMany<Order>().WithOne();

            var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);

            Assert.Equal(fk.PrincipalKey.Properties, newFk.PrincipalKey.Properties);
            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
            Assert.Contains("AlternateKey", principalType.GetProperties().Select(p => p.Name));
            Assert.Contains("AnotherCustomerId", dependentType.GetProperties().Select(p => p.Name));
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_specified_FK_even_if_found_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty("CustomerId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_existing_FK_not_found_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany()
                .HasForeignKey(c => c.BurgerId);
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));
            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.Single().Name == "BurgerId");
            fk.SetDependentToPrincipal((string)null);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            Assert.Same(fk, dependentType.GetForeignKeys().Single());
            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_creates_FK_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_specified_FK_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty(nameof(Pickle.BurgerId));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne()
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(BigMak.Pickles), fk.PrincipalToDependent.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_specified_FK_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak)
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(dependentType.FindProperty(nameof(Pickle.BurgerId)), fk.Properties.Single());
            Assert.Equal(nameof(Pickle.BigMak), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_no_navigations_and_specified_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");
            var fk = dependentType.GetForeignKeys().SingleOrDefault();

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany<Pickle>().WithOne()
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
            Assert.Same(fkProperty, newFk.Properties.Single());

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != fk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != fk));
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_creates_shadow_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            var fkProperty = (IReadOnlyProperty)fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty());
            Assert.Same(typeof(int?), fkProperty.ClrType);
            Assert.Same(dependentType, fkProperty.DeclaringType);

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne();

            modelBuilder.FinalizeModel();

            var fk = principalType.GetNavigations().Single().ForeignKey;
            var fkProperty = (IReadOnlyProperty)fk.Properties.Single();

            Assert.True(fkProperty.IsShadowProperty());
            Assert.Same(typeof(int?), fkProperty.ClrType);
            Assert.Same(dependentType, fkProperty.DeclaringType);

            Assert.Equal(nameof(BigMak.Pickles), fk.PrincipalToDependent.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            var fkProperty = (IReadOnlyProperty)fk.Properties.Single();

            Assert.True(fkProperty.IsShadowProperty());
            Assert.Same(typeof(int?), fkProperty.ClrType);
            Assert.Same(dependentType, fkProperty.DeclaringType);

            Assert.Equal(nameof(Pickle.BigMak), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_with_no_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();
            var existingFk = dependentType.GetForeignKeys().Single();

            modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne();

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            var fkProperty = (IReadOnlyProperty)fk.Properties.Single();

            Assert.Equal("BigMakId1", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty());
            Assert.Same(typeof(int?), fkProperty.ClrType);
            Assert.Same(dependentType, fkProperty.DeclaringType);

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_matches_shadow_FK_property_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            var fkProperty = dependentType.FindProperty("BigMakId");

            modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_overrides_existing_FK_when_uniqueness_does_not_match()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>().HasOne<Pickle>().WithOne()
                .HasForeignKey<Pickle>(e => e.BurgerId);
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            var fk = dependentType.GetForeignKeys()
                .Single(foreignKey => foreignKey.Properties.Any(p => p.Name == "BurgerId"));
            Assert.True(((IReadOnlyForeignKey)fk).IsUnique);

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            Assert.Single(dependentType.GetForeignKeys());
            Assert.False(fk.IsUnique);

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Resolves_ambiguous_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Friendship>().HasOne(e => e.ApplicationUser).WithMany(e => e.Friendships)
                .HasForeignKey(e => e.ApplicationUserId);

            modelBuilder.FinalizeModel();

            Assert.Equal(2, model.FindEntityType(typeof(Friendship)).GetNavigations().Count());
        }

        [ConditionalFact]
        public virtual void Can_use_explicitly_specified_PK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty("CustomerId");
            var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);

            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_use_non_PK_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasPrincipalKey(e => e.AlternateKey);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Throws_on_keyless_type_as_principal()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<CustomerDetails>();

            Assert.Equal(
                CoreStrings.PrincipalKeylessType(
                    nameof(Customer),
                    nameof(Customer) + "." + nameof(Customer.Orders),
                    nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder.Entity<Customer>().HasNoKey()
                            .HasMany(c => c.Orders)
                            .WithOne(o => o.Customer)).Message);
        }

        [ConditionalFact]
        public virtual void Keyless_type_with_unmapped_collection_navigations_does_not_throw()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<CustomerDetails>();

            modelBuilder.Entity<Customer>().HasNoKey();

            var model = modelBuilder.FinalizeModel();

            var customer = model.FindEntityType(typeof(Customer));
            Assert.Empty(customer.GetNavigations());
            Assert.Null(customer.FindPrimaryKey());
            Assert.Null(model.FindEntityType(typeof(Order)));
        }

        [ConditionalFact]
        public virtual void Keyless_type_discovered_before_referenced_entity_type_does_not_leave_temp_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Order>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<Product>();

            modelBuilder.Entity<KeylessEntity>().HasNoKey();
            modelBuilder.Entity<Customer>();

            var model = modelBuilder.FinalizeModel();

            var keyless = model.FindEntityType(typeof(KeylessEntity));
            Assert.Null(model.FindEntityType(typeof(Customer))?.FindProperty("TempId"));
            Assert.Null(keyless.FindPrimaryKey());

            var customerNavigation = keyless.GetNavigations().Single();
            Assert.Same(keyless, customerNavigation.ForeignKey.DeclaringEntityType);
        }

        [ConditionalFact]
        public virtual void Can_have_both_convention_properties_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty("CustomerId");
            var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_both_convention_properties_specified_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<BackOrder>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));

            var fkProperty = dependentType.FindProperty("CustomerId");
            var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasPrincipalKey(e => e.Id)
                .HasForeignKey(e => e.CustomerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasForeignKey(e => e.AnotherCustomerId)
                .HasPrincipalKey(e => e.AlternateKey);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
            Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasPrincipalKey(e => e.AlternateKey)
                .HasForeignKey(e => e.AnotherCustomerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
            Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_FK_semi_specified_with_explicit_PK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasForeignKey("CustomerAlternateKey")
                .IsRequired();

            modelBuilder
                .Entity<Customer>()
                .HasKey(o => o.AlternateKey);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            var principalKey = principalType.FindPrimaryKey();
            Assert.Same(principalKey, fk.PrincipalKey);

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            var fkProperty = fk.Properties.Single();
            Assert.False(fkProperty.IsNullable);
            expectedDependentProperties.Add(fkProperty);
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_specify_requiredness_after_OnDelete()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<Product>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.FindEntityType(typeof(Order));
            var principalType = model.FindEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasForeignKey("CustomerAlternateKey")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();

            Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            var principalKey = principalType.FindPrimaryKey();
            Assert.Same(principalKey, fk.PrincipalKey);

            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            var fkProperty = fk.Properties.Single();
            Assert.False(fkProperty.IsNullable);
            expectedDependentProperties.Add(fkProperty);
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");
            var principalProperty = principalType.FindProperty("AlternateKey");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasForeignKey(e => e.BurgerId)
                .HasPrincipalKey(e => e.AlternateKey);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var fkProperty = dependentType.FindProperty("BurgerId");
            var principalProperty = principalType.FindProperty("AlternateKey");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasPrincipalKey(e => e.AlternateKey)
                .HasForeignKey(e => e.BurgerId);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_have_principal_key_by_convention_replaced_with_primary_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasForeignKey(e => e.BurgerId);
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var dependentKey = dependentType.FindPrimaryKey();

            var expectedPrincipalProperties = principalType.GetProperties().ToList();
            var expectedDependentProperties = dependentType.GetProperties().ToList();

            modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            var principalProperty = principalType.FindProperty("AlternateKey");

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());

            var principalKey = principalType.FindPrimaryKey();
            Assert.Same(principalProperty, principalKey.Properties.Single());
            Assert.Same(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Principal_key_by_convention_is_not_replaced_with_new_incompatible_primary_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasForeignKey(e => new { e.BurgerId, e.Id });
            modelBuilder.Ignore<Bun>();

            var dependentType = model.FindEntityType(typeof(Pickle));
            var principalType = model.FindEntityType(typeof(BigMak));

            var modelClone = Clone(modelBuilder.Model);
            var nonPrimaryPrincipalKey = modelClone.FindEntityType(typeof(BigMak).FullName)
                .GetKeys().First(k => !k.IsPrimaryKey());
            var dependentKey = dependentType.FindPrimaryKey();

            var expectedDependentProperties = dependentType.GetProperties().ToList();
            var expectedPrincipalProperties = principalType.GetProperties().ToList();

            modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

            var principalProperty = principalType.FindProperty("AlternateKey");
            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal(2, fk.Properties.Count);

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equivalent(expectedPrincipalProperties.Select(p => p.Name), principalType.GetProperties().Select(p => p.Name));
            AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            Assert.Empty(principalType.GetForeignKeys());

            var primaryPrincipalKey = principalType.FindPrimaryKey();
            Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());

            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Explicit_principal_key_is_not_replaced_with_new_primary_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .HasPrincipalKey(e => new { e.Id });
            modelBuilder.Ignore<Bun>();

            var principalType = model.FindEntityType(typeof(BigMak));
            var dependentType = model.FindEntityType(typeof(Pickle));

            var nonPrimaryPrincipalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

            var principalProperty = principalType.FindProperty("AlternateKey");
            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(nonPrimaryPrincipalKey, fk.PrincipalKey);

            Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            var primaryPrincipalKey = principalType.FindPrimaryKey();
            Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
            Assert.Contains(nonPrimaryPrincipalKey, principalType.GetKeys());
            var oldKeyProperty = principalType.FindProperty(nameof(BigMak.Id));
            var newKeyProperty = principalType.FindProperty(nameof(BigMak.AlternateKey));
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_uses_existing_composite_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                .HasForeignKey(c => new { c.BurgerId1, c.BurgerId2 });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Equal(nameof(Tomato.Whoopper), dependentType.GetNavigations().Single().Name);
            Assert.Equal(nameof(Whoopper.Tomatoes), principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_both_navigations_and_creates_composite_FK_specified()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>(
                b => b.HasKey(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_use_alternate_composite_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>(
                b => b.HasKey(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");
            var principalProperty1 = principalType.FindProperty("AlternateKey1");
            var principalProperty2 = principalType.FindProperty("AlternateKey2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
            Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Can_use_alternate_composite_key_in_any_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>(
                b => b.HasKey(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");
            var principalProperty1 = principalType.FindProperty("AlternateKey1");
            var principalProperty2 = principalType.FindProperty("AlternateKey2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
            Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

            Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
            Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
            Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
            Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Empty(principalType.GetForeignKeys());

            Assert.Contains(principalKey, principalType.GetKeys());
            Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
            Assert.NotSame(principalKey, fk.PrincipalKey);

            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_specified_composite_FK_with_navigation_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(
                b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.FindProperty(nameof(Tomato.BurgerId1));
            var fkProperty2 = dependentType.FindProperty(nameof(Tomato.BurgerId2));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne()
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = principalType.GetNavigations().Single().ForeignKey;
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal(nameof(Whoopper.Tomatoes), fk.PrincipalToDependent.Name);
            Assert.Null(fk.DependentToPrincipal);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_specified_composite_FK_with_navigation_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany<Tomato>().WithOne(e => e.Whoopper)
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var fk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(dependentType.FindProperty(nameof(Tomato.BurgerId1)), fk.Properties[0]);
            Assert.Same(dependentType.FindProperty(nameof(Tomato.BurgerId2)), fk.Properties[1]);

            Assert.Equal(nameof(Tomato.Whoopper), fk.DependentToPrincipal.Name);
            Assert.Null(fk.PrincipalToDependent);
            Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Whoopper>().HasMany(w => w.Tomatoes).WithOne(t => t.Whoopper);
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<Mustard>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.FindEntityType(typeof(Tomato));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var fk = dependentType.GetForeignKeys().SingleOrDefault();
            var fkProperty1 = dependentType.FindProperty("BurgerId1");
            var fkProperty2 = dependentType.FindProperty("BurgerId2");

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany<Tomato>().WithOne()
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            modelBuilder.FinalizeModel();

            var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
            Assert.Same(fkProperty1, newFk.Properties[0]);
            Assert.Same(fkProperty2, newFk.Properties[1]);

            Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
            Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            Assert.Empty(principalType.GetIndexes());
        }

        [ConditionalFact]
        public virtual void Creates_relationship_on_existing_FK_is_using_different_principal_key()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .HasForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany<ToastedBun>().WithOne()
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var navigation = dependentType.GetNavigations().Single();
            var existingFk = navigation.ForeignKey;
            Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(ToastedBun.Whoopper), navigation.Name);
            Assert.Equal(nameof(Whoopper.ToastedBun), navigation.Inverse.Name);
            Assert.Equal(existingFk.DeclaringEntityType == dependentType ? 0 : 1, principalType.GetForeignKeys().Count());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.NotSame(principalKey, fk.PrincipalKey);
            Assert.NotEqual(existingFk.Properties, fk.Properties);
            Assert.Equal(principalType.GetForeignKeys().Count(), principalType.GetIndexes().Count());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.True(existingFk.DeclaringEntityType.FindIndex(existingFk.Properties).IsUnique);
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);

            Assert.Equal(
                CoreStrings.AmbiguousOneToOneRelationship(
                    existingFk.DeclaringEntityType.DisplayName() + "." + existingFk.DependentToPrincipal.Name,
                    existingFk.PrincipalEntityType.DisplayName() + "." + existingFk.PrincipalToDependent.Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Creates_relationship_on_existing_FK_is_using_different_principal_key_different_order()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .HasForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Mustard>();

            var dependentType = model.FindEntityType(typeof(ToastedBun));
            var principalType = model.FindEntityType(typeof(Whoopper));

            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany<ToastedBun>().WithOne()
                .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var existingFk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
            Assert.Equal(nameof(Tomato.Whoopper), existingFk.DependentToPrincipal.Name);
            Assert.Equal(nameof(Whoopper.ToastedBun), existingFk.PrincipalToDependent.Name);
            Assert.Empty(principalType.GetForeignKeys());
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());

            var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
            Assert.NotSame(principalKey, fk.PrincipalKey);
            Assert.Equal(existingFk.Properties, fk.Properties);
            Assert.Empty(principalType.GetIndexes());
            Assert.Single(dependentType.GetIndexes());
            Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Creates_overlapping_foreign_keys_with_different_nullability()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>(
                eb =>
                {
                    eb.HasKey(c => new { c.OrderId, c.CustomerId });
                });

            modelBuilder.Ignore<ProductCategory>();
            modelBuilder.Entity<Category>(eb => { eb.HasKey(c => new { c.Id, c.Name }); });

            modelBuilder.Entity<Product>(
                eb =>
                {
                    eb.Ignore(p => p.Categories);
                    eb.HasOne(p => p.Order).WithMany(o => o.Products).HasForeignKey("CommonId", "OrderId");
                    eb.HasOne<Category>().WithMany(c => c.Products).HasForeignKey("CommonId", "Category").IsRequired();

                    eb.HasIndex("Id", "OrderId").IsUnique();
                    eb.HasKey("Id", "CommonId");
                });

            modelBuilder.FinalizeModel();

            var dependentType = model.FindEntityType(typeof(Product));

            var optionalFk = dependentType.GetNavigations().Single().ForeignKey;
            Assert.False(optionalFk.IsRequired);
            Assert.True(optionalFk.Properties.Last().IsNullable);

            var requiredFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != optionalFk);
            Assert.True(requiredFk.IsRequired);
            Assert.False(requiredFk.Properties.Last().IsNullable);

            var dependentKey = dependentType.FindPrimaryKey();
            Assert.True(dependentKey.Properties.All(p => p.ValueGenerated == ValueGenerated.Never));

            var index = dependentType.FindIndex(new[] { dependentKey.Properties[0], optionalFk.Properties[1] });
            Assert.True(index.IsUnique);
        }

        [ConditionalFact]
        public virtual void Throws_on_existing_many_to_many()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithMany(c => c.Categories);

            Assert.Equal(
                CoreStrings.ConflictingRelationshipNavigation(
                    nameof(Category) + "." + nameof(Category.Products),
                    nameof(Product),
                    nameof(Category) + "." + nameof(Category.Products),
                    nameof(Product) + "." + nameof(Product.Categories)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Category>()
                        .HasMany(o => o.Products).WithOne()).Message);
        }

        [ConditionalFact]
        public virtual void Throws_on_existing_one_to_one_relationship()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;

            // set up a 1:1 relationship using Nob.Hob and Hob.Nob
            modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

            // Now that Nob.Hob and Hob.Nob are used, Nob.Hobs and Hob.Nobs
            // are no longer ambiguous and we do implicitly create the N:N
            // relationship between them. But this can be silently overridden
            // by the more explicit HasMany().WithOne() call below. So we do
            // not see a clash with that relationship, only with the 1:1
            // relationship configured above.

            var dependentType = model.FindEntityType(typeof(Hob));
            var principalType = model.FindEntityType(typeof(Nob));

            Assert.Equal(
                CoreStrings.ConflictingRelationshipNavigation(
                    principalType.DisplayName() + "." + nameof(Nob.Hobs),
                    dependentType.DisplayName() + "." + nameof(Hob.Nob),
                    dependentType.DisplayName() + "." + nameof(Hob.Nob),
                    principalType.DisplayName() + "." + nameof(Nob.Hob)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        modelBuilder.Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)).Message);
        }

        [ConditionalFact]
        public virtual void HasMany_with_a_collection_navigation_CLR_property_to_derived_type_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.NavigationCollectionWrongClrType(nameof(Dr.Jrs), nameof(Dr), "ICollection<DreJr>", nameof(Dre)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Dr>().HasMany<Dre>(d => d.Jrs)).Message);
        }

        [ConditionalFact]
        public virtual void Removes_existing_unidirectional_one_to_one_relationship()
        {
            var modelBuilder = HobNobBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

            // The below ensures that the relationship is no longer
            // using Nob.Hob. After that it is allowed to override
            // Hob.Nob's inverse in the HasMany().WithOne() call below.
            modelBuilder.Entity<Nob>().HasOne<Hob>().WithOne(e => e.Nob);

            var dependentType = model.FindEntityType(typeof(Hob));
            var principalType = model.FindEntityType(typeof(Nob));
            var principalKey = principalType.FindPrimaryKey();
            var dependentKey = dependentType.FindPrimaryKey();

            modelBuilder.Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob);

            // assert the 1:N relationship defined through the HasMany().WithOne() call above
            var fk = dependentType.GetForeignKeys().Single();
            Assert.False(fk.IsUnique);
            Assert.Equal(nameof(Nob.Hobs), fk.PrincipalToDependent.Name);
            Assert.Equal(nameof(Hob.Nob), fk.DependentToPrincipal.Name);

            // The 1:N relationship above has "used up" Hob.Nob and Nob.Hobs,
            // so now the RelationshipDiscoveryConvention should be able
            // to unambiguously and automatically match up Nob.Hob and Hob.Nobs
            // in a different 1:N relationship.
            var otherFk = principalType.GetForeignKeys().Single();
            Assert.False(fk.IsUnique);
            Assert.Equal(nameof(Hob.Nobs), otherFk.PrincipalToDependent.Name);
            Assert.Equal(nameof(Nob.Hob), otherFk.DependentToPrincipal.Name);
            Assert.Same(principalKey, principalType.FindPrimaryKey());
            Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
            Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
        }

        [ConditionalFact]
        public virtual void Can_add_annotations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.FindEntityType(typeof(Order));

            var builder = modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);
            builder = builder.HasAnnotation("Fus", "Ro");

            var fk = dependentType.GetForeignKeys().Single();
            Assert.Same(fk, builder.Metadata);
            Assert.Equal("Ro", fk["Fus"]);
        }

        [ConditionalFact]
        public virtual void Annotations_are_preserved_when_rebuilding()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var builder = modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);
            builder = builder.HasAnnotation("Fus", "Ro");
            builder = builder.HasForeignKey("ShadowFK");

            Assert.Equal("Ro", builder.Metadata["Fus"]);
        }

        [ConditionalFact]
        public virtual void Nullable_FK_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .HasForeignKey(
                    e => new { e.HobId1, e.HobId2 });

            modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            var fkProperty1 = entityType.FindProperty(nameof(Nob.HobId1));
            var fkProperty2 = entityType.FindProperty(nameof(Nob.HobId2));
            Assert.True(fkProperty1.IsNullable);
            Assert.True(fkProperty2.IsNullable);
            Assert.Contains(fkProperty1, fk.Properties);
            Assert.Contains(fkProperty2, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Non_nullable_FK_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                .HasForeignKey(
                    e => new { e.NobId1, e.NobId2 });

            modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var fk = entityType.GetForeignKeys().Single();
            Assert.True(fk.IsRequired);
            var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
            var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
            Assert.False(fkProperty1.IsNullable);
            Assert.False(fkProperty2.IsNullable);
            Assert.Contains(fkProperty1, fk.Properties);
            Assert.Contains(fkProperty2, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Nullable_FK_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .HasForeignKey(
                    e => new { e.HobId1, e.HobId2 })
                .IsRequired();

            modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
            var fk = entityType.GetForeignKeys().Single();
            Assert.True(fk.IsRequired);
            var fkProperty1 = entityType.FindProperty(nameof(Nob.HobId1));
            var fkProperty2 = entityType.FindProperty(nameof(Nob.HobId2));
            Assert.False(fkProperty1.IsNullable);
            Assert.False(fkProperty2.IsNullable);
            Assert.Contains(fkProperty1, fk.Properties);
            Assert.Contains(fkProperty2, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Non_nullable_FK_can_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                .HasForeignKey(e => new { e.NobId1, e.NobId2 })
                .IsRequired(false);

            modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
            var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
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
                .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                .HasForeignKey(e => new { e.NobId1, e.NobId2 });

            modelBuilder
                .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                .IsRequired(false);

            modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
            var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
            Assert.False(fkProperty1.IsNullable);
            Assert.False(fkProperty2.IsNullable);
            Assert.Contains(fkProperty1, fk.Properties);
            Assert.Contains(fkProperty2, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Nullable_FK_overrides_NRT_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<DependentEntity>(eb =>
            {
                eb.Property(d => d.PrincipalEntityId);
                eb.Ignore(d => d.Nav);
                eb.HasOne(d => d.Nav).WithMany(p => p.InverseNav);
            });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(DependentEntity));
            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            var fkProperty = entityType.FindProperty(nameof(DependentEntity.PrincipalEntityId));
            Assert.True(fkProperty.IsNullable);
            Assert.Contains(fkProperty, fk.Properties);
        }

        [ConditionalFact]
        public virtual void Can_change_delete_behavior()
        {
            var modelBuilder = HobNobBuilder();
            var dependentType = (IReadOnlyEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .OnDelete(DeleteBehavior.Cascade);

            Assert.Equal(DeleteBehavior.Cascade, dependentType.GetForeignKeys().Single().DeleteBehavior);

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .OnDelete(DeleteBehavior.Restrict);

            Assert.Equal(DeleteBehavior.Restrict, dependentType.GetForeignKeys().Single().DeleteBehavior);

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .OnDelete(DeleteBehavior.SetNull);

            Assert.Equal(DeleteBehavior.SetNull, dependentType.GetForeignKeys().Single().DeleteBehavior);
        }

        [ConditionalFact]
        public virtual void Can_set_foreign_key_property_when_matching_property_added()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<PrincipalEntity>();

            var foreignKey = model.FindEntityType(typeof(DependentEntity)).GetForeignKeys().Single();
            Assert.Equal("NavId", foreignKey.Properties.Single().Name);

            modelBuilder.Entity<DependentEntity>().Property(et => et.PrincipalEntityId);

            var newForeignKey = model.FindEntityType(typeof(DependentEntity)).GetForeignKeys().Single();
            Assert.Equal("PrincipalEntityId", newForeignKey.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Creates_shadow_property_for_foreign_key_according_to_navigation_to_principal_name_when_present()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Beta>();
            modelBuilder.Entity<Alpha>();
            modelBuilder.Ignore<Theta>();

            var model = modelBuilder.FinalizeModel();

            var entityB = model.FindEntityType(typeof(Beta));
            Assert.Equal("FirstNavId", entityB.FindNavigation("FirstNav").ForeignKey.Properties.First().Name);
            Assert.Equal("SecondNavId", entityB.FindNavigation("SecondNav").ForeignKey.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void
            Creates_shadow_property_for_foreign_key_according_to_target_type_when_navigation_to_principal_name_not_present()
        {
            var modelBuilder = CreateModelBuilder();
            var gamma = modelBuilder.Entity<Gamma>().Metadata;

            Assert.Equal("GammaId", gamma.FindNavigation("Alphas").ForeignKey.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_property_with_non_shadow_PK()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Alpha>();
            modelBuilder.Entity<Beta>(
                b =>
                {
                    b.HasOne(e => e.FirstNav)
                        .WithMany()
                        .HasForeignKey("ShadowId");
                });

            modelBuilder.FinalizeModel();

            Assert.Equal(
                "ShadowId",
                modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Creates_shadow_FK_property_with_shadow_PK()
        {
            var modelBuilder = CreateModelBuilder();

            var entityA = modelBuilder.Entity<Alpha>();
            entityA.Property<int>("ShadowPK");
            entityA.HasKey("ShadowPK");

            var entityB = modelBuilder.Entity<Beta>();

            entityB.HasOne(e => e.FirstNav).WithMany().HasForeignKey("ShadowId");

            modelBuilder.FinalizeModel();

            Assert.Equal(
                "ShadowId",
                modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Handles_identity_correctly_while_removing_navigation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Delta>();
            modelBuilder.Entity<Epsilon>().HasOne<Alpha>().WithMany(b => b.Epsilons);

            var property = modelBuilder.Model.FindEntityType(typeof(Epsilon)).FindProperty("Id");
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);

            modelBuilder.FinalizeModel();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Throws_when_foreign_key_references_shadow_key()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders).HasForeignKey(e => e.AnotherCustomerId);

            Assert.Equal(
                CoreStrings.ReferencedShadowKey(
                    typeof(Order).Name + "." + nameof(Order.Customer),
                    typeof(Customer).Name + "." + nameof(Customer.Orders),
                    "{'AnotherCustomerId' : Guid}",
                    "{'Id' : int}"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Can_exclude_navigation_pointed_by_foreign_key_attribute_from_explicit_configuration()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Delta>();
            modelBuilder.Entity<Epsilon>().HasOne<Alpha>().WithMany(b => b.Epsilons);

            modelBuilder.FinalizeModel();

            var model = modelBuilder.Model;

            var alphaFk = model.FindEntityType(typeof(Epsilon)).FindNavigation(nameof(Epsilon.Alpha)).ForeignKey;
            Assert.Null(alphaFk.PrincipalToDependent);
            Assert.False(alphaFk.IsUnique);
            Assert.Equal(nameof(Epsilon.Id), alphaFk.Properties.First().Name);

            var epsilonFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Epsilons)).ForeignKey;
            Assert.Null(epsilonFk.DependentToPrincipal);
            Assert.False(epsilonFk.IsUnique);
            Assert.Equal(nameof(Alpha) + nameof(Alpha.Id), epsilonFk.Properties.First().Name);

            var etaFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Etas)).ForeignKey;
            Assert.Equal(nameof(Eta.Alpha), etaFk.DependentToPrincipal.Name);
            Assert.False(etaFk.IsUnique);
            Assert.Equal("Id", etaFk.Properties.First().Name);

            var kappaFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Kappas)).ForeignKey;
            Assert.Equal(nameof(Kappa.Alpha), kappaFk.DependentToPrincipal.Name);
            Assert.False(kappaFk.IsUnique);
            Assert.Equal("Id", kappaFk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_exclude_navigation_with_foreign_key_attribute_from_explicit_configuration()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Delta>();
            modelBuilder.Entity<Eta>().HasOne<Alpha>().WithMany(b => b.Etas);

            modelBuilder.FinalizeModel();

            var model = modelBuilder.Model;

            var alphaFk = model.FindEntityType(typeof(Eta)).FindNavigation(nameof(Eta.Alpha)).ForeignKey;
            Assert.Null(alphaFk.PrincipalToDependent);
            Assert.False(alphaFk.IsUnique);
            Assert.Equal(nameof(Eta.Id), alphaFk.Properties.Single().Name);

            var etasFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Etas)).ForeignKey;
            Assert.Null(etasFk.DependentToPrincipal);
            Assert.False(etasFk.IsUnique);
            Assert.NotSame(alphaFk, etasFk);
            Assert.Equal(nameof(Alpha) + nameof(Alpha.Id), etasFk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_exclude_navigation_with_foreign_key_attribute_on_principal_type_from_explicit_configuration()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Delta>();
            modelBuilder.Ignore<Iota>();
            modelBuilder.Entity<Theta>().HasOne(e => e.Alpha).WithMany();

            modelBuilder.FinalizeModel();

            var model = modelBuilder.Model;

            var thetasFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Thetas)).ForeignKey;
            Assert.Null(thetasFk.DependentToPrincipal);
            Assert.False(thetasFk.IsUnique);
            Assert.Equal("Id", thetasFk.Properties.Single().Name);
            Assert.True(thetasFk.Properties.Single().IsShadowProperty());

            var alphaFk = model.FindEntityType(typeof(Theta)).FindNavigation(nameof(Theta.Alpha)).ForeignKey;
            Assert.Null(alphaFk.PrincipalToDependent);
            Assert.False(alphaFk.IsUnique);
            Assert.NotSame(alphaFk, thetasFk);
            Assert.Equal(nameof(Alpha) + nameof(Alpha.Id), alphaFk.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void
            Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_no_matching_properties_either_side()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                    b.Ignore(e => e.OneToOneDependentEntityId);
                });
            modelBuilder.Entity<OneToOneDependentEntity>(
                b =>
                {
                    b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                    b.Ignore(e => e.OneToOnePrincipalEntityId);
                });

            modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity))
                .FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.False(fk.IsUnique);
            Assert.Null(fk.PrincipalToDependent);
            Assert.True(fk.Properties.Single().IsShadowProperty());
        }

        [ConditionalFact]
        public virtual void
            Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_matching_navigation_name_properties_are_on_navigation_side()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                    b.Ignore(e => e.OneToOneDependentEntityId);
                });
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.OneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity))
                .FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.False(fk.IsUnique);
            Assert.Null(fk.PrincipalToDependent);
            Assert.False(fk.Properties.Single().IsShadowProperty());
            Assert.Equal(OneToOneDependentEntity.NavigationMatchingProperty.Name, fk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void
            Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_matching_entity_name_properties_are_on_navigation_side()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(
                b =>
                {
                    b.Ignore(e => e.NavOneToOneDependentEntityId);
                    b.Ignore(e => e.OneToOneDependentEntityId);
                });
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.NavOneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity))
                .FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.False(fk.IsUnique);
            Assert.Null(fk.PrincipalToDependent);
            Assert.False(fk.Properties.Single().IsShadowProperty());
            Assert.Equal(OneToOneDependentEntity.EntityMatchingProperty.Name, fk.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void
            Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_matching_properties_are_on_both_sides()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<OneToOnePrincipalEntity>(b => b.Ignore(e => e.NavOneToOneDependentEntityId));
            modelBuilder.Entity<OneToOneDependentEntity>(b => b.Ignore(e => e.NavOneToOnePrincipalEntityId));

            modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

            modelBuilder.FinalizeModel();

            var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity))
                .FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

            Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
            Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
            Assert.False(fk.IsUnique);
            Assert.Null(fk.PrincipalToDependent);
            Assert.True(fk.Properties.Single().IsShadowProperty());
        }

        [ConditionalFact]
        public virtual void Ambiguous_relationship_candidate_does_not_block_creating_further_relationships()
        {
            var modelBuilder = CreateModelBuilder();
            var theta = modelBuilder.Entity<Theta>().Metadata;

            Assert.NotNull(theta.FindNavigation("NavTheta"));
            Assert.NotNull(theta.FindNavigation("InverseNavThetas"));
            Assert.Same(theta.FindNavigation("NavTheta").ForeignKey, theta.FindNavigation("InverseNavThetas").ForeignKey);
        }

        [ConditionalFact]
        public virtual void Shadow_property_created_for_foreign_key_is_nullable()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>().HasMany(c => c.Orders).WithOne(o => o.Customer).HasForeignKey("MyShadowFk");

            Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).FindProperty("MyShadowFk").IsNullable);
            Assert.Equal(typeof(int?), modelBuilder.Model.FindEntityType(typeof(Order)).FindProperty("MyShadowFk").ClrType);
        }

        [ConditionalFact]
        public virtual void One_to_many_relationship_has_no_ambiguity_convention()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Alpha>();
            modelBuilder.Entity<Kappa>();

            Assert.Equal(
                "KappaId",
                modelBuilder.Model.FindEntityType(typeof(Kappa)).FindNavigation(nameof(Kappa.Omegas)).ForeignKey.Properties.Single()
                    .Name);
        }

        [ConditionalFact]
        public virtual void One_to_many_relationship_has_no_ambiguity_explicit()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Kappa>().Ignore(e => e.Omegas);
            modelBuilder.Entity<Omega>().HasOne(e => e.Kappa).WithMany();
            modelBuilder.Entity<Alpha>();

            modelBuilder.FinalizeModel();

            Assert.Equal(
                "KappaId",
                modelBuilder.Model.FindEntityType(typeof(Omega)).FindNavigation(nameof(Omega.Kappa)).ForeignKey.Properties.Single()
                    .Name);
        }

        [ConditionalFact]
        public virtual void RemoveKey_does_not_add_back_foreign_key_pointing_to_the_same_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<Alpha>();

            Assert.Equal(nameof(Alpha.Id), entityTypeBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);

            entityTypeBuilder.Property(e => e.Id).IsRequired(false);

            Assert.Null(entityTypeBuilder.Metadata.FindPrimaryKey());

            entityTypeBuilder.HasKey(e => e.AnotherId);

            Assert.Equal(nameof(Alpha.AnotherId), entityTypeBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Creates_shadow_fk_configuring_using_ForeignKeyAttribute()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<PrincipalShadowFk>().HasMany(e => e.Dependents).WithOne(e => e.Principal);

            modelBuilder.FinalizeModel();

            Assert.Equal(
                "PrincipalShadowFkId",
                modelBuilder.Model.FindEntityType(typeof(DependentShadowFk)).GetForeignKeys().Single().Properties[0].Name);
        }

        [ConditionalFact]
        public virtual void Do_not_match_non_unique_FK_when_overlap_with_PK()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<CompositeChild>().HasKey(e => new { e.Id, e.Value });

            var model = modelBuilder.FinalizeModel();

            var child = model.FindEntityType(typeof(CompositeChild));
            var fk = child.GetForeignKeys().Single();
            Assert.Equal("ParentId", fk.Properties[0].Name);
        }

        [ConditionalFact]
        public virtual void Navigation_properties_can_set_access_mode()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<NavDependent>()
                .HasOne(e => e.OneToManyPrincipal)
                .WithMany(e => e.Dependents);

            modelBuilder.Entity<NavDependent>()
                .Navigation(e => e.OneToManyPrincipal)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<OneToManyNavPrincipal>()
                .Navigation(e => e.Dependents)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            var principal = (IReadOnlyEntityType)model.FindEntityType(typeof(OneToManyNavPrincipal));
            var dependent = (IReadOnlyEntityType)model.FindEntityType(typeof(NavDependent));

            Assert.Equal(PropertyAccessMode.Field, principal.FindNavigation("Dependents").GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, dependent.FindNavigation("OneToManyPrincipal").GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Attempt_to_configure_Navigation_property_which_is_actually_a_Property_throws()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<NavDependent>()
                .HasOne(e => e.OneToManyPrincipal)
                .WithMany(e => e.Dependents);

            Assert.Equal(
                CoreStrings.CanOnlyConfigureExistingNavigations("Name", "NavDependent"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<NavDependent>()
                        .Navigation(e => e.Name)
                        .UsePropertyAccessMode(PropertyAccessMode.Property)
                ).Message);
        }

        [ConditionalFact]
        public virtual void Navigation_to_shared_type_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionNavigationToSharedType>();

            Assert.Equal(
                CoreStrings.NonConfiguredNavigationToSharedType("Navigation", nameof(CollectionNavigationToSharedType)),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        public virtual void WithMany_call_on_keyless_entity_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.PrincipalKeylessType(
                    nameof(KeylessCollectionNavigation),
                    nameof(KeylessCollectionNavigation) + "." + nameof(KeylessCollectionNavigation.Stores),
                    nameof(Store)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<KeylessCollectionNavigation>().HasNoKey().HasMany(e => e.Stores)).Message);
        }

        [ConditionalFact]
        public virtual void WithMany_pointing_to_keyless_entity_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.NavigationToKeylessType(
                    nameof(KeylessReferenceNavigation.Collection),
                    nameof(KeylessCollectionNavigation)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<KeylessCollectionNavigation>().HasNoKey()
                        .HasOne(e => e.Reference).WithMany(e => e.Collection)).Message);
        }

        [ConditionalFact]
        public virtual void HasNoKey_call_on_principal_entity_throws()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<KeylessCollectionNavigation>().HasMany(e => e.Stores).WithOne();
            Assert.Equal(
                CoreStrings.PrincipalKeylessType(
                    nameof(KeylessCollectionNavigation),
                    nameof(KeylessCollectionNavigation) + "." + nameof(KeylessCollectionNavigation.Stores),
                    nameof(Store)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<KeylessCollectionNavigation>().HasNoKey()).Message);
        }

        [ConditionalFact]
        public virtual void HasNoKey_call_on_principal_with_navigation_throws()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<KeylessReferenceNavigation>();
            modelBuilder.Entity<KeylessCollectionNavigation>()
                .HasOne(e => e.Reference)
                .WithMany(e => e.Collection);

            Assert.Equal(
                CoreStrings.NavigationToKeylessType(
                    nameof(KeylessReferenceNavigation.Collection),
                    nameof(KeylessCollectionNavigation)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<KeylessCollectionNavigation>().HasNoKey()).Message);
        }

        [ConditionalFact]
        public virtual void Reference_navigation_from_keyless_entity_type_works()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Discount>(
                entity =>
                {
                    entity.HasNoKey();

                    entity.HasOne(d => d.Store).WithMany();
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Collection(
                model.GetEntityTypes(),
                e =>
                {
                    Assert.Equal(typeof(Discount).DisplayName(), e.Name);
                    var fk = Assert.Single(e.GetForeignKeys());
                    Assert.False(fk.IsUnique);
                    Assert.Equal(nameof(Discount.Store), fk.DependentToPrincipal.Name);
                },
                e => Assert.Equal(typeof(Store).DisplayName(), e.Name));
        }
    }
}
