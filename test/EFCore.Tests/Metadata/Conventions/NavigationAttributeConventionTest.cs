// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
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

            RunConvention(principalEntityTypeBuilder);

            Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.BlogDetails));
            Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(BlogDetails.Blog));

            RunConvention(dependentEntityTypeBuilder);

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

            RunConvention(principalEntityTypeBuilder);

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
                ConfigurationSource.Convention);

            var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation(nameof(Post.Blog));

            relationshipBuilder.IsRequired(false, ConfigurationSource.Convention);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            RunConvention(relationshipBuilder, navigation);

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

            RunConvention(relationshipBuilder, navigation);

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

            RunConvention(relationshipBuilder, navigation);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            var logEntry = ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Debug, logEntry.Level);
            Assert.Equal(
                CoreResources.LogRequiredAttributeOnCollection(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Principal), nameof(Principal.Dependent)), logEntry.Message);
        }

        [ConditionalFact]
        public void RequiredAttribute_does_not_set_is_required_for_navigation_to_dependent()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
            var principalEntityTypeBuilder =
                dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                    principalEntityTypeBuilder.Metadata,
                    nameof(Dependent.Principal),
                    nameof(Principal.Dependent),
                    ConfigurationSource.Convention)
                .HasEntityTypes
                    (principalEntityTypeBuilder.Metadata, dependentEntityTypeBuilder.Metadata, ConfigurationSource.Explicit);

            var navigation = principalEntityTypeBuilder.Metadata.FindNavigation(nameof(Principal.Dependent));

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            RunConvention(relationshipBuilder, navigation);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            var logEntry = ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Debug, logEntry.Level);
            Assert.Equal(
                CoreResources.LogRequiredAttributeOnDependent(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Dependent), nameof(Dependent.Principal)), logEntry.Message);
        }

        [ConditionalFact]
        public void RequiredAttribute_inverts_when_navigation_to_dependent()
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

            RunConvention(relationshipBuilder, navigation);

            var newForeignKey = principalEntityTypeBuilder.Metadata.GetForeignKeys().Single();
            Assert.Equal(nameof(Principal.Dependent), newForeignKey.DependentToPrincipal.Name);
            Assert.Equal(nameof(Principal), newForeignKey.DeclaringEntityType.DisplayName());
            Assert.True(newForeignKey.IsRequired);

            var logEntry = ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Debug, logEntry.Level);
            Assert.Equal(
                CoreResources.LogRequiredAttributeInverted(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Principal.Dependent), nameof(Principal)), logEntry.Message);
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
        public void RequiredAttribute_can_be_specified_on_both_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            var model = (Model)modelBuilder.Model;
            modelBuilder.Entity<BlogDetails>().HasOne(b => b.Blog).WithOne(b => b.BlogDetails);

            Assert.True(
                model.FindEntityType(typeof(BlogDetails)).GetForeignKeys()
                    .Single(fk => fk.PrincipalEntityType?.ClrType == typeof(Blog)).IsRequired);

            var logEntry = ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Debug, logEntry.Level);
            Assert.Equal(
                CoreResources.LogRequiredAttributeOnBothNavigations(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Blog), nameof(Blog.BlogDetails), nameof(BlogDetails), nameof(BlogDetails.Blog)), logEntry.Message);
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

            RunConvention(dependentEntityTypeBuilder);

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

            RunConvention(dependentEntityTypeBuilder);

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

            var convention = new InversePropertyAttributeConvention(CreateDependencies(CreateLogger()));
            convention.ProcessEntityTypeAdded(dependentEntityTypeBuilder,
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
        public void InversePropertyAttribute_does_not_configure_non_defining_navigation()
        {
            var principalEntityTypeBuilder = CreateInternalEntityTypeBuilder<Principal>();

            var dependentEntityTypeBuilder = principalEntityTypeBuilder.ModelBuilder.Metadata.AddEntityType(
                    typeof(Dependent), nameof(Principal.Dependents), principalEntityTypeBuilder.Metadata, ConfigurationSource.Convention)
                .Builder;

            dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                nameof(Dependent.Principal),
                nameof(Principal.Dependents),
                ConfigurationSource.Convention);

            Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
            Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
            Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

            var convention = new InversePropertyAttributeConvention(CreateDependencies(CreateLogger()));
            convention.ProcessEntityTypeAdded(dependentEntityTypeBuilder,
                new ConventionContext<IConventionEntityTypeBuilder>(
                dependentEntityTypeBuilder.Metadata.Model.ConventionDispatcher));

            var logEntry = ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Warning, logEntry.Level);
            Assert.Equal(
                CoreResources.LogNonDefiningInverseNavigation(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Principal), nameof(Principal.Dependent), "Principal.Dependents#Dependent", nameof(Dependent.Principal),
                    nameof(Principal.Dependents)), logEntry.Message);

            Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
            Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
            Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

            Validate(dependentEntityTypeBuilder);
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

            var convention = new InversePropertyAttributeConvention(CreateDependencies(CreateLogger()));
            convention.ProcessEntityTypeAdded(dependentEntityTypeBuilder,
                new ConventionContext<IConventionEntityTypeBuilder>(
                dependentEntityTypeBuilder.Metadata.Model.ConventionDispatcher));

            Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependents));
            Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Principal.Dependent));
            Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Dependent.Principal));

            convention.ProcessModelFinalized(dependentEntityTypeBuilder.ModelBuilder,
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
                    "AnotherEntity", nameof(SelfReferencingEntity), "AnotherEntity", nameof(SelfReferencingEntity)),
                Assert.Throws<InvalidOperationException>(
                    () => RunConvention(entityTypeBuilder)).Message);
        }

        [ConditionalFact]
        public void InversePropertyAttribute_throws_if_navigation_does_not_exist()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<NonExistentNavigation>();

            Assert.Equal(
                CoreStrings.InvalidNavigationWithInverseProperty(
                    "Principal", nameof(NonExistentNavigation), "WrongNavigation", nameof(Principal)),
                Assert.Throws<InvalidOperationException>(
                    () => RunConvention(entityTypeBuilder)).Message);
        }

        [ConditionalFact]
        public void InversePropertyAttribute_throws_if_navigation_return_type_is_wrong()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<WrongNavigationType>();

            Assert.Equal(
                CoreStrings.InvalidNavigationWithInverseProperty("Principal", nameof(WrongNavigationType), "Dependent", nameof(Principal)),
                Assert.Throws<InvalidOperationException>(
                    () => RunConvention(entityTypeBuilder)).Message);
        }

        [ConditionalFact]
        public void InversePropertyAttribute_throws_if_inverse_properties_are_not_pointing_at_each_other()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<MismatchedInverseProperty>();

            Assert.Equal(
                CoreStrings.InversePropertyMismatch(
                    "Principal", nameof(MismatchedInverseProperty), "MismatchedInverseProperty", nameof(Principal)),
                Assert.Throws<InvalidOperationException>(
                    () => RunConvention(entityTypeBuilder)).Message);
        }

        #endregion

        #region ForeignKeyAttribute

        [ConditionalFact]
        public void ForeignKeyAttribute_overrides_configuration_from_convention_source()
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
                        new List<PropertyInfo>
                        {
                            Dependent.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

            RunConvention(relationshipBuilder);

            Assert.Equal("PrincipalFk", relationshipBuilder.Metadata.Properties.First().Name);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_does_not_override_configuration_from_explicit_source()
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
                        new List<PropertyInfo>
                        {
                            Dependent.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Explicit);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

            RunConvention(relationshipBuilder);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_sets_foreign_key_properties_when_applied_on_dependent_to_principal_navigation()
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
                        new List<PropertyInfo>
                        {
                            Dependent.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

            RunConvention(relationshipBuilder);

            Assert.Equal("PrincipalFk", relationshipBuilder.Metadata.Properties.First().Name);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_sets_foreign_key_properties_when_applied_on_principal_to_dependent_navigation()
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
                        new List<PropertyInfo>
                        {
                            Dependent.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

            RunConvention(relationshipBuilder);

            Assert.Equal("PrincipalAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_sets_foreign_key_properties_when_applied_on_property_on_dependent_side()
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
                        new List<PropertyInfo>
                        {
                            Dependent.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

            RunConvention(relationshipBuilder);

            Assert.Equal("PrincipalAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_on_field_sets_foreign_key_properties_when_applied_on_property_on_dependent_side()
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
                        new List<PropertyInfo>
                        {
                            DependentField.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("PrincipalFieldId", relationshipBuilder.Metadata.Properties.First().Name);

            RunConvention(relationshipBuilder);

            Assert.Equal("_principalFieldAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_sets_foreign_key_properties_after_inverting_when_applied_on_property_on_principal_side()
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
                        new List<PropertyInfo>
                        {
                            Principal.DependentIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("DependentId", relationshipBuilder.Metadata.Properties.First().Name);
            Assert.Equal(typeof(Principal), relationshipBuilder.Metadata.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(Dependent), relationshipBuilder.Metadata.PrincipalEntityType.ClrType);

            relationshipBuilder = RunConvention(relationshipBuilder);

            Assert.Equal("PrincipalAnotherFk", relationshipBuilder.Metadata.Properties.First().Name);
            Assert.Equal(typeof(Dependent), relationshipBuilder.Metadata.DeclaringEntityType.ClrType);
            Assert.Equal(typeof(Principal), relationshipBuilder.Metadata.PrincipalEntityType.ClrType);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_sets_composite_foreign_key_properties_when_applied_on_navigation()
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
                        new List<PropertyInfo>
                        {
                            Dependent.PrincipalIdProperty
                        }, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Equal("PrincipalId", relationshipBuilder.Metadata.Properties.First().Name);

            relationshipBuilder = RunConvention(relationshipBuilder);

            Assert.Equal(2, relationshipBuilder.Metadata.Properties.Count);
            Assert.Collection(
                relationshipBuilder.Metadata.Properties,
                p => Assert.Equal("PrincipalId", p.Name),
                p => Assert.Equal("PrincipalFk", p.Name));
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_throws_when_values_on_property_and_navigation_in_entity_type_do_not_match()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<FkPropertyNavigationMismatch>();
            var principalEntityTypeBuilder =
                dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Principal",
                null,
                ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.FkAttributeOnPropertyNavigationMismatch("PrincipalId", "Principal", nameof(FkPropertyNavigationMismatch)),
                Assert.Throws<InvalidOperationException>(() => RunConvention(relationshipBuilder)).Message);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_throws_when_defining_composite_foreign_key_using_attribute_on_properties()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<CompositeFkOnProperty>();
            var principalEntityTypeBuilder =
                dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Principal",
                null,
                ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.CompositeFkOnProperty("Principal", nameof(CompositeFkOnProperty)),
                Assert.Throws<InvalidOperationException>(() => RunConvention(relationshipBuilder)).Message);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_throws_when_property_list_on_navigation_is_in_incorrect_format()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<InvalidPropertyListOnNavigation>();
            var principalEntityTypeBuilder =
                dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "Principal",
                null,
                ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.InvalidPropertyListOnNavigation("Principal", nameof(InvalidPropertyListOnNavigation)),
                Assert.Throws<InvalidOperationException>(() => RunConvention(relationshipBuilder)).Message);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_throws_when_same_set_of_properties_are_pointed_by_multiple_navigations()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<MultipleNavigationsSameFk>();
            var principalEntityTypeBuilder =
                dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                "One",
                null,
                ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.MultipleNavigationsSameFk(typeof(MultipleNavigationsSameFk).Name, "CommonFkProperty"),
                Assert.Throws<InvalidOperationException>(() => RunConvention(relationshipBuilder)).Message);
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_throws_when_specified_on_principal_property_with_collection()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(
                typeof(InvertedPrincipal), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                null,
                nameof(InvertedPrincipal.Dependents),
                ConfigurationSource.Convention);

            RunConvention(relationshipBuilder);

            Assert.Equal(
                CoreStrings.FkAttributeOnNonUniquePrincipal(
                    nameof(InvertedPrincipal.Dependents),
                    nameof(InvertedPrincipal),
                    nameof(Dependent)),
                Assert.Throws<InvalidOperationException>(() => Validate(dependentEntityTypeBuilder)).Message);
        }

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var dependencies = CreateDependencies(CreateLogger());
            var context = new ConventionContext<IConventionEntityTypeBuilder>(
                entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new NotMappedMemberAttributeConvention(dependencies)
                .ProcessEntityTypeAdded(entityTypeBuilder, context);

            new RelationshipDiscoveryConvention(dependencies)
                .ProcessEntityTypeAdded(entityTypeBuilder, context);

            new InversePropertyAttributeConvention(dependencies)
                .ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private InternalRelationshipBuilder RunConvention(InternalRelationshipBuilder relationshipBuilder)
        {
            var dependencies = CreateDependencies(CreateLogger());
            var context = new ConventionContext<IConventionRelationshipBuilder>(
                relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

            new ForeignKeyAttributeConvention(dependencies)
                .ProcessForeignKeyAdded(relationshipBuilder, context);

            return context.ShouldStopProcessing() ? (InternalRelationshipBuilder)context.Result : relationshipBuilder;
        }

        private void RunConvention(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {
            var dependencies = CreateDependencies(CreateLogger());
            var context = new ConventionContext<IConventionNavigation>(
                relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

            new RequiredNavigationAttributeConvention(dependencies)
                .ProcessNavigationAdded(relationshipBuilder, navigation, context);
        }

        private void Validate(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var dependencies = CreateDependencies(CreateLogger());
            var context = new ConventionContext<IConventionModelBuilder>(
                entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new KeyAttributeConvention(dependencies)
                .ProcessModelFinalized(entityTypeBuilder.ModelBuilder, context);

            new InversePropertyAttributeConvention(dependencies)
                .ProcessModelFinalized(entityTypeBuilder.ModelBuilder, context);

            new ForeignKeyAttributeConvention(dependencies)
                .ProcessModelFinalized(entityTypeBuilder.ModelBuilder, context);
        }

        #endregion

        [ConditionalFact]
        public void Navigation_attribute_convention_runs_for_private_property()
        {
            var modelBuilder = CreateModelBuilder();
            var referenceBuilder = modelBuilder.Entity<BlogDetails>().HasOne<Post>("Post").WithOne();

            Assert.False(referenceBuilder.Metadata.Properties.First().IsNullable);
        }

        public ListLoggerFactory ListLoggerFactory { get; }
            = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Name);

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var dependencies = CreateDependencies(CreateLogger());
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention(dependencies));

            conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention(dependencies));

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private ModelBuilder CreateModelBuilder()
        {
            var dependencies = CreateDependencies(CreateLogger());

            return new ModelBuilder(
                new RuntimeConventionSetBuilder(
                        new InMemoryConventionSetBuilder(dependencies),
                        Enumerable.Empty<IConventionSetPlugin>())
                    .CreateConventionSet());
        }

        private static ProviderConventionSetBuilderDependencies CreateDependencies(DiagnosticsLogger<DbLoggerCategory.Model> logger)
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>().With(logger);

        private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
        {
            ListLoggerFactory.Clear();
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
            var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
                ListLoggerFactory,
                options,
                new DiagnosticListener("Fake"),
                new TestLoggingDefinitions());
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
        }

        private class Principal
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
    }
}
