// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class NavigationAttributeConventionTest
{
    #region NotMappedAttribute

    [ConditionalFact]
    public void NotMappedAttribute_overrides_configuration_from_convention_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

        dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(BlogDetails.Blog),
            nameof(Blog.BlogDetails),
            ConfigurationSource.Convention);

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));

        RunEntityTypeConventions(principalEntityTypeBuilder);

        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
        Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));

        RunEntityTypeConventions(dependentEntityTypeBuilder);

        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));
    }

    [ConditionalFact]
    public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

        dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(BlogDetails.Blog),
            nameof(Blog.BlogDetails),
            ConfigurationSource.Explicit);

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));

        RunEntityTypeConventions(principalEntityTypeBuilder);

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));
    }

    [ConditionalFact]
    public void NotMappedAttribute_ignores_navigation_with_conventional_builder()
    {
        var modelBuilder = CreateModelBuilder();
        var model = modelBuilder.Model;
        modelBuilder.Entity<BlogDetails>();

        Assert.DoesNotContain(model.FindEntityType(typeof(Blog)).GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
        Assert.Contains(model.FindEntityType(typeof(BlogDetails)).GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));
    }

    #endregion

    #region RequiredAttribute

    [ConditionalFact]
    public void RequiredAttribute_overrides_configuration_from_convention_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Post>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Post.Blog),
            nameof(Blog.Posts),
            ConfigurationSource.Convention,
            setTargetAsPrincipal: true);

        var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation(nameof(Post.Blog));

        relationshipBuilder.IsRequired(false, ConfigurationSource.Convention);

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunRequiredNavigationAttributeConvention(relationshipBuilder, navigation);

        Assert.True(relationshipBuilder.Metadata.IsRequired);
        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.Posts));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Post.Blog));
    }

    [ConditionalFact]
    public void RequiredAttribute_does_not_override_configuration_from_explicit_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Post>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Post.Blog),
            nameof(Blog.Posts),
            ConfigurationSource.Convention);

        var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation(nameof(BlogDetails.Blog));

        relationshipBuilder.IsRequired(false, ConfigurationSource.Explicit);

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunRequiredNavigationAttributeConvention(relationshipBuilder, navigation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);
        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.Posts));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Post.Blog));
    }

    [ConditionalFact]
    public void RequiredAttribute_does_not_set_is_required_for_collection_navigation()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = principalEntityTypeBuilder.HasRelationship(
            dependentEntityTypeBuilder.Metadata,
            nameof(Principal.Dependents),
            nameof(Dependent.Principal),
            ConfigurationSource.Convention);

        var navigation = principalEntityTypeBuilder.Metadata.FindNavigation(nameof(Principal.Dependents));

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunRequiredNavigationAttributeConvention(relationshipBuilder, navigation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        var logEntry = ListLoggerFactory.Log.Single();
        Assert.Equal(LogLevel.Debug, logEntry.Level);
        Assert.Equal(
            CoreResources.LogRequiredAttributeOnCollection(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(Principal), nameof(Principal.Dependents)), logEntry.Message);
    }

    [ConditionalFact]
    public void RequiredAttribute_does_nothing_when_principal_end_is_ambiguous()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Dependent.Principal),
            nameof(Principal.Dependent),
            ConfigurationSource.Convention);

        Assert.Equal(nameof(Dependent), relationshipBuilder.Metadata.DeclaringEntityType.DisplayName());
        Assert.False(relationshipBuilder.Metadata.IsRequired);

        var navigation = principalEntityTypeBuilder.Metadata.FindNavigation(nameof(Principal.Dependent));

        RunRequiredNavigationAttributeConvention(relationshipBuilder, navigation);

        var newForeignKey = dependentEntityTypeBuilder.Metadata.GetForeignKeys().Single();
        Assert.Equal(nameof(Principal.Dependent), newForeignKey.PrincipalToDependent.Name);
        Assert.False(newForeignKey.IsRequired);
        Assert.Empty(ListLoggerFactory.Log);
    }

    [ConditionalFact]
    public void RequiredAttribute_sets_is_required_with_conventional_builder()
    {
        var modelBuilder = CreateModelBuilder();
        var model = (Model)modelBuilder.Model;
        modelBuilder.Entity<BlogDetails>();

        Assert.True(
            model.FindEntityType(typeof(BlogDetails)).GetForeignKeys().Single(fk => fk.PrincipalEntityType?.ClrType == typeof(Blog))
                .IsRequired);
    }

    [ConditionalFact]
    public void RequiredAttribute_does_not_configure_skip_navigations()
    {
        var postEntityTypeBuilder = CreateInternalEntityTypeBuilder<Post>();
        var blogEntityTypeBuilder = postEntityTypeBuilder.ModelBuilder.Entity(
            typeof(Blog), ConfigurationSource.Convention);

        var navigationBuilder = postEntityTypeBuilder.HasSkipNavigation(
            new MemberIdentity(nameof(Post.Blogs)),
            blogEntityTypeBuilder.Metadata,
            null,
            new MemberIdentity(nameof(Blog.Posts)),
            null,
            ConfigurationSource.Convention,
            collections: true,
            onDependent: false);

        var convention = new RequiredNavigationAttributeConvention(CreateDependencies());
        convention.ProcessSkipNavigationAdded(
            navigationBuilder,
            new ConventionContext<IConventionSkipNavigationBuilder>(
                postEntityTypeBuilder.Metadata.Model.ConventionDispatcher));

        var logEntry = ListLoggerFactory.Log.Single();
        Assert.Equal(LogLevel.Debug, logEntry.Level);
        Assert.Equal(
            CoreResources.LogRequiredAttributeOnSkipNavigation(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(Post), nameof(Post.Blogs)), logEntry.Message);

        Validate(postEntityTypeBuilder);

        Assert.Empty(ListLoggerFactory.Log);
    }

    #endregion

    #region InversePropertyAttribute

    [ConditionalFact]
    public void InversePropertyAttribute_overrides_configuration_from_convention_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Dependent.Principal),
            nameof(Principal.Dependents),
            ConfigurationSource.Convention);

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

        RunEntityTypeConventions(dependentEntityTypeBuilder);

        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

        Validate(dependentEntityTypeBuilder);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_does_not_override_configuration_from_explicit_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Dependent.Principal),
            nameof(Principal.Dependents),
            ConfigurationSource.Explicit);

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

        RunEntityTypeConventions(dependentEntityTypeBuilder);

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

        Validate(dependentEntityTypeBuilder);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_does_not_configure_ambiguous_navigations()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<AmbiguousDependent>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(
            typeof(AmbiguousPrincipal), ConfigurationSource.Convention);

        dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(AmbiguousDependent.AmbiguousPrincipal),
            nameof(AmbiguousPrincipal.Dependent),
            ConfigurationSource.Convention);

        Assert.Contains(
            principalEntityTypeBuilder.Metadata.GetNavigations(),
            nav => nav.Name == nameof(AmbiguousPrincipal.Dependent));
        Assert.Contains(
            dependentEntityTypeBuilder.Metadata.GetNavigations(),
            nav => nav.Name == nameof(AmbiguousDependent.AmbiguousPrincipal));
        Assert.DoesNotContain(
            dependentEntityTypeBuilder.Metadata.GetNavigations(),
            nav => nav.Name == nameof(AmbiguousDependent.AnotherAmbiguousPrincipal));

        var convention = new InversePropertyAttributeConvention(CreateDependencies());
        convention.ProcessEntityTypeAdded(
            dependentEntityTypeBuilder,
            new ConventionContext<IConventionEntityTypeBuilder>(
                dependentEntityTypeBuilder.Metadata.Model.ConventionDispatcher));

        Assert.DoesNotContain(
            principalEntityTypeBuilder.Metadata.GetNavigations(),
            nav => nav.Name == nameof(AmbiguousPrincipal.Dependent));
        Assert.DoesNotContain(
            dependentEntityTypeBuilder.Metadata.GetNavigations(),
            nav => nav.Name == nameof(AmbiguousDependent.AnotherAmbiguousPrincipal));
        Assert.DoesNotContain(
            dependentEntityTypeBuilder.Metadata.GetNavigations(),
            nav => nav.Name == nameof(AmbiguousDependent.AmbiguousPrincipal));

        Validate(dependentEntityTypeBuilder);

        var logEntry = ListLoggerFactory.Log.Single();
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Equal(
            CoreResources.LogMultipleInversePropertiesSameTarget(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                "AmbiguousDependent.AmbiguousPrincipal, AmbiguousDependent.AnotherAmbiguousPrincipal",
                nameof(AmbiguousPrincipal.Dependent)), logEntry.Message);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_does_not_configure_non_ownership_navigation()
    {
        var principalEntityTypeBuilder = CreateInternalEntityTypeBuilder<Principal>();

        var dependentEntityTypeBuilder = principalEntityTypeBuilder.HasOwnership(
            typeof(Dependent),
            nameof(Principal.Dependents),
            ConfigurationSource.Convention).Metadata.DeclaringEntityType.Builder;

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
        Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

        var convention = new InversePropertyAttributeConvention(CreateDependencies());
        convention.ProcessEntityTypeAdded(
            dependentEntityTypeBuilder,
            new ConventionContext<IConventionEntityTypeBuilder>(
                dependentEntityTypeBuilder.Metadata.Model.ConventionDispatcher));

        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
        Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
        Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

        convention.ProcessModelFinalizing(
            dependentEntityTypeBuilder.ModelBuilder,
            new ConventionContext<IConventionModelBuilder>(
                dependentEntityTypeBuilder.Metadata.Model.ConventionDispatcher));

        var logEntry = ListLoggerFactory.Log.Single();
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Equal(
            CoreResources.LogNonOwnershipInverseNavigation(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(Principal), nameof(Principal.Dependent), nameof(Dependent), nameof(Dependent.Principal),
                nameof(Principal.Dependents)), logEntry.Message);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_throws_if_self_navigation()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<SelfReferencingEntity>();

        Assert.Equal(
            CoreStrings.SelfReferencingNavigationWithInverseProperty(
                nameof(SelfReferencingEntity), nameof(SelfReferencingEntity.AnotherEntity)),
            Assert.Throws<InvalidOperationException>(
                () => RunEntityTypeConventions(entityTypeBuilder)).Message);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_throws_if_navigation_does_not_exist()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<NonExistentNavigation>();

        Assert.Equal(
            CoreStrings.InvalidNavigationWithInverseProperty(
                "Principal", nameof(NonExistentNavigation), "WrongNavigation", nameof(Principal)),
            Assert.Throws<InvalidOperationException>(
                () => RunEntityTypeConventions(entityTypeBuilder)).Message);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_throws_if_navigation_return_type_is_wrong()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<WrongNavigationType>();

        Assert.Equal(
            CoreStrings.InvalidNavigationWithInverseProperty("Principal", nameof(WrongNavigationType), "Dependent", nameof(Principal)),
            Assert.Throws<InvalidOperationException>(
                () => RunEntityTypeConventions(entityTypeBuilder)).Message);
    }

    [ConditionalFact]
    public void InversePropertyAttribute_throws_if_inverse_properties_are_not_pointing_at_each_other()
    {
        var entityTypeBuilder = CreateInternalEntityTypeBuilder<MismatchedInverseProperty>();

        Assert.Equal(
            CoreStrings.InversePropertyMismatch(
                "Principal", nameof(MismatchedInverseProperty), "MismatchedInverseProperty", nameof(Principal)),
            Assert.Throws<InvalidOperationException>(
                () => RunEntityTypeConventions(entityTypeBuilder)).Message);
    }

    #endregion

    #region ForeignKeyAttribute

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_overrides_configuration_from_convention_source(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Principal",
                "Dependent",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Dependent.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("PrincipalFk", relationshipBuilder.Metadata.Properties.First().Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_does_not_override_configuration_from_explicit_source(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Principal",
                "Dependent",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Dependent.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Explicit);

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_sets_foreign_key_properties_when_applied_on_dependent_to_principal_navigation(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Principal",
                "Dependent",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Dependent.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("PrincipalFk", relationshipBuilder.Metadata.Properties.First().Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_sets_foreign_key_properties_when_applied_on_principal_to_dependent_navigation(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "AnotherPrincipal",
                "Dependent",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Dependent.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.PrincipalToDependent;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("PrincipalAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_sets_foreign_key_properties_when_applied_on_property_on_dependent_side(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "AnotherPrincipal",
                "Dependent",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Dependent.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("PrincipalAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_on_field_sets_foreign_key_properties_when_applied_on_property_on_dependent_side(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<DependentField>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(PrincipalField), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "AnotherPrincipalField",
                "DependentField",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { DependentField.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("PrincipalFieldId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("_principalFieldAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_sets_foreign_key_properties_after_inverting_when_applied_on_property_on_principal_side(
        bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Principal>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Dependent), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Dependent",
                "AnotherPrincipal",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Principal.DependentIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("DependentId", relationshipBuilder.Metadata.Properties.First().Name);
        Assert.Equal(typeof(Principal), relationshipBuilder.Metadata.DeclaringEntityType.ClrType);
        Assert.Equal(typeof(Dependent), relationshipBuilder.Metadata.PrincipalEntityType.ClrType);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.PrincipalToDependent;
            relationshipBuilder = RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            relationshipBuilder = RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal("PrincipalAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
        Assert.Equal(typeof(Dependent), relationshipBuilder.Metadata.DeclaringEntityType.ClrType);
        Assert.Equal(typeof(Principal), relationshipBuilder.Metadata.PrincipalEntityType.ClrType);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_sets_composite_foreign_key_properties_when_applied_on_navigation(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "CompositePrincipal",
                "Dependent",
                ConfigurationSource.Convention)
            .HasForeignKey(
                dependentEntityTypeBuilder.GetOrCreateProperties(
                    new List<PropertyInfo> { Dependent.PrincipalIdProperty }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

        Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.PrincipalToDependent;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal(2, relationshipBuilder.Metadata.Properties.Count);
        Assert.Collection(
            relationshipBuilder.Metadata.Properties,
            p => Assert.Equal("PrincipalId", p.Name),
            p => Assert.Equal("PrincipalFk", p.Name));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_throws_when_values_on_property_and_navigation_in_entity_type_do_not_match(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<FkPropertyNavigationMismatch>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            "Principal",
            null,
            ConfigurationSource.Convention);

        var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
        Assert.Equal(
            CoreStrings.FkAttributeOnPropertyNavigationMismatch("PrincipalId", "Principal", nameof(FkPropertyNavigationMismatch)),
            Assert.Throws<InvalidOperationException>(
                () => useNavigation
                    ? RunForeignKeyAttributeConvention(relationshipBuilder, navigation)
                    : RunForeignKeyAttributeConvention(relationshipBuilder)
            ).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_throws_when_defining_composite_foreign_key_using_attribute_on_properties(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<CompositeFkOnProperty>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            "Principal",
            null,
            ConfigurationSource.Convention);

        var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
        Assert.Equal(
            CoreStrings.CompositeFkOnProperty("Principal", nameof(CompositeFkOnProperty)),
            Assert.Throws<InvalidOperationException>(
                () => useNavigation
                    ? RunForeignKeyAttributeConvention(relationshipBuilder, navigation)
                    : RunForeignKeyAttributeConvention(relationshipBuilder)
            ).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_throws_when_property_list_on_navigation_is_in_incorrect_format(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<InvalidPropertyListOnNavigation>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            "Principal",
            null,
            ConfigurationSource.Convention);

        var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
        Assert.Equal(
            CoreStrings.InvalidPropertyListOnNavigation(
                "Principal", nameof(InvalidPropertyListOnNavigation), "PrincipalId1,,PrincipalId2"),
            Assert.Throws<InvalidOperationException>(
                () => useNavigation
                    ? RunForeignKeyAttributeConvention(relationshipBuilder, navigation)
                    : RunForeignKeyAttributeConvention(relationshipBuilder)
            ).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_throws_when_same_set_of_properties_are_pointed_by_multiple_navigations(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<MultipleNavigationsSameFk>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            "One",
            null,
            ConfigurationSource.Convention);

        var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
        Assert.Equal(
            CoreStrings.MultipleNavigationsSameFk(typeof(MultipleNavigationsSameFk).Name, "CommonFkProperty", "'One', 'Two'"),
            Assert.Throws<InvalidOperationException>(
                () => useNavigation
                    ? RunForeignKeyAttributeConvention(relationshipBuilder, navigation)
                    : RunForeignKeyAttributeConvention(relationshipBuilder)
            ).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignKeyAttribute_throws_when_specified_on_principal_property_with_collection(bool useNavigation)
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(
            typeof(InvertedPrincipal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            null,
            nameof(InvertedPrincipal.Dependents),
            ConfigurationSource.Convention);

        if (useNavigation)
        {
            var navigation = relationshipBuilder.Metadata.PrincipalToDependent;
            RunForeignKeyAttributeConvention(relationshipBuilder, navigation);
        }
        else
        {
            RunForeignKeyAttributeConvention(relationshipBuilder);
        }

        Assert.Equal(
            CoreStrings.FkAttributeOnNonUniquePrincipal(
                nameof(InvertedPrincipal.Dependents),
                nameof(InvertedPrincipal),
                nameof(Dependent)),
            Assert.Throws<InvalidOperationException>(() => Validate(dependentEntityTypeBuilder)).Message);
    }

    private void RunEntityTypeConventions(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionEntityTypeBuilder>(
            entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        new NotMappedMemberAttributeConvention(dependencies)
            .ProcessEntityTypeAdded(entityTypeBuilder, context);

        new RelationshipDiscoveryConvention(dependencies)
            .ProcessEntityTypeAdded(entityTypeBuilder, context);

        new InversePropertyAttributeConvention(dependencies)
            .ProcessEntityTypeAdded(entityTypeBuilder, context);
    }

    private InternalForeignKeyBuilder RunForeignKeyAttributeConvention(InternalForeignKeyBuilder relationshipBuilder)
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionForeignKeyBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

        new ForeignKeyAttributeConvention(dependencies)
            .ProcessForeignKeyAdded(relationshipBuilder, context);

        return context.ShouldStopProcessing() ? (InternalForeignKeyBuilder)context.Result : relationshipBuilder;
    }

    private InternalForeignKeyBuilder RunForeignKeyAttributeConvention(
        InternalForeignKeyBuilder relationshipBuilder,
        Navigation navigation)
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionNavigationBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

        new ForeignKeyAttributeConvention(dependencies)
            .ProcessNavigationAdded(navigation.Builder, context);

        return context.ShouldStopProcessing()
            ? (InternalForeignKeyBuilder)context.Result?.Metadata.ForeignKey.Builder
            : relationshipBuilder;
    }

    private void RunRequiredNavigationAttributeConvention(InternalForeignKeyBuilder relationshipBuilder, Navigation navigation)
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionNavigationBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

        new RequiredNavigationAttributeConvention(dependencies)
            .ProcessNavigationAdded(navigation.Builder, context);
    }

    private void RunNavigationBackingFieldAttributeConvention(
        InternalForeignKeyBuilder relationshipBuilder,
        IConventionNavigationBuilder navigationBuilder)
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionNavigationBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

        new NavigationBackingFieldAttributeConvention(dependencies)
            .ProcessNavigationAdded(navigationBuilder, context);
    }

    private void Validate(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionModelBuilder>(
            entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        new KeyAttributeConvention(dependencies)
            .ProcessModelFinalizing(entityTypeBuilder.ModelBuilder, context);

        new InversePropertyAttributeConvention(dependencies)
            .ProcessModelFinalizing(entityTypeBuilder.ModelBuilder, context);

        new ForeignKeyAttributeConvention(dependencies)
            .ProcessModelFinalizing(entityTypeBuilder.ModelBuilder, context);
    }

    #endregion

    #region BackingFieldAttribute

    [ConditionalFact]
    public void BackingFieldAttribute_overrides_configuration_from_convention_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<DependentForNavWithBackingField>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(
                typeof(PrincipalForNavWithBackingField), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            "PrincipalForNavWithBackingField",
            "DependentForNavWithBackingField",
            ConfigurationSource.Convention);

        IConventionNavigationBuilder navigationBuilder = relationshipBuilder.Metadata.DependentToPrincipal.Builder;
        RunNavigationBackingFieldAttributeConvention(relationshipBuilder, navigationBuilder);

        Assert.Equal("_backingFieldFromAttribute", navigationBuilder.Metadata.GetFieldName());
    }

    [ConditionalFact]
    public void BackingFieldAttribute_does_not_override_configuration_from_explicit_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<DependentForNavWithBackingField>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(
                typeof(PrincipalForNavWithBackingField), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            "PrincipalForNavWithBackingField",
            "DependentForNavWithBackingField",
            ConfigurationSource.Convention);

        var navigationBuilder = relationshipBuilder.Metadata.DependentToPrincipal.Builder;
        navigationBuilder.HasField("_backingFieldFromFluentApi", ConfigurationSource.Explicit);

        RunNavigationBackingFieldAttributeConvention(relationshipBuilder, navigationBuilder);

        Assert.Equal("_backingFieldFromFluentApi", ((IConventionNavigation)navigationBuilder.Metadata).GetFieldName());
    }

    #endregion

    #region DeleteBehaviorAttribute

    [ConditionalFact]
    public void DeleteBehaviorAttribute_overrides_configuration_from_convention_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(
                typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Dependent.Principal),
            nameof(Principal.Dependents),
            ConfigurationSource.Convention);

        var navigationBuilder = relationshipBuilder.Metadata.DependentToPrincipal.Builder;
        var foreignKey = navigationBuilder.Metadata.ForeignKey;
        foreignKey.SetDeleteBehavior(DeleteBehavior.NoAction, ConfigurationSource.Convention);

        RunDeleteBehaviorAttributeConvention(relationshipBuilder, navigationBuilder);

        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [ConditionalFact]
    public void DeleteBehaviorAttribute_does_not_override_configuration_from_explicit_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(
                typeof(Principal), ConfigurationSource.Convention);

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Dependent.Principal),
            nameof(Principal.Dependents),
            ConfigurationSource.Convention);

        var navigationBuilder = relationshipBuilder.Metadata.DependentToPrincipal.Builder;
        var foreignKey = navigationBuilder.Metadata.ForeignKey;
        foreignKey.SetDeleteBehavior(DeleteBehavior.NoAction, ConfigurationSource.Explicit);

        RunDeleteBehaviorAttributeConvention(relationshipBuilder, navigationBuilder);

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    private void RunDeleteBehaviorAttributeConvention(
        InternalForeignKeyBuilder relationshipBuilder,
        InternalNavigationBuilder navigationBuilder
    )
    {
        var dependencies = CreateDependencies();
        var context = new ConventionContext<IConventionNavigationBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

        new DeleteBehaviorAttributeConvention(dependencies)
            .ProcessNavigationAdded(navigationBuilder, context);
    }

    #endregion

    [ConditionalFact]
    public void Navigation_attribute_convention_runs_for_private_property()
    {
        var modelBuilder = CreateModelBuilder();
        var referenceBuilder = modelBuilder.Entity<BlogDetails>().HasOne<Post>("Post").WithOne().HasForeignKey<BlogDetails>();

        Assert.False(referenceBuilder.Metadata.Properties.First().IsNullable);
    }

    public ListLoggerFactory ListLoggerFactory { get; }
        = new(l => l == DbLoggerCategory.Model.Name);

    private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
    {
        var dependencies = CreateDependencies();
        var conventionSet = new ConventionSet();
        conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention(dependencies));

        conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention(dependencies));

        var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

        return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
    }

    private ModelBuilder CreateModelBuilder()
    {
        var serviceProvider = CreateServiceProvider();
        return new ModelBuilder(
            serviceProvider.GetService<IConventionSetBuilder>().CreateConventionSet(),
            serviceProvider.GetService<ModelDependencies>());
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => CreateServiceProvider().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    protected IServiceProvider CreateServiceProvider()
        => InMemoryTestHelpers.Instance.CreateContextServices(
            new ServiceCollection()
                .AddScoped<IDiagnosticsLogger<DbLoggerCategory.Model>>(_ => CreateLogger()));

    private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
    {
        ListLoggerFactory.Clear();
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
        var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
            ListLoggerFactory,
            options,
            new DiagnosticListener("Fake"),
            new TestLoggingDefinitions(),
            new NullDbContextLogger());
        return modelLogger;
    }

    private class Blog
    {
        public int Id { get; set; }

        [NotMapped]
        [Required]
        public BlogDetails BlogDetails { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    private class BlogDetails
    {
        public int Id { get; set; }
        public int BlogId { get; set; }

        [Required]
        public Blog Blog { get; set; }

        [Required]
        private Post Post { get; set; }
    }

    private class Post
    {
        public int Id { get; set; }

        [Required]
        public Blog Blog { get; set; }

        [NotMapped]
        [Required]
        public ICollection<Blog> Blogs { get; set; }
    }

    private interface IPrincipal
    {
        MismatchedInverseProperty MismatchedInverseProperty { get; set; }
    }

    private class Principal : IPrincipal
    {
        public static readonly PropertyInfo DependentIdProperty = typeof(Principal).GetProperty("DependentId");

        public int Id { get; set; }

        public int DependentId { get; set; }

        [Required]
        [ForeignKey("PrincipalFk")]
        public ICollection<Dependent> Dependents { get; set; }

        [Required]
        public Dependent Dependent { get; set; }

        public NonExistentNavigation MissingNavigation { get; set; }

        [InverseProperty("AnotherPrincipal")]
        public MismatchedInverseProperty MismatchedInverseProperty { get; set; }

        MismatchedInverseProperty IPrincipal.MismatchedInverseProperty { get; set; }
    }

    private class Dependent
    {
        public static readonly PropertyInfo PrincipalIdProperty = typeof(Dependent).GetProperty("PrincipalId");

        public int Id { get; set; }

        public int PrincipalId { get; set; }

        public int PrincipalFk { get; set; }

        [ForeignKey("AnotherPrincipal")]
        public int PrincipalAnotherFk { get; set; }

        [ForeignKey("PrincipalFk")]
        [InverseProperty("Dependent")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Principal Principal { get; set; }

        public Principal AnotherPrincipal { get; set; }

        [ForeignKey("PrincipalId, PrincipalFk")]
        public Principal CompositePrincipal { get; set; }
    }

    private class PrincipalField
    {
        public int Id { get; set; }

        public int DependentFieldId { get; set; }

        public DependentField DependentField { get; set; }
    }

    private class DependentField
    {
        public static readonly PropertyInfo PrincipalIdProperty = typeof(DependentField).GetProperty("PrincipalFieldId");

        public int Id { get; set; }

        public int PrincipalFieldId { get; set; }

        public int PrincipalFieldFk { get; set; }

        [ForeignKey(nameof(AnotherPrincipalField))]
#pragma warning disable 169
        private readonly int _principalFieldAnotherFk;
#pragma warning restore 169

        public PrincipalField AnotherPrincipalField { get; set; }
    }

    private class SelfReferencingEntity
    {
        public int Id { get; set; }

        [InverseProperty("AnotherEntity")]
        public SelfReferencingEntity AnotherEntity { get; set; }
    }

    private class NonExistentNavigation
    {
        public int Id { get; set; }

        [InverseProperty("WrongNavigation")]
        public Principal Principal { get; set; }
    }

    private class WrongNavigationType
    {
        public int Id { get; set; }

        [InverseProperty("Dependent")]
        public Principal Principal { get; set; }
    }

    private class MismatchedInverseProperty
    {
        public int Id { get; set; }

        [InverseProperty("MismatchedInverseProperty")]
        public Principal Principal { get; set; }
    }

    private class AmbiguousDependent
    {
        public int Id { get; set; }

        [InverseProperty("Dependent")]
        public AmbiguousPrincipal AmbiguousPrincipal { get; set; }

        [InverseProperty("Dependent")]
        public AmbiguousPrincipal AnotherAmbiguousPrincipal { get; set; }
    }

    private class AmbiguousPrincipal
    {
        public int Id { get; set; }

        public AmbiguousDependent Dependent { get; set; }
    }

    private class FkPropertyNavigationMismatch
    {
        public int Id { get; set; }

        [ForeignKey("Principal")]
        public int PrincipalId { get; set; }

        [ForeignKey("PrincipalFk")]
        public Principal Principal { get; set; }
    }

    private class CompositeFkOnProperty
    {
        public int Id { get; set; }

        [ForeignKey("Principal")]
        public int PrincipalId1 { get; set; }

        [ForeignKey("Principal")]
        public int PrincipalId2 { get; set; }

        public Principal Principal { get; set; }
    }

    private class InvalidPropertyListOnNavigation
    {
        public int Id { get; set; }

        public int PrincipalId1 { get; set; }

        public int PrincipalId2 { get; set; }

        [ForeignKey("PrincipalId1,,PrincipalId2")]
        public Principal Principal { get; set; }
    }

    private class MultipleNavigationsSameFk
    {
        public int Id { get; set; }

        public int CommonFkProperty { get; set; }

        [ForeignKey("CommonFkProperty")]
        public Principal One { get; set; }

        [ForeignKey("CommonFkProperty")]
        public Principal Two { get; set; }
    }

    private class InvertedPrincipal
    {
        public static readonly PropertyInfo DependentIdProperty = typeof(Principal).GetProperty("DependentId");

        public int Id { get; set; }

        [ForeignKey("Dependents")]
        public int DependentId { get; set; }

        public ICollection<Dependent> Dependents { get; set; }
    }

    private class PrincipalForNavWithBackingField
    {
        public int Id { get; set; }

        public DependentForNavWithBackingField DependentForNavWithBackingField { get; set; }
    }

    private class DependentForNavWithBackingField
    {
        public int Id { get; set; }

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0169 // Field never used
        private PrincipalForNavWithBackingField _backingFieldFromAttribute;
        private PrincipalForNavWithBackingField _backingFieldFromFluentApi;
#pragma warning restore CS0169 // Field never used
#pragma warning restore IDE0044 // Add readonly modifier

        [BackingField(nameof(_backingFieldFromAttribute))]
        public PrincipalForNavWithBackingField PrincipalForNavWithBackingField { get; set; }
    }
}
