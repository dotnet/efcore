// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderNonGenericTest : InMemoryModelBuilderTest
{
    public class InMemoryNonGenericNonRelationship(InMemoryModelBuilderFixture fixture) : InMemoryNonRelationship(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericComplexType(InMemoryModelBuilderFixture fixture) : InMemoryComplexType(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericInheritance(InMemoryModelBuilderFixture fixture) : InMemoryInheritance(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericOneToMany(InMemoryModelBuilderFixture fixture) : InMemoryOneToMany(fixture)
    {
        [ConditionalFact]
        public virtual void HasOne_with_just_string_navigation_for_non_CLR_property_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.NoClrNavigation("Snoop", nameof(Dr)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        ((NonGenericTestEntityTypeBuilder<Dr>)modelBuilder.Entity<Dr>()).GetInfrastructure()
                        .HasOne("Snoop")).Message);
        }

        [ConditionalFact]
        public virtual void HasMany_with_just_string_navigation_for_non_CLR_property_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.NoClrNavigation("Snoop", nameof(Dr)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        ((NonGenericTestEntityTypeBuilder<Dr>)modelBuilder.Entity<Dr>()).GetInfrastructure()
                        .HasMany("Snoop")).Message);
        }

        [ConditionalFact]
        public virtual void HasMany_with_a_non_collection_just_string_navigation_CLR_property_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.NavigationCollectionWrongClrType("Dre", nameof(Dr), nameof(Dre), "T"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        ((NonGenericTestEntityTypeBuilder<Dr>)modelBuilder.Entity<Dr>()).GetInfrastructure()
                        .HasMany("Dre")).Message);
        }

        [ConditionalFact] //Issue#13108
        public virtual void HasForeignKey_infers_type_for_shadow_property_when_not_specified()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ComplexCaseChild13108>(
                e =>
                {
                    e.HasKey(c => c.Key);
                    ((NonGenericTestEntityTypeBuilder<ComplexCaseChild13108>)e).GetInfrastructure().Property("ParentKey");
                    e.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey("ParentKey");
                });

            modelBuilder.Entity<ComplexCaseParent13108>().HasKey(c => c.Key);

            var model = (IConventionModel)modelBuilder.FinalizeModel();

            var property = model
                .FindEntityType(typeof(ComplexCaseChild13108))!.GetProperties().Single(p => p.Name == "ParentKey");
            Assert.Equal(typeof(int), property.ClrType);
            Assert.Equal(ConfigurationSource.Explicit, property.GetTypeConfigurationSource());
        }

        protected class ComplexCaseChild13108
        {
            public int Key { get; set; }
            public string? Id { get; set; }
            private int ParentKey { get; set; }
            public ComplexCaseParent13108? Parent { get; set; }
        }

        protected class ComplexCaseParent13108
        {
            public int Key { get; set; }
            public string? Id { get; set; }
            public ICollection<ComplexCaseChild13108>? Children { get; set; }
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericManyToMany(InMemoryModelBuilderFixture fixture) : InMemoryManyToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericManyToOne(InMemoryModelBuilderFixture fixture) : InMemoryManyToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericOneToOne(InMemoryModelBuilderFixture fixture) : InMemoryOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }

    public class InMemoryNonGenericOwnedTypes(InMemoryModelBuilderFixture fixture) : InMemoryOwnedTypes(fixture)
    {
        [ConditionalFact]
        public virtual void OwnsOne_HasOne_with_just_string_navigation_for_non_CLR_property_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.NoClrNavigation("Snoop", nameof(Dre)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        ((NonGenericTestOwnedNavigationBuilder<Dr, Dre>)modelBuilder.Entity<Dr>().OwnsOne(e => e.Dre))
                        .GetInfrastructure()
                        .HasOne("Snoop")).Message);
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new NonGenericTestModelBuilder(Fixture, configure);
    }
}
