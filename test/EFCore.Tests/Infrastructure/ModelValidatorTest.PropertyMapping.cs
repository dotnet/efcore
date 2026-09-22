// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class ModelValidatorTest
{
    [ConditionalFact]
    public virtual void Throws_when_added_property_is_not_of_primitive_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity));
        entityTypeBuilder.Property(
            typeof(NavigationAsProperty), nameof(NonPrimitiveAsPropertyEntity.Property));

        Assert.Equal(
            CoreStrings.PropertyNotMapped(
                typeof(NavigationAsProperty).ShortDisplayName(),
                typeof(NonPrimitiveAsPropertyEntity).ShortDisplayName(),
                nameof(NonPrimitiveAsPropertyEntity.Property)),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_added_shadow_property_by_convention_is_not_of_primitive_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity)).HasNoKey();
        entityTypeBuilder.GetInfrastructure().Property(typeof(NavigationAsProperty), "ShadowProperty");
        entityTypeBuilder.Ignore(nameof(NonPrimitiveAsPropertyEntity.Property));

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_when_primitive_type_property_is_not_added_or_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(PrimitivePropertyEntity));

        Assert.Equal(
            CoreStrings.PropertyNotAdded(
                typeof(PrimitivePropertyEntity).ShortDisplayName(), "Property", typeof(int).DisplayName()),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Throws_when_nonprimitive_value_type_property_is_not_added_or_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(NonPrimitiveValueTypePropertyEntity)).HasNoKey();

        Assert.Equal(
            CoreStrings.PropertyNotAdded(
                typeof(NonPrimitiveValueTypePropertyEntity).ShortDisplayName(), "Property", typeof(CancellationToken).Name),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_nonprimitive_value_type_property_type_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder(
            configurationBuilder => configurationBuilder.IgnoreAny<CancellationToken>());
        modelBuilder.Entity(typeof(NonPrimitiveValueTypePropertyEntity)).HasNoKey();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_when_keyless_type_property_is_not_added_or_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(NonPrimitiveReferenceTypePropertyEntity));

        Assert.Equal(
            CoreStrings.NavigationNotAdded(
                typeof(NonPrimitiveReferenceTypePropertyEntity).ShortDisplayName(),
                nameof(NonPrimitiveReferenceTypePropertyEntity.Property),
                typeof(ICollection<Uri>).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_primitive_type_property_is_added()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity)).HasNoKey();
        entityTypeBuilder.Property(typeof(int), "Property");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_primitive_type_property_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity)).HasNoKey();
        entityTypeBuilder.Ignore("Property");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_when_navigation_is_not_added_or_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(NavigationEntity));
        modelBuilder.Entity(typeof(PrimitivePropertyEntity));

        Assert.Equal(
            CoreStrings.NavigationNotAdded(
                typeof(NavigationEntity).ShortDisplayName(), "Navigation", typeof(PrimitivePropertyEntity).Name),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Throws_when_navigation_to_owned_type_is_not_added_or_ignored()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property(a => a.Id);
        modelBuilder.Entity<Cat>().OwnsOne(a => a.FavoritePerson);
        modelBuilder.Entity<Dog>();

        Assert.Equal(
            CoreStrings.NavigationNotAdded(
                typeof(Dog).ShortDisplayName(), nameof(Dog.FavoritePerson), typeof(Person).Name),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_navigation_is_added()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity));
        entityTypeBuilder.Property<int>("Id");
        entityTypeBuilder.HasKey("Id");
        var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity));
        referencedEntityTypeBuilder.Ignore("Property");
        referencedEntityTypeBuilder.Property<int>("Id");
        referencedEntityTypeBuilder.HasKey("Id");
        entityTypeBuilder.GetInfrastructure().HasRelationship(
            (IConventionEntityType)referencedEntityTypeBuilder.Metadata, "Navigation", null, setTargetAsPrincipal: true);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_navigation_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity)).HasNoKey();
        entityTypeBuilder.Ignore("Navigation");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_navigation_type_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder(
            configurationBuilder => configurationBuilder.IgnoreAny<PrimitivePropertyEntity>());
        modelBuilder.Entity(typeof(NavigationEntity)).HasNoKey();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_navigation_target_entity_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(NavigationEntity)).HasNoKey();
        modelBuilder.Ignore(typeof(PrimitivePropertyEntity));

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_explicit_navigation_is_not_added()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(ExplicitNavigationEntity));
        var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity));
        referencedEntityTypeBuilder.Ignore("Property");
        entityTypeBuilder.Property<int>("Id");
        entityTypeBuilder.HasKey("Id");
        referencedEntityTypeBuilder.Property<int>("Id");
        referencedEntityTypeBuilder.HasKey("Id");
        entityTypeBuilder.GetInfrastructure().HasRelationship(
            (IConventionEntityType)referencedEntityTypeBuilder.Metadata, "Navigation", null);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_when_interface_type_property_is_not_added_or_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(InterfaceNavigationEntity)).HasNoKey();

        Assert.Equal(
            CoreStrings.InterfacePropertyNotAdded(
                typeof(InterfaceNavigationEntity).ShortDisplayName(),
                "Navigation",
                typeof(IList<INavigationEntity>).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_interface_collection_type_property_type_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder(
            configurationBuilder => configurationBuilder.IgnoreAny<INavigationEntity>());
        modelBuilder.Entity(typeof(InterfaceNavigationEntity)).HasNoKey();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_interface_generic_type_property_type_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder(
            configurationBuilder => configurationBuilder.IgnoreAny(typeof(IList<>)));
        modelBuilder.Entity(typeof(InterfaceNavigationEntity)).HasNoKey();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_interface_base_type_property_type_is_ignored()
    {
        var modelBuilder = CreateConventionlessModelBuilder(
            configurationBuilder => configurationBuilder.IgnoreAny<IEnumerable<INavigationEntity>>());
        modelBuilder.Entity(typeof(InterfaceNavigationEntity)).HasNoKey();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_throw_when_non_candidate_property_is_not_added()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity(typeof(NonCandidatePropertyEntity)).HasNoKey();

        Validate(modelBuilder);
    }

    protected virtual IModel Validate(TestHelpers.TestModelBuilder modelBuilder)
        => modelBuilder.FinalizeModel(designTime: true);

    protected class NonPrimitiveNonNavigationAsPropertyEntity;

    protected class NonPrimitiveAsPropertyEntity
    {
        public NavigationAsProperty Property { get; set; }
    }

    protected class NavigationAsProperty;

    protected class PrimitivePropertyEntity
    {
        public int Property { get; set; }
    }

    protected class NonPrimitiveValueTypePropertyEntity
    {
        public CancellationToken Property { get; set; }
    }

    protected class NonPrimitiveReferenceTypePropertyEntity
    {
        public ICollection<Uri> Property { get; set; }
    }

    protected class NavigationEntity
    {
        public PrimitivePropertyEntity Navigation { get; set; }
    }

    protected class NonCandidatePropertyEntity
    {
        public static int StaticProperty { get; set; }

        public int _writeOnlyField = 1;

        public int WriteOnlyProperty
        {
            set => _writeOnlyField = value;
        }
    }

    protected interface INavigationEntity
    {
        PrimitivePropertyEntity Navigation { get; set; }
    }

    protected class ExplicitNavigationEntity : INavigationEntity
    {
        PrimitivePropertyEntity INavigationEntity.Navigation { get; set; }

        public PrimitivePropertyEntity Navigation { get; set; }
    }

    protected class InterfaceNavigationEntity
    {
        public IList<INavigationEntity> Navigation { get; set; }
    }

    protected abstract class LivingBeing
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public OwnedEntity Details { get; set; }
    }

    protected class Animal : LivingBeing
    {
        public Person FavoritePerson { get; set; }
    }

    protected class Cat : Animal
    {
        public string Breed { get; set; }

        [NotMapped]
        public string Type { get; set; }

        public int Identity { get; set; }
    }

    protected class Dog : Animal
    {
        public string Breed { get; set; }

        [NotMapped]
        public int Type { get; set; }

        public int Identity { get; set; }
    }

    protected class Person : LivingBeing
    {
        public string FavoriteBreed { get; set; }
    }

    protected class Employee : Person;

    protected class Owner
    {
        public int Id { get; set; }
        public OwnedEntity Owned { get; set; }
    }

    protected class OwnedEntity
    {
        public string Value { get; set; }
    }

    protected class CarbonComposite
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
    }
}
