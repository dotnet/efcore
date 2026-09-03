// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ForeignKeyTest
{
    [ConditionalFact]
    public void Throws_when_model_is_readonly()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType("E");
        var dependentProp = entityType.AddProperty("P", typeof(int));
        var principalProp = entityType.AddProperty("Id", typeof(int));
        var key = entityType.AddKey(principalProp);
        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, key, entityType);

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { principalProp }, key, entityType)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveForeignKey(foreignKey)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsRequired = false).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsRequiredDependent = false).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsOwnership = false).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsUnique = false).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.SetDependentToPrincipal((string)null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.SetPrincipalToDependent((string)null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => foreignKey.SetProperties(new[] { principalProp }, key)).Message);
    }

    [ConditionalFact]
    public void Can_create_foreign_key()
    {
        var entityType = (IConventionEntityType)CreateModel().AddEntityType("E");
        var dependentProp = entityType.AddProperty("P", typeof(int));
        var principalProp = entityType.AddProperty("Id", typeof(int));
        entityType.SetPrimaryKey(principalProp);

        var foreignKey = entityType.AddForeignKey(
            new[] { dependentProp }, entityType.FindPrimaryKey(), entityType);
        foreignKey.SetIsUnique(true);

        Assert.Same(entityType, foreignKey.PrincipalEntityType);
        Assert.Same(principalProp, foreignKey.PrincipalKey.Properties.Single());
        Assert.Same(dependentProp, foreignKey.Properties.Single());
        Assert.True(foreignKey.IsUnique);
        Assert.Same(entityType.FindPrimaryKey(), foreignKey.PrincipalKey);
        Assert.Equal(ConfigurationSource.Convention, foreignKey.GetConfigurationSource());

        ((ForeignKey)foreignKey).UpdateConfigurationSource(ConfigurationSource.DataAnnotation);

        Assert.Equal(ConfigurationSource.DataAnnotation, foreignKey.GetConfigurationSource());
    }

    [ConditionalFact]
    public void Constructor_throws_when_referenced_key_not_on_referenced_entity()
    {
        var model = CreateModel();

        var principalEntityType = model.AddEntityType("R");
        var dependentEntityType = model.AddEntityType("D");
        var fk = dependentEntityType.AddProperty("Fk", typeof(int));

        var principalKey = dependentEntityType.SetPrimaryKey(fk);

        Assert.Equal(
            CoreStrings.ForeignKeyReferencedEntityKeyMismatch("{'Fk'}", "R (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(
                () => dependentEntityType.AddForeignKey(new[] { fk }, principalKey, principalEntityType)).Message);
    }

    [ConditionalFact]
    public void Constructor_throws_when_principal_and_dependent_property_count_do_not_match()
    {
        var model = CreateModel();
        var dependentEntityType = model.AddEntityType("D");
        var principalEntityType = model.AddEntityType("P");

        var dependentProperty1 = dependentEntityType.AddProperty("P1", typeof(int));
        var dependentProperty2 = dependentEntityType.AddProperty("P2", typeof(int));

        var idProperty = principalEntityType.AddProperty("Id", typeof(int));
        principalEntityType.SetPrimaryKey(idProperty);

        Assert.Equal(
            CoreStrings.ForeignKeyCountMismatch(
                "{'P1', 'P2'}", "D (Dictionary<string, object>)", "{'Id'}", "P (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(
                    () => dependentEntityType.AddForeignKey(
                        new[] { dependentProperty1, dependentProperty2 }, principalEntityType.FindPrimaryKey(), principalEntityType))
                .Message);
    }

    [ConditionalFact]
    public void Constructor_throws_when_principal_and_dependent_property_types_do_not_match()
    {
        var dependentEntityType = CreateModel().AddEntityType("D");
        var principalEntityType = CreateModel().AddEntityType("P");

        var dependentProperty1 = dependentEntityType.AddProperty("P1", typeof(int));
        var dependentProperty2 = dependentEntityType.AddProperty("P2", typeof(string));

        var property2 = principalEntityType.AddProperty("Id1", typeof(int));
        var property3 = principalEntityType.AddProperty("Id2", typeof(int));
        principalEntityType.SetPrimaryKey(
            new[] { property2, property3 });

        Assert.Equal(
            CoreStrings.ForeignKeyTypeMismatch(
                "{'P1' : int, 'P2' : string}", "D (Dictionary<string, object>)", "{'Id1' : int, 'Id2' : int}",
                "P (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(
                    () => dependentEntityType.AddForeignKey(
                        new[] { dependentProperty1, dependentProperty2 }, principalEntityType.FindPrimaryKey(), principalEntityType))
                .Message);
    }

    [ConditionalFact]
    public void Can_create_foreign_key_with_non_pk_principal()
    {
        var entityType = CreateModel().AddEntityType("E");
        var keyProp = entityType.AddProperty("Id", typeof(int));
        var dependentProp = entityType.AddProperty("P", typeof(int));
        var principalProp = entityType.AddProperty("U", typeof(int));
        entityType.SetPrimaryKey(keyProp);
        var principalKey = entityType.AddKey(principalProp);

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, principalKey, entityType);
        foreignKey.IsUnique = false;

        Assert.Same(entityType, foreignKey.PrincipalEntityType);
        Assert.Same(principalProp, foreignKey.PrincipalKey.Properties.Single());
        Assert.Same(dependentProp, foreignKey.Properties.Single());
        Assert.False(foreignKey.IsUnique);
        Assert.Same(principalKey, foreignKey.PrincipalKey);
    }

    [ConditionalFact]
    public void IsRequired_and_IsUnique_false_when_dependent_property_not_nullable()
    {
        var entityType = CreateModel().AddEntityType("E");
        var property = entityType.AddProperty("Id", typeof(int));
        entityType.SetPrimaryKey(property);
        var dependentProp = entityType.AddProperty("P", typeof(int));

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, entityType.FindPrimaryKey(), entityType);

        Assert.False(dependentProp.IsNullable);
        Assert.True(foreignKey.IsRequired);
        Assert.False(foreignKey.IsUnique);
    }

    [ConditionalFact]
    public void IsRequired_and_IsUnique_false_when_dependent_property_nullable()
    {
        var entityType = CreateModel().AddEntityType("E");
        var property = entityType.AddProperty("Id", typeof(int));
        entityType.SetPrimaryKey(property);
        var dependentProp = entityType.AddProperty("P", typeof(int?));

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, entityType.FindPrimaryKey(), entityType);

        Assert.True(dependentProp.IsNullable);
        Assert.False(foreignKey.IsRequired);
        Assert.False(foreignKey.IsUnique);
    }

    [ConditionalFact]
    public void IsRequired_false_when_no_part_of_composite_FK_is_nullable()
    {
        var entityType = CreateModel().AddEntityType("E");
        var property = entityType.AddProperty("Id1", typeof(int));
        var property1 = entityType.AddProperty("Id2", typeof(string));
        property1.IsNullable = false;
        entityType.SetPrimaryKey(
            new[] { property, property1 });

        var dependentProp1 = entityType.AddProperty("P1", typeof(int));
        var dependentProp2 = entityType.AddProperty("P2", typeof(string));

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.FindPrimaryKey(), entityType);

        Assert.False(foreignKey.IsRequired);

        dependentProp2.IsNullable = false;

        Assert.False(foreignKey.IsRequired);
    }

    [ConditionalFact]
    public void Setting_IsRequired_to_true_does_not_configure_FK_properties_as_non_nullable()
    {
        var entityType = CreateModel().AddEntityType("E");
        var property = entityType.AddProperty("Id1", typeof(int));
        var property3 = entityType.AddProperty("Id2", typeof(string));
        property3.IsNullable = false;
        entityType.SetPrimaryKey(
            new[] { property, property3 });

        var dependentProp1 = entityType.AddProperty("P1", typeof(int));
        var dependentProp2 = entityType.AddProperty("P2", typeof(string));

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.FindPrimaryKey(), entityType);
        foreignKey.IsRequired = true;

        Assert.True(foreignKey.IsRequired);
        Assert.False(dependentProp1.IsNullable);
        Assert.True(dependentProp2.IsNullable);
    }

    [ConditionalFact]
    public void Setting_IsRequired_to_false_will_not_configure_FK_properties_as_nullable()
    {
        var entityType = CreateModel().AddEntityType("E");
        var property = entityType.AddProperty("Id1", typeof(int));
        var property1 = entityType.AddProperty("Id2", typeof(string));
        property1.IsNullable = false;
        entityType.SetPrimaryKey(
            new[] { property, property1 });

        var dependentProp1 = entityType.AddProperty("P1", typeof(int?));
        dependentProp1.IsNullable = false;
        var dependentProp2 = entityType.AddProperty("P2", typeof(string));
        dependentProp2.IsNullable = false;

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.FindPrimaryKey(), entityType);
        foreignKey.IsRequired = false;

        Assert.False(foreignKey.IsRequired);
        Assert.False(dependentProp1.IsNullable);
        Assert.False(dependentProp2.IsNullable);
    }

    [ConditionalFact]
    public void IsRequiredDependent_throws_for_incompatible_uniqueness()
    {
        var foreignKey = CreateOneToManyFK();

        Assert.Equal(
            CoreStrings.NonUniqueRequiredDependentForeignKey(
                "{'Id'}",
                nameof(OneToManyDependent)),
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsRequiredDependent = true).Message);
    }

    private IMutableForeignKey CreateOneToManyFK()
    {
        var model = CreateModel();
        var principalEntityType = model.AddEntityType(typeof(OneToManyPrincipal));
        var property = principalEntityType.AddProperty(NavigationBase.IdProperty);
        var pk = principalEntityType.SetPrimaryKey(property);

        var dependentEntityType = model.AddEntityType(typeof(OneToManyDependent));
        var fkProp = dependentEntityType.AddProperty(NavigationBase.IdProperty);
        var fk = dependentEntityType.AddForeignKey(new[] { fkProp }, pk, principalEntityType);
        fk.SetPrincipalToDependent(NavigationBase.OneToManyDependentsProperty);
        fk.SetDependentToPrincipal(NavigationBase.OneToManyPrincipalProperty);
        return fk;
    }

    private IMutableForeignKey CreateOneToManySameBaseFK()
    {
        var model = CreateModel();

        var baseEntityType = model.AddEntityType(typeof(NavigationBase));
        var property1 = baseEntityType.AddProperty(NavigationBase.IdProperty);
        var pk = baseEntityType.SetPrimaryKey(property1);

        var principalEntityType = model.AddEntityType(typeof(OneToManyPrincipal));
        principalEntityType.BaseType = baseEntityType;

        var dependentEntityType = model.AddEntityType(typeof(OneToManyDependent));
        dependentEntityType.BaseType = baseEntityType;
        var fk = dependentEntityType.AddForeignKey(new[] { property1 }, pk, principalEntityType);
        fk.SetPrincipalToDependent(NavigationBase.OneToManyDependentsProperty);
        fk.SetDependentToPrincipal(NavigationBase.OneToManyPrincipalProperty);
        return fk;
    }

    private IMutableForeignKey CreateOneToManySameHierarchyFK()
    {
        var model = CreateModel();

        var baseEntityType = model.AddEntityType(typeof(NavigationBase));
        var property1 = baseEntityType.AddProperty(NavigationBase.IdProperty);
        var pk = baseEntityType.SetPrimaryKey(property1);

        var dependentEntityType = model.AddEntityType(typeof(OneToManyDependent));
        dependentEntityType.BaseType = baseEntityType;
        var fk = dependentEntityType.AddForeignKey(new[] { property1 }, pk, baseEntityType);
        fk.SetPrincipalToDependent(NavigationBase.OneToManyDependentsProperty);
        return fk;
    }

    public abstract class NavigationBase
    {
        public static readonly PropertyInfo IdProperty = typeof(NavigationBase).GetProperty(nameof(Id));

        public static readonly PropertyInfo OneToManyDependentsProperty =
            typeof(NavigationBase).GetProperty(nameof(OneToManyDependents));

        public static readonly PropertyInfo OneToManyPrincipalProperty = typeof(NavigationBase).GetProperty(nameof(OneToManyPrincipal));

        public int Id { get; set; }
        public IEnumerable<OneToManyDependent> OneToManyDependents { get; set; }
        public OneToManyPrincipal OneToManyPrincipal { get; set; }
    }

    public class OneToManyPrincipal : NavigationBase
    {
        public IEnumerable<OneToManyDependent> Deception { get; set; }
    }

    public class DerivedOneToManyPrincipal : OneToManyPrincipal;

    public class OneToManyDependent : NavigationBase
    {
        public static readonly PropertyInfo DeceptionProperty = typeof(OneToManyDependent).GetProperty(nameof(Deception));

        public OneToManyPrincipal Deception { get; set; }
    }

    public class DerivedOneToManyDependent : OneToManyDependent;

    [ConditionalFact]
    public void Throws_when_setting_navigation_to_principal_on_wrong_FK()
    {
        var foreignKey1 = CreateOneToManyFK();
        foreignKey1.SetDependentToPrincipal(OneToManyDependent.DeceptionProperty);

        var newFkProp = foreignKey1.DeclaringEntityType.AddProperty("FkProp", typeof(int));
        var foreignKey2 = foreignKey1.DeclaringEntityType.AddForeignKey(
            new[] { newFkProp },
            foreignKey1.PrincipalEntityType.FindPrimaryKey(),
            foreignKey1.PrincipalEntityType);

        Assert.Equal(
            CoreStrings.NavigationForWrongForeignKey(
                nameof(OneToManyDependent.Deception),
                nameof(OneToManyDependent),
                foreignKey2.Properties.Format(),
                foreignKey1.Properties.Format()),
            Assert.Throws<InvalidOperationException>(
                ()
                    => foreignKey2.SetDependentToPrincipal(OneToManyDependent.DeceptionProperty)).Message);
    }

    [ConditionalFact]
    public void Throws_when_setting_navigation_to_dependent_on_wrong_FK()
    {
        var foreignKey1 = CreateOneToManyFK();
        foreignKey1.SetDependentToPrincipal(OneToManyDependent.DeceptionProperty);

        var newFkProp = foreignKey1.DeclaringEntityType.AddProperty("FkProp", typeof(int));
        var foreignKey2 = foreignKey1.DeclaringEntityType.AddForeignKey(
            new[] { newFkProp },
            foreignKey1.PrincipalEntityType.FindPrimaryKey(),
            foreignKey1.PrincipalEntityType);

        Assert.Equal(
            CoreStrings.NavigationForWrongForeignKey(
                nameof(OneToManyDependent.Deception),
                nameof(OneToManyDependent),
                foreignKey2.Properties.Format(),
                foreignKey1.Properties.Format()),
            Assert.Throws<InvalidOperationException>(
                ()
                    => foreignKey2.SetDependentToPrincipal(OneToManyDependent.DeceptionProperty)).Message);
    }

    [ConditionalFact]
    public void IsUnique_throws_for_incompatible_navigation()
    {
        var foreignKey = CreateOneToManyFK();
        foreignKey.SetPrincipalToDependent(nameof(OneToManyPrincipal.Deception));

        Assert.Equal(
            CoreStrings.UnableToSetIsUnique(
                "True",
                nameof(OneToManyPrincipal.Deception),
                nameof(OneToManyPrincipal)),
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsUnique = true).Message);
    }

    [ConditionalFact]
    public void IsUnique_throws_for_incompatible_required_dependent()
    {
        var foreignKey = CreateOneToManyFK();
        foreignKey.SetPrincipalToDependent((string)null);
        foreignKey.IsUnique = true;
        foreignKey.IsRequiredDependent = true;

        Assert.Equal(
            CoreStrings.NonUniqueRequiredDependentForeignKey(
                "{'Id'}",
                nameof(OneToManyDependent)),
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsUnique = false).Message);
    }

    private IMutableForeignKey CreateSelfRefFK(bool useAltKey = false)
    {
        var entityType = CreateModel().AddEntityType(typeof(SelfRef));
        var pk = entityType.SetPrimaryKey(entityType.AddProperty(SelfRef.IdProperty));

        var property = entityType.AddProperty("AltId", typeof(int));
        var principalKey = useAltKey
            ? entityType.AddKey(property)
            : pk;

        var fk = entityType.AddForeignKey(new[] { pk.Properties.Single() }, principalKey, entityType);
        fk.IsUnique = true;
        fk.SetDependentToPrincipal(SelfRef.SelfRefPrincipalProperty);
        fk.SetPrincipalToDependent(SelfRef.SelfRefDependentProperty);
        return fk;
    }

    private class SelfRef
    {
        public static readonly PropertyInfo IdProperty = typeof(SelfRef).GetProperty("Id");
        public static readonly PropertyInfo SelfRefIdProperty = typeof(SelfRef).GetProperty(nameof(SelfRefId));
        public static readonly PropertyInfo SelfRefPrincipalProperty = typeof(SelfRef).GetProperty(nameof(SelfRefPrincipal));
        public static readonly PropertyInfo SelfRefDependentProperty = typeof(SelfRef).GetProperty(nameof(SelfRefDependent));

        public int Id { get; set; }
        public SelfRef SelfRefPrincipal { get; set; }
        public SelfRef SelfRefDependent { get; set; }
        public int? SelfRefId { get; set; }
    }

    [ConditionalFact]
    public void IsSelfReferencing_returns_true_for_self_ref_foreign_keys()
    {
        var fk = CreateSelfRefFK();

        Assert.True(fk.IsSelfReferencing());
    }

    [ConditionalFact]
    public void IsSelfReferencing_returns_true_for_non_pk_self_ref_foreign_keys()
    {
        var fk = CreateSelfRefFK(useAltKey: true);

        Assert.True(fk.IsSelfReferencing());
    }

    [ConditionalFact]
    public void IsSelfReferencing_returns_false_for_same_hierarchy_foreign_keys()
    {
        var fk = CreateOneToManySameHierarchyFK();

        Assert.False(fk.IsSelfReferencing());
    }

    [ConditionalFact]
    public void IsSelfReferencing_returns_false_for_same_base_foreign_keys()
    {
        var fk = CreateOneToManySameBaseFK();

        Assert.False(fk.IsSelfReferencing());
    }

    [ConditionalFact]
    public void IsSelfReferencing_returns_false_for_non_hierarchical_foreign_keys()
    {
        var fk = CreateOneToManyFK();

        Assert.False(fk.IsSelfReferencing());
    }

    [ConditionalFact]
    public void IsBaseLinking_returns_true_for_self_ref_foreign_keys()
    {
        var fk = CreateSelfRefFK();

        Assert.True(fk.IsBaseLinking());
    }

    [ConditionalFact]
    public void IsBaseLinking_returns_false_for_non_pk_self_ref_foreign_keys()
    {
        var fk = CreateSelfRefFK(useAltKey: true);

        Assert.False(fk.IsBaseLinking());
    }

    [ConditionalFact]
    public void IsBaseLinking_returns_true_for_same_hierarchy_foreign_keys()
    {
        var fk = CreateOneToManySameHierarchyFK();

        Assert.True(fk.IsBaseLinking());
    }

    [ConditionalFact]
    public void IsBaseLinking_returns_true_for_same_base_foreign_keys()
    {
        var fk = CreateOneToManySameBaseFK();

        Assert.True(fk.IsBaseLinking());
    }

    [ConditionalFact]
    public void IsSelfPrimaryKeyReferencing_returns_false_for_non_hierarchical_foreign_keys()
    {
        var fk = CreateOneToManyFK();

        Assert.False(fk.IsBaseLinking());
    }

    [ConditionalFact]
    public void Can_change_cascade_delete_flag()
    {
        var entityType = CreateModel().AddEntityType("E");
        var keyProp = entityType.AddProperty("Id", typeof(int));
        var dependentProp = entityType.AddProperty("P", typeof(int));
        var principalProp = entityType.AddProperty("U", typeof(int));
        entityType.SetPrimaryKey(keyProp);
        var principalKey = entityType.AddKey(principalProp);

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, principalKey, entityType);

        Assert.Equal(DeleteBehavior.ClientSetNull, foreignKey.DeleteBehavior);

        foreignKey.DeleteBehavior = DeleteBehavior.Cascade;

        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);

        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;

        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);

        foreignKey.DeleteBehavior = DeleteBehavior.SetNull;

        Assert.Equal(DeleteBehavior.SetNull, foreignKey.DeleteBehavior);

        foreignKey.DeleteBehavior = DeleteBehavior.ClientSetNull;

        Assert.Equal(DeleteBehavior.ClientSetNull, foreignKey.DeleteBehavior);
    }

    [ConditionalFact]
    public void Can_change_cascade_ownership()
    {
        var entityType = CreateModel().AddOwnedEntityType("E");
        var keyProp = entityType.AddProperty("Id", typeof(int));
        var dependentProp = entityType.AddProperty("P", typeof(int));
        var principalProp = entityType.AddProperty("U", typeof(int));
        entityType.SetPrimaryKey(keyProp);
        var principalKey = entityType.AddKey(principalProp);

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, principalKey, entityType);
        foreignKey.SetPrincipalToDependent("S");

        Assert.False(foreignKey.IsOwnership);

        foreignKey.IsOwnership = true;

        Assert.True(foreignKey.IsOwnership);

        Assert.Equal(
            CoreStrings.OwnershipToDependent(
                "S",
                "E (Dictionary<string, object>)",
                "E (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(() => foreignKey.SetPrincipalToDependent((string)null)).Message);
    }

    [ConditionalFact]
    public void IsOwnership_throws_when_no_navigation()
    {
        var entityType = CreateModel().AddOwnedEntityType("E");
        var keyProp = entityType.AddProperty("Id", typeof(int));
        var dependentProp = entityType.AddProperty("P", typeof(int));
        var principalProp = entityType.AddProperty("U", typeof(int));
        entityType.SetPrimaryKey(keyProp);
        var principalKey = entityType.AddKey(principalProp);

        var foreignKey = entityType.AddForeignKey(new[] { dependentProp }, principalKey, entityType);

        Assert.False(foreignKey.IsOwnership);

        Assert.Equal(
            CoreStrings.NavigationlessOwnership(
                "E (Dictionary<string, object>)",
                "E (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(() => foreignKey.IsOwnership = true).Message);
    }

    [ConditionalFact]
    public void Can_find_targets_for_non_hierarchical_foreign_keys()
    {
        var fk = CreateOneToManyFK();

        Assert.Same(fk.PrincipalEntityType, fk.GetRelatedEntityType(fk.DeclaringEntityType));
        Assert.Same(fk.DeclaringEntityType, fk.GetRelatedEntityType(fk.PrincipalEntityType));
        Assert.Equal(new[] { fk.PrincipalToDependent }, fk.FindNavigationsFrom(fk.PrincipalEntityType));
        Assert.Equal(new[] { fk.DependentToPrincipal }, fk.FindNavigationsFrom(fk.DeclaringEntityType));
        Assert.Equal(new[] { fk.DependentToPrincipal }, fk.FindNavigationsTo(fk.PrincipalEntityType));
        Assert.Equal(new[] { fk.PrincipalToDependent }, fk.FindNavigationsTo(fk.DeclaringEntityType));

        Assert.Equal(new[] { fk.PrincipalToDependent }, fk.FindNavigationsFromInHierarchy(fk.PrincipalEntityType));
        Assert.Equal(new[] { fk.DependentToPrincipal }, fk.FindNavigationsFromInHierarchy(fk.DeclaringEntityType));
        Assert.Equal(new[] { fk.DependentToPrincipal }, fk.FindNavigationsToInHierarchy(fk.PrincipalEntityType));
        Assert.Equal(new[] { fk.PrincipalToDependent }, fk.FindNavigationsToInHierarchy(fk.DeclaringEntityType));
    }

    [ConditionalFact]
    public void Can_find_targets_for_same_base_foreign_keys()
    {
        var fk = CreateOneToManySameBaseFK();

        var model = fk.DeclaringEntityType.Model;
        var derivedPrincipal = model.AddEntityType(typeof(DerivedOneToManyPrincipal));
        derivedPrincipal.BaseType = fk.PrincipalEntityType;

        var derivedDependent = model.AddEntityType(typeof(DerivedOneToManyDependent));
        derivedDependent.BaseType = fk.DeclaringEntityType;

        Assert.Same(fk.PrincipalEntityType, fk.GetRelatedEntityType(fk.DeclaringEntityType));
        Assert.Same(fk.DeclaringEntityType, fk.GetRelatedEntityType(fk.PrincipalEntityType));
        Assert.Same(fk.PrincipalToDependent, fk.FindNavigationsFrom(fk.PrincipalEntityType).SingleOrDefault());
        Assert.Same(fk.DependentToPrincipal, fk.FindNavigationsFrom(fk.DeclaringEntityType).SingleOrDefault());
        Assert.Same(fk.DependentToPrincipal, fk.FindNavigationsTo(fk.PrincipalEntityType).SingleOrDefault());
        Assert.Same(fk.PrincipalToDependent, fk.FindNavigationsTo(fk.DeclaringEntityType).SingleOrDefault());

        Assert.Same(fk.PrincipalToDependent, fk.FindNavigationsFromInHierarchy(fk.PrincipalEntityType).SingleOrDefault());
        Assert.Same(fk.DependentToPrincipal, fk.FindNavigationsFromInHierarchy(fk.DeclaringEntityType).SingleOrDefault());
        Assert.Same(fk.DependentToPrincipal, fk.FindNavigationsToInHierarchy(fk.PrincipalEntityType).SingleOrDefault());
        Assert.Same(fk.PrincipalToDependent, fk.FindNavigationsToInHierarchy(fk.DeclaringEntityType).SingleOrDefault());

        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                derivedDependent.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.GetRelatedEntityType(derivedDependent)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                derivedPrincipal.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.GetRelatedEntityType(derivedPrincipal)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                derivedPrincipal.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsFrom(derivedPrincipal)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                derivedDependent.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsFrom(derivedDependent)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                derivedPrincipal.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsTo(derivedPrincipal)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                derivedDependent.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsTo(derivedDependent)).Message);

        Assert.Equal(new[] { fk.PrincipalToDependent }.Where(n => n != null), fk.FindNavigationsFromInHierarchy(derivedPrincipal));
        Assert.Equal(new[] { fk.DependentToPrincipal }.Where(n => n != null), fk.FindNavigationsFromInHierarchy(derivedDependent));
        Assert.Equal(new[] { fk.DependentToPrincipal }.Where(n => n != null), fk.FindNavigationsToInHierarchy(derivedPrincipal));
        Assert.Equal(new[] { fk.PrincipalToDependent }.Where(n => n != null), fk.FindNavigationsToInHierarchy(derivedDependent));
    }

    [ConditionalFact]
    public void Can_find_targets_for_self_ref_foreign_keys()
    {
        var fk = CreateSelfRefFK();

        Assert.Same(fk.PrincipalEntityType, fk.GetRelatedEntityType(fk.DeclaringEntityType));
        Assert.Same(fk.DeclaringEntityType, fk.GetRelatedEntityType(fk.PrincipalEntityType));

        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsFrom(fk.PrincipalEntityType).ToArray());
        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsFrom(fk.DeclaringEntityType).ToArray());
        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsTo(fk.PrincipalEntityType).ToArray());
        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsTo(fk.DeclaringEntityType).ToArray());

        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsFromInHierarchy(fk.PrincipalEntityType).ToArray());
        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsFromInHierarchy(fk.DeclaringEntityType).ToArray());
        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsToInHierarchy(fk.PrincipalEntityType).ToArray());
        Assert.Equal(
            [fk.PrincipalToDependent, fk.DependentToPrincipal],
            fk.FindNavigationsToInHierarchy(fk.DeclaringEntityType).ToArray());
    }

    [ConditionalFact]
    public void Can_finding_targets_for_same_hierarchy_foreign_keys()
    {
        var fk = CreateOneToManySameHierarchyFK();

        Assert.Same(fk.PrincipalEntityType, fk.GetRelatedEntityType(fk.DeclaringEntityType));
        Assert.Same(fk.DeclaringEntityType, fk.GetRelatedEntityType(fk.PrincipalEntityType));
        Assert.Same(fk.PrincipalToDependent, fk.FindNavigationsFrom(fk.PrincipalEntityType).SingleOrDefault());
        Assert.Same(fk.DependentToPrincipal, fk.FindNavigationsFrom(fk.DeclaringEntityType).SingleOrDefault());
        Assert.Same(fk.DependentToPrincipal, fk.FindNavigationsTo(fk.PrincipalEntityType).SingleOrDefault());
        Assert.Same(fk.PrincipalToDependent, fk.FindNavigationsTo(fk.DeclaringEntityType).SingleOrDefault());

        Assert.Equal(
            new[] { fk.PrincipalToDependent, fk.DependentToPrincipal }.Where(n => n != null),
            fk.FindNavigationsFromInHierarchy(fk.PrincipalEntityType));
        Assert.Equal(
            new[] { fk.PrincipalToDependent, fk.DependentToPrincipal }.Where(n => n != null),
            fk.FindNavigationsFromInHierarchy(fk.DeclaringEntityType));
        Assert.Equal(
            new[] { fk.PrincipalToDependent, fk.DependentToPrincipal }.Where(n => n != null),
            fk.FindNavigationsToInHierarchy(fk.PrincipalEntityType));
        Assert.Equal(
            new[] { fk.PrincipalToDependent, fk.DependentToPrincipal }.Where(n => n != null),
            fk.FindNavigationsToInHierarchy(fk.DeclaringEntityType));
    }

    [ConditionalFact]
    public void Finding_targets_throws_for_entity_types_not_in_the_relationship()
    {
        var fk = CreateOneToManyFK();
        var unrelatedType = fk.DeclaringEntityType.Model.AddEntityType(typeof(NavigationBase));

        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.GetRelatedEntityType(unrelatedType)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.GetRelatedEntityType(unrelatedType)).Message);

        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsFrom(unrelatedType)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsFrom(unrelatedType)).Message);

        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsTo(unrelatedType)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationshipStrict(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsTo(unrelatedType)).Message);

        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationship(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsFromInHierarchy(unrelatedType)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationship(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsFromInHierarchy(unrelatedType)).Message);

        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationship(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsToInHierarchy(unrelatedType)).Message);
        Assert.Equal(
            CoreStrings.EntityTypeNotInRelationship(
                unrelatedType.DisplayName(), fk.DeclaringEntityType.DisplayName(), fk.PrincipalEntityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => fk.FindNavigationsToInHierarchy(unrelatedType)).Message);
    }

    private static IMutableModel CreateModel()
        => new Model();
}
